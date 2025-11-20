using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Represents a set of data commands and a database connection that are used to fill the <see cref="T:System.Data.DataSet" /> and update the data source.</summary>
/// <filterpriority>1</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbDataAdapter : DbDataAdapter, IDataAdapter, IDbDataAdapter, ICloneable
{
	/// <summary>Gets or sets an SQL statement or stored procedure for deleting records from the data set.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to delete records in the data source that correspond to deleted rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OleDbCommand DeleteCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets an SQL statement or stored procedure used to insert new records into the data source.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to insert records in the data source that correspond to new rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OleDbCommand InsertCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets an SQL statement or stored procedure used to select records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbCommand" /> that is used during <see cref="M:System.Data.Common.DbDataAdapter.Fill(System.Data.DataSet)" /> to select records from data source for placement in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OleDbCommand SelectCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.DeleteCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during an update to delete records in the data source for deleted rows in the data set.</returns>
	IDbCommand IDbDataAdapter.DeleteCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.InsertCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during an update to insert records from a data source for placement in the data set.</returns>
	IDbCommand IDbDataAdapter.InsertCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.SelectCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during an update to select records from a data source for placement in the data set.</returns>
	IDbCommand IDbDataAdapter.SelectCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.UpdateCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during an update to update records in the data source for modified rows in the data set.</returns>
	IDbCommand IDbDataAdapter.UpdateCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets an SQL statement or stored procedure used to update records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to update records in the data source that correspond to modified rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OleDbCommand UpdateCommand
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Occurs during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> after a command is executed against the data source. The attempt to update is made. Therefore, the event occurs.</summary>
	/// <filterpriority>2</filterpriority>
	public event OleDbRowUpdatedEventHandler RowUpdated;

	/// <summary>Occurs during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> before a command is executed against the data source. The attempt to update is made. Therefore, the event occurs.</summary>
	/// <filterpriority>2</filterpriority>
	public event OleDbRowUpdatingEventHandler RowUpdating;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" /> class.</summary>
	public OleDbDataAdapter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" /> class with the specified <see cref="T:System.Data.OleDb.OleDbCommand" /> as the <see cref="P:System.Data.OleDb.OleDbDataAdapter.SelectCommand" /> property.</summary>
	/// <param name="selectCommand">An <see cref="T:System.Data.OleDb.OleDbCommand" /> that is a SELECT statement or stored procedure, and is set as the <see cref="P:System.Data.OleDb.OleDbDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" />.</param>
	public OleDbDataAdapter(OleDbCommand selectCommand)
	{
		throw ADP.OleDb();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" /> class with a <see cref="P:System.Data.OleDb.OleDbDataAdapter.SelectCommand" />.</summary>
	/// <param name="selectCommandText">A string that is an SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.OleDb.OleDbDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" />. </param>
	/// <param name="selectConnection">An <see cref="T:System.Data.OleDb.OleDbConnection" /> that represents the connection. </param>
	public OleDbDataAdapter(string selectCommandText, OleDbConnection selectConnection)
	{
		throw ADP.OleDb();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" /> class with a <see cref="P:System.Data.OleDb.OleDbDataAdapter.SelectCommand" />.</summary>
	/// <param name="selectCommandText">A string that is an SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.OleDb.OleDbDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" />. </param>
	/// <param name="selectConnectionString">The connection string. </param>
	public OleDbDataAdapter(string selectCommandText, string selectConnectionString)
	{
		throw ADP.OleDb();
	}

	protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
	{
		throw ADP.OleDb();
	}

	protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds or refreshes rows in the <see cref="T:System.Data.DataSet" /> to match those in an ADO Recordset or Record object using the specified <see cref="T:System.Data.DataSet" />, ADO object, and source table name.</summary>
	/// <returns>The number of rows successfully added to or refreshed in the <see cref="T:System.Data.DataSet" />. This does not include rows affected by statements that do not return rows.</returns>
	/// <param name="dataSet">A <see cref="T:System.Data.DataSet" /> to fill with records and, if it is required, schema. </param>
	/// <param name="ADODBRecordSet">An ADO Recordset or Record object. </param>
	/// <param name="srcTable">The source table used for the table mappings. </param>
	/// <exception cref="T:System.SystemException">The source table is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Fill(DataSet dataSet, object ADODBRecordSet, string srcTable)
	{
		throw ADP.OleDb();
	}

	/// <summary>Adds or refreshes rows in a <see cref="T:System.Data.DataTable" /> to match those in an ADO Recordset or Record object using the specified <see cref="T:System.Data.DataTable" /> and ADO objects.</summary>
	/// <returns>The number of rows successfully refreshed to the <see cref="T:System.Data.DataTable" />. This does not include rows affected by statements that do not return rows.</returns>
	/// <param name="dataTable">A <see cref="T:System.Data.DataTable" /> to fill with records and, if it is required, schema. </param>
	/// <param name="ADODBRecordSet">An ADO Recordset or Record object. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int Fill(DataTable dataTable, object ADODBRecordSet)
	{
		throw ADP.OleDb();
	}

	protected override void OnRowUpdated(RowUpdatedEventArgs value)
	{
		throw ADP.OleDb();
	}

	protected override void OnRowUpdating(RowUpdatingEventArgs value)
	{
		throw ADP.OleDb();
	}

	/// <summary>For a description of this member, see <see cref="M:System.ICloneable.Clone" />.</summary>
	/// <returns>A new <see cref="T:System.Object" /> that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		throw ADP.OleDb();
	}
}
