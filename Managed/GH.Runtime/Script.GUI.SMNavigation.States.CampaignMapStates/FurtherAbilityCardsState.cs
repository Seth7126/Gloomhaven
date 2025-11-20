using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class FurtherAbilityCardsState : CampaignMapState
{
	private readonly KeyAction[] _actionsToDisable = new KeyAction[4]
	{
		KeyAction.NEXT_MERCENARY_OPTION,
		KeyAction.CREATE_NEW_CHARACTER,
		KeyAction.CONTROL_LOCAL_OPTIONS_LEFT,
		KeyAction.UI_SUBMIT
	};

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.FurtherAbilityCards;

	protected override bool SelectedFirst => true;

	protected override string RootName => "LevelUpInventoryCards";

	public FurtherAbilityCardsState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		InputManager.RequestDisableInput(this, _actionsToDisable);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowBackHotkey(shown: true);
		Singleton<UIResetLevelUpWindow>.Instance.TryActiveResetMercenaryHotkey();
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowSwitchRightHotkey(shown: false);
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: false);
		InputManager.RegisterToOnReleased(KeyAction.UI_R_LEFT, ActiveFurtherAbilityCardPanel);
		InputManager.RegisterToOnReleased(KeyAction.UI_FURTHER_ABILITY_CARD, ActiveFurtherAbilityCardPanel);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		InputManager.UnregisterToOnReleased(KeyAction.UI_R_LEFT, ActiveFurtherAbilityCardPanel);
		InputManager.UnregisterToOnReleased(KeyAction.UI_FURTHER_ABILITY_CARD, ActiveFurtherAbilityCardPanel);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: false);
		InputManager.RequestEnableInput(this, _actionsToDisable);
		Singleton<UIResetLevelUpWindow>.Instance.UILevelUpCardInventory.SetShowBackHotkey(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.HideFullCardsPreview();
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowSwitchLeftHotkey(shown: false);
	}

	private void ActiveFurtherAbilityCardPanel()
	{
		Singleton<UIResetLevelUpWindow>.Instance.ChangeToggleValue();
	}
}
