using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class ControlsSettingsStateWithSelected : MainMenuState
{
	protected override bool SelectedFirst => true;

	public override MainStateTag StateTag => MainStateTag.ControlSettingsWithSelected;

	protected override string RootName => "ControlSettings";

	public ControlsSettingsStateWithSelected(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
