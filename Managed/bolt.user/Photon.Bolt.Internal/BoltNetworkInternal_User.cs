namespace Photon.Bolt.Internal;

public static class BoltNetworkInternal_User
{
	public static void EnvironmentSetup()
	{
		Factory.Register(ControllableAssignmentEvent_Meta.Instance);
		Factory.Register(ControllableAssignmentRequest_Meta.Instance);
		Factory.Register(ControllableDestructionEvent_Meta.Instance);
		Factory.Register(ControllableReleaseEvent_Meta.Instance);
		Factory.Register(GameActionEvent_Meta.Instance);
		Factory.Register(GameActionEventClassID_Meta.Instance);
		Factory.Register(GameDataEvent_Meta.Instance);
		Factory.Register(GameDataRequest_Meta.Instance);
		Factory.Register(NetworkActionEvent_Meta.Instance);
		Factory.Register(PlayerEntityInitializedEvent_Meta.Instance);
		Factory.Register(PlayerEntityRequest_Meta.Instance);
		Factory.Register(SavePointReachedEvent_Meta.Instance);
		Factory.Register(SessionNegotiationEvent_Meta.Instance);
		Factory.Register(BlockedUsersStateChangedEvent_Meta.Instance);
		Factory.Register(LogNetworkMessageEvent_Meta.Instance);
		Factory.Register(ControllableState_Meta.Instance);
		Factory.Register(GHControllableState_Meta.Instance);
		Factory.Register(PlayerState_Meta.Instance);
		Factory.Register(VoicePlayer_Meta.Instance);
	}

	public static void EnvironmentReset()
	{
	}
}
