namespace System.Runtime.CompilerServices;

/// <summary>Defines a constant value that a compiler can persist for a field or method parameter.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
public abstract class CustomConstantAttribute : Attribute
{
	/// <summary>Gets the constant value stored by this attribute.</summary>
	/// <returns>The constant value stored by this attribute.</returns>
	public abstract object Value { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CustomConstantAttribute" /> class. </summary>
	protected CustomConstantAttribute()
	{
	}
}
