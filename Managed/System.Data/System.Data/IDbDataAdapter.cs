namespace System.Data;

/// <summary>Represents a set of command-related properties that are used to fill the <see cref="T:System.Data.DataSet" /> and update a data source, and is implemented by .NET Framework data providers that access relational databases.</summary>
/// <filterpriority>2</filterpriority>
public interface IDbDataAdapter : IDataAdapter
{
	/// <summary>Gets or sets an SQL statement used to select records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> that is used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to select records from data source for placement in the data set.</returns>
	/// <filterpriority>2</filterpriority>
	IDbCommand SelectCommand { get; set; }

	/// <summary>Gets or sets an SQL statement used to insert new records into the data source.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to insert records in the data source for new rows in the data set.</returns>
	/// <filterpriority>2</filterpriority>
	IDbCommand InsertCommand { get; set; }

	/// <summary>Gets or sets an SQL statement used to update records in the data source.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to update records in the data source for modified rows in the data set.</returns>
	/// <filterpriority>2</filterpriority>
	IDbCommand UpdateCommand { get; set; }

	/// <summary>Gets or sets an SQL statement for deleting records from the data set.</summary>
	/// <returns>An <see cref="T:System.Data.IDbCommand" /> used during <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" /> to delete records in the data source for deleted rows in the data set.</returns>
	/// <filterpriority>2</filterpriority>
	IDbCommand DeleteCommand { get; set; }
}
