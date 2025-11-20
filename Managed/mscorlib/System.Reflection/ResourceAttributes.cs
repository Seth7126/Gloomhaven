namespace System.Reflection;

/// <summary>Specifies the attributes for a manifest resource.</summary>
[Flags]
public enum ResourceAttributes
{
	/// <summary>A mask used to retrieve public manifest resources.</summary>
	Public = 1,
	/// <summary>A mask used to retrieve private manifest resources.</summary>
	Private = 2
}
