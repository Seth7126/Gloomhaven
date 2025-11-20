using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CCharacterItemBound_MapClientMessage : CMapClientMessage
{
	public CMapCharacter m_Character;

	public CItem m_Item;

	public CCharacterItemBound_MapClientMessage(CMapCharacter character, CItem item)
		: base(EMapClientMessageType.CharacterItemBound)
	{
		m_Character = character;
		m_Item = item;
	}
}
