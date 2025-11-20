using System.Collections.Generic;
using SM.Gamepad;

namespace Script.GUI;

public class UiNavigationIsHoveredItemExistsCondition : UiNavigationCondition
{
	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return Singleton<UIPartyCharacterEquipmentDisplay>.Instance.IsHoveredItemExists();
	}
}
