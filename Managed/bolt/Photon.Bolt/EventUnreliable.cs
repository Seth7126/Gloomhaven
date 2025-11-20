using System.Collections.Generic;

namespace Photon.Bolt;

internal struct EventUnreliable
{
	internal class PriorityComparer : IComparer<EventUnreliable>
	{
		public static readonly PriorityComparer Instance = new PriorityComparer();

		private PriorityComparer()
		{
		}

		int IComparer<EventUnreliable>.Compare(EventUnreliable x, EventUnreliable y)
		{
			return y.Priority.CompareTo(x.Priority);
		}
	}

	public bool Skipped;

	public Event NetworkEvent;

	public float Priority;

	public static EventUnreliable Wrap(Event ev)
	{
		EventUnreliable result = default(EventUnreliable);
		result.NetworkEvent = ev;
		result.Priority = 0f;
		result.Skipped = false;
		return result;
	}
}
