namespace System.Data.Common;

/// <summary>Specifies how identifiers are treated by the data source when searching the system catalog.</summary>
/// <filterpriority>2</filterpriority>
public enum IdentifierCase
{
	/// <summary>The data source has ambiguous rules regarding identifier case and cannot discern this information.</summary>
	Unknown,
	/// <summary>The data source ignores identifier case when searching the system catalog. The identifiers "ab" and "AB" will match.</summary>
	Insensitive,
	/// <summary>The data source distinguishes identifier case when searching the system catalog. The identifiers "ab" and "AB" will not match.</summary>
	Sensitive
}
