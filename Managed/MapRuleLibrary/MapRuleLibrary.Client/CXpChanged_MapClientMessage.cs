namespace MapRuleLibrary.Client;

public class CXpChanged_MapClientMessage : CMapClientMessage
{
	public string m_CharacterId;

	public string m_CharacterName;

	public CXpChanged_MapClientMessage(string characterId, string characterName = null)
		: base(EMapClientMessageType.CharacterXpChanged)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
	}
}
