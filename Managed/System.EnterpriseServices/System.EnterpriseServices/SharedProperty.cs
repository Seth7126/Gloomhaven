using System.Runtime.InteropServices;
using Unity;

namespace System.EnterpriseServices;

/// <summary>Accesses a shared property. This class cannot be inherited.</summary>
[ComVisible(false)]
public sealed class SharedProperty
{
	private ISharedProperty property;

	/// <summary>Gets or sets the value of the shared property.</summary>
	/// <returns>The value of the shared property.</returns>
	public object Value
	{
		get
		{
			return property.Value;
		}
		set
		{
			property.Value = value;
		}
	}

	internal SharedProperty(ISharedProperty property)
	{
		this.property = property;
	}

	internal SharedProperty()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
