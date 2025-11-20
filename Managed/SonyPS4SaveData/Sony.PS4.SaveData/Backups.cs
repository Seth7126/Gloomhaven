using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Backups
{
	[StructLayout(LayoutKind.Sequential)]
	public class BackupRequest : RequestBase
	{
		internal DirName dirName;

		public DirName DirName
		{
			get
			{
				return dirName;
			}
			set
			{
				ThrowExceptionIfLocked();
				dirName = value;
			}
		}

		public BackupRequest()
			: base(FunctionTypes.Backup)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			PrxSaveDataBackup(this, out var result);
			Notifications.ExpectingNotification();
			if (pendingRequest.response is EmptyResponse emptyResponse)
			{
				emptyResponse.Populate(result);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CheckBackupRequest : RequestBase
	{
		internal DirName dirName;

		[MarshalAs(UnmanagedType.I1)]
		internal bool includeParams;

		[MarshalAs(UnmanagedType.I1)]
		internal bool includeIcon;

		public DirName DirName
		{
			get
			{
				return dirName;
			}
			set
			{
				ThrowExceptionIfLocked();
				dirName = value;
			}
		}

		public bool IncludeParams
		{
			get
			{
				return includeParams;
			}
			set
			{
				ThrowExceptionIfLocked();
				includeParams = value;
			}
		}

		public bool IncludeIcon
		{
			get
			{
				return includeIcon;
			}
			set
			{
				ThrowExceptionIfLocked();
				includeIcon = value;
			}
		}

		public CheckBackupRequest()
			: base(FunctionTypes.CheckBackup)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			MemoryBufferNative data = default(MemoryBufferNative);
			PrxSaveDataCheckBackup(this, out data, out var result);
			if (pendingRequest.response is CheckBackupResponse checkBackupResponse)
			{
				checkBackupResponse.Populate(result, data);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class RestoreBackupRequest : RequestBase
	{
		internal DirName dirName;

		public DirName DirName
		{
			get
			{
				return dirName;
			}
			set
			{
				ThrowExceptionIfLocked();
				dirName = value;
			}
		}

		public RestoreBackupRequest()
			: base(FunctionTypes.RestoreBackup)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			PrxSaveDataRestoreBackup(this, out var result);
			if (pendingRequest.response is EmptyResponse emptyResponse)
			{
				emptyResponse.Populate(result);
			}
		}
	}

	public class CheckBackupResponse : ResponseBase
	{
		internal bool hasParams;

		internal SaveDataParams sdParams;

		internal Icon icon = null;

		public Icon Icon
		{
			get
			{
				ThrowExceptionIfLocked();
				return icon;
			}
		}

		public bool HasParams
		{
			get
			{
				ThrowExceptionIfLocked();
				return hasParams;
			}
		}

		public bool HasIcon
		{
			get
			{
				ThrowExceptionIfLocked();
				return icon != null;
			}
		}

		public SaveDataParams Params
		{
			get
			{
				ThrowExceptionIfLocked();
				return sdParams;
			}
		}

		internal void Populate(APIResult result, MemoryBufferNative data)
		{
			Populate(result);
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			if (memoryBuffer.ReadBool())
			{
				hasParams = memoryBuffer.ReadBool();
				bool flag = memoryBuffer.ReadBool();
				if (hasParams)
				{
					sdParams.Read(memoryBuffer);
				}
				if (flag)
				{
					icon = Icon.ReadAndCreate(memoryBuffer);
				}
				else
				{
					icon = null;
				}
			}
			memoryBuffer.CheckEndMarker();
		}
	}

	[DllImport("SaveData")]
	private static extern void PrxSaveDataBackup(BackupRequest request, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataCheckBackup(CheckBackupRequest request, out MemoryBufferNative data, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataRestoreBackup(RestoreBackupRequest request, out APIResult result);

	public static int Backup(BackupRequest request, EmptyResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int CheckBackup(CheckBackupRequest request, CheckBackupResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int RestoreBackup(RestoreBackupRequest request, EmptyResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}
}
