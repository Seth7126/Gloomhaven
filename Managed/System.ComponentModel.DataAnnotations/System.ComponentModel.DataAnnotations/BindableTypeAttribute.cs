namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies whether a type is typically used for binding.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
public sealed class BindableTypeAttribute : Attribute
{
	/// <summary>Gets a value indicating that a type is typically used for binding.</summary>
	/// <returns>true if the property is typically used for binding; otherwise, false.</returns>
	public bool IsBindable { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.BindableTypeAttribute" /> class.</summary>
	public BindableTypeAttribute()
	{
		IsBindable = true;
	}
}
