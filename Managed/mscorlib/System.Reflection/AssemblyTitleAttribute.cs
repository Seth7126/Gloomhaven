namespace System.Reflection;

/// <summary>Specifies a description for an assembly.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyTitleAttribute : Attribute
{
	/// <summary>Gets assembly title information.</summary>
	/// <returns>The assembly title. </returns>
	public string Title { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyTitleAttribute" /> class.</summary>
	/// <param name="title">The assembly title. </param>
	public AssemblyTitleAttribute(string title)
	{
		Title = title;
	}
}
