using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using Unity;

namespace System.Data.SqlClient;

/// <summary>Provides a simple way to create and manage the contents of connection strings used by the <see cref="T:System.Data.SqlClient.SqlConnection" /> class. </summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlConnectionStringBuilder : DbConnectionStringBuilder
{
	private enum Keywords
	{
		DataSource,
		FailoverPartner,
		AttachDBFilename,
		InitialCatalog,
		IntegratedSecurity,
		PersistSecurityInfo,
		UserID,
		Password,
		Enlist,
		Pooling,
		MinPoolSize,
		MaxPoolSize,
		MultipleActiveResultSets,
		Replication,
		ConnectTimeout,
		Encrypt,
		TrustServerCertificate,
		LoadBalanceTimeout,
		PacketSize,
		TypeSystemVersion,
		ApplicationName,
		CurrentLanguage,
		WorkstationID,
		UserInstance,
		TransactionBinding,
		ApplicationIntent,
		MultiSubnetFailover,
		ConnectRetryCount,
		ConnectRetryInterval,
		KeywordsCount
	}

	private sealed class SqlInitialCatalogConverter : StringConverter
	{
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return GetStandardValuesSupportedInternal(context);
		}

		private bool GetStandardValuesSupportedInternal(ITypeDescriptorContext context)
		{
			bool result = false;
			if (context != null && context.Instance is SqlConnectionStringBuilder sqlConnectionStringBuilder && 0 < sqlConnectionStringBuilder.DataSource.Length && (sqlConnectionStringBuilder.IntegratedSecurity || 0 < sqlConnectionStringBuilder.UserID.Length))
			{
				result = true;
			}
			return result;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (GetStandardValuesSupportedInternal(context))
			{
				List<string> list = new List<string>();
				try
				{
					SqlConnectionStringBuilder sqlConnectionStringBuilder = (SqlConnectionStringBuilder)context.Instance;
					using SqlConnection sqlConnection = new SqlConnection();
					sqlConnection.ConnectionString = sqlConnectionStringBuilder.ConnectionString;
					sqlConnection.Open();
					foreach (DataRow row in sqlConnection.GetSchema("DATABASES").Rows)
					{
						string item = (string)row["database_name"];
						list.Add(item);
					}
				}
				catch (SqlException e)
				{
					ADP.TraceExceptionWithoutRethrow(e);
				}
				return new StandardValuesCollection(list);
			}
			return null;
		}
	}

	internal const int KeywordsCount = 29;

	internal const int DeprecatedKeywordsCount = 4;

	private static readonly string[] s_validKeywords = CreateValidKeywords();

	private static readonly Dictionary<string, Keywords> s_keywords = CreateKeywordsDictionary();

	private ApplicationIntent _applicationIntent;

	private string _applicationName = "Core .Net SqlClient Data Provider";

	private string _attachDBFilename = "";

	private string _currentLanguage = "";

	private string _dataSource = "";

	private string _failoverPartner = "";

	private string _initialCatalog = "";

	private string _password = "";

	private string _transactionBinding = "Implicit Unbind";

	private string _typeSystemVersion = "Latest";

	private string _userID = "";

	private string _workstationID = "";

	private int _connectTimeout = 15;

	private int _loadBalanceTimeout;

	private int _maxPoolSize = 100;

	private int _minPoolSize;

	private int _packetSize = 8000;

	private int _connectRetryCount = 1;

	private int _connectRetryInterval = 10;

	private bool _encrypt;

	private bool _trustServerCertificate;

	private bool _enlist = true;

	private bool _integratedSecurity;

	private bool _multipleActiveResultSets;

	private bool _multiSubnetFailover;

	private bool _persistSecurityInfo;

	private bool _pooling = true;

	private bool _replication;

	private bool _userInstance;

	private static readonly string[] s_notSupportedKeywords = new string[5] { "Asynchronous Processing", "Connection Reset", "Context Connection", "Transaction Binding", "async" };

	private static readonly string[] s_notSupportedNetworkLibraryKeywords = new string[3] { "Network Library", "net", "network" };

	/// <summary>Gets or sets the value associated with the specified key. In C#, this property is the indexer.</summary>
	/// <returns>The value associated with the specified key. </returns>
	/// <param name="keyword">The key of the item to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Tried to add a key that does not exist within the available keys.</exception>
	/// <exception cref="T:System.FormatException">Invalid value within the connection string (specifically, a Boolean or numeric value was expected but not supplied).</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object this[string keyword]
	{
		get
		{
			Keywords index = GetIndex(keyword);
			return GetAt(index);
		}
		set
		{
			if (value != null)
			{
				switch (GetIndex(keyword))
				{
				case Keywords.ApplicationIntent:
					ApplicationIntent = ConvertToApplicationIntent(keyword, value);
					break;
				case Keywords.ApplicationName:
					ApplicationName = ConvertToString(value);
					break;
				case Keywords.AttachDBFilename:
					AttachDBFilename = ConvertToString(value);
					break;
				case Keywords.CurrentLanguage:
					CurrentLanguage = ConvertToString(value);
					break;
				case Keywords.DataSource:
					DataSource = ConvertToString(value);
					break;
				case Keywords.FailoverPartner:
					FailoverPartner = ConvertToString(value);
					break;
				case Keywords.InitialCatalog:
					InitialCatalog = ConvertToString(value);
					break;
				case Keywords.Password:
					Password = ConvertToString(value);
					break;
				case Keywords.UserID:
					UserID = ConvertToString(value);
					break;
				case Keywords.TransactionBinding:
					TransactionBinding = ConvertToString(value);
					break;
				case Keywords.TypeSystemVersion:
					TypeSystemVersion = ConvertToString(value);
					break;
				case Keywords.WorkstationID:
					WorkstationID = ConvertToString(value);
					break;
				case Keywords.ConnectTimeout:
					ConnectTimeout = ConvertToInt32(value);
					break;
				case Keywords.LoadBalanceTimeout:
					LoadBalanceTimeout = ConvertToInt32(value);
					break;
				case Keywords.MaxPoolSize:
					MaxPoolSize = ConvertToInt32(value);
					break;
				case Keywords.MinPoolSize:
					MinPoolSize = ConvertToInt32(value);
					break;
				case Keywords.PacketSize:
					PacketSize = ConvertToInt32(value);
					break;
				case Keywords.IntegratedSecurity:
					IntegratedSecurity = ConvertToIntegratedSecurity(value);
					break;
				case Keywords.Encrypt:
					Encrypt = ConvertToBoolean(value);
					break;
				case Keywords.TrustServerCertificate:
					TrustServerCertificate = ConvertToBoolean(value);
					break;
				case Keywords.Enlist:
					Enlist = ConvertToBoolean(value);
					break;
				case Keywords.MultipleActiveResultSets:
					MultipleActiveResultSets = ConvertToBoolean(value);
					break;
				case Keywords.MultiSubnetFailover:
					MultiSubnetFailover = ConvertToBoolean(value);
					break;
				case Keywords.PersistSecurityInfo:
					PersistSecurityInfo = ConvertToBoolean(value);
					break;
				case Keywords.Pooling:
					Pooling = ConvertToBoolean(value);
					break;
				case Keywords.Replication:
					Replication = ConvertToBoolean(value);
					break;
				case Keywords.UserInstance:
					UserInstance = ConvertToBoolean(value);
					break;
				case Keywords.ConnectRetryCount:
					ConnectRetryCount = ConvertToInt32(value);
					break;
				case Keywords.ConnectRetryInterval:
					ConnectRetryInterval = ConvertToInt32(value);
					break;
				default:
					throw UnsupportedKeyword(keyword);
				}
			}
			else
			{
				Remove(keyword);
			}
		}
	}

	/// <summary>Declares the application workload type when connecting to a database in an SQL Server Availability Group. You can set the value of this property with <see cref="T:System.Data.SqlClient.ApplicationIntent" />. For more information about SqlClient support for Always On Availability Groups, see SqlClient Support for High Availability, Disaster Recovery.</summary>
	/// <returns>Returns the current value of the property (a value of type <see cref="T:System.Data.SqlClient.ApplicationIntent" />).</returns>
	public ApplicationIntent ApplicationIntent
	{
		get
		{
			return _applicationIntent;
		}
		set
		{
			if (!DbConnectionStringBuilderUtil.IsValidApplicationIntentValue(value))
			{
				throw ADP.InvalidEnumerationValue(typeof(ApplicationIntent), (int)value);
			}
			SetApplicationIntentValue(value);
			_applicationIntent = value;
		}
	}

	/// <summary>Gets or sets the name of the application associated with the connection string.</summary>
	/// <returns>The name of the application, or ".NET SqlClient Data Provider" if no name has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string ApplicationName
	{
		get
		{
			return _applicationName;
		}
		set
		{
			SetValue("Application Name", value);
			_applicationName = value;
		}
	}

	/// <summary>Gets or sets a string that contains the name of the primary data file. This includes the full path name of an attachable database.</summary>
	/// <returns>The value of the AttachDBFilename property, or String.Empty if no value has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string AttachDBFilename
	{
		get
		{
			return _attachDBFilename;
		}
		set
		{
			SetValue("AttachDbFilename", value);
			_attachDBFilename = value;
		}
	}

	/// <summary>Gets or sets the length of time (in seconds) to wait for a connection to the server before terminating the attempt and generating an error.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.ConnectTimeout" /> property, or 15 seconds if no value has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public int ConnectTimeout
	{
		get
		{
			return _connectTimeout;
		}
		set
		{
			if (value < 0)
			{
				throw ADP.InvalidConnectionOptionValue("Connect Timeout");
			}
			SetValue("Connect Timeout", value);
			_connectTimeout = value;
		}
	}

	/// <summary>Gets or sets the SQL Server Language record name.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.CurrentLanguage" /> property, or String.Empty if no value has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string CurrentLanguage
	{
		get
		{
			return _currentLanguage;
		}
		set
		{
			SetValue("Current Language", value);
			_currentLanguage = value;
		}
	}

	/// <summary>Gets or sets the name or network address of the instance of SQL Server to connect to.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.DataSource" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string DataSource
	{
		get
		{
			return _dataSource;
		}
		set
		{
			SetValue("Data Source", value);
			_dataSource = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether SQL Server uses SSL encryption for all data sent between the client and server if the server has a certificate installed.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.Encrypt" /> property, or false if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool Encrypt
	{
		get
		{
			return _encrypt;
		}
		set
		{
			SetValue("Encrypt", value);
			_encrypt = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the channel will be encrypted while bypassing walking the certificate chain to validate trust.</summary>
	/// <returns>A Boolean. Recognized values are true, false, yes, and no. </returns>
	public bool TrustServerCertificate
	{
		get
		{
			return _trustServerCertificate;
		}
		set
		{
			SetValue("TrustServerCertificate", value);
			_trustServerCertificate = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether the SQL Server connection pooler automatically enlists the connection in the creation thread's current transaction context.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.Enlist" /> property, or true if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool Enlist
	{
		get
		{
			return _enlist;
		}
		set
		{
			SetValue("Enlist", value);
			_enlist = value;
		}
	}

	/// <summary>Gets or sets the name or address of the partner server to connect to if the primary server is down.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.FailoverPartner" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>1</filterpriority>
	public string FailoverPartner
	{
		get
		{
			return _failoverPartner;
		}
		set
		{
			SetValue("Failover Partner", value);
			_failoverPartner = value;
		}
	}

	/// <summary>Gets or sets the name of the database associated with the connection.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.InitialCatalog" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[TypeConverter(typeof(SqlInitialCatalogConverter))]
	public string InitialCatalog
	{
		get
		{
			return _initialCatalog;
		}
		set
		{
			SetValue("Initial Catalog", value);
			_initialCatalog = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true).</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.IntegratedSecurity" /> property, or false if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool IntegratedSecurity
	{
		get
		{
			return _integratedSecurity;
		}
		set
		{
			SetValue("Integrated Security", value);
			_integratedSecurity = value;
		}
	}

	/// <summary>Gets or sets the minimum time, in seconds, for the connection to live in the connection pool before being destroyed.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.LoadBalanceTimeout" /> property, or 0 if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public int LoadBalanceTimeout
	{
		get
		{
			return _loadBalanceTimeout;
		}
		set
		{
			if (value < 0)
			{
				throw ADP.InvalidConnectionOptionValue("Load Balance Timeout");
			}
			SetValue("Load Balance Timeout", value);
			_loadBalanceTimeout = value;
		}
	}

	/// <summary>Gets or sets the maximum number of connections allowed in the connection pool for this specific connection string.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.MaxPoolSize" /> property, or 100 if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public int MaxPoolSize
	{
		get
		{
			return _maxPoolSize;
		}
		set
		{
			if (value < 1)
			{
				throw ADP.InvalidConnectionOptionValue("Max Pool Size");
			}
			SetValue("Max Pool Size", value);
			_maxPoolSize = value;
		}
	}

	public int ConnectRetryCount
	{
		get
		{
			return _connectRetryCount;
		}
		set
		{
			if (value < 0 || value > 255)
			{
				throw ADP.InvalidConnectionOptionValue("ConnectRetryCount");
			}
			SetValue("ConnectRetryCount", value);
			_connectRetryCount = value;
		}
	}

	public int ConnectRetryInterval
	{
		get
		{
			return _connectRetryInterval;
		}
		set
		{
			if (value < 1 || value > 60)
			{
				throw ADP.InvalidConnectionOptionValue("ConnectRetryInterval");
			}
			SetValue("ConnectRetryInterval", value);
			_connectRetryInterval = value;
		}
	}

	/// <summary>Gets or sets the minimum number of connections allowed in the connection pool for this specific connection string.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.MinPoolSize" /> property, or 0 if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public int MinPoolSize
	{
		get
		{
			return _minPoolSize;
		}
		set
		{
			if (value < 0)
			{
				throw ADP.InvalidConnectionOptionValue("Min Pool Size");
			}
			SetValue("Min Pool Size", value);
			_minPoolSize = value;
		}
	}

	/// <summary>When true, an application can maintain multiple active result sets (MARS). When false, an application must process or cancel all result sets from one batch before it can execute any other batch on that connection.For more information, see Multiple Active Result Sets (MARS).</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.MultipleActiveResultSets" /> property, or false if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool MultipleActiveResultSets
	{
		get
		{
			return _multipleActiveResultSets;
		}
		set
		{
			SetValue("MultipleActiveResultSets", value);
			_multipleActiveResultSets = value;
		}
	}

	/// <summary>If your application is connecting to an AlwaysOn availability group (AG) on different subnets, setting MultiSubnetFailover=true provides faster detection of and connection to the (currently) active server. For more information about SqlClient support for Always On Availability Groups, see SqlClient Support for High Availability, Disaster Recovery.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" /> indicating the current value of the property.</returns>
	public bool MultiSubnetFailover
	{
		get
		{
			return _multiSubnetFailover;
		}
		set
		{
			SetValue("MultiSubnetFailover", value);
			_multiSubnetFailover = value;
		}
	}

	/// <summary>Gets or sets the size in bytes of the network packets used to communicate with an instance of SQL Server.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.PacketSize" /> property, or 8000 if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public int PacketSize
	{
		get
		{
			return _packetSize;
		}
		set
		{
			if (value < 512 || 32768 < value)
			{
				throw SQL.InvalidPacketSizeValue();
			}
			SetValue("Packet Size", value);
			_packetSize = value;
		}
	}

	/// <summary>Gets or sets the password for the SQL Server account.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.Password" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">The password was incorrectly set to null.  See code sample below.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string Password
	{
		get
		{
			return _password;
		}
		set
		{
			SetValue("Password", value);
			_password = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates if security-sensitive information, such as the password, is not returned as part of the connection if the connection is open or has ever been in an open state.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.PersistSecurityInfo" /> property, or false if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool PersistSecurityInfo
	{
		get
		{
			return _persistSecurityInfo;
		}
		set
		{
			SetValue("Persist Security Info", value);
			_persistSecurityInfo = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether the connection will be pooled or explicitly opened every time that the connection is requested.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.Pooling" /> property, or true if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool Pooling
	{
		get
		{
			return _pooling;
		}
		set
		{
			SetValue("Pooling", value);
			_pooling = value;
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether replication is supported using the connection.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.Replication" /> property, or false if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool Replication
	{
		get
		{
			return _replication;
		}
		set
		{
			SetValue("Replication", value);
			_replication = value;
		}
	}

	/// <summary>Gets or sets a string value that indicates how the connection maintains its association with an enlisted System.Transactions transaction.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.TransactionBinding" /> property, or String.Empty if none has been supplied.</returns>
	public string TransactionBinding
	{
		get
		{
			return _transactionBinding;
		}
		set
		{
			SetValue("Transaction Binding", value);
			_transactionBinding = value;
		}
	}

	/// <summary>Gets or sets a string value that indicates the type system the application expects.</summary>
	/// <returns>The following table shows the possible values for the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.TypeSystemVersion" /> property:ValueDescriptionSQL Server 2005Uses the SQL Server 2005 type system. No conversions are made for the current version of ADO.NET.SQL Server 2008Uses the SQL Server 2008 type system.LatestUse the latest version than this client-server pair can handle. This will automatically move forward as the client and server components are upgraded.</returns>
	public string TypeSystemVersion
	{
		get
		{
			return _typeSystemVersion;
		}
		set
		{
			SetValue("Type System Version", value);
			_typeSystemVersion = value;
		}
	}

	/// <summary>Gets or sets the user ID to be used when connecting to SQL Server.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.UserID" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string UserID
	{
		get
		{
			return _userID;
		}
		set
		{
			SetValue("User ID", value);
			_userID = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to redirect the connection from the default SQL Server Express instance to a runtime-initiated instance running under the account of the caller.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.UserInstance" /> property, or False if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	public bool UserInstance
	{
		get
		{
			return _userInstance;
		}
		set
		{
			SetValue("User Instance", value);
			_userInstance = value;
		}
	}

	/// <summary>Gets or sets the name of the workstation connecting to SQL Server.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.WorkstationID" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string WorkstationID
	{
		get
		{
			return _workstationID;
		}
		set
		{
			SetValue("Workstation ID", value);
			_workstationID = value;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override ICollection Keys => new ReadOnlyCollection<string>(s_validKeywords);

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the values in the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains the values in the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override ICollection Values
	{
		get
		{
			object[] array = new object[s_validKeywords.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetAt((Keywords)i);
			}
			return new ReadOnlyCollection<object>(array);
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether asynchronous processing is allowed by the connection created by using this connection string.</summary>
	/// <returns>CautionThis property is ignored beginning in .NET Framework 4.5. For more information about SqlClient support for asynchronous programming, see Asynchronous Programming.The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.AsynchronousProcessing" /> property, or false if no value has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("This property is ignored beginning in .NET Framework 4.5.For more information about SqlClient support for asynchronous programming, seehttps://docs.microsoft.com/en-us/dotnet/framework/data/adonet/asynchronous-programming")]
	public bool AsynchronousProcessing { get; set; }

	/// <summary>Obsolete. Gets or sets a Boolean value that indicates whether the connection is reset when drawn from the connection pool.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.ConnectionReset" /> property, or true if no value has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("ConnectionReset has been deprecated.  SqlConnection will ignore the 'connection reset'keyword and always reset the connection")]
	public bool ConnectionReset { get; set; }

	[System.MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
	public SqlAuthenticationMethod Authentication
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

	/// <summary>Gets or sets a value that indicates whether a client/server or in-process connection to SQL Server should be made.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.ContextConnection" /> property, or False if none has been supplied.</returns>
	[System.MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
	public bool ContextConnection
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

	/// <summary>Gets or sets a string that contains the name of the network library used to establish a connection to the SQL Server.</summary>
	/// <returns>The value of the <see cref="P:System.Data.SqlClient.SqlConnectionStringBuilder.NetworkLibrary" /> property, or String.Empty if none has been supplied.</returns>
	/// <exception cref="T:System.ArgumentNullException">To set the value to null, use <see cref="F:System.DBNull.Value" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
	public string NetworkLibrary
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

	[System.MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
	public PoolBlockingPeriod PoolBlockingPeriod
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

	[System.MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
	public bool TransparentNetworkIPResolution
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

	[System.MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
	public SqlConnectionColumnEncryptionSetting ColumnEncryptionSetting
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

	public string EnclaveAttestationUrl
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	private static string[] CreateValidKeywords()
	{
		string[] array = new string[29];
		array[25] = "ApplicationIntent";
		array[20] = "Application Name";
		array[2] = "AttachDbFilename";
		array[14] = "Connect Timeout";
		array[21] = "Current Language";
		array[0] = "Data Source";
		array[15] = "Encrypt";
		array[8] = "Enlist";
		array[1] = "Failover Partner";
		array[3] = "Initial Catalog";
		array[4] = "Integrated Security";
		array[17] = "Load Balance Timeout";
		array[11] = "Max Pool Size";
		array[10] = "Min Pool Size";
		array[12] = "MultipleActiveResultSets";
		array[26] = "MultiSubnetFailover";
		array[18] = "Packet Size";
		array[7] = "Password";
		array[5] = "Persist Security Info";
		array[9] = "Pooling";
		array[13] = "Replication";
		array[24] = "Transaction Binding";
		array[16] = "TrustServerCertificate";
		array[19] = "Type System Version";
		array[6] = "User ID";
		array[23] = "User Instance";
		array[22] = "Workstation ID";
		array[27] = "ConnectRetryCount";
		array[28] = "ConnectRetryInterval";
		return array;
	}

	private static Dictionary<string, Keywords> CreateKeywordsDictionary()
	{
		return new Dictionary<string, Keywords>(47, StringComparer.OrdinalIgnoreCase)
		{
			{
				"ApplicationIntent",
				Keywords.ApplicationIntent
			},
			{
				"Application Name",
				Keywords.ApplicationName
			},
			{
				"AttachDbFilename",
				Keywords.AttachDBFilename
			},
			{
				"Connect Timeout",
				Keywords.ConnectTimeout
			},
			{
				"Current Language",
				Keywords.CurrentLanguage
			},
			{
				"Data Source",
				Keywords.DataSource
			},
			{
				"Encrypt",
				Keywords.Encrypt
			},
			{
				"Enlist",
				Keywords.Enlist
			},
			{
				"Failover Partner",
				Keywords.FailoverPartner
			},
			{
				"Initial Catalog",
				Keywords.InitialCatalog
			},
			{
				"Integrated Security",
				Keywords.IntegratedSecurity
			},
			{
				"Load Balance Timeout",
				Keywords.LoadBalanceTimeout
			},
			{
				"MultipleActiveResultSets",
				Keywords.MultipleActiveResultSets
			},
			{
				"Max Pool Size",
				Keywords.MaxPoolSize
			},
			{
				"Min Pool Size",
				Keywords.MinPoolSize
			},
			{
				"MultiSubnetFailover",
				Keywords.MultiSubnetFailover
			},
			{
				"Packet Size",
				Keywords.PacketSize
			},
			{
				"Password",
				Keywords.Password
			},
			{
				"Persist Security Info",
				Keywords.PersistSecurityInfo
			},
			{
				"Pooling",
				Keywords.Pooling
			},
			{
				"Replication",
				Keywords.Replication
			},
			{
				"Transaction Binding",
				Keywords.TransactionBinding
			},
			{
				"TrustServerCertificate",
				Keywords.TrustServerCertificate
			},
			{
				"Type System Version",
				Keywords.TypeSystemVersion
			},
			{
				"User ID",
				Keywords.UserID
			},
			{
				"User Instance",
				Keywords.UserInstance
			},
			{
				"Workstation ID",
				Keywords.WorkstationID
			},
			{
				"ConnectRetryCount",
				Keywords.ConnectRetryCount
			},
			{
				"ConnectRetryInterval",
				Keywords.ConnectRetryInterval
			},
			{
				"app",
				Keywords.ApplicationName
			},
			{
				"extended properties",
				Keywords.AttachDBFilename
			},
			{
				"initial file name",
				Keywords.AttachDBFilename
			},
			{
				"connection timeout",
				Keywords.ConnectTimeout
			},
			{
				"timeout",
				Keywords.ConnectTimeout
			},
			{
				"language",
				Keywords.CurrentLanguage
			},
			{
				"addr",
				Keywords.DataSource
			},
			{
				"address",
				Keywords.DataSource
			},
			{
				"network address",
				Keywords.DataSource
			},
			{
				"server",
				Keywords.DataSource
			},
			{
				"database",
				Keywords.InitialCatalog
			},
			{
				"trusted_connection",
				Keywords.IntegratedSecurity
			},
			{
				"connection lifetime",
				Keywords.LoadBalanceTimeout
			},
			{
				"pwd",
				Keywords.Password
			},
			{
				"persistsecurityinfo",
				Keywords.PersistSecurityInfo
			},
			{
				"uid",
				Keywords.UserID
			},
			{
				"user",
				Keywords.UserID
			},
			{
				"wsid",
				Keywords.WorkstationID
			}
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> class.</summary>
	public SqlConnectionStringBuilder()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> class. The provided connection string provides the data for the instance's internal connection information.</summary>
	/// <param name="connectionString">The basis for the object's internal connection information. Parsed into name/value pairs. Invalid key names raise <see cref="T:System.Collections.Generic.KeyNotFoundException" />.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Invalid key name within the connection string.</exception>
	/// <exception cref="T:System.FormatException">Invalid value within the connection string (specifically, when a Boolean or numeric value was expected but not supplied).</exception>
	/// <exception cref="T:System.ArgumentException">The supplied <paramref name="connectionString" /> is not valid.</exception>
	public SqlConnectionStringBuilder(string connectionString)
	{
		if (!string.IsNullOrEmpty(connectionString))
		{
			base.ConnectionString = connectionString;
		}
	}

	/// <summary>Clears the contents of the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> instance.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override void Clear()
	{
		base.Clear();
		for (int i = 0; i < s_validKeywords.Length; i++)
		{
			Reset((Keywords)i);
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> contains an element that has the specified key; otherwise, false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic)</exception>
	/// <filterpriority>1</filterpriority>
	public override bool ContainsKey(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		return s_keywords.ContainsKey(keyword);
	}

	private static bool ConvertToBoolean(object value)
	{
		return DbConnectionStringBuilderUtil.ConvertToBoolean(value);
	}

	private static int ConvertToInt32(object value)
	{
		return DbConnectionStringBuilderUtil.ConvertToInt32(value);
	}

	private static bool ConvertToIntegratedSecurity(object value)
	{
		return DbConnectionStringBuilderUtil.ConvertToIntegratedSecurity(value);
	}

	private static string ConvertToString(object value)
	{
		return DbConnectionStringBuilderUtil.ConvertToString(value);
	}

	private static ApplicationIntent ConvertToApplicationIntent(string keyword, object value)
	{
		return DbConnectionStringBuilderUtil.ConvertToApplicationIntent(keyword, value);
	}

	private object GetAt(Keywords index)
	{
		return index switch
		{
			Keywords.ApplicationIntent => ApplicationIntent, 
			Keywords.ApplicationName => ApplicationName, 
			Keywords.AttachDBFilename => AttachDBFilename, 
			Keywords.ConnectTimeout => ConnectTimeout, 
			Keywords.CurrentLanguage => CurrentLanguage, 
			Keywords.DataSource => DataSource, 
			Keywords.Encrypt => Encrypt, 
			Keywords.Enlist => Enlist, 
			Keywords.FailoverPartner => FailoverPartner, 
			Keywords.InitialCatalog => InitialCatalog, 
			Keywords.IntegratedSecurity => IntegratedSecurity, 
			Keywords.LoadBalanceTimeout => LoadBalanceTimeout, 
			Keywords.MultipleActiveResultSets => MultipleActiveResultSets, 
			Keywords.MaxPoolSize => MaxPoolSize, 
			Keywords.MinPoolSize => MinPoolSize, 
			Keywords.MultiSubnetFailover => MultiSubnetFailover, 
			Keywords.PacketSize => PacketSize, 
			Keywords.Password => Password, 
			Keywords.PersistSecurityInfo => PersistSecurityInfo, 
			Keywords.Pooling => Pooling, 
			Keywords.Replication => Replication, 
			Keywords.TransactionBinding => TransactionBinding, 
			Keywords.TrustServerCertificate => TrustServerCertificate, 
			Keywords.TypeSystemVersion => TypeSystemVersion, 
			Keywords.UserID => UserID, 
			Keywords.UserInstance => UserInstance, 
			Keywords.WorkstationID => WorkstationID, 
			Keywords.ConnectRetryCount => ConnectRetryCount, 
			Keywords.ConnectRetryInterval => ConnectRetryInterval, 
			_ => throw UnsupportedKeyword(s_validKeywords[(int)index]), 
		};
	}

	private Keywords GetIndex(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		if (s_keywords.TryGetValue(keyword, out var value))
		{
			return value;
		}
		throw UnsupportedKeyword(keyword);
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> instance.</summary>
	/// <returns>true if the key existed within the connection string and was removed; false if the key did not exist.</returns>
	/// <param name="keyword">The key of the key/value pair to be removed from the connection string in this <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic)</exception>
	/// <filterpriority>1</filterpriority>
	public override bool Remove(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		if (s_keywords.TryGetValue(keyword, out var value) && base.Remove(s_validKeywords[(int)value]))
		{
			Reset(value);
			return true;
		}
		return false;
	}

	private void Reset(Keywords index)
	{
		switch (index)
		{
		case Keywords.ApplicationIntent:
			_applicationIntent = ApplicationIntent.ReadWrite;
			break;
		case Keywords.ApplicationName:
			_applicationName = "Core .Net SqlClient Data Provider";
			break;
		case Keywords.AttachDBFilename:
			_attachDBFilename = "";
			break;
		case Keywords.ConnectTimeout:
			_connectTimeout = 15;
			break;
		case Keywords.CurrentLanguage:
			_currentLanguage = "";
			break;
		case Keywords.DataSource:
			_dataSource = "";
			break;
		case Keywords.Encrypt:
			_encrypt = false;
			break;
		case Keywords.Enlist:
			_enlist = true;
			break;
		case Keywords.FailoverPartner:
			_failoverPartner = "";
			break;
		case Keywords.InitialCatalog:
			_initialCatalog = "";
			break;
		case Keywords.IntegratedSecurity:
			_integratedSecurity = false;
			break;
		case Keywords.LoadBalanceTimeout:
			_loadBalanceTimeout = 0;
			break;
		case Keywords.MultipleActiveResultSets:
			_multipleActiveResultSets = false;
			break;
		case Keywords.MaxPoolSize:
			_maxPoolSize = 100;
			break;
		case Keywords.MinPoolSize:
			_minPoolSize = 0;
			break;
		case Keywords.MultiSubnetFailover:
			_multiSubnetFailover = false;
			break;
		case Keywords.PacketSize:
			_packetSize = 8000;
			break;
		case Keywords.Password:
			_password = "";
			break;
		case Keywords.PersistSecurityInfo:
			_persistSecurityInfo = false;
			break;
		case Keywords.Pooling:
			_pooling = true;
			break;
		case Keywords.ConnectRetryCount:
			_connectRetryCount = 1;
			break;
		case Keywords.ConnectRetryInterval:
			_connectRetryInterval = 10;
			break;
		case Keywords.Replication:
			_replication = false;
			break;
		case Keywords.TransactionBinding:
			_transactionBinding = "Implicit Unbind";
			break;
		case Keywords.TrustServerCertificate:
			_trustServerCertificate = false;
			break;
		case Keywords.TypeSystemVersion:
			_typeSystemVersion = "Latest";
			break;
		case Keywords.UserID:
			_userID = "";
			break;
		case Keywords.UserInstance:
			_userInstance = false;
			break;
		case Keywords.WorkstationID:
			_workstationID = "";
			break;
		default:
			throw UnsupportedKeyword(s_validKeywords[(int)index]);
		}
	}

	private void SetValue(string keyword, bool value)
	{
		base[keyword] = value.ToString();
	}

	private void SetValue(string keyword, int value)
	{
		base[keyword] = value.ToString((IFormatProvider)null);
	}

	private void SetValue(string keyword, string value)
	{
		ADP.CheckArgumentNull(value, keyword);
		base[keyword] = value;
	}

	private void SetApplicationIntentValue(ApplicationIntent value)
	{
		base["ApplicationIntent"] = DbConnectionStringBuilderUtil.ApplicationIntentToString(value);
	}

	/// <summary>Indicates whether the specified key exists in this <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> instance.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" /> contains an entry with the specified key; otherwise, false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</param>
	/// <filterpriority>1</filterpriority>
	public override bool ShouldSerialize(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		if (s_keywords.TryGetValue(keyword, out var value))
		{
			return base.ShouldSerialize(s_validKeywords[(int)value]);
		}
		return false;
	}

	/// <summary>Retrieves a value corresponding to the supplied key from this <see cref="T:System.Data.SqlClient.SqlConnectionStringBuilder" />.</summary>
	/// <returns>true if <paramref name="keyword" /> was found within the connection string; otherwise, false.</returns>
	/// <param name="keyword">The key of the item to retrieve.</param>
	/// <param name="value">The value corresponding to <paramref name="keyword." /></param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> contains a null value (Nothing in Visual Basic).</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool TryGetValue(string keyword, out object value)
	{
		if (s_keywords.TryGetValue(keyword, out var value2))
		{
			value = GetAt(value2);
			return true;
		}
		value = null;
		return false;
	}

	private Exception UnsupportedKeyword(string keyword)
	{
		if (s_notSupportedKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
		{
			return SQL.UnsupportedKeyword(keyword);
		}
		if (s_notSupportedNetworkLibraryKeywords.Contains(keyword, StringComparer.OrdinalIgnoreCase))
		{
			return SQL.NetworkLibraryKeywordNotSupported();
		}
		return ADP.KeywordNotSupported(keyword);
	}
}
