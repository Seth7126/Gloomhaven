using AStar;

namespace ScenarioRuleLibrary;

public class CActorHasTeleported_MessageData : CMessageData
{
	public Point m_StartLocation;

	public Point m_EndLocation;

	public CActor m_ActorTeleported;

	public CAbility m_TeleportAbility;

	public bool m_skipAnimationState;

	public CActorHasTeleported_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorHasTeleported, actorSpawningMessage)
	{
	}
}
