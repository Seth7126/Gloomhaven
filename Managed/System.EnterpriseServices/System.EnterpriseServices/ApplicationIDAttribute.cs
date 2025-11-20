using System.Collections;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Specifies the application ID (as a GUID) for this assembly. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
[ComVisible(false)]
public sealed class ApplicationIDAttribute : Attribute, IConfigurationAttribute
{
	private Guid guid;

	/// <summary>Gets the GUID of the COM+ application.</summary>
	/// <returns>The GUID representing the COM+ application.</returns>
	public Guid Value => guid;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ApplicationIDAttribute" /> class specifying the GUID representing the application ID for the COM+ application.</summary>
	/// <param name="guid">The GUID associated with the COM+ application. </param>
	public ApplicationIDAttribute(string guid)
	{
		this.guid = new Guid(guid);
	}

	bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
	{
		return false;
	}

	bool IConfigurationAttribute.Apply(Hashtable cache)
	{
		return false;
	}

	bool IConfigurationAttribute.IsValidTarget(string s)
	{
		return s == "Application";
	}
}
