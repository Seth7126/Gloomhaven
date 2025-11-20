namespace System.Runtime.CompilerServices;

/// <summary>Specifies the name of the property that accesses the attributed field.</summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AccessedThroughPropertyAttribute : Attribute
{
	/// <summary>Gets the name of the property used to access the attributed field.</summary>
	/// <returns>The name of the property used to access the attributed field.</returns>
	public string PropertyName { get; }

	/// <summary>Initializes a new instance of the AccessedThroughPropertyAttribute class with the name of the property used to access the attributed field.</summary>
	/// <param name="propertyName">The name of the property used to access the attributed field. </param>
	public AccessedThroughPropertyAttribute(string propertyName)
	{
		PropertyName = propertyName;
	}
}
