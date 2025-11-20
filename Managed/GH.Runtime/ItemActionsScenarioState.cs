using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates;

public class ItemActionsScenarioState : ScenarioState
{
	public override ScenarioStateTag StateTag => ScenarioStateTag.ItemActions;

	public ItemActionsScenarioState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIUseItemsBar>.Instance.SetActiveItemButtons(value: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIUseItemsBar>.Instance.SetActiveItemButtons(value: false);
	}
}
