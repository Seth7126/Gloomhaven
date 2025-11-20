using System.Collections.Generic;

namespace Sony.NP;

internal static class PendingAsyncRequestList
{
	private static Dictionary<uint, PendingRequest> requestsLookup = new Dictionary<uint, PendingRequest>();

	private static List<PendingRequest> pendingRequests = new List<PendingRequest>();

	private static object syncObject = new object();

	public static List<PendingRequest> PendingRequests
	{
		get
		{
			lock (syncObject)
			{
				return new List<PendingRequest>(pendingRequests);
			}
		}
	}

	internal static void Shutdown()
	{
		lock (syncObject)
		{
			pendingRequests.Clear();
		}
	}

	public static bool IsPending(uint npRequestId)
	{
		lock (syncObject)
		{
			return requestsLookup.ContainsKey(npRequestId);
		}
	}

	internal static void AddRequest(uint npRequestId, RequestBase request)
	{
		lock (syncObject)
		{
			PendingRequest pendingRequest = new PendingRequest();
			pendingRequest.npRequestId = npRequestId;
			pendingRequest.request = request;
			pendingRequest.abortPending = false;
			requestsLookup.Add(npRequestId, pendingRequest);
			pendingRequests.Add(pendingRequest);
		}
	}

	internal static RequestBase RemoveRequest(uint npRequestId)
	{
		lock (syncObject)
		{
			PendingRequest value = null;
			if (!requestsLookup.TryGetValue(npRequestId, out value))
			{
				return null;
			}
			requestsLookup.Remove(npRequestId);
			pendingRequests.Remove(value);
			return value.request;
		}
	}

	internal static bool MarkRequestAsAborting(uint npRequestId)
	{
		lock (syncObject)
		{
			PendingRequest value = null;
			if (!requestsLookup.TryGetValue(npRequestId, out value))
			{
				return false;
			}
			value.abortPending = true;
			return true;
		}
	}

	internal static void RequestHasBeenAborted(uint npRequestId)
	{
		lock (syncObject)
		{
			RemoveRequest(npRequestId);
		}
	}
}
