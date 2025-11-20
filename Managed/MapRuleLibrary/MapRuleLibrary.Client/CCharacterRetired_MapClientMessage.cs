namespace MapRuleLibrary.Client;

public class CCharacterRetired_MapClientMessage : CMapClientMessage
{
	public string m_CharacterID;

	public string m_PersonalQuestID;

	public CCharacterRetired_MapClientMessage(string characterID, string personalQuestID)
		: base(EMapClientMessageType.CharacterRetired)
	{
		m_CharacterID = characterID;
		m_PersonalQuestID = personalQuestID;
	}
}
