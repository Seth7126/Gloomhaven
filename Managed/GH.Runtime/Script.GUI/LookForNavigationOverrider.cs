using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class LookForNavigationOverrider : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private string OverriderTag = string.Empty;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		NavigationOverrider navigationOverrider = FindOverrider(inNode);
		if (navigationOverrider != null)
		{
			return navigationOverrider.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
		}
		return false;
	}

	private NavigationOverrider FindOverrider(IUiNavigationNode inNode)
	{
		NavigationOverrider navigationOverrider = null;
		while (inNode != null)
		{
			NavigationOverrider[] components = inNode.GameObject.GetComponents<NavigationOverrider>();
			foreach (NavigationOverrider navigationOverrider2 in components)
			{
				if (navigationOverrider2.OverriderTag == OverriderTag)
				{
					navigationOverrider = navigationOverrider2;
					break;
				}
			}
			if (navigationOverrider != null)
			{
				break;
			}
			inNode = inNode.Parent;
		}
		return navigationOverrider;
	}
}
