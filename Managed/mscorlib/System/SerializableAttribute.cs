namespace System;

/// <summary>Indicates that a class can be serialized. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
public sealed class SerializableAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.SerializableAttribute" /> class.</summary>
	public SerializableAttribute()
	{
	}
}
