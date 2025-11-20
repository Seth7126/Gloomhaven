using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables you to pass context properties from the COM Transaction Integrator (COMTI) into the COM+ context.</summary>
[ComVisible(false)]
[AttributeUsage(AttributeTargets.Class)]
public sealed class COMTIIntrinsicsAttribute : Attribute
{
	private bool val;

	/// <summary>Gets a value indicating whether the COM Transaction Integrator (COMTI) context properties are passed into the COM+ context.</summary>
	/// <returns>true if the COMTI context properties are passed into the COM+ context; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.COMTIIntrinsicsAttribute" /> class, setting the <see cref="P:System.EnterpriseServices.COMTIIntrinsicsAttribute.Value" /> property to true.</summary>
	public COMTIIntrinsicsAttribute()
	{
		val = false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.COMTIIntrinsicsAttribute" /> class, enabling the setting of the <see cref="P:System.EnterpriseServices.COMTIIntrinsicsAttribute.Value" /> property.</summary>
	/// <param name="val">true if the COMTI context properties are passed into the COM+ context; otherwise, false. </param>
	public COMTIIntrinsicsAttribute(bool val)
	{
		this.val = val;
	}
}
