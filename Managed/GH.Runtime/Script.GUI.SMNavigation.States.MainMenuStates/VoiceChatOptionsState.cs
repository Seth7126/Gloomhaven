using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class VoiceChatOptionsState : MainMenuState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[9]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.NEXT_MERCENARY_OPTION,
		KeyAction.CREATE_NEW_CHARACTER,
		KeyAction.CONTROL_LOCAL_OPTIONS_LEFT,
		KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT,
		KeyAction.UI_NEXT_TAB_MERCENARY
	};

	public override MainStateTag StateTag => MainStateTag.VoiceChatOptions;

	protected override bool SelectedFirst => true;

	protected override string RootName => "VoiceChatOptionsRoot";

	public VoiceChatOptionsState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		InputManager.RequestDisableInput(this, _actionsToDisable);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		InputManager.RequestEnableInput(this, _actionsToDisable);
	}
}
