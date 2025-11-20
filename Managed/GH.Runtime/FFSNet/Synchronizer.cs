using System;
using System.Linq;
using Assets.Script.Networking.Tokens;
using GLOOM;
using Photon.Bolt;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FFSNet;

public static class Synchronizer
{
	public static void SendGameAction(GameActionType actionType, ActionPhaseType targetPhaseType = ActionPhaseType.NONE, bool validateOnServerBeforeExecuting = false, bool disableAutoReplication = false, int actorID = 0, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, Guid supplementaryDataGuid = default(Guid), IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null, byte[] customBinaryData = null, bool binaryDataIncludesLoggingDetails = false)
	{
		ReplicateAction(actionType, targetPhaseType, syncViaStateUpdate: false, executeLocallyFirst: false, validateOnServerBeforeExecuting, disableAutoReplication, actorID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataGuid, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, customBinaryData, binaryDataIncludesLoggingDetails, null);
	}

	public static void SendGameActionClassID(GameActionType actionType, ActionPhaseType targetPhaseType, int actorID, int supplementaryDataIDMin, bool supplementaryDataBool, string classID)
	{
		ReplicateAction(actionType, targetPhaseType, syncViaStateUpdate: false, executeLocallyFirst: false, validateOnServerBeforeExecuting: false, disableAutoReplication: false, actorID, supplementaryDataIDMin, 0, 0, supplementaryDataBool, default(Guid), null, null, null, null, null, binaryDataIncludesLoggingDetails: false, classID);
	}

	public static void SendSideAction(GameActionType actionType, IProtocolToken supplementaryDataToken = null, bool canBeUnreliable = false, bool sendToHostOnly = false, int targetPlayerID = 0, int dataInt = 0, int dataInt2 = 0, bool dataBool = false)
	{
		if (!FFSNetwork.HasDesynchronized)
		{
			Console.LogInfo("Sending SideAction (" + actionType.ToString() + ") to " + (sendToHostOnly ? "server" : "others") + ". DataTOKEN: " + ((supplementaryDataToken != null) ? "YES" : "NO") + ", Unreliable: " + canBeUnreliable);
			NetworkActionEvent networkActionEvent = NetworkActionEvent.Create(sendToHostOnly ? GlobalTargets.OnlyServer : GlobalTargets.Others, (!canBeUnreliable) ? ReliabilityModes.ReliableOrdered : ReliabilityModes.Unreliable);
			networkActionEvent.Token = new NetworkAction(actionType, PlayerRegistry.MyPlayer, supplementaryDataToken, targetPlayerID, dataInt, dataInt2, dataBool);
			networkActionEvent.Send();
		}
	}

	public static void AutoExecuteServerAuthGameAction(GameActionType actionType, ActionPhaseType targetPhaseType = ActionPhaseType.NONE, bool disableAutoReplication = false, int actorID = 0, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, Guid supplementaryDataGuid = default(Guid), IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null, byte[] customBinaryData = null, bool binaryDataIncludesLoggingDetails = false)
	{
		ReplicateAction(actionType, targetPhaseType, syncViaStateUpdate: false, executeLocallyFirst: true, validateOnServerBeforeExecuting: true, disableAutoReplication, actorID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataGuid, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, customBinaryData, binaryDataIncludesLoggingDetails, null);
	}

	public static void ReplicateControllableStateChange(GameActionType actionType, ActionPhaseType targetPhaseType, int controllableID, int supplementaryDataIDMin = 0, int supplementaryDataIDMed = 0, int supplementaryDataIDMax = 0, bool supplementaryDataBoolean = false, Guid supplementaryDataGuid = default(Guid), IProtocolToken supplementaryDataToken = null, IProtocolToken supplementaryDataToken2 = null, IProtocolToken supplementaryDataToken3 = null, IProtocolToken supplementaryDataToken4 = null, bool validateOnServerBeforeExecuting = false)
	{
		ReplicateAction(actionType, targetPhaseType, syncViaStateUpdate: true, executeLocallyFirst: false, validateOnServerBeforeExecuting, disableAutoReplication: false, controllableID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataGuid, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, null, binaryDataIncludesLoggingDetails: false, null);
	}

