using System;
using System.Collections.Generic;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

namespace Script.GUI;

public class UiNavigationStateSeparator : CustomizableNavigationHandlerBehaviour
{
	[Serializable]
	public class StateHolder
	{
		[SerializeField]
		private List<string> _states;

		[SerializeField]
		private CustomizableNavigationHandlerBehaviour _handler;

		public List<string> States => _states;

		public CustomizableNavigationHandlerBehaviour Handler => _handler;
	}

	[SerializeField]
	private StateHolder[] _stateHolders;

	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _anyOtherStateHandler;

	public override bool TryHandleNavigation(HashSet<IUiNavigationNode> proceededNodes, IUiNavigationNode inNode, UINavigationDirection navigationDirection)
	{
		bool flag = false;
		string item = Singleton<UINavigation>.Instance.StateMachine.CurrentState.GetType().Name;
		StateHolder[] stateHolders = _stateHolders;
		foreach (StateHolder stateHolder in stateHolders)
		{
			if (stateHolder.States.Contains(item))
			{
				flag = stateHolder.Handler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			proceededNodes.Add(inNode);
			return _anyOtherStateHandler.TryHandleNavigation(proceededNodes, inNode, navigationDirection);
		}
		return true;
	}
}
