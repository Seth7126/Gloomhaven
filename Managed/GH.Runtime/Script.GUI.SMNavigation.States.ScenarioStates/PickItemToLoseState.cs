using SM.Gamepad;
using Script.GUI.SMNavigation.States.PopupStates;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class PickItemToLoseState : PickItemSlotState
{
	public override PopupStateTag StateTag => PopupStateTag.PickItemToLose;

	protected override string RootName => "PickItemToLose";

	public PickItemToLoseState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
