using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CCharacterItemUnbound_MapClientMessage : CMapClientMessage
{
	public CMapCharacter m_Character;

	public CItem m_Item;

	public CCharacterItemUnbound_MapClientMessage(CMapCharacter character, CItem item)
		: base(EMapClientMessageType.CharacterItemUnbound)
	{
		m_Character = character;
		m_Item = item;
	}
}
