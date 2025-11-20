using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MenuInfoState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.MenuInfo;

	protected override bool SelectedFirst => true;

	protected override string RootName => "";

	public MenuInfoState(NavigationStateMachine navigationStateMachine, UiNavigationManager navManager)
	{
		_navigationManager = navManager;
		_stateMachine = navigationStateMachine;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_navigationManager.DeselectAll();
	}
}
