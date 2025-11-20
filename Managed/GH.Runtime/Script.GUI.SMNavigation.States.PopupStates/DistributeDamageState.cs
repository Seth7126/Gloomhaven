using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class DistributeDamageState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.DistributeDamage;

	protected override string RootName => "DistributeDamage";

	public DistributeDamageState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		CameraController.s_CameraController.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		CameraController.s_CameraController.FreeDisableCameraInput(this);
	}
}
