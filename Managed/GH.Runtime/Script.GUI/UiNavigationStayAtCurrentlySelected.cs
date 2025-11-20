using System.Collections.Generic;
using SM.Gamepad;

namespace Script.GUI;

public class UiNavigationStayAtCurrentlySelected : CustomizableNavigationHandlerBehaviour
{
	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		return false;
	}
}
