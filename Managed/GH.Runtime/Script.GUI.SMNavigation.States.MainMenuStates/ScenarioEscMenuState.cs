using Code.State;
using SM.Gamepad;
using UnityEngine.EventSystems;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class ScenarioEscMenuState : MainMenuState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[5]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.CONTROL_COMBAT_LOG
	};

	public override MainStateTag StateTag => MainStateTag.ScenarioEscMenu;

	protected override bool SelectedFirst => false;

	protected override string RootName => "ScenarioEscMenu";

	public ScenarioEscMenuState(StateMachine stateMachine, UiNavigationManager navigationManager)
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
		EventSystem.current.SetSelectedGameObject(null);
		_navigationManager.DeselectAll();
		InputManager.RequestEnableInput(this, _actionsToDisable);
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}
}
