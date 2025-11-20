using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates;

public class RewardState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.Reward;

	public RewardState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_navigationManager.DeselectAll();
		CameraController.s_CameraController.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		CameraController.s_CameraController.FreeDisableCameraInput(this);
	}
}
