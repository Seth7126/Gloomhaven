#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Script.Networking.Tokens;
using FFSNet;
using GLOOM;
using GLOOM.MainMenu;
using MapRuleLibrary.Party;
using Photon.Bolt;
using Platforms;
using Platforms.Social;
using ScenarioRuleLibrary;
using UdpKit;
using UnityEngine;

[BoltGlobalBehaviour]
public sealed class GHNetworkCallbacks : NetworkCallbacks
{
	private const string ReliableStreamChannelName = "ReliableStream";

	private static UdpChannelName m_ReliableStreamChannel;

	public static UdpChannelName ReliableStreamChannel => m_ReliableStreamChannel;

	public override void BoltStartBegin()
	{
		base.BoltStartBegin();
		BoltNetwork.RegisterTokenClass<LocationToken>();
		BoltNetwork.RegisterTokenClass<AbilityAugmentToken>();
		BoltNetwork.RegisterTokenClass<AbilityAugmentGroupToken>();
		BoltNetwork.RegisterTokenClass<ActiveBonusToken>();
		BoltNetwork.RegisterTokenClass<ActiveBonusesToken>();
		BoltNetwork.RegisterTokenClass<TileToken>();
		BoltNetwork.RegisterTokenClass<TilesToken>();
		BoltNetwork.RegisterTokenClass<TargetSelectionToken>();
		BoltNetwork.RegisterTokenClass<CardsToken>();
		BoltNetwork.RegisterTokenClass<CardInventoryToken>();
		BoltNetwork.RegisterTokenClass<ItemToken>();
		BoltNetwork.RegisterTokenClass<ItemsToken>();
		BoltNetwork.RegisterTokenClass<ItemInventoryToken>();
		BoltNetwork.RegisterTokenClass<LevelToken>();
		BoltNetwork.RegisterTokenClass<PerkPointsToken>();
		BoltNetwork.RegisterTokenClass<PerksToken>();
		BoltNetwork.RegisterTokenClass<AchievementToken>();
		BoltNetwork.RegisterTokenClass<EnhancementToken>();
		BoltNetwork.RegisterTokenClass<InfuseToken>();
		BoltNetwork.RegisterTokenClass<ControllableStateRevisionToken>();
		BoltNetwork.RegisterTokenClass<ChooseAbilityToken>();
		BoltNetwork.RegisterTokenClass<BattleGoalSelectionToken>();
		BoltNetwork.RegisterTokenClass<CampaignCharacterData>();
		BoltNetwork.RegisterTokenClass<CampaignPersonalQuestData>();
		BoltNetwork.RegisterTokenClass<IndexToken>();
		BoltNetwork.RegisterTokenClass<RoadEventToken>();
		BoltNetwork.RegisterTokenClass<BlessingToken>();
		BoltNetwork.RegisterTokenClass<ReadyUpToken>();
		BoltNetwork.RegisterTokenClass<QuestCompletionToken>();
		BoltNetwork.RegisterTokenClass<IdListToken>();
		BoltNetwork.RegisterTokenClass<StartRoundCardsToken>();
		BoltNetwork.RegisterTokenClass<BlockedUsersDataToken>();
		m_ReliableStreamChannel = BoltNetwork.CreateStreamChannel("ReliableStream", UdpChannelMode.Reliable, 1);
	}

