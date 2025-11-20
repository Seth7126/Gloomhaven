using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class GamepadDisconnectionBoxState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.GamepadDisconnectionBox;

	protected override bool SelectedFirst => true;

	protected override string RootName => "GamepadDisconnectionBox";

	public GamepadDisconnectionBoxState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
