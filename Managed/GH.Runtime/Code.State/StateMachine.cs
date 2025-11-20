#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using SM.Gamepad;
using SM.Utils;
using Script.GUI.SMNavigation.States;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.PopupStates;

namespace Code.State;

public class StateMachine : IStateProvider
{
	private readonly Dictionary<Type, Dictionary<Enum, IState>> _states = new Dictionary<Type, Dictionary<Enum, IState>>();

	private readonly List<IState> _stateHistory = new List<IState>();

	private IStateFilter _nonMenuFilter = new StateFilterByTagType<MainStateTag>().InverseFilter();

	private IStateFilter _simplifiedFilter = new StateFilterByType(typeof(GamepadDisconnectionBoxState), typeof(LevelMessageState)).InverseFilter();

	private IStateFilter _stateFilter;

	protected UiNavigationManager NavManager;

	public IState CurrentState
	{
		get
		{
			if (_stateHistory.Count > 0)
			{
				return _stateHistory[_stateHistory.Count - 1];
			}
			return null;
		}
	}

	public IState PreviousState
	{
		get
		{
			if (_stateHistory.Count > 1)
			{
				return _stateHistory[_stateHistory.Count - 2];
			}
			return null;
		}
	}

	public int HistoryCount => _stateHistory.Count;

	public event Action<IState> EventStateChanged;

	public StateMachine(UiNavigationManager navigationManager)
	{
		NavManager = navigationManager;
	}

	public virtual void Init()
	{
	}

	public virtual void Reset()
	{
		if (_stateHistory.Count > 0)
		{
			IState currentState = CurrentState;
			bool num = currentState is GamepadDisconnectionBoxState || currentState is SelectInputDeviceBoxState;
			if (!num)
			{
				CurrentState?.Exit<object>(null);
			}
			_stateHistory.RemoveAll((IState x) => !(x is GamepadDisconnectionBoxState) && !(x is SelectInputDeviceBoxState));
			if (!num)
			{
				this.EventStateChanged?.Invoke(CurrentState);
			}
		}
	}

	public void ExitCurrentState()
	{
		CurrentState?.Exit<object>(null);
		RemoveLast(CurrentState);
		this.EventStateChanged?.Invoke(CurrentState);
	}

	public void SetFilter(IStateFilter availableStates)
	{
		_stateFilter = availableStates;
	}

	public void RemoveFilter()
	{
		_stateFilter = null;
	}

