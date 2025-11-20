namespace System.EnterpriseServices.CompensatingResourceManager;

/// <summary>Describes the origin of a Compensating Resource Manager (CRM) log record.</summary>
[Serializable]
[Flags]
public enum LogRecordFlags
{
	/// <summary>Indicates the delivered record should be forgotten.</summary>
	ForgetTarget = 1,
	/// <summary>Log record was written during prepare.</summary>
	WrittenDuringPrepare = 2,
	/// <summary>Log record was written during commit.</summary>
	WrittenDuringCommit = 4,
	/// <summary>Log record was written during abort.</summary>
	WrittenDuringAbort = 8,
	/// <summary>Log record was written during recovery.</summary>
	WrittenDurringRecovery = 0x10,
	/// <summary>Log record was written during replay.</summary>
	WrittenDuringReplay = 0x20,
	/// <summary>Log record was written when replay was in progress.</summary>
	ReplayInProgress = 0x40
}
