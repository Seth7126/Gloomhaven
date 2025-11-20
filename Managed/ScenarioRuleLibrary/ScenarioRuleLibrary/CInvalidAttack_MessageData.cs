namespace ScenarioRuleLibrary;

public class CInvalidAttack_MessageData : CMessageData
{
	public CActor m_AttackingActor;

	public CInvalidAttack_MessageData(CActor actorSpawningMessage)
		: base(MessageType.InvalidAttack, actorSpawningMessage)
	{
	}
}
