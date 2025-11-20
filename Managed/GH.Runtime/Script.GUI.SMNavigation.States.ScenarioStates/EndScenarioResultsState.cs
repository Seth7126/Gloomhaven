using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class EndScenarioResultsState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.EndResults;

	public EndScenarioResultsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		UINewAdventureResultsManager.Implementation.DisableStatsPanelBlocker();
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		UINewAdventureResultsManager.Implementation.ActiveStatsPanelBlocker();
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}
}
