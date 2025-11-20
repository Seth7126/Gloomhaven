using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class SelectAbilityInfusionElementState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.SelectAbilityInfusion;

	public SelectAbilityInfusionElementState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
	}
}
