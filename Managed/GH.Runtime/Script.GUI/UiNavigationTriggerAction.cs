using System.Collections.Generic;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.Events;

namespace Script.GUI;

public class UiNavigationTriggerAction : CustomizableNavigationHandlerBehaviour
{
	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _navigationHandler;

	[SerializeField]
	private UnityEvent _onSuccessfullyNavigationActions;

	[SerializeField]
	private UnityEvent _onFailedNavigationActions;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		bool num = _navigationHandler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
		if (num)
		{
			UnityEvent onSuccessfullyNavigationActions = _onSuccessfullyNavigationActions;
			if (onSuccessfullyNavigationActions != null)
			{
				onSuccessfullyNavigationActions.Invoke();
				return num;
			}
			return num;
		}
		UnityEvent onFailedNavigationActions = _onFailedNavigationActions;
		if (onFailedNavigationActions != null)
		{
			onFailedNavigationActions.Invoke();
			return num;
		}
		return num;
	}
}
