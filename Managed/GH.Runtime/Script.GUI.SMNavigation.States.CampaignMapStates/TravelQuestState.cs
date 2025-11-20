using System;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class TravelQuestState : TravelMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.TravelQuestState;

	protected override bool SelectedFirst => true;

	protected override string RootName => "AdventureMap";

	protected override bool NeedQuestPopup => true;

	protected override bool NeedPartyUI => true;

	protected CampaignMapStateTag[] TagsFilter { get; }

	public TravelQuestState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		TagsFilter = Array.Empty<CampaignMapStateTag>();
	}
}
