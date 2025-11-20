namespace ScenarioRuleLibrary;

public class CActorHasPushed_MessageData : CMessageData
{
	public CActor m_ActorBeingPushed;

	public CAbilityPush m_PushAbility;

	public CActorHasPushed_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorHasPushed, actorSpawningMessage)
	{
	}
}
