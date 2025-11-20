namespace System.Data;

/// <summary>Gets the state of a <see cref="T:System.Data.DataRow" /> object.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum DataRowState
{
	/// <summary>The row has been created but is not part of any <see cref="T:System.Data.DataRowCollection" />. A <see cref="T:System.Data.DataRow" /> is in this state immediately after it has been created and before it is added to a collection, or if it has been removed from a collection.</summary>
	Detached = 1,
	/// <summary>The row has not changed since <see cref="M:System.Data.DataRow.AcceptChanges" /> was last called.</summary>
	Unchanged = 2,
	/// <summary>The row has been added to a <see cref="T:System.Data.DataRowCollection" />, and <see cref="M:System.Data.DataRow.AcceptChanges" /> has not been called.</summary>
	Added = 4,
	/// <summary>The row was deleted using the <see cref="M:System.Data.DataRow.Delete" /> method of the <see cref="T:System.Data.DataRow" />.</summary>
	Deleted = 8,
	/// <summary>The row has been modified and <see cref="M:System.Data.DataRow.AcceptChanges" /> has not been called.</summary>
	Modified = 0x10
}
