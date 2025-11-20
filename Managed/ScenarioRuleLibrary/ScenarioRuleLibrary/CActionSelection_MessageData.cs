namespace ScenarioRuleLibrary;

public class CActionSelection_MessageData : CMessageData
{
	public CActionSelection_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActionSelection, actorSpawningMessage)
	{
	}
}
