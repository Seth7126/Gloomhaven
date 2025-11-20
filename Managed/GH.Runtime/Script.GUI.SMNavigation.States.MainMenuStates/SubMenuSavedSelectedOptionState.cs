using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class SubMenuSavedSelectedOptionState : SubMenuOptionsState
{
	public override MainStateTag StateTag => MainStateTag.SubMenuOptionsWithSelected;

	protected override bool SelectedFirst => false;

	public SubMenuSavedSelectedOptionState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
