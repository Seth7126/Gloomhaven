using System.Collections.Generic;
using SM.Utils;
using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationSimpleCompositeNavigationHandler : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private List<CustomizableNavigationHandlerBehaviour> _composedHanlders;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (_composedHanlders.Count == 0)
		{
			LogUtils.LogError(string.Format("[{0}] Element {1}({2}) doesn't have any navigation handlers to compose {3}", "UiNavigationSimpleCompositeNavigationHandler", base.name, inNode.NavigationName, this));
		}
		int i;
		for (i = 0; i < _composedHanlders.Count && !_composedHanlders[i].TryHandleNavigation(proceededNodes, inNode, navigationDirection); i++)
		{
		}
		return i < _composedHanlders.Count;
	}
}
