using SM.Gamepad;
using Script.GUI.SMNavigation.States.PopupStates;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class PickItemToRefreshOrConsumeState : PickItemSlotState
{
	public override PopupStateTag StateTag => PopupStateTag.PickItemToRefreshOrConsume;

	protected override string RootName => "PickItemToRefreshOrConsume";

	public PickItemToRefreshOrConsumeState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
