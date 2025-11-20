using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class EscMenuHouseRulesSelect : MainMenuState
{
	public override MainStateTag StateTag => MainStateTag.EscMenuHouseRulesSelect;

	protected override bool SelectedFirst => true;

	protected override string RootName => "EscMenuHouseRulesSelect";

	public EscMenuHouseRulesSelect(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
