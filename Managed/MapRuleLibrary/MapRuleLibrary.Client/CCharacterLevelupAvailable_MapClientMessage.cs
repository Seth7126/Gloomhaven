namespace MapRuleLibrary.Client;

public class CCharacterLevelupAvailable_MapClientMessage : CMapClientMessage
{
	public string m_CharacterId;

	public string m_CharacterName;

	public CCharacterLevelupAvailable_MapClientMessage(string characterId, string characterName = null)
		: base(EMapClientMessageType.CharacterLevelUpAvailable)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
	}
}
