namespace System.Data;

/// <summary>Describes an action performed on a <see cref="T:System.Data.DataRow" />.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum DataRowAction
{
	/// <summary>The row has not changed.</summary>
	Nothing = 0,
	/// <summary>The row was deleted from the table.</summary>
	Delete = 1,
	/// <summary>The row has changed.</summary>
	Change = 2,
	/// <summary>The most recent change to the row has been rolled back.</summary>
	Rollback = 4,
	/// <summary>The changes to the row have been committed.</summary>
	Commit = 8,
	/// <summary>The row has been added to the table.</summary>
	Add = 0x10,
	/// <summary>The original version of the row has been changed.</summary>
	ChangeOriginal = 0x20,
	/// <summary>The original and the current versions of the row have been changed.</summary>
	ChangeCurrentAndOriginal = 0x40
}
