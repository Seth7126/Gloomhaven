namespace UdpKit;

public abstract class UdpEventBase
{
	public abstract int Type { get; }

	public static implicit operator UdpEvent(UdpEventBase ev)
	{
		return new UdpEvent
		{
			Type = ev.Type,
			Object0 = ev
		};
	}
}
