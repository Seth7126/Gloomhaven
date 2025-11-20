namespace System.Data;

/// <summary>Specifies how a command string is interpreted.</summary>
/// <filterpriority>2</filterpriority>
public enum CommandType
{
	/// <summary>An SQL text command. (Default.) </summary>
	Text = 1,
	/// <summary>The name of a stored procedure.</summary>
	StoredProcedure = 4,
	/// <summary>The name of a table.</summary>
	TableDirect = 0x200
}
