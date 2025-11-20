using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class HouseRulesState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.HouseRules;

	protected override bool SelectedFirst => false;

	protected override string RootName => "HouseRules";

	public HouseRulesState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, new KeyAction[5]
		{
			KeyAction.UI_PREVIOUS_TAB,
			KeyAction.UI_NEXT_TAB,
			KeyAction.NEXT_SHIELD_TAB,
			KeyAction.PREVIOUS_SHIELD_TAB,
			KeyAction.UI_INFO
		});
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		InputManager.RequestEnableInput(this, new KeyAction[5]
		{
			KeyAction.UI_PREVIOUS_TAB,
			KeyAction.UI_NEXT_TAB,
			KeyAction.NEXT_SHIELD_TAB,
			KeyAction.PREVIOUS_SHIELD_TAB,
			KeyAction.UI_INFO
		});
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}
}
