using System;
using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationDirectionSeparator : CustomizableNavigationHandlerBehaviour
{
	[Serializable]
	public class DirectionsHolder
	{
		[SerializeField]
		private List<UINavigationDirection> _directions;

		[SerializeField]
		private CustomizableNavigationHandlerBehaviour _handler;

		public List<UINavigationDirection> Directions => _directions;

		public CustomizableNavigationHandlerBehaviour Handler => _handler;
	}

	[SerializeField]
	private DirectionsHolder[] _directionsHolders;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		bool flag = false;
		DirectionsHolder[] directionsHolders = _directionsHolders;
		foreach (DirectionsHolder directionsHolder in directionsHolders)
		{
			if (directionsHolder.Directions.Contains(navigationDirection))
			{
				flag = directionsHolder.Handler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			proceededNodes.Add(inNode);
		}
		return flag;
	}
}
