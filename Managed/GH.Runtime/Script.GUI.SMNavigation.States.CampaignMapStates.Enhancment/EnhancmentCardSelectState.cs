using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;

public class EnhancmentCardSelectState : CampaignMapState
{
	private IHotkeySession _hotkeySession;

	private SessionHotkey _selectHotkey;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.EnhancmentCardSelect;

	protected override bool SelectedFirst => false;

	protected override string RootName => "EnhancmentCards";

	private UINewEnhancementWindow EnhancementWindow => Singleton<UIGuildmasterHUD>.Instance.EnhancementWindow;

	public EnhancmentCardSelectState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		base.Enter(stateProvider, payload);
		NewPartyDisplayUI.PartyDisplay.SelectCurrentCharacter();
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		NewPartyDisplayUI.PartyDisplay.DisplayNextPrevHotkey(value: true);
		NewPartyDisplayUI.PartyDisplay.EnhancementCardsDisplay.SetActiveControllerInputScroll(active: true);
		EnhancementWindow.EventOnHoveredCard += EnhancementWindowOnEventOnHoveredCard;
		_hotkeySession = EnhancementWindow.CardSelectionHotkeys.GetSessionOrEmpty();
		_selectHotkey = _hotkeySession.GetHotkey("Select");
		EnhancementWindowOnEventOnHoveredCard(EnhancementWindow.SowingCard);
		SetActiveReturnToMap(value: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		SetActiveReturnToMap(value: false);
		EnhancementWindow.EventOnHoveredCard -= EnhancementWindowOnEventOnHoveredCard;
		NewPartyDisplayUI.PartyDisplay.DisplayNextPrevHotkey(value: false);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		NewPartyDisplayUI.PartyDisplay.DisplayNextPrevHotkey(value: false);
		NewPartyDisplayUI.PartyDisplay.EnhancementCardsDisplay.SetActiveControllerInputScroll(active: false);
		_hotkeySession.Dispose();
	}

	private void EnhancementWindowOnEventOnHoveredCard(AbilityCardUI card)
	{
		_selectHotkey.SetShown(card != null && EnhancementUtils.CanBeEnhanced(card));
	}
}
