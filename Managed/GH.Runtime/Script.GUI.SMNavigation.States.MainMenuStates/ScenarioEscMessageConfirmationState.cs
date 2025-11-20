using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class ScenarioEscMessageConfirmationState : MainMenuState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[7]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.UI_PAUSE,
		KeyAction.CONTROL_COMBAT_LOG,
		KeyAction.UI_INFO
	};

	public override MainStateTag StateTag => MainStateTag.MessageConfirmationEscMenu;

	protected override bool SelectedFirst => false;

	protected override string RootName => "MessageConfirmationEscMenu";

	public ScenarioEscMessageConfirmationState(StateMachine stateMachine, UiNavigationManager navigationManager)
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
