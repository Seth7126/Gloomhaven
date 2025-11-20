using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class PartyInventoryTooltipState : CampaignMapState
{
	private CampaignMapStateTag _previousState;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.ItemTooltip;

	protected override bool SelectedFirst => true;

	protected override string RootName => "ItemTooltip";

	public PartyInventoryTooltipState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.DisableDarkenPanel();
		DefaultNavigation();
		_previousState = Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PreviousCampaignState;
		InitState(_previousState, isActive: true);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PartyItemsHotkeyContainer.SetHotkeyAction("Tips", Singleton<UIPartyCharacterEquipmentDisplay>.Instance.Inventory.ToggleItemTextTooltip);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PartyItemsHotkeyContainer.SetActiveHotkey("Tips", Singleton<UIPartyCharacterEquipmentDisplay>.Instance.Inventory.CanShowTooltip());
	}

	private void InitState(CampaignMapStateTag previousStateTag, bool isActive)
	{
		switch (previousStateTag)
		{
		case CampaignMapStateTag.Merchant:
			Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleFocusBackground(isActive);
			break;
		default:
			Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive);
			break;
		case CampaignMapStateTag.Temple:
			break;
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.Inventory.HideItemTextTooltip();
		InitState(_previousState, isActive: false);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.PartyItemsHotkeyContainer.SetActiveHotkey("Tips", value: false);
	}
}
