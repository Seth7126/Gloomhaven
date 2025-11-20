using System.Collections.Generic;
using SM.Gamepad;

namespace Script.GUI;

public class UiNavigationFromInside : UiNavigationCondition
{
	public override bool IsTrue(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		IUiNavigationManager uiNavigationManager = inNode.UiNavigationManager;
		return uiNavigationManager.PathToSelectable(uiNavigationManager.CurrentlySelectedElement)?.Contains(inNode) ?? false;
	}
}
