using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationRedirectionHandler : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _redirectTo;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return _redirectTo.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
	}
}