	public void SwitchToState(IState newState)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (newState == CurrentState)
		{
			Debug.Log($"[UIStateMachine] already in {newState.GetType()} state");
			return;
		}
		if (newState == null)
		{
			throw new ArgumentNullException("newState");
		}
		if (_stateFilter != null && !_stateFilter.IsValid(newState))
		{
			Debug.Log($"[UIStateMachine] {newState.GetType()} is rejected by state filter");
			_stateHistory.Insert(_stateHistory.Count - 1, newState);
			return;
		}
		IState currentState = CurrentState;
		_stateHistory.Add(newState);
		currentState?.Exit<object>(this);
		LogUtils.Log($"[UIStateMachine] switch to {newState.GetType()} state FROM {currentState} state");
		newState.Enter<object>(this);
		this.EventStateChanged?.Invoke(CurrentState);
	}

	public void SwitchToState<TPayload>(IState newState, TPayload payload = null) where TPayload : class
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (newState == CurrentState)
		{
			if (newState == null)
			{
				Debug.Log("[UIStateMachine] already in root with no previous state");
			}
			else
			{
				Debug.Log($"[UIStateMachine] already in {newState.GetType()} state");
			}
			return;
		}
		if (newState == null)
		{
			throw new ArgumentNullException("newState");
		}
		if (_stateFilter != null && !_stateFilter.IsValid(newState))
		{
			Debug.Log($"[UIStateMachine] {newState.GetType()} is rejected by state filter");
			_stateHistory.Insert(_stateHistory.Count - 1, newState);
			return;
		}
		IState currentState = CurrentState;
		_stateHistory.Add(newState);
		currentState?.Exit(this, payload);
		LogUtils.Log($"[UIStateMachine] switch to {newState.GetType()} state FROM {currentState} state");
		newState.Enter(this, payload);
		this.EventStateChanged?.Invoke(CurrentState);
	}

	public void Enter<TTag>(TTag tag) where TTag : Enum
	{
		Enter<TTag, object>(tag);
	}

	public void Enter<TTag, TPayload>(TTag tag, TPayload payload = null) where TTag : Enum where TPayload : class
	{
		if (InputManager.GamePadInUse)
		{
			SwitchToState(GetState(tag), payload);
		}
	}

	public IState GetState<TTag>(TTag tag) where TTag : Enum
	{
		Type typeFromHandle = typeof(TTag);
		return GetCategory(typeFromHandle)[tag];
	}

	public bool IsCurrentState<TState>() where TState : IState
	{
		return CurrentState is TState;
	}

	public void ToPreviousState()
	{
		SwitchToState<object>(PreviousState);
	}

	public void ToPreviousState<TPayload>(TPayload payload) where TPayload : class
	{
		SwitchToState(PreviousState, payload);
	}

	public void ToPreviousState(params IStateFilter[] filters)
	{
		IState previousDifferentState = GetPreviousDifferentState(filters);
		if (previousDifferentState != null)
		{
			SwitchToState<object>(previousDifferentState);
		}
	}

	public void ToPreviousStateIfPossible()
	{
		if (PreviousState != null)
		{
			SwitchToState<object>(PreviousState);
		}
	}

	public void ToNonMenuPreviousState(IStateFilter filter = null, bool useSimplifiedFilter = true)
	{
		ToNonMenuPreviousState<object>(null, filter, useSimplifiedFilter);
	}

	public void ToNonMenuPreviousState<TPayload>(TPayload payload, IStateFilter filter = null, bool useSimplifiedFilter = true) where TPayload : class
	{
		ToPreviousDifferentState(payload, _nonMenuFilter, useSimplifiedFilter ? _simplifiedFilter : null, filter);
	}

	public void ToPreviousDifferentState<TPayload>(TPayload payload, params IStateFilter[] filters) where TPayload : class
	{
		IState previousDifferentState = GetPreviousDifferentState(filters);
		if (previousDifferentState != null)
		{
			SwitchToState(previousDifferentState, (object)payload);
		}
	}

	public void ToPreviousLatestState<TTag>(params TTag[] tags) where TTag : Enum
	{
		TTag latestState = GetLatestState(tags);
		Enter(latestState);
	}

	public void ToPreviousLatestState<TTag>(params IStateFilter[] filters) where TTag : Enum
	{
		ToPreviousLatestState<TTag, object>(null, filters);
	}

	public void ToPreviousLatestState<TTag, TPayload>(TPayload payload, params IStateFilter[] filters) where TTag : Enum where TPayload : class
	{
		TTag latestState = GetLatestState<TTag>(filters);
		Enter(latestState, payload);
	}

	public void SetState<TTagCategory>(NavigationState<TTagCategory> state) where TTagCategory : Enum
	{
		Type typeFromHandle = typeof(TTagCategory);
		GetCategory(typeFromHandle)[state.StateTag] = state;
	}

	private Dictionary<Enum, IState> GetCategory(Type categoryType)
	{
		if (_states.TryGetValue(categoryType, out var value))
		{
			return value;
		}
		_states[categoryType] = new Dictionary<Enum, IState>();
		return _states[categoryType];
	}

	public TTag GetLatestState<TTag>(params TTag[] tags) where TTag : Enum
	{
		return GetLatestState<TTag>(new IStateFilter[1]
		{
			new StateFilterByTag<TTag>(tags)
		});
	}

	public TTag GetLatestState<TTag>(params IStateFilter[] filters) where TTag : Enum
	{
		for (int num = _stateHistory.Count - 1; num >= 0; num--)
		{
			IState state = _stateHistory[num];
			if (IsValid(state, filters) && state is NavigationState<TTag> navigationState)
			{
				return navigationState.StateTag;
			}
		}
		return default(TTag);
	}

	private IState GetPreviousDifferentState(params IStateFilter[] filters)
	{
		for (int num = _stateHistory.Count - 2; num >= 0; num--)
		{
			IState state = _stateHistory[num];
			if (IsValid(state, filters) && _stateHistory[num] != CurrentState)
			{
				return _stateHistory[num];
			}
		}
		return null;
	}

	private bool IsValid(IState state, IStateFilter[] filters)
	{
		for (int i = 0; i < filters.Length; i++)
		{
			if (filters[i] != null && !filters[i].IsValid(state))
			{
				return false;
			}
		}
		return true;
	}

	public void RemovePreviousState()
	{
		if (_stateHistory.Count > 1)
		{
			_stateHistory.RemoveAt(_stateHistory.Count - 2);
		}
	}

	public void RemoveLast(IState state)
	{
		int num = _stateHistory.LastIndexOf(state);
		if (num >= 0)
		{
			_stateHistory.RemoveAt(num);
		}
	}
}
