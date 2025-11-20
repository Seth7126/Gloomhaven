using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class EscMenuMultiplayer : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.EscMenuMultiplayer;

	protected override bool SelectedFirst => true;

	protected override string RootName => "EscMenuMultiplayer";

	public EscMenuMultiplayer(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
