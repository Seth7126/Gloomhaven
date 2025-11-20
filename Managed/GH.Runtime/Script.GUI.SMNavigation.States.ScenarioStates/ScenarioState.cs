using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public abstract class ScenarioState : NavigationState<ScenarioStateTag>
{
	protected readonly StateMachine _stateMachine;

	protected readonly UiNavigationManager _navigationManager;

	protected ScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
	{
		_stateMachine = stateMachine;
		_navigationManager = navigationManager;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
