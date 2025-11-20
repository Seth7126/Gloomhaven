using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationJumpToFirstIn : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private UiNavigationGroup _target;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		IUiNavigationManager uiNavigationManager = inNode.UiNavigationManager;
		List<IUiNavigationElement> elements = _target.Elements;
		for (int i = 0; i < elements.Count; i++)
		{
			IUiNavigationElement uiNavigationElement = elements[i];
			if (!uiNavigationElement.GameObject.activeInHierarchy)
			{
				continue;
			}
			if (uiNavigationElement is IUiNavigationSelectable uiNavigationSelectable)
			{
				if (uiNavigationSelectable.ControlledSelectable.interactable)
				{
					return uiNavigationManager.TrySelect(uiNavigationSelectable);
				}
			}
			else if (uiNavigationElement is IUiNavigationNode navigationNode)
			{
				return uiNavigationManager.TrySelectFirstIn(navigationNode);
			}
		}
		return false;
	}
}
