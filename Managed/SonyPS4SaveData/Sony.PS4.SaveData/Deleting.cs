using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Deleting
{
	[StructLayout(LayoutKind.Sequential)]
	public class DeleteRequest : RequestBase
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

		public DeleteRequest()
			: base(FunctionTypes.Delete)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			PrxSaveDataDelete(this, out var result);
			if (pendingRequest.response is EmptyResponse emptyResponse)
			{
				emptyResponse.Populate(result);
			}
		}
	}

	[DllImport("SaveData")]
	private static extern void PrxSaveDataDelete(DeleteRequest request, out APIResult result);

	public static int Delete(DeleteRequest request, EmptyResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}
}
