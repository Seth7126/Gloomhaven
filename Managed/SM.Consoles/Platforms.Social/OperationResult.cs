namespace Platforms.Social;

public enum OperationResult
{
	Success = 0,
	UserWasAskedToGetPrivilege = 50,
	CancelledByUser = 100,
	UserNotSignedIn = 500,
	RequestIsLocked = 900,
	UnspecifiedError = 1000
}
