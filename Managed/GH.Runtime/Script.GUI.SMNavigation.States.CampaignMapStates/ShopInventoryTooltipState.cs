using System.Collections.Generic;
using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public sealed class ShopInventoryTooltipState : CampaignMapState
{
	private readonly List<HotkeyAction> _shopTooltipActionsList;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.ShopItemTooltip;

	protected override bool SelectedFirst => false;

	protected override string RootName => "ShopItemTooltip";

	public ShopInventoryTooltipState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		_shopTooltipActionsList = new List<HotkeyAction>
		{
			new HotkeyAction("Tips", delegate
			{
				Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleItemTextTooltip();
			}),
			new HotkeyAction("Switch_Left", delegate
			{
				NewPartyDisplayUI.PartyDisplay.SelectCurrentCharacter();
			})
		};
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		DefaultNavigation();
		Singleton<UIShopItemWindow>.Instance.ItemInventory.SetHotkeysActive(value: true);
		NewPartyDisplayUI.PartyDisplay.ToggleBackgroundElements(isActive: true);
		SetHotkeysAction(_shopTooltipActionsList);
		ToggleHotkey("Switch_Left", isActive: true);
		ToggleHotkey("Tips", isActive: true);
	}

	private void SetHotkeysAction(IEnumerable<HotkeyAction> hotkeyActions)
	{
		NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.SetHotkeysAction(hotkeyActions);
	}

	private void ToggleHotkey(string hotkey, bool isActive)
	{
		NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.SetActiveHotkey(hotkey, isActive);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		ToggleHotkey("Switch_Left", isActive: false);
		ToggleHotkey("Tips", isActive: false);
		Singleton<UIShopItemWindow>.Instance.ItemInventory.SetHotkeysActive(value: false);
		NewPartyDisplayUI.PartyDisplay.ToggleBackgroundElements(isActive: false);
		if (Singleton<UIShopItemWindow>.Instance.ItemInventory.TooltipShown)
		{
			Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleItemTextTooltip();
		}
		Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleForceHoveredCurrentItem(isForceHovered: false);
		_navigationManager.DeselectAll();
		if (Singleton<UIShopItemWindow>.Instance.ItemInventory.CurrentHoveredItemSlot != null)
		{
			Singleton<UIShopItemWindow>.Instance.ItemInventory.CurrentHoveredItemSlot.OnHovered(hovered: false);
		}
	}
}
