using System.Collections.Generic;
using UnityEngine;

namespace SM.Gamepad;

public abstract class CustomizableNavigationHandlerBehaviour : MonoBehaviour, ICustomizableNavigationHandler
{
	public abstract bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection);
}
