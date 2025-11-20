using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class GlobalEventListener : GlobalEventListenerBase, IControllableAssignmentEventListener, IControllableAssignmentRequestListener, IControllableDestructionEventListener, IControllableReleaseEventListener, IGameActionEventListener, IGameActionEventClassIDListener, IGameDataEventListener, IGameDataRequestListener, INetworkActionEventListener, IPlayerEntityInitializedEventListener, IPlayerEntityRequestListener, ISavePointReachedEventListener, ISessionNegotiationEventListener, IBlockedUsersStateChangedEventListener, ILogNetworkMessageEventListener
{
	public virtual void OnEvent(ControllableAssignmentEvent evnt)
	{
	}

	public virtual void OnEvent(ControllableAssignmentRequest evnt)
	{
	}

	public virtual void OnEvent(ControllableDestructionEvent evnt)
	{
	}

	public virtual void OnEvent(ControllableReleaseEvent evnt)
	{
	}

	public virtual void OnEvent(GameActionEvent evnt)
	{
	}

	public virtual void OnEvent(GameActionEventClassID evnt)
	{
	}

	public virtual void OnEvent(GameDataEvent evnt)
	{
	}

	public virtual void OnEvent(GameDataRequest evnt)
	{
	}

	public virtual void OnEvent(NetworkActionEvent evnt)
	{
	}

	public virtual void OnEvent(PlayerEntityInitializedEvent evnt)
	{
	}

	public virtual void OnEvent(PlayerEntityRequest evnt)
	{
	}

	public virtual void OnEvent(SavePointReachedEvent evnt)
	{
	}

	public virtual void OnEvent(SessionNegotiationEvent evnt)
	{
	}

	public virtual void OnEvent(BlockedUsersStateChangedEvent evnt)
	{
	}

	public virtual void OnEvent(LogNetworkMessageEvent evnt)
	{
	}
}
