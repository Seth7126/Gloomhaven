using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationJumpToLastIn : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private UiNavigationGroup _target;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		IUiNavigationManager uiNavigationManager = inNode.UiNavigationManager;
		List<IUiNavigationElement> elements = _target.Elements;
		for (int num = _target.Elements.Count - 1; num >= 0; num--)
		{
			IUiNavigationElement uiNavigationElement = elements[num];
			if (uiNavigationElement.GameObject.activeInHierarchy)
			{
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
		}
		return false;
	}
}
