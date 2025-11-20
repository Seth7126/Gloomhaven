using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class SelectDLCState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.SelectDLC;

	protected override bool SelectedFirst => true;

	protected override string RootName => "SelectDLC";

	public SelectDLCState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.Scenario);
	}
}
