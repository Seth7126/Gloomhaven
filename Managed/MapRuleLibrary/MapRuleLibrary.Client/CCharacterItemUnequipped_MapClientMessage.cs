using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CCharacterItemUnequipped_MapClientMessage : CMapClientMessage
{
	public CMapCharacter m_Character;

	public List<CItem> m_Items;

	public CCharacterItemUnequipped_MapClientMessage(CMapCharacter character, List<CItem> items)
		: base(EMapClientMessageType.CharacterItemUnequipped)
	{
		m_Character = character;
		m_Items = items;
	}
}
