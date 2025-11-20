namespace System.Data;

/// <summary>Determines the action that occurs when a mapping is missing from a source table or a source column.</summary>
/// <filterpriority>2</filterpriority>
public enum MissingMappingAction
{
	/// <summary>The source column or source table is created and added to the <see cref="T:System.Data.DataSet" /> using its original name.</summary>
	Passthrough = 1,
	/// <summary>The column or table not having a mapping is ignored. Returns null.</summary>
	Ignore,
	/// <summary>An <see cref="T:System.InvalidOperationException" /> is generated if the specified column mapping is missing.</summary>
	Error
}
