#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using GLOOM.MainMenu;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.State;
using OdinSerializer;
using Platforms.PlatformData;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Client;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.UI;

public class SaveData : MonoBehaviour
{
	private const string c_CLA_AutoTest = "-autotest";

	private const string c_CLA_LoadMostRecentSave = "-loadmostrecentsave";

	private const string c_CLA_Debug = "-debug";

	private const string c_CLA_AutoupdateAutotests = "-autoupdate";

	public static SaveData Instance;

	[NonSerialized]
	public GlobalData Global;

	[NonSerialized]
	public volatile bool IsSavingData;

	[NonSerialized]
	public volatile bool IsSavingThreadActive;

	public SaveQueue SaveQueue = new SaveQueue();

	private bool _isOpen = true;

	[NonSerialized]
	private PartyAdventureData m_TempPartyData;

	private bool m_TempIsMPClient;

	public RootSaveData RootData { get; private set; }

	public string PersistentDataPath { get; private set; }

	public bool LoadMostRecentSave { get; set; }

	public bool GameBootedForAutoTests { get; private set; }

	public string AutoTestResultsLog { get; private set; }

	public string AutoTestFolder { get; private set; }

	public bool IsModdingEnabled { get; private set; }

	public string CurrentFileBeingDeserialized { get; set; }

	public string AssemblyInfoSaveDir { get; private set; }

	public bool AutoupdateAutotests { get; private set; }

	public AutoTestDataManager AutoTestDataManager { get; private set; }

	public LevelEditorDataManager LevelEditorDataManager { get; private set; }

	public IEnumerator LoadRulebase()
	{
		Debug.Log("Loading Rulebase");
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			yield return SceneController.Instance.InitialiseGloomhavenCoroutine(Instance.PersistentDataPath, RootSaveData.CoreRulebasePath);
			yield return null;
		}
		yield return LoadGlobalData();
		List<string> restoredSaves = new List<string>();
		yield return Global.ValidateSaves(EGameMode.Guildmaster, restoredSaves);
		yield return Global.ValidateSaves(EGameMode.Campaign, restoredSaves);
		if (restoredSaves.Count != 0)
		{
			Debug.Log("Next saves have been restored from checkpoints:\n " + string.Join("\n", restoredSaves));
			SaveGlobalData();
		}
		Instance.InitialiseDataManagers();
		yield return null;
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		ParseCommandLineArgs();
		PersistentDataPath = PathsManager.PersistionDataPath;
		try
		{
			if (!PlatformLayer.Instance.IsDelayedInit)
			{
				LoadRootData();
				StartCoroutine(Singleton<InputManager>.Instance.InitKeyBindings());
			}
			else
			{
				Global = new GlobalData();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while loading rulebase at startup\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			return;
		}
		RootSaveData.InitReleaseVersion();
	}

	public async Task<bool> IsEnoughSpace()
	{
		if (!_isOpen)
		{
			Debug.Log("[SaveData] IsEnoughSpace() is closed... return false");
			return false;
		}
		_isOpen = false;
		while (SaveQueue.IsAnyOperationExecuting)
		{
			await Task.Delay(50);
		}
		PlatformLayer.FileSystem.ExistsFile(RootSaveData.RootSaveFile, out var operationResult, out var _);
		bool flag = operationResult != OperationResult.NotEnoughSpace;
		_isOpen = true;
		Debug.Log($"[SaveData] IsEnoughSpace() = {flag}");
		return flag;
	}

