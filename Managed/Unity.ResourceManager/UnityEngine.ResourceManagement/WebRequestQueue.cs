using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement;

internal static class WebRequestQueue
{
	internal static int s_MaxRequest = 500;

	internal static Queue<WebRequestQueueOperation> s_QueuedOperations = new Queue<WebRequestQueueOperation>();

	internal static List<UnityWebRequestAsyncOperation> s_ActiveRequests = new List<UnityWebRequestAsyncOperation>();

	public static void SetMaxConcurrentRequests(int maxRequests)
	{
		if (maxRequests < 1)
		{
			throw new ArgumentException("MaxRequests must be 1 or greater.", "maxRequests");
		}
		s_MaxRequest = maxRequests;
	}

	public static WebRequestQueueOperation QueueRequest(UnityWebRequest request)
	{
		WebRequestQueueOperation webRequestQueueOperation = new WebRequestQueueOperation(request);
		if (s_ActiveRequests.Count < s_MaxRequest)
		{
			UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = null;
			try
			{
				unityWebRequestAsyncOperation = request.SendWebRequest();
				s_ActiveRequests.Add(unityWebRequestAsyncOperation);
				if (unityWebRequestAsyncOperation.isDone)
				{
					OnWebAsyncOpComplete(unityWebRequestAsyncOperation);
				}
				else
				{
					unityWebRequestAsyncOperation.completed += OnWebAsyncOpComplete;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
			}
			webRequestQueueOperation.Complete(unityWebRequestAsyncOperation);
		}
		else
		{
			s_QueuedOperations.Enqueue(webRequestQueueOperation);
		}
		return webRequestQueueOperation;
	}

	internal static void WaitForRequestToBeActive(WebRequestQueueOperation request, int millisecondsTimeout)
	{
		List<UnityWebRequestAsyncOperation> list = new List<UnityWebRequestAsyncOperation>();
		while (s_QueuedOperations.Contains(request))
		{
			list.Clear();
			foreach (UnityWebRequestAsyncOperation s_ActiveRequest in s_ActiveRequests)
			{
				if (UnityWebRequestUtilities.IsAssetBundleDownloaded(s_ActiveRequest))
				{
					list.Add(s_ActiveRequest);
				}
			}
			foreach (UnityWebRequestAsyncOperation item in list)
			{
				bool flag = s_QueuedOperations.Peek() == request;
				item.completed -= OnWebAsyncOpComplete;
				OnWebAsyncOpComplete(item);
				if (flag)
				{
					return;
				}
			}
			Thread.Sleep(millisecondsTimeout);
		}
	}

	private static void OnWebAsyncOpComplete(AsyncOperation operation)
	{
		s_ActiveRequests.Remove(operation as UnityWebRequestAsyncOperation);
		if (s_QueuedOperations.Count > 0)
		{
			WebRequestQueueOperation webRequestQueueOperation = s_QueuedOperations.Dequeue();
			UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = webRequestQueueOperation.m_WebRequest.SendWebRequest();
			unityWebRequestAsyncOperation.completed += OnWebAsyncOpComplete;
			s_ActiveRequests.Add(unityWebRequestAsyncOperation);
			webRequestQueueOperation.Complete(unityWebRequestAsyncOperation);
		}
	}
}
