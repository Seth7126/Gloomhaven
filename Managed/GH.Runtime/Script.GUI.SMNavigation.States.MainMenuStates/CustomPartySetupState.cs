using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class CustomPartySetupState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.CustomPartySetup;

	protected override bool SelectedFirst => true;

	protected override string RootName => "CustomPartySetup";

	public CustomPartySetupState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
