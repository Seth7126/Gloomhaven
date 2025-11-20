using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Sets the synchronization value of the component. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class SynchronizationAttribute : Attribute
{
	private SynchronizationOption val;

	/// <summary>Gets the current setting of the <see cref="P:System.EnterpriseServices.SynchronizationAttribute.Value" /> property.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.SynchronizationOption" /> values. The default is Required.</returns>
	public SynchronizationOption Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.SynchronizationAttribute" /> class with the default <see cref="T:System.EnterpriseServices.SynchronizationOption" />.</summary>
	public SynchronizationAttribute()
		: this(SynchronizationOption.Required)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.SynchronizationAttribute" /> class with the specified <see cref="T:System.EnterpriseServices.SynchronizationOption" />.</summary>
	/// <param name="val">One of the <see cref="T:System.EnterpriseServices.SynchronizationOption" /> values. </param>
	public SynchronizationAttribute(SynchronizationOption val)
	{
		this.val = val;
	}
}
