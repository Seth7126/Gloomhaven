namespace ScenarioRuleLibrary;

public class CPreventDamageTriggered_MessageData : CMessageData
{
	public CBaseCard m_PreventDamageBaseCard;

	public CPreventDamageTriggered_MessageData(string animOverload, CActor actorSpawningMessage)
		: base(MessageType.PreventDamageTriggered, actorSpawningMessage, animOverload)
	{
	}
}
