namespace ScenarioRuleLibrary;

public class CPlayerToSelectAbilityCardsOrLongRest_MessageData : CMessageData
{
	public CPlayerToSelectAbilityCardsOrLongRest_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerToSelectAbilityCardsOrLongRest, actorSpawningMessage)
	{
	}
}
