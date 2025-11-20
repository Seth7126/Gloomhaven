namespace ScenarioRuleLibrary;

public class CActorIsInfusing_MessageData : CMessageData
{
	public ElementInfusionBoardManager.EElement m_Element;

	public CActorIsInfusing_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsInfusing, actorSpawningMessage)
	{
	}
}
