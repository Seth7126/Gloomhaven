namespace System.Data;

/// <summary>Specifies the type of SQL query to be used by the <see cref="T:System.Data.OleDb.OleDbRowUpdatedEventArgs" />, <see cref="T:System.Data.OleDb.OleDbRowUpdatingEventArgs" />, <see cref="T:System.Data.SqlClient.SqlRowUpdatedEventArgs" />, or <see cref="T:System.Data.SqlClient.SqlRowUpdatingEventArgs" /> class.</summary>
/// <filterpriority>2</filterpriority>
public enum StatementType
{
	/// <summary>An SQL query that is a SELECT statement.</summary>
	Select,
	/// <summary>An SQL query that is an INSERT statement.</summary>
	Insert,
	/// <summary>An SQL query that is an UPDATE statement.</summary>
	Update,
	/// <summary>An SQL query that is a DELETE statement.</summary>
	Delete,
	/// <summary>A SQL query that is a batch statement.</summary>
	Batch
}
