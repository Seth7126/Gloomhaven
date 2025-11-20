namespace Photon.Bolt;

public struct EventReliable
{
	public Event NetworkEvent;

	public uint Sequence;

	public static EventReliable Wrap(Event ev)
	{
		return Wrap(ev, 0u);
	}

	public static EventReliable Wrap(Event ev, uint sequence)
	{
		EventReliable result = default(EventReliable);
		result.NetworkEvent = ev;
		result.Sequence = sequence;
		return result;
	}
}
