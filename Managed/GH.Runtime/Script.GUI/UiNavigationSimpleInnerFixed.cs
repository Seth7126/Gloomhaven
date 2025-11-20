using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationSimpleInnerFixed : CustomizableNavigationHandlerBehaviour
{
	private List<IUiNavigationElement> _collectedElementsBuffer = new List<IUiNavigationElement>();

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (proceededNodes.Contains(inNode))
		{
			return false;
		}
		IUiNavigationManager uiNavigationManager = inNode.UiNavigationManager;
		CollectElementsByDirection(proceededNodes, inNode, inNode.Elements, navigationDirection, _collectedElementsBuffer);
		IUiNavigationElement closestElement = GetClosestElement(uiNavigationManager, _collectedElementsBuffer);
		if (closestElement == null)
		{
			proceededNodes.Add(inNode);
			return false;
		}
		if (!(closestElement is IUiNavigationSelectable uiNavigationSelectable))
		{
			if (closestElement is IUiNavigationNode uiNavigationNode)
			{
				bool num = uiNavigationNode.TryToNavigate(proceededNodes, navigationDirection);
				if (!num)
				{
					proceededNodes.Add(uiNavigationNode);
				}
				return num;
			}
			proceededNodes.Add(inNode);
			return false;
		}
		bool num2 = uiNavigationManager.TrySelect(uiNavigationSelectable);
		if (!num2)
		{
			proceededNodes.Add(inNode);
		}
		return num2;
	}

	private bool NeedToSkip(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationElement element)
	{
		if (!element.GameObject.activeInHierarchy)
		{
			return true;
		}
		if (element is IUiNavigationNode item && proceededNodes.Contains(item))
		{
			return true;
		}
		if (element is IUiNavigationSelectable uiNavigationSelectable && !uiNavigationSelectable.ControlledSelectable.interactable)
		{
			return true;
		}
		return false;
	}

	protected virtual void CollectElementsByDirection(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode node, List<IUiNavigationElement> elements, UINavigationDirection navigationDirection, List<IUiNavigationElement> result)
	{
		result.Clear();
		switch (navigationDirection)
		{
		case UINavigationDirection.Left:
		{
			foreach (IUiNavigationElement element in elements)
			{
				if (!NeedToSkip(proceededNodes, element) && element.NavigationPosition.x < node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.x)
				{
					result.Add(element);
				}
			}
			break;
		}
		case UINavigationDirection.Right:
		{
			foreach (IUiNavigationElement element2 in elements)
			{
				if (!NeedToSkip(proceededNodes, element2) && element2.NavigationPosition.x > node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.x)
				{
					result.Add(element2);
				}
			}
			break;
		}
		case UINavigationDirection.Down:
		{
			foreach (IUiNavigationElement element3 in elements)
			{
				if (!NeedToSkip(proceededNodes, element3) && element3.NavigationPosition.y < node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.y)
				{
					result.Add(element3);
				}
			}
			break;
		}
		case UINavigationDirection.Up:
		{
			foreach (IUiNavigationElement element4 in elements)
			{
				if (!NeedToSkip(proceededNodes, element4) && element4.NavigationPosition.y > node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.y)
				{
					result.Add(element4);
				}
			}
			break;
		}
		}
	}

	private IUiNavigationElement GetClosestElement(IUiNavigationManager uiNavigationManager, List<IUiNavigationElement> elements)
	{
		if (elements.Count == 0)
		{
			return null;
		}
		float num = float.MaxValue;
		Vector2 navigationPosition = uiNavigationManager.CurrentlySelectedElement.NavigationPosition;
		IUiNavigationElement result = null;
		foreach (IUiNavigationElement element in elements)
		{
			float sqrMagnitude = (element.NavigationPosition - navigationPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = element;
			}
		}
		return result;
	}
}
