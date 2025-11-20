using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace System.Data.Common;

/// <summary>Represents a set of static methods for creating one or more instances of <see cref="T:System.Data.Common.DbProviderFactory" /> classes.</summary>
/// <filterpriority>2</filterpriority>
public static class DbProviderFactories
{
	private struct ProviderRegistration
	{
		internal string FactoryTypeAssemblyQualifiedName { get; }

		internal DbProviderFactory FactoryInstance { get; }

		internal ProviderRegistration(string factoryTypeAssemblyQualifiedName, DbProviderFactory factoryInstance)
		{
			FactoryTypeAssemblyQualifiedName = factoryTypeAssemblyQualifiedName;
			FactoryInstance = factoryInstance;
		}
	}

	private const string AssemblyQualifiedName = "AssemblyQualifiedName";

	private const string Instance = "Instance";

	private const string InvariantName = "InvariantName";

	private const string Name = "Name";

	private const string Description = "Description";

	private const string InstanceFieldName = "Instance";

	private static ConcurrentDictionary<string, ProviderRegistration> _registeredFactories = new ConcurrentDictionary<string, ProviderRegistration>();

	private static ConnectionState _initState;

	private static DataTable _providerTable;

	private static object _lockobj = new object();

	/// <summary>Returns an instance of a <see cref="T:System.Data.Common.DbProviderFactory" />.</summary>
	/// <returns>An instance of a <see cref="T:System.Data.Common.DbProviderFactory" /> for a specified provider name.</returns>
	/// <param name="providerInvariantName">Invariant name of a provider.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static DbProviderFactory GetFactory(string providerInvariantName)
	{
		return GetFactory(providerInvariantName, throwOnError: true);
	}

	public static DbProviderFactory GetFactory(string providerInvariantName, bool throwOnError)
	{
		if (throwOnError)
		{
			ADP.CheckArgumentLength(providerInvariantName, "providerInvariantName");
		}
		DataTable providerTable = GetProviderTable();
		if (providerTable != null)
		{
			DataRow dataRow = providerTable.Rows.Find(providerInvariantName);
			if (dataRow != null)
			{
				return GetFactory(dataRow);
			}
		}
		if (throwOnError)
		{
			throw ADP.ConfigProviderNotFound();
		}
		return null;
	}

