namespace System.Data;

/// <summary>Describes the version of data in a <see cref="T:System.Data.DataRow" />.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum DataViewRowState
{
	/// <summary>None.</summary>
	None = 0,
	/// <summary>An unchanged row.</summary>
	Unchanged = 2,
	/// <summary>A new row.</summary>
	Added = 4,
	/// <summary>A deleted row.</summary>
	Deleted = 8,
	/// <summary>A current version of original data that has been modified (see ModifiedOriginal).</summary>
	ModifiedCurrent = 0x10,
	/// <summary>The original version of the data that was modified. (Although the data has since been modified, it is available as ModifiedCurrent).</summary>
	ModifiedOriginal = 0x20,
	/// <summary>Original rows including unchanged and deleted rows.</summary>
	OriginalRows = 0x2A,
	/// <summary>Current rows including unchanged, new, and modified rows. By default, <see cref="T:System.Data.DataViewRowState" /> is set to CurrentRows.</summary>
	CurrentRows = 0x16
}
