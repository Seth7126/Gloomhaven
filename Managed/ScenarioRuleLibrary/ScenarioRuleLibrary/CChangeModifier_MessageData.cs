namespace ScenarioRuleLibrary;

public class CChangeModifier_MessageData : CMessageData
{
	public CBaseCard m_BaseCard;

	public string m_OriginalModifier;

	public string m_ReplaceModifier;

	public CChangeModifier_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ChangeModifier, actorSpawningMessage)
	{
	}
}
