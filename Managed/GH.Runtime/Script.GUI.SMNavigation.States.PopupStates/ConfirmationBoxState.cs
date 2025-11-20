using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class ConfirmationBoxState : PopupState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[11]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.NEXT_MERCENARY_OPTION,
		KeyAction.CREATE_NEW_CHARACTER,
		KeyAction.CONTROL_LOCAL_OPTIONS_LEFT,
		KeyAction.UI_NEXT_TAB_MERCENARY,
		KeyAction.UI_INFO,
		KeyAction.CONFIRM_ACTION_BUTTON,
		KeyAction.UI_PAUSE
	};

	public override PopupStateTag StateTag => PopupStateTag.ConfirmationBox;

	protected override string RootName => "ConfirmationBox";

	public ConfirmationBoxState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, _actionsToDisable);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		InputManager.RequestEnableInput(this, _actionsToDisable);
	}
}
