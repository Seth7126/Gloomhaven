using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class VisualKeyboardState : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.VisualKeyboard;

	protected override string RootName => "VisualKeyboard";

	public VisualKeyboardState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