	public override void BoltStartDone()
	{
		if (FFSNetwork.IsHost)
		{
			foreach (NetworkControllable allControllable in ControllableRegistry.AllControllables)
			{
				BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.GHControllableState, new ControllableToken(allControllable.ID));
				GHNetworkControllable component = boltEntity.GetComponent<GHNetworkControllable>();
				boltEntity.TakeControl();
				if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
				{
					if (allControllable.ControllableObject is CharacterManager { CharacterActor: var characterActor })
					{
						CAbilityCard initiativeAbilityCard = ((CPlayerActor)characterActor).CharacterClass.InitiativeAbilityCard;
						List<AbilityCardUI> selectedCards = CardsHandManager.Instance.GetSelectedCards(characterActor.ID);
						if (initiativeAbilityCard != null && selectedCards != null && selectedCards.Count == 2)
						{
							Console.LogInfo("Checking initiative \n Initiative Card ID: " + initiativeAbilityCard.ID + " Initiative CardInstanceID: + " + initiativeAbilityCard.CardInstanceID + "\nSelectedCards[0] ID: " + (selectedCards[0].IsLongRest ? "LongRest" : selectedCards[0].AbilityCard.ID.ToString()) + " SelectedCards[0] CardInstanceID: " + (selectedCards[0].IsLongRest ? "LongRest" : selectedCards[0].AbilityCard.CardInstanceID.ToString()) + "\nSelectedCards[1] ID: " + (selectedCards[1].IsLongRest ? "LongRest" : selectedCards[1].AbilityCard.ID.ToString()) + " SelectedCards[1] CardInstanceID: " + (selectedCards[1].IsLongRest ? "LongRest" : selectedCards[1].AbilityCard.CardInstanceID.ToString()));
							if (selectedCards[1].AbilityCard.ID != initiativeAbilityCard.ID)
							{
								selectedCards.Reverse();
							}
						}
						if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
						{
							component.state.StartingTile = new TileToken(characterActor.ArrayIndex);
							Console.LogInfo("Round Cards set: " + string.Join(", ", selectedCards.Select((AbilityCardUI s) => s.CardName)));
							component.state.StartRoundCards = CardsHandManager.Instance.GetHand(characterActor.ID).CachedStartRoundToken;
						}
					}
					else
					{
						Debug.LogError("Invalid type for controllable.ControllableObject.  Type: " + allControllable.ControllableObject.GetType().ToString());
					}
				}
				else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
				{
					CMapCharacter cMapCharacter = null;
					if (allControllable.ControllableObject is NewPartyCharacterUI newPartyCharacterUI)
					{
						cMapCharacter = newPartyCharacterUI.Data;
					}
					else if (allControllable.ControllableObject is BenchedCharacter benchedCharacter)
					{
						cMapCharacter = benchedCharacter.CharacterData;
					}
					component.state.Level = new LevelToken(cMapCharacter.Level);
					component.state.PerkPoints = new PerkPointsToken(cMapCharacter.PerkPoints);
					component.state.ActivePerks = new PerksToken(cMapCharacter.Perks);
					component.state.CardInventory = new CardInventoryToken(cMapCharacter.HandAbilityCardIDs, cMapCharacter.OwnedAbilityCardIDs);
					component.state.ItemInventory = new ItemInventoryToken(cMapCharacter, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode);
				}
			}
		}
		base.BoltStartDone();
	}

	public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
		Console.LogError("ERROR_MULTIPLAYER", "BoltStartFailed: UdpConnectionDisconnectReason: " + disconnectReason);
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_ERROR_MULTIPLAYER_CONNECTION_NOTIFICATION");
		PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation, null);
		FFSNetwork.IsStartingUp = false;
		base.BoltStartFailed(disconnectReason);
	}

	private void OnNetworkDisconnected()
	{
	}

	public override void BoltShutdownBegin(AddCallback callback, UdpConnectionDisconnectReason disconnectReason)
	{
		if (FFSNetwork.IsHost && !FFSNetwork.HasDesynchronized && disconnectReason != UdpConnectionDisconnectReason.Disconnected)
		{
			Console.LogError("ERROR_MULTIPLAYER_00038", "Disconnected from the session. Reason: " + disconnectReason);
			string translation = LocalizationManager.GetTranslation("Consoles/GUI_ERROR_MULTIPLAYER_CONNECTION_NOTIFICATION");
			PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation, null);
			UIMultiplayerNotifications.ShowFailedHostSession();
			OnNetworkDisconnected();
		}
		else if (FFSNetwork.IsClient && !FFSNetwork.HasDesynchronized && disconnectReason != UdpConnectionDisconnectReason.Disconnected)
		{
			Console.LogError("ERROR_MULTIPLAYER_00038", "Disconnected from the session. Reason: " + disconnectReason);
			string translation2 = LocalizationManager.GetTranslation("Consoles/GUI_ERROR_MULTIPLAYER_CONNECTION_NOTIFICATION");
			PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation2, null);
			UIMultiplayerNotifications.ShowFailedConnectToSession();
			OnNetworkDisconnected();
		}
		if (UIMultiplayerJoinSessionWindow.s_This != null && UIMultiplayerJoinSessionWindow.s_This.CurrentConnectionState == ConnectionState.DownloadingNewSave && !FFSNetwork.IsShuttingDown)
		{
			UIMultiplayerJoinSessionWindow.s_This.OnConnectionFailed(ConnectionErrorCode.ConnectionToSessionFailed);
			FFSNetwork.Shutdown();
		}
		if (UIMultiplayerJoinSessionWindow.s_This != null && UIMultiplayerJoinSessionWindow.s_This.CurrentConnectionState == ConnectionState.WaitUntilSavePoint && !FFSNetwork.IsShuttingDown)
		{
			UIMultiplayerJoinSessionWindow.s_This.OnConnectionFailed(ConnectionErrorCode.SessionShutDown);
			FFSNetwork.Shutdown();
		}
		base.BoltShutdownBegin(callback, disconnectReason);
		if (FFSNetwork.IsClient && SaveData.Instance.Global.CurrentGameState != EGameState.None && !SceneController.Instance.GlobalErrorMessage.ShowingMessage)
		{
			if (!FFSNetwork.IsKickedState)
			{
				string translation3 = LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_CONNECTION_FAILED_KickedByHost");
				PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation3, null);
			}
			else
			{
				FFSNetwork.IsKickedState = false;
			}
			if (!SceneController.Instance.IsLoading)
			{
				Singleton<ESCMenu>.Instance.LoadMainMenu(skipConfirmation: true);
			}
			else
			{
				UIManager.LoadMainMenuAfterSceneLoaded();
			}
		}
	}

	private bool CheckVersions(string tokenVersion)
	{
		return tokenVersion == NetworkVersion.Current;
	}

	private async Task<bool> IsPlayerInBlockedList(string id)
	{
		bool processing = true;
		bool isInBlockedList = false;
		PlatformLayer.Networking.GetPermissionsTowardsPlatformUsersAsync(new HashSet<string> { id }, OnGetPermissionsTowardsPlatformUsers);
		while (processing)
		{
			await Task.Delay(50);
		}
		return isInBlockedList;
		void OnGetPermissionsTowardsPlatformUsers(OperationResult operationResultResponse, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>> usersResponse)
		{
			if (operationResultResponse == OperationResult.Success)
			{
				isInBlockedList = usersResponse.First().Value[Permission.PlayMultiplayer][0] == PermissionOperationResult.UserInBlockList;
			}
			processing = false;
		}
	}

	public override async void ConnectRequest(UdpEndPoint endPoint, IProtocolToken token)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.HasDesynchronized)
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.SessionShuttingDown));
			return;
		}
		UserToken userToken = (UserToken)token;
		bool flag = false;
		if (userToken == null)
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.InvalidUserData));
		}
		else if (!flag && userToken.GameVersion != "DevVersion" && !CheckVersions(userToken.GameVersion))
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.DifferentVersion));
		}
		else if (!PassesGHPlayerLimitationChecks())
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.SessionFull));
		}
		else if (SaveData.Instance.Global.CrossplayEnabled && !userToken.CrossplayEnabled)
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.CrossplayDisabledByClient));
		}
		else if (!SaveData.Instance.Global.CrossplayEnabled && !PlatformLayer.MatchesCurrentPlatform(userToken.PlatformName))
		{
			BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.CrossplayDisabledByServer));
		}
		else
		{
			if (!PassesBasicConnectionTests(endPoint, userToken, string.Empty))
			{
				return;
			}
			if (!CheckVersions(userToken.GameVersion))
			{
				Debug.LogError("Host and client versions do not match.  Host: " + Application.version + " Client: " + userToken.GameVersion);
			}
			PlayerRegistry.ConnectingUsers.Add(userToken);
			PlayerRegistry.OnUserEnterRoom?.Invoke(userToken);
			string customLevelWorkshopID = string.Empty;
			_ = string.Empty;
			string saveName;
			bool waitUntilSavePoint;
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
				saveName = SaveData.Instance.Global.CampaignData.PartyName;
				waitUntilSavePoint = SceneController.Instance.SelectingPersonalQuest || SceneController.Instance.BusyProcessingResults || SceneController.Instance.RetiringCharacter || SceneController.Instance.CheckingLockedContent || (FFSNetwork.IsOnline && PlayerRegistry.AllPlayers.Any((NetworkPlayer a) => a.IsCreatingCharacter)) || (Singleton<MapChoreographer>.Instance != null && Singleton<MapChoreographer>.Instance.ImportingQuestCompletions) || !ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.StartOfRound);
				break;
			case EGameMode.Guildmaster:
				saveName = SaveData.Instance.Global.AdventureData.PartyName;
				waitUntilSavePoint = !ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.StartOfRound);
				break;
			case EGameMode.SingleScenario:
				customLevelWorkshopID = SaveData.Instance.LevelEditorDataManager.GetModMetadataForCustomLevel(SaveData.Instance.Global.CurrentCustomLevelData).PublishedFileId.ToString();
				saveName = SaveData.Instance.Global.CurrentCustomLevelData.Name;
				waitUntilSavePoint = ActionProcessor.CurrentPhase != ActionPhaseType.StartOfRound;
				break;
			default:
				Console.LogError("ERROR_MULTIPLAYER_00013", "Error accepting a connection. Playing non-multiplayer game mode (" + SaveData.Instance.Global.GameMode.ToString() + "). Disconnecting.");
				BoltNetwork.Refuse(endPoint, new ConnectionErrorToken(ConnectionErrorCode.SessionNotFound));
				FFSNetwork.Shutdown();
				PlayerRegistry.ConnectingUsers.RemoveAll((UserToken x) => x.Username == userToken.Username && x.PlatformPlayerID == userToken.PlatformPlayerID);
				return;
			}
			Debug.Log(userToken.Username + " User connecting at phase " + ActionProcessor.CurrentPhase.ToString() + " has to wait? " + waitUntilSavePoint + ", connecting " + string.Join(",", PlayerRegistry.ConnectingUsers.Select((UserToken it) => it.Username)));
			bool isModded = false;
			string rulesetHash = string.Empty;
			if (!string.IsNullOrEmpty(SaveData.Instance.Global.CurrentModdedRuleset))
			{
				GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.SingleOrDefault((GHRuleset s) => s.Name == SaveData.Instance.Global.CurrentModdedRuleset);
				if (gHRuleset != null && gHRuleset.IsCompiled)
				{
					isModded = true;
					rulesetHash = (gHRuleset.CompiledHash = gHRuleset.GetRulesetHash());
				}
			}
			HashSet<string> usersIds = new HashSet<string>();
			foreach (BoltConnection client in BoltNetwork.Clients)
			{
				UserToken userToken2 = (UserToken)client.ConnectToken;
				if (userToken2.PlatformName == userToken.PlatformName)
				{
					usersIds.Add(userToken2.PlatformNetworkAccountPlayerID);
				}
			}
			if (PlatformLayer.Instance.PlatformID == userToken.PlatformName)
			{
				usersIds.Add(PlatformLayer.UserData.PlatformNetworkAccountPlayerID);
			}
			Console.LogInfo("Host enabled DLC flag on the save file to send: " + SaveData.Instance.Global.CurrentAdventureData.DLCEnabled);
			string saveHash = await Task.Run(() => GloomUtility.GetMapStateHash());
			EGameMode gameMode = SaveData.Instance.Global.GameMode;
			string saveName2 = saveName;
			string platformPlayerID = PlatformLayer.UserData.PlatformPlayerID;
			string platformAccountID = PlatformLayer.UserData.PlatformAccountID;
			string platformNetworkAccountPlayerID = PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
			string userName = PlatformLayer.UserData.UserName;
			string platformID = PlatformLayer.Instance.PlatformID;
			string currentModdedRuleset = SaveData.Instance.Global.CurrentModdedRuleset;
			uint dLCEnabled = (uint)SaveData.Instance.Global.CurrentAdventureData.DLCEnabled;
			GameToken gameToken = new GameToken(isModded: isModded, dlcFlag: dLCEnabled, currentPlatformUsersInSession: usersIds, rulesetHash: rulesetHash, customLevelWorkshopID: customLevelWorkshopID, waitUntilSavePoint: waitUntilSavePoint, gameModeID: (int)gameMode, saveName: saveName2, saveHash: saveHash, hostPlayerID: platformPlayerID, hostAccountID: platformAccountID, hostNetworkAccountID: platformNetworkAccountPlayerID, hostUsername: userName, hostPlatformName: platformID, customRulesetName: currentModdedRuleset, isCrossplaySession: SaveData.Instance.Global.CrossplayEnabled);
			switch (PlatformLayer.Instance.PlatformID)
			{
			case "Standalone":
			case "Steam":
			case "GoGGalaxy":
			case "EpicGamesStore":
				gameToken.MaskBadWordsInUsername(delegate
				{
					BoltNetwork.Accept(endPoint, gameToken);
				});
				break;
			default:
				BoltNetwork.Accept(endPoint, gameToken);
				break;
			}
		}
	}

	private bool PassesGHPlayerLimitationChecks()
	{
		Debug.Log("Starting PassesGHPlayerLimitationChecks");
		if (SaveData.Instance == null || SaveData.Instance.Global == null)
		{
			return false;
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario || (Singleton<MapChoreographer>.Instance != null && !Singleton<MapChoreographer>.Instance.PartyAtHQ))
		{
			List<CMapCharacter> list = SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState?.MapParty?.SelectedCharacters.ToList();
			if (list == null)
			{
				Debug.Log("PassesGHPlayerLimitationChecks Failed.  selectedCharacters is null.");
				return false;
			}
			bool flag = PlayerRegistry.AllPlayers.Count + PlayerRegistry.JoiningPlayers.Count + PlayerRegistry.ConnectingUsers.Count < list.Count;
			Debug.Log("PassesGHPlayerLimitationChecks " + (flag ? "Passed" : "Failed!") + " AllPlayers.Count: " + PlayerRegistry.AllPlayers.Count + "  JoiningPlayers: " + PlayerRegistry.JoiningPlayers.Count + "  ConnectingUsers: " + PlayerRegistry.ConnectingUsers.Count + "  selectedCharacters: " + list.Count);
			return flag;
		}
		if (Singleton<MapChoreographer>.Instance == null)
		{
			Debug.Log("PassesGHPlayerLimitationChecks Failed.  MapChoreographer Instance is null.");
			return false;
		}
		Debug.Log("PassesGHPlayerLimitationChecks Passed");
		return true;
	}

	protected override void InitializeGameState()
	{
		base.InitializeGameState();
	}

	protected override void SaveLatestProcessedActionIDToSaveFile()
	{
	}
}
