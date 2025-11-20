namespace ScenarioRuleLibrary;

public class CEndActorAbilityAnimSync_MessageData : CMessageData
{
	public bool m_IsLastAbility;

	public CEndActorAbilityAnimSync_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndAbilityAnimSync, actorSpawningMessage)
	{
	}
}
