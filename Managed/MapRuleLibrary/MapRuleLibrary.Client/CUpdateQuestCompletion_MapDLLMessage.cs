namespace MapRuleLibrary.Client;

public class CUpdateQuestCompletion_MapDLLMessage : CMapDLLMessage
{
	public string m_QuestID;

	public bool m_AutoComplete;

	public CUpdateQuestCompletion_MapDLLMessage(string questID, bool autoComplete)
		: base(EMapDLLMessageType.UpdateQuestCompletion)
	{
		m_QuestID = questID;
		m_AutoComplete = autoComplete;
	}
}
