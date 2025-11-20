using System.Collections.Generic;

namespace Sony.NP;

internal static class PendingAsyncResponseList
{
	private static Dictionary<uint, ResponseBase> responseLookup = new Dictionary<uint, ResponseBase>();

	private static object syncObject = new object();

	public static void AddResponse(uint npRequestId, ResponseBase response)
	{
		lock (syncObject)
		{
			responseLookup.Add(npRequestId, response);
		}
	}

	public static ResponseBase FindAndRemoveResponse(uint npRequestId)
	{
		lock (syncObject)
		{
			if (responseLookup.TryGetValue(npRequestId, out var value))
			{
				responseLookup.Remove(npRequestId);
				return value;
			}
			return null;
		}
	}
}
