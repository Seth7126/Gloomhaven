namespace System.Security.Policy;

/// <summary>Specifies how to match versions when locating application trusts in a collection.</summary>
public enum ApplicationVersionMatch
{
	/// <summary>Match on all versions.</summary>
	MatchAllVersions = 1,
	/// <summary>Match on the exact version.</summary>
	MatchExactVersion = 0
}
