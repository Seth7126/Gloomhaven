using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class FileOps
{
	[StructLayout(LayoutKind.Sequential)]
	public abstract class FileOperationRequest : RequestBase
	{
		internal Mounting.MountPointName mountPointName;

		public Mounting.MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
			}
		}

		public FileOperationRequest()
			: base(FunctionTypes.FileOps)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			FileOperationResponse fileOperationResponse = pendingRequest.response as FileOperationResponse;
			Mounting.MountPoint mountPoint = Mounting.FindMountPoint(mountPointName);
			if (mountPoint == null || !mountPoint.IsMounted)
			{
				fileOperationResponse.returnCode = -1979711487;
			}
			else
			{
				DoFileOperations(mountPoint, fileOperationResponse);
			}
		}

		public abstract void DoFileOperations(Mounting.MountPoint mp, FileOperationResponse response);
	}

	public abstract class FileOperationResponse : ResponseBase
	{
		internal float progress;

		public float Progress => progress;

		public void UpdateProgress(float progress)
		{
			this.progress = progress;
		}

		public FileOperationResponse()
		{
			progress = 0f;
		}
	}

	public static int CustomFileOp(FileOperationRequest request, FileOperationResponse response)
	{
		response.progress = 0f;
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}
}
