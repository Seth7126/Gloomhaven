using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Specifies the name of the COM+ application to be used for the install of the components in the assembly. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
[ComVisible(false)]
public sealed class ApplicationNameAttribute : Attribute, IConfigurationAttribute
{
	private string name;

	/// <summary>Gets a value indicating the name of the COM+ application that contains the components in the assembly.</summary>
	/// <returns>The name of the COM+ application.</returns>
	public string Value => name;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ApplicationNameAttribute" /> class, specifying the name of the COM+ application to be used for the install of the components.</summary>
	/// <param name="name">The name of the COM+ application. </param>
	public ApplicationNameAttribute(string name)
	{
		this.name = name;
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
