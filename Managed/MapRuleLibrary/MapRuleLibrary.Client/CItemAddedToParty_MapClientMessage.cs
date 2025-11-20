using ScenarioRuleLibrary;

namespace MapRuleLibrary.Client;

public class CItemAddedToParty_MapClientMessage : CMapClientMessage
{
	public CItem m_Item;

	public CItemAddedToParty_MapClientMessage(CItem item)
		: base(EMapClientMessageType.ItemAddedToParty)
	{
		m_Item = item;
	}
}
