using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Identifies a component as a private component that is only seen and activated by components in the same application. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class PrivateComponentAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.PrivateComponentAttribute" /> class.</summary>
	public PrivateComponentAttribute()
	{
	}
}
