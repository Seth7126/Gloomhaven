namespace System.Data;

/// <summary>Describes the version of a <see cref="T:System.Data.DataRow" />.</summary>
/// <filterpriority>2</filterpriority>
public enum DataRowVersion
{
	/// <summary>The row contains its original values.</summary>
	Original = 256,
	/// <summary>The row contains current values.</summary>
	Current = 512,
	/// <summary>The row contains a proposed value.</summary>
	Proposed = 1024,
	/// <summary>The default version of <see cref="T:System.Data.DataRowState" />. For a DataRowState value of Added, Modified or Deleted, the default version is Current. For a <see cref="T:System.Data.DataRowState" /> value of Detached, the version is Proposed.</summary>
	Default = 1536
}
