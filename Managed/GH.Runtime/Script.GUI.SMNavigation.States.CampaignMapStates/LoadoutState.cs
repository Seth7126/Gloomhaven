using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class LoadoutState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Loadout;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Loadout";

	public LoadoutState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		_navigationManager.DeselectAll();
		FirstOrCurrentNavigation();
		Singleton<UIQuestPopupManager>.Instance.TryFocusQuest();
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		Singleton<UIQuestPopupManager>.Instance.TryUnfocusQuest();
	}
}
