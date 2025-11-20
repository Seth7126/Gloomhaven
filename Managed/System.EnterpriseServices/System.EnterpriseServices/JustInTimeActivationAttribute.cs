using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Turns just-in-time (JIT) activation on or off. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class JustInTimeActivationAttribute : Attribute
{
	private bool val;

	/// <summary>Gets the value of the <see cref="T:System.EnterpriseServices.JustInTimeActivationAttribute" /> setting.</summary>
	/// <returns>true if JIT activation is enabled; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.JustInTimeActivationAttribute" /> class. The default constructor enables just-in-time (JIT) activation.</summary>
	public JustInTimeActivationAttribute()
		: this(val: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.JustInTimeActivationAttribute" /> class, optionally allowing the disabling of just-in-time (JIT) activation by passing false as the parameter.</summary>
	/// <param name="val">true to enable JIT activation; otherwise, false. </param>
	public JustInTimeActivationAttribute(bool val)
	{
		this.val = val;
	}
}
