#define ENABLE_LOGS
using Assets.Script.GUI.NewAdventureMode.Guildmaster;
using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class TempleState : CampaignMapState, IGuildmasterBannerConfigContainer
{
	private SessionHotkey? _selectHotkey;

	private SessionHotkey? _switchRightHotkey;

	private IHotkeySession _templeHotkeySession;

	private IHotkeySession _switchToPartyInfoHotkeySession;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Temple;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Temple";

	public IGuildmasterBannerConfig GuildmasterBannerConfig => Singleton<UIGuildmasterHUD>.Instance.TempleWindow.GuildmasterBannerConfig;

	public TempleState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		FirstOrCurrentNavigation();
		UITempleWindow templeWindow = Singleton<UIGuildmasterHUD>.Instance.TempleWindow;
		_templeHotkeySession = templeWindow.HotkeyContainer.GetSessionOrEmpty();
		templeWindow.SetDarkenActive(value: false);
		_selectHotkey = _templeHotkeySession?.GetHotkey("Select");
		bool shown = templeWindow.Shop.HasAvailableSlots();
		_selectHotkey?.SetShown(shown);
		Debug.Log($"[Temple] Previous State: {Singleton<UINavigation>.Instance.StateMachine.PreviousState}");
		if (CanSwitchToPartyInfoByStick(out var partyInfoHotkeyContainer))
		{
			_switchToPartyInfoHotkeySession?.Dispose();
			_switchToPartyInfoHotkeySession = partyInfoHotkeyContainer.GetSession().AddOrReplaceHotkey("Switch_Left", OnPartySelected);
		}
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		NewPartyDisplayUI.PartyDisplay.DisplayNextPrevHotkey(value: true);
		NewPartyDisplayUI.PartyDisplay.SetTooltipHotkeyPanelActive(value: true);
		InputManager.RegisterToOnPressed(KeyAction.UI_NEXT_TAB, OnPartySelected);
		InputManager.RegisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, OnPartySelected);
		SetActiveReturnToMap(value: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		SetActiveReturnToMap(value: false);
		_switchToPartyInfoHotkeySession?.Dispose();
		_switchToPartyInfoHotkeySession = null;
		Singleton<UIGuildmasterHUD>.Instance.TempleWindow.SetDarkenActive(value: true);
		NewPartyDisplayUI.PartyDisplay.DisplayNextPrevHotkey(value: false);
		NewPartyDisplayUI.PartyDisplay.SetTooltipHotkeyPanelActive(value: false);
		Singleton<UIGuildmasterHUD>.Instance.TempleInputArea.Unfocus();
		InputManager.UnregisterToOnPressed(KeyAction.UI_NEXT_TAB, OnPartySelected);
		InputManager.UnregisterToOnPressed(KeyAction.UI_PREVIOUS_TAB, OnPartySelected);
		DisposeHotkeys();
	}

	private void OnPartySelected()
	{
		if (!LevelMessageUILayoutGroup.IsShown)
		{
			_selectHotkey?.SetShown(shown: false);
			Singleton<UIGuildmasterHUD>.Instance.TempleInputArea.Unfocus();
			NewPartyDisplayUI.PartyDisplay.SelectCurrentCharacter();
		}
	}

	private void DisposeHotkeys()
	{
		_selectHotkey?.Dispose();
		_selectHotkey = null;
		_templeHotkeySession?.Dispose();
		_templeHotkeySession = null;
	}

	private bool CanSwitchToPartyInfoByStick(out IHotkeyContainer partyInfoHotkeyContainer)
	{
		partyInfoHotkeyContainer = null;
		IState previousState = Singleton<UINavigation>.Instance.StateMachine.PreviousState;
		if (!(previousState is CharacterAbilityCardsState))
		{
			if (previousState is EquipmentState)
			{
				UIPartyCharacterEquipmentDisplay itemInventoryDisplay = NewPartyDisplayUI.PartyDisplay.ItemInventoryDisplay;
				if (itemInventoryDisplay.IsOpen)
				{
					partyInfoHotkeyContainer = itemInventoryDisplay.ShopInventoryLocalHotkeys;
				}
			}
		}
		else
		{
			UIPartyCharacterAbilityCardsDisplay abilityCardsDisplay = NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay;
			if (abilityCardsDisplay.IsOpen)
			{
				partyInfoHotkeyContainer = abilityCardsDisplay.PanelHotkeyContainer;
			}
		}
		return partyInfoHotkeyContainer != null;
	}
}
