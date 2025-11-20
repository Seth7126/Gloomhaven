using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MultiplayerHowToState : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.MultiplayerHowTo;

	protected override string RootName => "MultiplayerHowTo";

	public MultiplayerHowToState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		InputManager.DisableMoveEvents();
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Exit(stateProvider, payload);
		InputManager.EnableMoveEvents();
	}
}
