using System.Collections.Generic;

namespace SM.Gamepad;

public interface ICustomizableNavigationHandler
{
	bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededHandlers, IUiNavigationNode inNode, UINavigationDirection navigationDirection);
}
