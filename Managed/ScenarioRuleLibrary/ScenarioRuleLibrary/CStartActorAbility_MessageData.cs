namespace ScenarioRuleLibrary;

public class CStartActorAbility_MessageData : CMessageData
{
	public bool m_IsFirstAbility;

	public CAbility m_Ability;

	public bool merged;

	public CStartActorAbility_MessageData(CActor actorSpawningMessage)
		: base(MessageType.StartAbility, actorSpawningMessage)
	{
	}
}
