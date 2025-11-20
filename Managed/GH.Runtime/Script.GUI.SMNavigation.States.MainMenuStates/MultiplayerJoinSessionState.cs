using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MultiplayerJoinSessionState : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.MultiplayerJoinSession;

	protected override string RootName => "MultiplayerJoinSession";

	public MultiplayerJoinSessionState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
