using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables security checking on calls to a component. This class cannot be inherited.</summary>
[ComVisible(false)]
[AttributeUsage(AttributeTargets.Class)]
public sealed class ComponentAccessControlAttribute : Attribute
{
	private bool val;

	/// <summary>Gets a value indicating whether to enable security checking on calls to a component.</summary>
	/// <returns>true if security checking is to be enabled; otherwise, false.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ComponentAccessControlAttribute" /> class.</summary>
	public ComponentAccessControlAttribute()
	{
		val = false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ComponentAccessControlAttribute" /> class and sets the <see cref="P:System.EnterpriseServices.ComponentAccessControlAttribute.Value" /> property to indicate whether to enable COM+ security configuration.</summary>
	/// <param name="val">true to enable security checking on calls to a component; otherwise, false. </param>
	public ComponentAccessControlAttribute(bool val)
	{
		this.val = val;
	}
}
