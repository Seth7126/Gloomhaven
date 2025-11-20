namespace ScenarioRuleLibrary;

public class CSelectLoseCards_MessageData : CMessageData
{
	public CActor m_ActorLosingCards;

	public CAbility m_Ability;

	public CSelectLoseCards_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.SelectLoseCards, actorSpawningMessage, animOverload)
	{
	}
}
