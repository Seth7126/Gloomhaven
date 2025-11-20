using System;
using System.Collections.Generic;
using System.Threading;

namespace Sony.PS4.SaveData;

internal static class ProcessQueueThread
{
	private static Thread thread;

	private static bool stopThread = false;

	private static Semaphore workLoad = new Semaphore(0, 1000);

	private static object syncQueue = new object();

	private static int nextRequestId = 1;

	private static ThreadSettingsNative threadSettings;

	private static ManualResetEvent syncTaskCompleted = new ManualResetEvent(initialState: false);

	private static List<PendingRequest> completedSyncRequests = new List<PendingRequest>();

	private static object completedLock = new object();

	private static Queue<PendingRequest> pendingQueue = new Queue<PendingRequest>();

	private static List<PendingRequest> pollingList = new List<PendingRequest>();

	internal static List<PendingRequest> PendingRequests
	{
		get
		{
			lock (syncQueue)
			{
				return new List<PendingRequest>(pendingQueue);
			}
		}
	}

	private static void AddCompletedSyncRequest(PendingRequest pe)
	{
		lock (completedLock)
		{
			completedSyncRequests.Add(pe);
			syncTaskCompleted.Set();
		}
	}

	private static bool RemoveCompletedSyncRequest(PendingRequest pe)
	{
		lock (completedLock)
		{
			bool flag = completedSyncRequests.Remove(pe);
			if (flag && completedSyncRequests.Count == 0)
			{
				syncTaskCompleted.Reset();
			}
			return flag;
		}
	}

	public static void Start(ThreadAffinity affinity)
	{
		stopThread = false;
		thread = new Thread(RunProc);
		threadSettings = new ThreadSettingsNative(affinity, thread.Name);
		thread.Start();
	}

	private static void RunProc()
	{
		Init.SetThreadAffinity(threadSettings);
		thread.Name = "SaveDataProcessQueue";
		workLoad.WaitOne();
		while (!stopThread)
		{
			PendingRequest pendingRequest = null;
			for (int i = 0; i < pollingList.Count; i++)
			{
				pendingRequest = pollingList[i];
				if (pendingRequest.request.ExecutePolling(pendingRequest))
				{
					continue;
				}
				pollingList.RemoveAt(i);
				i--;
				if (pendingRequest.request.async)
				{
					if (!pendingRequest.request.IgnoreCallback)
					{
						DispatchQueueThread.AddRequest(pendingRequest);
						continue;
					}
					if (pendingRequest.response != null)
					{
						pendingRequest.response.locked = false;
					}
					if (pendingRequest.request != null)
					{
						pendingRequest.request.locked = false;
					}
					Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
				}
				else
				{
					if (pendingRequest.response != null)
					{
						pendingRequest.response.locked = false;
					}
					if (pendingRequest.request != null)
					{
						pendingRequest.request.locked = false;
					}
					Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
					AddCompletedSyncRequest(pendingRequest);
				}
			}
			pendingRequest = null;
			lock (syncQueue)
			{
				if (pendingQueue.Count > 0)
				{
					pendingRequest = pendingQueue.Peek();
				}
			}
			if (pendingRequest != null)
			{
				if (pendingRequest.abortPending)
				{
					lock (syncQueue)
					{
						pendingQueue.Dequeue();
					}
					EmptyResponse emptyResponse = new EmptyResponse();
					emptyResponse.returnCode = 0;
					int userId = -1;
					if (pendingRequest.request != null)
					{
						userId = pendingRequest.request.userId;
					}
					DispatchQueueThread.AddNotificationRequest(emptyResponse, FunctionTypes.NotificationAborted, userId, pendingRequest.requestId, pendingRequest.request);
				}
				else
				{
					try
					{
						pendingRequest.request.Execute(pendingRequest);
					}
					catch (Exception exception)
					{
						if (pendingRequest.response != null)
						{
							pendingRequest.response.exception = exception;
						}
					}
					lock (syncQueue)
					{
						pendingQueue.Dequeue();
					}
					if (!pendingRequest.request.IsDeferred || pendingRequest.response.IsErrorCodeWithoutLockCheck)
					{
						if (pendingRequest.request.async)
						{
							if (!pendingRequest.request.IgnoreCallback)
							{
								DispatchQueueThread.AddRequest(pendingRequest);
							}
							else
							{
								if (pendingRequest.response != null)
								{
									pendingRequest.response.locked = false;
								}
								if (pendingRequest.request != null)
								{
									pendingRequest.request.locked = false;
								}
								Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
							}
						}
						else
						{
							if (pendingRequest.response != null)
							{
								pendingRequest.response.locked = false;
							}
							if (pendingRequest.request != null)
							{
								pendingRequest.request.locked = false;
							}
							Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
							AddCompletedSyncRequest(pendingRequest);
						}
					}
					else
					{
						pollingList.Add(pendingRequest);
					}
				}
			}
			int millisecondsTimeout = -1;
			if (pollingList.Count > 0)
			{
				millisecondsTimeout = 16;
			}
			workLoad.WaitOne(millisecondsTimeout);
		}
	}

	internal static int GenerateNotificationRequestId()
	{
		int result = 0;
		lock (syncQueue)
		{
			result = nextRequestId;
			nextRequestId++;
		}
		return result;
	}

	internal static PendingRequest AddEvent(RequestBase request, ResponseBase response)
	{
		PendingRequest pendingRequest = new PendingRequest();
		pendingRequest.request = request;
		pendingRequest.response = response;
		pendingRequest.requestId = 0;
		response.locked = true;
		request.locked = true;
		lock (syncQueue)
		{
			pendingRequest.requestId = nextRequestId;
			nextRequestId++;
			pendingQueue.Enqueue(pendingRequest);
		}
		workLoad.Release();
		return pendingRequest;
	}

	internal static int WaitIfSyncRequest(PendingRequest pendingEvent)
	{
		if (!pendingEvent.request.async)
		{
			while (!RemoveCompletedSyncRequest(pendingEvent))
			{
				syncTaskCompleted.WaitOne();
			}
			return pendingEvent.response.returnCode;
		}
		return pendingEvent.requestId;
	}

	internal static bool AbortRequest(int requestId)
	{
		lock (syncQueue)
		{
			PendingRequest[] array = pendingQueue.ToArray();
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i].requestId == requestId && array[i].request != null && array[i].request.async)
				{
					array[i].abortPending = true;
					return true;
				}
			}
		}
		return false;
	}

	internal static void Stop()
	{
		stopThread = true;
		workLoad.Release();
	}
}
