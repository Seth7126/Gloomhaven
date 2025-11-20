using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationSimpleInnerNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	private const float DirectionModifier = 2f;

	[SerializeField]
	private bool includeNotInteractableSelectables;

	[SerializeField]
	private bool _takeIntoAccountDirection;

	private readonly List<IUiNavigationElement> _collectedElementsBuffer = new List<IUiNavigationElement>();

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (proceededNodes.Contains(inNode))
		{
			return false;
		}
		IUiNavigationManager uiNavigationManager = inNode.UiNavigationManager;
		CollectElementsByDirection(proceededNodes, inNode, navigationDirection, _collectedElementsBuffer);
		IUiNavigationElement closestElement = GetClosestElement(uiNavigationManager, _collectedElementsBuffer, navigationDirection);
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

	private bool CheckConditions(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationElement element)
	{
		if (element.GameObject.activeInHierarchy)
		{
			if (element is IUiNavigationNode item && !proceededNodes.Contains(item))
			{
				return true;
			}
			if (element is IUiNavigationSelectable uiNavigationSelectable)
			{
				if (!includeNotInteractableSelectables)
				{
					return uiNavigationSelectable.ControlledSelectable.interactable;
				}
				return true;
			}
		}
		return false;
	}

	private void CollectElementsByDirection(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode node, UINavigationDirection navigationDirection, List<IUiNavigationElement> result)
	{
		result.Clear();
		switch (navigationDirection)
		{
		case UINavigationDirection.Left:
		{
			foreach (IUiNavigationElement element in node.Elements)
			{
				if (CheckConditions(proceededNodes, element) && element.NavigationPosition.x < node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.x)
				{
					result.Add(element);
				}
			}
			break;
		}
		case UINavigationDirection.Right:
		{
			foreach (IUiNavigationElement element2 in node.Elements)
			{
				if (CheckConditions(proceededNodes, element2) && element2.NavigationPosition.x > node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.x)
				{
					result.Add(element2);
				}
			}
			break;
		}
		case UINavigationDirection.Down:
		{
			foreach (IUiNavigationElement element3 in node.Elements)
			{
				if (CheckConditions(proceededNodes, element3) && element3.NavigationPosition.y < node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.y)
				{
					result.Add(element3);
				}
			}
			break;
		}
		case UINavigationDirection.Up:
		{
			foreach (IUiNavigationElement element4 in node.Elements)
			{
				if (CheckConditions(proceededNodes, element4) && element4.NavigationPosition.y > node.UiNavigationManager.CurrentlySelectedElement.NavigationPosition.y)
				{
					result.Add(element4);
				}
			}
			break;
		}
		}
	}

	private IUiNavigationElement GetClosestElement(IUiNavigationManager uiNavigationManager, List<IUiNavigationElement> elements, UINavigationDirection navigationDirection)
	{
		return GetClosestElementStatic(elements, uiNavigationManager.CurrentlySelectedElement, navigationDirection, _takeIntoAccountDirection);
	}

	public static IUiNavigationElement GetClosestElementStatic(List<IUiNavigationElement> elements, IUiNavigationSelectable navigationSelectable, UINavigationDirection navigationDirection, bool takeIntoAccountDirection)
	{
		if (elements.Count == 0)
		{
			return null;
		}
		float num = float.MaxValue;
		IUiNavigationElement result = null;
		Vector2 navigationPosition = navigationSelectable.NavigationPosition;
		foreach (IUiNavigationElement element in elements)
		{
			Vector2 vector = element.NavigationPosition - navigationPosition;
			if (takeIntoAccountDirection)
			{
				switch (navigationDirection)
				{
				case UINavigationDirection.Up:
				case UINavigationDirection.Down:
					vector.y *= 2f;
					break;
				case UINavigationDirection.Left:
				case UINavigationDirection.Right:
					vector.x *= 2f;
					break;
				}
			}
			float sqrMagnitude = vector.sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = element;
			}
		}
		return result;
	}
}
