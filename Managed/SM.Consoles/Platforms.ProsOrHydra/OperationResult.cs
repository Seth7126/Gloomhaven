namespace Platforms.ProsOrHydra;

public enum OperationResult
{
	NotLinked,
	Linked,
	UnspecifiedError,
	CodeTimeout,
	CodeRead,
	CodeIsNotInitialized,
	RemovedFromQueue,
	Success,
	NoTransfers,
	PendingTransfer,
	InvalidSaveData
}
