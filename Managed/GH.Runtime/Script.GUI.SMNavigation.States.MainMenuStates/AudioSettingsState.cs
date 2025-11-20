using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class AudioSettingsState : MainMenuState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[5]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.CONTROL_COMBAT_LOG
	};

	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.AudioSettings;

	protected override string RootName => "AudioSettings";

	public AudioSettingsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, _actionsToDisable);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		InputManager.RequestEnableInput(this, _actionsToDisable);
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}
}
