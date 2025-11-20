using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class LevelUpState : CampaignMapState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[8]
	{
		KeyAction.UI_PREVIOUS_TAB,
		KeyAction.UI_NEXT_TAB,
		KeyAction.NEXT_SHIELD_TAB,
		KeyAction.PREVIOUS_SHIELD_TAB,
		KeyAction.NEXT_MERCENARY_OPTION,
		KeyAction.CREATE_NEW_CHARACTER,
		KeyAction.CONTROL_LOCAL_OPTIONS_LEFT,
		KeyAction.UI_NEXT_TAB_MERCENARY
	};

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.LevelUp;

	protected override bool SelectedFirst => true;

	protected override string RootName => "LevelUpInventoryCards";

	public LevelUpState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		InputManager.RequestDisableInput(this, _actionsToDisable);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowFurtherAbilityCardHotkey(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: false);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSelectHotkey(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSwitchRightHotkey(shown: false);
		InputManager.RegisterToOnReleased(KeyAction.UI_R_LEFT, SwitchToCurrentCards);
		FullAbilityCard.FullCardHoveringStateChanged += OnFullCardHoveringStateChanged;
		Singleton<UIReadyToggle>.Instance.BlockVisibility(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		FullAbilityCard.FullCardHoveringStateChanged -= OnFullCardHoveringStateChanged;
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		InputManager.RequestEnableInput(this, _actionsToDisable);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: false);
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: true);
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_RIGHT, SwitchToLevelUpCards);
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_LEFT, SwitchToCurrentCards);
		Singleton<UIReadyToggle>.Instance.UnblockVisibility(this);
	}

	private void SwitchToCurrentCards()
	{
		_navigationManager.SetCurrentRoot(CharacterAbilityCardsState.DefaultRootName);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: true);
		InputManager.RegisterToOnReleased(KeyAction.UI_R_RIGHT, SwitchToLevelUpCards);
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_LEFT, SwitchToCurrentCards);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: false);
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSelectHotkey(shown: false);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSwitchRightHotkey(shown: true);
	}

	private void SwitchToLevelUpCards()
	{
		DefaultNavigation();
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.HideFullCards();
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: false);
		InputManager.RegisterToOnReleased(KeyAction.UI_R_LEFT, SwitchToCurrentCards);
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_RIGHT, SwitchToLevelUpCards);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: false);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSelectHotkey(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSwitchRightHotkey(shown: false);
	}

	private void OnFullCardHoveringStateChanged(bool value)
	{
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowTipHotkey(value);
	}
}
