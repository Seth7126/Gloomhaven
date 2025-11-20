namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies whether a class or data column uses scaffolding.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ScaffoldColumnAttribute : Attribute
{
	/// <summary>Gets or sets the value that specifies whether scaffolding is enabled.</summary>
	/// <returns>true, if scaffolding is enabled; otherwise false.</returns>
	public bool Scaffold { get; private set; }

	/// <summary>Initializes a new instance of <see cref="T:System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute" /> using the <see cref="P:System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute.Scaffold" /> property.</summary>
	/// <param name="scaffold">The value that specifies whether scaffolding is enabled.</param>
	public ScaffoldColumnAttribute(bool scaffold)
	{
		Scaffold = scaffold;
	}
}
