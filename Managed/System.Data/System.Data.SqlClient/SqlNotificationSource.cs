namespace System.Data.SqlClient;

/// <summary>Indicates the source of the notification received by the dependency event handler.</summary>
/// <filterpriority>2</filterpriority>
public enum SqlNotificationSource
{
	/// <summary>Data has changed; for example, an insert, update, delete, or truncate operation occurred.</summary>
	Data = 0,
	/// <summary>The subscription time-out expired.</summary>
	Timeout = 1,
	/// <summary>A database object changed; for example, an underlying object related to the query was dropped or modified.</summary>
	Object = 2,
	/// <summary>The database state changed; for example, the database related to the query was dropped or detached.</summary>
	Database = 3,
	/// <summary>A system-related event occurred. For example, there was an internal error, the server was restarted, or resource pressure caused the invalidation.</summary>
	System = 4,
	/// <summary>The Transact-SQL statement is not valid for notifications; for example, a SELECT statement that could not be notified or a non-SELECT statement was executed.</summary>
	Statement = 5,
	/// <summary>The run-time environment was not compatible with notifications; for example, the isolation level was set to snapshot, or one or more SET options are not compatible.</summary>
	Environment = 6,
	/// <summary>A run-time error occurred during execution.</summary>
	Execution = 7,
	/// <summary>Internal only; not intended to be used in your code.</summary>
	Owner = 8,
	/// <summary>Used when the source option sent by the server was not recognized by the client. </summary>
	Unknown = -1,
	/// <summary>A client-initiated notification occurred, such as a client-side time-out or as a result of attempting to add a command to a dependency that has already fired.</summary>
	Client = -2
}
