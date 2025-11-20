using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class SignOutConfirmationBoxState : MainMenuState
{
	public const string RootNameString = "SignOutConfirmationBox";

	public override MainStateTag StateTag => MainStateTag.SignOutConfirmationBox;

	protected override bool SelectedFirst => true;

	protected override string RootName => "SignOutConfirmationBox";

	public SignOutConfirmationBoxState(StateMachine stateMachine, UiNavigationManager manager)
		: base(stateMachine, manager)
	{
	}
}
