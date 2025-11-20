namespace System.Data.Common;

/// <summary>Specifies what types of Transact-SQL join statements are supported by the data source.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum SupportedJoinOperators
{
	/// <summary>The data source does not support join queries.</summary>
	None = 0,
	/// <summary>The data source supports inner joins.</summary>
	Inner = 1,
	/// <summary>The data source supports left outer joins.</summary>
	LeftOuter = 2,
	/// <summary>The data source supports right outer joins.</summary>
	RightOuter = 4,
	/// <summary>The data source supports full outer joins.</summary>
	FullOuter = 8
}
