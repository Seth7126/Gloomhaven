namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies whether a class or data table uses scaffolding.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ScaffoldTableAttribute : Attribute
{
	/// <summary>Gets or sets the value that specifies whether scaffolding is enabled.</summary>
	/// <returns>true, if scaffolding is enabled; otherwise false.</returns>
	public bool Scaffold { get; private set; }

	/// <summary>Initializes a new instance of <see cref="T:System.ComponentModel.DataAnnotations.ScaffoldTableAttribute" /> using the <see cref="P:System.ComponentModel.DataAnnotations.ScaffoldTableAttribute.Scaffold" /> property.</summary>
	/// <param name="scaffold">The value that specifies whether scaffolding is enabled.</param>
	public ScaffoldTableAttribute(bool scaffold)
	{
		Scaffold = scaffold;
	}
}
