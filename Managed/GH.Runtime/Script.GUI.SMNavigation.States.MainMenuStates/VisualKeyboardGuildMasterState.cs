using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class VisualKeyboardGuildMasterState : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.VisualKeyboardGuildMaster;

	protected override string RootName => "VisualKeyboardGuildMaster";

	public VisualKeyboardGuildMasterState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
