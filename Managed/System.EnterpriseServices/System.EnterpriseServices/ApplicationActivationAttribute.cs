using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Specifies whether components in the assembly run in the creator's process or in a system process.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
[ComVisible(false)]
public sealed class ApplicationActivationAttribute : Attribute, IConfigurationAttribute
{
	private ActivationOption opt;

	private string soapMailbox;

	private string soapVRoot;

	/// <summary>This property is not supported in the current version.</summary>
	/// <returns>This property is not supported in the current version.</returns>
	public string SoapMailbox
	{
		get
		{
			return soapMailbox;
		}
		set
		{
			soapMailbox = value;
		}
	}

	/// <summary>Gets or sets a value representing a virtual root on the Web for the COM+ application.</summary>
	/// <returns>The virtual root on the Web for the COM+ application.</returns>
	public string SoapVRoot
	{
		get
		{
			return soapVRoot;
		}
		set
		{
			soapVRoot = value;
		}
	}

	/// <summary>Gets the specified <see cref="T:System.EnterpriseServices.ActivationOption" /> value.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.ActivationOption" /> values, either Library or Server.</returns>
	public ActivationOption Value => opt;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ApplicationActivationAttribute" /> class, setting the specified <see cref="T:System.EnterpriseServices.ActivationOption" /> value.</summary>
	/// <param name="opt">One of the <see cref="T:System.EnterpriseServices.ActivationOption" /> values. </param>
	public ApplicationActivationAttribute(ActivationOption opt)
	{
		this.opt = opt;
	}

	[System.MonoTODO]
	bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
	{
		throw new NotImplementedException();
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
