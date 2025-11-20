namespace ScenarioRuleLibrary;

public class CActorHasAttacked_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_AttackAbility;

	public CActorHasAttacked_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.ActorHasAttacked, actorSpawningMessage, animOverload)
	{
	}
}
