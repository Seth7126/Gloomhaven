using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class CombatLogSettingsStateWithSelected : MainMenuState
{
	protected override bool SelectedFirst => false;

	public override MainStateTag StateTag => MainStateTag.CombatLogSettingsWithSelected;

	protected override string RootName => "CombatLogSettingsWithSelected";

	public CombatLogSettingsStateWithSelected(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
