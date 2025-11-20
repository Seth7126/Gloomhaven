using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class NavigationOverrider : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private string _overriderTag = string.Empty;

	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _navigationHandler;

	public string OverriderTag => _overriderTag;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return _navigationHandler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
	}
}
