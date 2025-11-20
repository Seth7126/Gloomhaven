namespace MapRuleLibrary.Client;

public class CCharacterPerkPointsChanged_MapClientMessage : CMapClientMessage
{
	public string m_CharacterId;

	public string m_CharacterName;

	public int m_PerkPoints;

	public CCharacterPerkPointsChanged_MapClientMessage(string characterId, string characterName, int perkPoints)
		: base(EMapClientMessageType.CharacterPerkPointsChanged)
	{
		m_CharacterId = characterId;
		m_CharacterName = characterName;
		m_PerkPoints = perkPoints;
	}
}
