namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies that a property participates in optimistic concurrency checks.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class ConcurrencyCheckAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ConcurrencyCheckAttribute" /> class.</summary>
	public ConcurrencyCheckAttribute()
	{
	}
}
