namespace System.EnterpriseServices;

/// <summary>Specifies the type of automatic synchronization requested by the component.</summary>
[Serializable]
public enum SynchronizationOption
{
	/// <summary>COM+ ignores the synchronization requirements of the component when determining context for the object.</summary>
	Disabled = 0,
	/// <summary>An object with this value never participates in synchronization, regardless of the status of its caller. This setting is only available for components that are non-transactional and do not use just-in-time (JIT) activation.</summary>
	NotSupported = 1,
	/// <summary>Ensures that all objects created from the component are synchronized.</summary>
	Required = 3,
	/// <summary>An object with this value must participate in a new synchronization where COM+ manages contexts and apartments on behalf of all components involved in the call.</summary>
	RequiresNew = 4,
	/// <summary>An object with this value participates in synchronization, if it exists.</summary>
	Supported = 2
}
