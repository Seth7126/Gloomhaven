using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class DistributeItemsRewardsState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.DistributeItemsRewards;

	protected override string RootName => "DistributeItemsRewards";

	public DistributeItemsRewardsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