	private IEnumerator Start()
	{
		while (!PersistentData.s_Instance.IsDataLoaded || Global == null)
		{
			yield return null;
		}
		while (!AnalyticsWrapper.IsReady)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		AnalyticsWrapper.InitHeader();
		AnalyticsWrapper.LogAppBoot();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	private void OnApplicationQuit()
	{
		Debug.Log("BRYNN: SaveData OnApplicationQuit Start" + DateTime.Now);
		GlobalData global = Global;
		if (global != null && global.CurrentGameState == EGameState.Scenario)
		{
			Instance.EndScenario(EResult.InProgressAppKill);
		}
		SaveGlobalData();
		SimpleLog.WriteSimpleLogToFile();
		Debug.Log("BRYNN: SaveData OnApplicationQuit Start" + DateTime.Now);
	}

	public void StartNewCampaignMode(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, EGoldMode goldMode, DLCRegistry.EDLCKey dlcEnabled, EEnhancementMode enhancementMode)
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), partyName, difficulty, houseRulesSetting, new SaveOwner(), goldMode, dlcEnabled, enhancementMode, EGameMode.Campaign, Global.CurrentModdedRuleset);
		Global.AllCampaigns.Add(partyAdventureData);
		Global.ResumeCampaignName = partyName;
		SaveGlobalData();
		Global.CurrentHostAccountID = partyAdventureData.Owner.PlatformAccountID;
		Global.CurrentHostNetworkAccountID = partyAdventureData.Owner.PlatformNetworkAccountID;
		ResumeNewCampaignMode(isJoiningMPClient: false);
	}

	public void ResumeNewCampaignMode(bool isJoiningMPClient)
	{
		if (Global.CampaignData != null)
		{
			if (Global.CampaignData.AdventureMapState == null)
			{
				Global.CampaignData.Load(EGameMode.Campaign, isJoiningMPClient);
			}
			Global.GameMode = EGameMode.Campaign;
			ScenarioManager.Mode = ScenarioManager.EDLLMode.Campaign;
			return;
		}
		throw new Exception("No Adventure data was found for Adventure party " + Global.ResumeAdventureName);
	}

	public void LoadCampaignMode(PartyAdventureData partyData, bool isJoiningMPClient, bool loadMenuOnCancel = true, Action onUnloadPreviousScene = null, Action onCancelLoad = null, bool refreshMapState = false)
	{
		if (!PlatformLayer.DLC.CanPlayPartyData(partyData))
		{
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("GUI_CANCEL", SceneController.Instance.LoadMainMenu, KeyAction.UI_CANCEL)
			};
			SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("DLC_REQUIRED", "MISSING_DLC", Environment.StackTrace, buttons);
		}
		else if (!FFSNetwork.IsOnline && NeedToCreateSave(partyData.Owner.PlatformNetworkAccountID, partyData.Owner.PlatformAccountID, PlatformLayer.UserData.PlatformNetworkAccountPlayerID, PlatformLayer.UserData.PlatformAccountID))
		{
			m_TempPartyData = partyData;
			m_TempIsMPClient = isJoiningMPClient;
			if (Global.AllCampaigns?.SingleOrDefault((PartyAdventureData s) => s.PartyName == partyData.PartyName && ((!PlatformLayer.UserData.PlatformNetworkAccountPlayerID.IsNullOrEmpty() && !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.Equals("0") && PlatformLayer.UserData.PlatformNetworkAccountPlayerID == s.Owner.PlatformNetworkAccountID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID))) != null)
			{
				List<ErrorMessage.LabelAction> buttons2 = new List<ErrorMessage.LabelAction>
				{
					new ErrorMessage.LabelAction("OVERWRITE_SAVE_BUTTON", CreateNewSaveFromMPHostSaveCampaign, KeyAction.UI_SUBMIT),
					new ErrorMessage.LabelAction("GUI_CANCEL", delegate
					{
						OnCancelCreateLocalSave(loadMenuOnCancel, onCancelLoad);
					}, KeyAction.UI_CANCEL)
				};
				SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("Consoles/OVERWRITE_SAVE_TITLE", "Consoles/OVERWRITE_EXISTING_LOCAL_SAVE", Environment.StackTrace, buttons2);
			}
			else
			{
				List<ErrorMessage.LabelAction> buttons3 = new List<ErrorMessage.LabelAction>
				{
					new ErrorMessage.LabelAction("CREATE_NEW_LOCAL_SAVE_BUTTON", CreateNewSaveFromMPHostSaveCampaign, KeyAction.UI_SUBMIT),
					new ErrorMessage.LabelAction("GUI_CANCEL", delegate
					{
						OnCancelCreateLocalSave(loadMenuOnCancel, onCancelLoad);
					}, KeyAction.UI_CANCEL)
				};
				SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("Consoles/CREATE_NEW_SAVE_MESSAGE_TITLE", "Consoles/CREATE_NEW_LOCAL_SAVE", Environment.StackTrace, buttons3);
			}
		}
		else
		{
			if (partyData.IsModded)
			{
				Global.ResumeCampaignName = partyData.PartyName;
			}
			else
			{
				Global.ResumeCampaignName = partyData.PartyName;
				SaveGlobalData();
			}
			Global.CurrentHostAccountID = (FFSNetwork.IsClient ? partyData.Owner.PlatformAccountID : PlatformLayer.UserData.PlatformAccountID);
			Global.CurrentHostNetworkAccountID = (FFSNetwork.IsClient ? partyData.Owner.PlatformNetworkAccountID : PlatformLayer.UserData.PlatformNetworkAccountPlayerID);
			ResumeNewCampaignMode(isJoiningMPClient);
			SceneController.Instance.CampaignResume(partyData, isJoiningMPClient, isFromMPHost: false, onUnloadPreviousScene, refreshMapState);
		}
	}

	public void StartNewAdventureMode(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, EGoldMode goldMode, DLCRegistry.EDLCKey dlcEnabled)
	{
		PartyAdventureData partyAdventureData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), partyName, difficulty, houseRulesSetting, new SaveOwner(), goldMode, dlcEnabled, EEnhancementMode.CharacterPersistent, EGameMode.Guildmaster, Global.CurrentModdedRuleset);
		Global.AddAdventureSave(partyAdventureData);
		Global.ResumeAdventureName = partyName;
		SaveGlobalData();
		Global.CurrentHostAccountID = partyAdventureData.Owner.PlatformAccountID;
		Global.CurrentHostNetworkAccountID = partyAdventureData.Owner.PlatformNetworkAccountID;
		ResumeNewAdventureMode(isMPClient: false);
	}

	private bool NeedToCreateSave(string saveNetworkID, string saveAccountID, string userNetworkID, string userAccountID)
	{
		bool flag = ((Application.platform != RuntimePlatform.Switch) ? ((saveNetworkID.IsNullOrEmpty() || saveNetworkID.Equals("0") || saveNetworkID != userNetworkID) && saveAccountID != userAccountID) : (!saveNetworkID.IsNullOrEmpty() && !saveNetworkID.Equals("0") && saveNetworkID != userNetworkID));
		UnityEngine.Debug.Log($"CheckForCreatingNewSave result: {flag} saveNetworkID:{saveNetworkID} userNetworkID: {userNetworkID}");
		return flag;
	}

	public void LoadGuildmasterMode(PartyAdventureData partyData, bool isJoiningMPClient, bool loadMenuOnCancel = true, Action onUnloadPreviousScene = null, Action onCancelLoad = null, bool refreshMapState = false)
	{
		if (!PlatformLayer.DLC.CanPlayPartyData(partyData))
		{
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("GUI_CANCEL", SceneController.Instance.LoadMainMenu, KeyAction.UI_CANCEL)
			};
			SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("DLC_REQUIRED", "MISSING_DLC", Environment.StackTrace, buttons);
		}
		else if (!FFSNetwork.IsOnline && NeedToCreateSave(partyData.Owner.PlatformNetworkAccountID, partyData.Owner.PlatformAccountID, PlatformLayer.UserData.PlatformNetworkAccountPlayerID, PlatformLayer.UserData.PlatformAccountID))
		{
			m_TempPartyData = partyData;
			m_TempIsMPClient = isJoiningMPClient;
			if (Global.AllAdventures?.SingleOrDefault((PartyAdventureData s) => s.PartyName == partyData.PartyName && ((!s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID) || (Application.platform != RuntimePlatform.Switch && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID))) != null)
			{
				List<ErrorMessage.LabelAction> buttons2 = new List<ErrorMessage.LabelAction>
				{
					new ErrorMessage.LabelAction("OVERWRITE_SAVE_BUTTON", CreateNewSaveFromMPHostSaveGuildmaster, KeyAction.UI_SUBMIT),
					new ErrorMessage.LabelAction("GUI_CANCEL", delegate
					{
						OnCancelCreateLocalSave(loadMenuOnCancel, onCancelLoad);
					}, KeyAction.UI_CANCEL)
				};
				SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("Consoles/OVERWRITE_SAVE_TITLE", "Consoles/OVERWRITE_EXISTING_LOCAL_SAVE", Environment.StackTrace, buttons2);
			}
			else
			{
				List<ErrorMessage.LabelAction> buttons3 = new List<ErrorMessage.LabelAction>
				{
					new ErrorMessage.LabelAction("CREATE_NEW_LOCAL_SAVE_BUTTON", CreateNewSaveFromMPHostSaveGuildmaster, KeyAction.UI_SUBMIT),
					new ErrorMessage.LabelAction("GUI_CANCEL", delegate
					{
						OnCancelCreateLocalSave(loadMenuOnCancel, onCancelLoad);
					}, KeyAction.UI_CANCEL)
				};
				SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("Consoles/CREATE_NEW_SAVE_MESSAGE_TITLE", "Consoles/CREATE_NEW_LOCAL_SAVE", Environment.StackTrace, buttons3);
			}
		}
		else
		{
			Global.ResumeAdventureName = partyData.PartyName;
			SaveGlobalData();
			Global.CurrentHostAccountID = (FFSNetwork.IsClient ? partyData.Owner.PlatformAccountID : PlatformLayer.UserData.PlatformAccountID);
			Global.CurrentHostNetworkAccountID = (FFSNetwork.IsClient ? partyData.Owner.PlatformNetworkAccountID : PlatformLayer.UserData.PlatformNetworkAccountPlayerID);
			ResumeNewAdventureMode(isJoiningMPClient);
			SceneController.Instance.GuildmasterResume(partyData, isJoiningMPClient, regenerateMonsterCards: true, isFromMPHost: false, onUnloadPreviousScene, refreshMapState);
		}
	}

	private void OnCancelCreateLocalSave(bool loadMenuOnCancel, Action cancelCallback = null)
	{
		cancelCallback?.Invoke();
		if (loadMenuOnCancel)
		{
			SceneController.Instance.LoadMainMenu();
			return;
		}
		SceneController.Instance.GlobalErrorMessage.Hide();
		Singleton<UILoadGameWindow>.Instance.Window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
	}

	public void CreateNewSaveFromMPHostSaveCampaign()
	{
		CreateNewSaveFromMPHostSave(EGameMode.Campaign);
	}

	public void CreateNewSaveFromMPHostSaveGuildmaster()
	{
		CreateNewSaveFromMPHostSave(EGameMode.Guildmaster);
	}

	private void CreateNewSaveFromMPHostSave(EGameMode mode)
	{
		switch (mode)
		{
		case EGameMode.Campaign:
			Global.ResumeCampaignName = m_TempPartyData.PartyName;
			SaveGlobalData();
			break;
		case EGameMode.Guildmaster:
			Global.ResumeAdventureName = m_TempPartyData.PartyName;
			SaveGlobalData();
			break;
		}
		Global.CurrentHostAccountID = (m_TempIsMPClient ? m_TempPartyData.Owner.PlatformAccountID : PlatformLayer.UserData.PlatformAccountID);
		Global.CurrentHostNetworkAccountID = (m_TempIsMPClient ? m_TempPartyData.Owner.PlatformNetworkAccountID : PlatformLayer.UserData.PlatformNetworkAccountPlayerID);
		switch (mode)
		{
		case EGameMode.Campaign:
			if (Global.CampaignData != null)
			{
				Global.AllCampaigns.Remove(Global.CampaignData);
			}
			break;
		case EGameMode.Guildmaster:
			if (Global.AdventureData != null)
			{
				Global.RemoveAdventureSave(Global.AdventureData);
			}
			break;
		}
		PartyAdventureData partyAdventureData = m_TempPartyData.DeepCopySerializableObject<PartyAdventureData>();
		partyAdventureData.Owner = new SaveOwner();
		partyAdventureData.Load(mode, isJoiningMPClient: false);
		partyAdventureData.AdventureMapState.SetSaveOwner(partyAdventureData.Owner.ConvertToMapStateSaveOwner());
		switch (mode)
		{
		case EGameMode.Campaign:
			Global.AllCampaigns.Add(partyAdventureData);
			break;
		case EGameMode.Guildmaster:
			Global.AddAdventureSave(partyAdventureData);
			break;
		}
		partyAdventureData.UpdateSavePaths();
		SaveGlobalData();
		switch (mode)
		{
		case EGameMode.Campaign:
			ResumeNewCampaignMode(isJoiningMPClient: false);
			break;
		case EGameMode.Guildmaster:
			ResumeNewAdventureMode(isMPClient: false);
			break;
		}
		m_TempPartyData = null;
		m_TempIsMPClient = false;
		switch (mode)
		{
		case EGameMode.Campaign:
			SceneController.Instance.CampaignResume(partyAdventureData, isJoiningMPClient: false, isFromMPHost: true);
			break;
		case EGameMode.Guildmaster:
			SceneController.Instance.GuildmasterResume(partyAdventureData, isJoiningMPClient: false, regenerateMonsterCards: true, isFromMPHost: true);
			break;
		}
	}

	public void CopySaveToBackupBeforeEnablingDLCOnIt(PartyAdventureData partyData, DLCRegistry.EDLCKey dlcToAdd)
	{
		partyData.CopyMapStateFileToBackupFolder(partyData.GameMode);
		partyData.RefreshMapStateFromFile(partyData.AdventureMapStateFilePath);
		partyData.AdventureMapState?.SetDLCEnabled(dlcToAdd);
		partyData.Save();
	}

	public void ResumeNewAdventureMode(bool isMPClient)
	{
		if (Global.AdventureData != null)
		{
			if (Global.AdventureData.AdventureMapState == null)
			{
				Global.AdventureData.Load(EGameMode.Guildmaster, isMPClient);
			}
			Global.GameMode = EGameMode.Guildmaster;
			ScenarioManager.Mode = ScenarioManager.EDLLMode.Guildmaster;
			return;
		}
		throw new Exception("No Adventure data was found for Adventure party " + Global.ResumeAdventureName);
	}

	public void StartLevelEditor()
	{
		if (Global.CurrentEditorLevelData == null)
		{
			Global.CurrentEditorLevelData = new CCustomLevelData(string.Empty, new ScenarioState());
		}
		Instance.Global.GameMode = EGameMode.LevelEditor;
		SaveGlobalData();
	}

	public void StartCustomLevel()
	{
		ScenarioManager.Mode = ScenarioManager.EDLLMode.CustomScenario;
		Global.GameMode = EGameMode.SingleScenario;
		if (Global.SingleScenarioData?.AdventureMapState == null)
		{
			Global.SingleScenarioData.Load(EGameMode.SingleScenario, isJoiningMPClient: false);
		}
	}

	public void StartFrontEndTutorialLevel()
	{
		ScenarioManager.Mode = ScenarioManager.EDLLMode.CustomScenario;
		Global.GameMode = EGameMode.FrontEndTutorial;
		if (Global.SingleScenarioData != null && Global.SingleScenarioData.AdventureMapState == null)
		{
			Global.SingleScenarioData.Load(EGameMode.FrontEndTutorial, isJoiningMPClient: false);
		}
	}

	public void StartAutoTest(AutoTestData autoTest)
	{
		AutoTestController.s_Instance.AutotestStarted = true;
		Global.CurrentlyPlayingCustomLevel = false;
		Global.CurrentCustomLevelData = autoTest;
		Global.GameMode = EGameMode.Autotest;
		ScenarioManager.Mode = ScenarioManager.EDLLMode.CustomScenario;
	}

	public IEnumerator EndScenarioThreaded(EResult result)
	{
		IsSavingThreadActive = true;
		Thread endScenarioThread = new Thread((ThreadStart)delegate
		{
			EndScenarioSaveThread(result);
		});
		endScenarioThread.Start();
		while (endScenarioThread.IsAlive)
		{
			yield return null;
		}
	}

	public void EndScenarioSaveThread(EResult result)
	{
		if (AutoTestController.s_AutoTestCurrentlyLoaded || (Global.CurrentAdventureData == null && !Global.CurrentlyPlayingCustomLevel))
		{
			return;
		}
		switch (Global.GameMode)
		{
		case EGameMode.Guildmaster:
			if (result == EResult.Win)
			{
				Global.AdventureData.Save();
				Instance.Global.AdventureData.EnqueueSaveCheckpoint(delegate
				{
					IsSavingThreadActive = false;
				});
			}
			else
			{
				IsSavingThreadActive = false;
			}
			break;
		case EGameMode.Campaign:
			if (result == EResult.Win)
			{
				Global.CampaignData.Save();
				Instance.Global.CampaignData.EnqueueSaveCheckpoint(delegate
				{
					IsSavingThreadActive = false;
				});
			}
			else
			{
				IsSavingThreadActive = false;
			}
			break;
		default:
			IsSavingThreadActive = false;
			break;
		}
	}

	public void EndScenario(EResult result)
	{
		if (AutoTestController.s_AutoTestCurrentlyLoaded || (Global.CurrentAdventureData == null && !Global.CurrentlyPlayingCustomLevel))
		{
			return;
		}
		_ = Global.GameMode;
		bool currentlyPlayingCustomLevel = Global.CurrentlyPlayingCustomLevel;
		switch (Global.GameMode)
		{
		case EGameMode.Guildmaster:
		{
			CMapScenarioState currentMapScenarioState = Global.AdventureData.AdventureMapState.CurrentMapScenarioState;
			currentMapScenarioState.CurrentState.GetCharacterStatuses();
			currentMapScenarioState.CurrentState.AreAllCharactersExhausted();
			if (currentlyPlayingCustomLevel)
			{
				Global.CloseCustomLevel();
			}
			CQuestState inProgressQuestState = Global.AdventureData.AdventureMapState.InProgressQuestState;
			if (result != EResult.InProgress && result != EResult.InProgressAppKill)
			{
				Global.AdventureData.EndCurrentScenario(result);
			}
			switch (result)
			{
			case EResult.Win:
				Global.AdventureData.WinScenario(inProgressQuestState);
				break;
			case EResult.Lose:
			case EResult.Resign:
				if (AdventureState.MapState.IsPlayingTutorial)
				{
					AdventureState.MapState.CurrentMapScenarioState.RegenerateMapScenario(1);
				}
				else
				{
					Global.AdventureData.LoseScenario(!currentMapScenarioState.IsIntroScenario);
				}
				break;
			case EResult.InProgress:
			case EResult.InProgressAppKill:
				Global.AdventureData.RefreshMapStateFromFile(Global.AdventureData.PartyMainSaveFile);
				Global.GameMode = EGameMode.MainMenu;
				break;
			}
			break;
		}
		case EGameMode.Campaign:
		{
			Global.CampaignData.AdventureMapState.CurrentMapScenarioState.CurrentState.GetCharacterStatuses();
			Global.CampaignData.AdventureMapState.CurrentMapScenarioState.CurrentState.AreAllCharactersExhausted();
			if (currentlyPlayingCustomLevel)
			{
				Global.CloseCustomLevel();
			}
			CQuestState inProgressQuestState2 = Global.CampaignData.AdventureMapState.InProgressQuestState;
			if (result != EResult.InProgress && result != EResult.InProgressAppKill)
			{
				Global.CampaignData.EndCurrentScenario(result);
			}
			switch (result)
			{
			case EResult.Win:
				Global.CampaignData.WinScenario(inProgressQuestState2);
				break;
			case EResult.Lose:
			case EResult.Resign:
				Global.CampaignData.LoseScenario();
				break;
			case EResult.InProgress:
			case EResult.InProgressAppKill:
				SaveQueue.EnqueueOperation(delegate(Action action)
				{
					Global.CampaignData.RefreshMapStateFromFile(Global.CampaignData.PartyMainSaveFile, action);
				});
				Global.GameMode = EGameMode.MainMenu;
				break;
			}
			break;
		}
		case EGameMode.SingleScenario:
			Global.CloseCustomLevel();
			Global.GameMode = EGameMode.MainMenu;
			break;
		case EGameMode.FrontEndTutorial:
			Global.CloseCustomLevel();
			if (result == EResult.Win && !Global.CompletedTutorialIDs.Contains(Global.CurrentFrontEndTutorialID))
			{
				Global.CompletedTutorialIDs.Add(Global.CurrentFrontEndTutorialID);
			}
			if (result != EResult.Lose && result != EResult.Resign)
			{
				Global.CurrentFrontEndTutorialID = null;
				Global.CurrentFrontEndTutorialFilename = null;
			}
			SaveGlobalData();
			break;
		case EGameMode.Autotest:
			Global.CloseCustomLevel();
			Global.GameMode = EGameMode.MainMenu;
			break;
		default:
			Debug.LogError("Invalid GameMode.  Unable to end run.");
			Global.GameMode = EGameMode.MainMenu;
			break;
		}
		SEventLog.ClearEventLog();
	}

	public void SkipTutorial()
	{
		if (Global.GameMode == EGameMode.Guildmaster)
		{
			AdventureState.MapState.SkipFTUE(skipTutorial: true, skipIntro: true);
			AdventureState.MapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InHQ));
			Global.AdventureData.SkipIntroActivities();
			Global.AdventureData.Save();
			Global.AdventureData.EnqueueSaveCheckpoint();
		}
		else
		{
			Debug.LogError("Invalid GameMode to skip tutorial");
			Global.AdventureData.RefreshMapStateFromFile(Global.AdventureData.PartyMainSaveFile);
			Global.GameMode = EGameMode.MainMenu;
		}
	}

	public void LoadRootData()
	{
		Debug.Log("Loading RootSaveData");
		if (PlatformLayer.FileSystem.ExistsFile(RootSaveData.RootSaveFile))
		{
			try
			{
				using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(RootSaveData.RootSaveFile));
				Instance.CurrentFileBeingDeserialized = RootSaveData.RootSaveFile;
				BinaryFormatter binaryFormatter = new BinaryFormatter
				{
					Binder = new SerializationBinding()
				};
				RootData = binaryFormatter.Deserialize(serializationStream) as RootSaveData;
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to load root data.  It will be recreated.\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		if (RootData == null)
		{
			RootData = RootSaveData.CreateRootSaveData();
		}
		if (!SceneController.Instance.DataRestoring)
		{
			SaveRootData();
			if (PlatformLayer.FileSystem.ExistsFile(RootSaveData.SimpleLogPrevPath))
			{
				PlatformLayer.FileSystem.RemoveFile(RootSaveData.SimpleLogPrevPath);
			}
			if (PlatformLayer.FileSystem.ExistsFile(RootSaveData.SimpleLogPath))
			{
				PlatformLayer.FileSystem.MoveFile(RootSaveData.SimpleLogPath, RootSaveData.SimpleLogPrevPath);
			}
			if (PlatformLayer.FileSystem.ExistsFile(RootSaveData.SimpleLogPathFromHost))
			{
				PlatformLayer.FileSystem.RemoveFile(RootSaveData.SimpleLogPathFromHost);
			}
		}
	}

	public void SaveRootData()
	{
		Debug.Log("Saving RootSaveData");
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		try
		{
			Debug.Log("SEE!!! " + RootSaveData.RootSaveFile);
			string directoryName = Path.GetDirectoryName(RootSaveData.RootSaveFile);
			Debug.Log("SEE!!! " + directoryName);
			if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(directoryName))
			{
				PlatformLayer.FileSystem.CreateDirectory(directoryName);
			}
			using (MemoryStream memoryStream = new MemoryStream())
			{
				new BinaryFormatter().Serialize(memoryStream, RootData);
				PlatformLayer.FileSystem.WriteFile(memoryStream.ToArray(), RootSaveData.RootSaveFile);
			}
			Debug.Log("RootSaveData saved");
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to save.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
	}

	public void CreatePrimaryRootData()
	{
		RootData = RootSaveData.CreateRootSaveData();
	}

	public ModMetadata LoadModMetadata(string modPath)
	{
		string text = Path.Combine(modPath, "meta.dat");
		ModMetadata modMetadata;
		try
		{
			using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(text));
			Instance.CurrentFileBeingDeserialized = text;
			modMetadata = new BinaryFormatter
			{
				Binder = new SerializationBinding()
			}.Deserialize(serializationStream) as ModMetadata;
		}
		catch (Exception ex)
		{
			Debug.Log("[SaveData]: Unable to load Mod metadata. Creating new one.\n Exception:" + ex.Message + " \n " + ex.StackTrace);
			modMetadata = new ModMetadata();
		}
		string path = Path.Combine(modPath, "preview.png");
		try
		{
			if (PlatformLayer.FileSystem.ExistsFile(path))
			{
				byte[] data = PlatformLayer.FileSystem.ReadFile(path);
				modMetadata.Thumbnail = new Texture2D(2, 2);
				modMetadata.Thumbnail.LoadImage(data);
			}
		}
		catch
		{
			Debug.Log("[SaveData]: Unable to load Mod thumbnail.");
			return modMetadata;
		}
		return modMetadata;
	}

	public void SaveModMetadata(string metaFilePath, ModMetadata metaData)
	{
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		try
		{
			using MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, metaData);
			PlatformLayer.FileSystem.WriteFile(memoryStream.ToArray(), metaFilePath);
		}
		catch (Exception ex)
		{
			Debug.LogError("[SaveData]: An exception occurred attempting to save.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
	}

	public void UpdateModMetadata(string metaFilePath)
	{
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		ModMetadata modMetadata;
		try
		{
			using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(metaFilePath));
			Instance.CurrentFileBeingDeserialized = metaFilePath;
			modMetadata = new BinaryFormatter
			{
				Binder = new SerializationBinding()
			}.Deserialize(serializationStream) as ModMetadata;
		}
		catch (Exception ex)
		{
			Debug.Log("[SaveData]: Unable to load Mod metadata. Creating new one.\n" + ex.Message + "\n" + ex.StackTrace);
			modMetadata = new ModMetadata();
		}
		modMetadata.Version++;
		modMetadata.BuildVersion = Application.version;
		Debug.Log($"[SaveData]: Update Mod metadata {{ version: {modMetadata.Version}, buildVersion: {modMetadata.BuildVersion} }}");
		try
		{
			using MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, modMetadata);
			PlatformLayer.FileSystem.WriteFile(memoryStream.ToArray(), metaFilePath);
		}
		catch (Exception ex2)
		{
			Debug.LogError("[SaveData]: An exception occurred attempting to save.\n" + ex2.Message + "\n" + ex2.StackTrace);
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
	}

	public List<string> FindPartyDataFileNames(EGameMode gameMode)
	{
		try
		{
			string text = "";
			switch (gameMode)
			{
			case EGameMode.Campaign:
				text = RootData.CampaignSavePath;
				break;
			case EGameMode.Guildmaster:
				text = RootData.GuildmasterSavePath;
				break;
			default:
				Debug.LogError("Invalid Game mode sent to FindPartyDataFileNames");
				return new List<string>();
			}
			if (PlatformLayer.FileSystem.ExistsDirectory(text))
			{
				string[] directories = PlatformLayer.FileSystem.GetDirectories(text);
				if (directories.Length != 0)
				{
					List<string> list = new List<string>();
					string[] array = directories;
					foreach (string text2 in array)
					{
						string[] array2 = new string[1] { gameMode.ToString() + "_" };
						if (text2.Contains(array2[0]))
						{
							string[] array3 = text2.Split(array2, 2, StringSplitOptions.RemoveEmptyEntries);
							if (array3.Length < 2)
							{
								Debug.LogWarning("Unexpected party entry");
							}
							else if (!list.Contains(array3[1]))
							{
								list.Add(array3[1]);
							}
							else
							{
								Debug.LogError("Two party files with the same party name exist");
							}
						}
					}
					return list;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred in FindPartyDataFileNames.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return new List<string>();
	}

	public bool ExistsPartyData(EGameMode gameMode, string partyName)
	{
		switch (gameMode)
		{
		case EGameMode.Campaign:
			return Global.AllCampaigns.Any((PartyAdventureData a) => a.PartyName.ToLower() == partyName.ToLower());
		case EGameMode.Guildmaster:
			return Global.AllAdventures.Any((PartyAdventureData a) => a.PartyName.ToLower() == partyName.ToLower());
		default:
			Debug.LogError("Invalid game mode sent to ExistsPartyData");
			return false;
		}
	}

	public void DeletePartyData(PartyAdventureData partyData, EGameMode mode)
	{
		try
		{
			switch (mode)
			{
			case EGameMode.Campaign:
				Global.AllCampaigns.Remove(partyData);
				if (Global.ResumeCampaignName == partyData.PartyName)
				{
					Global.ResumeCampaignName = string.Empty;
					SaveGlobalData();
				}
				break;
			case EGameMode.Guildmaster:
				Global.RemoveAdventureSave(partyData);
				if (Global.ResumeAdventureName == partyData.PartyName)
				{
					Global.ResumeAdventureName = string.Empty;
					SaveGlobalData();
				}
				break;
			default:
				Debug.LogError("Invalid mode set for DeletePartyData");
				return;
			}
			if (PlatformLayer.FileSystem.ExistsDirectory(partyData.PartySaveDir))
			{
				PlatformLayer.FileSystem.RemoveDirectory(partyData.PartySaveDir, recursive: true);
			}
			SaveGlobalData();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to delete party data.\n" + ex.Message + "\n" + ex.StackTrace);
			List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
			list.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, KeyAction.UI_SUBMIT));
			SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00010", ex.StackTrace, list, ex.Message);
		}
	}

	public IEnumerator LoadGlobalData()
	{
		Debug.Log("Loading Global Data");
		bool messageDisplayed = false;
		bool globalDataReset = false;
		if (PlatformLayer.FileSystem.ExistsFile(RootData.GlobalSaveFile))
		{
			byte[] fileData = PlatformLayer.FileSystem.ReadFile(RootData.GlobalSaveFile);
			bool dataCorrupted = false;
			try
			{
				CheckDataForCorrupt(PlatformLayer.FileSystem.ReadFile(RootData.GlobalSaveFile));
			}
			catch
			{
				dataCorrupted = true;
			}
			if (dataCorrupted)
			{
				Debug.Log("Global data was corrupted - resetting global data");
				yield return ResetGlobalData();
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("Consoles/ERROR_CORRUPTED_GLOBAL_DATA", "GUI_OK", Environment.StackTrace, delegate
				{
				}, null, showErrorReportButton: false);
				yield return new WaitWhile(() => SceneController.Instance.GlobalErrorMessage.ShowingMessage);
			}
			bool shouldLoad = true;
			using (MemoryStream fs = new MemoryStream(dataCorrupted ? PlatformLayer.FileSystem.ReadFile(RootData.GlobalSaveFile) : fileData))
			{
				Instance.CurrentFileBeingDeserialized = RootData.GlobalSaveFile;
				BinaryFormatter binaryFormatter = new BinaryFormatter
				{
					Binder = new SerializationBinding()
				};
				bool flag = false;
				try
				{
					Global = binaryFormatter.Deserialize(fs) as GlobalData;
				}
				catch (SerializationException ex)
				{
					if (ex.Message.Contains("The ObjectManager found an invalid number of fixups"))
					{
						flag = true;
					}
					else
					{
						Global = SerializationUtility.DeserializeValue<GlobalData>(fileData, DataFormat.Binary);
					}
				}
				if (flag || !Global.IsCorrectVersion())
				{
					Debug.Log("Reset Global Data version has been incremented - resetting global data");
					shouldLoad = false;
					if (Instance.GameBootedForAutoTests)
					{
						yield return ResetGlobalData();
					}
					else
					{
						List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
						list.Add(new ErrorMessage.LabelAction("GUI_OK", delegate
						{
							StartCoroutine(ResetGlobalData());
						}, KeyAction.UI_SUBMIT));
						list.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, KeyAction.UI_CANCEL));
						SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessage("ERROR_SAVEDATA_00006", list, "ERROR_SAVEDATA_00018", Environment.StackTrace, null, showErrorReportButton: false);
						messageDisplayed = true;
					}
				}
				if (!messageDisplayed && !PlatformLayer.Instance.IsPlatformPartnerIDCorrect(Global.partnerID) && Application.platform != RuntimePlatform.LinuxPlayer && Application.platform != RuntimePlatform.Stadia)
				{
					Debug.Log("Steam ID saved in global data is different to logged in Steam ID - resetting global data");
					shouldLoad = false;
					if (Instance.GameBootedForAutoTests)
					{
						yield return ResetGlobalData();
					}
					else
					{
						List<ErrorMessage.LabelAction> list2 = new List<ErrorMessage.LabelAction>();
						list2.Add(new ErrorMessage.LabelAction("GUI_OK", ResetGlobalDataInternal, KeyAction.UI_SUBMIT));
						list2.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, KeyAction.UI_CANCEL));
						SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessage("ERROR_SAVEDATA_00006", list2, "ERROR_SAVEDATA_00018", Environment.StackTrace, null, showErrorReportButton: false);
						messageDisplayed = true;
					}
				}
			}
			if (!messageDisplayed && shouldLoad)
			{
				Global.Load();
				Global.GameMode = EGameMode.MainMenu;
			}
		}
		else if (PlatformLayer.FileSystem.ExistsFile(RootData.PreviousGlobalSaveFile))
		{
			using (MemoryStream fs = new MemoryStream(PlatformLayer.FileSystem.ReadFile(RootData.PreviousGlobalSaveFile)))
			{
				Instance.CurrentFileBeingDeserialized = RootData.PreviousGlobalSaveFile;
				BinaryFormatter binaryFormatter2 = new BinaryFormatter
				{
					Binder = new SerializationBinding()
				};
				Global = binaryFormatter2.Deserialize(fs) as GlobalData;
				if (!PlatformLayer.Instance.IsPlatformPartnerIDCorrect(Global.partnerID) && Application.platform != RuntimePlatform.LinuxPlayer)
				{
					Debug.Log("Steam ID saved in global data is different to logged in Steam ID - resetting global data");
					if (Instance.GameBootedForAutoTests)
					{
						yield return ResetGlobalData();
					}
					else
					{
						List<ErrorMessage.LabelAction> list3 = new List<ErrorMessage.LabelAction>();
						list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SAVEFILE_BUTTON", ResetGlobalDataInternal, KeyAction.UI_SUBMIT));
						list3.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, KeyAction.UI_CANCEL));
						SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00008", Environment.StackTrace, list3);
						messageDisplayed = true;
					}
				}
				Global.Load();
				Global.GameMode = EGameMode.MainMenu;
			}
			PlatformLayer.FileSystem.RemoveFile(RootData.PreviousGlobalSaveFile);
			SaveGlobalData();
		}
		else
		{
			yield return ResetGlobalData();
			globalDataReset = true;
		}
		if (!messageDisplayed && !globalDataReset)
		{
			SaveGlobalData();
		}
	}

	private void ResetGlobalDataInternal()
	{
		StartCoroutine(LoadGlobalData());
	}

	public IEnumerator ResetGlobalData()
	{
		Debug.Log("Reset Global Data");
		Global = new GlobalData();
		Global.Load();
		Global.ValidateAllAdventures();
		yield return Global.ValidateSaves(EGameMode.Guildmaster);
		yield return Global.ValidateSaves(EGameMode.Campaign);
		Global.GameMode = EGameMode.MainMenu;
		SaveGlobalData();
	}

	public void SaveGlobalData()
	{
		if (Global != null)
		{
			LogUtils.Log("Enqueue SaveGlobalData");
			Instance.SaveQueue.EnqueueOperation(SaveGlobalDataExecutor);
		}
	}

	public void PerformCustomFileOperation(Action<Action> saveAction)
	{
		LogUtils.Log("Enqueue PerformCustomFileOperation");
		Instance.SaveQueue.EnqueueOperation(saveAction);
	}

	private void SaveGlobalDataExecutor(Action onCompleted)
	{
		LogUtils.Log("Execute SaveGlobalData");
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		try
		{
			Global.Save();
			bool flag = false;
			int num = 0;
			while (!flag && num < 3)
			{
				string directoryName = Path.GetDirectoryName(RootData.GlobalSaveFile);
				if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(directoryName))
				{
					Debug.Log("Creating GlobalSaveFile directory: " + directoryName);
					PlatformLayer.FileSystem.CreateDirectory(directoryName);
				}
				try
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						new BinaryFormatter().Serialize(memoryStream, Global);
						byte[] data = memoryStream.ToArray();
						CheckDataForCorrupt(data);
						PlatformLayer.FileSystem.WriteFile(data, RootData.GlobalSaveFile);
						CheckFileForCorrupt(RootData.GlobalSaveFile);
					}
					flag = true;
				}
				catch
				{
					num++;
				}
			}
			if (!flag)
			{
				string tempFileName = Path.GetTempFileName();
				try
				{
					using (MemoryStream memoryStream2 = new MemoryStream())
					{
						new BinaryFormatter().Serialize(memoryStream2, Global);
						byte[] data2 = memoryStream2.ToArray();
						CheckDataForCorrupt(data2);
						PlatformLayer.FileSystem.WriteFile(data2, tempFileName);
						CheckFileForCorrupt(tempFileName);
					}
					flag = true;
				}
				catch
				{
				}
				if (flag)
				{
					if (PlatformLayer.FileSystem.ExistsFile(RootData.GlobalSaveFile))
					{
						PlatformLayer.FileSystem.RemoveFile(RootData.GlobalSaveFile);
					}
					PlatformLayer.FileSystem.MoveFile(tempFileName, RootData.GlobalSaveFile);
				}
				else
				{
					try
					{
						byte[] data3 = SerializationUtility.SerializeValue(Global, DataFormat.Binary);
						CheckDataForCorrupt(data3);
						PlatformLayer.FileSystem.WriteFile(data3, RootData.GlobalSaveFile);
						CheckFileForCorrupt(RootData.GlobalSaveFile);
						flag = true;
					}
					catch
					{
					}
				}
			}
			if (!flag)
			{
				throw new Exception("Unable to save GlobalData data to " + RootData.GlobalSaveFile + ".  Something prevented saving to this location");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to save global data.\n" + ex.Message + "\n" + ex.StackTrace);
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("GUI_ERROR_RETRY", SaveGlobalData, KeyAction.UI_SUBMIT),
				new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, KeyAction.UI_CANCEL)
			};
			SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00017", ex.StackTrace, buttons, ex.Message);
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
		onCompleted();
	}

	public void SaveCurrentAdventureData(Action onSaveDone = null, byte[] data = null)
	{
		try
		{
			if (AutoTestController.s_AutoTestCurrentlyLoaded || Global.GameMode == EGameMode.MainMenu)
			{
				return;
			}
			SimpleLog.AddToSimpleLog("Saving current adventure data\n" + Environment.StackTrace);
			if (Global.CurrentAdventureData != null)
			{
				Global.CurrentAdventureData.Save(delegate
				{
					onSaveDone?.Invoke();
				}, data);
			}
			else
			{
				Debug.LogError("Current party adventure data was null when trying to save");
			}
		}
		catch (Exception ex)
		{
			onSaveDone?.Invoke();
			Debug.LogError("An exception occurred attempting to save current party adventure data.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void InitialiseDataManagers()
	{
		AutoTestDataManager = new AutoTestDataManager();
		LevelEditorDataManager = new LevelEditorDataManager();
		if (PlatformLayer.Modding.ModdingSupported)
		{
			PlatformLayer.Modding.RefreshLevels();
		}
		if (Instance.Global.EpicLogin)
		{
			PlatformLayer.Instance.EOSInitialise();
		}
	}

	public void StartNewCustomLevel(CCustomLevelData levelData, PartyAdventureData customParty)
	{
		Global.CurrentEditorAutoTestData = null;
		Global.CurrentAutoTestDataCopy = null;
		Global.CurrentCustomLevelData = levelData;
		Global.CurrentlyPlayingCustomLevel = true;
		Global.ResumeSingleScenarioName = customParty.PartyName;
		Global.CurrentHostAccountID = customParty.Owner.PlatformAccountID;
		Global.CurrentHostNetworkAccountID = customParty.Owner.PlatformNetworkAccountID;
		PartyAdventureData partyAdventureData = Global.AllSingleScenarios.SingleOrDefault((PartyAdventureData e) => e.PartyName == customParty.PartyName && ((!e.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !e.Owner.PlatformNetworkAccountID.Equals("0") && e.Owner.PlatformNetworkAccountID == customParty.Owner.PlatformNetworkAccountID) || ((Application.platform != RuntimePlatform.Switch || e.Owner.PlatformNetworkAccountID.Equals("0")) && e.Owner.PlatformAccountID == customParty.Owner.PlatformAccountID)));
		if (partyAdventureData != null)
		{
			Global.AllSingleScenarios.Remove(partyAdventureData);
		}
		Global.AllSingleScenarios.Add(customParty);
		Global.CurrentHostAccountID = customParty.Owner.PlatformAccountID;
		Global.CurrentHostNetworkAccountID = customParty.Owner.PlatformNetworkAccountID;
		Instance.SaveGlobalData();
		levelData.ScenarioState.OverridePlayersInState(customParty.AdventureMapState.MapParty.ExportPlayerStates(levelData.ScenarioState));
		customParty.AdventureMapState.CurrentSingleScenario = levelData;
		customParty.Save();
	}

	public void LoadCustomLevelFromData(CCustomLevelData levelData, LevelEditorController.ELevelEditorState forLevelEditorState = LevelEditorController.ELevelEditorState.Idle)
	{
		try
		{
			if (levelData.ScenarioState.ScenarioType != EScenarioType.Custom)
			{
				levelData.ScenarioState.ScenarioType = EScenarioType.Custom;
				Debug.LogError("Custom level data was set to YML not Custom! Please re-save this file");
			}
			Global.CurrentEditorAutoTestData = null;
			Global.CurrentAutoTestDataCopy = null;
			Global.CurrentCustomLevelData = levelData;
			switch (forLevelEditorState)
			{
			case LevelEditorController.ELevelEditorState.Idle:
				Global.CurrentEditorLevelData = null;
				Global.CurrentlyPlayingCustomLevel = true;
				break;
			case LevelEditorController.ELevelEditorState.Editing:
				Global.CurrentEditorLevelData = levelData;
				Global.CurrentlyPlayingCustomLevel = false;
				break;
			case LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty:
			case LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel:
				Global.CurrentEditorLevelData = levelData.DeepCopySerializableObject<CCustomLevelData>();
				Global.CurrentlyPlayingCustomLevel = true;
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load custom scenario.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public void LoadCustomLevelDataFromFile(string filePath, LevelEditorController.ELevelEditorState forLevelEditorState = LevelEditorController.ELevelEditorState.Idle)
	{
		if (!PlatformLayer.FileSystem.ExistsFile(filePath))
		{
			return;
		}
		try
		{
			using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(filePath));
			Instance.CurrentFileBeingDeserialized = filePath;
			CCustomLevelData levelData = new BinaryFormatter
			{
				Binder = new SerializationBinding()
			}.Deserialize(serializationStream) as CCustomLevelData;
			LoadCustomLevelFromData(levelData, forLevelEditorState);
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load custom scenario.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public void LoadAutoTestFromData(AutoTestData autoTest, FileInfo autoTestFile, bool loadIntoEditor = false)
	{
		try
		{
			if (autoTest.ScenarioState.ScenarioType != EScenarioType.Custom)
			{
				autoTest.ScenarioState.ScenarioType = EScenarioType.Custom;
				Debug.LogError("Custom level data was set to YML not Custom! Please re-save this file");
			}
			if (autoTest != null)
			{
				if (autoTest.ScenarioState != null)
				{
					foreach (EnemyState allEnemyState in autoTest.ScenarioState.AllEnemyStates)
					{
						if (allEnemyState.ConfigPerPartySize != null && allEnemyState.ConfigPerPartySize.Count > 0 && allEnemyState.ConfigPerPartySize.ContainsKey(1) && allEnemyState.ConfigPerPartySize.ContainsKey(2))
						{
							allEnemyState.ConfigPerPartySize[1] = allEnemyState.ConfigPerPartySize[2];
						}
					}
				}
				if (autoTest.ExpectedResultingScenarioState != null)
				{
					foreach (EnemyState allEnemyState2 in autoTest.ExpectedResultingScenarioState.AllEnemyStates)
					{
						if (allEnemyState2.ConfigPerPartySize != null && allEnemyState2.ConfigPerPartySize.Count > 0 && allEnemyState2.ConfigPerPartySize.ContainsKey(1) && allEnemyState2.ConfigPerPartySize.ContainsKey(2))
						{
							allEnemyState2.ConfigPerPartySize[1] = allEnemyState2.ConfigPerPartySize[2];
						}
					}
				}
			}
			AutoTestDataManager.CurrentlyRunningAutotestFile = autoTestFile;
			Global.CurrentAutoTestDataCopy = autoTest.DeepCopySerializableObject<AutoTestData>();
			Global.CurrentAutoTestDataCopy.ScenarioState.StateNeedsUpdatesSaved = autoTest.ScenarioState.StateNeedsUpdatesSaved;
			Global.LastLoadedAutoTestData = autoTest.DeepCopySerializableObject<AutoTestData>();
			Global.LastLoadedAutoTestData.ScenarioState.StateNeedsUpdatesSaved = autoTest.ScenarioState.StateNeedsUpdatesSaved;
			if (loadIntoEditor)
			{
				Global.CurrentEditorAutoTestData = autoTest;
				Global.CurrentEditorLevelData = autoTest;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Autotest.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void ParseCommandLineArgs()
	{
		try
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				Debug.Log("COMMAND LINE ARG " + i + ": " + commandLineArgs[i]);
				if (commandLineArgs[i].ToLower() == "-autotest")
				{
					if (i + 2 >= commandLineArgs.Length)
					{
						Debug.LogError("Bad arguments for Autotests - it requires an additional argument: event log to run.");
					}
					else
					{
						AutoTestFolder = commandLineArgs[i + 1];
						AutoTestResultsLog = commandLineArgs[i + 2];
						GameBootedForAutoTests = true;
						i += 2;
					}
				}
				if (commandLineArgs[i].ToLower() == "-loadmostrecentsave")
				{
					LoadMostRecentSave = true;
				}
				if (commandLineArgs[i].ToLower() == "-debug")
				{
					Main.s_DevMode = true;
				}
				if (commandLineArgs[i].ToLower() == "-autoupdate")
				{
					AutoupdateAutotests = true;
				}
			}
			IsModdingEnabled = true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurrs attempting to parse the command line arguments. \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void ValidateCheckpointSaves(string directoryPath)
	{
		List<FileInfo> list = (from f in new DirectoryInfo(directoryPath).GetFiles()
			orderby f.LastWriteTime descending
			select f).ToList();
		if (list.Count <= 10)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 10; num--)
		{
			FileInfo fileInfo = list[num];
			if (fileInfo.Directory.Exists)
			{
				PlatformLayer.FileSystem.RemoveFile(fileInfo.FullName);
			}
		}
	}

	public static string NextAvailableFilename(string path)
	{
		string text = "_({0})";
		if (!PlatformLayer.FileSystem.ExistsFile(path))
		{
			return path;
		}
		if (Path.HasExtension(path))
		{
			return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), text));
		}
		return GetNextFilename(path + text);
	}

	private void CheckDataForCorrupt(byte[] data)
	{
		if (data.All((byte x) => x == 0))
		{
			throw new Exception("Data was corrupted");
		}
	}

	private void CheckFileForCorrupt(string path)
	{
		if (PlatformLayer.FileSystem.CheckFileForCorrupt(path))
		{
			throw new Exception("File was corrupted");
		}
	}

	private static string GetNextFilename(string pattern)
	{
		string text = string.Format(pattern, 1);
		if (text == pattern)
		{
			throw new ArgumentException("The pattern must include an index place-holder", "pattern");
		}
		if (!PlatformLayer.FileSystem.ExistsFile(text))
		{
			return text;
		}
		int num = 1;
		int num2 = 2;
		while (PlatformLayer.FileSystem.ExistsFile(string.Format(pattern, num2)))
		{
			num = num2;
			num2 *= 2;
		}
		while (num2 != num + 1)
		{
			int num3 = (num2 + num) / 2;
			if (PlatformLayer.FileSystem.ExistsFile(string.Format(pattern, num3)))
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		return string.Format(pattern, num2);
	}
}
