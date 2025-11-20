using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables queuing support for the marked interface. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
[ComVisible(false)]
public sealed class InterfaceQueuingAttribute : Attribute
{
	private bool enabled;

	private string interfaceName;

	/// <summary>Gets or sets a value indicating whether queuing support is enabled.</summary>
	/// <returns>true if queuing support is enabled; otherwise, false. The default is true.</returns>
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	/// <summary>Gets or sets the name of the interface on which queuing is enabled.</summary>
	/// <returns>The name of the interface on which queuing is enabled.</returns>
	public string Interface
	{
		get
		{
			return interfaceName;
		}
		set
		{
			interfaceName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.InterfaceQueuingAttribute" /> class setting the <see cref="P:System.EnterpriseServices.InterfaceQueuingAttribute.Enabled" /> and <see cref="P:System.EnterpriseServices.InterfaceQueuingAttribute.Interface" /> properties to their default values.</summary>
	public InterfaceQueuingAttribute()
		: this(enabled: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.InterfaceQueuingAttribute" /> class, optionally disabling queuing support.</summary>
	/// <param name="enabled">true to enable queuing support; otherwise, false. </param>
	public InterfaceQueuingAttribute(bool enabled)
	{
		this.enabled = enabled;
		interfaceName = null;
	}
}
