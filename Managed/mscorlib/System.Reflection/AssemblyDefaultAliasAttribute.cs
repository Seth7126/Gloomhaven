namespace System.Reflection;

/// <summary>Defines a friendly default alias for an assembly manifest.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyDefaultAliasAttribute : Attribute
{
	/// <summary>Gets default alias information.</summary>
	/// <returns>A string containing the default alias information.</returns>
	public string DefaultAlias { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyDefaultAliasAttribute" /> class.</summary>
	/// <param name="defaultAlias">The assembly default alias information. </param>
	public AssemblyDefaultAliasAttribute(string defaultAlias)
	{
		DefaultAlias = defaultAlias;
	}
}
