using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables access to ASP intrinsic values from <see cref="M:System.EnterpriseServices.ContextUtil.GetNamedProperty(System.String)" />. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class IISIntrinsicsAttribute : Attribute
{
	private bool val;

	/// <summary>Gets a value that indicates whether access to the ASP intrinsic values is enabled.</summary>
	/// <returns>true if access is enabled; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.IISIntrinsicsAttribute" /> class, enabling access to the ASP intrinsic values.</summary>
	public IISIntrinsicsAttribute()
	{
		val = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.IISIntrinsicsAttribute" /> class, optionally disabling access to the ASP intrinsic values.</summary>
	/// <param name="val">true to enable access to the ASP intrinsic values; otherwise, false. </param>
	public IISIntrinsicsAttribute(bool val)
	{
		this.val = val;
	}
}
