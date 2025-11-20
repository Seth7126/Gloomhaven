namespace MapRuleLibrary.Client;

public class CGoldChanged_MapClientMessage : CMapClientMessage
{
	public int m_NewGold;

	public int m_PreviousGold;

	public string m_CharacterId;

	public string m_CharacterName;

	public CGoldChanged_MapClientMessage(int newGold, int previousGold, string characterId = null, string characterName = null)
		: base(EMapClientMessageType.GoldChanged)
	{
		m_NewGold = newGold;
		m_PreviousGold = previousGold;
		m_CharacterId = characterId;
		m_CharacterName = characterName;
	}
}
