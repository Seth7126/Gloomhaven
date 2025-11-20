using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CItemRemovedFromParty_MapClientMessage : CMapClientMessage
{
	public CItem m_Item;

	public int m_SlotIndex;

	public CItemRemovedFromParty_MapClientMessage(CItem item, int slotIndex)
		: base(EMapClientMessageType.ItemRemovedFromParty)
	{
		m_Item = item;
		m_SlotIndex = slotIndex;
	}
}
