#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using GLOOM;
using GLOOM.MainMenu;
using MapRuleLibrary.State;
using Photon.Bolt;
using Platforms;
using Platforms.Activities;
using Platforms.Social;
using SM.Utils;
using ScenarioRuleLibrary;
using SharedLibrary.SimpleLog;
using UdpKit;
using UnityEngine;

[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public sealed class GHClientCallbacks : ClientCallbacks
{
	public enum EStreamMode
	{
		NotSet,
		ReceivingRulesetZip,
		ReceivingRulesetInstance,
		ReceivingCSVFiles,
		ReceivingSimpleLog,
		ReceivingCompareState,
		ReceivingInitialGameState,
		ReceivingPlayerLog
	}

	private static EStreamMode m_CurrentStreamMode;

	private GameToken gameData;

	private string targetSaveFilePath;

	private string m_LastRulesetHash;

	private List<string> m_ModCSVs;

	private const string DefaultSaveNameLocalisationKey = "Consoles/DEFAULT_SAVE_NAME";

	private const string UgcDisabledLocalisationKey = "Consoles/UGC_DISABLED_CONNECTION_MESSAGE";

	public static EStreamMode CurrentStreamMode
	{
		get
		{
			return m_CurrentStreamMode;
		}
		set
		{
			m_CurrentStreamMode = value;
		}
	}

	public override void Connected(BoltConnection connection)
	{
		base.Connected(connection);
		m_CurrentStreamMode = EStreamMode.NotSet;
		gameData = connection.AcceptToken as GameToken;
		if (gameData == null)
		{
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00006", "Invalid session data provided. Disconnecting from the session.");
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.InvalidSessionData);
			FFSNetwork.Shutdown();
		}
		else
		{
			EGameMode gameMode = (EGameMode)gameData.GameModeID;
			IsUsersInSessionAvailableToPlayWith(gameData.CurrentPlatformUsersInSession, delegate(Tuple<OperationResult, PermissionOperationResult> result)
			{
				PlatformLayer.Networking.GetCurrentUserPrivilegesAsync(OnGetCurrentUserUgcPrivilege, PrivilegePlatform.Xbox, Privilege.UserGeneratedContent);
				void LaunchMultiplayer()
				{
					if (result.Item1 != OperationResult.Success || result.Item2 != PermissionOperationResult.Success)
					{
						ConnectionErrorCode arg = ConnectionErrorCode.None;
						if (result.Item2 == PermissionOperationResult.NotAllowed)
						{
							arg = ConnectionErrorCode.CurrentUserBlockedByUserInSession;
						}
						else if (result.Item2 == PermissionOperationResult.UserInBlockList)
						{
							arg = ((Application.platform != RuntimePlatform.Switch) ? ConnectionErrorCode.UserInSessionBlockedByCurrentUser : ConnectionErrorCode.SessionCouldNotBeJoined);
						}
						FFSNetwork.Manager.OnConnectionFailed?.Invoke(arg);
						FFSNetwork.Shutdown();
					}
					else
					{
						FFSNet.Console.LogInfo("Receiving game data from an on-going session.");
						FFSNet.Console.LogInfo("Game Mode : " + gameMode);
						FFSNet.Console.LogInfo("Save Name: " + gameData.SaveName);
						if (gameMode.In(EGameMode.Guildmaster, EGameMode.Campaign, EGameMode.SingleScenario))
						{
							targetSaveFilePath = (gameMode.Equals(EGameMode.SingleScenario) ? SaveData.Instance.RootData.GetDefaultFilePathForCustomLevel(gameData.SaveName, gameData.CustomLevelWorkshopID) : SaveData.Instance.RootData.GetFilePathForParty((EGameMode)gameData.GameModeID, gameData.SaveName, (Application.platform == RuntimePlatform.Switch) ? gameData.HostNetworkAccountID : gameData.HostAccountID));
							FFSNet.Console.LogInfo("Target file Path: " + targetSaveFilePath);
							if (targetSaveFilePath.IsNullOrEmpty())
							{
								FFSNet.Console.LogError("ERROR_MULTIPLAYER_00007", "Could not process the target save file path. Disconnecting from the session.");
								FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.InvalidFilePath);
								FFSNetwork.Shutdown();
							}
							else if (gameData.WaitUntilSavePoint)
							{
								Debug.Log("Stopped WaitUntilSavePoint");
								FFSNetwork.Manager.OnConnectionStateUpdated?.Invoke(ConnectionState.WaitUntilSavePoint);
							}
							else
							{
								ProceedToComparingSaveFiles();
							}
						}
						else
						{
							StartCoroutine(TryLaunchMultiplayerSave(gameData));
						}
					}
				}
				void OnGetCurrentUserUgcPrivilege(OperationResult operationResult, bool isPrivilegeValid)
				{
					FFSNetwork.IsUGCEnabled = operationResult == OperationResult.Success && isPrivilegeValid;
					if (!FFSNetwork.IsUGCEnabled)
					{
						PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, LocalizationManager.GetTranslation("Consoles/UGC_DISABLED_CONNECTION_MESSAGE"), delegate
						{
							LaunchMultiplayer();
						});
					}
					else
					{
						LaunchMultiplayer();
					}
				}
			});
		}
	}

	public override void OnEvent(SessionNegotiationEvent evnt)
	{
		SessionMessageType messageType = (SessionMessageType)evnt.MessageType;
		PlatformType platform = (PlatformType)evnt.Platform;
		LogUtils.Log($"NetworkCallbacksClient {evnt.Data} {messageType} {platform}");
		switch (messageType)
		{
		case SessionMessageType.ServerSessionResponse:
			PlatformLayer.Networking.JoinSession(evnt.Data, OnSessionCreated);
			break;
		case SessionMessageType.ServerCreateSessionRequest:
			PlatformLayer.Networking.CreateClientSession(evnt.Data, ServerCreateSessionRequestCallback);
			break;
		default:
			throw new Exception($"Not implemented {messageType}");
		}
	}

	private void OnSessionCreated(OperationResult result)
	{
		LogUtils.Log($"OnSessionCreated {result}");
		if (result == OperationResult.Success)
		{
			FFSNetwork.OnSessionReceivedFromHost(PlatformLayer.Platform.PlatformSocial.GetPSPlayerSession());
			return;
		}
		throw new Exception($"Invalid result {result}");
	}

	private void ServerCreateSessionRequestCallback(OperationResult result)
	{
		LogUtils.Log($"ServerCreateSessionRequestCallback {result}");
		if (result == OperationResult.Success)
		{
			string pSPlayerSession = PlatformLayer.Platform.PlatformSocial.GetPSPlayerSession();
			LogUtils.Log($"Send response to server {result} {pSPlayerSession}");
			NetworkUtils.SendSessionCommunicationEvent(SessionMessageType.ClientSessionCreatedResponse, pSPlayerSession, BoltNetwork.Server);
			FFSNetwork.OnSessionReceivedFromHost(pSPlayerSession);
			return;
		}
		throw new Exception($"Invalid result {result}");
	}

	public override void OnEvent(SavePointReachedEvent evnt)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a SavePointReachedEvent but the event returns null."));
		}
		else if (UIMultiplayerJoinSessionWindow.s_This != null && UIMultiplayerJoinSessionWindow.s_This.CurrentConnectionState == ConnectionState.WaitUntilSavePoint)
		{
			FFSNetwork.Manager.OnConnectionStateUpdated?.Invoke(ConnectionState.SavePointReached);
			ProceedToComparingSaveFiles(autoFailSaveComparison: true);
		}
	}

	private async void ProceedToComparingSaveFiles(bool autoFailSaveComparison = false)
	{
		if (gameData.IsModded)
		{
			m_LastRulesetHash = gameData.RulesetHash;
			GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.Where((GHRuleset w) => !w.LinkedMods.Any((GHMod a) => a.IsLocalMod)).SingleOrDefault((GHRuleset s) => s.IsCompiled && s.CompiledHash == gameData.RulesetHash);
			if (gHRuleset != null)
			{
				SaveData.Instance.Global.CurrentModdedRuleset = gHRuleset.Name;
				StartCoroutine(LoadRulesetCoroutine(gHRuleset));
			}
			else
			{
				m_CurrentStreamMode = EStreamMode.ReceivingRulesetInstance;
				Synchronizer.RequestCustomData(DataActionType.SendRulesetInstance);
			}
		}
		else
		{
			LogUtils.LogWarning("GHClientCallback.ProceedToComparingSaveFiles. TargetSaveFilePath = " + targetSaveFilePath);
			string text = await Task.Run(() => GloomUtility.GetMapStateHash(targetSaveFilePath));
			if (!autoFailSaveComparison && text != string.Empty && text == gameData.SaveHash)
			{
				FFSNet.Console.LogInfo("The save files match.");
				StartCoroutine(TryLaunchMultiplayerSave(gameData, targetSaveFilePath));
				return;
			}
			FFSNet.Console.LogInfo("Waiting for the host to start streaming the save file.");
			FFSNetwork.Manager.OnConnectionStateUpdated?.Invoke(ConnectionState.DownloadingNewSave);
			m_CurrentStreamMode = EStreamMode.ReceivingInitialGameState;
			Synchronizer.RequestCustomData(DataActionType.SendInitialGameState);
		}
	}

	public override void OnEvent(GameDataRequest request)
	{
		if (request == null)
		{
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00010", "Received a GameDataRequest but the request returns null.");
			return;
		}
		if (request.RaisedBy == null)
		{
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00011", "Received a GameDataRequest for " + ((DataActionType)request.DataActionID/*cast due to .constrained prefix*/).ToString() + " but the requesting player does not exist anymore.");
			return;
		}
		switch ((DataActionType)request.DataActionID)
		{
		case DataActionType.SendSimpleLogFromClient:
			SimpleLog.WriteSimpleLogToFile();
			if (PlatformLayer.FileSystem.ExistsFile(RootSaveData.SimpleLogPath))
			{
				Synchronizer.StreamCustomData(DataActionType.SendSimpleLogFromClient, GetBytesFromFilePath(RootSaveData.SimpleLogPath), request.RaisedBy);
			}
			break;
		case DataActionType.SendPlayerLogFromClient:
			if (PlatformLayer.FileSystem.ExistsFile(RootSaveData.PlayerLogPath))
			{
				Synchronizer.StreamCustomData(DataActionType.SendPlayerLogFromClient, GetBytesFromFilePath(RootSaveData.PlayerLogPath), request.RaisedBy);
			}
			break;
		default:
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00012", "Unprocessable DataActionType found: " + (DataActionType)request.DataActionID/*cast due to .constrained prefix*/);
			break;
		}
	}

	private byte[] GetBytesFromFilePath(string pathToOutputLog)
	{
		using FileStream fileStream = File.Open(pathToOutputLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using MemoryStream memoryStream = new MemoryStream();
		fileStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private IEnumerator LoadRulesetCoroutine(GHRuleset ruleset)
	{
		yield return SceneController.Instance.YML.LoadRulesetZip(ruleset);
		if (YMLLoading.LastLoadResult)
		{
			FFSNetwork.Manager.OnConnectionStateUpdated?.Invoke(ConnectionState.DownloadingNewSave);
			m_CurrentStreamMode = EStreamMode.ReceivingInitialGameState;
			Synchronizer.RequestCustomData(DataActionType.SendInitialGameState);
		}
		else
		{
			FFSNetwork.HandleDesync(new Exception("Error loading modded ruleset"));
		}
	}

	private void WriteAndLaunchSaveFile(byte[] saveFileData)
	{
		new FileInfo(targetSaveFilePath);
		bool flag = PlatformLayer.FileSystem.ExistsDirectory(targetSaveFilePath);
		if (PlatformLayer.Instance.PlatformID != "GameCore" && !flag)
		{
			PlatformLayer.FileSystem.CreateDirectory(Path.GetDirectoryName(targetSaveFilePath));
		}
		PlatformLayer.FileSystem.WriteFile(saveFileData, targetSaveFilePath);
		if (!PlatformLayer.FileSystem.ExistsFile(targetSaveFilePath))
		{
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.NotEnoughMemory);
			FFSNetwork.Shutdown();
		}
		else
		{
			StartCoroutine(TryLaunchMultiplayerSave(gameData, targetSaveFilePath));
		}
	}

	private bool CompareScenarioStates(ScenarioState receivedScenarioState)
	{
		try
		{
			List<Tuple<int, string>> list = ScenarioState.CompareStates(ScenarioManager.CurrentScenarioState, receivedScenarioState, isMPCompare: true);
			if (list.Count == 0)
			{
				FFSNet.Console.LogInfo("SCENARIO STATE COMPARISONS PASSED.");
				return true;
			}
			FFSNet.Console.LogError("", "Scenario state mismatches:");
			foreach (Tuple<int, string> item in list)
			{
				FFSNet.Console.LogError("", "Error # " + item.Item1 + ": " + item.Item2);
			}
			FFSNet.Console.LogWarning("SCENARIO STATE MISMATCHES LOGGED.");
			return false;
		}
		catch (Exception ex)
		{
			FFSNet.Console.LogError("", "Exception trying to do scenario state comparison:\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	private IEnumerator TryLaunchMultiplayerSave(GameToken gameData, FileInfo fileInfo = null)
	{
		DLCRegistry.EDLCKey dLCFlag = (DLCRegistry.EDLCKey)gameData.DLCFlag;
		FFSNet.Console.LogInfo("Host enabled DLC flag on the save file received: " + dLCFlag);
		DLCRegistry.EDLCKey invalidDLCs = PlatformLayer.DLC.GetInvalidDLCs(dLCFlag);
		if (invalidDLCs != DLCRegistry.EDLCKey.None)
		{
			ConnectionErrorCode arg = ConnectionErrorCode.None;
			if (invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC1) && invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC2))
			{
				arg = ConnectionErrorCode.DLCDoNotMatchJotlAndSoloScenarious;
			}
			else if (invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC1))
			{
				arg = ConnectionErrorCode.DLCDoNotMatchJotl;
			}
			else if (invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC2))
			{
				arg = ConnectionErrorCode.DLCDoNotMatchSoloScenarious;
			}
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(arg);
			FFSNetwork.Shutdown();
			PlatformLayer.FileSystem.RemoveFile(fileInfo.FullName);
			yield break;
		}
		bool gameClosedWhileSaving;
		Task<CMapState> task = new Task<CMapState>(() => PartyAdventureData.LoadMapStateFromFile(fileInfo.FullName, out gameClosedWhileSaving));
		task.Start();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		CMapState mapState = task.Result;
		if (mapState == null)
		{
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.NotEnoughMemory);
			FFSNetwork.Shutdown();
			yield break;
		}
		switch ((EGameMode)gameData.GameModeID)
		{
		case EGameMode.Guildmaster:
		{
			FFSNet.Console.LogInfo("Continuing adventure mode: " + gameData.SaveName);
			FFSNetwork.Manager.OnConnectionEstablished?.Invoke(gameData);
			PartyAdventureData partyAdventureData = SaveData.Instance.Global.AllAdventures.SingleOrDefault((PartyAdventureData s) => s.PartyName == gameData.SaveName && ((!gameData.HostNetworkAccountID.IsNullOrEmpty() && !gameData.HostNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == gameData.HostNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == gameData.HostAccountID)));
			if (partyAdventureData == null)
			{
				bool isMaskingInProcess = true;
				SaveOwner saveOwner = new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostNetworkAccountID, gameData.HostUsername, gameData.HostPlatformName);
				saveOwner.MaskBadWordsInUsername(delegate
				{
					isMaskingInProcess = false;
				});
				while (isMaskingInProcess)
				{
					yield return null;
				}
				partyAdventureData = new PartyAdventureData(mapState, gameData.SaveName, saveOwner, EGameMode.Guildmaster, (gameData.CustomRulesetName != string.Empty) ? (gameData.CustomRulesetName + gameData.HostAccountID) : string.Empty);
				SaveData.Instance.Global.AddAdventureSave(partyAdventureData);
			}
			else
			{
				partyAdventureData.AdventureMapState = null;
				if (PlatformLayer.IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
				if (partyAdventureData.IsModded)
				{
					partyAdventureData.RulesetName = gameData.CustomRulesetName + gameData.HostAccountID;
				}
			}
			partyAdventureData.DisplayPartyName = (FFSNetwork.IsUGCEnabled ? partyAdventureData.PartyName : LocalizationManager.GetTranslation("Consoles/DEFAULT_SAVE_NAME"));
			PlayerRegistry.LoadingInFromJoiningClient = true;
			SaveData.Instance.LoadGuildmasterMode(partyAdventureData, isJoiningMPClient: true);
			break;
		}
		case EGameMode.Campaign:
		{
			FFSNet.Console.LogInfo("Continuing campaign mode: " + gameData.SaveName);
			FFSNetwork.Manager.OnConnectionEstablished?.Invoke(gameData);
			PartyAdventureData partyAdventureData2 = SaveData.Instance.Global.AllCampaigns.SingleOrDefault((PartyAdventureData s) => s.PartyName == gameData.SaveName && ((!gameData.HostNetworkAccountID.IsNullOrEmpty() && !gameData.HostNetworkAccountID.Equals("0") && gameData.HostNetworkAccountID == s.Owner.PlatformNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == gameData.HostAccountID)));
			if (partyAdventureData2 == null)
			{
				bool isMaskingInProcess2 = true;
				SaveOwner saveOwner = new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostNetworkAccountID, gameData.HostUsername, gameData.HostPlatformName);
				saveOwner.MaskBadWordsInUsername(delegate
				{
					isMaskingInProcess2 = false;
				});
				while (isMaskingInProcess2)
				{
					yield return null;
				}
				partyAdventureData2 = new PartyAdventureData(mapState, gameData.SaveName, saveOwner, EGameMode.Campaign, gameData.CustomRulesetName);
				SaveData.Instance.Global.AllCampaigns.Add(partyAdventureData2);
			}
			else
			{
				partyAdventureData2.AdventureMapState = null;
				if (PlatformLayer.IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
			}
			partyAdventureData2.DisplayPartyName = (FFSNetwork.IsUGCEnabled ? partyAdventureData2.PartyName : LocalizationManager.GetTranslation("Consoles/DEFAULT_SAVE_NAME"));
			PlayerRegistry.LoadingInFromJoiningClient = true;
			SaveData.Instance.LoadCampaignMode(partyAdventureData2, isJoiningMPClient: true);
			break;
		}
		case EGameMode.SingleScenario:
		{
			FFSNet.Console.LogInfo("Continuing custom level: " + gameData.SaveName);
			FFSNetwork.Manager.OnConnectionEstablished?.Invoke(gameData);
			SaveData.Instance.LoadCustomLevelDataFromFile(fileInfo.FullName);
			PartyAdventureData partyAdventureData3 = SaveData.Instance.Global.AllSingleScenarios.SingleOrDefault((PartyAdventureData s) => s.PartyName == gameData.SaveName && ((!gameData.HostNetworkAccountID.IsNullOrEmpty() && !gameData.HostNetworkAccountID.Equals("0") && gameData.HostNetworkAccountID == s.Owner.PlatformNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == gameData.HostAccountID)));
			if (partyAdventureData3 == null)
			{
				bool isMaskingInProcess3 = true;
				SaveOwner saveOwner = new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostNetworkAccountID, gameData.HostUsername, gameData.HostPlatformName);
				saveOwner.MaskBadWordsInUsername(delegate
				{
					isMaskingInProcess3 = false;
				});
				while (isMaskingInProcess3)
				{
					yield return null;
				}
				partyAdventureData3 = new PartyAdventureData(mapState, gameData.SaveName, saveOwner, EGameMode.SingleScenario, gameData.CustomRulesetName);
				SaveData.Instance.Global.AllCampaigns.Add(partyAdventureData3);
			}
			else
			{
				partyAdventureData3.AdventureMapState = null;
				if (PlatformLayer.IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
			}
			partyAdventureData3.DisplayPartyName = (FFSNetwork.IsUGCEnabled ? partyAdventureData3.PartyName : LocalizationManager.GetTranslation("Consoles/DEFAULT_SAVE_NAME"));
			SceneController.Instance.LoadSingleScenario(partyAdventureData3);
			break;
		}
		default:
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00008", "Error launching multiplayer game. Unsuitable or unfinished game mode found: " + (EGameMode)gameData.GameModeID/*cast due to .constrained prefix*/);
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.InvalidGameMode);
			FFSNetwork.Shutdown();
			break;
		}
	}

	private IEnumerator TryLaunchMultiplayerSave(GameToken gameData, string path)
	{
		DLCRegistry.EDLCKey dLCFlag = (DLCRegistry.EDLCKey)gameData.DLCFlag;
		FFSNet.Console.LogInfo("Host enabled DLC flag on the save file received: " + dLCFlag);
		DLCRegistry.EDLCKey invalidDLCs = PlatformLayer.DLC.GetInvalidDLCs(dLCFlag);
		if (invalidDLCs != DLCRegistry.EDLCKey.None)
		{
			ConnectionErrorCode arg = ConnectionErrorCode.None;
			if (invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC1) && invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC2))
			{
				arg = ConnectionErrorCode.DLCDoNotMatchJotlAndSoloScenarious;
			}
			else if (invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC1))
			{
				arg = ConnectionErrorCode.DLCDoNotMatchJotl;
			}
			else if (invalidDLCs.HasFlag(DLCRegistry.EDLCKey.DLC2))
			{
				arg = ConnectionErrorCode.DLCDoNotMatchSoloScenarious;
			}
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(arg);
			FFSNetwork.Shutdown();
			PlatformLayer.FileSystem.RemoveFile(path);
			yield break;
		}
		bool gameClosedWhileSaving;
		Task<CMapState> task = new Task<CMapState>(() => PartyAdventureData.LoadMapStateFromFile(path, out gameClosedWhileSaving));
		task.Start();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		CMapState adventureMapState = task.Result;
		if (adventureMapState == null)
		{
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.NotEnoughMemory);
			FFSNetwork.Shutdown();
			yield break;
		}
		switch ((EGameMode)gameData.GameModeID)
		{
		case EGameMode.Guildmaster:
		{
			FFSNet.Console.LogInfo("Continuing adventure mode: " + gameData.SaveName);
			LogUtils.LogWarning("Continuing adventure mode: " + gameData.SaveName);
			FFSNetwork.Manager.OnConnectionEstablished?.Invoke(gameData);
			PartyAdventureData partyAdventureData = SaveData.Instance.Global.AllAdventures.SingleOrDefault((PartyAdventureData s) => s.PartyName == gameData.SaveName && ((!gameData.HostNetworkAccountID.IsNullOrEmpty() && !gameData.HostNetworkAccountID.Equals("0") && gameData.HostNetworkAccountID == s.Owner.PlatformNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == gameData.HostAccountID)));
			if (partyAdventureData == null)
			{
				bool isMaskingInProcess = true;
				SaveOwner saveOwner = new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostNetworkAccountID, gameData.HostUsername, gameData.HostPlatformName);
				saveOwner.MaskBadWordsInUsername(delegate
				{
					isMaskingInProcess = false;
				});
				while (isMaskingInProcess)
				{
					yield return null;
				}
				partyAdventureData = new PartyAdventureData(adventureMapState, gameData.SaveName, saveOwner, EGameMode.Guildmaster, (gameData.CustomRulesetName != string.Empty) ? (gameData.CustomRulesetName + gameData.HostAccountID) : string.Empty);
				SaveData.Instance.Global.AddAdventureSave(partyAdventureData);
			}
			else
			{
				partyAdventureData.AdventureMapState = null;
				if (PlatformLayer.IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
				if (partyAdventureData.IsModded)
				{
					partyAdventureData.RulesetName = gameData.CustomRulesetName + gameData.HostAccountID;
				}
			}
			partyAdventureData.DisplayPartyName = (FFSNetwork.IsUGCEnabled ? partyAdventureData.PartyName : LocalizationManager.GetTranslation("Consoles/DEFAULT_SAVE_NAME"));
			PlayerRegistry.LoadingInFromJoiningClient = true;
			SaveData.Instance.LoadGuildmasterMode(partyAdventureData, isJoiningMPClient: true);
			break;
		}
		case EGameMode.Campaign:
		{
			FFSNet.Console.LogInfo("Continuing campaign mode: " + gameData.SaveName);
			LogUtils.LogWarning("Continuing campaign mode: " + gameData.SaveName);
			FFSNetwork.Manager.OnConnectionEstablished?.Invoke(gameData);
			PartyAdventureData partyAdventureData2 = SaveData.Instance.Global.AllCampaigns.SingleOrDefault((PartyAdventureData s) => s.PartyName == gameData.SaveName && ((!gameData.HostNetworkAccountID.IsNullOrEmpty() && !gameData.HostNetworkAccountID.Equals("0") && gameData.HostNetworkAccountID == s.Owner.PlatformNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == gameData.HostAccountID)));
			if (partyAdventureData2 == null)
			{
				bool isMaskingInProcess2 = true;
				SaveOwner saveOwner = new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostNetworkAccountID, gameData.HostUsername, gameData.HostPlatformName);
				saveOwner.MaskBadWordsInUsername(delegate
				{
					isMaskingInProcess2 = false;
				});
				while (isMaskingInProcess2)
				{
					yield return null;
				}
				partyAdventureData2 = new PartyAdventureData(adventureMapState, gameData.SaveName, saveOwner, EGameMode.Campaign, gameData.CustomRulesetName);
				SaveData.Instance.Global.AllCampaigns.Add(partyAdventureData2);
			}
			else
			{
				partyAdventureData2.AdventureMapState = null;
				if (PlatformLayer.IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
				LogUtils.LogWarning("partyData.AdventureMapState: null");
			}
			partyAdventureData2.DisplayPartyName = (FFSNetwork.IsUGCEnabled ? partyAdventureData2.PartyName : LocalizationManager.GetTranslation("Consoles/DEFAULT_SAVE_NAME"));
			PlayerRegistry.LoadingInFromJoiningClient = true;
			SaveData.Instance.LoadCampaignMode(partyAdventureData2, isJoiningMPClient: true);
			break;
		}
		case EGameMode.SingleScenario:
		{
			FFSNet.Console.LogInfo("Continuing custom level: " + gameData.SaveName);
			FFSNetwork.Manager.OnConnectionEstablished?.Invoke(gameData);
			SaveData.Instance.LoadCustomLevelDataFromFile(path);
			PartyAdventureData partyAdventureData3 = SaveData.Instance.Global.AllSingleScenarios.SingleOrDefault((PartyAdventureData s) => s.PartyName == gameData.SaveName && ((!gameData.HostNetworkAccountID.IsNullOrEmpty() && !gameData.HostNetworkAccountID.Equals("0") && gameData.HostNetworkAccountID == s.Owner.PlatformNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == gameData.HostAccountID)));
			if (partyAdventureData3 == null)
			{
				bool isMaskingInProcess3 = true;
				SaveOwner saveOwner = new SaveOwner(gameData.HostPlayerID, gameData.HostAccountID, gameData.HostNetworkAccountID, gameData.HostUsername, gameData.HostPlatformName);
				saveOwner.MaskBadWordsInUsername(delegate
				{
					isMaskingInProcess3 = false;
				});
				while (isMaskingInProcess3)
				{
					yield return null;
				}
				partyAdventureData3 = new PartyAdventureData(adventureMapState, gameData.SaveName, saveOwner, EGameMode.SingleScenario, gameData.CustomRulesetName);
				SaveData.Instance.Global.AllCampaigns.Add(partyAdventureData3);
			}
			else
			{
				partyAdventureData3.AdventureMapState = null;
				if (PlatformLayer.IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
			}
			partyAdventureData3.DisplayPartyName = (FFSNetwork.IsUGCEnabled ? partyAdventureData3.PartyName : LocalizationManager.GetTranslation("Consoles/DEFAULT_SAVE_NAME"));
			SceneController.Instance.LoadSingleScenario(partyAdventureData3);
			break;
		}
		default:
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00008", "Error launching multiplayer game. Unsuitable or unfinished game mode found: " + (EGameMode)gameData.GameModeID/*cast due to .constrained prefix*/);
			FFSNetwork.Manager.OnConnectionFailed?.Invoke(ConnectionErrorCode.InvalidGameMode);
			FFSNetwork.Shutdown();
			break;
		}
	}

	private void IsUsersInSessionAvailableToPlayWith(HashSet<string> usersIds, Action<Tuple<OperationResult, PermissionOperationResult>> callback)
	{
		OperationResult operationResult = OperationResult.Success;
		PermissionOperationResult permissionOperationResult = PermissionOperationResult.Success;
		if (usersIds.Count == 0)
		{
			callback?.Invoke(new Tuple<OperationResult, PermissionOperationResult>(operationResult, permissionOperationResult));
		}
		else
		{
			PlatformLayer.Networking.GetPermissionsTowardsPlatformUsersAsync(usersIds, OnGetPermissionsTowardsPlatformUsers);
		}
		void OnGetPermissionsTowardsPlatformUsers(OperationResult operationResultResponse, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>> usersResponse)
		{
			LogUtils.Log("IsUsersInSessionAvailableToPlayWith... Callback");
			if (operationResult != OperationResult.Success)
			{
				operationResult = operationResultResponse;
				callback?.Invoke(new Tuple<OperationResult, PermissionOperationResult>(operationResult, permissionOperationResult));
			}
			else
			{
				foreach (KeyValuePair<string, Dictionary<Permission, List<PermissionOperationResult>>> item in usersResponse)
				{
					if (item.Value[Permission.PlayMultiplayer][0] != PermissionOperationResult.Success)
					{
						permissionOperationResult = item.Value[Permission.PlayMultiplayer][0];
						break;
					}
				}
				callback?.Invoke(new Tuple<OperationResult, PermissionOperationResult>(operationResult, permissionOperationResult));
			}
		}
	}

	public override void StreamDataStarted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		base.StreamDataStarted(connection, channel, streamID);
		Debug.Log("Starting Data Stream");
	}

	public override void StreamDataProgress(BoltConnection connection, UdpChannelName channel, ulong streamID, float progress)
	{
		base.StreamDataProgress(connection, channel, streamID, progress);
		Debug.Log("Stream Progress: " + progress);
	}

	public override void StreamDataAborted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		base.StreamDataAborted(connection, channel, streamID);
		FFSNet.Console.LogError("ERROR_MULTIPLAYER_00009", "Data stream aborted");
		FFSNetwork.Shutdown();
	}

	public override void StreamDataReceived(BoltConnection connection, UdpStreamData data)
	{
		base.StreamDataReceived(connection, data);
		Debug.Log("Stream completed");
		switch (m_CurrentStreamMode)
		{
		case EStreamMode.ReceivingRulesetZip:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.SingleOrDefault((GHRuleset s) => s.Name == SaveData.Instance.Global.CurrentModdedRuleset);
			if (gHRuleset != null)
			{
				if (PlatformLayer.FileSystem.ExistsFile(gHRuleset.RulesetCompiledZip))
				{
					PlatformLayer.FileSystem.ReadFile(gHRuleset.RulesetCompiledZip);
				}
				else if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(Path.GetDirectoryName(gHRuleset.RulesetCompiledZip)))
				{
					Debug.Log("Creating ruleset directory: " + Path.GetDirectoryName(gHRuleset.RulesetCompiledZip));
					PlatformLayer.FileSystem.CreateDirectory(Path.GetDirectoryName(gHRuleset.RulesetCompiledZip));
				}
				PlatformLayer.FileSystem.WriteFile(data.Data, gHRuleset.RulesetCompiledZip);
				string rulesetHash = gHRuleset.GetRulesetHash();
				if (rulesetHash != string.Empty)
				{
					gHRuleset.CompiledHash = rulesetHash;
				}
				else
				{
					FFSNetwork.HandleDesync(new Exception("Error getting hash for downloaded ruleset."));
				}
				if (rulesetHash != m_LastRulesetHash)
				{
					FFSNetwork.HandleDesync(new Exception("Downloaded hash of ruleset does not match file sent by host."));
				}
				StartCoroutine(LoadRulesetCoroutine(gHRuleset));
			}
			else
			{
				FFSNetwork.HandleDesync(new Exception("Error processing received chunk of compiled ruleset data."));
			}
			break;
		}
		case EStreamMode.ReceivingRulesetInstance:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			FFSNetwork.Manager.OnConnectionStateUpdated?.Invoke(ConnectionState.DownloadingRuleset);
			byte[] byteArray2 = FFSNet.Utility.DecompressData(data.Data);
			GHRuleset ruleset = (GHRuleset)FFSNet.Utility.ByteArrayToObject(byteArray2);
			if (ruleset != null)
			{
				ruleset.IsMPRuleset = true;
				string text2 = ruleset.Name;
				int num = 1;
				while (SceneController.Instance.Modding.Rulesets.Exists((GHRuleset e) => e.Name == ruleset.Name))
				{
					ruleset.Name = text2 + num;
				}
				if (ruleset.LinkedCSV != null)
				{
					for (int num2 = 0; num2 < ruleset.LinkedCSV.Count; num2++)
					{
						string directoryName = Path.GetDirectoryName(Path.GetDirectoryName(ruleset.LinkedCSV[num2]));
						ruleset.LinkedCSV[num2] = Path.Combine(RootSaveData.MultiplayerLocalizationFolder, Path.GetFileName(directoryName) + ".csv");
					}
				}
				ruleset.Save();
				SceneController.Instance.Modding.Rulesets.Add(ruleset);
				SaveData.Instance.Global.CurrentModdedRuleset = ruleset.Name;
				m_ModCSVs = ruleset.LinkedCSV.ToList();
				Debug.Log("Receiving RulesetZip CSV Count: " + m_ModCSVs.Count);
				if (m_ModCSVs != null && m_ModCSVs.Count() > 0)
				{
					Debug.Log("Requesting CSV");
					m_CurrentStreamMode = EStreamMode.ReceivingCSVFiles;
					Synchronizer.RequestCustomData(DataActionType.SendCSVFiles);
				}
				else
				{
					m_CurrentStreamMode = EStreamMode.ReceivingRulesetZip;
					Synchronizer.RequestCustomData(DataActionType.SendModdedRuleset);
				}
			}
			else
			{
				FFSNetwork.HandleDesync(new Exception("Error processing received chunk of ruleset data."));
			}
			break;
		}
		case EStreamMode.ReceivingCSVFiles:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			byte[] data2 = FFSNet.Utility.DecompressData(data.Data);
			SceneController.Instance.Modding.Rulesets.Single((GHRuleset s) => s.Name == SaveData.Instance.Global.CurrentModdedRuleset);
			string text = m_ModCSVs[0];
			m_ModCSVs.Remove(text);
			if (PlatformLayer.FileSystem.ExistsFile(text))
			{
				PlatformLayer.FileSystem.RemoveFile(text);
			}
			else if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(Path.GetDirectoryName(text)))
			{
				Debug.Log("Receiving CSV files. Creating ruleset directory: " + Path.GetDirectoryName(text));
				PlatformLayer.FileSystem.CreateDirectory(Path.GetDirectoryName(text));
			}
			PlatformLayer.FileSystem.WriteFile(data2, text);
			Debug.Log("Receiving CSV Path: " + text);
			Debug.Log("Receiving CSV Count: " + m_ModCSVs.Count);
			if (m_ModCSVs.Count > 0)
			{
				m_CurrentStreamMode = EStreamMode.ReceivingCSVFiles;
				Synchronizer.RequestCustomData(DataActionType.SendCSVFiles);
			}
			else
			{
				m_CurrentStreamMode = EStreamMode.ReceivingRulesetZip;
				Synchronizer.RequestCustomData(DataActionType.SendModdedRuleset);
			}
			break;
		}
		case EStreamMode.ReceivingSimpleLog:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			byte[] bytes2 = FFSNet.Utility.DecompressData(data.Data);
			File.WriteAllBytes(RootSaveData.SimpleLogPathFromHost, bytes2);
			break;
		}
		case EStreamMode.ReceivingPlayerLog:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			byte[] bytes = FFSNet.Utility.DecompressData(data.Data);
			File.WriteAllBytes(RootSaveData.PlayerLogPathFromHost, bytes);
			break;
		}
		case EStreamMode.ReceivingCompareState:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			byte[] byteArray = FFSNet.Utility.DecompressData(data.Data);
			bool flag = CompareScenarioStates((ScenarioState)FFSNet.Utility.ByteArrayToObject(byteArray));
			FFSNet.Console.LogInfo("ReceivingCompareState. SceneController.Instance.IsLoading: " + SceneController.Instance.IsLoading);
			Synchronizer.SendSideAction(GameActionType.EndOfRoundCompareClientFinished, null, canBeUnreliable: false, sendToHostOnly: true);
			if (DebugMenu.DebugMenuNotNull)
			{
				DebugMenu.Instance.DisplayDebugText(1, "State comparison: " + (flag ? "PASSED" : "FAILED"));
			}
			if (!flag)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MULTIPLAYER_00037", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
			}
			break;
		}
		case EStreamMode.ReceivingInitialGameState:
		{
			m_CurrentStreamMode = EStreamMode.NotSet;
			byte[] saveFileData = FFSNet.Utility.DecompressData(data.Data);
			WriteAndLaunchSaveFile(saveFileData);
			break;
		}
		default:
			m_CurrentStreamMode = EStreamMode.NotSet;
			FFSNet.Console.LogError("ERROR_MULTIPLAYER_00009", "Invalid data stream mode set");
			FFSNetwork.Shutdown();
			break;
		}
	}
}
