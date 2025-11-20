using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class MapStoryState : CampaignMapState
{
	private bool _isRegistered;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.MapStory;

	protected override bool SelectedFirst => true;

	protected override string RootName => "MapStory";

	public MapStoryState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_isRegistered = NewPartyDisplayUI.PartyDisplay.TabInput.IsRegistered;
		if (_isRegistered)
		{
			NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
		}
		Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: false);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		if (_isRegistered)
		{
			NewPartyDisplayUI.PartyDisplay.TabInput.Register();
		}
		Singleton<UIGuildmasterHUD>.Instance.EnableShieldInput(active: true);
	}
}
