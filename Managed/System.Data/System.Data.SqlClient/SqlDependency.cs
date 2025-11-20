using System.Collections.Generic;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Data.Sql;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Data.SqlClient;

/// <summary>The <see cref="T:System.Data.SqlClient.SqlDependency" /> object represents a query notification dependency between an application and an instance of SQL Server. An application can create a <see cref="T:System.Data.SqlClient.SqlDependency" /> object and register to receive notifications via the <see cref="T:System.Data.SqlClient.OnChangeEventHandler" /> event handler.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlDependency
{
	internal class IdentityUserNamePair
	{
		private DbConnectionPoolIdentity _identity;

		private string _userName;

		internal DbConnectionPoolIdentity Identity => _identity;

		internal string UserName => _userName;

		internal IdentityUserNamePair(DbConnectionPoolIdentity identity, string userName)
		{
			_identity = identity;
			_userName = userName;
		}

		public override bool Equals(object value)
		{
			IdentityUserNamePair identityUserNamePair = (IdentityUserNamePair)value;
			bool result = false;
			if (identityUserNamePair == null)
			{
				result = false;
			}
			else if (this == identityUserNamePair)
			{
				result = true;
			}
			else if (_identity != null)
			{
				if (_identity.Equals(identityUserNamePair._identity))
				{
					result = true;
				}
			}
			else if (_userName == identityUserNamePair._userName)
			{
				result = true;
			}
			return result;
		}

		public override int GetHashCode()
		{
			int num = 0;
			if (_identity != null)
			{
				return _identity.GetHashCode();
			}
			return _userName.GetHashCode();
		}
	}

	private class DatabaseServicePair
	{
		private string _database;

		private string _service;

		internal string Database => _database;

		internal string Service => _service;

		internal DatabaseServicePair(string database, string service)
		{
			_database = database;
			_service = service;
		}

		public override bool Equals(object value)
		{
			DatabaseServicePair databaseServicePair = (DatabaseServicePair)value;
			bool result = false;
			if (databaseServicePair == null)
			{
				result = false;
			}
			else if (this == databaseServicePair)
			{
				result = true;
			}
			else if (_database == databaseServicePair._database)
			{
				result = true;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return _database.GetHashCode();
		}
	}

	internal class EventContextPair
	{
		private OnChangeEventHandler _eventHandler;

		private ExecutionContext _context;

		private SqlDependency _dependency;

		private SqlNotificationEventArgs _args;

		private static ContextCallback s_contextCallback = InvokeCallback;

		internal EventContextPair(OnChangeEventHandler eventHandler, SqlDependency dependency)
		{
			_eventHandler = eventHandler;
			_context = ExecutionContext.Capture();
			_dependency = dependency;
		}

		public override bool Equals(object value)
		{
			EventContextPair eventContextPair = (EventContextPair)value;
			bool result = false;
			if (eventContextPair == null)
			{
				result = false;
			}
			else if (this == eventContextPair)
			{
				result = true;
			}
			else if (_eventHandler == eventContextPair._eventHandler)
			{
				result = true;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return _eventHandler.GetHashCode();
		}

		internal void Invoke(SqlNotificationEventArgs args)
		{
			_args = args;
			ExecutionContext.Run(_context, s_contextCallback, this);
		}

		private static void InvokeCallback(object eventContextPair)
		{
			EventContextPair eventContextPair2 = (EventContextPair)eventContextPair;
			eventContextPair2._eventHandler(eventContextPair2._dependency, eventContextPair2._args);
		}
	}

	private readonly string _id = Guid.NewGuid().ToString() + ";" + s_appDomainKey;

	private string _options;

	private int _timeout;

	private bool _dependencyFired;

	private List<EventContextPair> _eventList = new List<EventContextPair>();

	private object _eventHandlerLock = new object();

	private DateTime _expirationTime = DateTime.MaxValue;

	private List<string> _serverList = new List<string>();

	private static object s_startStopLock = new object();

	private static readonly string s_appDomainKey = Guid.NewGuid().ToString();

	private static Dictionary<string, Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>> s_serverUserHash = new Dictionary<string, Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>>(StringComparer.OrdinalIgnoreCase);

	private static SqlDependencyProcessDispatcher s_processDispatcher = null;

	private static readonly string s_assemblyName = typeof(SqlDependencyProcessDispatcher).Assembly.FullName;

	private static readonly string s_typeName = typeof(SqlDependencyProcessDispatcher).FullName;

	/// <summary>Gets a value that indicates whether one of the result sets associated with the dependency has changed.</summary>
	/// <returns>A Boolean value indicating whether one of the result sets has changed.</returns>
	/// <filterpriority>2</filterpriority>
	public bool HasChanges => _dependencyFired;

	/// <summary>Gets a value that uniquely identifies this instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class.</summary>
	/// <returns>A string representation of a GUID that is generated for each instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class.</returns>
	/// <filterpriority>2</filterpriority>
	public string Id => _id;

	internal static string AppDomainKey => s_appDomainKey;

	internal DateTime ExpirationTime => _expirationTime;

	internal string Options => _options;

	internal static SqlDependencyProcessDispatcher ProcessDispatcher => s_processDispatcher;

	internal int Timeout => _timeout;

	/// <summary>Occurs when a notification is received for any of the commands associated with this <see cref="T:System.Data.SqlClient.SqlDependency" /> object.</summary>
	public event OnChangeEventHandler OnChange
	{
		add
		{
			if (value == null)
			{
				return;
			}
			SqlNotificationEventArgs e = null;
			lock (_eventHandlerLock)
			{
				if (_dependencyFired)
				{
					e = new SqlNotificationEventArgs(SqlNotificationType.Subscribe, SqlNotificationInfo.AlreadyChanged, SqlNotificationSource.Client);
				}
				else
				{
					EventContextPair item = new EventContextPair(value, this);
					if (_eventList.Contains(item))
					{
						throw SQL.SqlDependencyEventNoDuplicate();
					}
					_eventList.Add(item);
				}
			}
			if (e != null)
			{
				value(this, e);
			}
		}
		remove
		{
			if (value == null)
			{
				return;
			}
			EventContextPair item = new EventContextPair(value, this);
			lock (_eventHandlerLock)
			{
				int num = _eventList.IndexOf(item);
				if (0 <= num)
				{
					_eventList.RemoveAt(num);
				}
			}
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class with the default settings.</summary>
	public SqlDependency()
		: this(null, null, 0)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class and associates it with the <see cref="T:System.Data.SqlClient.SqlCommand" /> parameter.</summary>
	/// <param name="command">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object to associate with this <see cref="T:System.Data.SqlClient.SqlDependency" /> object. The constructor will set up a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object and bind it to the command. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="command" /> parameter is NULL. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object already has a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object assigned to its <see cref="P:System.Data.SqlClient.SqlCommand.Notification" /> property, and that <see cref="T:System.Data.Sql.SqlNotificationRequest" /> is not associated with this dependency. </exception>
	public SqlDependency(SqlCommand command)
		: this(command, null, 0)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class, associates it with the <see cref="T:System.Data.SqlClient.SqlCommand" /> parameter, and specifies notification options and a time-out value.</summary>
	/// <param name="command">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object to associate with this <see cref="T:System.Data.SqlClient.SqlDependency" /> object. The constructor sets up a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object and bind it to the command.</param>
	/// <param name="options">The notification request options to be used by this dependency.  <paramref name="null" /> to use the default service. </param>
	/// <param name="timeout">The time-out for this notification in seconds. The default is 0, indicating that the server's time-out should be used.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="command" /> parameter is NULL. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The time-out value is less than zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object already has a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object assigned to its <see cref="P:System.Data.SqlClient.SqlCommand.Notification" /> property and that <see cref="T:System.Data.Sql.SqlNotificationRequest" /> is not associated with this dependency.An attempt was made to create a SqlDependency instance from within SQLCLR.</exception>
	public SqlDependency(SqlCommand command, string options, int timeout)
	{
		if (timeout < 0)
		{
			throw SQL.InvalidSqlDependencyTimeout("timeout");
		}
		_timeout = timeout;
		if (options != null)
		{
			_options = options;
		}
		AddCommandInternal(command);
		SqlDependencyPerAppDomainDispatcher.SingletonInstance.AddDependencyEntry(this);
	}

	/// <summary>Associates a <see cref="T:System.Data.SqlClient.SqlCommand" /> object with this <see cref="T:System.Data.SqlClient.SqlDependency" /> instance.</summary>
	/// <param name="command">A <see cref="T:System.Data.SqlClient.SqlCommand" /> object containing a statement that is valid for notifications. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="command" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object already has a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object assigned to its <see cref="P:System.Data.SqlClient.SqlCommand.Notification" /> property, and that <see cref="T:System.Data.Sql.SqlNotificationRequest" /> is not associated with this dependency. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public void AddCommandDependency(SqlCommand command)
	{
		if (command == null)
		{
			throw ADP.ArgumentNull("command");
		}
		AddCommandInternal(command);
	}

	/// <summary>Starts the listener for receiving dependency change notifications from the instance of SQL Server specified by the connection string.</summary>
	/// <returns>true if the listener initialized successfully; false if a compatible listener already exists.</returns>
	/// <param name="connectionString">The connection string for the instance of SQL Server from which to obtain change notifications.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="connectionString" /> parameter is the same as a previous call to this method, but the parameters are different.The method was called from within the CLR.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">A subsequent call to the method has been made with an equivalent <paramref name="connectionString" /> parameter with a different user, or a user that does not default to the same schema.Also, any underlying SqlClient exceptions.</exception>
	public static bool Start(string connectionString)
	{
		return Start(connectionString, null, useDefaults: true);
	}

	/// <summary>Starts the listener for receiving dependency change notifications from the instance of SQL Server specified by the connection string using the specified SQL Server Service Broker queue.</summary>
	/// <returns>true if the listener initialized successfully; false if a compatible listener already exists.</returns>
	/// <param name="connectionString">The connection string for the instance of SQL Server from which to obtain change notifications.</param>
	/// <param name="queue">An existing SQL Server Service Broker queue to be used. If null, the default queue is used.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="connectionString" /> parameter is the same as a previous call to this method, but the parameters are different.The method was called from within the CLR.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">A subsequent call to the method has been made with an equivalent <paramref name="connectionString" /> parameter but a different user, or a user that does not default to the same schema.Also, any underlying SqlClient exceptions.</exception>
	public static bool Start(string connectionString, string queue)
	{
		return Start(connectionString, queue, useDefaults: false);
	}

	internal static bool Start(string connectionString, string queue, bool useDefaults)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			if (connectionString == null)
			{
				throw ADP.ArgumentNull("connectionString");
			}
			throw ADP.Argument("connectionString");
		}
		if (!useDefaults && string.IsNullOrEmpty(queue))
		{
			useDefaults = true;
			queue = null;
		}
		bool errorOccurred = false;
		bool result = false;
		lock (s_startStopLock)
		{
			try
			{
				if (s_processDispatcher == null)
				{
					s_processDispatcher = SqlDependencyProcessDispatcher.SingletonProcessDispatcher;
				}
				if (useDefaults)
				{
					string server = null;
					DbConnectionPoolIdentity identity = null;
					string user = null;
					string database = null;
					string service = null;
					bool appDomainStart = false;
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
						result = s_processDispatcher.StartWithDefault(connectionString, out server, out identity, out user, out database, ref service, s_appDomainKey, SqlDependencyPerAppDomainDispatcher.SingletonInstance, out errorOccurred, out appDomainStart);
					}
					finally
					{
						if (appDomainStart && !errorOccurred)
						{
							IdentityUserNamePair identityUser = new IdentityUserNamePair(identity, user);
							DatabaseServicePair databaseService = new DatabaseServicePair(database, service);
							if (!AddToServerUserHash(server, identityUser, databaseService))
							{
								try
								{
									Stop(connectionString, queue, useDefaults, startFailed: true);
								}
								catch (Exception e)
								{
									if (!ADP.IsCatchableExceptionType(e))
									{
										throw;
									}
									ADP.TraceExceptionWithoutRethrow(e);
								}
								throw SQL.SqlDependencyDuplicateStart();
							}
						}
					}
				}
				else
				{
					result = s_processDispatcher.Start(connectionString, queue, s_appDomainKey, SqlDependencyPerAppDomainDispatcher.SingletonInstance);
				}
			}
			catch (Exception e2)
			{
				if (!ADP.IsCatchableExceptionType(e2))
				{
					throw;
				}
				ADP.TraceExceptionWithoutRethrow(e2);
				throw;
			}
		}
		return result;
	}

	/// <summary>Stops a listener for a connection specified in a previous <see cref="Overload:System.Data.SqlClient.SqlDependency.Start" /> call.</summary>
	/// <returns>true if the listener was completely stopped; false if the <see cref="T:System.AppDomain" /> was unbound from the listener, but there are is at least one other <see cref="T:System.AppDomain" /> using the same listener.</returns>
	/// <param name="connectionString">Connection string for the instance of SQL Server that was used in a previous <see cref="M:System.Data.SqlClient.SqlDependency.Start(System.String)" /> call.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL. </exception>
	/// <exception cref="T:System.InvalidOperationException">The method was called from within SQLCLR.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">An underlying SqlClient exception occurred.</exception>
	public static bool Stop(string connectionString)
	{
		return Stop(connectionString, null, useDefaults: true, startFailed: false);
	}

	/// <summary>Stops a listener for a connection specified in a previous <see cref="Overload:System.Data.SqlClient.SqlDependency.Start" /> call.</summary>
	/// <returns>true if the listener was completely stopped; false if the <see cref="T:System.AppDomain" /> was unbound from the listener, but there is at least one other <see cref="T:System.AppDomain" /> using the same listener.</returns>
	/// <param name="connectionString">Connection string for the instance of SQL Server that was used in a previous <see cref="M:System.Data.SqlClient.SqlDependency.Start(System.String,System.String)" /> call.</param>
	/// <param name="queue">The SQL Server Service Broker queue that was used in a previous <see cref="M:System.Data.SqlClient.SqlDependency.Start(System.String,System.String)" /> call.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL. </exception>
	/// <exception cref="T:System.InvalidOperationException">The method was called from within SQLCLR.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
	/// <exception cref="T:System.Data.SqlClient.SqlException">And underlying SqlClient exception occurred.</exception>
	public static bool Stop(string connectionString, string queue)
	{
		return Stop(connectionString, queue, useDefaults: false, startFailed: false);
	}

	internal static bool Stop(string connectionString, string queue, bool useDefaults, bool startFailed)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			if (connectionString == null)
			{
				throw ADP.ArgumentNull("connectionString");
			}
			throw ADP.Argument("connectionString");
		}
		if (!useDefaults && string.IsNullOrEmpty(queue))
		{
			useDefaults = true;
			queue = null;
		}
		bool result = false;
		lock (s_startStopLock)
		{
			if (s_processDispatcher != null)
			{
				try
				{
					string server = null;
					DbConnectionPoolIdentity identity = null;
					string user = null;
					string database = null;
					string queueService = null;
					if (useDefaults)
					{
						bool appDomainStop = false;
						RuntimeHelpers.PrepareConstrainedRegions();
						try
						{
							result = s_processDispatcher.Stop(connectionString, out server, out identity, out user, out database, ref queueService, s_appDomainKey, out appDomainStop);
						}
						finally
						{
							if (appDomainStop && !startFailed)
							{
								IdentityUserNamePair identityUser = new IdentityUserNamePair(identity, user);
								DatabaseServicePair databaseService = new DatabaseServicePair(database, queueService);
								RemoveFromServerUserHash(server, identityUser, databaseService);
							}
						}
					}
					else
					{
						result = s_processDispatcher.Stop(connectionString, out server, out identity, out user, out database, ref queue, s_appDomainKey, out var _);
					}
				}
				catch (Exception e)
				{
					if (!ADP.IsCatchableExceptionType(e))
					{
						throw;
					}
					ADP.TraceExceptionWithoutRethrow(e);
				}
			}
		}
		return result;
	}

	private static bool AddToServerUserHash(string server, IdentityUserNamePair identityUser, DatabaseServicePair databaseService)
	{
		bool result = false;
		lock (s_serverUserHash)
		{
			Dictionary<IdentityUserNamePair, List<DatabaseServicePair>> dictionary;
			if (!s_serverUserHash.ContainsKey(server))
			{
				dictionary = new Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>();
				s_serverUserHash.Add(server, dictionary);
			}
			else
			{
				dictionary = s_serverUserHash[server];
			}
			List<DatabaseServicePair> list;
			if (!dictionary.ContainsKey(identityUser))
			{
				list = new List<DatabaseServicePair>();
				dictionary.Add(identityUser, list);
			}
			else
			{
				list = dictionary[identityUser];
			}
			if (!list.Contains(databaseService))
			{
				list.Add(databaseService);
				result = true;
			}
		}
		return result;
	}

	private static void RemoveFromServerUserHash(string server, IdentityUserNamePair identityUser, DatabaseServicePair databaseService)
	{
		lock (s_serverUserHash)
		{
			if (!s_serverUserHash.ContainsKey(server))
			{
				return;
			}
			Dictionary<IdentityUserNamePair, List<DatabaseServicePair>> dictionary = s_serverUserHash[server];
			if (!dictionary.ContainsKey(identityUser))
			{
				return;
			}
			List<DatabaseServicePair> list = dictionary[identityUser];
			int num = list.IndexOf(databaseService);
			if (num < 0)
			{
				return;
			}
			list.RemoveAt(num);
			if (list.Count == 0)
			{
				dictionary.Remove(identityUser);
				if (dictionary.Count == 0)
				{
					s_serverUserHash.Remove(server);
				}
			}
		}
	}

	internal static string GetDefaultComposedOptions(string server, string failoverServer, IdentityUserNamePair identityUser, string database)
	{
		lock (s_serverUserHash)
		{
			if (!s_serverUserHash.ContainsKey(server))
			{
				if (s_serverUserHash.Count == 0)
				{
					throw SQL.SqlDepDefaultOptionsButNoStart();
				}
				if (string.IsNullOrEmpty(failoverServer) || !s_serverUserHash.ContainsKey(failoverServer))
				{
					throw SQL.SqlDependencyNoMatchingServerStart();
				}
				server = failoverServer;
			}
			Dictionary<IdentityUserNamePair, List<DatabaseServicePair>> dictionary = s_serverUserHash[server];
			List<DatabaseServicePair> list = null;
			if (!dictionary.ContainsKey(identityUser))
			{
				if (dictionary.Count > 1)
				{
					throw SQL.SqlDependencyNoMatchingServerStart();
				}
				using Dictionary<IdentityUserNamePair, List<DatabaseServicePair>>.Enumerator enumerator = dictionary.GetEnumerator();
				if (enumerator.MoveNext())
				{
					list = enumerator.Current.Value;
				}
			}
			else
			{
				list = dictionary[identityUser];
			}
			DatabaseServicePair item = new DatabaseServicePair(database, null);
			DatabaseServicePair databaseServicePair = null;
			int num = list.IndexOf(item);
			if (num != -1)
			{
				databaseServicePair = list[num];
			}
			if (databaseServicePair != null)
			{
				database = FixupServiceOrDatabaseName(databaseServicePair.Database);
				string text = FixupServiceOrDatabaseName(databaseServicePair.Service);
				return "Service=" + text + ";Local Database=" + database;
			}
			if (list.Count == 1)
			{
				object[] array = list.ToArray();
				databaseServicePair = (DatabaseServicePair)array[0];
				string text2 = FixupServiceOrDatabaseName(databaseServicePair.Database);
				string text3 = FixupServiceOrDatabaseName(databaseServicePair.Service);
				return "Service=" + text3 + ";Local Database=" + text2;
			}
			throw SQL.SqlDependencyNoMatchingServerDatabaseStart();
		}
	}

	internal void AddToServerList(string server)
	{
		lock (_serverList)
		{
			int num = _serverList.BinarySearch(server, StringComparer.OrdinalIgnoreCase);
			if (0 > num)
			{
				num = ~num;
				_serverList.Insert(num, server);
			}
		}
	}

	internal bool ContainsServer(string server)
	{
		lock (_serverList)
		{
			return _serverList.Contains(server);
		}
	}

	internal string ComputeHashAndAddToDispatcher(SqlCommand command)
	{
		string commandHash = ComputeCommandHash(command.Connection.ConnectionString, command);
		return SqlDependencyPerAppDomainDispatcher.SingletonInstance.AddCommandEntry(commandHash, this);
	}

	internal void Invalidate(SqlNotificationType type, SqlNotificationInfo info, SqlNotificationSource source)
	{
		List<EventContextPair> list = null;
		lock (_eventHandlerLock)
		{
			if (_dependencyFired && SqlNotificationInfo.AlreadyChanged != info && SqlNotificationSource.Client != source)
			{
				if (!(ExpirationTime >= DateTime.UtcNow))
				{
				}
			}
			else
			{
				_dependencyFired = true;
				list = _eventList;
				_eventList = new List<EventContextPair>();
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EventContextPair item in list)
		{
			item.Invoke(new SqlNotificationEventArgs(type, info, source));
		}
	}

	internal void StartTimer(SqlNotificationRequest notificationRequest)
	{
		if (_expirationTime == DateTime.MaxValue)
		{
			int num = 432000;
			if (_timeout != 0)
			{
				num = _timeout;
			}
			if (notificationRequest != null && notificationRequest.Timeout < num && notificationRequest.Timeout != 0)
			{
				num = notificationRequest.Timeout;
			}
			_expirationTime = DateTime.UtcNow.AddSeconds(num);
			SqlDependencyPerAppDomainDispatcher.SingletonInstance.StartTimer(this);
		}
	}

	private void AddCommandInternal(SqlCommand cmd)
	{
		if (cmd == null)
		{
			return;
		}
		_ = cmd.Connection;
		if (cmd.Notification != null)
		{
			if (cmd._sqlDep == null || cmd._sqlDep != this)
			{
				throw SQL.SqlCommandHasExistingSqlNotificationRequest();
			}
			return;
		}
		bool flag = false;
		lock (_eventHandlerLock)
		{
			if (!_dependencyFired)
			{
				cmd.Notification = new SqlNotificationRequest
				{
					Timeout = _timeout
				};
				if (_options != null)
				{
					cmd.Notification.Options = _options;
				}
				cmd._sqlDep = this;
			}
			else if (_eventList.Count == 0)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Invalidate(SqlNotificationType.Subscribe, SqlNotificationInfo.AlreadyChanged, SqlNotificationSource.Client);
		}
	}

	private string ComputeCommandHash(string connectionString, SqlCommand command)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0};{1}", connectionString, command.CommandText);
		for (int i = 0; i < command.Parameters.Count; i++)
		{
			object value = command.Parameters[i].Value;
			if (value == null || value == DBNull.Value)
			{
				stringBuilder.Append("; NULL");
				continue;
			}
			Type type = value.GetType();
			if (type == typeof(byte[]))
			{
				stringBuilder.Append(";");
				byte[] array = (byte[])value;
				for (int j = 0; j < array.Length; j++)
				{
					stringBuilder.Append(array[j].ToString("x2", CultureInfo.InvariantCulture));
				}
			}
			else if (type == typeof(char[]))
			{
				stringBuilder.Append((char[])value);
			}
			else if (type == typeof(XmlReader))
			{
				stringBuilder.Append(";");
				stringBuilder.Append(Guid.NewGuid().ToString());
			}
			else
			{
				stringBuilder.Append(";");
				stringBuilder.Append(value.ToString());
			}
		}
		return stringBuilder.ToString();
	}

	internal static string FixupServiceOrDatabaseName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return "\"" + name.Replace("\"", "\"\"") + "\"";
		}
		return name;
	}
}
