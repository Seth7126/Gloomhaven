using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class EntityEventListener : EntityEventListenerBase, ILogNetworkMessageEventListener
{
	public virtual void OnEvent(LogNetworkMessageEvent evnt)
	{
	}
}
public class EntityEventListener<TState> : EntityEventListenerBase<TState>, ILogNetworkMessageEventListener
{
	public virtual void OnEvent(LogNetworkMessageEvent evnt)
	{
	}
}
