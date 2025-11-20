using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.GUI.SMNavigation.States.SpecialStates;

namespace Script.GUI.SMNavigation;

public class NavigationStateMachine : StateMachine
{
	public NavigationStateMachine(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		RegistryMainStates();
		RegistryCampaignMapStates();
		RegistryScenarioStates();
		RegistryMultiplayerStates();
		RegistryPopupStates();
		RegistrySpecialStates();
		RegisterEscMenuStates();
	}

	private void RegistryPopupStates()
	{
		SetState(new LevelMessageState(NavManager));
		SetState(new GuildmasterRewardsState(NavManager));
		SetState(new DistributeItemsRewardsState(NavManager));
		SetState(new DistributeGoldRewardsState(NavManager));
		SetState(new PersonalQuestCompletedState(NavManager));
		SetState(new TravelQuestState(NavManager));
		SetState(new CharacterConfirmationBoxState(NavManager));
		SetState(new DialogueMessageState(NavManager));
		SetState(new ConfirmationBoxState(NavManager));
		SetState(new SelectInputDeviceBoxState(NavManager));
		SetState(new SelectCharacterToBurnCardState(NavManager));
		SetState(new DistributeDamageState(NavManager));
		SetState(new PickItemToRefreshOrConsumeState(NavManager));
		SetState(new PickItemToLoseState(NavManager));
		SetState(new ErrorMessageState(NavManager));
		SetState(new RetirementConfirmationState(NavManager));
	}

	private void RegistrySpecialStates()
	{
		SetState(new LockState());
	}

	private void RegistryMainStates()
	{
		RegistrySimpleScreensStates();
		RegistrySettingsStates();
		RegistryKeyboardsStates();
		RegistryUIElementsStates();
	}

	private void RegistrySimpleScreensStates()
	{
		SetState(new InitialInteractionState(this, NavManager));
		SetState(new MainMenuOptionsState(this, NavManager));
		SetState(new SubMenuOptionsState(this, NavManager));
		SetState(new CampaignCreateGame(this, NavManager));
		SetState(new GuildMasterCreateGame(this, NavManager));
		SetState(new SubMenuSavedSelectedOptionState(this, NavManager));
		SetState(new LoadGameState(this, NavManager));
		SetState(new TutorialOptionsState(this, NavManager));
		SetState(new CompendiumState(this, NavManager));
		SetState(new SandboxOptionsState(this, NavManager));
		SetState(new ModeSelectionState(this, NavManager));
		SetState(new HouseRulesState(this, NavManager));
		SetState(new SelectDLCState(this, NavManager));
		SetState(new MapEscMenuState(this, NavManager));
		SetState(new CustomPartySetupState(this, NavManager));
		SetState(new GuildMasterTutorial(this, NavManager));
		SetState(new GamepadDisconnectionBoxState(this, NavManager));
		SetState(new SignOutConfirmationBoxState(this, NavManager));
		SetState(new CreditsState(this, NavManager));
		SetState(new EULAScreenState());
	}

	private void RegistryMultiplayerStates()
	{
		SetState(new MultiplayerJoinSessionState(this, NavManager));
		SetState(new MultiplayerHowToState(this, NavManager));
		SetState(new MultiplayerOnlineContainerState(this, NavManager));
		SetState(new MultiplayerSelectPlayerState(this, NavManager));
		SetState(new MultiplayerOnlineContainerWithSelectedState(this, NavManager));
		SetState(new MultiplayerFriendListState(this, NavManager));
		SetState(new VoiceChatOptionsState(this, NavManager));
		SetState(new MultiplayerInvitePlayersState(this, NavManager));
	}

	private void RegistrySettingsStates()
	{
		SetState(new SettingsOptionsState(this, NavManager));
		SetState(new SettingsOptionsSavedSelectedOptionState(this, NavManager));
		SetState(new DisplaySettingsState(this, NavManager));
		SetState(new DisplaySettingsStateWithSelected(this, NavManager));
		SetState(new AudioSettingsState(this, NavManager));
		SetState(new AudioSettingsStateWithSelected(this, NavManager));
		SetState(new ControlsSettingsState(this, NavManager));
		SetState(new ControlsSettingsStateWithSelected(this, NavManager));
		SetState(new CombatLogSettingsState(this, NavManager));
		SetState(new CombatLogSettingsStateWithSelected(this, NavManager));
		SetState(new LanguageSettingsState(this, NavManager));
		SetState(new MenuInfoState(this, NavManager));
		SetState(new GeneralSettingsState(this, NavManager));
	}

	private void RegistryKeyboardsStates()
	{
		SetState(new VisualKeyboardState(this, NavManager));
		SetState(new VisualKeyboardCampaignState(this, NavManager));
		SetState(new VisualKeyboardGuildMasterState(this, NavManager));
		SetState(new VisualKeyboardJoinSession(this, NavManager));
	}

