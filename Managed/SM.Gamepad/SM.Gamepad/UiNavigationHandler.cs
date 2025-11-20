using System;
using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationHandler
{
	public enum NavigationHandleType
	{
		PositionBasedNavigation,
		DirectionBasedNavigation,
		PriorityBasedNavigation
	}

	protected UiNavigationRoot Root;

	protected UiNavigationGroup Group;

	private UiNavigationManager _manager => Group.UiNavigationManager as UiNavigationManager;

	private UiNavigationGroup _currentGroup => _manager.CurrentlySelectedElement.Parent;

	private UINavigationSelectable _currentSelectable => _manager.CurrentlySelectedElement as UINavigationSelectable;

	public UiNavigationHandler(UiNavigationRoot root, UiNavigationGroup group)
	{
		Root = root;
		Group = group;
	}

	public virtual bool TryPerformNavigation(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection, IUiNavigationSelectable uiNavigationSelectable = null)
	{
		if (handledNodes.Contains(Group))
		{
			return false;
		}
		bool flag = false;
		switch (Group.InnerNavigationHandleType)
		{
		case NavigationHandleType.PositionBasedNavigation:
			flag = TryPerformDistanceBasedNavigation(handledNodes, Group, navigationDirection);
			break;
		case NavigationHandleType.DirectionBasedNavigation:
			flag = TryPerformDirectionBasedNavigation(handledNodes, Group, navigationDirection);
			break;
		case NavigationHandleType.PriorityBasedNavigation:
			flag = TryPerformPriorityBasedNavigation(handledNodes, Group, navigationDirection);
			break;
		}
		if (!flag)
		{
			handledNodes.Add(Group);
		}
		return flag;
	}

	protected virtual bool TryPerformDistanceBasedNavigation(HashSet<IUiNavigationNode> handledNodes, UiNavigationGroup inGroup, UINavigationDirection navigationDirection)
	{
		List<IUiNavigationElement> allElementsByPosition = GetAllElementsByPosition(handledNodes, inGroup, navigationDirection);
		IUiNavigationElement closestElement = GetClosestElement(allElementsByPosition, navigationDirection);
		return Select(handledNodes, closestElement, navigationDirection);
	}

	protected List<IUiNavigationElement> GetAllElementsByPosition(HashSet<IUiNavigationNode> handledNodes, UiNavigationGroup group, UINavigationDirection navigationDirection)
	{
		List<IUiNavigationElement> list = new List<IUiNavigationElement>();
		Vector2 navigationPosition = _currentSelectable.NavigationPosition;
		for (int i = 0; i < group.Elements.Count; i++)
		{
			IUiNavigationElement uiNavigationElement = group.Elements[i];
			if (uiNavigationElement == _currentSelectable || uiNavigationElement == _currentGroup || handledNodes.Contains(uiNavigationElement as IUiNavigationNode) || !uiNavigationElement.GameObject.activeInHierarchy)
			{
				continue;
			}
			Vector2 navigationPosition2 = uiNavigationElement.NavigationPosition;
			switch (navigationDirection)
			{
			case UINavigationDirection.Left:
				if (navigationPosition2.x < navigationPosition.x)
				{
					list.Add(uiNavigationElement);
				}
				break;
			case UINavigationDirection.Right:
				if (navigationPosition2.x > navigationPosition.x)
				{
					list.Add(uiNavigationElement);
				}
				break;
			case UINavigationDirection.Up:
				if (navigationPosition2.y > navigationPosition.y)
				{
					list.Add(uiNavigationElement);
				}
				break;
			case UINavigationDirection.Down:
				if (navigationPosition2.y < navigationPosition.y)
				{
					list.Add(uiNavigationElement);
				}
				break;
			}
		}
		return list;
	}

	protected IUiNavigationElement GetClosestElement(List<IUiNavigationElement> elements, UINavigationDirection navigationDirection)
	{
		return UiNavigationSimpleInnerNavigationHandler.GetClosestElementStatic(elements, _currentSelectable, navigationDirection, takeIntoAccountDirection: true);
	}

	protected virtual bool TryPerformDirectionBasedNavigation(HashSet<IUiNavigationNode> handledNodes, UiNavigationGroup inGroup, UINavigationDirection navigationDirection)
	{
		List<IUiNavigationElement> allElementsByPosition = GetAllElementsByPosition(handledNodes, inGroup, navigationDirection);
		IUiNavigationElement closestElementByDirection = GetClosestElementByDirection(allElementsByPosition, navigationDirection);
		return Select(handledNodes, closestElementByDirection, navigationDirection);
	}

	protected IUiNavigationElement GetClosestElementByDirection(List<IUiNavigationElement> elements, UINavigationDirection navigationDirection)
	{
		if (elements.Count == 0)
		{
			return null;
		}
		float num = float.MaxValue;
		Vector2 navigationPosition = _currentSelectable.NavigationPosition;
		IUiNavigationElement result = null;
		foreach (IUiNavigationElement element in elements)
		{
			Vector2 navigationPosition2 = element.NavigationPosition;
			float num2 = navigationDirection switch
			{
				UINavigationDirection.Down => navigationPosition.y - navigationPosition2.y, 
				UINavigationDirection.Up => navigationPosition.y + navigationPosition2.y, 
				UINavigationDirection.Left => navigationPosition.x - navigationPosition2.x, 
				UINavigationDirection.Right => navigationPosition.x + navigationPosition2.x, 
				_ => throw new ArgumentOutOfRangeException("navigationDirection", navigationDirection, null), 
			};
			if (num2 < num)
			{
				num = num2;
				result = element;
			}
		}
		return result;
	}

	protected virtual bool TryPerformPriorityBasedNavigation(HashSet<IUiNavigationNode> handledNodes, UiNavigationGroup inGroup, UINavigationDirection navigationDirection)
	{
		IUiNavigationElement uiNavigationElement = null;
		switch (navigationDirection)
		{
		case UINavigationDirection.Left:
		case UINavigationDirection.Up:
			uiNavigationElement = UiNavigationElementsUtils.GetNextWithHigherPriority(inGroup, _currentSelectable.Priority);
			break;
		case UINavigationDirection.Right:
		case UINavigationDirection.Down:
			uiNavigationElement = UiNavigationElementsUtils.GetNextWithLowerPriority(inGroup, _currentSelectable.Priority);
			break;
		}
		if (uiNavigationElement != null)
		{
			return Select(handledNodes, uiNavigationElement, navigationDirection);
		}
		return false;
	}

	protected bool Select(HashSet<IUiNavigationNode> handledNodes, Transform child, UINavigationDirection navigationDirection)
	{
		UiNavigationBase component = child.GetComponent<UiNavigationBase>();
		return Select(handledNodes, component, navigationDirection);
	}

	protected bool Select(HashSet<IUiNavigationNode> handledNodes, IUiNavigationElement newElement, UINavigationDirection navigationDirection)
	{
		if (!(newElement is UINavigationSelectable uiNavigationSelectable))
		{
			if (newElement is UiNavigationGroup uiNavigationGroup)
			{
				return uiNavigationGroup.OldFashionNavigation(handledNodes, navigationDirection);
			}
			return false;
		}
		return _manager.TrySelect(uiNavigationSelectable);
	}
}
