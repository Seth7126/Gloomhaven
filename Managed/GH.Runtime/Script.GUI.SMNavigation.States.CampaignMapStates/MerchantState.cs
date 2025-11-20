using System.Collections.Generic;
using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class MerchantState : CampaignMapState
{
	private bool _oldHotkeysMercenariesState;

	private List<HotkeyAction> _merchantHotkeysActionsList = new List<HotkeyAction>
	{
		new HotkeyAction("Switch_Left", delegate
		{
			Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Unfocus();
			NewPartyDisplayUI.PartyDisplay.TrySelectCurrentCharacter();
			NewPartyDisplayUI.PartyDisplay.TryToggleEquipmentPanelForSelectedCharacter(isEnabled: true, focusIfToggled: true);
		}),
		new HotkeyAction("Tips", delegate
		{
			Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleItemTextTooltip();
		})
	};

	private SkipFrameKeyActionHandlerBlocker _skipFrameBlocker;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Merchant;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Merchant";

	public MerchantState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		FirstOrCurrentNavigation();
		NewPartyDisplayUI.PartyDisplay.ToggleBackgroundElements(isActive: true);
		_oldHotkeysMercenariesState = NewPartyDisplayUI.PartyDisplay.MercenariesHotkeysActiveState;
		NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
		Singleton<UIShopItemWindow>.Instance.ItemInventory.OnItemsRefreshed += ItemInventoryOnOnItemsRefreshed;
		Singleton<UIShopItemWindow>.Instance.ItemInventory.NewItemTooltipShown += HandleItemInventoryOnNewItemTooltipShown;
		if (_skipFrameBlocker == null)
		{
			_skipFrameBlocker = new SkipFrameKeyActionHandlerBlocker(Singleton<InputManager>.Instance);
		}
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_PREVIOUS_TAB, HandleNextCharacter).AddBlocker(_skipFrameBlocker));
		_skipFrameBlocker.Run();
		SetHotkeysAction(_merchantHotkeysActionsList);
		ToggleHotkey("Switch_Left", isActive: true);
		SetActiveReturnToMap(value: true);
	}

	private void HandleItemInventoryOnNewItemTooltipShown(UIPartyItemInventoryTooltip obj)
	{
		ToggleHotkey("Tips", obj.CanShowTooltip());
	}

	private void HandleNextCharacter()
	{
		NewPartyDisplayUI.PartyDisplay.NextCharacter();
		NewPartyDisplayUI.PartyDisplay.TryToggleEquipmentPanelForSelectedCharacter(isEnabled: true);
		Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Focus();
	}

	private void SetHotkeysAction(IEnumerable<HotkeyAction> hotkeyActions)
	{
		NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.SetHotkeysAction(hotkeyActions);
	}

	private void ToggleHotkey(string hotkey, bool isActive)
	{
		NewPartyDisplayUI.PartyDisplay.ShopInventoryPanelHotkeyContainerProxy.SetActiveHotkey(hotkey, isActive);
	}

	private void ItemInventoryOnOnItemsRefreshed(IUiNavigationSelectable obj)
	{
		_navigationManager.SetCurrentRoot(RootName, selectFirst: false, obj);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		SetActiveReturnToMap(value: false);
		ToggleHotkey("Switch_Left", isActive: false);
		ToggleHotkey("Tips", isActive: false);
		if (Singleton<UIShopItemWindow>.Instance.ItemInventory.TooltipShown)
		{
			Singleton<UIShopItemWindow>.Instance.ItemInventory.ToggleItemTextTooltip();
		}
		Singleton<UIShopItemWindow>.Instance.ItemInventory.OnItemsRefreshed -= ItemInventoryOnOnItemsRefreshed;
		Singleton<UIShopItemWindow>.Instance.ItemInventory.NewItemTooltipShown -= HandleItemInventoryOnNewItemTooltipShown;
		NewPartyDisplayUI.PartyDisplay.ToggleBackgroundElements(isActive: false);
		Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Unfocus();
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_PREVIOUS_TAB, HandleNextCharacter);
	}

	private CampaignMapStateTag GetPreviousStateTag(IStateProvider stateProvider, CampaignMapStateTag[] tagsFilter)
	{
		return stateProvider.GetLatestState(tagsFilter);
	}
}
