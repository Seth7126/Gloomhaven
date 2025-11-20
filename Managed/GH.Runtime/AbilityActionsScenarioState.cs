using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates;

public class AbilityActionsScenarioState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.AbilityActions;

	public AbilityActionsScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_navigationManager.DeselectCurrentSelectable();
		Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Unfocus();
	}
}
