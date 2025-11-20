using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Determines whether the component participates in load balancing, if the component load balancing service is installed and enabled on the server.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class LoadBalancingSupportedAttribute : Attribute
{
	private bool val;

	/// <summary>Gets a value that indicates whether load balancing support is enabled.</summary>
	/// <returns>true if load balancing support is enabled; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.LoadBalancingSupportedAttribute" /> class, specifying load balancing support.</summary>
	public LoadBalancingSupportedAttribute()
		: this(val: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.LoadBalancingSupportedAttribute" /> class, optionally disabling load balancing support.</summary>
	/// <param name="val">true to enable load balancing support; otherwise, false. </param>
	public LoadBalancingSupportedAttribute(bool val)
	{
		this.val = val;
	}
}
