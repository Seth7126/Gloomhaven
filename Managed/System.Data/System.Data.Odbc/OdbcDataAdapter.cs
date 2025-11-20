using System.Data.Common;

namespace System.Data.Odbc;

/// <summary>Represents a set of data commands and a connection to a data source that are used to fill the <see cref="T:System.Data.DataSet" /> and update the data source. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class OdbcDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
{
	private static readonly object s_eventRowUpdated = new object();

	private static readonly object s_eventRowUpdating = new object();

	private OdbcCommand _deleteCommand;

	private OdbcCommand _insertCommand;

	private OdbcCommand _selectCommand;

	private OdbcCommand _updateCommand;

	/// <summary>Gets or sets an SQL statement or stored procedure used to delete records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcCommand" /> used during an update operation to delete records in the data source that correspond to deleted rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand DeleteCommand
	{
		get
		{
			return _deleteCommand;
		}
		set
		{
			_deleteCommand = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.DeleteCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during an update to delete records in the data source for deleted rows in the data set.</returns>
	IDbCommand IDbDataAdapter.DeleteCommand
	{
		get
		{
			return _deleteCommand;
		}
		set
		{
			_deleteCommand = (OdbcCommand)value;
		}
	}

	/// <summary>Gets or sets an SQL statement or stored procedure used to insert new records into the data source.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcCommand" /> used during an update operation to insert records in the data source that correspond to new rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand InsertCommand
	{
		get
		{
			return _insertCommand;
		}
		set
		{
			_insertCommand = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.InsertCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during an update to insert records from a data source for placement in the data set.</returns>
	IDbCommand IDbDataAdapter.InsertCommand
	{
		get
		{
			return _insertCommand;
		}
		set
		{
			_insertCommand = (OdbcCommand)value;
		}
	}

	/// <summary>Gets or sets an SQL statement or stored procedure used to select records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcCommand" /> that is used during a fill operation to select records from data source for placement in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand SelectCommand
	{
		get
		{
			return _selectCommand;
		}
		set
		{
			_selectCommand = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.SelectCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during an update to select records from a data source for placement in the data set.</returns>
	IDbCommand IDbDataAdapter.SelectCommand
	{
		get
		{
			return _selectCommand;
		}
		set
		{
			_selectCommand = (OdbcCommand)value;
		}
	}

	/// <summary>Gets or sets an SQL statement or stored procedure used to update records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcCommand" /> used during an update operation to update records in the data source that correspond to modified rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new OdbcCommand UpdateCommand
	{
		get
		{
			return _updateCommand;
		}
		set
		{
			_updateCommand = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbDataAdapter.UpdateCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during an update to update records in the data source for modified rows in the data set.</returns>
	IDbCommand IDbDataAdapter.UpdateCommand
	{
		get
		{
			return _updateCommand;
		}
		set
		{
			_updateCommand = (OdbcCommand)value;
		}
	}

	/// <summary>Occurs during an update operation after a command is executed against the data source.</summary>
	/// <filterpriority>2</filterpriority>
	public event OdbcRowUpdatedEventHandler RowUpdated
	{
		add
		{
			base.Events.AddHandler(s_eventRowUpdated, value);
		}
		remove
		{
			base.Events.RemoveHandler(s_eventRowUpdated, value);
		}
	}

	/// <summary>Occurs during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> before a command is executed against the data source.</summary>
	/// <filterpriority>2</filterpriority>
	public event OdbcRowUpdatingEventHandler RowUpdating
	{
		add
		{
			OdbcRowUpdatingEventHandler odbcRowUpdatingEventHandler = (OdbcRowUpdatingEventHandler)base.Events[s_eventRowUpdating];
			if (odbcRowUpdatingEventHandler != null && value.Target is OdbcCommandBuilder)
			{
				OdbcRowUpdatingEventHandler odbcRowUpdatingEventHandler2 = (OdbcRowUpdatingEventHandler)ADP.FindBuilder(odbcRowUpdatingEventHandler);
				if (odbcRowUpdatingEventHandler2 != null)
				{
					base.Events.RemoveHandler(s_eventRowUpdating, odbcRowUpdatingEventHandler2);
				}
			}
			base.Events.AddHandler(s_eventRowUpdating, value);
		}
		remove
		{
			base.Events.RemoveHandler(s_eventRowUpdating, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> class.</summary>
	public OdbcDataAdapter()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> class with the specified SQL SELECT statement.</summary>
	/// <param name="selectCommand">An <see cref="T:System.Data.Odbc.OdbcCommand" /> that is an SQL SELECT statement or stored procedure, and is set as the <see cref="P:System.Data.Odbc.OdbcDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" />. </param>
	public OdbcDataAdapter(OdbcCommand selectCommand)
		: this()
	{
		SelectCommand = selectCommand;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> class with an SQL SELECT statement and an <see cref="T:System.Data.Odbc.OdbcConnection" />.</summary>
	/// <param name="selectCommandText">A string that is a SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.Odbc.OdbcDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" />. </param>
	/// <param name="selectConnection">An <see cref="T:System.Data.Odbc.OdbcConnection" /> that represents the connection. </param>
	public OdbcDataAdapter(string selectCommandText, OdbcConnection selectConnection)
		: this()
	{
		SelectCommand = new OdbcCommand(selectCommandText, selectConnection);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" /> class with an SQL SELECT statement and a connection string.</summary>
	/// <param name="selectCommandText">A string that is a SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.Odbc.OdbcDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.Odbc.OdbcDataAdapter" />. </param>
	/// <param name="selectConnectionString">The connection string. </param>
	public OdbcDataAdapter(string selectCommandText, string selectConnectionString)
		: this()
	{
		OdbcConnection connection = new OdbcConnection(selectConnectionString);
		SelectCommand = new OdbcCommand(selectCommandText, connection);
	}

	private OdbcDataAdapter(OdbcDataAdapter from)
		: base(from)
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>For a description of this member, see <see cref="M:System.ICloneable.Clone" />.</summary>
	/// <returns>A new <see cref="T:System.Object" /> that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		return new OdbcDataAdapter(this);
	}

	protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
	{
		return new OdbcRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
	}

	protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
	{
		return new OdbcRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
	}

	protected override void OnRowUpdated(RowUpdatedEventArgs value)
	{
		OdbcRowUpdatedEventHandler odbcRowUpdatedEventHandler = (OdbcRowUpdatedEventHandler)base.Events[s_eventRowUpdated];
		if (odbcRowUpdatedEventHandler != null && value is OdbcRowUpdatedEventArgs)
		{
			odbcRowUpdatedEventHandler(this, (OdbcRowUpdatedEventArgs)value);
		}
		base.OnRowUpdated(value);
	}

	protected override void OnRowUpdating(RowUpdatingEventArgs value)
	{
		OdbcRowUpdatingEventHandler odbcRowUpdatingEventHandler = (OdbcRowUpdatingEventHandler)base.Events[s_eventRowUpdating];
		if (odbcRowUpdatingEventHandler != null && value is OdbcRowUpdatingEventArgs)
		{
			odbcRowUpdatingEventHandler(this, (OdbcRowUpdatingEventArgs)value);
		}
		base.OnRowUpdating(value);
	}
}
