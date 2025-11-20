using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiRedirectToTargetParentNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private UiNavigationBase _target;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (_target is IUiNavigationNode item)
		{
			proceededNodes.Add(item);
		}
		return _target.Parent.TryToNavigate(proceededNodes, navigationDirection);
	}
}
