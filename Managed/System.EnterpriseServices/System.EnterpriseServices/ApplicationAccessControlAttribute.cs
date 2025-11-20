using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Specifies access controls to an assembly containing <see cref="T:System.EnterpriseServices.ServicedComponent" /> classes.</summary>
[ComVisible(false)]
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ApplicationAccessControlAttribute : Attribute, IConfigurationAttribute
{
	private AccessChecksLevelOption accessChecksLevel;

	private AuthenticationOption authentication;

	private ImpersonationLevelOption impersonation;

	private bool val;

	/// <summary>Gets or sets the access checking level to process level or to component level.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.AccessChecksLevelOption" /> values.</returns>
	public AccessChecksLevelOption AccessChecksLevel
	{
		get
		{
			return accessChecksLevel;
		}
		set
		{
			accessChecksLevel = value;
		}
	}

	/// <summary>Gets or sets the remote procedure call (RPC) authentication level.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.AuthenticationOption" /> values.</returns>
	public AuthenticationOption Authentication
	{
		get
		{
			return authentication;
		}
		set
		{
			authentication = value;
		}
	}

	/// <summary>Gets or sets the impersonation level that is allowed for calling targets of this application.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.ImpersonationLevelOption" /> values.</returns>
	public ImpersonationLevelOption ImpersonationLevel
	{
		get
		{
			return impersonation;
		}
		set
		{
			impersonation = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to enable COM+ security configuration.</summary>
	/// <returns>true if COM+ security configuration is enabled; otherwise, false.</returns>
	public bool Value
	{
		get
		{
			return val;
		}
		set
		{
			val = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ApplicationAccessControlAttribute" /> class, enabling the COM+ security configuration.</summary>
	public ApplicationAccessControlAttribute()
	{
		val = false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ApplicationAccessControlAttribute" /> class and sets the <see cref="P:System.EnterpriseServices.ApplicationAccessControlAttribute.Value" /> property indicating whether to enable COM security configuration.</summary>
	/// <param name="val">true to allow configuration of security; otherwise, false. </param>
	public ApplicationAccessControlAttribute(bool val)
	{
		this.val = val;
	}

	bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
	{
		return false;
	}

	[System.MonoTODO]
	bool IConfigurationAttribute.Apply(Hashtable cache)
	{
		throw new NotImplementedException();
	}

	bool IConfigurationAttribute.IsValidTarget(string s)
	{
		return s == "Application";
	}
}
