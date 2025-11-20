namespace System.EnterpriseServices.CompensatingResourceManager;

/// <summary>Specifies flags that control which phases of transaction completion should be received by the Compensating Resource Manager (CRM) Compensator, and whether recovery should fail if questionable transactions remain after recovery has been attempted.</summary>
[Serializable]
[Flags]
public enum CompensatorOptions
{
	/// <summary>Represents the prepare phase.</summary>
	PreparePhase = 1,
	/// <summary>Represents the commit phase.</summary>
	CommitPhase = 2,
	/// <summary>Represents the abort phase.</summary>
	AbortPhase = 4,
	/// <summary>Represents all phases.</summary>
	AllPhases = 7,
	/// <summary>Fails if in-doubt transactions remain after recovery has been attempted.</summary>
	FailIfInDoubtsRemain = 0x10
}
