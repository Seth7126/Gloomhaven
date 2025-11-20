using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Data.Odbc;

/// <summary>Represents an SQL statement or stored procedure to execute against a data source. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class OdbcCommand : DbCommand, ICloneable
{
	private static int s_objectTypeCount;

	internal readonly int ObjectID = Interlocked.Increment(ref s_objectTypeCount);

	private string _commandText;

	private CommandType _commandType;

	private int _commandTimeout = 30;

	private UpdateRowSource _updatedRowSource = UpdateRowSource.Both;

	private bool _designTimeInvisible;

	private bool _isPrepared;

	private OdbcConnection _connection;

	private OdbcTransaction _transaction;

	private WeakReference _weakDataReaderReference;

	private CMDWrapper _cmdWrapper;

	private OdbcParameterCollection _parameterCollection;

	private ConnectionState _cmdState;

	internal bool Canceling => _cmdWrapper.Canceling;

	/// <summary>Gets or sets the SQL statement or stored procedure to execute against the data source.</summary>
	/// <returns>The SQL statement or stored procedure to execute. The default value is an empty string ("").</returns>
	/// <filterpriority>1</filterpriority>
	public override string CommandText
	{
		get
		{
			string commandText = _commandText;
			if (commandText == null)
			{
				return ADP.StrEmpty;
			}
			return commandText;
		}
		set
		{
			if (_commandText != value)
			{
				PropertyChanging();
				_commandText = value;
			}
		}
	}

	/// <summary>Gets or sets the wait time before terminating an attempt to execute a command and generating an error.</summary>
	/// <returns>The time in seconds to wait for the command to execute. The default is 30 seconds.</returns>
	public override int CommandTimeout
	{
		get
		{
			return _commandTimeout;
		}
		set
		{
			if (value < 0)
			{
				throw ADP.InvalidCommandTimeout(value, "CommandTimeout");
			}
			if (value != _commandTimeout)
			{
				PropertyChanging();
				_commandTimeout = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates how the <see cref="P:System.Data.Odbc.OdbcCommand.CommandText" /> property is interpreted.</summary>
	/// <returns>One of the <see cref="T:System.Data.CommandType" /> values. The default is Text.</returns>
	/// <exception cref="T:System.ArgumentException">The value was not a valid <see cref="T:System.Data.CommandType" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(CommandType.Text)]
	public override CommandType CommandType
	{
		get
		{
			CommandType commandType = _commandType;
			if (commandType == (CommandType)0)
			{
				return CommandType.Text;
			}
			return commandType;
		}
		set
		{
			switch (value)
			{
			case CommandType.Text:
			case CommandType.StoredProcedure:
				PropertyChanging();
				_commandType = value;
				break;
			case CommandType.TableDirect:
				throw ODBC.NotSupportedCommandType(value);
			default:
				throw ADP.InvalidCommandType(value);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Odbc.OdbcConnection" /> used by this instance of the <see cref="T:System.Data.Odbc.OdbcCommand" />.</summary>
	/// <returns>The connection to a data source. The default is a null value.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Data.Odbc.OdbcCommand.Connection" /> property was changed while a transaction was in progress. </exception>
	/// <filterpriority>1</filterpriority>
	public new OdbcConnection Connection
	{
		get
		{
			return _connection;
		}
		set
		{
			if (value != _connection)
			{
				PropertyChanging();
				DisconnectFromDataReaderAndConnection();
				_connection = value;
			}
		}
	}

	protected override DbConnection DbConnection
	{
		get
		{
			return Connection;
		}
		set
		{
			Connection = (OdbcConnection)value;
		}
	}

	protected override DbParameterCollection DbParameterCollection => Parameters;

	protected override DbTransaction DbTransaction
	{
		get
		{
			return Transaction;
		}
		set
		{
			Transaction = (OdbcTransaction)value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the command object should be visible in a customized interface control.</summary>
	/// <returns>true, if the command object should be visible in a control; otherwise false. The default is true.</returns>
	[Browsable(false)]
	[DefaultValue(true)]
	[DesignOnly(true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool DesignTimeVisible
	{
		get
		{
			return !_designTimeInvisible;
		}
		set
		{
			_designTimeInvisible = !value;
			TypeDescriptor.Refresh(this);
		}
	}

	internal bool HasParameters => _parameterCollection != null;

	/// <summary>Gets the <see cref="T:System.Data.Odbc.OdbcParameterCollection" />.</summary>
	/// <returns>The parameters of the SQL statement or stored procedure. The default is an empty collection.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public new OdbcParameterCollection Parameters
	{
		get
		{
			if (_parameterCollection == null)
			{
				_parameterCollection = new OdbcParameterCollection();
			}
			return _parameterCollection;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Odbc.OdbcTransaction" /> within which the <see cref="T:System.Data.Odbc.OdbcCommand" /> executes.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcTransaction" />. The default is a null value.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new OdbcTransaction Transaction
	{
		get
		{
			if (_transaction != null && _transaction.Connection == null)
			{
				_transaction = null;
			}
			return _transaction;
		}
		set
		{
			if (_transaction != value)
			{
				PropertyChanging();
				_transaction = value;
			}
		}
	}

	/// <summary>Gets or sets a value that specifies how the Update method should apply command results to the DataRow.</summary>
	/// <returns>One of the <see cref="T:System.Data.UpdateRowSource" /> values.</returns>
	[DefaultValue(UpdateRowSource.Both)]
	public override UpdateRowSource UpdatedRowSource
	{
		get
		{
			return _updatedRowSource;
		}
		set
		{
			if ((uint)value <= 3u)
			{
				_updatedRowSource = value;
				return;
			}
			throw ADP.InvalidUpdateRowSource(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcCommand" /> class.</summary>
	public OdbcCommand()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcCommand" /> class with the text of the query.</summary>
	/// <param name="cmdText">The text of the query. </param>
	public OdbcCommand(string cmdText)
		: this()
	{
		CommandText = cmdText;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcCommand" /> class with the text of the query and an <see cref="T:System.Data.Odbc.OdbcConnection" /> object.</summary>
	/// <param name="cmdText">The text of the query. </param>
	/// <param name="connection">An <see cref="T:System.Data.Odbc.OdbcConnection" /> object that represents the connection to a data source. </param>
	public OdbcCommand(string cmdText, OdbcConnection connection)
		: this()
	{
		CommandText = cmdText;
		Connection = connection;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcCommand" /> class with the text of the query, an <see cref="T:System.Data.Odbc.OdbcConnection" /> object, and the <see cref="P:System.Data.Odbc.OdbcCommand.Transaction" />.</summary>
	/// <param name="cmdText">The text of the query. </param>
	/// <param name="connection">An <see cref="T:System.Data.Odbc.OdbcConnection" /> object that represents the connection to a data source. </param>
	/// <param name="transaction">The transaction in which the <see cref="T:System.Data.Odbc.OdbcCommand" /> executes. </param>
	public OdbcCommand(string cmdText, OdbcConnection connection, OdbcTransaction transaction)
		: this()
	{
		CommandText = cmdText;
		Connection = connection;
		Transaction = transaction;
	}

	private void DisposeDeadDataReader()
	{
		if (ConnectionState.Fetching == _cmdState && _weakDataReaderReference != null && !_weakDataReaderReference.IsAlive)
		{
			if (_cmdWrapper != null)
			{
				_cmdWrapper.FreeKeyInfoStatementHandle(ODBC32.STMT.CLOSE);
				_cmdWrapper.FreeStatementHandle(ODBC32.STMT.CLOSE);
			}
			CloseFromDataReader();
		}
	}

	private void DisposeDataReader()
	{
		if (_weakDataReaderReference != null)
		{
			IDisposable disposable = (IDisposable)_weakDataReaderReference.Target;
			if (disposable != null && _weakDataReaderReference.IsAlive)
			{
				disposable.Dispose();
			}
			CloseFromDataReader();
		}
	}

	internal void DisconnectFromDataReaderAndConnection()
	{
		OdbcDataReader odbcDataReader = null;
		if (_weakDataReaderReference != null)
		{
			OdbcDataReader odbcDataReader2 = (OdbcDataReader)_weakDataReaderReference.Target;
			if (_weakDataReaderReference.IsAlive)
			{
				odbcDataReader = odbcDataReader2;
			}
		}
		if (odbcDataReader != null)
		{
			odbcDataReader.Command = null;
		}
		_transaction = null;
		if (_connection != null)
		{
			_connection.RemoveWeakReference(this);
			_connection = null;
		}
		if (odbcDataReader == null)
		{
			CloseCommandWrapper();
		}
		_cmdWrapper = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			DisconnectFromDataReaderAndConnection();
			_parameterCollection = null;
			CommandText = null;
		}
		_cmdWrapper = null;
		_isPrepared = false;
		base.Dispose(disposing);
	}

	/// <summary>Resets the <see cref="P:System.Data.Odbc.OdbcCommand.CommandTimeout" /> property to the default value.</summary>
	public void ResetCommandTimeout()
	{
		if (30 != _commandTimeout)
		{
			PropertyChanging();
			_commandTimeout = 30;
		}
	}

	private bool ShouldSerializeCommandTimeout()
	{
		return 30 != _commandTimeout;
	}

	internal OdbcDescriptorHandle GetDescriptorHandle(ODBC32.SQL_ATTR attribute)
	{
		return _cmdWrapper.GetDescriptorHandle(attribute);
	}

	internal CMDWrapper GetStatementHandle()
	{
		if (_cmdWrapper == null)
		{
			_cmdWrapper = new CMDWrapper(_connection);
			_connection.AddWeakReference(this, 1);
		}
		if (_cmdWrapper._dataReaderBuf == null)
		{
			_cmdWrapper._dataReaderBuf = new CNativeBuffer(4096);
		}
		if (_cmdWrapper.StatementHandle == null)
		{
			_isPrepared = false;
			_cmdWrapper.CreateStatementHandle();
		}
		else if (_parameterCollection != null && _parameterCollection.RebindCollection)
		{
			_cmdWrapper.FreeStatementHandle(ODBC32.STMT.RESET_PARAMS);
		}
		return _cmdWrapper;
	}

	/// <summary>Tries to cancel the execution of an <see cref="T:System.Data.Odbc.OdbcCommand" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Cancel()
	{
		CMDWrapper cmdWrapper = _cmdWrapper;
		if (cmdWrapper == null)
		{
			return;
		}
		cmdWrapper.Canceling = true;
		OdbcStatementHandle statementHandle = cmdWrapper.StatementHandle;
		if (statementHandle == null)
		{
			return;
		}
		lock (statementHandle)
		{
			ODBC32.RetCode retCode = statementHandle.Cancel();
			if ((uint)retCode > 1u)
			{
				throw cmdWrapper.Connection.HandleErrorNoThrow(statementHandle, retCode);
			}
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.ICloneable.Clone" />.</summary>
	/// <returns>A new <see cref="T;System.Object" /> that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		OdbcCommand odbcCommand = new OdbcCommand();
		odbcCommand.CommandText = CommandText;
		odbcCommand.CommandTimeout = CommandTimeout;
		odbcCommand.CommandType = CommandType;
		odbcCommand.Connection = Connection;
		odbcCommand.Transaction = Transaction;
		odbcCommand.UpdatedRowSource = UpdatedRowSource;
		if (_parameterCollection != null && 0 < Parameters.Count)
		{
			OdbcParameterCollection parameters = odbcCommand.Parameters;
			foreach (ICloneable parameter in Parameters)
			{
				parameters.Add(parameter.Clone());
			}
		}
		return odbcCommand;
	}

	internal bool RecoverFromConnection()
	{
		DisposeDeadDataReader();
		return _cmdState == ConnectionState.Closed;
	}

	private void CloseCommandWrapper()
	{
		CMDWrapper cmdWrapper = _cmdWrapper;
		if (cmdWrapper == null)
		{
			return;
		}
		try
		{
			cmdWrapper.Dispose();
			if (_connection != null)
			{
				_connection.RemoveWeakReference(this);
			}
		}
		finally
		{
			_cmdWrapper = null;
		}
	}

	internal void CloseFromConnection()
	{
		if (_parameterCollection != null)
		{
			_parameterCollection.RebindCollection = true;
		}
		DisposeDataReader();
		CloseCommandWrapper();
		_isPrepared = false;
		_transaction = null;
	}

	internal void CloseFromDataReader()
	{
		_weakDataReaderReference = null;
		_cmdState = ConnectionState.Closed;
	}

	/// <summary>Creates a new instance of an <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcParameter" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public new OdbcParameter CreateParameter()
	{
		return new OdbcParameter();
	}

	protected override DbParameter CreateDbParameter()
	{
		return CreateParameter();
	}

	protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		return ExecuteReader(behavior);
	}

	/// <summary>Executes an SQL statement against the <see cref="P:System.Data.Odbc.OdbcCommand.Connection" /> and returns the number of rows affected.</summary>
	/// <returns>For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the command. For all other types of statements, the return value is -1.</returns>
	/// <exception cref="T:System.InvalidOperationException">The connection does not exist.-or- The connection is not open. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int ExecuteNonQuery()
	{
		using OdbcDataReader odbcDataReader = ExecuteReaderObject(CommandBehavior.Default, "ExecuteNonQuery", needReader: false);
		odbcDataReader.Close();
		return odbcDataReader.RecordsAffected;
	}

	/// <summary>Sends the <see cref="P:System.Data.Odbc.OdbcCommand.CommandText" /> to the <see cref="P:System.Data.Odbc.OdbcCommand.Connection" /> and builds an <see cref="T:System.Data.Odbc.OdbcDataReader" />.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcDataReader" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public new OdbcDataReader ExecuteReader()
	{
		return ExecuteReader(CommandBehavior.Default);
	}

	/// <summary>Sends the <see cref="P:System.Data.Odbc.OdbcCommand.CommandText" /> to the <see cref="P:System.Data.Odbc.OdbcCommand.Connection" />, and builds an <see cref="T:System.Data.Odbc.OdbcDataReader" /> using one of the CommandBehavior values.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcDataReader" /> object.</returns>
	/// <param name="behavior">One of the System.Data.CommandBehavior values. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public new OdbcDataReader ExecuteReader(CommandBehavior behavior)
	{
		return ExecuteReaderObject(behavior, "ExecuteReader", needReader: true);
	}

	internal OdbcDataReader ExecuteReaderFromSQLMethod(object[] methodArguments, ODBC32.SQL_API method)
	{
		return ExecuteReaderObject(CommandBehavior.Default, method.ToString(), needReader: true, methodArguments, method);
	}

	private OdbcDataReader ExecuteReaderObject(CommandBehavior behavior, string method, bool needReader)
	{
		if (CommandText == null || CommandText.Length == 0)
		{
			throw ADP.CommandTextRequired(method);
		}
		return ExecuteReaderObject(behavior, method, needReader, null, ODBC32.SQL_API.SQLEXECDIRECT);
	}

	private OdbcDataReader ExecuteReaderObject(CommandBehavior behavior, string method, bool needReader, object[] methodArguments, ODBC32.SQL_API odbcApiMethod)
	{
		OdbcDataReader odbcDataReader = null;
		try
		{
			DisposeDeadDataReader();
			ValidateConnectionAndTransaction(method);
			if ((CommandBehavior.SingleRow & behavior) != CommandBehavior.Default)
			{
				behavior |= CommandBehavior.SingleResult;
			}
			OdbcStatementHandle statementHandle = GetStatementHandle().StatementHandle;
			_cmdWrapper.Canceling = false;
			if (_weakDataReaderReference != null && _weakDataReaderReference.IsAlive)
			{
				object target = _weakDataReaderReference.Target;
				if (target != null && _weakDataReaderReference.IsAlive && !((OdbcDataReader)target).IsClosed)
				{
					throw ADP.OpenReaderExists();
				}
			}
			odbcDataReader = new OdbcDataReader(this, _cmdWrapper, behavior);
			if (!Connection.ProviderInfo.NoQueryTimeout)
			{
				TrySetStatementAttribute(statementHandle, ODBC32.SQL_ATTR.QUERY_TIMEOUT, (IntPtr)CommandTimeout);
			}
			if (needReader && Connection.IsV3Driver && !Connection.ProviderInfo.NoSqlSoptSSNoBrowseTable && !Connection.ProviderInfo.NoSqlSoptSSHiddenColumns)
			{
				if (odbcDataReader.IsBehavior(CommandBehavior.KeyInfo))
				{
					if (!_cmdWrapper._ssKeyInfoModeOn)
					{
						TrySetStatementAttribute(statementHandle, (ODBC32.SQL_ATTR)1228, (IntPtr)1);
						TrySetStatementAttribute(statementHandle, ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION, (IntPtr)1);
						_cmdWrapper._ssKeyInfoModeOff = false;
						_cmdWrapper._ssKeyInfoModeOn = true;
					}
				}
				else if (!_cmdWrapper._ssKeyInfoModeOff)
				{
					TrySetStatementAttribute(statementHandle, (ODBC32.SQL_ATTR)1228, (IntPtr)0);
					TrySetStatementAttribute(statementHandle, ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION, (IntPtr)0);
					_cmdWrapper._ssKeyInfoModeOff = true;
					_cmdWrapper._ssKeyInfoModeOn = false;
				}
			}
			if (odbcDataReader.IsBehavior(CommandBehavior.KeyInfo) || odbcDataReader.IsBehavior(CommandBehavior.SchemaOnly))
			{
				ODBC32.RetCode retCode = statementHandle.Prepare(CommandText);
				if (retCode != ODBC32.RetCode.SUCCESS)
				{
					_connection.HandleError(statementHandle, retCode);
				}
			}
			bool success = false;
			CNativeBuffer cNativeBuffer = _cmdWrapper._nativeParameterBuffer;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				if (_parameterCollection != null && 0 < _parameterCollection.Count)
				{
					int num = _parameterCollection.CalcParameterBufferSize(this);
					if (cNativeBuffer == null || cNativeBuffer.Length < num)
					{
						cNativeBuffer?.Dispose();
						cNativeBuffer = new CNativeBuffer(num);
						_cmdWrapper._nativeParameterBuffer = cNativeBuffer;
					}
					else
					{
						cNativeBuffer.ZeroMemory();
					}
					cNativeBuffer.DangerousAddRef(ref success);
					_parameterCollection.Bind(this, _cmdWrapper, cNativeBuffer);
				}
				if (!odbcDataReader.IsBehavior(CommandBehavior.SchemaOnly))
				{
					ODBC32.RetCode retCode;
					if ((odbcDataReader.IsBehavior(CommandBehavior.KeyInfo) || odbcDataReader.IsBehavior(CommandBehavior.SchemaOnly)) && CommandType != CommandType.StoredProcedure)
					{
						retCode = statementHandle.NumberOfResultColumns(out var columnsAffected);
						switch (retCode)
						{
						case ODBC32.RetCode.SUCCESS:
						case ODBC32.RetCode.SUCCESS_WITH_INFO:
							if (columnsAffected > 0)
							{
								odbcDataReader.GetSchemaTable();
							}
							break;
						default:
							_connection.HandleError(statementHandle, retCode);
							break;
						case ODBC32.RetCode.NO_DATA:
							break;
						}
					}
					retCode = odbcApiMethod switch
					{
						ODBC32.SQL_API.SQLEXECDIRECT => (!odbcDataReader.IsBehavior(CommandBehavior.KeyInfo) && !_isPrepared) ? statementHandle.ExecuteDirect(CommandText) : statementHandle.Execute(), 
						ODBC32.SQL_API.SQLTABLES => statementHandle.Tables((string)methodArguments[0], (string)methodArguments[1], (string)methodArguments[2], (string)methodArguments[3]), 
						ODBC32.SQL_API.SQLCOLUMNS => statementHandle.Columns((string)methodArguments[0], (string)methodArguments[1], (string)methodArguments[2], (string)methodArguments[3]), 
						ODBC32.SQL_API.SQLPROCEDURES => statementHandle.Procedures((string)methodArguments[0], (string)methodArguments[1], (string)methodArguments[2]), 
						ODBC32.SQL_API.SQLPROCEDURECOLUMNS => statementHandle.ProcedureColumns((string)methodArguments[0], (string)methodArguments[1], (string)methodArguments[2], (string)methodArguments[3]), 
						ODBC32.SQL_API.SQLSTATISTICS => statementHandle.Statistics((string)methodArguments[0], (string)methodArguments[1], (string)methodArguments[2], (short)methodArguments[3], (short)methodArguments[4]), 
						ODBC32.SQL_API.SQLGETTYPEINFO => statementHandle.GetTypeInfo((short)methodArguments[0]), 
						_ => throw ADP.InvalidOperation(method.ToString()), 
					};
					if (retCode != ODBC32.RetCode.SUCCESS && ODBC32.RetCode.NO_DATA != retCode)
					{
						_connection.HandleError(statementHandle, retCode);
					}
				}
			}
			finally
			{
				if (success)
				{
					cNativeBuffer.DangerousRelease();
				}
			}
			_weakDataReaderReference = new WeakReference(odbcDataReader);
			if (!odbcDataReader.IsBehavior(CommandBehavior.SchemaOnly))
			{
				odbcDataReader.FirstResult();
			}
			_cmdState = ConnectionState.Fetching;
		}
		finally
		{
			if (ConnectionState.Fetching != _cmdState)
			{
				if (odbcDataReader != null)
				{
					if (_parameterCollection != null)
					{
						_parameterCollection.ClearBindings();
					}
					((IDisposable)odbcDataReader).Dispose();
				}
				if (_cmdState != ConnectionState.Closed)
				{
					_cmdState = ConnectionState.Closed;
				}
			}
		}
		return odbcDataReader;
	}

	/// <summary>Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.</summary>
	/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override object ExecuteScalar()
	{
		object result = null;
		using IDataReader dataReader = ExecuteReaderObject(CommandBehavior.Default, "ExecuteScalar", needReader: false);
		if (dataReader.Read() && 0 < dataReader.FieldCount)
		{
			result = dataReader.GetValue(0);
		}
		dataReader.Close();
		return result;
	}

	internal string GetDiagSqlState()
	{
		return _cmdWrapper.GetDiagSqlState();
	}

	private void PropertyChanging()
	{
		_isPrepared = false;
	}

	/// <summary>Creates a prepared or compiled version of the command at the data source.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Data.Odbc.OdbcCommand.Connection" /> is not set.-or- The <see cref="P:System.Data.Odbc.OdbcCommand.Connection" /> is not <see cref="!:System.Data.Odbc.OdbcConnection.Open" />. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Prepare()
	{
		ValidateOpenConnection("Prepare");
		if ((ConnectionState.Fetching & _connection.InternalState) != ConnectionState.Closed)
		{
			throw ADP.OpenReaderExists();
		}
		if (CommandType != CommandType.TableDirect)
		{
			DisposeDeadDataReader();
			GetStatementHandle();
			OdbcStatementHandle statementHandle = _cmdWrapper.StatementHandle;
			ODBC32.RetCode retCode = statementHandle.Prepare(CommandText);
			if (retCode != ODBC32.RetCode.SUCCESS)
			{
				_connection.HandleError(statementHandle, retCode);
			}
			_isPrepared = true;
		}
	}

	private void TrySetStatementAttribute(OdbcStatementHandle stmt, ODBC32.SQL_ATTR stmtAttribute, IntPtr value)
	{
		if (stmt.SetStatementAttribute(stmtAttribute, value, ODBC32.SQL_IS.UINTEGER) == ODBC32.RetCode.ERROR)
		{
			stmt.GetDiagnosticField(out var sqlState);
			if (sqlState == "HYC00" || sqlState == "HY092")
			{
				Connection.FlagUnsupportedStmtAttr(stmtAttribute);
			}
		}
	}

	private void ValidateOpenConnection(string methodName)
	{
		OdbcConnection connection = Connection;
		if (connection == null)
		{
			throw ADP.ConnectionRequired(methodName);
		}
		ConnectionState state = connection.State;
		if (ConnectionState.Open != state)
		{
			throw ADP.OpenConnectionRequired(methodName, state);
		}
	}

	private void ValidateConnectionAndTransaction(string method)
	{
		if (_connection == null)
		{
			throw ADP.ConnectionRequired(method);
		}
		_transaction = _connection.SetStateExecuting(method, Transaction);
		_cmdState = ConnectionState.Executing;
	}
}
