namespace ScenarioRuleLibrary;

public class CActorIsSelectingActiveBonusBuffTarget_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CAbilityAttack m_AttackAbility;

	public CAttackSummary m_AttackSummary;

	public string m_ActiveBonusName;

	public string m_BaseCardID;

	public CActorIsSelectingActiveBonusBuffTarget_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingActiveBonusBuffTarget, actorSpawningMessage)
	{
	}
}
