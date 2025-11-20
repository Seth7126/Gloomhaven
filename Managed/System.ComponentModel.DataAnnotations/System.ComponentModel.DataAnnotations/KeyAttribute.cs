namespace System.ComponentModel.DataAnnotations;

/// <summary>Denotes one or more properties that uniquely identify an entity.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class KeyAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.KeyAttribute" /> class.</summary>
	public KeyAttribute()
	{
	}
}
