using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class GloomhavenTravelCityState : TravelMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.GloomhavenTravelCityState;

	protected override bool SelectedFirst => true;

	protected override string RootName => "AdventureMap";

	protected override bool NeedQuestPopup => false;

	protected override bool NeedPartyUI => false;

	public GloomhavenTravelCityState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}
}
