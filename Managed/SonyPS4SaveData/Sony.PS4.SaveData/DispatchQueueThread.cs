using System.Collections.Generic;
using System.Threading;

namespace Sony.PS4.SaveData;

internal static class DispatchQueueThread
{
	private static Thread thread;

	private static bool stopThread = false;

	private static Semaphore workLoad = new Semaphore(0, 1000);

	private static object syncQueue = new object();

	private static ThreadSettingsNative threadSettings;

	private static Queue<SaveDataCallbackEvent> pendingQueue = new Queue<SaveDataCallbackEvent>();

	public static void Start(ThreadAffinity affinity)
	{
		stopThread = false;
		thread = new Thread(RunProc);
		thread.Name = "SaveDataDispatchQueue";
		threadSettings = new ThreadSettingsNative(affinity, thread.Name);
		thread.Start();
	}

	private static void RunProc()
	{
		Init.SetThreadAffinity(threadSettings);
		workLoad.WaitOne();
		while (!stopThread)
		{
			SaveDataCallbackEvent saveDataCallbackEvent = null;
			lock (syncQueue)
			{
				if (pendingQueue.Count > 0)
				{
					saveDataCallbackEvent = pendingQueue.Dequeue();
				}
			}
			if (saveDataCallbackEvent != null)
			{
				if (saveDataCallbackEvent.response != null)
				{
					saveDataCallbackEvent.response.locked = false;
				}
				if (saveDataCallbackEvent.request != null)
				{
					saveDataCallbackEvent.request.locked = false;
				}
				Main.ProcessInternalResponses(saveDataCallbackEvent.request, saveDataCallbackEvent.response);
				Main.CallOnAsyncEvent(saveDataCallbackEvent);
			}
			workLoad.WaitOne();
		}
	}

	internal static void AddRequest(PendingRequest finishedRequest)
	{
		SaveDataCallbackEvent saveDataCallbackEvent = new SaveDataCallbackEvent();
		saveDataCallbackEvent.apiCalled = finishedRequest.request.functionType;
		saveDataCallbackEvent.requestId = finishedRequest.requestId;
		saveDataCallbackEvent.userId = finishedRequest.request.userId;
		saveDataCallbackEvent.request = finishedRequest.request;
		saveDataCallbackEvent.response = finishedRequest.response;
		lock (syncQueue)
		{
			pendingQueue.Enqueue(saveDataCallbackEvent);
		}
		workLoad.Release();
	}

	internal static void AddNotificationRequest(ResponseBase response, FunctionTypes apiCalled, int userId, int requestId = -1, RequestBase request = null)
	{
		if (requestId == -1)
		{
			requestId = ProcessQueueThread.GenerateNotificationRequestId();
		}
		SaveDataCallbackEvent saveDataCallbackEvent = new SaveDataCallbackEvent();
		saveDataCallbackEvent.apiCalled = apiCalled;
		saveDataCallbackEvent.requestId = requestId;
		saveDataCallbackEvent.userId = userId;
		saveDataCallbackEvent.request = request;
		saveDataCallbackEvent.response = response;
		lock (syncQueue)
		{
			pendingQueue.Enqueue(saveDataCallbackEvent);
		}
		workLoad.Release();
	}

	internal static void Stop()
	{
		stopThread = true;
		workLoad.Release();
	}

	internal static bool IsSameThread()
	{
		return Thread.CurrentThread.ManagedThreadId == thread.ManagedThreadId;
	}

	internal static void ThrowExceptionIfSameThread(bool asyncRequest)
	{
		if (!asyncRequest && IsSameThread())
		{
			throw new SaveDataException("A synchronous (blocking) request can't be made on this thread.");
		}
	}
}
