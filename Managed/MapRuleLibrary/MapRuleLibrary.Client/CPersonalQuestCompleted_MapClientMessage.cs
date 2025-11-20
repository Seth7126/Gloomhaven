namespace MapRuleLibrary.Client;

public class CPersonalQuestCompleted_MapClientMessage : CMapClientMessage
{
	public string m_CharacterID;

	public string m_PersonalQuestID;

	public int m_Step;

	public CPersonalQuestCompleted_MapClientMessage(string characterID, string personalQuestID, int step)
		: base(EMapClientMessageType.PersonalQuestCompleted)
	{
		m_CharacterID = characterID;
		m_PersonalQuestID = personalQuestID;
		m_Step = step;
	}
}