	/// <summary>Returns an instance of a <see cref="T:System.Data.Common.DbProviderFactory" />.</summary>
	/// <returns>An instance of a <see cref="T:System.Data.Common.DbProviderFactory" /> for a specified <see cref="T:System.Data.DataRow" />.</returns>
	/// <param name="providerRow">
	///   <see cref="T:System.Data.DataRow" /> containing the provider's configuration information.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static DbProviderFactory GetFactory(DataRow providerRow)
	{
		ADP.CheckArgumentNull(providerRow, "providerRow");
		DataColumn dataColumn = providerRow.Table.Columns["AssemblyQualifiedName"];
		if (dataColumn != null)
		{
			string text = providerRow[dataColumn] as string;
			if (!ADP.IsEmpty(text))
			{
				Type type = Type.GetType(text);
				if (null != type)
				{
					FieldInfo field = type.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
					if (null != field && field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
					{
						object value = field.GetValue(null);
						if (value != null)
						{
							return (DbProviderFactory)value;
						}
					}
					throw ADP.ConfigProviderInvalid();
				}
				throw ADP.ConfigProviderNotInstalled();
			}
		}
		throw ADP.ConfigProviderMissing();
	}

	/// <summary>Returns an instance of a <see cref="T:System.Data.Common.DbProviderFactory" />.</summary>
	/// <returns>An instance of a <see cref="T:System.Data.Common.DbProviderFactory" /> for a specified connection.</returns>
	/// <param name="connection">The connection used.</param>
	public static DbProviderFactory GetFactory(DbConnection connection)
	{
		ADP.CheckArgumentNull(connection, "connection");
		return connection.ProviderFactory;
	}

	/// <summary>Returns a <see cref="T:System.Data.DataTable" /> that contains information about all installed providers that implement <see cref="T:System.Data.Common.DbProviderFactory" />.</summary>
	/// <returns>Returns a <see cref="T:System.Data.DataTable" /> containing <see cref="T:System.Data.DataRow" /> objects that contain the following data. Column ordinalColumn nameDescription0NameHuman-readable name for the data provider.1DescriptionHuman-readable description of the data provider.2InvariantNameName that can be used programmatically to refer to the data provider.3AssemblyQualifiedNameFully qualified name of the factory class, which contains enough information to instantiate the object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static DataTable GetFactoryClasses()
	{
		DataTable providerTable = GetProviderTable();
		if (providerTable != null)
		{
			return providerTable.Copy();
		}
		return DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
	}

	private static DataTable IncludeFrameworkFactoryClasses(DataTable configDataTable)
	{
		DataTable dataTable = DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
		string factoryAssemblyQualifiedName = typeof(SqlClientFactory).AssemblyQualifiedName.ToString().Replace("System.Data.SqlClient.SqlClientFactory, System.Data,", "System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient,");
		DbProviderFactoryConfigSection[] array = new DbProviderFactoryConfigSection[4]
		{
			new DbProviderFactoryConfigSection(typeof(OdbcFactory), "Odbc Data Provider", ".Net Framework Data Provider for Odbc"),
			new DbProviderFactoryConfigSection(typeof(OleDbFactory), "OleDb Data Provider", ".Net Framework Data Provider for OleDb"),
			new DbProviderFactoryConfigSection("OracleClient Data Provider", "System.Data.OracleClient", ".Net Framework Data Provider for Oracle", factoryAssemblyQualifiedName),
			new DbProviderFactoryConfigSection(typeof(SqlClientFactory), "SqlClient Data Provider", ".Net Framework Data Provider for SqlServer")
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsNull())
			{
				continue;
			}
			bool flag = false;
			if (i == 2)
			{
				Type type = Type.GetType(array[i].AssemblyQualifiedName);
				if (type != null)
				{
					FieldInfo field = type.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
					if (null != field && field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
					{
						object value = field.GetValue(null);
						if (value != null)
						{
							flag = true;
						}
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				DataRow dataRow = dataTable.NewRow();
				dataRow["Name"] = array[i].Name;
				dataRow["InvariantName"] = array[i].InvariantName;
				dataRow["Description"] = array[i].Description;
				dataRow["AssemblyQualifiedName"] = array[i].AssemblyQualifiedName;
				dataTable.Rows.Add(dataRow);
			}
		}
		int num = 0;
		while (configDataTable != null && num < configDataTable.Rows.Count)
		{
			try
			{
				bool flag2 = false;
				if (configDataTable.Rows[num]["AssemblyQualifiedName"].ToString().ToLowerInvariant().Contains("System.Data.OracleClient".ToString().ToLowerInvariant()))
				{
					Type type2 = Type.GetType(configDataTable.Rows[num]["AssemblyQualifiedName"].ToString());
					if (type2 != null)
					{
						FieldInfo field2 = type2.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
						if (null != field2 && field2.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
						{
							object value2 = field2.GetValue(null);
							if (value2 != null)
							{
								flag2 = true;
							}
						}
					}
				}
				else
				{
					flag2 = true;
				}
				if (flag2)
				{
					dataTable.Rows.Add(configDataTable.Rows[num].ItemArray);
				}
			}
			catch (ConstraintException)
			{
			}
			num++;
		}
		return dataTable;
	}

	private static DataTable GetProviderTable()
	{
		Initialize();
		return _providerTable;
	}

	private static void Initialize()
	{
		if (ConnectionState.Open == _initState)
		{
			return;
		}
		lock (_lockobj)
		{
			switch (_initState)
			{
			case ConnectionState.Closed:
				_initState = ConnectionState.Connecting;
				try
				{
					_providerTable = ((System.Configuration.PrivilegedConfigurationManager.GetSection("system.data") is DataSet dataSet) ? IncludeFrameworkFactoryClasses(dataSet.Tables["DbProviderFactories"]) : IncludeFrameworkFactoryClasses(null));
					break;
				}
				finally
				{
					_initState = ConnectionState.Open;
				}
			}
		}
	}

	public static bool TryGetFactory(string providerInvariantName, out DbProviderFactory factory)
	{
		factory = GetFactory(providerInvariantName, throwOnError: false);
		return factory != null;
	}

	public static IEnumerable<string> GetProviderInvariantNames()
	{
		return _registeredFactories.Keys.ToList();
	}

	public static void RegisterFactory(string providerInvariantName, string factoryTypeAssemblyQualifiedName)
	{
		ADP.CheckArgumentLength(providerInvariantName, "providerInvariantName");
		ADP.CheckArgumentLength(factoryTypeAssemblyQualifiedName, "factoryTypeAssemblyQualifiedName");
		_registeredFactories[providerInvariantName] = new ProviderRegistration(factoryTypeAssemblyQualifiedName, null);
	}

	private static DbProviderFactory GetFactoryInstance(Type providerFactoryClass)
	{
		ADP.CheckArgumentNull(providerFactoryClass, "providerFactoryClass");
		if (!providerFactoryClass.IsSubclassOf(typeof(DbProviderFactory)))
		{
			throw ADP.Argument(global::SR.Format("The type '{0}' doesn't inherit from DbProviderFactory.", providerFactoryClass.FullName));
		}
		FieldInfo field = providerFactoryClass.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		if (null == field)
		{
			throw ADP.InvalidOperation("The requested .NET Data Provider's implementation does not have an Instance field of a System.Data.Common.DbProviderFactory derived type.");
		}
		if (!field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
		{
			throw ADP.InvalidOperation("The requested .NET Data Provider's implementation does not have an Instance field of a System.Data.Common.DbProviderFactory derived type.");
		}
		object value = field.GetValue(null);
		if (value == null)
		{
			throw ADP.InvalidOperation("The requested .NET Data Provider's implementation does not have an Instance field of a System.Data.Common.DbProviderFactory derived type.");
		}
		return (DbProviderFactory)value;
	}

	public static void RegisterFactory(string providerInvariantName, Type providerFactoryClass)
	{
		RegisterFactory(providerInvariantName, GetFactoryInstance(providerFactoryClass));
	}

	public static void RegisterFactory(string providerInvariantName, DbProviderFactory factory)
	{
		ADP.CheckArgumentLength(providerInvariantName, "providerInvariantName");
		ADP.CheckArgumentNull(factory, "factory");
		_registeredFactories[providerInvariantName] = new ProviderRegistration(factory.GetType().AssemblyQualifiedName, factory);
	}

	public static bool UnregisterFactory(string providerInvariantName)
	{
		ProviderRegistration value;
		if (!string.IsNullOrWhiteSpace(providerInvariantName))
		{
			return _registeredFactories.TryRemove(providerInvariantName, out value);
		}
		return false;
	}
}
