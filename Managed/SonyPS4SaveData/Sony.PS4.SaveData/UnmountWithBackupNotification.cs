namespace Sony.PS4.SaveData;

public class UnmountWithBackupNotification : ResponseBase
{
	internal int userId;

	internal DirName dirName;

	public int UserId
	{
		get
		{
			ThrowExceptionIfLocked();
			return userId;
		}
	}

	public DirName DirName
	{
		get
		{
			ThrowExceptionIfLocked();
			return dirName;
		}
	}
}
