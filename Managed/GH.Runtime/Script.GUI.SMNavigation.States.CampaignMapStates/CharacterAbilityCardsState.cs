using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class CharacterAbilityCardsState : CampaignMapState
{
	public static string DefaultRootName = "CharacterAbilityCards";

	private IHotkeySession _templeHotkeySession;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.CharacterAbilityCards;

	protected override bool SelectedFirst => true;

	protected override string RootName => DefaultRootName;

	protected CampaignMapStateTag[] TagsFilter { get; }

	public CharacterAbilityCardsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		TagsFilter = new CampaignMapStateTag[3]
		{
			CampaignMapStateTag.Merchant,
			CampaignMapStateTag.Temple,
			CampaignMapStateTag.WorldMap
		};
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.Mercenaries);
		NewPartyDisplayUI.PartyDisplay.TabInput.Register();
		CampaignMapStateTag previousStateTag = GetPreviousStateTag(stateProvider, TagsFilter);
		InitHotkeys(previousStateTag);
		if (!NewPartyDisplayUI.PartyDisplay.IsDisableFurtherAbilityPanel)
		{
			InputManager.RegisterToOnReleased(KeyAction.UI_FURTHER_ABILITY_CARD, Singleton<UIResetLevelUpWindow>.Instance.ChangeToggleValue);
		}
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: false);
		FullAbilityCard.FullCardHoveringStateChanged += OnFullCardHoveringStateChanged;
		if (previousStateTag == CampaignMapStateTag.Temple)
		{
			_templeHotkeySession = Singleton<UIGuildmasterHUD>.Instance.TempleWindow.HotkeyContainer?.GetSession();
			_templeHotkeySession?.AddOrReplaceHotkey("Switch_Right", null);
		}
		if (Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.Loadout) && Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.TravelQuestState) && Singleton<UINavigation>.Instance.StateMachine.PreviousState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.Temple))
		{
			Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		}
		Singleton<UIQuestPopupManager>.Instance.SetSwitchNavigation(canSwitch: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: true);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.EnableNavigation();
	}

	private void OnFullCardHoveringStateChanged(bool value)
	{
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowTipHotkey(value);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.Scenario);
		Singleton<UIQuestPopupManager>.Instance.SetSwitchNavigation(canSwitch: false);
		FullAbilityCard.FullCardHoveringStateChanged -= OnFullCardHoveringStateChanged;
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchRightHotkey(shown: false);
		InputManager.UnregisterToOnReleased(KeyAction.UI_FURTHER_ABILITY_CARD, Singleton<UIResetLevelUpWindow>.Instance.ChangeToggleValue);
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_RIGHT, OnReleasedFromTempleState);
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_RIGHT, OnReleasedFromMerchantState);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowTipHotkey(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: false);
		_templeHotkeySession?.Dispose();
		_templeHotkeySession = null;
	}

	private void InitHotkeys(CampaignMapStateTag previousStateTag)
	{
		GetHotKeys(previousStateTag);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowTipHotkey(FullAbilityCard.IsHover);
		InitActiveHotkeys(previousStateTag);
	}

	private void InitActiveHotkeys(CampaignMapStateTag previousStateTag)
	{
		bool showSwitchRightHotkey = previousStateTag == CampaignMapStateTag.Merchant;
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchRightHotkey(showSwitchRightHotkey);
	}

	private CampaignMapStateTag GetPreviousStateTag(IStateProvider stateProvider, params CampaignMapStateTag[] tagsFilter)
	{
		return stateProvider.GetLatestState(tagsFilter);
	}

	private List<HotkeyAction> GetHotKeys(CampaignMapStateTag previousStateTag)
	{
		List<HotkeyAction> result = new List<HotkeyAction>();
		switch (previousStateTag)
		{
		case CampaignMapStateTag.Temple:
			InitTempleOrMerchantPreviousHotkeys(OnReleasedFromTempleState);
			break;
		case CampaignMapStateTag.Merchant:
			InitTempleOrMerchantPreviousHotkeys(OnReleasedFromMerchantState);
			break;
		default:
			return null;
		}
		return result;
	}

	private void OnReleasedFromMerchantState()
	{
		Singleton<UIShopItemWindow>.Instance.TryFocus();
	}

	private void OnReleasedFromTempleState()
	{
		if (!LevelMessageUILayoutGroup.IsShown)
		{
			Singleton<UIGuildmasterHUD>.Instance.TempleInputArea.Focus();
		}
	}

	private void InitTempleOrMerchantPreviousHotkeys(Action onReleased)
	{
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		InputManager.RegisterToOnReleased(KeyAction.UI_R_RIGHT, onReleased);
	}
}
