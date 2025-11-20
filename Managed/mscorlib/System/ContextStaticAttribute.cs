using System.Runtime.InteropServices;

namespace System;

/// <summary>Indicates that the value of a static field is unique for a particular context.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
[ComVisible(true)]
public class ContextStaticAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ContextStaticAttribute" /> class.</summary>
	public ContextStaticAttribute()
	{
	}
}
