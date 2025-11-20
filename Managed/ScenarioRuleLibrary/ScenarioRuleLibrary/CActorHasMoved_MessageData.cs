using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CActorHasMoved_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CActor m_MovingActor;

	public List<CActor> m_ActorsToCarry;

	public bool m_Jump;

	public List<CTile> m_Waypoints;

	public CActorHasMoved_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorHasMoved, actorSpawningMessage)
	{
	}
}
