namespace ScenarioRuleLibrary;

public class CEndRound_MessageData : CMessageData
{
	public CEndRound_MessageData(CActor actorSpawningMessage)
		: base(MessageType.EndRound, actorSpawningMessage)
	{
	}
}
