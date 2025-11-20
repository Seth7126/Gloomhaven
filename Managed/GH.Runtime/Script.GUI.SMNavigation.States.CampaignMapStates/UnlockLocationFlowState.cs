using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class UnlockLocationFlowState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.UnlockLocationFlow;

	protected override bool SelectedFirst => true;

	protected override string RootName => "UnlockLocationFlow";

	public UnlockLocationFlowState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
