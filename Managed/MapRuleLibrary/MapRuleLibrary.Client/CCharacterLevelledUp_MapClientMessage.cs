namespace MapRuleLibrary.Client;

public class CCharacterLevelledUp_MapClientMessage : CMapClientMessage
{
	public string m_CharacterId;

	public string m_CharacterName;

	public CCharacterLevelledUp_MapClientMessage(string characterId, string characterName = null)
		: base(EMapClientMessageType.CharacterLevelledUp)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
	}
}