	private static void ReplicateAction(GameActionType actionType, ActionPhaseType targetPhaseType, bool syncViaStateUpdate, bool executeLocallyFirst, bool validateOnServerBeforeExecuting, bool disableAutoReplication, int actorID, int supplementaryDataIDMin, int supplementaryDataIDMed, int supplementaryDataIDMax, bool supplementaryDataBoolean, Guid supplementaryDataGuid, IProtocolToken supplementaryDataToken, IProtocolToken supplementaryDataToken2, IProtocolToken supplementaryDataToken3, IProtocolToken supplementaryDataToken4, byte[] customBinaryData, bool binaryDataIncludesLoggingDetails, string classID)
	{
		if (FFSNetwork.HasDesynchronized)
		{
			return;
		}
		int clientActionID = PlayerRegistry.HostPlayer.state.LatestProcessedActionID + 1;
		GameAction gameAction = new GameAction(actionType, targetPhaseType, syncViaStateUpdate, executeLocallyFirst, validateOnServerBeforeExecuting, disableAutoReplication, actorID, supplementaryDataIDMin, supplementaryDataIDMed, supplementaryDataIDMax, supplementaryDataBoolean, supplementaryDataGuid, supplementaryDataToken, supplementaryDataToken2, supplementaryDataToken3, supplementaryDataToken4, customBinaryData, binaryDataIncludesLoggingDetails, classID, clientActionID);
		string text = string.Empty;
		try
		{
			switch ((GameActionType)gameAction.ActionTypeID)
			{
			case GameActionType.UseItem:
			{
				IProtocolToken supplementaryDataToken5 = gameAction.SupplementaryDataToken;
				ItemToken itemToken = supplementaryDataToken5 as ItemToken;
				if (itemToken != null)
				{
					string text2 = ScenarioManager.Scenario?.PlayerActors?.SelectMany((CPlayerActor s) => s.Inventory.AllItems).FirstOrDefault((CItem w) => w.NetworkID == itemToken.ItemNetworkID)?.Name;
					if (text2 != null)
					{
						text = text + " Item Name: " + text2;
					}
				}
				break;
			}
			case GameActionType.CreateWaypoint:
				if (gameAction.SupplementaryDataToken is TilesToken tilesToken)
				{
					text += " Waypoint";
					for (int i = 0; i < tilesToken.Tiles.GetLength(0); i++)
					{
						text = text + " , (" + tilesToken.Tiles[i, 0] + "," + tilesToken.Tiles[i, 1] + ")";
					}
				}
				break;
			case GameActionType.SelectTarget:
				if (gameAction.SupplementaryDataToken is TargetSelectionToken targetSelectionToken)
				{
					text += " Target at";
					for (int num = 0; num < targetSelectionToken.Tiles.GetLength(0); num++)
					{
						text = text + " , (" + targetSelectionToken.Tiles[num, 0] + "," + targetSelectionToken.Tiles[num, 1] + ")";
					}
				}
				break;
			case GameActionType.AugmentCardAbility:
			{
				IProtocolToken supplementaryDataToken5 = gameAction.SupplementaryDataToken;
				AbilityAugmentToken augmentToken = supplementaryDataToken5 as AbilityAugmentToken;
				if (augmentToken != null)
				{
					text = text + " CardID: " + augmentToken.CardID + " " + ((augmentToken.ActionTypeID == 0) ? "Top" : "Bottom");
					text = text + " Card Name: " + LocalizationManager.GetTranslation(ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData s) => s.ID == augmentToken.CardID).Name);
					text = text + " Augment Index: " + augmentToken.ConsumeIndex;
				}
				break;
			}
			case GameActionType.ClickActiveBonusSlot:
			{
				IProtocolToken supplementaryDataToken5 = gameAction.SupplementaryDataToken;
				ActiveBonusToken bonusToken = supplementaryDataToken5 as ActiveBonusToken;
				if (bonusToken != null)
				{
					text = text + " CardID: " + bonusToken.BaseCardID;
					text = text + " Card Name: " + LocalizationManager.GetTranslation(ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData s) => s.ID == bonusToken.BaseCardID).Name);
					text = text + " Ability: " + bonusToken.AbilityName;
				}
				break;
			}
			case GameActionType.CampaignAssignPersonalQuest:
				if (gameAction.SupplementaryDataToken is CampaignPersonalQuestData campaignPersonalQuestData)
				{
					text = text + " Character ID: " + campaignPersonalQuestData.CharacterID;
					text = text + " Character Name: " + campaignPersonalQuestData.CharacterName;
					text = text + " Personal Quest ID: " + campaignPersonalQuestData.PersonalQuestID;
				}
				break;
			case GameActionType.ContinueRoadEvent:
				if (gameAction.SupplementaryDataToken is RoadEventToken roadEventToken)
				{
					text = text + " Event ID: " + roadEventToken.EventID;
					text = text + " Current Screen: " + roadEventToken.CurrentScreenName;
				}
				break;
			case GameActionType.BuyBlessing:
				text = text + " Blessed Actor ID: " + actorID;
				if (gameAction.SupplementaryDataToken is BlessingToken blessingToken)
				{
					text = text + " Blessed Character ID: " + blessingToken.CharacterID;
				}
				break;
			case GameActionType.ReadyUpPlayer:
				if (gameAction.SupplementaryDataToken2 is ReadyUpToken readyUpToken)
				{
					text = text + " ReadyUpToggleState: " + readyUpToken.ToggleState;
				}
				break;
			case GameActionType.DeleteCharacter:
				if (gameAction.SupplementaryDataToken is CampaignCharacterData campaignCharacterData)
				{
					text = text + " Character ID: " + campaignCharacterData.CharacterID;
					text = text + " Character Name: " + campaignCharacterData.CharacterName;
				}
				break;
			case GameActionType.ClaimAchievementReward:
				if (gameAction.SupplementaryDataToken is AchievementToken achievementToken)
				{
					text = text + " Achievement Name: " + achievementToken.AchievementName;
				}
				break;
			}
		}
		catch
		{
		}
		Console.LogCoreInfo("[[Sending GameAction #" + gameAction.ActionID + " (" + ((GameActionType)gameAction.ActionTypeID/*cast due to .constrained prefix*/).ToString() + " @ " + ((ActionPhaseType)gameAction.TargetPhaseID/*cast due to .constrained prefix*/).ToString() + ") to " + (FFSNetwork.IsHost ? "clients." : "server.") + text + "]]", customFlag: true);
		if (syncViaStateUpdate)
		{
			NetworkControllable controllable = ControllableRegistry.GetControllable(gameAction.ActorID);
			if (controllable == null)
			{
				FFSNetwork.HandleDesync(new Exception("Error syncing controllable state. No controllable exists with ControllableID: " + gameAction.ActorID));
				return;
			}
			controllable.UpdateState(gameAction);
		}
		else if (FFSNetwork.IsHost)
		{
			if (executeLocallyFirst)
			{
				ActionProcessor.QueueUpAction(gameAction);
			}
			else
			{
				foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
				{
					if (allPlayer != PlayerRegistry.MyPlayer)
					{
						SendGameActionToPlayer(gameAction, allPlayer);
					}
				}
			}
		}
		if (FFSNetwork.IsClient)
		{
			SendGameActionToPlayer(gameAction, PlayerRegistry.HostPlayer);
		}
	}

	private static void SendGameActionToPlayer(GameAction action, NetworkPlayer targetPlayer)
	{
		if (targetPlayer == null)
		{
			Console.LogWarning("Error sending game action. The target player does not exist anymore.");
		}
		else if (action.ClassID == null)
		{
			GameActionEvent gameActionEvent = ((!(targetPlayer == PlayerRegistry.HostPlayer)) ? GameActionEvent.Create(targetPlayer.NetStats.Connection, ReliabilityModes.ReliableOrdered) : GameActionEvent.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered));
			gameActionEvent.ActionID = action.ActionID;
			gameActionEvent.ActionTypeID = action.ActionTypeID;
			gameActionEvent.PlayerID = action.PlayerID;
			gameActionEvent.ActorID = action.ActorID;
			gameActionEvent.TargetPhaseID = action.TargetPhaseID;
			gameActionEvent.SupplementaryDataIDMin = action.SupplementaryDataIDMin;
			gameActionEvent.SupplementaryDataIDMed = action.SupplementaryDataIDMed;
			gameActionEvent.SupplementaryDataIDMax = action.SupplementaryDataIDMax;
			gameActionEvent.SupplementaryDataBoolean = action.SupplementaryDataBoolean;
			gameActionEvent.SupplementaryDataGuid = action.SupplementaryDataGuid;
			gameActionEvent.SupplementaryDataToken = action.SupplementaryDataToken;
			gameActionEvent.SupplementaryDataToken2 = action.SupplementaryDataToken2;
			gameActionEvent.SupplementaryDataToken3 = action.SupplementaryDataToken3;
			gameActionEvent.SupplementaryDataToken4 = action.SupplementaryDataToken4;
			gameActionEvent.BinaryData = action.CustomBinaryData;
			gameActionEvent.SyncViaStateUpdate = action.SyncViaStateUpdate;
			gameActionEvent.ValidateAction = action.ValidateAction;
			gameActionEvent.DoNotForwardAction = action.DoNotForwardAction;
			gameActionEvent.BinaryDataIncludesLoggingDetails = action.BinaryDataIncludesLoggingDetails;
			gameActionEvent.Send();
		}
		else
		{
			GameActionEventClassID gameActionEventClassID = ((!(targetPlayer == PlayerRegistry.HostPlayer)) ? GameActionEventClassID.Create(targetPlayer.NetStats.Connection, ReliabilityModes.ReliableOrdered) : GameActionEventClassID.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered));
			gameActionEventClassID.ActionID = action.ActionID;
			gameActionEventClassID.ActionTypeID = action.ActionTypeID;
			gameActionEventClassID.PlayerID = action.PlayerID;
			gameActionEventClassID.ActorID = action.ActorID;
			gameActionEventClassID.TargetPhaseID = action.TargetPhaseID;
			gameActionEventClassID.SupplementaryDataIDMin = action.SupplementaryDataIDMin;
			gameActionEventClassID.SupplementaryDataBoolean = action.SupplementaryDataBoolean;
			gameActionEventClassID.BinaryData = action.CustomBinaryData;
			gameActionEventClassID.ClassID = action.ClassID;
			gameActionEventClassID.Send();
		}
	}

	public static void ForwardGameActionToClients(GameAction action)
	{
		if (FFSNetwork.HasDesynchronized)
		{
			return;
		}
		Console.LogInfo("Forwarding action #" + action.ActionID + " (" + ((GameActionType)action.ActionTypeID/*cast due to .constrained prefix*/).ToString() + ") to " + (action.ValidateAction ? "all clients." : "remaining clients."));
		if (action.SyncViaStateUpdate)
		{
			ControllableRegistry.GetControllable(action.ActorID).UpdateState(action);
			return;
		}
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		foreach (NetworkPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer != PlayerRegistry.MyPlayer && (action.ValidateAction || allPlayer != player))
			{
				SendGameActionToPlayer(action, allPlayer);
			}
		}
	}

	public static void RequestPlayerEntity()
	{
		if (!FFSNetwork.HasDesynchronized)
		{
			PlayerEntityRequest.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered).Send();
		}
	}

	public static void RequestControllable(bool onlyIfHasNoControllables = true)
	{
		if (FFSNetwork.HasDesynchronized)
		{
			return;
		}
		if (onlyIfHasNoControllables)
		{
			if (PlayerRegistry.MyPlayer == null)
			{
				PlayerRegistry.MyPlayerInitialized.AddListener(delegate
				{
					RequestControllable();
				});
				return;
			}
			if (PlayerRegistry.MyPlayer.MyControllables.Count != 0)
			{
				return;
			}
		}
		ControllableAssignmentRequest.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered).Send();
	}

	public static void RequestCustomData(DataActionType dataActionType)
	{
		Console.LogInfo("Requesting custom data (" + dataActionType.ToString() + ") from the server.");
		GameDataRequest gameDataRequest = GameDataRequest.Create(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
		gameDataRequest.DataActionID = (int)dataActionType;
		gameDataRequest.Send();
	}

	public static void RequestCustomDataFromClients(DataActionType dataActionType)
	{
		Console.LogInfo("Requesting custom data (" + dataActionType.ToString() + ") from the clients.");
		GameDataRequest gameDataRequest = GameDataRequest.Create(GlobalTargets.AllClients, ReliabilityModes.ReliableOrdered);
		gameDataRequest.DataActionID = (int)dataActionType;
		gameDataRequest.Send();
	}

	public static void StreamCustomData(DataActionType actionType, object obj, NetworkTargets targets)
	{
		Singleton<DataStreamingManager>.Instance.CompressAndSendDataInChunks(actionType, Utility.ObjectToByteArray(obj), targets);
	}

	public static void StreamCustomData(DataActionType actionType, object obj, BoltConnection connection)
	{
		Singleton<DataStreamingManager>.Instance.CompressAndSendDataInChunks(actionType, Utility.ObjectToByteArray(obj), NetworkTargets.TargetClient, connection);
	}

	public static void StreamCustomData(DataActionType actionType, byte[] data, BoltConnection connection)
	{
		Singleton<DataStreamingManager>.Instance.CompressAndSendDataInChunks(actionType, data, NetworkTargets.TargetClient, connection);
	}

	public static void NotifyJoiningPlayersAboutReachingSavePoint()
	{
		try
		{
			if (FFSNetwork.HasDesynchronized)
			{
				return;
			}
			foreach (BoltConnection joiningPlayer in PlayerRegistry.JoiningPlayers)
			{
				if (joiningPlayer != null)
				{
					SavePointReachedEvent.Create(joiningPlayer, ReliabilityModes.ReliableOrdered).Send();
				}
			}
			global::Singleton<UIMapMultiplayerController>.Instance?.RefreshWaitingNotifications();
		}
		catch (Exception ex)
		{
			Console.LogWarning("Exception in NotifyJoiningPlayersAboutReachingSavePoint.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}
}
