using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class ModeSelectionState : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.ModeSelection;

	protected override bool SelectedFirst => true;

	protected override string RootName => "ModeSelection";

	public ModeSelectionState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
