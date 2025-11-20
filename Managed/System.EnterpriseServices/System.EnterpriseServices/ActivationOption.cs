namespace System.EnterpriseServices;

/// <summary>Specifies the manner in which serviced components are activated in the application.</summary>
[Serializable]
public enum ActivationOption
{
	/// <summary>Specifies that serviced components in the marked application are activated in the creator's process.</summary>
	Library,
	/// <summary>Specifies that serviced components in the marked application are activated in a system-provided process.</summary>
	Server
}
