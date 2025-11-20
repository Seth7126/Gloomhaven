using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationConditionalHandler : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private UiNavigationCondition _condition;

	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _onTrueHandler;

	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _onFalseHandler;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		if (_condition.IsTrue(proceededNodes, inNode, navigationDirection))
		{
			return _onTrueHandler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
		}
		return _onFalseHandler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
	}
}
