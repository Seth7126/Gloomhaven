using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationSimpleJumpNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private UINavigationSelectable _jumpTo;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return inNode.UiNavigationManager.TrySelect(_jumpTo);
	}
}