	private void RegistryUIElementsStates()
	{
		SetState(new DropdownOptionSelectState(this, NavManager));
		SetState(new MainMenuConfirmationBoxState(this, NavManager));
	}

	private void RegistryCampaignMapStates()
	{
		SetState(new WorldMapState(NavManager));
		SetState(new EquipmentState(NavManager));
		SetState(new InventoryState(NavManager));
		SetState(new GuildmasterHUDState(NavManager));
		SetState(new MercenariesState(NavManager));
		SetState(new MapStoryState(NavManager));
		SetState(new QuestLogState(NavManager));
		SetState(new MapEventState(NavManager));
		SetState(new CampaignRewardState(NavManager));
		SetState(new UnlockLocationFlowState(NavManager));
		SetState(new CharacterCreatorClassRosterState(NavManager));
		SetState(new AdventurePartyAssemblyRosterState(NavManager));
		SetState(new TempleState(NavManager));
		SetState(new PerksState(NavManager));
		SetState(new PerksInventoryState(NavManager));
		SetState(new VisualKeyboardCharacterNameState(NavManager));
		SetState(new BattleGoalPickerState(NavManager));
		SetState(new LoadoutState(NavManager));
		SetState(new CharacterAbilityCardsState(NavManager));
		SetState(new LevelUpState(NavManager));
		SetState(new ShowingCardLevelUpState(NavManager));
		SetState(new FurtherAbilityCardsState(NavManager));
		SetState(new CreateNameStepState(NavManager));
		SetState(new PersonalQuestChoiceState(NavManager));
		SetState(new StartingAbilityCardsState(NavManager));
		SetState(new MerchantState(NavManager));
		SetState(new PartyInventoryTooltipState(NavManager));
		SetState(new UIItemConfirmationBoxState(NavManager));
		SetState(new ShopInventoryTooltipState(NavManager));
		SetState(new UiMultiplayerConfirmationBoxState(NavManager));
		SetState(new LocationHoverState(NavManager));
		SetState(new TownRecordsState(NavManager));
		SetState(new GloomhavenTravelCityState(NavManager));
		SetState(new YesNoCharacterActionDialogState(NavManager));
		SetState(new GuildmasterTrainerState(NavManager));
		RegistryEnhancmentStates();
	}

	private void RegistryEnhancmentStates()
	{
		SetState(new EnhancmentCardSelectState(NavManager));
		SetState(new EnhancmentSelectCardOptionState(this, NavManager));
		SetState(new EnhancmentSelectOptionUpgradeState(this, NavManager));
		SetState(new EnhancmentConfirmationState(NavManager));
	}

	private void RegistryScenarioStates()
	{
		SetState(new AllCardsScenarioState(this, NavManager));
		SetState(new BurnScenarioState(this, NavManager));
		SetState(new CardSelectionScenarioState(this, NavManager));
		SetState(new ChooseCardsScenarioState(this, NavManager));
		SetState(new CheckOutRoundCardsScenarioState(this, NavManager));
		SetState(new DamageScenarioState(this, NavManager));
		SetState(new DeckScenarioState(this, NavManager));
		SetState(new EnemyActionScenarioState(this, NavManager));
		SetState(new LongRestScenarioState(this, NavManager));
		SetState(new RoundStartScenarioState(this, NavManager));
		SetState(new SelectActionScenarioState(this, NavManager));
		SetState(new LoseCardScenarioState(this, NavManager));
		SetState(new UseActionScenarioState(this, NavManager));
		SetState(new EndScenarioState(this, NavManager));
		SetState(new EndScenarioResultsState(this, NavManager));
		SetState(new SelectTargetState(this, NavManager));
		SetState(new SelectItemState(this, NavManager));
		SetState(new SelectElementItemState(this, NavManager));
		SetState(new ItemActionsScenarioState(this, NavManager));
		SetState(new HexMovementOnSelectActionState(this, NavManager));
		SetState(new AnimationScenarioState(this, NavManager));
		SetState(new AbilityActionsScenarioState(this, NavManager));
		SetState(new SelectAbilityInfusionElementState(this, NavManager));
		SetState(new CheckForInitiativeAdjustmentsState(this, NavManager));
		SetState(new AbilityCardPickerState(this, NavManager));
		SetState(new RewardState(this, NavManager));
	}

	private void RegisterEscMenuStates()
	{
		SetState(new EscMenuDifficultySelect(this, NavManager));
		SetState(new EscMenuHouseRulesSelect(this, NavManager));
		SetState(new EscMenuMultiplayer(this, NavManager));
		SetState(new ScenarioEscMenuState(this, NavManager));
		SetState(new ScenarioEscMessageConfirmationState(this, NavManager));
	}
}
