namespace System.EnterpriseServices;

/// <summary>Specifies the level of access checking for an application, either at the process level only or at all levels, including component, interface, and method levels.</summary>
[Serializable]
public enum AccessChecksLevelOption
{
	/// <summary>Enables access checks only at the process level. No access checks are made at the component, interface, or method level.</summary>
	Application,
	/// <summary>Enables access checks at every level on calls into the application.</summary>
	ApplicationComponent
}
