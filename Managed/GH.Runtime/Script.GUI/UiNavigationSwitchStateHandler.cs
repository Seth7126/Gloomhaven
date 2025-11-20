using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.Events;

namespace Script.GUI;

public class UiNavigationSwitchStateHandler<T> : CustomizableNavigationHandlerBehaviour where T : Enum
{
	[SerializeField]
	protected T _stateTag;

	[SerializeField]
	protected bool _toPreviousState;

	[SerializeField]
	protected bool _selectPreviousAfterStateEnter;

	[SerializeField]
	protected UnityEvent _onStartHandleNavigation;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		_onStartHandleNavigation?.Invoke();
		if (_toPreviousState)
		{
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(new SelectionStateData(null, selectFirst: false, _selectPreviousAfterStateEnter));
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(_stateTag);
		}
		return true;
	}
}
