#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Bolt;

namespace FFSNet;

public class GameAction
{
	private static readonly Dictionary<GameActionType, GameActionDelegate> actions = new Dictionary<GameActionType, GameActionDelegate>
	{
		{
			GameActionType.NONE,
			delegate
			{
				ShowUninitializedActionErrorMessage();
			}
		},
		{
			GameActionType.PickDifficulty,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				UIDifficultySelector.ClientSetDifficulty(a);
			}
		},
		{
			GameActionType.GameLoadedAndClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIMultiplayerSelectPlayerScreen>.Instance.ServerACKClientLoadedInAndReady(a);
			}
		},
		{
			GameActionType.SwitchCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.MPSwitchCharacter(a, ref v, ref f);
			}
		},
		{
			GameActionType.RemoveCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.MPRemoveCharacter(a, ref v, ref f);
			}
		},
		{
			GameActionType.ResetCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIResetLevelUpWindow>.Instance.MPResetCharacter(a, ref v);
			}
		},
		{
			GameActionType.LevelUp,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ProxyLevelUp(a);
			}
		},
		{
			GameActionType.ModifyPerks,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ProxyModifyPerks(a);
			}
		},
		{
			GameActionType.ModifyCardInventory,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ProxyModifyCardInventory(a);
			}
		},
		{
			GameActionType.EquipItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerEquipItem(a);
			}
		},
		{
			GameActionType.UnequipItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerUnequipItem(a);
			}
		},
		{
			GameActionType.BindAndEquipItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerBindAndEquipItem(a);
			}
		},
		{
			GameActionType.UnbindItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NewPartyDisplayUI.PartyDisplay.ServerUnbindItem(a);
			}
		},
		{
			GameActionType.BuyEnhancement,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIGuildmasterHUD>.Instance.ProxyBuyEnhancement(a, ref v);
			}
		},
		{
			GameActionType.SellEnhancement,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIGuildmasterHUD>.Instance.ProxySellEnhancement(a, ref v);
			}
		},
		{
			GameActionType.BuyItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIShopItemWindow>.Instance.MPBuyItem(a, ref v);
			}
		},
		{
			GameActionType.SellItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIShopItemWindow>.Instance.MPSellItem(a, ref v);
			}
		},
		{
			GameActionType.ClaimAchievementReward,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIGuildmasterHUD>.Instance.ClientClaimAchievementReward(a);
			}
		},
		{
			GameActionType.BuyBlessing,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIGuildmasterHUD>.Instance.ProxyBuyBlessing(a, ref v);
			}
		},
		{
			GameActionType.ReadyUpPlayer,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ProxySetReadyState(a, ref v);
			}
		},
		{
			GameActionType.UnreadyPlayer,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ProxySetReadyState(a, ref v);
			}
		},
		{
			GameActionType.AllPlayersReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ClientCheckPlayerControllableStateRevisionsMatch(a);
			}
		},
		{
			GameActionType.StatesSynchronized,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIReadyToggle>.Instance.ServerACKSyncedStateRevision(a);
			}
		},
		{
			GameActionType.ReadyProceed,
			delegate
			{
				global::Singleton<UIReadyToggle>.Instance.ClientProceed();
			}
		},
		{
			GameActionType.MoveToNewNode,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MapChoreographer>.Instance.ClientMoveToNode(a);
			}
		},
		{
			GameActionType.ContinueRoadEvent,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIEventPanel>.Instance.ClientContinueRoadEvent(a);
			}
		},
		{
			GameActionType.ProcessNextReward,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIRewardsManager>.Instance.ClientProcessNextReward(a);
			}
		},
		{
			GameActionType.EnterScenario,
			delegate
			{
				global::Singleton<MapChoreographer>.Instance.ClientEnterScenario();
			}
		},
		{
			GameActionType.SelectQuest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MapChoreographer>.Instance.ProxySelectedLocation(a);
			}
		},
		{
			GameActionType.DebugWinScenario,
			delegate
			{
				Choreographer.s_Choreographer.ClientDebugWinScenario();
			}
		},
		{
			GameActionType.DebugLoseScenario,
			delegate
			{
				Choreographer.s_Choreographer.ClientDebugLoseScenario();
			}
		},
		{
			GameActionType.AbandonScenario,
			delegate
			{
				Choreographer.s_Choreographer.ClientAbandonScenario();
			}
		},
		{
			GameActionType.SelectRoundCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxySelectRoundCards(a);
			}
		},
		{
			GameActionType.SelectCardAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxySelectCardAbility(a);
			}
		},
		{
			GameActionType.AugmentCardAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyToggleAugment(a);
			}
		},
		{
			GameActionType.CancelAbilityAugment,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyToggleAugment(a);
			}
		},
		{
			GameActionType.GroupAugmentCardAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugmentationGroup(a);
			}
		},
		{
			GameActionType.CancelAbilityAugmentGroup,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAugmentationsBar>.Instance.ProxyToggleAugmentationGroup(a);
			}
		},
		{
			GameActionType.SelectTarget,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ProxySelectAbilityTarget(a, deselectInstead: false, ref f);
			}
		},
		{
			GameActionType.DeselectTarget,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ProxySelectAbilityTarget(a, deselectInstead: true, ref f);
			}
		},
		{
			GameActionType.CreateWaypoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Waypoint.ProxySelectTileToCreateMovementPath(a);
			}
		},
		{
			GameActionType.DeleteLastWaypoint,
			delegate
			{
				Waypoint.ProxyDeleteLastWaypoint();
			}
		},
		{
			GameActionType.PressWaypoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Waypoint.ProxyPressWaypoint(a);
			}
		},
		{
			GameActionType.PlaceCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ServerTryAndPlaceCharacterAtRoundStart(a, ref v, ref f);
			}
		},
		{
			GameActionType.UndoAction,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ProxyUndoAction(a, ref f);
			}
		},
		{
			GameActionType.ClearTargets,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ProxyUndoAction(a, ref f);
			}
		},
		{
			GameActionType.ConfirmAction,
			delegate
			{
				Choreographer.s_Choreographer.ProxyConfirmAction();
			}
		},
		{
			GameActionType.SkipAction,
			delegate
			{
				Choreographer.s_Choreographer.ProxySkipAction();
			}
		},
		{
			GameActionType.TakeDamage,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyTakeDamage(a);
			}
		},
		{
			GameActionType.BurnAvailableCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyBurnAvailableCard(a);
			}
		},
		{
			GameActionType.BurnDiscardedCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<TakeDamagePanel>.Instance.ProxyBurnDiscardedCards(a);
			}
		},
		{
			GameActionType.AbilityDiscardCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyAbilityDiscardCard(a);
			}
		},
		{
			GameActionType.AbilityLoseCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyAbilityLoseCard(a);
			}
		},
		{
			GameActionType.ShortRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyShortRest(a);
			}
		},
		{
			GameActionType.LongRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyLongRest(a);
			}
		},
		{
			GameActionType.DebugAddItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (DebugMenu.DebugMenuNotNull)
				{
					DebugMenu.Instance.ProxyDebugAddItem(a);
				}
			}
		},
		{
			GameActionType.UseItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseItemsBar>.Instance.ProxyUseItemBonus(a);
			}
		},
		{
			GameActionType.ClickItemBonusSlot,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseItemsBar>.Instance.ProxyUseItemBonus(a);
			}
		},
		{
			GameActionType.ClickActiveBonusSlot,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIActiveBonusBar>.Instance.ProxyUseActiveBonus(a);
			}
		},
		{
			GameActionType.CancelActiveBonus,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyCancelActiveBonuses(a);
			}
		},
		{
			GameActionType.CancelImprovedShortRest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyCancelImprovedShortRest(a);
			}
		},
		{
			GameActionType.PickElement,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAbilitiesBar>.Instance.ProxyInfuseAbility(a);
			}
		},
		{
			GameActionType.RecoverCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyRecoverCards(a);
			}
		},
		{
			GameActionType.RefreshItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<ItemCardRefreshPicker>.Instance.ProxyRefreshItems(a);
			}
		},
		{
			GameActionType.ApplySingleTargetEffect,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ApplySingleTargetEffect(a);
			}
		},
		{
			GameActionType.SelectExtraTurnCards,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxySelectExtraTurnCards(a);
			}
		},
		{
			GameActionType.PingHex,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<PingManager>.Instance?.ProxyPingHex(a);
			}
		},
		{
			GameActionType.ToggleLongRestAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				CardsHandManager.Instance.ProxyToggleSelectLongRestAbility(a);
			}
		},
		{
			GameActionType.RedistributeHealthAssign,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyDistributeAssignPopup(a);
			}
		},
		{
			GameActionType.SelectAbilityCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIAbilityCardPicker>.Instance.ProxySelectAbilityCard(a, select: true);
			}
		},
		{
			GameActionType.DeselectAbilityCard,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIAbilityCardPicker>.Instance.ProxySelectAbilityCard(a, select: false);
			}
		},
		{
			GameActionType.EndOfRoundCompareClientFinished,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				Choreographer.s_Choreographer.ServerACKClientFinishingEndOfRoundComparison(a);
			}
		},
		{
			GameActionType.EndOfRoundCompareComplete,
			delegate
			{
				Choreographer.s_Choreographer.StateCompareFinished();
			}
		},
		{
			GameActionType.EndOfAbilityClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ServerACKClientReachingEndOfAbility(a);
				}
			}
		},
		{
			GameActionType.EndOfAbilityAllPlayersReady,
			delegate
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ClientProceedToActionSelection();
				}
			}
		},
		{
			GameActionType.EndOfTurnClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ServerACKClientReachingEndOfTurn(a);
				}
			}
		},
		{
			GameActionType.EndOfTurnAllPlayersReady,
			delegate
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ClientProceedToNextTurn();
				}
			}
		},
		{
			GameActionType.EndOfRoundClientReady,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ServerACKClientReachingEndOfRound(a);
				}
			}
		},
		{
			GameActionType.EndOfRoundAllPlayersReady,
			delegate
			{
				if (Choreographer.s_Choreographer != null)
				{
					Choreographer.s_Choreographer.ClientProceedToNextRound();
				}
			}
		},
		{
			GameActionType.RestartRoundMessage,
			delegate
			{
				Choreographer.s_Choreographer.ProxyRestartRoundMessage();
			}
		},
		{
			GameActionType.RestartRound,
			delegate
			{
				Choreographer.s_Choreographer.RestartRound();
			}
		},
		{
			GameActionType.SendActionProcessorState,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				ActionProcessor.SetStateOnControllerChanged(a);
			}
		},
		{
			GameActionType.SendBackActionProcessorState,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				ActionProcessor.SetState(a);
			}
		},
		{
			GameActionType.ReadyForAssignment,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				SceneController.Instance.PlayerReadyForAssignment(a);
			}
		},
		{
			GameActionType.RegenerateAllScenarios,
			delegate
			{
				global::Singleton<MapChoreographer>.Instance.ProxyRequestRegenerateAllMapScenarios();
			}
		},
		{
			GameActionType.NotifyClientsPlayersAreJoining,
			delegate
			{
				PlayerRegistry.ToggleOtherClientsAreJoining(value: true);
			}
		},
		{
			GameActionType.NotifyClientsPlayersAreFinishedJoining,
			delegate
			{
				PlayerRegistry.ToggleOtherClientsAreJoining(value: false);
			}
		},
		{
			GameActionType.NotifyLoadingFinished,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				PlayerRegistry.ProxyNotifyLoadingFinished(a);
			}
		},
		{
			GameActionType.ChooseAbility,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIUseAbilitiesBar>.Instance.ProxyChooseAbility(a);
			}
		},
		{
			GameActionType.SelectBattleGoal,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				BattleGoalMultiplayerService.ProxySelectBattleGoal(a);
			}
		},
		{
			GameActionType.RetireCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MapChoreographer>.Instance.ProxyRetireCharacter(a, ref v);
			}
		},
		{
			GameActionType.ProgressPersonalQuest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MapChoreographer>.Instance.ProxyProgressPersonalQuest(a);
			}
		},
		{
			GameActionType.DistributeUIAddPoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIDistributeRewardManager>.Instance.ProxyAddPoints(a);
			}
		},
		{
			GameActionType.DistributeUIRemovePoint,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIDistributeRewardManager>.Instance.ProxyRemovePoints(a);
			}
		},
		{
			GameActionType.DistributeUIConfirm,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIDistributeRewardManager>.Instance.ProxyConfirmClick(a);
			}
		},
		{
			GameActionType.ConsumeItem,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<ItemCardRefreshPicker>.Instance.ProxyConsumeItems(a);
			}
		},
		{
			GameActionType.AssignSlot,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIMultiplayerSelectPlayerScreen>.Instance.Service.AssignSlotToPlayer(a);
			}
		},
		{
			GameActionType.CampaignCreateCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				(global::Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI).CampaignCharacterSelector.CharacterCreator.CreatorWindow.ProxyCreateCharacter(a);
			}
		},
		{
			GameActionType.CampaignAssignPersonalQuest,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				(global::Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI).CampaignCharacterSelector.ProxyCreateCampaignCharacter(a);
			}
		},
		{
			GameActionType.ToggleCharacterCreationUI,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				NetworkPlayer.ProxyToggleCharacterCreationUI(a);
				global::Singleton<UIMultiplayerEscSubmenu>.Instance?.RefreshAssignInteraction();
				global::Singleton<UIMapMultiplayerController>.Instance?.RefreshWaitingNotifications();
				NewPartyDisplayUI.PartyDisplay?.EnableAssignPlayer(PlayerRegistry.AllPlayers.All((NetworkPlayer it) => !it.IsCreatingCharacter), global::Singleton<UIMultiplayerEscSubmenu>.Instance);
			}
		},
		{
			GameActionType.ToggleClientOverlay,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIMultiplayerLockOverlay>.Instance?.ToggleLock(GameActionType.ToggleClientOverlay, a.DataInt > 0);
			}
		},
		{
			GameActionType.ToggleIsSwitchingCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				PlayerRegistry.ProxyToggleIsSwitchingCharacter(a);
			}
		},
		{
			GameActionType.SelectCityEvent,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MapChoreographer>.Instance.ProxySelectedCityEvent(a);
			}
		},
		{
			GameActionType.DistributeUISelect,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<UIScenarioDistributePointsManager>.Instance.ProxyDistributeSelectPopup(a);
			}
		},
		{
			GameActionType.LoseItemReward,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<ItemRewardLosePicker>.Instance.ProxyItemRewardLose(a);
			}
		},
		{
			GameActionType.DeleteCharacter,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				(global::Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI).CampaignCharacterSelector.ProxyDeleteCharacter(a);
			}
		},
		{
			GameActionType.ForceUnreadyUp,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (PlayerRegistry.MyPlayer.PlayerID == a.SupplementaryDataIDMax)
				{
					ReadyUpToken readyUpToken = (ReadyUpToken)a.SupplementaryDataToken;
					EReadyUpToggleStates eReadyUpToggleStates = (EReadyUpToggleStates)Enum.Parse(typeof(EReadyUpToggleStates), readyUpToken.ToggleState);
					EReadyUpToggleStates eReadyUpToggleStates2 = (global::Singleton<UIReadyToggle>.Instance.PlayerReadiedState.ContainsKey(PlayerRegistry.MyPlayer) ? global::Singleton<UIReadyToggle>.Instance.PlayerReadiedState[PlayerRegistry.MyPlayer] : EReadyUpToggleStates.NotSet);
					Debug.LogGUI("Received request to unready state " + eReadyUpToggleStates.ToString() + " (Player state: " + eReadyUpToggleStates2.ToString() + ")");
					if (eReadyUpToggleStates != eReadyUpToggleStates2)
					{
						global::Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
					}
				}
			}
		},
		{
			GameActionType.SelectTownRecords,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MapChoreographer>.Instance.ProxySelectedTownRecords(a);
			}
		},
		{
			GameActionType.ClientDesync,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				PlayerRegistry.ProxyClientDesync(a);
			}
		},
		{
			GameActionType.ConfirmReward,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				((CampaignScenarioRewardManager)global::Singleton<ScenarioRewardManager>.Instance).ProxyConfirm(a);
			}
		},
		{
			GameActionType.ChangeHouseRuleSettings,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				HouseRulesSettings.ProxySetHouseRuleSettings(a);
			}
		},
		{
			GameActionType.GameLoadedAndHostReady,
			delegate
			{
				SceneController.Instance.GameLoadedAndHostReady = true;
			}
		},
		{
			GameActionType.RetryScenario,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				UINewAdventureResultsManager.Implementation.ProxyRetryScenario(a);
			}
		},
		{
			GameActionType.ExitScenario,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				UINewAdventureResultsManager.Implementation.ProxyExitScenario(a);
			}
		},
		{
			GameActionType.SendLocalSaveFileQuestCompletion,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
				{
					NetworkPlayer networkPlayer = null;
					networkPlayer = ((a.SupplementaryDataIDMin == 0) ? PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer it) => it.PlayerID == a.PlayerID) : PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer it) => it.PlayerID == a.SupplementaryDataIDMin));
					if (networkPlayer != null)
					{
						global::Singleton<MapChoreographer>.Instance.QueueQuestCompletionsToImport(networkPlayer, (QuestCompletionToken)a.SupplementaryDataToken);
					}
					else
					{
						Debug.LogError("Unable to find player with ID for SendLocalSaveFileQuestCompletion: " + ((a.SupplementaryDataIDMin != 0) ? a.SupplementaryDataIDMin : a.PlayerID));
					}
				}
			}
		},
		{
			GameActionType.UpdatedImportSettings,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MultiplayerImportProgressManager>.Instance.ProxySelectedImportSettings(a);
			}
		},
		{
			GameActionType.ImportSettings,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				global::Singleton<MultiplayerImportProgressManager>.Instance.ProxyConfirmImportSettings(a);
			}
		},
		{
			GameActionType.ShowQuestCompletionImport,
			delegate
			{
				if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
				{
					global::Singleton<MapChoreographer>.Instance.ShowQuestCompletionsToImport();
				}
			}
		},
		{
			GameActionType.FinishedSendingLocalSaveFileQuestCompletion,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
				{
					global::Singleton<MapChoreographer>.Instance.PlayerFinishedSendingQuestCompletionData(PlayerRegistry.AllPlayers.First((NetworkPlayer it) => it.PlayerID == a.PlayerID));
				}
			}
		},
		{
			GameActionType.NotifyFullLoadingFinished,
			delegate(GameAction a, ref bool v, ref bool f)
			{
				PlayerRegistry.ProxyNotifyFullLoadingFinished(a);
			}
		}
	};

	private bool isValid = true;

	public int ActionID { get; set; }

	public int ActionTypeID { get; private set; }

	public int PlayerID { get; private set; }

	public int ActorID { get; private set; }

	public int TargetPhaseID { get; set; }

	public int TargetPlayerID { get; private set; }

	public int DataInt { get; private set; }

	public int DataInt2 { get; private set; }

	public int SupplementaryDataIDMin { get; set; }

	public int SupplementaryDataIDMed { get; set; }

	public int SupplementaryDataIDMax { get; set; }

	public bool SupplementaryDataBoolean { get; set; }

	public Guid SupplementaryDataGuid { get; set; }

	public IProtocolToken SupplementaryDataToken { get; set; }

	public IProtocolToken SupplementaryDataToken2 { get; set; }

	public IProtocolToken SupplementaryDataToken3 { get; set; }

	public IProtocolToken SupplementaryDataToken4 { get; set; }

	public byte[] CustomBinaryData { get; set; }

	public bool SyncViaStateUpdate { get; private set; }

	public bool ValidateAction { get; private set; }

	public bool DoNotForwardAction { get; private set; }

	public bool BinaryDataIncludesLoggingDetails { get; private set; }

	public string ClassID { get; private set; }

	public bool ExecuteLocallyFirst { get; private set; }

	public bool IsValid => isValid;

	public GameAction(GameActionType actionType, ActionPhaseType targetPhaseType = ActionPhaseType.NONE, bool syncViaStateUpdate = false, bool executeLocallyFirst = false, bool validateOnServerBeforeExecuting = false, bool doNotForward = false, int actorID = 0, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, Guid supplementaryDataGuid = default(Guid), IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null, byte[] customBinaryData = null, bool binaryDataIncludesLoggingDetails = false, string classID = null, int clientActionID = 0)
	{
		if (PlayerRegistry.MyPlayer == null)
		{
			return;
		}
		ActionTypeID = (int)actionType;
		PlayerID = PlayerRegistry.MyPlayer.PlayerID;
		ActorID = actorID;
		TargetPhaseID = (int)targetPhaseType;
		SupplementaryDataIDMin = supplementaryDataIDMin;
		SupplementaryDataIDMed = supplementaryDataIDMed;
		SupplementaryDataIDMax = supplementaryDataIDMax;
		SupplementaryDataBoolean = supplementaryDataBoolean;
		SupplementaryDataGuid = supplementaryDataGuid;
		SupplementaryDataToken = supplementaryDataToken;
		SupplementaryDataToken2 = supplementaryDataToken2;
		SupplementaryDataToken3 = supplementaryDataToken3;
		SupplementaryDataToken4 = supplementaryDataToken4;
		CustomBinaryData = customBinaryData;
		SyncViaStateUpdate = syncViaStateUpdate;
		ValidateAction = validateOnServerBeforeExecuting;
		DoNotForwardAction = doNotForward;
		ExecuteLocallyFirst = executeLocallyFirst;
		BinaryDataIncludesLoggingDetails = binaryDataIncludesLoggingDetails;
		ClassID = classID;
		if (FFSNetwork.IsHost)
		{
			if (!ExecuteLocallyFirst || PlayerID != PlayerRegistry.HostPlayerID)
			{
				ActionID = ++PlayerRegistry.MyPlayer.state.LatestProcessedActionID;
				ActionProcessor.LogAction(this);
			}
		}
		else
		{
			ActionID = clientActionID;
		}
	}

	public GameAction(GameActionEvent evnt)
	{
		ActionID = evnt.ActionID;
		ActionTypeID = evnt.ActionTypeID;
		PlayerID = evnt.PlayerID;
		ActorID = evnt.ActorID;
		TargetPhaseID = evnt.TargetPhaseID;
		SupplementaryDataIDMin = evnt.SupplementaryDataIDMin;
		SupplementaryDataIDMed = evnt.SupplementaryDataIDMed;
		SupplementaryDataIDMax = evnt.SupplementaryDataIDMax;
		SupplementaryDataBoolean = evnt.SupplementaryDataBoolean;
		SupplementaryDataGuid = evnt.SupplementaryDataGuid;
		SupplementaryDataToken = evnt.SupplementaryDataToken;
		SupplementaryDataToken2 = evnt.SupplementaryDataToken2;
		SupplementaryDataToken3 = evnt.SupplementaryDataToken3;
		SupplementaryDataToken4 = evnt.SupplementaryDataToken4;
		CustomBinaryData = evnt.BinaryData;
		SyncViaStateUpdate = evnt.SyncViaStateUpdate;
		ValidateAction = evnt.ValidateAction;
		DoNotForwardAction = evnt.DoNotForwardAction;
		BinaryDataIncludesLoggingDetails = evnt.BinaryDataIncludesLoggingDetails;
	}

	public GameAction(NetworkActionEvent actionEvent)
	{
		NetworkAction networkAction = (NetworkAction)actionEvent.Token;
		ActionTypeID = networkAction.ActionTypeID;
		PlayerID = networkAction.PlayerID;
		SupplementaryDataToken = networkAction.DataToken;
		TargetPlayerID = networkAction.TargetPlayerID;
		DataInt = networkAction.DataInt;
		DataInt2 = networkAction.DataInt2;
		SupplementaryDataBoolean = networkAction.DataBoolean;
	}

	public GameAction(GameActionEventClassID evnt)
	{
		ActionID = evnt.ActionID;
		ActionTypeID = evnt.ActionTypeID;
		PlayerID = evnt.PlayerID;
		ActorID = evnt.ActorID;
		TargetPhaseID = evnt.TargetPhaseID;
		SupplementaryDataIDMin = evnt.SupplementaryDataIDMin;
		SupplementaryDataBoolean = evnt.SupplementaryDataBoolean;
		CustomBinaryData = evnt.BinaryData;
		ClassID = evnt.ClassID;
	}

	public bool Execute()
	{
		if (actions.TryGetValue((GameActionType)ActionTypeID, out var value))
		{
			bool executionFinished = true;
			value(this, ref isValid, ref executionFinished);
			return executionFinished;
		}
		throw new Exception("Error executing action. Matching delegate method not found.");
	}

	private static void ShowUninitializedActionErrorMessage()
	{
		throw new Exception("Error executing action. Uninitialized action detected.");
	}
}
