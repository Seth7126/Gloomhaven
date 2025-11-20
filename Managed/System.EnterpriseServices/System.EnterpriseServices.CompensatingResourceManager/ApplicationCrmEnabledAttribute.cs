using System.Runtime.InteropServices;

namespace System.EnterpriseServices.CompensatingResourceManager;

/// <summary>Enables Compensating Resource Manger (CRM) on the tagged application.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
[ComVisible(false)]
[ProgId("System.EnterpriseServices.Crm.ApplicationCrmEnabledAttribute")]
public sealed class ApplicationCrmEnabledAttribute : Attribute
{
	private bool val;

	/// <summary>Enables or disables Compensating Resource Manager (CRM) on the tagged application.</summary>
	/// <returns>true if CRM is enabled; otherwise, false. The default is true.</returns>
	public bool Value => val;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.ApplicationCrmEnabledAttribute" /> class, setting the <see cref="P:System.EnterpriseServices.CompensatingResourceManager.ApplicationCrmEnabledAttribute.Value" /> property to true.</summary>
	public ApplicationCrmEnabledAttribute()
	{
		val = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.ApplicationCrmEnabledAttribute" /> class, optionally setting the <see cref="P:System.EnterpriseServices.CompensatingResourceManager.ApplicationCrmEnabledAttribute.Value" /> property to false.</summary>
	/// <param name="val">true to enable Compensating Resource Manager (CRM); otherwise, false. </param>
	public ApplicationCrmEnabledAttribute(bool val)
	{
		this.val = val;
	}
}
