using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class VisualKeyboardJoinSession : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.VisualKeyboardMultiplayer;

	protected override string RootName => "VisualKeyboardJoinSession";

	public VisualKeyboardJoinSession(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
