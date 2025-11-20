using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MapEscMenuState : MainMenuState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[8]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.NEXT_MERCENARY_OPTION,
		KeyAction.CREATE_NEW_CHARACTER,
		KeyAction.CONTROL_LOCAL_OPTIONS_LEFT,
		KeyAction.CONTROL_LOCAL_OPTIONS_RIGHT
	};

	public override MainStateTag StateTag => MainStateTag.MapEscMenu;

	protected override bool SelectedFirst => false;

	protected override string RootName => "MapEscMenu";

	public MapEscMenuState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		if (Singleton<UILoadoutManager>.Instance.IsOpen)
		{
			Singleton<UILoadoutManager>.Instance.SetActiveConfirmationButton(value: false);
		}
		InputManager.RequestDisableInput(this, KeyActionConstants.GamepadTriggersActions);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		if (Singleton<UILoadoutManager>.Instance.IsOpen)
		{
			Singleton<UILoadoutManager>.Instance.RefreshRequirements();
		}
		InputManager.RequestEnableInput(this, KeyActionConstants.GamepadTriggersActions);
	}
}
