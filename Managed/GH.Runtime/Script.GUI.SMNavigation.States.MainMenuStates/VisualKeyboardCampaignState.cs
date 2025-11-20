using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class VisualKeyboardCampaignState : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.VisualKeyboardCampaign;

	protected override string RootName => "VisualKeyboardCampaign";

	public VisualKeyboardCampaignState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
