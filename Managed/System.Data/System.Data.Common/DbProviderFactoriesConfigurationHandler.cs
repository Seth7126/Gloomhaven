using System.Configuration;
using System.Globalization;
using System.Xml;

namespace System.Data.Common;

/// <summary>This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
/// <filterpriority>2</filterpriority>
public class DbProviderFactoriesConfigurationHandler : IConfigurationSectionHandler
{
	private static class DbProviderDictionarySectionHandler
	{
		internal static DataTable CreateStatic(DataTable config, object context, XmlNode section)
		{
			if (section != null)
			{
				HandlerBase.CheckForUnrecognizedAttributes(section);
				if (config == null)
				{
					config = CreateProviderDataTable();
				}
				foreach (XmlNode childNode in section.ChildNodes)
				{
					if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(childNode))
					{
						switch (childNode.Name)
						{
						case "add":
							HandleAdd(childNode, config);
							break;
						case "remove":
							HandleRemove(childNode, config);
							break;
						case "clear":
							HandleClear(childNode, config);
							break;
						default:
							throw ADP.ConfigUnrecognizedElement(childNode);
						}
					}
				}
				config.AcceptChanges();
			}
			return config;
		}

		private static void HandleAdd(XmlNode child, DataTable config)
		{
			HandlerBase.CheckForChildNodes(child);
			DataRow dataRow = config.NewRow();
			dataRow[0] = HandlerBase.RemoveAttribute(child, "name", required: true, allowEmpty: false);
			dataRow[1] = HandlerBase.RemoveAttribute(child, "description", required: true, allowEmpty: false);
			dataRow[2] = HandlerBase.RemoveAttribute(child, "invariant", required: true, allowEmpty: false);
			dataRow[3] = HandlerBase.RemoveAttribute(child, "type", required: true, allowEmpty: false);
			HandlerBase.RemoveAttribute(child, "support", required: false, allowEmpty: false);
			HandlerBase.CheckForUnrecognizedAttributes(child);
			config.Rows.Add(dataRow);
		}

		private static void HandleRemove(XmlNode child, DataTable config)
		{
			HandlerBase.CheckForChildNodes(child);
			string key = HandlerBase.RemoveAttribute(child, "invariant", required: true, allowEmpty: false);
			HandlerBase.CheckForUnrecognizedAttributes(child);
			config.Rows.Find(key)?.Delete();
		}

		private static void HandleClear(XmlNode child, DataTable config)
		{
			HandlerBase.CheckForChildNodes(child);
			HandlerBase.CheckForUnrecognizedAttributes(child);
			config.Clear();
		}
	}

	internal const string sectionName = "system.data";

	internal const string providerGroup = "DbProviderFactories";

	internal const string odbcProviderName = "Odbc Data Provider";

	internal const string odbcProviderDescription = ".Net Framework Data Provider for Odbc";

	internal const string oledbProviderName = "OleDb Data Provider";

	internal const string oledbProviderDescription = ".Net Framework Data Provider for OleDb";

	internal const string oracleclientProviderName = "OracleClient Data Provider";

	internal const string oracleclientProviderNamespace = "System.Data.OracleClient";

	internal const string oracleclientProviderDescription = ".Net Framework Data Provider for Oracle";

	internal const string sqlclientProviderName = "SqlClient Data Provider";

	internal const string sqlclientProviderDescription = ".Net Framework Data Provider for SqlServer";

	internal const string sqlclientPartialAssemblyQualifiedName = "System.Data.SqlClient.SqlClientFactory, System.Data,";

	internal const string oracleclientPartialAssemblyQualifiedName = "System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient,";

	/// <summary>This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	public DbProviderFactoriesConfigurationHandler()
	{
	}

	/// <summary>This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</returns>
	/// <param name="parent">This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</param>
	/// <param name="configContext">This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</param>
	/// <param name="section">This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public virtual object Create(object parent, object configContext, XmlNode section)
	{
		return CreateStatic(parent, configContext, section);
	}

	internal static object CreateStatic(object parent, object configContext, XmlNode section)
	{
		object obj = parent;
		if (section != null)
		{
			obj = HandlerBase.CloneParent(parent as DataSet, insenstive: false);
			bool flag = false;
			HandlerBase.CheckForUnrecognizedAttributes(section);
			foreach (XmlNode childNode in section.ChildNodes)
			{
				if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(childNode))
				{
					string name = childNode.Name;
					if (!(name == "DbProviderFactories"))
					{
						throw ADP.ConfigUnrecognizedElement(childNode);
					}
					if (flag)
					{
						throw ADP.ConfigSectionsUnique("DbProviderFactories");
					}
					flag = true;
					HandleProviders(obj as DataSet, configContext, childNode, name);
				}
			}
		}
		return obj;
	}

	private static void HandleProviders(DataSet config, object configContext, XmlNode section, string sectionName)
	{
		DataTableCollection tables = config.Tables;
		DataTable dataTable = tables[sectionName];
		bool num = dataTable != null;
		dataTable = DbProviderDictionarySectionHandler.CreateStatic(dataTable, configContext, section);
		if (!num)
		{
			tables.Add(dataTable);
		}
	}

	internal static DataTable CreateProviderDataTable()
	{
		DataColumn dataColumn = new DataColumn("Name", typeof(string));
		dataColumn.ReadOnly = true;
		DataColumn dataColumn2 = new DataColumn("Description", typeof(string));
		dataColumn2.ReadOnly = true;
		DataColumn dataColumn3 = new DataColumn("InvariantName", typeof(string));
		dataColumn3.ReadOnly = true;
		DataColumn dataColumn4 = new DataColumn("AssemblyQualifiedName", typeof(string));
		dataColumn4.ReadOnly = true;
		DataColumn[] primaryKey = new DataColumn[1] { dataColumn3 };
		DataColumn[] columns = new DataColumn[4] { dataColumn, dataColumn2, dataColumn3, dataColumn4 };
		DataTable dataTable = new DataTable("DbProviderFactories");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.AddRange(columns);
		dataTable.PrimaryKey = primaryKey;
		return dataTable;
	}
}
