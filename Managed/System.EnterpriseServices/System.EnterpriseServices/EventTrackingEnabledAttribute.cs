using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables event tracking for a component. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class EventTrackingEnabledAttribute : Attribute
{
	private bool val;

	/// <summary>Gets the value of the <see cref="P:System.EnterpriseServices.EventTrackingEnabledAttribute.Value" /> property, which indicates whether tracking is enabled.</summary>
	/// <returns>true if tracking is enabled; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.EventTrackingEnabledAttribute" /> class, enabling event tracking.</summary>
	public EventTrackingEnabledAttribute()
	{
		val = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.EventTrackingEnabledAttribute" /> class, optionally disabling event tracking.</summary>
	/// <param name="val">true to enable event tracking; otherwise, false. </param>
	public EventTrackingEnabledAttribute(bool val)
	{
		this.val = val;
	}
}
