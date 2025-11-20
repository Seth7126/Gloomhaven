using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.MainMenuStates;

public class DisplaySettingsStateWithSelected : MainMenuState
{
	protected override bool SelectedFirst => false;

	public override MainStateTag StateTag => MainStateTag.DisplaySettingsWithSelected;

	protected override string RootName
	{
		get
		{
			if (!global::PlatformLayer.Instance.IsConsole)
			{
				return "DisplaySettings";
			}
			return "PerformanceSettings";
		}
	}

	public DisplaySettingsStateWithSelected(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(stateMachine, navigationManager)
	{
	}
}
