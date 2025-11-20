namespace Platforms.PlatformData;

public enum OperationResult
{
	Success = 0,
	InvalidPath = 10,
	AccessDenied = 20,
	NotEnoughSpace = 30,
	FileCorrupt = 40,
	UnspecifiedError = 1000
}
