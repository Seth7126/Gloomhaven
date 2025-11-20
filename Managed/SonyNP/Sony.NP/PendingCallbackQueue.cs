using System.Collections.Generic;
using System.Threading;

namespace Sony.NP;

internal static class PendingCallbackQueue
{
	private static Queue<NpCallbackEvent> pendingEvents = new Queue<NpCallbackEvent>();

	private static object syncObject = new object();

	public static void AddEvent(NpCallbackEvent callbackEvent)
	{
		Monitor.Enter(syncObject);
		pendingEvents.Enqueue(callbackEvent);
		Monitor.Exit(syncObject);
	}

	public static NpCallbackEvent PopEvent()
	{
		NpCallbackEvent result = null;
		if (Monitor.TryEnter(syncObject))
		{
			if (pendingEvents.Count == 0)
			{
				Monitor.Exit(syncObject);
				return null;
			}
			result = pendingEvents.Dequeue();
			Monitor.Exit(syncObject);
		}
		return result;
	}
}
