using System.Data.Common;

namespace System.Data.SqlClient;

/// <summary>Represents a set of data commands and a database connection that are used to fill the <see cref="T:System.Data.DataSet" /> and update a SQL Server database. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
{
	private static readonly object EventRowUpdated = new object();

	private static readonly object EventRowUpdating = new object();

	private SqlCommand _deleteCommand;

	private SqlCommand _insertCommand;

	private SqlCommand _selectCommand;

	private SqlCommand _updateCommand;

	private SqlCommandSet _commandSet;

	private int _updateBatchSize = 1;

	/// <summary>Gets or sets a Transact-SQL statement or stored procedure to delete records from the data set.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to delete records in the database that correspond to deleted rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand DeleteCommand
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

	/// <summary>For a description of this member, see <see cref="P:System.Data.IDbDataAdapter.DeleteCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IdbCommandthatis" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to delete records in the data source for deleted rows in the data set.</returns>
	IDbCommand IDbDataAdapter.DeleteCommand
	{
		get
		{
			return _deleteCommand;
		}
		set
		{
			_deleteCommand = (SqlCommand)value;
		}
	}

	/// <summary>Gets or sets a Transact-SQL statement or stored procedure to insert new records into the data source.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to insert records into the database that correspond to new rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand InsertCommand
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

	/// <summary>For a description of this member, see <see cref="P:System.Data.IDbDataAdapter.InsertCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to insert records in the data source for new rows in the data set.</returns>
	IDbCommand IDbDataAdapter.InsertCommand
	{
		get
		{
			return _insertCommand;
		}
		set
		{
			_insertCommand = (SqlCommand)value;
		}
	}

	/// <summary>Gets or sets a Transact-SQL statement or stored procedure used to select records in the data source.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Fill(System.Data.DataSet)" /> to select records from the database for placement in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand SelectCommand
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

	/// <summary>For a description of this member, see <see cref="P:System.Data.IDbDataAdapter.SelectCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to select records from data source for placement in the data set.</returns>
	IDbCommand IDbDataAdapter.SelectCommand
	{
		get
		{
			return _selectCommand;
		}
		set
		{
			_selectCommand = (SqlCommand)value;
		}
	}

	/// <summary>Gets or sets a Transact-SQL statement or stored procedure used to update records in the data source.</summary>
	/// <returns>A <see cref="T:System.Data.SqlClient.SqlCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to update records in the database that correspond to modified rows in the <see cref="T:System.Data.DataSet" />.</returns>
	/// <filterpriority>1</filterpriority>
	public new SqlCommand UpdateCommand
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

	/// <summary>For a description of this member, see <see cref="P:System.Data.IDbDataAdapter.UpdateCommand" />.</summary>
	/// <returns>An <see cref="T:System.Data.IdbCommand" /> that is used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to update records in the data source for modified rows in the data set.</returns>
	IDbCommand IDbDataAdapter.UpdateCommand
	{
		get
		{
			return _updateCommand;
		}
		set
		{
			_updateCommand = (SqlCommand)value;
		}
	}

	/// <summary>Gets or sets the number of rows that are processed in each round-trip to the server.</summary>
	/// <returns>The number of rows to process per-batch. Value isEffect0There is no limit on the batch size..1Disables batch updating.&gt;1Changes are sent using batches of <see cref="P:System.Data.SqlClient.SqlDataAdapter.UpdateBatchSize" /> operations at a time.When setting this to a value other than 1, all the commands associated with the <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> have to have their UpdatedRowSource property set to None or OutputParameters. An exception is thrown otherwise.</returns>
	/// <filterpriority>1</filterpriority>
	public override int UpdateBatchSize
	{
		get
		{
			return _updateBatchSize;
		}
		set
		{
			if (0 > value)
			{
				throw ADP.ArgumentOutOfRange("UpdateBatchSize");
			}
			_updateBatchSize = value;
		}
	}

	/// <summary>Occurs during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> after a command is executed against the data source. The attempt to update is made, so the event fires.</summary>
	/// <filterpriority>2</filterpriority>
	public event SqlRowUpdatedEventHandler RowUpdated
	{
		add
		{
			base.Events.AddHandler(EventRowUpdated, value);
		}
		remove
		{
			base.Events.RemoveHandler(EventRowUpdated, value);
		}
	}

	/// <summary>Occurs during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> before a command is executed against the data source. The attempt to update is made, so the event fires.</summary>
	/// <filterpriority>2</filterpriority>
	public event SqlRowUpdatingEventHandler RowUpdating
	{
		add
		{
			SqlRowUpdatingEventHandler sqlRowUpdatingEventHandler = (SqlRowUpdatingEventHandler)base.Events[EventRowUpdating];
			if (sqlRowUpdatingEventHandler != null && value.Target is DbCommandBuilder)
			{
				SqlRowUpdatingEventHandler sqlRowUpdatingEventHandler2 = (SqlRowUpdatingEventHandler)ADP.FindBuilder(sqlRowUpdatingEventHandler);
				if (sqlRowUpdatingEventHandler2 != null)
				{
					base.Events.RemoveHandler(EventRowUpdating, sqlRowUpdatingEventHandler2);
				}
			}
			base.Events.AddHandler(EventRowUpdating, value);
		}
		remove
		{
			base.Events.RemoveHandler(EventRowUpdating, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> class.</summary>
	public SqlDataAdapter()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> class with the specified <see cref="T:System.Data.SqlClient.SqlCommand" /> as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property.</summary>
	/// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />. </param>
	public SqlDataAdapter(SqlCommand selectCommand)
		: this()
	{
		SelectCommand = selectCommand;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> class with a <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> and a connection string.</summary>
	/// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />. </param>
	/// <param name="selectConnectionString">The connection string. If your connection string does not use Integrated Security = true, you can use <see cref="M:System.Data.SqlClient.SqlDataAdapter.#ctor(System.String,System.Data.SqlClient.SqlConnection)" /> and <see cref="T:System.Data.SqlClient.SqlCredential" /> to pass the user ID and password more securely than by specifying the user ID and password as text in the connection string.</param>
	public SqlDataAdapter(string selectCommandText, string selectConnectionString)
		: this()
	{
		SqlConnection connection = new SqlConnection(selectConnectionString);
		SelectCommand = new SqlCommand(selectCommandText, connection);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" /> class with a <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> and a <see cref="T:System.Data.SqlClient.SqlConnection" /> object.</summary>
	/// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />. </param>
	/// <param name="selectConnection">A <see cref="T:System.Data.SqlClient.SqlConnection" /> that represents the connection. If your connection string does not use Integrated Security = true, you can use <see cref="T:System.Data.SqlClient.SqlCredential" /> to pass the user ID and password more securely than by specifying the user ID and password as text in the connection string.</param>
	public SqlDataAdapter(string selectCommandText, SqlConnection selectConnection)
		: this()
	{
		SelectCommand = new SqlCommand(selectCommandText, selectConnection);
	}

	private SqlDataAdapter(SqlDataAdapter from)
		: base(from)
	{
		GC.SuppressFinalize(this);
	}

	protected override int AddToBatch(IDbCommand command)
	{
		int commandCount = _commandSet.CommandCount;
		_commandSet.Append((SqlCommand)command);
		return commandCount;
	}

	protected override void ClearBatch()
	{
		_commandSet.Clear();
	}

	protected override int ExecuteBatch()
	{
		return _commandSet.ExecuteNonQuery();
	}

	protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
	{
		return _commandSet.GetParameter(commandIdentifier, parameterIndex);
	}

	protected override bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error)
	{
		return _commandSet.GetBatchedAffected(commandIdentifier, out recordsAffected, out error);
	}

	protected override void InitializeBatching()
	{
		_commandSet = new SqlCommandSet();
		SqlCommand sqlCommand = SelectCommand;
		if (sqlCommand == null)
		{
			sqlCommand = InsertCommand;
			if (sqlCommand == null)
			{
				sqlCommand = UpdateCommand;
				if (sqlCommand == null)
				{
					sqlCommand = DeleteCommand;
				}
			}
		}
		if (sqlCommand != null)
		{
			_commandSet.Connection = sqlCommand.Connection;
			_commandSet.Transaction = sqlCommand.Transaction;
			_commandSet.CommandTimeout = sqlCommand.CommandTimeout;
		}
	}

	protected override void TerminateBatching()
	{
		if (_commandSet != null)
		{
			_commandSet.Dispose();
			_commandSet = null;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.ICloneable.Clone" />.</summary>
	/// <returns>A new object that is a copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new SqlDataAdapter(this);
	}

	protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
	{
		return new SqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
	}

	protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
	{
		return new SqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
	}

	protected override void OnRowUpdated(RowUpdatedEventArgs value)
	{
		SqlRowUpdatedEventHandler sqlRowUpdatedEventHandler = (SqlRowUpdatedEventHandler)base.Events[EventRowUpdated];
		if (sqlRowUpdatedEventHandler != null && value is SqlRowUpdatedEventArgs)
		{
			sqlRowUpdatedEventHandler(this, (SqlRowUpdatedEventArgs)value);
		}
		base.OnRowUpdated(value);
	}

	protected override void OnRowUpdating(RowUpdatingEventArgs value)
	{
		SqlRowUpdatingEventHandler sqlRowUpdatingEventHandler = (SqlRowUpdatingEventHandler)base.Events[EventRowUpdating];
		if (sqlRowUpdatingEventHandler != null && value is SqlRowUpdatingEventArgs)
		{
			sqlRowUpdatingEventHandler(this, (SqlRowUpdatingEventArgs)value);
		}
		base.OnRowUpdating(value);
	}
}
