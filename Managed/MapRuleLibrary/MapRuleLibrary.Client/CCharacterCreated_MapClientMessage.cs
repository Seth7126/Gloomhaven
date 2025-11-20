using MapRuleLibrary.Party;

namespace MapRuleLibrary.Client;

public class CCharacterCreated_MapClientMessage : CMapClientMessage
{
	public CMapCharacter m_Character;

	public CCharacterCreated_MapClientMessage(CMapCharacter character)
		: base(EMapClientMessageType.CharacterCreated)
	{
		m_Character = character;
	}
}
