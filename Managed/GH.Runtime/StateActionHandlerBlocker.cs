using System;
using System.Collections.Generic;
using Code.State;
using Script.GUI.SMNavigation;

public class StateActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly ControllerInputAreaLocal _area;

	private readonly HashSet<IState> _states;

	private bool _isBlocked;

	private bool _invert;

	public bool IsBlock => _states.Contains(Singleton<UINavigation>.Instance.StateMachine.CurrentState) ^ _invert;

	public event Action BlockStateChanged;

	public StateActionHandlerBlocker(HashSet<IState> states, bool invert = false)
	{
		_invert = invert;
		_states = states;
		_isBlocked = IsBlock;
		Singleton<UINavigation>.Instance.StateMachine.EventStateChanged += OnStateChanged;
	}

	private void OnStateChanged(IState obj)
	{
		bool isBlock = IsBlock;
		if (_isBlocked != IsBlock)
		{
			_isBlocked = isBlock;
			this.BlockStateChanged?.Invoke();
		}
	}

	public void Clear()
	{
		if (Singleton<UINavigation>.Instance != null)
		{
			Singleton<UINavigation>.Instance.StateMachine.EventStateChanged -= OnStateChanged;
		}
		this.BlockStateChanged = null;
	}
}
