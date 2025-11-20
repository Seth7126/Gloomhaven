namespace System.Data;

/// <summary>Represents a transaction to be performed at a data source, and is implemented by .NET Framework data providers that access relational databases.</summary>
/// <filterpriority>2</filterpriority>
public interface IDbTransaction : IDisposable
{
	/// <summary>Specifies the Connection object to associate with the transaction.</summary>
	/// <returns>The Connection object to associate with the transaction.</returns>
	/// <filterpriority>2</filterpriority>
	IDbConnection Connection { get; }

	/// <summary>Specifies the <see cref="T:System.Data.IsolationLevel" /> for this transaction.</summary>
	/// <returns>The <see cref="T:System.Data.IsolationLevel" /> for this transaction. The default is ReadCommitted.</returns>
	/// <filterpriority>2</filterpriority>
	IsolationLevel IsolationLevel { get; }

	/// <summary>Commits the database transaction.</summary>
	/// <exception cref="T:System.Exception">An error occurred while trying to commit the transaction. </exception>
	/// <exception cref="T:System.InvalidOperationException">The transaction has already been committed or rolled back.-or- The connection is broken. </exception>
	/// <filterpriority>2</filterpriority>
	void Commit();

	/// <summary>Rolls back a transaction from a pending state.</summary>
	/// <exception cref="T:System.Exception">An error occurred while trying to commit the transaction. </exception>
	/// <exception cref="T:System.InvalidOperationException">The transaction has already been committed or rolled back.-or- The connection is broken. </exception>
	/// <filterpriority>2</filterpriority>
	void Rollback();
}
