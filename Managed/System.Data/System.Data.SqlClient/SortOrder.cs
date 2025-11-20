namespace System.Data.SqlClient;

/// <summary>Specifies how rows of data are sorted.</summary>
public enum SortOrder
{
	/// <summary>The default. No sort order is specified.</summary>
	Unspecified = -1,
	/// <summary>Rows are sorted in ascending order.</summary>
	Ascending,
	/// <summary>Rows are sorted in descending order.</summary>
	Descending
}
