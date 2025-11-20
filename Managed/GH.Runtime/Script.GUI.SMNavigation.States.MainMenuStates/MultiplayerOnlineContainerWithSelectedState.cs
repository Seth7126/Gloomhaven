using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class MultiplayerOnlineContainerWithSelectedState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.MultiplayerOnlineContainerWithSelected;

	protected override bool SelectedFirst => false;

	protected override string RootName => "MultiplayerOnlineContainer";

	public MultiplayerOnlineContainerWithSelectedState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
