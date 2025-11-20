namespace System.Reflection;

/// <summary>Defines additional version information for an assembly manifest.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyInformationalVersionAttribute : Attribute
{
	/// <summary>Gets version information.</summary>
	/// <returns>A string containing the version information.</returns>
	public string InformationalVersion { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyInformationalVersionAttribute" /> class.</summary>
	/// <param name="informationalVersion">The assembly version information. </param>
	public AssemblyInformationalVersionAttribute(string informationalVersion)
	{
		InformationalVersion = informationalVersion;
	}
}
