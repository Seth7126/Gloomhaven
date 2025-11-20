namespace ScenarioRuleLibrary;

public class CFinishedProcessingUndo_MessageData : CMessageData
{
	public CAbility m_Ability;

	public CFinishedProcessingUndo_MessageData()
		: base(MessageType.FinishedProcessingUndo, null)
	{
	}
}
