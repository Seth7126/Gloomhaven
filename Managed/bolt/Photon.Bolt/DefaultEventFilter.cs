namespace Photon.Bolt;

public class DefaultEventFilter : IEventFilter
{
	bool IEventFilter.EventReceived(Event ev)
	{
		return true;
	}
}
