using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class GuildMasterTutorial : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.GuildMasterTutorial;

	protected override bool SelectedFirst => false;

	protected override string RootName => "GuildMasterTutorial";

	public GuildMasterTutorial(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
