namespace ScenarioRuleLibrary;

public class CPlayerLongRested_MessageData : CMessageData
{
	public int m_ActorOriginalHealth;

	public CAbilityCard m_AbilityCard;

	public CPlayerLongRested_MessageData(int actorOriginalHealth, CAbilityCard abilityCard, CActor actorSpawningMessage)
		: base(MessageType.PlayerLongRested, actorSpawningMessage)
	{
		m_ActorOriginalHealth = actorOriginalHealth;
		m_AbilityCard = abilityCard;
	}
}
