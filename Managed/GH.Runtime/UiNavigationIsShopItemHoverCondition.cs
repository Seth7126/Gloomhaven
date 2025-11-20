using System.Collections.Generic;
using SM.Gamepad;
using Script.GUI;

public class UiNavigationIsShopItemHoverCondition : UiNavigationCondition
{
	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return Singleton<UIShopItemWindow>.Instance.ItemInventory.CurrentHoveredItemSlot != null;
	}
}
