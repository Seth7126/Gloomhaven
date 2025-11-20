namespace Sony.PS4.SaveData;

public enum FunctionTypes
{
	Invalid,
	Mount,
	Unmount,
	GetMountInfo,
	GetMountParams,
	SetMountParams,
	SaveIcon,
	LoadIcon,
	Delete,
	DirNameSearch,
	Backup,
	CheckBackup,
	RestoreBackup,
	FileOps,
	OpenDialog,
	NotificationUnmountWithBackup,
	NotificationBackup,
	NotificationAborted,
	NotificationDialogOpened,
	NotificationDialogClosed
}
