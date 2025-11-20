using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CCharacterItemEquipped_MapClientMessage : CMapClientMessage
{
	public CMapCharacter m_Character;

	public CItem m_Item;

	public CCharacterItemEquipped_MapClientMessage(CMapCharacter character, CItem item)
		: base(EMapClientMessageType.CharacterItemEquipped)
	{
		m_Character = character;
		m_Item = item;
	}
}
