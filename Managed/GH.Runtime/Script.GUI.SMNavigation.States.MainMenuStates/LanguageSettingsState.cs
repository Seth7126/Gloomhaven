using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class LanguageSettingsState : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.LanguageSettings;

	protected override string RootName => "LanguageSettings";

	public LanguageSettingsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, new KeyAction[4]
		{
			KeyAction.UI_PREVIOUS_TAB,
			KeyAction.UI_NEXT_TAB,
			KeyAction.NEXT_SHIELD_TAB,
			KeyAction.PREVIOUS_SHIELD_TAB
		});
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		InputManager.RequestEnableInput(this, new KeyAction[4]
		{
			KeyAction.UI_PREVIOUS_TAB,
			KeyAction.UI_NEXT_TAB,
			KeyAction.NEXT_SHIELD_TAB,
			KeyAction.PREVIOUS_SHIELD_TAB
		});
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}
}
