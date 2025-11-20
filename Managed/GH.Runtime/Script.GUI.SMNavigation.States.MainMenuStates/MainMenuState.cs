using System;
using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public abstract class MainMenuState : NavigationState<MainStateTag>
{
	protected StateMachine _stateMachine;

	protected UiNavigationManager _navigationManager;

	protected abstract bool SelectedFirst { get; }

	protected abstract string RootName { get; }

	public MainMenuState()
	{
	}

	protected MainMenuState(StateMachine stateMachine, UiNavigationManager navigationManager)
	{
		_stateMachine = stateMachine;
		_navigationManager = navigationManager;
	}

	public void Construct(StateMachine stateMachine, UiNavigationManager navigationManager)
	{
		_stateMachine = stateMachine;
		_navigationManager = navigationManager;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		if (payload != null)
		{
			if (!(payload is MainStateData data))
			{
				throw new Exception();
			}
			EnterWithMainStateData(stateProvider, data);
		}
		else
		{
			SetNavigationRootWithoutStateData();
		}
	}

	private void SetNavigationRootWithoutStateData()
	{
		if (string.IsNullOrEmpty(RootName))
		{
			_navigationManager.DeselectAll();
		}
		else
		{
			_navigationManager.SetCurrentRoot(RootName, SelectedFirst);
		}
	}

	public void EnterWithMainStateData(IStateProvider stateProvider, MainStateData data)
	{
		if (data.NavigationRoot == null)
		{
			SetNavigationRootWithoutStateData();
		}
		else
		{
			_navigationManager.SetCurrentRoot(data.NavigationRoot, SelectedFirst);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
	}
}
