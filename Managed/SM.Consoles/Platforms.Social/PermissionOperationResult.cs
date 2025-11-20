namespace Platforms.Social;

public enum PermissionOperationResult
{
	Success,
	NotAllowed,
	UserInBlockList,
	UserInMuteList,
	PrivilegeSettingsRestricts,
	Unknown
}
