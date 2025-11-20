namespace System.Data.SqlClient;

/// <summary>This enumeration provides additional information about the different notifications that can be received by the dependency event handler. </summary>
/// <filterpriority>2</filterpriority>
public enum SqlNotificationInfo
{
	/// <summary>One or more tables were truncated.</summary>
	Truncate = 0,
	/// <summary>Data was changed by an INSERT statement.</summary>
	Insert = 1,
	/// <summary>Data was changed by an UPDATE statement.</summary>
	Update = 2,
	/// <summary>Data was changed by a DELETE statement.</summary>
	Delete = 3,
	/// <summary>An underlying object related to the query was dropped.</summary>
	Drop = 4,
	/// <summary>An underlying server object related to the query was modified.</summary>
	Alter = 5,
	/// <summary>The server was restarted (notifications are sent during restart.).</summary>
	Restart = 6,
	/// <summary>An internal server error occurred.</summary>
	Error = 7,
	/// <summary>A SELECT statement that cannot be notified or was provided.</summary>
	Query = 8,
	/// <summary>A statement was provided that cannot be notified (for example, an UPDATE statement).</summary>
	Invalid = 9,
	/// <summary>The SET options were not set appropriately at subscription time.</summary>
	Options = 10,
	/// <summary>The statement was executed under an isolation mode that was not valid (for example, Snapshot).</summary>
	Isolation = 11,
	/// <summary>The SqlDependency object has expired.</summary>
	Expired = 12,
	/// <summary>Fires as a result of server resource pressure.</summary>
	Resource = 13,
	/// <summary>A previous statement has caused query notifications to fire under the current transaction.</summary>
	PreviousFire = 14,
	/// <summary>The subscribing query causes the number of templates on one of the target tables to exceed the maximum allowable limit.</summary>
	TemplateLimit = 15,
	/// <summary>Used to distinguish the server-side cause for a query notification firing.</summary>
	Merge = 16,
	/// <summary>Used when the info option sent by the server was not recognized by the client.</summary>
	Unknown = -1,
	/// <summary>The SqlDependency object already fired, and new commands cannot be added to it.</summary>
	AlreadyChanged = -2
}
