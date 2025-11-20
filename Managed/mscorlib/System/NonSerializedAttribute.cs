namespace System;

/// <summary>Indicates that a field of a serializable class should not be serialized. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class NonSerializedAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.NonSerializedAttribute" /> class.</summary>
	public NonSerializedAttribute()
	{
	}
}
