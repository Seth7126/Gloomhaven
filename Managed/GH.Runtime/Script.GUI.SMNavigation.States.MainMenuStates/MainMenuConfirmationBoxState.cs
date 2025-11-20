using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MainMenuConfirmationBoxState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.ConfirmationBox;

	protected override bool SelectedFirst => false;

	protected override string RootName => "ConfirmationBox";

	public MainMenuConfirmationBoxState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
