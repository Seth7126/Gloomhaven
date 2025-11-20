namespace ScenarioRuleLibrary;

public class CAbilityStartUpdateCombatLog_MessageData : CMessageData
{
	public CAbility.EAbilityType m_AbilityType;

	public CAbility m_Ability;

	public CAbilityStartUpdateCombatLog_MessageData(CActor actorSpawningMessage)
		: base(MessageType.AbilityStartUpdateCombatLog, actorSpawningMessage)
	{
	}
}
