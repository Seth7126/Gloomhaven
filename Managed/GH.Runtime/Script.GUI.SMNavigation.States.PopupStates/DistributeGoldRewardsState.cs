using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.PopupStates;

public class DistributeGoldRewardsState : PopupState
{
	public override PopupStateTag StateTag => PopupStateTag.DistributeGoldRewards;

	protected override string RootName => "DistributeGoldRewards";

	public DistributeGoldRewardsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
