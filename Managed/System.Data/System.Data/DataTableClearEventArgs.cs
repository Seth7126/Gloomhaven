namespace System.Data;

/// <summary>Provides data for the <see cref="M:System.Data.DataTable.Clear" /> method.</summary>
/// <filterpriority>2</filterpriority>
public sealed class DataTableClearEventArgs : EventArgs
{
	/// <summary>Gets the table whose rows are being cleared.</summary>
	/// <returns>The <see cref="T:System.Data.DataTable" /> whose rows are being cleared.</returns>
	/// <filterpriority>1</filterpriority>
	public DataTable Table { get; }

	/// <summary>Gets the table name whose rows are being cleared.</summary>
	/// <returns>A <see cref="T:System.String" /> indicating the table name.</returns>
	/// <filterpriority>1</filterpriority>
	public string TableName => Table.TableName;

	/// <summary>Gets the namespace of the table whose rows are being cleared.</summary>
	/// <returns>A <see cref="T:System.String" /> indicating the namespace name.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public string TableNamespace => Table.Namespace;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataTableClearEventArgs" /> class.</summary>
	/// <param name="dataTable">The <see cref="T:System.Data.DataTable" /> whose rows are being cleared.</param>
	public DataTableClearEventArgs(DataTable dataTable)
	{
		Table = dataTable;
	}
}
