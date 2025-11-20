namespace System.Data;

/// <summary>Represents an open connection to a data source, and is implemented by .NET Framework data providers that access relational databases.</summary>
/// <filterpriority>2</filterpriority>
public interface IDbConnection : IDisposable
{
	/// <summary>Gets or sets the string used to open a database.</summary>
	/// <returns>A string containing connection settings.</returns>
	/// <filterpriority>2</filterpriority>
	string ConnectionString { get; set; }

	/// <summary>Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.</summary>
	/// <returns>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</returns>
	/// <filterpriority>2</filterpriority>
	int ConnectionTimeout { get; }

	/// <summary>Gets the name of the current database or the database to be used after a connection is opened.</summary>
	/// <returns>The name of the current database or the name of the database to be used once a connection is open. The default value is an empty string.</returns>
	/// <filterpriority>2</filterpriority>
	string Database { get; }

	/// <summary>Gets the current state of the connection.</summary>
	/// <returns>One of the <see cref="T:System.Data.ConnectionState" /> values.</returns>
	/// <filterpriority>2</filterpriority>
	ConnectionState State { get; }

	/// <summary>Begins a database transaction.</summary>
	/// <returns>An object representing the new transaction.</returns>
	/// <filterpriority>2</filterpriority>
	IDbTransaction BeginTransaction();

	/// <summary>Begins a database transaction with the specified <see cref="T:System.Data.IsolationLevel" /> value.</summary>
	/// <returns>An object representing the new transaction.</returns>
	/// <param name="il">One of the <see cref="T:System.Data.IsolationLevel" /> values. </param>
	/// <filterpriority>2</filterpriority>
	IDbTransaction BeginTransaction(IsolationLevel il);

	/// <summary>Closes the connection to the database.</summary>
	/// <filterpriority>2</filterpriority>
	void Close();

	/// <summary>Changes the current database for an open Connection object.</summary>
	/// <param name="databaseName">The name of the database to use in place of the current database. </param>
	/// <filterpriority>2</filterpriority>
	void ChangeDatabase(string databaseName);

	/// <summary>Creates and returns a Command object associated with the connection.</summary>
	/// <returns>A Command object associated with the connection.</returns>
	/// <filterpriority>2</filterpriority>
	IDbCommand CreateCommand();

	/// <summary>Opens a database connection with the settings specified by the ConnectionString property of the provider-specific Connection object.</summary>
	/// <filterpriority>2</filterpriority>
	void Open();
}
