using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class ShowingCardLevelUpState : CampaignMapState
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

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.ShowingCardLevelUp;

	protected override bool SelectedFirst => true;

	protected override string RootName => "LevelUpInventoryCards";

	public ShowingCardLevelUpState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		InputManager.RequestDisableInput(this, _actionsToDisable);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: true);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.DisplaySelectHotkeys(value: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowFurtherAbilityCardHotkey(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetActiveControllerInputScroll(active: false);
		Singleton<UIReadyToggle>.Instance.BlockVisibility(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		Singleton<UIResetLevelUpWindow>.Instance.SetShowDarkenPanel(shown: false);
		NewPartyDisplayUI.PartyDisplay.AbilityCardsDisplay.SetShowDarkenPanel(shown: false);
		Singleton<UIPartyCharacterEquipmentDisplay>.Instance.ToggleBackground(isActive: true);
		InputManager.RequestEnableInput(this, _actionsToDisable);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		Singleton<UIReadyToggle>.Instance.UnblockVisibility(this);
	}
}
