using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.ProviderBase;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.SqlServer.Server;
using Unity;

namespace System.Data.SqlClient;

/// <summary>Represents an open connection to a SQL Server database. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class SqlConnection : DbConnection, ICloneable, IDbConnection, IDisposable
{
	private class OpenAsyncRetry
	{
		private SqlConnection _parent;

		private TaskCompletionSource<DbConnectionInternal> _retry;

		private TaskCompletionSource<object> _result;

		private CancellationTokenRegistration _registration;

		public OpenAsyncRetry(SqlConnection parent, TaskCompletionSource<DbConnectionInternal> retry, TaskCompletionSource<object> result, CancellationTokenRegistration registration)
		{
			_parent = parent;
			_retry = retry;
			_result = result;
			_registration = registration;
		}

		internal void Retry(Task<DbConnectionInternal> retryTask)
		{
			_registration.Dispose();
			try
			{
				SqlStatistics statistics = null;
				try
				{
					statistics = SqlStatistics.StartTimer(_parent.Statistics);
					if (retryTask.IsFaulted)
					{
						_ = retryTask.Exception.InnerException;
						_parent.CloseInnerConnection();
						_parent._currentCompletion = null;
						_result.SetException(retryTask.Exception.InnerException);
						return;
					}
					if (retryTask.IsCanceled)
					{
						_parent.CloseInnerConnection();
						_parent._currentCompletion = null;
						_result.SetCanceled();
						return;
					}
					bool flag;
					lock (_parent.InnerConnection)
					{
						flag = _parent.TryOpen(_retry);
					}
					if (flag)
					{
						_parent._currentCompletion = null;
						_result.SetResult(null);
					}
					else
					{
						_parent.CloseInnerConnection();
						_parent._currentCompletion = null;
						_result.SetException(ADP.ExceptionWithStackTrace(ADP.InternalError(ADP.InternalErrorCode.CompletedConnectReturnedPending)));
					}
				}
				finally
				{
					SqlStatistics.StopTimer(statistics);
				}
			}
			catch (Exception exception)
			{
				_parent.CloseInnerConnection();
				_parent._currentCompletion = null;
				_result.SetException(exception);
			}
		}
	}

	private bool _AsyncCommandInProgress;

	internal SqlStatistics _statistics;

	private bool _collectstats;

	private bool _fireInfoMessageEventOnUserErrors;

	private Tuple<TaskCompletionSource<DbConnectionInternal>, Task> _currentCompletion;

	private SqlCredential _credential;

	private string _connectionString;

	private int _connectRetryCount;

	private string _accessToken;

	private object _reconnectLock = new object();

	internal Task _currentReconnectionTask;

	private Task _asyncWaitingForReconnection;

	private Guid _originalConnectionId = Guid.Empty;

	private CancellationTokenSource _reconnectionCancellationSource;

	internal SessionData _recoverySessionData;

	internal new bool _suppressStateChangeForReconnection;

	private int _reconnectCount;

	private static readonly DiagnosticListener s_diagnosticListener = new DiagnosticListener("SqlClientDiagnosticListener");

	internal bool _applyTransientFaultHandling;

	private static readonly DbConnectionFactory s_connectionFactory = SqlConnectionFactory.SingletonInstance;

	private DbConnectionOptions _userConnectionOptions;

	private DbConnectionPoolGroup _poolGroup;

	private DbConnectionInternal _innerConnection;

	private int _closeCount;

	/// <summary>When set to true, enables statistics gathering for the current connection.</summary>
	/// <returns>Returns true if statistics gathering is enabled; otherwise false. false is the default.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public bool StatisticsEnabled
	{
		get
		{
			return _collectstats;
		}
		set
		{
			if (value)
			{
				if (ConnectionState.Open == State)
				{
					if (_statistics == null)
					{
						_statistics = new SqlStatistics();
						ADP.TimerCurrent(out _statistics._openTimestamp);
					}
					Parser.Statistics = _statistics;
				}
			}
			else if (_statistics != null && ConnectionState.Open == State)
			{
				Parser.Statistics = null;
				ADP.TimerCurrent(out _statistics._closeTimestamp);
			}
			_collectstats = value;
		}
	}

	internal bool AsyncCommandInProgress
	{
		get
		{
			return _AsyncCommandInProgress;
		}
		set
		{
			_AsyncCommandInProgress = value;
		}
	}

	internal SqlConnectionString.TransactionBindingEnum TransactionBinding => ((SqlConnectionString)ConnectionOptions).TransactionBinding;

	internal SqlConnectionString.TypeSystem TypeSystem => ((SqlConnectionString)ConnectionOptions).TypeSystemVersion;

	internal Version TypeSystemAssemblyVersion => ((SqlConnectionString)ConnectionOptions).TypeSystemAssemblyVersion;

	internal int ConnectRetryInterval => ((SqlConnectionString)ConnectionOptions).ConnectRetryInterval;

	/// <summary>Gets or sets the string used to open a SQL Server database.</summary>
	/// <returns>The connection string that includes the source database name, and other parameters needed to establish the initial connection. The default value is an empty string.</returns>
	/// <exception cref="T:System.ArgumentException">An invalid connection string argument has been supplied, or a required connection string argument has not been supplied. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string ConnectionString
	{
		get
		{
			return ConnectionString_Get();
		}
		set
		{
			if (_credential != null || _accessToken != null)
			{
				SqlConnectionString connectionOptions = new SqlConnectionString(value);
				if (_credential != null)
				{
					CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential(connectionOptions);
				}
				else
				{
					CheckAndThrowOnInvalidCombinationOfConnectionOptionAndAccessToken(connectionOptions);
				}
			}
			ConnectionString_Set(new SqlConnectionPoolKey(value, _credential, _accessToken));
			_connectionString = value;
			CacheConnectionStringProperties();
		}
	}

	/// <summary>Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.</summary>
	/// <returns>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</returns>
	/// <exception cref="T:System.ArgumentException">The value set is less than 0. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int ConnectionTimeout => ((SqlConnectionString)ConnectionOptions)?.ConnectTimeout ?? 15;

	public string AccessToken
	{
		get
		{
			_ = _accessToken;
			SqlConnectionString sqlConnectionString = (SqlConnectionString)UserConnectionOptions;
			if (!InnerConnection.ShouldHidePassword || sqlConnectionString == null || sqlConnectionString.PersistSecurityInfo)
			{
				return _accessToken;
			}
			return null;
		}
		set
		{
			if (!InnerConnection.AllowSetConnectionString)
			{
				throw ADP.OpenConnectionPropertySet("AccessToken", InnerConnection.State);
			}
			if (value != null)
			{
				CheckAndThrowOnInvalidCombinationOfConnectionOptionAndAccessToken((SqlConnectionString)ConnectionOptions);
			}
			ConnectionString_Set(new SqlConnectionPoolKey(_connectionString, _credential, value));
			_accessToken = value;
		}
	}

	/// <summary>Gets the name of the current database or the database to be used after a connection is opened.</summary>
	/// <returns>The name of the current database or the name of the database to be used after a connection is opened. The default value is an empty string.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string Database
	{
		get
		{
			if (!(InnerConnection is SqlInternalConnection { CurrentDatabase: var currentDatabase }))
			{
				SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
				return (sqlConnectionString != null) ? sqlConnectionString.InitialCatalog : "";
			}
			return currentDatabase;
		}
	}

	/// <summary>Gets the name of the instance of SQL Server to which to connect.</summary>
	/// <returns>The name of the instance of SQL Server to which to connect. The default value is an empty string.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string DataSource
	{
		get
		{
			if (!(InnerConnection is SqlInternalConnection { CurrentDataSource: var currentDataSource }))
			{
				SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
				return (sqlConnectionString != null) ? sqlConnectionString.DataSource : "";
			}
			return currentDataSource;
		}
	}

	/// <summary>Gets the size (in bytes) of network packets used to communicate with an instance of SQL Server.</summary>
	/// <returns>The size (in bytes) of network packets. The default value is 8000.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int PacketSize
	{
		get
		{
			if (!(InnerConnection is SqlInternalConnectionTds { PacketSize: var packetSize }))
			{
				return ((SqlConnectionString)ConnectionOptions)?.PacketSize ?? 8000;
			}
			return packetSize;
		}
	}

	/// <summary>The connection ID of the most recent connection attempt, regardless of whether the attempt succeeded or failed.</summary>
	/// <returns>The connection ID of the most recent connection attempt.</returns>
	public Guid ClientConnectionId
	{
		get
		{
			if (InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds)
			{
				return sqlInternalConnectionTds.ClientConnectionId;
			}
			Task currentReconnectionTask = _currentReconnectionTask;
			if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
			{
				return _originalConnectionId;
			}
			return Guid.Empty;
		}
	}

	/// <summary>Gets a string that contains the version of the instance of SQL Server to which the client is connected.</summary>
	/// <returns>The version of the instance of SQL Server.</returns>
	/// <exception cref="T:System.InvalidOperationException">The connection is closed. <see cref="P:System.Data.SqlClient.SqlConnection.ServerVersion" /> was called while the returned Task was not completed and the connection was not opened after a call to <see cref="M:System.Data.SqlClient.SqlConnection.OpenAsync(System.Threading.CancellationToken)" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string ServerVersion => GetOpenTdsConnection().ServerVersion;

	/// <summary>Indicates the state of the <see cref="T:System.Data.SqlClient.SqlConnection" /> during the most recent network operation performed on the connection.</summary>
	/// <returns>An <see cref="T:System.Data.ConnectionState" /> enumeration.</returns>
	public override ConnectionState State
	{
		get
		{
			Task currentReconnectionTask = _currentReconnectionTask;
			if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
			{
				return ConnectionState.Open;
			}
			return InnerConnection.State;
		}
	}

	internal SqlStatistics Statistics => _statistics;

	/// <summary>Gets a string that identifies the database client.</summary>
	/// <returns>A string that identifies the database client. If not specified, the name of the client computer. If neither is specified, the value is an empty string.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string WorkstationId => ((SqlConnectionString)ConnectionOptions)?.WorkstationId ?? Environment.MachineName;

	/// <summary>Gets or sets the <see cref="T:System.Data.SqlClient.SqlCredential" /> object for this connection.</summary>
	/// <returns>The <see cref="T:System.Data.SqlClient.SqlCredential" /> object for this connection.</returns>
	public SqlCredential Credential
	{
		get
		{
			SqlCredential result = _credential;
			SqlConnectionString sqlConnectionString = (SqlConnectionString)UserConnectionOptions;
			if (InnerConnection.ShouldHidePassword && sqlConnectionString != null && !sqlConnectionString.PersistSecurityInfo)
			{
				result = null;
			}
			return result;
		}
		set
		{
			if (!InnerConnection.AllowSetConnectionString)
			{
				throw ADP.OpenConnectionPropertySet("Credential", InnerConnection.State);
			}
			if (value != null)
			{
				CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential((SqlConnectionString)ConnectionOptions);
				if (_accessToken != null)
				{
					throw ADP.InvalidMixedUsageOfCredentialAndAccessToken();
				}
			}
			_credential = value;
			ConnectionString_Set(new SqlConnectionPoolKey(_connectionString, _credential, _accessToken));
		}
	}

	protected override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

	/// <summary>Gets or sets the <see cref="P:System.Data.SqlClient.SqlConnection.FireInfoMessageEventOnUserErrors" /> property.</summary>
	/// <returns>true if the <see cref="P:System.Data.SqlClient.SqlConnection.FireInfoMessageEventOnUserErrors" /> property has been set; otherwise false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool FireInfoMessageEventOnUserErrors
	{
		get
		{
			return _fireInfoMessageEventOnUserErrors;
		}
		set
		{
			_fireInfoMessageEventOnUserErrors = value;
		}
	}

	internal int ReconnectCount => _reconnectCount;

	internal bool ForceNewConnection { get; set; }

	internal bool HasLocalTransaction => GetOpenTdsConnection().HasLocalTransaction;

	internal bool HasLocalTransactionFromAPI
	{
		get
		{
			Task currentReconnectionTask = _currentReconnectionTask;
			if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
			{
				return false;
			}
			return GetOpenTdsConnection().HasLocalTransactionFromAPI;
		}
	}

	internal bool IsKatmaiOrNewer
	{
		get
		{
			if (_currentReconnectionTask != null)
			{
				return true;
			}
			return GetOpenTdsConnection().IsKatmaiOrNewer;
		}
	}

	internal TdsParser Parser => GetOpenTdsConnection().Parser;

	internal int CloseCount => _closeCount;

	internal DbConnectionFactory ConnectionFactory => s_connectionFactory;

	internal DbConnectionOptions ConnectionOptions => PoolGroup?.ConnectionOptions;

	internal DbConnectionInternal InnerConnection => _innerConnection;

	internal DbConnectionPoolGroup PoolGroup
	{
		get
		{
			return _poolGroup;
		}
		set
		{
			_poolGroup = value;
		}
	}

	internal DbConnectionOptions UserConnectionOptions => _userConnectionOptions;

	[System.MonoTODO]
	public SqlCredential Credentials
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public static TimeSpan ColumnEncryptionKeyCacheTtl
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TimeSpan);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	public static bool ColumnEncryptionQueryMetadataCacheEnabled
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	public static IDictionary<string, IList<string>> ColumnEncryptionTrustedMasterKeyPaths
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IDictionary<string, IList<string>>)0;
		}
	}

	/// <summary>Occurs when SQL Server returns a warning or informational message.</summary>
	/// <filterpriority>2</filterpriority>
	public event SqlInfoMessageEventHandler InfoMessage;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlConnection" /> class when given a string that contains the connection string.</summary>
	/// <param name="connectionString">The connection used to open the SQL Server database.</param>
	public SqlConnection(string connectionString)
		: this()
	{
		ConnectionString = connectionString;
		CacheConnectionStringProperties();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlConnection" /> class given a connection string, that does not use Integrated Security = true and a <see cref="T:System.Data.SqlClient.SqlCredential" /> object that contains the user ID and password.</summary>
	/// <param name="connectionString">A connection string that does not use any of the following connection string keywords: Integrated Security = true, UserId, or Password; or that does not use ContextConnection = true.</param>
	/// <param name="credential">A <see cref="T:System.Data.SqlClient.SqlCredential" /> object. If <paramref name="credential" /> is null, <see cref="M:System.Data.SqlClient.SqlConnection.#ctor(System.String,System.Data.SqlClient.SqlCredential)" /> is functionally equivalent to <see cref="M:System.Data.SqlClient.SqlConnection.#ctor(System.String)" />.</param>
	public SqlConnection(string connectionString, SqlCredential credential)
		: this()
	{
		ConnectionString = connectionString;
		if (credential != null)
		{
			SqlConnectionString opt = (SqlConnectionString)ConnectionOptions;
			if (UsesClearUserIdOrPassword(opt))
			{
				throw ADP.InvalidMixedArgumentOfSecureAndClearCredential();
			}
			if (UsesIntegratedSecurity(opt))
			{
				throw ADP.InvalidMixedArgumentOfSecureCredentialAndIntegratedSecurity();
			}
			Credential = credential;
		}
		CacheConnectionStringProperties();
	}

	private SqlConnection(SqlConnection connection)
	{
		GC.SuppressFinalize(this);
		CopyFrom(connection);
		_connectionString = connection._connectionString;
		if (connection._credential != null)
		{
			SecureString secureString = connection._credential.Password.Copy();
			secureString.MakeReadOnly();
			_credential = new SqlCredential(connection._credential.UserId, secureString);
		}
		_accessToken = connection._accessToken;
		CacheConnectionStringProperties();
	}

	private void CacheConnectionStringProperties()
	{
		if (ConnectionOptions is SqlConnectionString sqlConnectionString)
		{
			_connectRetryCount = sqlConnectionString.ConnectRetryCount;
		}
	}

	private bool UsesIntegratedSecurity(SqlConnectionString opt)
	{
		return opt?.IntegratedSecurity ?? false;
	}

	private bool UsesClearUserIdOrPassword(SqlConnectionString opt)
	{
		bool result = false;
		if (opt != null)
		{
			result = !string.IsNullOrEmpty(opt.UserID) || !string.IsNullOrEmpty(opt.Password);
		}
		return result;
	}

	private void CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential(SqlConnectionString connectionOptions)
	{
		if (UsesClearUserIdOrPassword(connectionOptions))
		{
			throw ADP.InvalidMixedUsageOfSecureAndClearCredential();
		}
		if (UsesIntegratedSecurity(connectionOptions))
		{
			throw ADP.InvalidMixedUsageOfSecureCredentialAndIntegratedSecurity();
		}
	}

	private void CheckAndThrowOnInvalidCombinationOfConnectionOptionAndAccessToken(SqlConnectionString connectionOptions)
	{
		if (UsesClearUserIdOrPassword(connectionOptions))
		{
			throw ADP.InvalidMixedUsageOfAccessTokenAndUserIDPassword();
		}
		if (UsesIntegratedSecurity(connectionOptions))
		{
			throw ADP.InvalidMixedUsageOfAccessTokenAndIntegratedSecurity();
		}
		if (_credential != null)
		{
			throw ADP.InvalidMixedUsageOfCredentialAndAccessToken();
		}
	}

	protected override void OnStateChange(StateChangeEventArgs stateChange)
	{
		if (!_suppressStateChangeForReconnection)
		{
			base.OnStateChange(stateChange);
		}
	}

	/// <summary>Starts a database transaction.</summary>
	/// <returns>An object representing the new transaction.</returns>
	/// <exception cref="T:System.Data.SqlClient.SqlException">Parallel transactions are not allowed when using Multiple Active Result Sets (MARS).</exception>
	/// <exception cref="T:System.InvalidOperationException">Parallel transactions are not supported. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy, ControlAppDomain" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Data.SqlClient.SqlClientPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public new SqlTransaction BeginTransaction()
	{
		return BeginTransaction(IsolationLevel.Unspecified, null);
	}

	/// <summary>Starts a database transaction with the specified isolation level.</summary>
	/// <returns>An object representing the new transaction.</returns>
	/// <param name="iso">The isolation level under which the transaction should run. </param>
	/// <exception cref="T:System.Data.SqlClient.SqlException">Parallel transactions are not allowed when using Multiple Active Result Sets (MARS).</exception>
	/// <exception cref="T:System.InvalidOperationException">Parallel transactions are not supported. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy, ControlAppDomain" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Data.SqlClient.SqlClientPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public new SqlTransaction BeginTransaction(IsolationLevel iso)
	{
		return BeginTransaction(iso, null);
	}

	/// <summary>Starts a database transaction with the specified transaction name.</summary>
	/// <returns>An object representing the new transaction.</returns>
	/// <param name="transactionName">The name of the transaction. </param>
	/// <exception cref="T:System.Data.SqlClient.SqlException">Parallel transactions are not allowed when using Multiple Active Result Sets (MARS).</exception>
	/// <exception cref="T:System.InvalidOperationException">Parallel transactions are not supported. </exception>
	/// <filterpriority>2</filterpriority>
	public SqlTransaction BeginTransaction(string transactionName)
	{
		return BeginTransaction(IsolationLevel.Unspecified, transactionName);
	}

	protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
	{
		SqlTransaction result = BeginTransaction(isolationLevel);
		GC.KeepAlive(this);
		return result;
	}

	/// <summary>Starts a database transaction with the specified isolation level and transaction name.</summary>
	/// <returns>An object representing the new transaction.</returns>
	/// <param name="iso">The isolation level under which the transaction should run. </param>
	/// <param name="transactionName">The name of the transaction. </param>
	/// <exception cref="T:System.Data.SqlClient.SqlException">Parallel transactions are not allowed when using Multiple Active Result Sets (MARS).</exception>
	/// <exception cref="T:System.InvalidOperationException">Parallel transactions are not supported. </exception>
	/// <filterpriority>2</filterpriority>
	public SqlTransaction BeginTransaction(IsolationLevel iso, string transactionName)
	{
		WaitForPendingReconnection();
		SqlStatistics statistics = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			bool shouldReconnect = true;
			SqlTransaction sqlTransaction;
			do
			{
				sqlTransaction = GetOpenTdsConnection().BeginSqlTransaction(iso, transactionName, shouldReconnect);
				shouldReconnect = false;
			}
			while (sqlTransaction.InternalTransaction.ConnectionHasBeenRestored);
			GC.KeepAlive(this);
			return sqlTransaction;
		}
		finally
		{
			SqlStatistics.StopTimer(statistics);
		}
	}

	/// <summary>Changes the current database for an open <see cref="T:System.Data.SqlClient.SqlConnection" />.</summary>
	/// <param name="database">The name of the database to use instead of the current database. </param>
	/// <exception cref="T:System.ArgumentException">The database name is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">The connection is not open. </exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">Cannot change the database. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override void ChangeDatabase(string database)
	{
		SqlStatistics statistics = null;
		RepairInnerConnection();
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			InnerConnection.ChangeDatabase(database);
		}
		finally
		{
			SqlStatistics.StopTimer(statistics);
		}
	}

	/// <summary>Empties the connection pool.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void ClearAllPools()
	{
		SqlConnectionFactory.SingletonInstance.ClearAllPools();
	}

	/// <summary>Empties the connection pool associated with the specified connection.</summary>
	/// <param name="connection">The <see cref="T:System.Data.SqlClient.SqlConnection" /> to be cleared from the pool.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void ClearPool(SqlConnection connection)
	{
		ADP.CheckArgumentNull(connection, "connection");
		DbConnectionOptions userConnectionOptions = connection.UserConnectionOptions;
		if (userConnectionOptions != null)
		{
			SqlConnectionFactory.SingletonInstance.ClearPool(connection);
		}
	}

	private void CloseInnerConnection()
	{
		InnerConnection.CloseConnection(this, ConnectionFactory);
	}

	/// <summary>Closes the connection to the database. This is the preferred method of closing any open connection.</summary>
	/// <exception cref="T:System.Data.SqlClient.SqlException">The connection-level error that occurred while opening the connection. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Close()
	{
		ConnectionState state = State;
		Guid operationId = default(Guid);
		Guid clientConnectionId = default(Guid);
		if (state != ConnectionState.Closed)
		{
			operationId = s_diagnosticListener.WriteConnectionCloseBefore(this, "Close");
			clientConnectionId = ClientConnectionId;
		}
		SqlStatistics statistics = null;
		Exception ex = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			Task currentReconnectionTask = _currentReconnectionTask;
			if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
			{
				_reconnectionCancellationSource?.Cancel();
				AsyncHelper.WaitForCompletion(currentReconnectionTask, 0, null, rethrowExceptions: false);
				if (State != ConnectionState.Open)
				{
					OnStateChange(DbConnectionInternal.StateChangeClosed);
				}
			}
			CancelOpenAndWait();
			CloseInnerConnection();
			GC.SuppressFinalize(this);
			if (Statistics != null)
			{
				ADP.TimerCurrent(out _statistics._closeTimestamp);
			}
		}
		catch (Exception ex2)
		{
			ex = ex2;
			throw;
		}
		finally
		{
			SqlStatistics.StopTimer(statistics);
			if (state != ConnectionState.Closed)
			{
				if (ex != null)
				{
					s_diagnosticListener.WriteConnectionCloseError(operationId, clientConnectionId, this, ex, "Close");
				}
				else
				{
					s_diagnosticListener.WriteConnectionCloseAfter(operationId, clientConnectionId, this, "Close");
				}
			}
		}
	}

	/// <summary>Creates and returns a <see cref="T:System.Data.SqlClient.SqlCommand" /> object associated with the <see cref="T:System.Data.SqlClient.SqlConnection" />.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlCommand" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public new SqlCommand CreateCommand()
	{
		return new SqlCommand(null, this);
	}

	private void DisposeMe(bool disposing)
	{
		_credential = null;
		_accessToken = null;
		if (!disposing && InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds && !sqlInternalConnectionTds.ConnectionOptions.Pooling)
		{
			TdsParser parser = sqlInternalConnectionTds.Parser;
			if (parser != null && parser._physicalStateObj != null)
			{
				parser._physicalStateObj.DecrementPendingCallbacks(release: false);
			}
		}
	}

	/// <summary>Opens a database connection with the property settings specified by the <see cref="P:System.Data.SqlClient.SqlConnection.ConnectionString" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">Cannot open a connection without specifying a data source or server.orThe connection is already open.</exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">A connection-level error occurred while opening the connection. If the <see cref="P:System.Data.SqlClient.SqlException.Number" /> property contains the value 18487 or 18488, this indicates that the specified password has expired or must be reset. See the <see cref="M:System.Data.SqlClient.SqlConnection.ChangePassword(System.String,System.String)" /> method for more information.The &lt;system.data.localdb&gt; tag in the app.config file has invalid or unknown elements.</exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">There are two entries with the same name in the &lt;localdbinstances&gt; section.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override void Open()
	{
		Guid operationId = s_diagnosticListener.WriteConnectionOpenBefore(this, "Open");
		PrepareStatisticsForNewConnection();
		SqlStatistics statistics = null;
		Exception ex = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			if (!TryOpen(null))
			{
				throw ADP.InternalError(ADP.InternalErrorCode.SynchronousConnectReturnedPending);
			}
		}
		catch (Exception ex2)
		{
			ex = ex2;
			throw;
		}
		finally
		{
			SqlStatistics.StopTimer(statistics);
			if (ex != null)
			{
				s_diagnosticListener.WriteConnectionOpenError(operationId, this, ex, "Open");
			}
			else
			{
				s_diagnosticListener.WriteConnectionOpenAfter(operationId, this, "Open");
			}
		}
	}

	internal void RegisterWaitingForReconnect(Task waitingTask)
	{
		if (!((SqlConnectionString)ConnectionOptions).MARS)
		{
			Interlocked.CompareExchange(ref _asyncWaitingForReconnection, waitingTask, null);
			if (_asyncWaitingForReconnection != waitingTask)
			{
				throw SQL.MARSUnspportedOnConnection();
			}
		}
	}

	private async Task ReconnectAsync(int timeout)
	{
		_ = 1;
		try
		{
			long commandTimeoutExpiration = 0L;
			if (timeout > 0)
			{
				commandTimeoutExpiration = ADP.TimerCurrent() + ADP.TimerFromSeconds(timeout);
			}
			CancellationToken ctoken = (_reconnectionCancellationSource = new CancellationTokenSource()).Token;
			int retryCount = _connectRetryCount;
			for (int attempt = 0; attempt < retryCount; attempt++)
			{
				if (ctoken.IsCancellationRequested)
				{
					break;
				}
				try
				{
					try
					{
						ForceNewConnection = true;
						await OpenAsync(ctoken).ConfigureAwait(continueOnCapturedContext: false);
						_reconnectCount++;
						break;
					}
					finally
					{
						ForceNewConnection = false;
					}
				}
				catch (SqlException innerException)
				{
					if (attempt == retryCount - 1)
					{
						throw SQL.CR_AllAttemptsFailed(innerException, _originalConnectionId);
					}
					if (timeout > 0 && ADP.TimerRemaining(commandTimeoutExpiration) < ADP.TimerFromSeconds(ConnectRetryInterval))
					{
						throw SQL.CR_NextAttemptWillExceedQueryTimeout(innerException, _originalConnectionId);
					}
				}
				await Task.Delay(1000 * ConnectRetryInterval, ctoken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			_recoverySessionData = null;
			_suppressStateChangeForReconnection = false;
		}
	}

	internal Task ValidateAndReconnect(Action beforeDisconnect, int timeout)
	{
		Task task = _currentReconnectionTask;
		while (task != null && task.IsCompleted)
		{
			Interlocked.CompareExchange(ref _currentReconnectionTask, null, task);
			task = _currentReconnectionTask;
		}
		if (task == null)
		{
			if (_connectRetryCount > 0)
			{
				SqlInternalConnectionTds openTdsConnection = GetOpenTdsConnection();
				if (openTdsConnection._sessionRecoveryAcknowledged && !openTdsConnection.Parser._physicalStateObj.ValidateSNIConnection())
				{
					if (openTdsConnection.Parser._sessionPool != null && openTdsConnection.Parser._sessionPool.ActiveSessionsCount > 0)
					{
						beforeDisconnect?.Invoke();
						OnError(SQL.CR_UnrecoverableClient(ClientConnectionId), breakConnection: true, null);
					}
					SessionData currentSessionData = openTdsConnection.CurrentSessionData;
					if (currentSessionData._unrecoverableStatesCount == 0)
					{
						bool flag = false;
						lock (_reconnectLock)
						{
							openTdsConnection.CheckEnlistedTransactionBinding();
							task = _currentReconnectionTask;
							if (task == null)
							{
								if (currentSessionData._unrecoverableStatesCount == 0)
								{
									_originalConnectionId = ClientConnectionId;
									_recoverySessionData = currentSessionData;
									beforeDisconnect?.Invoke();
									try
									{
										_suppressStateChangeForReconnection = true;
										openTdsConnection.DoomThisConnection();
									}
									catch (SqlException)
									{
									}
									task = (_currentReconnectionTask = Task.Run(() => ReconnectAsync(timeout)));
								}
							}
							else
							{
								flag = true;
							}
						}
						if (flag)
						{
							beforeDisconnect?.Invoke();
						}
					}
					else
					{
						beforeDisconnect?.Invoke();
						OnError(SQL.CR_UnrecoverableServer(ClientConnectionId), breakConnection: true, null);
					}
				}
			}
		}
		else
		{
			beforeDisconnect?.Invoke();
		}
		return task;
	}

	private void WaitForPendingReconnection()
	{
		Task currentReconnectionTask = _currentReconnectionTask;
		if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
		{
			AsyncHelper.WaitForCompletion(currentReconnectionTask, 0, null, rethrowExceptions: false);
		}
	}

	private void CancelOpenAndWait()
	{
		Tuple<TaskCompletionSource<DbConnectionInternal>, Task> currentCompletion = _currentCompletion;
		if (currentCompletion != null)
		{
			currentCompletion.Item1.TrySetCanceled();
			((IAsyncResult)currentCompletion.Item2).AsyncWaitHandle.WaitOne();
		}
	}

	/// <summary>An asynchronous version of <see cref="M:System.Data.SqlClient.SqlConnection.Open" />, which opens a database connection with the property settings specified by the <see cref="P:System.Data.SqlClient.SqlConnection.ConnectionString" />. The cancellation token can be used to request that the operation be abandoned before the connection timeout elapses.  Exceptions will be propagated via the returned Task. If the connection timeout time elapses without successfully connecting, the returned Task will be marked as faulted with an Exception. The implementation returns a Task without blocking the calling thread for both pooled and non-pooled connections.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">The cancellation instruction.</param>
	/// <exception cref="T:System.InvalidOperationException">Calling <see cref="M:System.Data.SqlClient.SqlConnection.OpenAsync(System.Threading.CancellationToken)" /> more than once for the same instance before task completion.Context Connection=true is specified in the connection string.A connection was not available from the connection pool before the connection time out elapsed.</exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">Any error returned by SQL Server that occurred while opening the connection.</exception>
	public override Task OpenAsync(CancellationToken cancellationToken)
	{
		Guid operationId = s_diagnosticListener.WriteConnectionOpenBefore(this, "OpenAsync");
		PrepareStatisticsForNewConnection();
		SqlStatistics statistics = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			TaskCompletionSource<DbConnectionInternal> taskCompletionSource = new TaskCompletionSource<DbConnectionInternal>(ADP.GetCurrentTransaction());
			TaskCompletionSource<object> taskCompletionSource2 = new TaskCompletionSource<object>();
			if (s_diagnosticListener.IsEnabled("System.Data.SqlClient.WriteConnectionOpenAfter") || s_diagnosticListener.IsEnabled("System.Data.SqlClient.WriteConnectionOpenError"))
			{
				taskCompletionSource2.Task.ContinueWith(delegate(Task<object> t)
				{
					if (t.Exception != null)
					{
						s_diagnosticListener.WriteConnectionOpenError(operationId, this, t.Exception, "OpenAsync");
					}
					else
					{
						s_diagnosticListener.WriteConnectionOpenAfter(operationId, this, "OpenAsync");
					}
				}, TaskScheduler.Default);
			}
			if (cancellationToken.IsCancellationRequested)
			{
				taskCompletionSource2.SetCanceled();
				return taskCompletionSource2.Task;
			}
			bool flag;
			try
			{
				flag = TryOpen(taskCompletionSource);
			}
			catch (Exception ex)
			{
				s_diagnosticListener.WriteConnectionOpenError(operationId, this, ex, "OpenAsync");
				taskCompletionSource2.SetException(ex);
				return taskCompletionSource2.Task;
			}
			if (flag)
			{
				taskCompletionSource2.SetResult(null);
				return taskCompletionSource2.Task;
			}
			CancellationTokenRegistration registration = default(CancellationTokenRegistration);
			if (cancellationToken.CanBeCanceled)
			{
				registration = cancellationToken.Register(delegate(object s)
				{
					((TaskCompletionSource<DbConnectionInternal>)s).TrySetCanceled();
				}, taskCompletionSource);
			}
			OpenAsyncRetry openAsyncRetry = new OpenAsyncRetry(this, taskCompletionSource, taskCompletionSource2, registration);
			_currentCompletion = new Tuple<TaskCompletionSource<DbConnectionInternal>, Task>(taskCompletionSource, taskCompletionSource2.Task);
			taskCompletionSource.Task.ContinueWith(openAsyncRetry.Retry, TaskScheduler.Default);
			return taskCompletionSource2.Task;
		}
		catch (Exception ex2)
		{
			s_diagnosticListener.WriteConnectionOpenError(operationId, this, ex2, "OpenAsync");
			throw;
		}
		finally
		{
			SqlStatistics.StopTimer(statistics);
		}
	}

	/// <summary>Returns schema information for the data source of this <see cref="T:System.Data.SqlClient.SqlConnection" />. For more information about scheme, see SQL Server Schema Collections.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that contains schema information.</returns>
	public override DataTable GetSchema()
	{
		return GetSchema(DbMetaDataCollectionNames.MetaDataCollections, null);
	}

	/// <summary>Returns schema information for the data source of this <see cref="T:System.Data.SqlClient.SqlConnection" /> using the specified string for the schema name.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that contains schema information. </returns>
	/// <param name="collectionName">Specifies the name of the schema to return.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="collectionName" /> is specified as null.</exception>
	public override DataTable GetSchema(string collectionName)
	{
		return GetSchema(collectionName, null);
	}

	/// <summary>Returns schema information for the data source of this <see cref="T:System.Data.SqlClient.SqlConnection" /> using the specified string for the schema name and the specified string array for the restriction values.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that contains schema information. </returns>
	/// <param name="collectionName">Specifies the name of the schema to return.</param>
	/// <param name="restrictionValues">A set of restriction values for the requested schema.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="collectionName" /> is specified as null.</exception>
	public override DataTable GetSchema(string collectionName, string[] restrictionValues)
	{
		return InnerConnection.GetSchema(ConnectionFactory, PoolGroup, this, collectionName, restrictionValues);
	}

	private void PrepareStatisticsForNewConnection()
	{
		if (StatisticsEnabled || s_diagnosticListener.IsEnabled("System.Data.SqlClient.WriteCommandAfter") || s_diagnosticListener.IsEnabled("System.Data.SqlClient.WriteConnectionOpenAfter"))
		{
			if (_statistics == null)
			{
				_statistics = new SqlStatistics();
			}
			else
			{
				_statistics.ContinueOnNewConnection();
			}
		}
	}

	private bool TryOpen(TaskCompletionSource<DbConnectionInternal> retry)
	{
		SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
		_applyTransientFaultHandling = retry == null && sqlConnectionString != null && sqlConnectionString.ConnectRetryCount > 0;
		if (ForceNewConnection)
		{
			if (!InnerConnection.TryReplaceConnection(this, ConnectionFactory, retry, UserConnectionOptions))
			{
				return false;
			}
		}
		else if (!InnerConnection.TryOpenConnection(this, ConnectionFactory, retry, UserConnectionOptions))
		{
			return false;
		}
		SqlInternalConnectionTds sqlInternalConnectionTds = (SqlInternalConnectionTds)InnerConnection;
		if (!sqlInternalConnectionTds.ConnectionOptions.Pooling)
		{
			GC.ReRegisterForFinalize(this);
		}
		SqlStatistics statistics = _statistics;
		if (StatisticsEnabled || (s_diagnosticListener.IsEnabled("System.Data.SqlClient.WriteCommandAfter") && statistics != null))
		{
			ADP.TimerCurrent(out _statistics._openTimestamp);
			sqlInternalConnectionTds.Parser.Statistics = _statistics;
		}
		else
		{
			sqlInternalConnectionTds.Parser.Statistics = null;
			_statistics = null;
		}
		return true;
	}

	internal void ValidateConnectionForExecute(string method, SqlCommand command)
	{
		Task asyncWaitingForReconnection = _asyncWaitingForReconnection;
		if (asyncWaitingForReconnection != null)
		{
			if (!asyncWaitingForReconnection.IsCompleted)
			{
				throw SQL.MARSUnspportedOnConnection();
			}
			Interlocked.CompareExchange(ref _asyncWaitingForReconnection, null, asyncWaitingForReconnection);
		}
		if (_currentReconnectionTask != null)
		{
			Task currentReconnectionTask = _currentReconnectionTask;
			if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
			{
				return;
			}
		}
		GetOpenTdsConnection(method).ValidateConnectionForExecute(command);
	}

	internal static string FixupDatabaseTransactionName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return SqlServerEscapeHelper.EscapeIdentifier(name);
		}
		return name;
	}

	internal void OnError(SqlException exception, bool breakConnection, Action<Action> wrapCloseInAction)
	{
		if (breakConnection && ConnectionState.Open == State)
		{
			if (wrapCloseInAction != null)
			{
				int capturedCloseCount = _closeCount;
				Action obj = delegate
				{
					if (capturedCloseCount == _closeCount)
					{
						Close();
					}
				};
				wrapCloseInAction(obj);
			}
			else
			{
				Close();
			}
		}
		if (exception.Class >= 11)
		{
			throw exception;
		}
		OnInfoMessage(new SqlInfoMessageEventArgs(exception));
	}

	internal SqlInternalConnectionTds GetOpenTdsConnection()
	{
		if (!(InnerConnection is SqlInternalConnectionTds result))
		{
			throw ADP.ClosedConnectionError();
		}
		return result;
	}

	internal SqlInternalConnectionTds GetOpenTdsConnection(string method)
	{
		if (!(InnerConnection is SqlInternalConnectionTds result))
		{
			throw ADP.OpenConnectionRequired(method, InnerConnection.State);
		}
		return result;
	}

	internal void OnInfoMessage(SqlInfoMessageEventArgs imevent)
	{
		OnInfoMessage(imevent, out var _);
	}

	internal void OnInfoMessage(SqlInfoMessageEventArgs imevent, out bool notified)
	{
		SqlInfoMessageEventHandler sqlInfoMessageEventHandler = this.InfoMessage;
		if (sqlInfoMessageEventHandler != null)
		{
			notified = true;
			try
			{
				sqlInfoMessageEventHandler(this, imevent);
				return;
			}
			catch (Exception e)
			{
				if (!ADP.IsCatchableOrSecurityExceptionType(e))
				{
					throw;
				}
				return;
			}
		}
		notified = false;
	}

	/// <summary>Changes the SQL Server password for the user indicated in the connection string to the supplied new password.</summary>
	/// <param name="connectionString">The connection string that contains enough information to connect to the server that you want. The connection string must contain the user ID and the current password.</param>
	/// <param name="newPassword">The new password to set. This password must comply with any password security policy set on the server, including minimum length, requirements for specific characters, and so on.</param>
	/// <exception cref="T:System.ArgumentException">The connection string includes the option to use integrated security. Or The <paramref name="newPassword" /> exceeds 128 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either the <paramref name="connectionString" /> or the <paramref name="newPassword" /> parameter is null.</exception>
	/// <filterpriority>1</filterpriority>
	public static void ChangePassword(string connectionString, string newPassword)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			throw SQL.ChangePasswordArgumentMissing("newPassword");
		}
		if (string.IsNullOrEmpty(newPassword))
		{
			throw SQL.ChangePasswordArgumentMissing("newPassword");
		}
		if (128 < newPassword.Length)
		{
			throw ADP.InvalidArgumentLength("newPassword", 128);
		}
		SqlConnectionString sqlConnectionString = SqlConnectionFactory.FindSqlConnectionOptions(new SqlConnectionPoolKey(connectionString, null, null));
		if (sqlConnectionString.IntegratedSecurity)
		{
			throw SQL.ChangePasswordConflictsWithSSPI();
		}
		if (!string.IsNullOrEmpty(sqlConnectionString.AttachDBFilename))
		{
			throw SQL.ChangePasswordUseOfUnallowedKey("attachdbfilename");
		}
		ChangePassword(connectionString, sqlConnectionString, null, newPassword, null);
	}

	/// <summary>Changes the SQL Server password for the user indicated in the <see cref="T:System.Data.SqlClient.SqlCredential" /> object.</summary>
	/// <param name="connectionString">The connection string that contains enough information to connect to a server. The connection string should not use any of the following connection string keywords: Integrated Security = true, UserId, or Password; or ContextConnection = true.</param>
	/// <param name="credential">A <see cref="T:System.Data.SqlClient.SqlCredential" /> object.</param>
	/// <param name="newSecurePassword">The new password. <paramref name="newSecurePassword" /> must be read only. The password must also comply with any password security policy set on the server (for example, minimum length and requirements for specific characters).</param>
	/// <exception cref="T:System.ArgumentException">The connection string contains any combination of UserId, Password, or Integrated Security=true.The connection string contains Context Connection=true.<paramref name="newSecurePassword" /> is greater than 128 characters.<paramref name="newSecurePassword" /> is not read only.<paramref name="newSecurePassword" /> is an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">One of the parameters (<paramref name="connectionString" />, <paramref name="credential" />, or <paramref name="newSecurePassword" />) is null.</exception>
	public static void ChangePassword(string connectionString, SqlCredential credential, SecureString newSecurePassword)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			throw SQL.ChangePasswordArgumentMissing("connectionString");
		}
		if (credential == null)
		{
			throw SQL.ChangePasswordArgumentMissing("credential");
		}
		if (newSecurePassword == null || newSecurePassword.Length == 0)
		{
			throw SQL.ChangePasswordArgumentMissing("newSecurePassword");
		}
		if (!newSecurePassword.IsReadOnly())
		{
			throw ADP.MustBeReadOnly("newSecurePassword");
		}
		if (128 < newSecurePassword.Length)
		{
			throw ADP.InvalidArgumentLength("newSecurePassword", 128);
		}
		SqlConnectionString sqlConnectionString = SqlConnectionFactory.FindSqlConnectionOptions(new SqlConnectionPoolKey(connectionString, null, null));
		if (!string.IsNullOrEmpty(sqlConnectionString.UserID) || !string.IsNullOrEmpty(sqlConnectionString.Password))
		{
			throw ADP.InvalidMixedArgumentOfSecureAndClearCredential();
		}
		if (sqlConnectionString.IntegratedSecurity)
		{
			throw SQL.ChangePasswordConflictsWithSSPI();
		}
		if (!string.IsNullOrEmpty(sqlConnectionString.AttachDBFilename))
		{
			throw SQL.ChangePasswordUseOfUnallowedKey("attachdbfilename");
		}
		ChangePassword(connectionString, sqlConnectionString, credential, null, newSecurePassword);
	}

	private static void ChangePassword(string connectionString, SqlConnectionString connectionOptions, SqlCredential credential, string newPassword, SecureString newSecurePassword)
	{
		SqlInternalConnectionTds sqlInternalConnectionTds = null;
		try
		{
			sqlInternalConnectionTds = new SqlInternalConnectionTds(null, connectionOptions, credential, null, newPassword, newSecurePassword, redirectedUserInstance: false);
		}
		finally
		{
			sqlInternalConnectionTds?.Dispose();
		}
		SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, null, null);
		SqlConnectionFactory.SingletonInstance.ClearPool(key);
	}

	internal void RegisterForConnectionCloseNotification<T>(ref Task<T> outerTask, object value, int tag)
	{
		outerTask = outerTask.ContinueWith(delegate(Task<T> task)
		{
			RemoveWeakReference(value);
			return task;
		}, TaskScheduler.Default).Unwrap();
	}

	/// <summary>If statistics gathering is enabled, all values are reset to zero.</summary>
	/// <filterpriority>2</filterpriority>
	public void ResetStatistics()
	{
		if (Statistics != null)
		{
			Statistics.Reset();
			if (ConnectionState.Open == State)
			{
				ADP.TimerCurrent(out _statistics._openTimestamp);
			}
		}
	}

	/// <summary>Returns a name value pair collection of statistics at the point in time the method is called.</summary>
	/// <returns>Returns a reference of type <see cref="T:System.Collections.IDictionary" /> of <see cref="T:System.Collections.DictionaryEntry" /> items.</returns>
	/// <filterpriority>2</filterpriority>
	public IDictionary RetrieveStatistics()
	{
		if (Statistics != null)
		{
			UpdateStatistics();
			return Statistics.GetDictionary();
		}
		return new SqlStatistics().GetDictionary();
	}

	private void UpdateStatistics()
	{
		if (ConnectionState.Open == State)
		{
			ADP.TimerCurrent(out _statistics._closeTimestamp);
		}
		Statistics.UpdateStatistics();
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		return new SqlConnection(this);
	}

	private void CopyFrom(SqlConnection connection)
	{
		ADP.CheckArgumentNull(connection, "connection");
		_userConnectionOptions = connection.UserConnectionOptions;
		_poolGroup = connection.PoolGroup;
		if (DbConnectionClosedNeverOpened.SingletonInstance == connection._innerConnection)
		{
			_innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
		}
		else
		{
			_innerConnection = DbConnectionClosedPreviouslyOpened.SingletonInstance;
		}
	}

	private Assembly ResolveTypeAssembly(AssemblyName asmRef, bool throwOnError)
	{
		if (string.Compare(asmRef.Name, "Microsoft.SqlServer.Types", StringComparison.OrdinalIgnoreCase) == 0)
		{
			asmRef.Version = TypeSystemAssemblyVersion;
		}
		try
		{
			return Assembly.Load(asmRef);
		}
		catch (Exception e)
		{
			if (throwOnError || !ADP.IsCatchableExceptionType(e))
			{
				throw;
			}
			return null;
		}
	}

	internal void CheckGetExtendedUDTInfo(SqlMetaDataPriv metaData, bool fThrow)
	{
		if (metaData.udtType == null)
		{
			metaData.udtType = Type.GetType(metaData.udtAssemblyQualifiedName, (AssemblyName asmRef) => ResolveTypeAssembly(asmRef, fThrow), null, fThrow);
			if (fThrow && metaData.udtType == null)
			{
				throw SQL.UDTUnexpectedResult(metaData.udtAssemblyQualifiedName);
			}
		}
	}

	internal object GetUdtValue(object value, SqlMetaDataPriv metaData, bool returnDBNull)
	{
		if (returnDBNull && ADP.IsNull(value))
		{
			return DBNull.Value;
		}
		if (ADP.IsNull(value))
		{
			return metaData.udtType.InvokeMember("Null", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty, null, null, new object[0], CultureInfo.InvariantCulture);
		}
		return SerializationHelperSql9.Deserialize(new MemoryStream((byte[])value), metaData.udtType);
	}

	internal byte[] GetBytes(object o)
	{
		Format format = Format.Native;
		int maxSize;
		return GetBytes(o, out format, out maxSize);
	}

	internal byte[] GetBytes(object o, out Format format, out int maxSize)
	{
		SqlUdtInfo infoFromType = GetInfoFromType(o.GetType());
		maxSize = infoFromType.MaxByteSize;
		format = infoFromType.SerializationFormat;
		if (maxSize < -1 || maxSize >= 65535)
		{
			throw new InvalidOperationException(o.GetType()?.ToString() + ": invalid Size");
		}
		using MemoryStream memoryStream = new MemoryStream((maxSize >= 0) ? maxSize : 0);
		SerializationHelperSql9.Serialize(memoryStream, o);
		return memoryStream.ToArray();
	}

	private SqlUdtInfo GetInfoFromType(Type t)
	{
		Type type = t;
		do
		{
			SqlUdtInfo sqlUdtInfo = SqlUdtInfo.TryGetFromType(t);
			if (sqlUdtInfo != null)
			{
				return sqlUdtInfo;
			}
			t = t.BaseType;
		}
		while (t != null);
		throw SQL.UDTInvalidSqlType(type.AssemblyQualifiedName);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlConnection" /> class.</summary>
	public SqlConnection()
	{
		GC.SuppressFinalize(this);
		_innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
	}

	private string ConnectionString_Get()
	{
		bool shouldHidePassword = InnerConnection.ShouldHidePassword;
		DbConnectionOptions userConnectionOptions = UserConnectionOptions;
		if (userConnectionOptions == null)
		{
			return "";
		}
		return userConnectionOptions.UsersConnectionString(shouldHidePassword);
	}

	private void ConnectionString_Set(DbConnectionPoolKey key)
	{
		DbConnectionOptions userConnectionOptions = null;
		DbConnectionPoolGroup connectionPoolGroup = ConnectionFactory.GetConnectionPoolGroup(key, null, ref userConnectionOptions);
		DbConnectionInternal innerConnection = InnerConnection;
		bool flag = innerConnection.AllowSetConnectionString;
		if (flag)
		{
			flag = SetInnerConnectionFrom(DbConnectionClosedBusy.SingletonInstance, innerConnection);
			if (flag)
			{
				_userConnectionOptions = userConnectionOptions;
				_poolGroup = connectionPoolGroup;
				_innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
			}
		}
		if (!flag)
		{
			throw ADP.OpenConnectionPropertySet("ConnectionString", innerConnection.State);
		}
	}

	internal void Abort(Exception e)
	{
		DbConnectionInternal innerConnection = _innerConnection;
		if (ConnectionState.Open == innerConnection.State)
		{
			Interlocked.CompareExchange(ref _innerConnection, DbConnectionClosedPreviouslyOpened.SingletonInstance, innerConnection);
			innerConnection.DoomThisConnection();
		}
	}

	internal void AddWeakReference(object value, int tag)
	{
		InnerConnection.AddWeakReference(value, tag);
	}

	protected override DbCommand CreateDbCommand()
	{
		DbCommand dbCommand = ConnectionFactory.ProviderFactory.CreateCommand();
		dbCommand.Connection = this;
		return dbCommand;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_userConnectionOptions = null;
			_poolGroup = null;
			Close();
		}
		DisposeMe(disposing);
		base.Dispose(disposing);
	}

	private void RepairInnerConnection()
	{
		WaitForPendingReconnection();
		if (_connectRetryCount != 0 && InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds)
		{
			sqlInternalConnectionTds.ValidateConnectionForExecute(null);
			sqlInternalConnectionTds.GetSessionAndReconnectIfNeeded(this);
		}
	}

	/// <summary>Enlists in the specified transaction as a distributed transaction.</summary>
	/// <param name="transaction">A reference to an existing <see cref="T:System.Transactions.Transaction" /> in which to enlist.</param>
	public override void EnlistTransaction(Transaction transaction)
	{
		Transaction enlistedTransaction = InnerConnection.EnlistedTransaction;
		if (enlistedTransaction != null)
		{
			if (enlistedTransaction.Equals(transaction))
			{
				return;
			}
			if (enlistedTransaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Active)
			{
				throw ADP.TransactionPresent();
			}
		}
		RepairInnerConnection();
		InnerConnection.EnlistTransaction(transaction);
		GC.KeepAlive(this);
	}

	internal void NotifyWeakReference(int message)
	{
		InnerConnection.NotifyWeakReference(message);
	}

	internal void PermissionDemand()
	{
		DbConnectionOptions dbConnectionOptions = PoolGroup?.ConnectionOptions;
		if (dbConnectionOptions == null || dbConnectionOptions.IsEmpty)
		{
			throw ADP.NoConnectionString();
		}
		_ = UserConnectionOptions;
	}

	internal void RemoveWeakReference(object value)
	{
		InnerConnection.RemoveWeakReference(value);
	}

	internal void SetInnerConnectionEvent(DbConnectionInternal to)
	{
		ConnectionState connectionState = _innerConnection.State & ConnectionState.Open;
		ConnectionState connectionState2 = to.State & ConnectionState.Open;
		if (connectionState != connectionState2 && connectionState2 == ConnectionState.Closed)
		{
			_closeCount++;
		}
		_innerConnection = to;
		if (connectionState == ConnectionState.Closed && ConnectionState.Open == connectionState2)
		{
			OnStateChange(DbConnectionInternal.StateChangeOpen);
		}
		else if (ConnectionState.Open == connectionState && connectionState2 == ConnectionState.Closed)
		{
			OnStateChange(DbConnectionInternal.StateChangeClosed);
		}
		else if (connectionState != connectionState2)
		{
			OnStateChange(new StateChangeEventArgs(connectionState, connectionState2));
		}
	}

	internal bool SetInnerConnectionFrom(DbConnectionInternal to, DbConnectionInternal from)
	{
		return from == Interlocked.CompareExchange(ref _innerConnection, to, from);
	}

	internal void SetInnerConnectionTo(DbConnectionInternal to)
	{
		_innerConnection = to;
	}

	/// <summary>Enlists in the specified transaction as a distributed transaction.</summary>
	/// <param name="transaction">A reference to an existing <see cref="T:System.EnterpriseServices.ITransaction" /> in which to enlist.</param>
	[System.MonoTODO]
	public void EnlistDistributedTransaction(ITransaction transaction)
	{
		throw new NotImplementedException();
	}

	public static void RegisterColumnEncryptionKeyStoreProviders(IDictionary<string, SqlColumnEncryptionKeyStoreProvider> customProviders)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
