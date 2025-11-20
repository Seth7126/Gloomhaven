using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI;

public class UiNavigationVisibleInScroll : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _innerNavigation;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		IUiNavigationSelectable currentlySelectedElement = inNode.UiNavigationManager.CurrentlySelectedElement;
		if (_scroll == null || _scroll.IsPartiallyVisibleInViewport(currentlySelectedElement))
		{
			if (_innerNavigation != null)
			{
				return _innerNavigation.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
			}
			return ((UiNavigationGroup)inNode).OldFashionNavigation(proceededNodes, navigationDirection);
		}
		return JumpToFirstVisible(inNode);
	}

	private bool JumpToFirstVisible(IUiNavigationNode uiNode)
	{
		IUiNavigationManager uiNavigationManager = uiNode.UiNavigationManager;
		IUiNavigationElement element = null;
		float num = float.MinValue;
		foreach (IUiNavigationElement element2 in uiNode.Elements)
		{
			if (_scroll.IsPartiallyVisibleInViewport(element2) && (!(element2 is IUiNavigationSelectable uiNavigationSelectable) || uiNavigationSelectable.ControlledSelectable.interactable) && element2.NavigationPosition.y > num)
			{
				num = element2.NavigationPosition.y;
				element = element2;
			}
		}
		return Select(uiNavigationManager, element);
	}

	private bool Select(IUiNavigationManager manager, IUiNavigationElement element)
	{
		if (!(element is IUiNavigationSelectable uiNavigationSelectable))
		{
			if (element is IUiNavigationNode navigationNode)
			{
				return manager.TrySelectFirstIn(navigationNode);
			}
			return false;
		}
		return manager.TrySelect(uiNavigationSelectable);
	}
}
