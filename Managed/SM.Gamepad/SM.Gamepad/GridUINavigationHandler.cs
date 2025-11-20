using System;
using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class GridUINavigationHandler : UiNavigationHandler
{
	[Flags]
	public enum LoopingTypes
	{
		HorizontalLooping = 1,
		VerticalLooping = 2
	}

	protected new GridUINavigationGroup Group;

	private IUiNavigationElement _currentlyPickedElement;

	public GridUINavigationHandler(UiNavigationRoot root, UiNavigationGroup group)
		: base(root, group)
	{
		Group = group as GridUINavigationGroup;
	}

	public override bool TryPerformNavigation(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection, IUiNavigationSelectable uiNavigationSelectable)
	{
		IUiNavigationManager uiNavigationManager = Group.UiNavigationManager;
		if (handledNodes.Contains(Group))
		{
			_currentlyPickedElement = null;
			return false;
		}
		if (_currentlyPickedElement == null)
		{
			if (!((uiNavigationSelectable != null) ? uiNavigationManager.TrySelectIn(Group, uiNavigationSelectable) : uiNavigationManager.TrySelectFirstIn(Group)))
			{
				handledNodes.Add(Group);
				_currentlyPickedElement = null;
				return false;
			}
			_currentlyPickedElement = uiNavigationManager.GetPreviouslyPickedIn(Group);
			return true;
		}
		if (_currentlyPickedElement == null)
		{
			handledNodes.Add(Group);
			_currentlyPickedElement = null;
			return false;
		}
		Transform transform = _currentlyPickedElement.GameObject.transform;
		int siblingIndex = transform.GetSiblingIndex();
		Transform parent = transform.parent;
		GetPositionByIndex(siblingIndex, out var x, out var y);
		UpdatePositionBasedOnNavigationDirection(navigationDirection, ref x, ref y);
		if (!HandleBoundsAndLooping(ref x, ref y))
		{
			handledNodes.Add(Group);
			_currentlyPickedElement = null;
			return false;
		}
		int indexByPosition = GetIndexByPosition(x, y);
		if (indexByPosition >= parent.childCount)
		{
			handledNodes.Add(Group);
			_currentlyPickedElement = null;
			return false;
		}
		if (!Select(handledNodes, parent.GetChild(indexByPosition), navigationDirection))
		{
			handledNodes.Add(Group);
			_currentlyPickedElement = null;
			return false;
		}
		_currentlyPickedElement = uiNavigationManager.GetPreviouslyPickedIn(Group);
		return true;
	}

	private static void UpdatePositionBasedOnNavigationDirection(UINavigationDirection navigationDirection, ref int x, ref int y)
	{
		switch (navigationDirection)
		{
		case UINavigationDirection.Left:
			x--;
			break;
		case UINavigationDirection.Right:
			x++;
			break;
		case UINavigationDirection.Up:
			y--;
			break;
		case UINavigationDirection.Down:
			y++;
			break;
		}
	}

	private bool HandleBoundsAndLooping(ref int x, ref int y)
	{
		if (x > Group.Columns - 1)
		{
			if ((Group.LoopingTypes & LoopingTypes.HorizontalLooping) != 0)
			{
				x = 0;
			}
			else if ((Group.LoopingTypes & LoopingTypes.HorizontalLooping) == 0)
			{
				return false;
			}
		}
		if (x < 0)
		{
			if ((Group.LoopingTypes & LoopingTypes.HorizontalLooping) != 0)
			{
				x = Group.Columns - 1;
			}
			else if ((Group.LoopingTypes & LoopingTypes.HorizontalLooping) == 0)
			{
				return false;
			}
		}
		if (y > Group.Rows - 1)
		{
			if ((Group.LoopingTypes & LoopingTypes.VerticalLooping) != 0)
			{
				y = 0;
			}
			else if ((Group.LoopingTypes & LoopingTypes.VerticalLooping) == 0)
			{
				return false;
			}
		}
		if (y < 0)
		{
			if ((Group.LoopingTypes & LoopingTypes.VerticalLooping) != 0)
			{
				y = Group.Rows - 1;
			}
			else if ((Group.LoopingTypes & LoopingTypes.VerticalLooping) == 0)
			{
				return false;
			}
		}
		return true;
	}

	private void GetPositionByIndex(int index, out int x, out int y)
	{
		x = index % Group.Columns;
		y = index / Group.Columns;
	}

	private int GetIndexByPosition(int x, int y)
	{
		return Group.Columns * y + x;
	}
}
