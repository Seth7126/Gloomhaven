using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sony.PS4.SaveData;

internal class Notifications
{
	private static Thread thread;

	private static bool stopThread = false;

	private static ThreadSettingsNative threadSettings;

	private static bool isBusy = false;

	private static Semaphore workLoad = new Semaphore(0, 1000);

	[DllImport("SaveData")]
	private static extern int PrxNotificationPoll(out MemoryBufferNative data, out APIResult result);

	public static void Start(ThreadAffinity affinity)
	{
		stopThread = false;
		thread = new Thread(RunProc);
		thread.Name = "SaveDataNotifications";
		threadSettings = new ThreadSettingsNative(affinity, thread.Name);
		thread.Start();
	}

	private static void RunProc()
	{
		Init.SetThreadAffinity(threadSettings);
		workLoad.WaitOne();
		while (!stopThread)
		{
			int millisecondsTimeout = -1;
			try
			{
				if (ReadAndCreateNotification())
				{
					millisecondsTimeout = 1000;
					isBusy = true;
				}
				else
				{
					isBusy = false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception Occured in Notifications handler : " + ex.Message);
				Console.WriteLine(ex.StackTrace);
				throw;
			}
			workLoad.WaitOne(millisecondsTimeout);
		}
	}

	internal static void Stop()
	{
		stopThread = true;
		workLoad.Release();
	}

	internal static void ExpectingNotification()
	{
		workLoad.Release();
	}

	internal static bool IsBusy()
	{
		return isBusy;
	}

	private static bool ReadAndCreateNotification()
	{
		bool result = true;
		MemoryBufferNative data = default(MemoryBufferNative);
		PrxNotificationPoll(out data, out var _);
		MemoryBuffer memoryBuffer = new MemoryBuffer(data);
		memoryBuffer.CheckStartMarker();
		switch (memoryBuffer.ReadInt32())
		{
		case 0:
		{
			DirName dirName = default(DirName);
			int num = memoryBuffer.ReadInt32();
			int userId = memoryBuffer.ReadInt32();
			int returnCode = memoryBuffer.ReadInt32();
			dirName.Read(memoryBuffer);
			switch (num)
			{
			case 1:
			{
				UnmountWithBackupNotification unmountWithBackupNotification = new UnmountWithBackupNotification();
				unmountWithBackupNotification.returnCode = returnCode;
				unmountWithBackupNotification.userId = userId;
				unmountWithBackupNotification.dirName = dirName;
				DispatchQueueThread.AddNotificationRequest(unmountWithBackupNotification, FunctionTypes.NotificationUnmountWithBackup, userId);
				result = true;
				break;
			}
			case 2:
			{
				BackupNotification backupNotification = new BackupNotification();
				backupNotification.returnCode = returnCode;
				backupNotification.userId = userId;
				backupNotification.dirName = dirName;
				DispatchQueueThread.AddNotificationRequest(backupNotification, FunctionTypes.NotificationBackup, userId);
				result = true;
				break;
			}
			}
			break;
		}
		case -2137063400:
			result = true;
			break;
		case -2137063416:
			result = false;
			break;
		}
		memoryBuffer.CheckEndMarker();
		return result;
	}
}
