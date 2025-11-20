using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public class ListUiNavigationHandler : UiNavigationHandler
{
	[Flags]
	public enum ListLoopingTypes
	{
		FirstToLast = 1,
		LastToFirst = 2
	}

	public enum ListNavigationMode
	{
		Horizontal,
		Vertical
	}

	private int _pickedElementIndex = -1;

	protected new ListUiNavigationGroup Group;

	public ListUiNavigationHandler(UiNavigationRoot root, UiNavigationGroup group)
		: base(root, group)
	{
		Group = group as ListUiNavigationGroup;
	}

	public override bool TryPerformNavigation(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection, IUiNavigationSelectable uiNavigationSelectable)
	{
		if (handledNodes.Contains(Group))
		{
			_pickedElementIndex = -1;
			return false;
		}
		int num = Group.Elements.IndexOf(Group.UiNavigationManager.CurrentlySelectedElement);
		if (num != -1)
		{
			switch (Group.NavigationMode)
			{
			case ListNavigationMode.Horizontal:
				if (handledNodes.Count == 0 && (navigationDirection == UINavigationDirection.Down || navigationDirection == UINavigationDirection.Up))
				{
					handledNodes.Add(Group);
					return false;
				}
				break;
			case ListNavigationMode.Vertical:
				if (handledNodes.Count == 0 && (navigationDirection == UINavigationDirection.Left || navigationDirection == UINavigationDirection.Right))
				{
					handledNodes.Add(Group);
					return false;
				}
				break;
			}
		}
		if (uiNavigationSelectable != null)
		{
			_pickedElementIndex = Group.Elements.IndexOf(uiNavigationSelectable);
			if (_pickedElementIndex == -1)
			{
				throw new Exception("Navigation selectable named " + uiNavigationSelectable.NavigationName + " doesn't belong to " + Group.NavigationName);
			}
			return Group.UiNavigationManager.TrySelectIn(Group, uiNavigationSelectable);
		}
		if (handledNodes.Count != 0 && _pickedElementIndex != -1)
		{
			return Select(handledNodes, Group.Elements[_pickedElementIndex], navigationDirection);
		}
		_pickedElementIndex = num;
		if (_pickedElementIndex == -1)
		{
			_pickedElementIndex = 0;
			return Group.UiNavigationManager.TrySelectFirstIn(Group);
		}
		if (Group.NavigationMode == ListNavigationMode.Vertical)
		{
			if (navigationDirection != UINavigationDirection.Up)
			{
				return MovePositive(handledNodes, ref _pickedElementIndex, navigationDirection);
			}
			return MoveNegative(handledNodes, ref _pickedElementIndex, navigationDirection);
		}
		if (navigationDirection != UINavigationDirection.Left)
		{
			return MovePositive(handledNodes, ref _pickedElementIndex, navigationDirection);
		}
		return MoveNegative(handledNodes, ref _pickedElementIndex, navigationDirection);
	}

	private bool MoveNegative(HashSet<IUiNavigationNode> handledNodes, ref int index, UINavigationDirection navigationDirection)
	{
		bool flag = (Group.LoopingTypes & ListLoopingTypes.FirstToLast) != 0;
		int num = index - 1;
		bool flag2 = false;
		while (!flag2)
		{
			if (num < 0)
			{
				num = (flag ? (Group.Elements.Count - 1) : 0);
			}
			if (num >= 0 && !handledNodes.Contains(Group.Elements[num] as IUiNavigationNode))
			{
				flag2 = true;
			}
			else
			{
				num--;
			}
			if (num < 0 || num == index)
			{
				break;
			}
		}
		if (!flag2)
		{
			return false;
		}
		if (num < 0 && !flag)
		{
			return false;
		}
		if (num < 0 && flag)
		{
			num = Group.Elements.Count - 1;
		}
		index = num;
		return Select(handledNodes, Group.Elements[num], navigationDirection);
	}

	private bool MovePositive(HashSet<IUiNavigationNode> handledNodes, ref int index, UINavigationDirection navigationDirection)
	{
		bool flag = (Group.LoopingTypes & ListLoopingTypes.LastToFirst) != 0;
		int num = index + 1;
		bool flag2 = false;
		while (!flag2)
		{
			if (num == Group.Elements.Count)
			{
				num = ((!flag) ? (Group.Elements.Count - 1) : 0);
			}
			if (num < Group.Elements.Count && !handledNodes.Contains(Group.Elements[num] as IUiNavigationNode))
			{
				flag2 = true;
			}
			else
			{
				num++;
			}
			if (num == Group.Elements.Count || num == index)
			{
				break;
			}
		}
		if (!flag2)
		{
			return false;
		}
		if (num >= Group.Elements.Count && !flag)
		{
			return false;
		}
		if (num >= Group.Elements.Count && flag)
		{
			num = 0;
		}
		index = num;
		return Select(handledNodes, Group.Elements[num], navigationDirection);
	}
}
