using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Marks the attributed method as an AutoComplete object. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Method)]
[ComVisible(false)]
public sealed class AutoCompleteAttribute : Attribute
{
	private bool val;

	/// <summary>Gets a value indicating the setting of the AutoComplete option in COM+.</summary>
	/// <returns>true if AutoComplete is enabled in COM+; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.AutoCompleteAttribute" /> class, specifying that the application should automatically call <see cref="M:System.EnterpriseServices.ContextUtil.SetComplete" /> if the transaction completes successfully.</summary>
	public AutoCompleteAttribute()
	{
		val = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.AutoCompleteAttribute" /> class, specifying whether COM+ AutoComplete is enabled.</summary>
	/// <param name="val">true to enable AutoComplete in the COM+ object; otherwise, false. </param>
	public AutoCompleteAttribute(bool val)
	{
		this.val = val;
	}
}
