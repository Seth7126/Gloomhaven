namespace System.Data;

/// <summary>Indicates the action that occurs when a <see cref="T:System.Data.ForeignKeyConstraint" /> is enforced.</summary>
/// <filterpriority>2</filterpriority>
public enum Rule
{
	/// <summary>No action taken on related rows.</summary>
	None,
	/// <summary>Delete or update related rows. This is the default.</summary>
	Cascade,
	/// <summary>Set values in related rows to DBNull.</summary>
	SetNull,
	/// <summary>Set values in related rows to the value contained in the <see cref="P:System.Data.DataColumn.DefaultValue" /> property.</summary>
	SetDefault
}
