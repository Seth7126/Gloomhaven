using System.Collections.Generic;

namespace SM.Gamepad;

public class UiNavigationSimpleOuterNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return inNode.Parent.TryToNavigate(proceededNodes, navigationDirection);
	}
}
