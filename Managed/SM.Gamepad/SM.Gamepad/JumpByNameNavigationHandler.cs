using System;
using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class JumpByNameNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	[Serializable]
	private class DirNameContainer
	{
		public UINavigationDirection _direction;

		public string _name;
	}

	[SerializeField]
	private DirNameContainer[] _dirNameContainers;

	[SerializeField]
	private bool SelectFirst;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededHandlers, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		DirNameContainer[] dirNameContainers = _dirNameContainers;
		foreach (DirNameContainer dirNameContainer in dirNameContainers)
		{
			if (dirNameContainer._direction == UINavigationDirection.Any || dirNameContainer._direction == navigationDirection)
			{
				return TryJump(inNode.UiNavigationManager, dirNameContainer._name);
			}
		}
		return false;
	}

	private bool TryJump(IUiNavigationManager manager, string jumpToName)
	{
		IUiNavigationElement uiNavigationElement = manager.ElementByName(jumpToName);
		if (!(uiNavigationElement is IUiNavigationSelectable uiNavigationSelectable))
		{
			if (uiNavigationElement is IUiNavigationNode navigationNode)
			{
				if (SelectFirst)
				{
					return manager.TrySelectFirstIn(navigationNode);
				}
				return manager.TrySelectPreviousIn(navigationNode);
			}
			return false;
		}
		return manager.TrySelect(uiNavigationSelectable);
	}
}
