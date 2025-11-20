namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the data type of the column as a row version.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class TimestampAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.TimestampAttribute" /> class.</summary>
	public TimestampAttribute()
	{
	}
}
