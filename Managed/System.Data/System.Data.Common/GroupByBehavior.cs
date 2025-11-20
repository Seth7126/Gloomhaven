namespace System.Data.Common;

/// <summary>Specifies the relationship between the columns in a GROUP BY clause and the non-aggregated columns in the select-list of a SELECT statement.</summary>
/// <filterpriority>2</filterpriority>
public enum GroupByBehavior
{
	/// <summary>The support for the GROUP BY clause is unknown.</summary>
	Unknown,
	/// <summary>The GROUP BY clause is not supported.</summary>
	NotSupported,
	/// <summary>There is no relationship between the columns in the GROUP BY clause and the nonaggregated columns in the SELECT list. You may group by any column.</summary>
	Unrelated,
	/// <summary>The GROUP BY clause must contain all nonaggregated columns in the select list, and can contain other columns not in the select list.</summary>
	MustContainAll,
	/// <summary>The GROUP BY clause must contain all nonaggregated columns in the select list, and must not contain other columns not in the select list.</summary>
	ExactMatch
}
