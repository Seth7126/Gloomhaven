namespace System.Data;

/// <summary>Describes the current state of the connection to a data source.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum ConnectionState
{
	/// <summary>The connection is closed.</summary>
	Closed = 0,
	/// <summary>The connection is open.</summary>
	Open = 1,
	/// <summary>The connection object is connecting to the data source.</summary>
	Connecting = 2,
	/// <summary>The connection object is executing a command. (This value is reserved for future versions of the product.) </summary>
	Executing = 4,
	/// <summary>The connection object is retrieving data. (This value is reserved for future versions of the product.) </summary>
	Fetching = 8,
	/// <summary>The connection to the data source is broken. This can occur only after the connection has been opened. A connection in this state may be closed and then re-opened. (This value is reserved for future versions of the product.) </summary>
	Broken = 0x10
}
