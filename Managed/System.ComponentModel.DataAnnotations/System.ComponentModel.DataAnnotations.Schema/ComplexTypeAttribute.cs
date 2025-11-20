namespace System.ComponentModel.DataAnnotations.Schema;

/// <summary>Denotes that the class is a complex type.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ComplexTypeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.ComplexTypeAttribute" /> class.</summary>
	public ComplexTypeAttribute()
	{
	}
}
