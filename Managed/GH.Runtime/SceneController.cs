#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AStar;
using Assets.Script.GUI.MainMenu.Modding;
using Chronos;
using ClockStone;
using EPOOutline;
using FFSNet;
using FFSThreads;
using GLOOM;
using GLOOM.MainMenu;
using I2.Loc;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.State;
using Platforms;
using Platforms.PS5;
using Platforms.Utils;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.PlatformLayer;
using SharedLibrary;
using SharedLibrary.Client;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
	private enum ESceneType
	{
		None,
		Empty,
		MainMenu,
		CampaignMap,
		NewAdventureMap,
		Scenario
	}

	private const string InitialLoadingScreen = "InitialLoadingScreen";

	public const string MainMenuSceneKeyboard = "MainMenu";

	public const string MainMenuSceneGamepad = "MainMenu_gamepad";

	private const string NewAdventureMapSceneKeyboard = "NewAdventureMap";

	private const string NewAdventureMapSceneGamepad = "NewAdventureMap_gamepad";

	private const string CampaignMapSceneKeyboard = "CampaignMap";

	private const string CampaignMapSceneGamepad = "CampaignMap_gamepad";

	private const string ScenarioSceneKeyboard = "Game";

	private const string ScenarioSceneGamepad = "Game_gamepad";

	private const string EmptyScene = "EmptyScene";

	public static SceneController Instance;

	[NonSerialized]
	public Thread MainThread;

	public GameObject MainSceneCamera;

	public GameObject LoadingScreenGO;

	public LoadingScreen LoadingScreenInstance;

	public GameObject GlobalCanvas;

	[SerializeField]
	private AssetReference _errorMessagePrefabReference;

	[SerializeField]
	private AssetReference _errorMessageGamepadPrefabReference;

	[SerializeField]
	private ApparanceResourceListLoader _apparanceResourceListLoader;

	[SerializeField]
	private SettingsHolder _settingsHolder;

	private List<string> scenesInBuild;

	private Action LoadSceneCallback;

	private Action _unloadSceneCallback;

	private bool ymlChecksumsMatch;

	private Scene currentlyLoadedScene;

	private YMLChecksums ymlChecksums;

	private bool mostRecentSaveLoaded;

	private AsyncOperationHandle<GameObject> _errorMessageHandler;

	private bool _isStarted;

	private Coroutine _playerPrefsInitialization;

	private ErrorMessage _errorMessage;

	private string _applicationDataPath;

	private ESceneType _loadingSceneType;

	private const int IncreasedStackSize = 1048576;

	public bool ReloadMainMenu { get; set; }

	public ErrorMessage GlobalErrorMessage => GetErrorMessage();

	public bool IsLoading { get; private set; }

	public bool ScenarioIsLoading { get; set; }

	public Scene GetCurrentScene => currentlyLoadedScene;

	public YMLLoading YML { get; private set; }

	public GHModding Modding { get; private set; }

	public ModdingService ModService { get; private set; }

	public bool GameLoadedAndHostReady { get; set; }

	public bool GameLoadedAndClientReady { get; set; }

	public bool ModLoadingCompleted { get; set; }

	public float GameSpeedIncreaseAmount { get; set; }

	public bool SelectingPersonalQuest { get; set; }

	public bool BusyProcessingResults { get; set; }

	public bool RetiringCharacter { get; set; }

	public bool CheckingLockedContent { get; set; }

	public List<string> MainMenuVideos { get; set; }

	public HashSet<int> MainMenuVideoHashSet { get; set; }

	public bool InitialLoadingComplete { get; private set; }

	public IUpdater Updater { get; private set; }

	public string ApplicationDataPath
	{
		get
		{
			if (!string.IsNullOrEmpty(_applicationDataPath))
			{
				return _applicationDataPath;
			}
			return Application.dataPath;
		}
	}

	public string GetCurrentSceneName
	{
		get
		{
			_ = currentlyLoadedScene;
			return currentlyLoadedScene.name;
		}
	}

	public bool DataRestoring { get; set; }

	public SettingsHolder SettingsHolder => _settingsHolder;

	public event Action UnloadSpecialMemory;

	public event BasicEventHandler OnSaveDataLoaded;

	private string GetSceneNameForType(ESceneType type)
	{
		return type switch
		{
			ESceneType.Empty => "EmptyScene", 
			ESceneType.MainMenu => InputManager.GamePadInUse ? "MainMenu_gamepad" : "MainMenu", 
			ESceneType.NewAdventureMap => InputManager.GamePadInUse ? "NewAdventureMap_gamepad" : "NewAdventureMap", 
			ESceneType.CampaignMap => InputManager.GamePadInUse ? "CampaignMap_gamepad" : "CampaignMap", 
			ESceneType.Scenario => InputManager.GamePadInUse ? "Game_gamepad" : "Game", 
			_ => throw new NotImplementedException($"Scene type {type} is unknown!"), 
		};
	}

	private void Awake()
	{
		Instance = this;
		_applicationDataPath = Application.dataPath;
		ReloadMainMenu = false;
		ScenarioIsLoading = false;
		MainThread = Thread.CurrentThread;
		ModService = new ModdingService();
		GameSpeedIncreaseAmount = 3f;
		try
		{
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				MainMenuVideos = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "Resources", "Data", "StreamingAssets", "Movies", "Ambient"), "*.mov").ToList();
			}
			else if (Application.platform == RuntimePlatform.Switch)
			{
				MainMenuVideos = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies_MP4_30", "Ambient"), "*.mp4").ToList();
			}
			else if (Application.platform == RuntimePlatform.GameCoreXboxOne)
			{
				MainMenuVideos = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies_MOV_30", "Ambient"), "*.mov").ToList();
			}
			else
			{
				MainMenuVideos = PlatformLayer.FileSystem.GetFiles(Path.Combine(Application.dataPath, "StreamingAssets", "Movies", "Ambient"), "*.mov").ToList();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while trying to find Main Menu movie files.\n" + ex.Message + "\n" + ex.StackTrace);
			MainMenuVideos = new List<string>();
		}
		MainMenuVideoHashSet = new HashSet<int>();
		ModLoadingCompleted = false;
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			EnableCanvas();
		}
		UnitySystemConsoleRedirector.Redirect();
		Updater = base.gameObject.AddComponent<Updater>();
		InputManager.DeviceChangedEvent = (Action)Delegate.Combine(InputManager.DeviceChangedEvent, new Action(OnDeviceChanged));
	}

	private void EnableCanvas()
	{
		if (GlobalCanvas != null)
		{
			GlobalCanvas.SetActive(value: true);
		}
	}

	private void OnDestroy()
	{
		if (YML != null)
		{
			YML.Close();
		}
		FFSNetwork.OnDesyncDetected = (DesyncDetectedEvent)Delegate.Remove(FFSNetwork.OnDesyncDetected, new DesyncDetectedEvent(ShowDesyncDetectionError));
		SceneManager.sceneLoaded -= OnSceneLoaded;
		Instance = null;
		InputManager.DeviceChangedEvent = (Action)Delegate.Remove(InputManager.DeviceChangedEvent, new Action(OnDeviceChanged));
	}

	private IEnumerator Start()
	{
		Debug.Log("Start SceneController");
		yield return Timekeeper.instance.WaitForSeconds(0.1f);
		LogUtils.Log("[SceneController] Waiting for data restoration...");
		while (DataRestoring)
		{
			yield return null;
		}
		LogUtils.Log("[SceneController] Data restored");
		AdditionalInitAudioController();
		if (PlatformLayer.Instance.IsDelayedInit)
		{
			if (PlayerPrefs.HasKey("GlobalData.dat"))
			{
				GlobalData global = JsonUtility.FromJson<GlobalData>(PlayerPrefs.GetString("GlobalData.dat"));
				SaveData.Instance.Global = global;
				SettingsHolder.SetupPrimaryAudio();
			}
			if (PlayerPrefs.HasKey("GloomSaven.dat"))
			{
				RootSaveData rootSaveData = JsonUtility.FromJson<RootSaveData>(PlayerPrefs.GetString("GloomSaven.dat"));
				SaveData.Instance.CreatePrimaryRootData();
				Debug.Log("Set saved language");
				SaveData.Instance.RootData.CurrentLanguage = rootSaveData.CurrentLanguage;
				yield return SettingsHolder.SetupPrimaryLanguage();
			}
			else
			{
				SaveData.Instance.CreatePrimaryRootData();
				yield return SettingsHolder.SetupPrimaryLanguage();
			}
		}
		else if (PlatformLayer.Instance.IsConsole)
		{
			if (SaveData.Instance == null)
			{
				UnityEngine.Debug.LogError("[SceneController] Looks like SaveData is not initialized");
			}
			else
			{
				UnityEngine.Debug.Log("[SceneController] Trying to load SaveData if it does already exist");
				yield return SaveData.Instance.LoadGlobalData();
				UnityEngine.Debug.Log("[SceneController] Finished Loading GlobalData...");
			}
			if (SaveData.Instance.RootData == null)
			{
				Debug.Log("[SceneController] SaveData.Instance.RootData == null, creating a new one.");
				SaveData.Instance.CreatePrimaryRootData();
			}
			else if (string.IsNullOrEmpty(SaveData.Instance.RootData.CurrentLanguage))
			{
				Debug.Log("[SceneController] SaveData.Instance.RootData.CurrentLanguage is null or empty, creating a new PrimaryRootData");
				SaveData.Instance.CreatePrimaryRootData();
			}
			else
			{
				Debug.Log("[SceneController] The current language recorded in SaveData is " + SaveData.Instance.RootData.CurrentLanguage + ".");
			}
			yield return SettingsHolder.SetupPrimaryLanguage();
			ShowLoadingScreen();
		}
		try
		{
			YML = new YMLLoading();
		}
		catch
		{
			Debug.LogError("Failed to Initialise YMLLoading class!");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		yield return CheckYMLCheckSums();
		if (!ymlChecksumsMatch)
		{
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00002_MAC", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			}
			else
			{
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00002", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			}
			yield break;
		}
		if (PlatformLayer.Instance.IsDelayedInit)
		{
			yield return Instance.InitialiseGloomhavenCoroutine(SaveData.Instance.PersistentDataPath, RootSaveData.CoreRulebasePath);
			LoadingScreenInstance.UpdateProgressBar(12f);
		}
		else
		{
			yield return SaveData.Instance.LoadRulebase();
		}
		while (SaveData.Instance.Global == null)
		{
			yield return null;
		}
		StartCoroutine(StartAudio());
		while (GlobalErrorMessage.ShowingMessage)
		{
			yield return null;
		}
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			LoadDlcLanguageUpdate();
		}
		if (PlatformLayer.Modding.ModdingSupported)
		{
			Modding = new GHModding();
			PlatformLayer.Modding.RefreshMods();
			while (!PlatformLayer.Modding.ModsLoaded)
			{
				yield return null;
			}
			float progressIncrement = 90f / (float)Modding.Mods.Count((GHMod w) => w.MetaData.ModType == GHModMetaData.EModType.Language || w.MetaData.ModType == GHModMetaData.EModType.CustomLevels);
			Debug.Log(progressIncrement);
			foreach (GHMod item in Modding.Mods.Where((GHMod w) => w.MetaData.ModType == GHModMetaData.EModType.Language || w.MetaData.ModType == GHModMetaData.EModType.CustomLevels))
			{
				yield return GHModding.ValidateMod(item, null, writeResultsToFile: false);
				Instance.LoadingScreenInstance.IncrementProgressBar(progressIncrement);
				yield return null;
			}
			yield return Modding.LoadLanguageMods();
			Instance.LoadingScreenInstance.UpdateProgressBar(95f);
			if (SaveData.Instance.RootData.CurrentLanguage != null && SaveData.Instance.RootData.CurrentLanguage != string.Empty && I2.Loc.LocalizationManager.GetAllLanguages().Contains(SaveData.Instance.RootData.CurrentLanguage))
			{
				I2.Loc.LocalizationManager.CurrentLanguage = SaveData.Instance.RootData.CurrentLanguage;
			}
			Instance.LoadingScreenInstance.UpdateProgressBar(100f);
			yield return null;
		}
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.LoadingWithProgress);
		yield return null;
		if (Singleton<InputManager>.Instance.IsPCAndGamepadVersion() && !PlatformLayer.Instance.IsConsole)
		{
			Singleton<InitialInputScreen>.Instance.Show();
			while (!Singleton<InputManager>.Instance.IsPlayerSelectInputDevice)
			{
				yield return null;
			}
		}
		else
		{
			Singleton<InputManager>.Instance.IsPlayerSelectInputDevice = true;
		}
		Debug.Log("Creating Custom Level directories");
		try
		{
			if (!PlatformLayer.FileSystem.ExistsDirectory(RootSaveData.LevelEditorCustomLVLDAT))
			{
				PlatformLayer.FileSystem.CreateDirectory(RootSaveData.LevelEditorCustomLVLDAT);
			}
		}
		catch
		{
		}
		try
		{
			if (!PlatformLayer.FileSystem.ExistsDirectory(RootSaveData.LevelEditorCustomYML))
			{
				PlatformLayer.FileSystem.CreateDirectory(RootSaveData.LevelEditorCustomYML);
			}
		}
		catch
		{
		}
		try
		{
			this.OnSaveDataLoaded?.Invoke();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while loading main menu scene at startup\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			yield break;
		}
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			yield return SettingsHolder.Setup();
		}
		while (GlobalErrorMessage.ShowingMessage)
		{
			yield return null;
		}
		LoadingScreenInstance.UpdateProgressBar(100f);
		yield return null;
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
		yield return null;
		try
		{
			scenesInBuild = new List<string>();
			for (int num = 1; num < SceneManager.sceneCountInBuildSettings; num++)
			{
				string scenePathByBuildIndex = SceneUtility.GetScenePathByBuildIndex(num);
				int num2 = scenePathByBuildIndex.LastIndexOf("/");
				scenesInBuild.Add(scenePathByBuildIndex.Substring(num2 + 1, scenePathByBuildIndex.LastIndexOf(".") - num2 - 1));
			}
			string text = string.Empty;
			for (int num3 = 0; num3 < scenesInBuild.Count; num3++)
			{
				text = text + "Added scene: " + scenesInBuild[num3] + "\n";
			}
			Debug.Log(text);
		}
		catch (Exception ex2)
		{
			Debug.LogError("An exception occurred while loading main menu scene at startup\n" + ex2.Message + "\n" + ex2.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
			yield break;
		}
		if (SaveData.Instance.LoadMostRecentSave)
		{
			yield return LoadMostRecentSave();
			if (mostRecentSaveLoaded)
			{
				mostRecentSaveLoaded = false;
				yield break;
			}
		}
		if (RootSaveData.ReleaseVersionType == RootSaveData.EReleaseTypes.Dev || RootSaveData.ReleaseVersionType == RootSaveData.EReleaseTypes.OpenBeta || RootSaveData.ReleaseVersionType == RootSaveData.EReleaseTypes.ClosedBeta)
		{
			SaveData.Instance.Global.MPEndOfTurnCompare = true;
		}
		else if (RootSaveData.ReleaseVersionType != RootSaveData.EReleaseTypes.InEditor)
		{
			SaveData.Instance.Global.MPEndOfTurnCompare = false;
		}
		if (GlobalErrorMessage.ShowingMessage)
		{
			yield break;
		}
		try
		{
			if (SaveData.Instance.GameBootedForAutoTests)
			{
				AutoTestController.s_Instance.EvaluateAllAvailableTestsAndReportString(OnAutoTestComplete);
			}
			else if (PlatformLayer.GameProvider.GameIntentReceiver is SampleGameIntentReceiver { LatestReceivedData: not null, LatestReceivedData: LaunchActivityData latestReceivedData } sampleGameIntentReceiver)
			{
				PartyAdventureData partyAdventureData = null;
				PartyAdventureData resumeCampaign = SaveData.Instance.Global.ResumeCampaign;
				if (resumeCampaign != null && SaveData.Instance.Global.AllCampaigns.Contains(resumeCampaign) && !resumeCampaign.HasInvalidDLCs())
				{
					partyAdventureData = resumeCampaign;
				}
				PartyAdventureData partyAdventureData2 = null;
				PartyAdventureData resumeAdventure = SaveData.Instance.Global.ResumeAdventure;
				if (resumeAdventure != null && SaveData.Instance.Global.AllAdventures.Contains(resumeAdventure) && !resumeAdventure.HasInvalidDLCs())
				{
					partyAdventureData2 = resumeAdventure;
				}
				if (partyAdventureData == null && partyAdventureData2 == null)
				{
					LoadMainMenu();
					sampleGameIntentReceiver.ResetLatestReceivedData();
				}
				else if (partyAdventureData != null && partyAdventureData2 != null)
				{
					if (partyAdventureData.LastSavedTimeStamp > partyAdventureData2.LastSavedTimeStamp)
					{
						SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Campaign, partyAdventureData);
						SaveData.Instance.LoadCampaignMode(partyAdventureData, isJoiningMPClient: false);
					}
					else
					{
						SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Guildmaster, partyAdventureData2);
						SaveData.Instance.LoadGuildmasterMode(partyAdventureData2, isJoiningMPClient: false);
					}
					sampleGameIntentReceiver.ResetLatestReceivedData();
				}
				else if (partyAdventureData != null && IsLaunchingCampaignActivity(latestReceivedData))
				{
					SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Campaign, partyAdventureData);
					SaveData.Instance.LoadCampaignMode(partyAdventureData, isJoiningMPClient: false);
				}
				else if (partyAdventureData2 != null && IsLaunchingGuildmasterActivity(latestReceivedData))
				{
					SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Guildmaster, partyAdventureData2);
					SaveData.Instance.LoadGuildmasterMode(partyAdventureData2, isJoiningMPClient: false);
				}
				else
				{
					LoadMainMenu();
				}
			}
			else
			{
				LoadMainMenu();
			}
		}
		catch (Exception ex3)
		{
			Debug.LogError("An exception occurred while loading main menu scene at startup\n" + ex3.Message + "\n" + ex3.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex3.StackTrace, Application.Quit, ex3.Message);
			yield break;
		}
		FFSNetwork.OnDesyncDetected = (DesyncDetectedEvent)Delegate.Remove(FFSNetwork.OnDesyncDetected, new DesyncDetectedEvent(ShowDesyncDetectionError));
		FFSNetwork.OnDesyncDetected = (DesyncDetectedEvent)Delegate.Combine(FFSNetwork.OnDesyncDetected, new DesyncDetectedEvent(ShowDesyncDetectionError));
		ClearAfterSceneUnloading();
		InitialLoadingComplete = true;
		_isStarted = true;
		static bool IsLaunchingCampaignActivity(LaunchActivityData data)
		{
			return data.ActivityId.ToLower().Contains("campaign");
		}
		static bool IsLaunchingGuildmasterActivity(LaunchActivityData data)
		{
			return data.ActivityId.ToLower().Contains("guildmaster");
		}
	}

	public IEnumerator TrySetupPrimaryLanguage()
	{
		if (PlatformLayer.Instance.StartupLanguage != null)
		{
			string startupLanguage = PlatformLayer.Instance.StartupLanguage.GetStartupLanguage();
			SaveData.Instance.RootData.CurrentLanguage = startupLanguage;
			I2.Loc.LocalizationManager.CurrentLanguage = startupLanguage;
		}
		yield return SettingsHolder.SetupPrimaryLanguage();
	}

	public void LoadDlcLanguageUpdate()
	{
		bool flag = SaveData.Instance.Global.IsJotlDlcSaveExists();
		foreach (DLCRegistry.EDLCKey item in DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey w) => w != DLCRegistry.EDLCKey.None && w != DLCRegistry.EDLCKey.DLC3))
		{
			if (PlatformLayer.DLC.CanPlayDLC(item) || flag)
			{
				string empty = string.Empty;
				empty = YML.DLCLanguageCSVPackage(item);
				if (PlatformLayer.FileSystem.ExistsFile(empty))
				{
					GLOOM.LocalizationManager.AddCSVSource(empty);
				}
			}
		}
	}

	private ErrorMessage GetErrorMessage()
	{
		if (_errorMessage == null)
		{
			AssetBundleManager.ReleaseHandle(_errorMessageHandler, releaseInstance: true);
			object key = (InputManager.GamePadInUse ? _errorMessageGamepadPrefabReference.RuntimeKey : _errorMessagePrefabReference.RuntimeKey);
			_errorMessageHandler = Addressables.InstantiateAsync(key, GlobalCanvas.transform, instantiateInWorldSpace: false, trackHandle: false);
			_errorMessage = _errorMessageHandler.WaitForCompletion().GetComponent<ErrorMessage>();
			_errorMessage.Hide();
		}
		return _errorMessage;
	}

	private void UnloadErrorMessage()
	{
		AssetBundleManager.ReleaseHandle(_errorMessageHandler, releaseInstance: true);
		_errorMessageHandler = default(AsyncOperationHandle<GameObject>);
		_errorMessage = null;
	}

	private void OnDeviceChanged()
	{
		UnloadErrorMessage();
	}

	private IEnumerator CompileDLC(DLCRegistry.EDLCKey dlc)
	{
		string zipPath = YML.DLCGlobalRulesetZip(dlc);
		if (!YML.CompileRuleset(YML.DLCGlobalRuleset(dlc), zipPath))
		{
			Debug.LogError("Failed to compile Global ruleset for DLC " + GloomUtility.GetEnumDescription(dlc));
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		string zipPath2 = YML.DLCSharedRulesetZip(dlc);
		if (!YML.CompileRuleset(YML.DLCSharedRuleset(dlc), zipPath2))
		{
			Debug.LogError("Failed to compile Shared ruleset for DLC " + GloomUtility.GetEnumDescription(dlc));
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		string zipPath3 = YML.DLCCampaignRulesetZip(dlc);
		if (!YML.CompileRuleset(YML.DLCCampaignRuleset(dlc), zipPath3))
		{
			Debug.LogError("Failed to compile Campaign ruleset for DLC " + GloomUtility.GetEnumDescription(dlc));
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		string zipPath4 = YML.DLCGuildmasterRulesetZip(dlc);
		if (!YML.CompileRuleset(YML.DLCGuildmasterRuleset(dlc), zipPath4))
		{
			Debug.LogError("Failed to compile Guildmaster ruleset for DLC " + GloomUtility.GetEnumDescription(dlc));
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		string zipPath5 = YML.DLCCustomScenariosRulesetZip(dlc);
		if (!YML.CompileRuleset(YML.DLCCustomScenariosRuleset(dlc), zipPath5))
		{
			Debug.LogError("Failed to compile Custom Scenarios ruleset for DLC " + GloomUtility.GetEnumDescription(dlc));
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
		}
	}

	public IEnumerator ReloadGlobalCardAssets(bool ignoreLoadingScreen = false)
	{
		if (!ignoreLoadingScreen)
		{
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.LoadingWithProgress);
			ShowLoadingScreen();
		}
		yield return null;
		yield return PersistentData.s_Instance.InitGlobalData();
		if (!ignoreLoadingScreen)
		{
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			DisableLoadingScreen();
		}
	}

	private IEnumerator StartAudio()
	{
		if (!string.IsNullOrEmpty(GlobalSettings.Instance.m_MainMenuMusicPlaylist))
		{
			while (!AudioController.IsMusicEnabled())
			{
				yield return null;
			}
			if (!PlatformLayer.Instance.IsDelayedInit)
			{
				yield return SaveData.Instance.LoadGlobalData();
			}
			if (SaveData.Instance != null && SaveData.Instance.Global != null)
			{
				AudioController.SetGlobalVolume((float)SaveData.Instance.Global.MasterVolume / 100f);
				AudioController.SetCategoryVolume("Music", (float)SaveData.Instance.Global.MusicVolume / 100f);
			}
			AudioController.PlayMusicPlaylist(GlobalSettings.Instance.m_MainMenuMusicPlaylist);
		}
	}

	private void OnAutoTestComplete(List<AutotestResult> completionReport)
	{
		try
		{
			if (!PlatformLayer.FileSystem.ExistsDirectory(Path.GetDirectoryName(SaveData.Instance.AutoTestResultsLog)))
			{
				Debug.Log("Creating AutoTestResultsLog directory: " + SaveData.Instance.AutoTestResultsLog);
				PlatformLayer.FileSystem.CreateDirectory(Path.GetDirectoryName(SaveData.Instance.AutoTestResultsLog));
			}
			else if (PlatformLayer.FileSystem.ExistsFile(SaveData.Instance.AutoTestResultsLog))
			{
				PlatformLayer.FileSystem.RemoveFile(SaveData.Instance.AutoTestResultsLog);
			}
			new List<string>();
			int num = 0;
			int num2 = 0;
			XmlTextWriter xmlTextWriter = new XmlTextWriter(SaveData.Instance.AutoTestResultsLog, Encoding.UTF8);
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.WriteStartElement("testsuite");
			xmlTextWriter.WriteAttributeString("tests", completionReport.Count.ToString());
			xmlTextWriter.WriteAttributeString("name", "AllTests");
			foreach (AutotestResult item in completionReport)
			{
				xmlTextWriter.WriteStartElement("testcase");
				xmlTextWriter.WriteAttributeString("classname", "Autotest");
				xmlTextWriter.WriteAttributeString("name", item.TestName);
				if (!item.Result)
				{
					num2++;
					xmlTextWriter.WriteStartElement("failure");
					xmlTextWriter.WriteAttributeString("type", "Fail");
					xmlTextWriter.WriteValue(item.TestLog);
					xmlTextWriter.WriteEndElement();
				}
				else
				{
					num++;
				}
				xmlTextWriter.WriteEndElement();
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Close();
			string content = "Tests Passed: " + num + "\nTests Failed: " + num2 + "\n" + string.Join("\n", completionReport.Select((AutotestResult s) => s.TestName + ": " + (s.Result ? "PASS" : ("FAIL\n" + s.TestLog))));
			PlatformLayer.FileSystem.FileWriteAllText(SaveData.Instance.AutoTestResultsLog + "2", content);
			if (!GloomhavenShared.OverrideGameBootedForAutoTests)
			{
				Application.Quit(completionReport.Any((AutotestResult a) => !a.Result) ? 42 : 0);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
			Application.Quit(-1);
		}
	}

	public IEnumerator InitialiseGloomhavenCoroutine(string applicationPersistenDataPath, string rulebaseDataRoot)
	{
		try
		{
			SharedClient.Initialise(applicationPersistenDataPath, rulebaseDataRoot);
			ScenarioRuleClient.Initialise();
			MapRuleLibraryClient.Instance.Initialise();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while initialising the DLLs\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
		LoadingScreenInstance.UpdateProgressBar(6f);
		yield return null;
		if (!PlatformLayer.UserData.IsUserDataInitialised())
		{
			PlatformLayer.UserData.Initialise(PlatformLayer.Platform);
		}
		while (!PlatformLayer.UserData.IsUserDataInitialised())
		{
			yield return null;
		}
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			yield return RefreshEntitlements();
			yield return InitAndLoadYML();
		}
		LoadingScreenInstance.UpdateProgressBar(12f);
	}

	public IEnumerator InitAndLoadYML()
	{
		Thread loadYML = new Thread((ThreadStart)delegate
		{
			YML.Init();
		}, 1048576);
		loadYML.Start();
		while (loadYML.IsAlive)
		{
			yield return null;
		}
		if (!YMLLoading.InitResult)
		{
			Debug.LogError("Failed to Initialise YMLLoading class!");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		LoadingScreenInstance.UpdateProgressBar(7f);
		yield return null;
		Thread loadGlobalYML = new Thread((ThreadStart)delegate
		{
			YML.LoadGlobal();
		}, 1048576);
		loadGlobalYML.Start();
		while (loadGlobalYML.IsAlive)
		{
			yield return null;
		}
		LoadingScreenInstance.UpdateProgressBar(10f);
		yield return null;
		if (!YMLLoading.LastLoadResult)
		{
			Debug.LogError("Unable to load Global YML");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
			yield break;
		}
		try
		{
			ScenarioRuleClient.LoadData();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while initialising the DLLs\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public IEnumerator RefreshEntitlements()
	{
		Debug.Log("[SceneController] Refreshing entitlements...");
		bool entitlementsRefreshed = false;
		if (PlatformLayer.Platform.PlatformEntitlements == null)
		{
			LogUtils.LogError($"There is no Entitlements implementation for {PlatformLayer.Platform.GetType()}");
			yield break;
		}
		PlatformLayer.Platform.PlatformEntitlements.RefreshEntitlements(OnEntitlementsRefreshed);
		while (!entitlementsRefreshed)
		{
			Debug.LogWarning("[SceneController] Entitlements refreshing...");
			yield return null;
		}
		Debug.Log("[SceneController] Entitlements refreshed");
		void OnEntitlementsRefreshed()
		{
			entitlementsRefreshed = true;
		}
	}

	private IEnumerator CheckYMLCheckSums()
	{
		Debug.Log("Checking YML CheckSums");
		yield return null;
		ymlChecksumsMatch = true;
		float ymlChecksumsProgress = 5f;
		new List<string>();
		ymlChecksums = new YMLChecksums();
		List<string> compiledRulesets;
		try
		{
			if (!ymlChecksums.Load())
			{
				throw new Exception("Failed to load YML Checksums file at " + ymlChecksums.SavePath);
			}
			compiledRulesets = new List<string>(PlatformLayer.FileSystem.GetFiles(RootSaveData.CoreRulebasePath, "*.ruleset"));
			for (int i = 0; i < compiledRulesets.Count; i++)
			{
				compiledRulesets[i] = compiledRulesets[i].Replace("\\", "/");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An unexpected error occurred while attempting to parse the YML file hashes.\n" + ex.Message + "\n" + ex.StackTrace);
			ymlChecksumsMatch = false;
			yield break;
		}
		if (ymlChecksums == null || ymlChecksums.YMLChecksumsFilenames == null || ymlChecksums.YMLChecksumsFilenames.Count == 0)
		{
			Debug.LogError("Invalid Ruleset Checksums!");
			ymlChecksumsMatch = false;
			yield break;
		}
		float ymlChecksumsIncrement = ymlChecksumsProgress / (float)ymlChecksums.YMLChecksumsFilenames.Count;
		for (int x = 0; x < ymlChecksums.YMLChecksumsFilenames.Count; x++)
		{
			try
			{
				string fullFilename = Path.Combine(RootSaveData.CoreRulebasePath, ymlChecksums.YMLChecksumsFilenames[x]);
				fullFilename = fullFilename.Replace("\\", "/");
				if (compiledRulesets.Any((string a) => a == fullFilename))
				{
					compiledRulesets.Remove(fullFilename);
					using MD5 mD = MD5.Create();
					using MemoryStream inputStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(fullFilename));
					if (BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", string.Empty).ToLowerInvariant() != ymlChecksums.YMLChecksumsValues[x])
					{
						Debug.LogError("Checksum mismatch on file " + fullFilename);
						ymlChecksumsMatch = false;
					}
				}
				else
				{
					Debug.LogError("File is missing: " + fullFilename);
					ymlChecksumsMatch = false;
				}
				LoadingScreenInstance.IncrementProgressBar(ymlChecksumsIncrement);
			}
			catch (Exception ex2)
			{
				Debug.LogError("An unexpected error occurred while attempting to parse the YML file hashes.\n" + ex2.Message + "\n" + ex2.StackTrace);
				ymlChecksumsMatch = false;
			}
			yield return null;
		}
		try
		{
			if (compiledRulesets.Count > 0)
			{
				Debug.Log("Invalid rulesets were found in the rulebase.  All rulesets must be compiled with the build.\n" + string.Join("\n", compiledRulesets));
				ymlChecksumsMatch = false;
			}
		}
		catch (Exception ex3)
		{
			Debug.LogError("An unexpected error occurred while attempting to parse the YML file hashes.\n" + ex3.Message + "\n" + ex3.StackTrace);
			ymlChecksumsMatch = false;
		}
		LoadingScreenInstance.UpdateProgressBar(ymlChecksumsProgress);
	}

	public void UnloadInitialLoadingScreen()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).name == "InitialLoadingScreen")
			{
				SceneManager.UnloadSceneAsync("InitialLoadingScreen", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				break;
			}
		}
	}

	public void ExitApplication()
	{
		Application.Quit();
	}

	private IEnumerator LoadSceneCoroutine(ESceneType sceneType, Action callback = null, Action unloadCallback = null)
	{
		_loadingSceneType = sceneType;
		if (DataRestoring)
		{
			_loadingSceneType = ESceneType.None;
			yield break;
		}
		LoadSceneCallback = callback;
		_unloadSceneCallback = unloadCallback;
		ShowLoadingScreen();
		yield return null;
		Singleton<UINavigation>.Instance.Reset();
		Singleton<KeyActionHandlerController>.Instance.ResetSceneHandlers();
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			ObjectPool.ClearAllExceptCards();
		}
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
		if (currentlyLoadedScene.IsValid() && currentlyLoadedScene.isLoaded)
		{
			if (Choreographer.s_Choreographer != null && Choreographer.s_Choreographer.m_ProcGenScene.IsValid() && Choreographer.s_Choreographer.m_ProcGenScene.isLoaded)
			{
				try
				{
					SceneManager.UnloadSceneAsync(Choreographer.s_Choreographer.m_ProcGenScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
				}
				catch (Exception ex)
				{
					InputManager.RequestEnableInput(this, EKeyActionTag.All);
					Debug.LogError("An exception occurred while unloading the ProcGen scene\n" + ex.Message + "\n" + ex.StackTrace);
					GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
					_loadingSceneType = ESceneType.None;
					yield break;
				}
			}
			MainSceneCamera.SetActive(value: true);
			AsyncOperation asyncUnload;
			try
			{
				asyncUnload = SceneManager.UnloadSceneAsync(currentlyLoadedScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
			}
			catch (Exception ex2)
			{
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Debug.LogError("An exception occurred while unloading the current scene\n" + ex2.Message + "\n" + ex2.StackTrace);
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
				_loadingSceneType = ESceneType.None;
				yield break;
			}
			WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
			while (!asyncUnload.isDone)
			{
				yield return waitForEndOfFrameDelay;
			}
			yield return null;
			ClearAfterSceneUnloading();
			yield return null;
			asyncUnload = Resources.UnloadUnusedAssets();
			while (!asyncUnload.isDone)
			{
				yield return null;
			}
			yield return null;
			if (_unloadSceneCallback != null)
			{
				_unloadSceneCallback?.Invoke();
				_unloadSceneCallback = null;
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
		AsyncOperation asyncLoad;
		try
		{
			asyncLoad = SceneManager.LoadSceneAsync(GetSceneNameForType(sceneType), LoadSceneMode.Additive);
		}
		catch (Exception ex3)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("An exception occurred while loading the new scene\n" + ex3.Message + "\n" + ex3.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", ex3.StackTrace, Application.Quit, ex3.Message);
			_loadingSceneType = ESceneType.None;
			yield break;
		}
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		_loadingSceneType = ESceneType.None;
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Singleton<PhysicsController>.Instance.Setup(scene.name == "Game_gamepad");
		if (MainSceneCamera.activeInHierarchy && PersistentData.s_Instance.IsDataLoaded)
		{
			MainSceneCamera.SetActive(value: false);
		}
		currentlyLoadedScene = scene;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (SharedClient.CriticalYMLFailure != string.Empty)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			GlobalErrorMessage.ShowMessageDefaultTitle(SharedClient.CriticalYMLFailure, "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
		}
		else if (ReloadMainMenu)
		{
			ReloadMainMenu = false;
			LoadMainMenu();
		}
		else if (LoadSceneCallback != null)
		{
			LoadSceneCallback();
			LoadSceneCallback = null;
		}
	}

	public void ShowLoadingScreen()
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
		if (!IsLoading)
		{
			IsLoading = true;
			Debug.Log("Showing loading screen.");
			try
			{
				LoadingScreenGO.SetActive(value: true);
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred while showing loading screen\n" + ex.Message + "\n" + ex.StackTrace);
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			}
		}
	}

	public void DisableLoadingScreen()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		if (!IsLoading || YML.IsUnloading)
		{
			return;
		}
		MainMenuUIManager instance = MainMenuUIManager.Instance;
		if ((object)instance == null || !instance.ModeSelectionScreen.BusyLoading)
		{
			IsLoading = false;
			Debug.Log("Disabling loading screen.");
			try
			{
				LoadingScreenGO.SetActive(value: false);
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred while disabling loading screen\n" + ex.Message + "\n" + ex.StackTrace);
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
			}
		}
	}

	public void LoadMainMenu()
	{
		LoadMainMenu(null, null);
	}

	public void LoadMainMenu(Action onUnloadPreviousScene = null, Action onFinish = null)
	{
		try
		{
			InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			Timekeeper.instance.m_GlobalClock.localTimeScale = 1f;
			ScenarioRuleClient.ToggleMessageProcessing(process: false);
			ObjectPool.RecyclePendingObjects();
			if (AutoTestController.s_Instance != null && AutoTestController.s_Instance.CurrentOverlay != null)
			{
				UnityEngine.Object.Destroy(AutoTestController.s_Instance.CurrentOverlay.gameObject);
			}
			ScenarioManager.Mode = ScenarioManager.EDLLMode.None;
			SimpleLog.AddToSimpleLog("User exit to Main Menu");
			SimpleLog.WriteSimpleLogToFile();
			if (SaveData.Instance.Global.GameMode != EGameMode.MainMenu && SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
			{
				if (SaveData.Instance.Global.CurrentAdventureData != null)
				{
					SaveData.Instance.SaveQueue.EnqueueOperation(delegate(Action action)
					{
						SaveData.Instance.Global.CurrentAdventureData.RefreshMapStateFromFile(SaveData.Instance.Global.CurrentAdventureData.PartyMainSaveFile, action);
					}, delegate
					{
						SaveData.Instance.Global.GameMode = EGameMode.MainMenu;
						GameLoadedAndClientReady = false;
						GameLoadedAndHostReady = false;
						ControllableRegistry.Reset();
						FFSNetwork.Shutdown(FFSNetwork.IsHost ? new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession) : null);
						YML.Unload(regenCards: false);
						StartCoroutine(LoadMainMenuCoroutine(onUnloadPreviousScene, onFinish));
					});
				}
			}
			else
			{
				SaveData.Instance.Global.GameMode = EGameMode.MainMenu;
				GameLoadedAndClientReady = false;
				GameLoadedAndHostReady = false;
				ControllableRegistry.Reset();
				FFSNetwork.Shutdown(FFSNetwork.IsHost ? new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession) : null);
				YML.Unload(regenCards: false);
				StartCoroutine(LoadMainMenuCoroutine(onUnloadPreviousScene, onFinish));
			}
		}
		catch (Exception ex)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("An exception occurred while loading main menu\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00006", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public void WaitForDataRestorationAndReturnToMainMenu()
	{
		try
		{
			SaveData.Instance.SaveQueue.AbortAllOperations();
			InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
			ShowLoadingScreen();
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			Timekeeper.instance.m_GlobalClock.localTimeScale = 1f;
			ScenarioRuleClient.ToggleMessageProcessing(process: false);
			ObjectPool.RecyclePendingObjects();
			if (AutoTestController.s_Instance != null && AutoTestController.s_Instance.CurrentOverlay != null)
			{
				UnityEngine.Object.Destroy(AutoTestController.s_Instance.CurrentOverlay.gameObject);
			}
			ScenarioManager.Mode = ScenarioManager.EDLLMode.None;
			SimpleLog.AddToSimpleLog("Restoring user's data...");
			SimpleLog.WriteSimpleLogToFile();
			GameLoadedAndClientReady = false;
			GameLoadedAndHostReady = false;
			ControllableRegistry.Reset();
			FFSNetwork.Shutdown(FFSNetwork.IsHost ? new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession) : null);
			YML?.Unload(regenCards: false);
			StartCoroutine(WaitForRestorationComplete());
		}
		catch (Exception ex)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("An exception occurred while restoring data\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00006", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	public void LoadEmptyScene()
	{
		try
		{
			InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
			LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
			Timekeeper.instance.m_GlobalClock.localTimeScale = 1f;
			ScenarioRuleClient.ToggleMessageProcessing(process: false);
			ObjectPool.RecyclePendingObjects();
			if (AutoTestController.s_Instance != null && AutoTestController.s_Instance.CurrentOverlay != null)
			{
				UnityEngine.Object.Destroy(AutoTestController.s_Instance.CurrentOverlay.gameObject);
			}
			GameLoadedAndClientReady = false;
			GameLoadedAndHostReady = false;
			ControllableRegistry.Reset();
			FFSNetwork.Shutdown(FFSNetwork.IsHost ? new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession) : null);
			YML.Unload(regenCards: false);
			StartCoroutine(LoadEmptySceneCor());
		}
		catch (Exception ex)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("An exception occurred while loading main menu\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00006", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
		}
	}

	private IEnumerator LoadMainMenuCoroutine(Action unloadPreiousScene = null, Action onFinish = null)
	{
		if (_loadingSceneType != ESceneType.MainMenu)
		{
			yield return new WaitWhile(() => FFSNetwork.IsShuttingDown);
			yield return LoadSceneCoroutine(ESceneType.MainMenu, null, unloadPreiousScene);
			AdventureState.End();
			AssetBundleManager.Instance.UnloadNotRequiredBundles();
			yield return null;
			while (!MainMenuUIManager.Instance.Initialised)
			{
				yield return null;
			}
			if (PlatformLayer.UserData.IsSignedIn)
			{
				yield return RefreshEntitlements();
			}
			DisableLoadingScreen();
			onFinish?.Invoke();
		}
	}

	private IEnumerator WaitForRestorationComplete()
	{
		LogUtils.Log("[SceneController] Waiting for data restoration...");
		while (DataRestoring)
		{
			yield return null;
		}
		LogUtils.Log("[SceneController] Data restored");
		SimpleLog.AddToSimpleLog("User's data restored");
		yield return new WaitWhile(() => FFSNetwork.IsShuttingDown);
		AdventureState.End();
		AssetBundleManager.Instance.UnloadNotRequiredBundles();
		SaveData.Instance.LoadRootData();
		if (_isStarted)
		{
			yield return SaveData.Instance.LoadGlobalData();
			LoadMainMenu();
		}
	}

	private IEnumerator LoadEmptySceneCor()
	{
		yield return new WaitWhile(() => FFSNetwork.IsShuttingDown);
		yield return LoadSceneCoroutine(ESceneType.Empty);
		AdventureState.End();
		AssetBundleManager.Instance.UnloadNotRequiredBundles();
		DisableLoadingScreen();
		Resources.UnloadUnusedAssets();
	}

	public void OnFinishedLoadToFrontEndTutorialMenu()
	{
		if (MainMenuUIManager.Instance != null)
		{
			UIMenuOption tutorialUIMenuOption = MainMenuUIManager.Instance.TutorialUIMenuOption;
			if (InputManager.GamePadInUse)
			{
				Singleton<UINavigation>.Instance.NavigationManager.TrySelect(tutorialUIMenuOption.Selectable);
			}
			tutorialUIMenuOption.Select();
		}
	}

	public void OnStartScenarioCallback(AWLaunchMethod launchMethod, bool restart = false)
	{
		try
		{
			try
			{
				SimpleLog.WriteSimpleLogToFile();
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to cleanup Simple Log files while starting scenario.\n" + ex.Message + "\n" + ex.StackTrace);
			}
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				Instance.LoadScenarioAssetBundles(null, AdventureState.MapState);
				foreach (CMapCharacter selectedCharacter in AdventureState.MapState.MapParty.SelectedCharacters)
				{
					SaveDataShared.ApplyEnhancementIcons(selectedCharacter.Enhancements, selectedCharacter.CharacterID);
				}
				SaveData.Instance.SaveCurrentAdventureData(SaveData.Instance.SaveGlobalData);
			}
			else
			{
				if (SaveData.Instance.Global.GameMode != EGameMode.Guildmaster)
				{
					OnCustomLevelLoadedCallback(SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario);
					return;
				}
				Instance.LoadScenarioAssetBundles(null, AdventureState.MapState);
				foreach (CMapCharacter selectedCharacter2 in AdventureState.MapState.MapParty.SelectedCharacters)
				{
					SaveDataShared.ApplyEnhancementIcons(selectedCharacter2.Enhancements, selectedCharacter2.CharacterID);
				}
				SaveData.Instance.SaveCurrentAdventureData(SaveData.Instance.SaveGlobalData);
			}
			SimpleLog.AddToSimpleLog((launchMethod == AWLaunchMethod.resume) ? "User resumed scenario" : "User started scenario");
			SimpleLog.WriteSimpleLogToFile();
			Choreographer.s_Choreographer.Play(restart);
		}
		catch (Exception ex2)
		{
			Debug.LogError("An exception occurred while starting scenario\n" + ex2.Message + "\n" + ex2.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, LoadMainMenu, ex2.Message);
		}
	}

	public void RestartScenario()
	{
		Choreographer.s_Choreographer.IsRestarting = true;
		StartCoroutine(RestartScenarioCoroutine(fullRestart: false));
	}

	public void RestartScenarioFromInitial()
	{
		Choreographer.s_Choreographer.IsRestarting = true;
		StartCoroutine(RestartScenarioCoroutine(fullRestart: true));
	}

	public void RegenerateAndRestartScenario(Action onFinish = null)
	{
		Choreographer.s_Choreographer.IsRestarting = true;
		StartCoroutine(RegenerateAndRestartScenarioCoroutine(onFinish));
	}

	private IEnumerator RestartScenarioCoroutine(bool fullRestart)
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
		ShowLoadingScreen();
		Singleton<StoryController>.Instance.Clear();
		if (Singleton<FullCardHandViewer>.Instance.IsActive)
		{
			Singleton<FullCardHandViewer>.Instance.Hide();
		}
		yield return EndScenarioSafely();
		ActionProcessor.OnRestartRound();
		ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.StartOfRound);
		Choreographer.s_Choreographer.IsRestarting = false;
		if (FFSNetwork.IsOnline)
		{
			PlayerRegistry.WaitForOtherPlayersFullLoaded = true;
		}
		StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
		{
			OnStartScenarioCallback(AWLaunchMethod.resume, fullRestart);
		}));
	}

	private IEnumerator RegenerateAndRestartScenarioCoroutine(Action onFinish = null)
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
		ShowLoadingScreen();
		yield return null;
		if (FFSNetwork.IsOnline)
		{
			ControllableRegistry.AllControllables.ForEach(delegate(NetworkControllable x)
			{
				x.ResetState();
			});
		}
		Singleton<StoryController>.Instance.Clear();
		yield return EndScenarioSafely();
		Choreographer.s_Choreographer.IsRestarting = false;
		AdventureState.MapState.CurrentMapScenarioState.RegenerateMapScenario(AdventureState.MapState.HighestUnlockedChapter);
		AdventureState.MapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
		AdventureState.MapState.EnterScenario();
		StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
		{
			onFinish?.Invoke();
			OnStartScenarioCallback(AWLaunchMethod.resume, restart: true);
		}));
	}

	public IEnumerator EndScenarioSafely()
	{
		WaitForSeconds waitForSecondsDelay = new WaitForSeconds(0.5f);
		StoryController storyController = Singleton<StoryController>.Instance;
		if (storyController != null && storyController.IsVisible)
		{
			while (storyController.IsVisible)
			{
				yield return waitForSecondsDelay;
			}
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.End);
		SimpleLog.AddToSimpleLog("Entered EndScenarioSafely coroutine");
		if (Choreographer.s_Choreographer == null)
		{
			SimpleLog.AddToSimpleLog("Exited EndScenarioSafely coroutine early as Choreographer instance was null");
			yield break;
		}
		if (GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse)
		{
			GameState.s_ThreadAboutToAbort = true;
		}
		yield return waitForSecondsDelay;
		if (ScenarioRuleClient.IsMessageProcessingBlocked)
		{
			ScenarioRuleClient.ToggleMessageProcessing(process: true);
		}
		Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
		SimpleLog.AddToSimpleLog("EndScenarioSafely coroutine about to pause until pathfinder is not locked + choreographer and SRL are done processing messages");
		float timeSinceLastDebugLog = 0f;
		while (CPathFinder.Locked || Choreographer.s_Choreographer.m_MessageQueue.Count > 0 || ScenarioRuleClient.IsProcessingOrMessagesQueued || GameState.s_ThreadAboutToAbort || DeathDissolve.s_DeathDissolvesInProgress.Count > 0 || (FFSNetwork.IsOnline && ActionProcessor.IsProcessing))
		{
			timeSinceLastDebugLog += 0.5f;
			if (timeSinceLastDebugLog >= 3f)
			{
				timeSinceLastDebugLog = 0f;
				SimpleLog.AddToSimpleLog("EndScenarioSafely coroutine softlock reporting: \nPathfinder is locked: " + CPathFinder.Locked + "\nChoreographer Message Queue Count: " + Choreographer.s_Choreographer.m_MessageQueue.Count + "\nScenarioRuleClient Is Processing or Messages Queued: " + ScenarioRuleClient.IsProcessingOrMessagesQueued + "\nGameState Thread About to Abort: " + GameState.s_ThreadAboutToAbort + "\nDeath Dissolves in Progress Count: " + DeathDissolve.s_DeathDissolvesInProgress.Count);
			}
			if (AutoTestController.s_AutoTestCurrentlyLoaded)
			{
				AutoTestController.SetChoreographerPauseFlag(shouldPause: false);
			}
			yield return waitForSecondsDelay;
		}
		SimpleLog.AddToSimpleLog("EndScenarioSafely coroutine about to pause until player is done selecting damage response if needed");
		while (GameState.ThreadIsSleeping || GameState.WaitingForPlayerToSelectDamageResponse || GameState.WaitingForPlayerActorToAvoidDamageResponse)
		{
			GameState.s_ThreadAboutToAbort = true;
			yield return null;
		}
		ScenarioRuleClient.Stop();
		SimpleLog.AddToSimpleLog("Exited EndScenarioSafely coroutine");
	}

	public void NewCampaignStart(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, DLCRegistry.EDLCKey dlcEnabled, EGoldMode goldMode = EGoldMode.PartyGold, EEnhancementMode enhancementMode = EEnhancementMode.CharacterPersistent)
	{
		StartCoroutine(NewCampaignStartCoroutine(partyName, difficulty, houseRulesSetting, goldMode, dlcEnabled, enhancementMode));
	}

	private IEnumerator NewCampaignStartCoroutine(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, EGoldMode goldMode, DLCRegistry.EDLCKey dlcEnabled, EEnhancementMode enhancementMode)
	{
		ShowLoadingScreen();
		yield return null;
		yield return LoadRequiredDLCs(dlcEnabled);
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				Instance.YML.LoadCampaign(dlcEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				Debug.LogError("Unable to load Campaign YML");
				Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, Instance.LoadMainMenu);
				yield break;
			}
		}
		PersistentData.s_Instance.FailedLoading = false;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			MapRuleLibraryClient.Instance.SetMessageHandler(MapChoreographer.MessageHandler);
			SaveData.Instance.StartNewCampaignMode(partyName, difficulty, houseRulesSetting, goldMode, dlcEnabled, enhancementMode);
			AdventureState.StartAdventure(SaveData.Instance.Global.CampaignData.AdventureMapState, skipTutorial: true, skipIntro: true);
			LoadAdventureAssetBundles();
			if (PlatformLayer.IsSupportActivities)
			{
				SaveData.Instance.Global.CampaignData.SetupActivitiesForNewCampaign();
			}
			SaveData.Instance.Global.CampaignData.Save();
			SaveData.Instance.Global.CampaignData.EnqueueSaveCheckpoint();
			if (SaveData.Instance.Global.CampaignData.AdventureMapState.IsInScenarioPhase)
			{
				CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState;
				if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
				{
					SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
					SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
					SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
				}
				if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
				{
					AdventureState.MapState.EnterScenario();
				}
				StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
				{
					OnCampaignScenarioLoaded(AWLaunchMethod.create);
				}));
			}
			else
			{
				StartCoroutine(LoadSceneCoroutine(ESceneType.CampaignMap, OnCampaignMapLoaded));
			}
			AnalyticsWrapper.LogRunStart(AWGameMode.campaign, AWLaunchMethod.create, SaveData.Instance.Global.CampaignData.AdventureMapState, PlayerRegistry.AllPlayers.Count > 1);
			SimpleLog.AddToSimpleLog("User started new Campaign adventure");
		}
		catch (Exception ex)
		{
			PartyAdventureData partyAdventureData = SaveData.Instance.Global.AllCampaigns.SingleOrDefault((PartyAdventureData x) => x.PartyName == partyName);
			if (partyAdventureData != null)
			{
				SaveData.Instance.Global.AllCampaigns.Remove(partyAdventureData);
				SaveData.Instance.Global.ResumeCampaignName = string.Empty;
				SaveData.Instance.SaveGlobalData();
			}
			Debug.LogError("An exception occurred while starting the campaign map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void CampaignResume(PartyAdventureData partyData, bool isJoiningMPClient, bool isFromMPHost = false, Action onUnloadPreviousScene = null, bool refreshMapState = false)
	{
		StartCoroutine(CampaignResumeCoroutine(partyData, isJoiningMPClient, isFromMPHost, onUnloadPreviousScene, refreshMapState));
	}

	private IEnumerator CampaignResumeCoroutine(PartyAdventureData partyData, bool isJoiningMPClient, bool isFromMPHost, Action onUnloadPreviousScene, bool refreshMapState)
	{
		ShowLoadingScreen();
		yield return null;
		AssetBundleManager.Instance.UnloadNotRequiredBundles();
		yield return null;
		yield return LoadRequiredDLCs(partyData.DLCEnabled);
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				Instance.YML.LoadCampaign(partyData.AdventureMapState.DLCEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				Debug.LogError("Unable to load Campaign YML");
				Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, Instance.LoadMainMenu);
				yield break;
			}
		}
		if (isFromMPHost || isJoiningMPClient)
		{
			partyData.AdventureMapState.ResetAchievementsProgress();
		}
		PersistentData.s_Instance.FailedLoading = false;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			AWLaunchMethod launchMethod = AWLaunchMethod.resume;
			if (SaveData.Instance.Global.CampaignData != null)
			{
				LoadAdventureAssetBundles();
				if (refreshMapState)
				{
					SaveData.Instance.Global.CampaignData.RefreshMapStateFromFile(SaveData.Instance.Global.CampaignData.PartyMainSaveFile);
				}
				AdventureState.StartAdventure(SaveData.Instance.Global.CampaignData.AdventureMapState);
				if (PlatformLayer.IsSupportActivities)
				{
					SaveData.Instance.Global.CampaignData.SetupActivitiesForResumedCampaign();
				}
				if (SaveData.Instance.Global.CampaignData.AdventureMapState.IsInScenarioPhase)
				{
					bool flag = false;
					if (SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState != null)
					{
						SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
						CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState;
						if (currentMapScenarioState.MatchSessionID == null && (!FFSNetwork.IsOnline || FFSNetwork.IsHost))
						{
							AdventureState.MapState.EnterScenario();
						}
						if (SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState.CurrentState != null)
						{
							if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
							{
								SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
								SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
							}
							if (FFSNetwork.IsOnline && !FFSNetwork.IsHost)
							{
								launchMethod = AWLaunchMethod.join;
							}
							StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
							{
								OnCampaignScenarioLoaded(launchMethod);
							}, delegate
							{
								onUnloadPreviousScene?.Invoke();
								PersistentData.s_Instance.ClearCardPools();
							}));
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						Debug.LogError("Unable to resume Adventure game. CurrentMapScenarioState is null");
						GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00022", "GUI_ERROR_RETURN_TO_MAP_BUTTON", Environment.StackTrace, delegate
						{
							AdventureState.MapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InHQ));
							AdventureState.MapState.RegenerateAllMapScenarios(excludeInProgressQuest: false, rerollQuestRewards: false);
							SaveData.Instance.Global.CampaignData.EnqueueSaveCheckpoint();
							if (isJoiningMPClient)
							{
								StartCoroutine(LoadSceneCoroutine(ESceneType.CampaignMap, OnCampaignMapJoined, onUnloadPreviousScene));
							}
							else
							{
								StartCoroutine(LoadSceneCoroutine(ESceneType.CampaignMap, OnCampaignMapLoaded, onUnloadPreviousScene));
							}
						});
					}
				}
				else if (isJoiningMPClient)
				{
					StartCoroutine(LoadSceneCoroutine(ESceneType.CampaignMap, OnCampaignMapJoined, onUnloadPreviousScene));
				}
				else
				{
					StartCoroutine(LoadSceneCoroutine(ESceneType.CampaignMap, OnCampaignMapLoaded, onUnloadPreviousScene));
				}
			}
			else
			{
				Debug.LogError("Unable to resume Campaign game.  Save data is null.");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, delegate
				{
					SaveData.Instance.Global.CampaignData.MoveMapStateFileToCorruptedSaveFolder(EGameMode.Campaign);
					LoadMainMenu();
				});
			}
			if (SaveData.Instance.Global.m_StatsDataStorage.m_RunSessionID == null)
			{
				if (isJoiningMPClient)
				{
					launchMethod = AWLaunchMethod.join;
				}
				AnalyticsWrapper.LogRunStart(AWGameMode.campaign, launchMethod, SaveData.Instance.Global.CampaignData.AdventureMapState, PlayerRegistry.AllPlayers.Count > 1);
				SimpleLog.AddToSimpleLog((launchMethod == AWLaunchMethod.join) ? "User joined a Campaign adventure" : "User resumed a Campaign adventure");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to resume Adventure game\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, delegate
			{
				SaveData.Instance.Global.CampaignData.MoveMapStateFileToCorruptedSaveFolder(EGameMode.Campaign);
				LoadMainMenu();
			}, ex.Message);
		}
	}

	private void OnCampaignMapJoined()
	{
		try
		{
			StartCoroutine(OnCampaignMapLoadedCoroutine(isMPClientJoining: true));
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Campaign Map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private void OnCampaignMapLoaded()
	{
		try
		{
			StartCoroutine(OnCampaignMapLoadedCoroutine(isMPClientJoining: false));
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Campaign Map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private IEnumerator OnCampaignMapLoadedCoroutine(bool isMPClientJoining)
	{
		Debug.Log("OnCampaignMapLoadedCoroutine");
		while (AssetBundleManager.Instance.ScenarioBundlesLoading)
		{
			yield return null;
		}
		while (MapRuleLibraryClient.Instance.MessageHandler == null || MapRuleLibraryClient.Instance.MessageQueueLength > 0 || AdventureState.MapState.AdventureStateStarting)
		{
			yield return null;
		}
		AdventureState.MapState.OnMapLoaded(isMPClientJoining);
		while (MapRuleLibraryClient.Instance.MessageQueueLength > 0)
		{
			yield return null;
		}
		yield return Singleton<MapChoreographer>.Instance.InitMap();
		while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
		{
			yield return null;
		}
		SaveData.Instance.SaveGlobalData();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}

	private void OnCampaignScenarioLoaded(AWLaunchMethod launchMethod)
	{
		try
		{
			StartCoroutine(OnCampaignScenarioLoadedCoroutine(launchMethod));
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Campaign Scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private IEnumerator OnCampaignScenarioLoadedCoroutine(AWLaunchMethod launchMethod)
	{
		Debug.Log("OnCampaignScenarioLoadedCoroutine");
		while (MapRuleLibraryClient.Instance.MessageQueueLength > 0 || AdventureState.MapState.AdventureStateStarting)
		{
			yield return null;
		}
		OnStartScenarioCallback(launchMethod);
	}

	public void CampaignRestart()
	{
		try
		{
			SaveData.Instance.Global.CampaignData.RetryScenario();
			if (SaveData.Instance.Global.CampaignData != null && SaveData.Instance.Global.CampaignData.AdventureMapState.IsInScenarioPhase)
			{
				CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState;
				if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
				{
					SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
					SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
				}
				StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
				{
					OnStartScenarioCallback(AWLaunchMethod.create, restart: true);
				}));
			}
			else
			{
				Debug.LogError("Unable to restart Adventure game scenario.  Save data is null.");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00021", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to restart Adventure game scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void CampaignScenarioStart()
	{
		try
		{
			CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.CampaignData.AdventureMapState.CurrentMapScenarioState;
			if (currentMapScenarioState.LoadInitialState && currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnStartScenarioCallback((FFSNetwork.IsOnline && !FFSNetwork.IsHost) ? AWLaunchMethod.join : AWLaunchMethod.create);
			}));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting campaign scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void GuildmasterStart(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, DLCRegistry.EDLCKey dlcEnabled, EGoldMode goldMode = EGoldMode.PartyGold, bool skipTutorial = false, bool skipIntro = false)
	{
		StartCoroutine(GuildmasterStartCoroutine(partyName, difficulty, houseRulesSetting, goldMode, dlcEnabled, skipTutorial, skipIntro));
	}

	private IEnumerator GuildmasterStartCoroutine(string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSetting, EGoldMode goldMode, DLCRegistry.EDLCKey dlcEnabled, bool skipTutorial, bool skipIntro)
	{
		ShowLoadingScreen();
		yield return null;
		yield return LoadRequiredDLCs(dlcEnabled);
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				Instance.YML.LoadGuildMaster(dlcEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Debug.LogError("Unable to load Guildmaster YML");
				Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, Instance.LoadMainMenu);
				yield break;
			}
		}
		PersistentData.s_Instance.FailedLoading = false;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			SaveData.Instance.StartNewAdventureMode(partyName, difficulty, houseRulesSetting, goldMode, dlcEnabled);
			AdventureState.StartAdventure(SaveData.Instance.Global.AdventureData.AdventureMapState, skipTutorial, skipIntro);
			LoadAdventureAssetBundles();
			if (PlatformLayer.IsSupportActivities)
			{
				SaveData.Instance.Global.AdventureData.SetupActivitiesForNewGuildmaster(skipIntro);
			}
			SaveData.Instance.Global.AdventureData.Save();
			SaveData.Instance.Global.AdventureData.EnqueueSaveCheckpoint();
			if (SaveData.Instance.Global.AdventureData.AdventureMapState.IsInScenarioPhase)
			{
				CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState;
				if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
				{
					SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
					SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
					SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
				}
				if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
				{
					AdventureState.MapState.EnterScenario();
				}
				StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
				{
					OnNewAdventureScenarioLoaded(AWLaunchMethod.create);
				}));
			}
			else
			{
				StartCoroutine(LoadSceneCoroutine(ESceneType.NewAdventureMap, OnNewAdventureMapLoaded));
			}
			AnalyticsWrapper.LogRunStart(AWGameMode.new_adventure_mode, AWLaunchMethod.create, SaveData.Instance.Global.AdventureData.AdventureMapState, PlayerRegistry.AllPlayers.Count > 1);
			SimpleLog.AddToSimpleLog("User started new Guildmaster adventure");
		}
		catch (Exception ex)
		{
			PartyAdventureData partyAdventureData = SaveData.Instance.Global.AllAdventures.SingleOrDefault((PartyAdventureData x) => x.PartyName == partyName);
			if (partyAdventureData != null)
			{
				SaveData.Instance.Global.RemoveAdventureSave(partyAdventureData);
				SaveData.Instance.Global.ResumeAdventureName = string.Empty;
				SaveData.Instance.SaveGlobalData();
			}
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("An exception occurred while starting the adventure map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void GuildmasterResume(PartyAdventureData partyData, bool isJoiningMPClient, bool regenerateMonsterCards, bool isFromMPHost = false, Action onUnloadPreviousScene = null, bool refreshMapState = false)
	{
		StartCoroutine(GuildmasterResumeCoroutine(partyData, isJoiningMPClient, regenerateMonsterCards, isFromMPHost, onUnloadPreviousScene, refreshMapState));
	}

	private IEnumerator GuildmasterResumeCoroutine(PartyAdventureData partyData, bool isJoiningMPClient, bool regenerateMonsterCards, bool isFromMpHost, Action onUnloadPreviousScene, bool refreshMapState)
	{
		ShowLoadingScreen();
		yield return null;
		AssetBundleManager.Instance.UnloadNotRequiredBundles();
		yield return null;
		yield return LoadRequiredDLCs(partyData.DLCEnabled);
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				Instance.YML.LoadGuildMaster(partyData.AdventureMapState.DLCEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Debug.LogError("Unable to load Guildmaster YML");
				Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, Instance.LoadMainMenu);
				yield break;
			}
		}
		if (isFromMpHost || isJoiningMPClient)
		{
			partyData.AdventureMapState.ResetAchievementsProgress();
		}
		PersistentData.s_Instance.FailedLoading = false;
		if (regenerateMonsterCards)
		{
			yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		}
		if (PersistentData.s_Instance.FailedLoading)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			AWLaunchMethod launchMethod = AWLaunchMethod.resume;
			if (SaveData.Instance.Global.AdventureData != null)
			{
				LoadAdventureAssetBundles();
				if (refreshMapState)
				{
					SaveData.Instance.Global.AdventureData.RefreshMapStateFromFile(SaveData.Instance.Global.AdventureData.PartyMainSaveFile);
				}
				AdventureState.StartAdventure(SaveData.Instance.Global.AdventureData.AdventureMapState);
				if (PlatformLayer.IsSupportActivities)
				{
					SaveData.Instance.Global.AdventureData.SetupActivitiesForResumedGuildmaster();
				}
				if (SaveData.Instance.Global.AdventureData.AdventureMapState.IsInScenarioPhase)
				{
					bool flag = false;
					if (SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState != null)
					{
						SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
						CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState;
						if (currentMapScenarioState.MatchSessionID == null && (!FFSNetwork.IsOnline || FFSNetwork.IsHost))
						{
							AdventureState.MapState.EnterScenario();
						}
						if (SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState.CurrentState != null)
						{
							if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
							{
								SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
								SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
							}
							if (FFSNetwork.IsOnline && !FFSNetwork.IsHost)
							{
								launchMethod = AWLaunchMethod.join;
							}
							StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
							{
								OnNewAdventureScenarioLoaded(launchMethod);
							}, delegate
							{
								onUnloadPreviousScene?.Invoke();
								PersistentData.s_Instance.ClearCardPools();
							}));
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						InputManager.RequestEnableInput(this, EKeyActionTag.All);
						Debug.LogError("Unable to resume Adventure game. CurrentMapScenarioState is null");
						GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00022", "GUI_ERROR_RETURN_TO_MAP_BUTTON", Environment.StackTrace, delegate
						{
							AdventureState.MapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InHQ));
							AdventureState.MapState.RegenerateAllMapScenarios(excludeInProgressQuest: false, rerollQuestRewards: false);
							SaveData.Instance.Global.AdventureData.EnqueueSaveCheckpoint();
							if (isJoiningMPClient)
							{
								StartCoroutine(LoadSceneCoroutine(ESceneType.NewAdventureMap, OnNewAdventureMapJoined, onUnloadPreviousScene));
							}
							else
							{
								StartCoroutine(LoadSceneCoroutine(ESceneType.NewAdventureMap, OnNewAdventureMapLoaded, onUnloadPreviousScene));
							}
						});
					}
				}
				else if (isJoiningMPClient)
				{
					StartCoroutine(LoadSceneCoroutine(ESceneType.NewAdventureMap, OnNewAdventureMapJoined, onUnloadPreviousScene));
				}
				else
				{
					StartCoroutine(LoadSceneCoroutine(ESceneType.NewAdventureMap, OnNewAdventureMapLoaded, onUnloadPreviousScene));
				}
				if (SaveData.Instance.Global.m_StatsDataStorage.m_RunSessionID == null)
				{
					if (isJoiningMPClient)
					{
						launchMethod = AWLaunchMethod.join;
					}
					AnalyticsWrapper.LogRunStart(AWGameMode.new_adventure_mode, launchMethod, SaveData.Instance.Global.AdventureData.AdventureMapState, PlayerRegistry.AllPlayers.Count > 1);
					SimpleLog.AddToSimpleLog((launchMethod == AWLaunchMethod.join) ? "User joined a Guildmaster adventure" : "User resumed a Guildmaster adventure");
				}
			}
			else
			{
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Debug.LogError("Unable to resume Adventure game.  Save data is null.");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, delegate
				{
					SaveData.Instance.Global.AdventureData.MoveMapStateFileToCorruptedSaveFolder(EGameMode.Guildmaster);
					LoadMainMenu();
				});
			}
		}
		catch (Exception ex)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to resume Adventure game\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, delegate
			{
				SaveData.Instance.Global.AdventureData.MoveMapStateFileToCorruptedSaveFolder(EGameMode.Guildmaster);
				LoadMainMenu();
			}, ex.Message);
		}
	}

	public void NewAdventureRestart()
	{
		try
		{
			SaveData.Instance.Global.AdventureData.RetryScenario();
			if (SaveData.Instance.Global.AdventureData != null && SaveData.Instance.Global.AdventureData.AdventureMapState.IsInScenarioPhase)
			{
				CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState;
				if (currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
				{
					SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
					SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
				}
				StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
				{
					OnStartScenarioCallback(AWLaunchMethod.create, restart: true);
				}));
			}
			else
			{
				Debug.LogError("Unable to restart Adventure game scenario.  Save data is null.");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00021", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to restart Adventure game scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private void OnNewAdventureMapLoaded()
	{
		try
		{
			StartCoroutine(OnNewAdventureMapLoadedCoroutine(isMPClientJoining: false));
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Adventure Map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private void OnNewAdventureMapJoined()
	{
		try
		{
			StartCoroutine(OnNewAdventureMapLoadedCoroutine(isMPClientJoining: true));
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Adventure Map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private IEnumerator OnNewAdventureMapLoadedCoroutine(bool isMPClientJoining)
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.PERSISTENT_SUBMIT);
		while (AssetBundleManager.Instance.ScenarioBundlesLoading)
		{
			yield return null;
		}
		while (MapRuleLibraryClient.Instance.MessageHandler == null || MapRuleLibraryClient.Instance.MessageQueueLength > 0 || AdventureState.MapState.AdventureStateStarting)
		{
			yield return null;
		}
		AdventureState.MapState.OnMapLoaded(isMPClientJoining);
		while (MapRuleLibraryClient.Instance.MessageQueueLength > 0)
		{
			yield return null;
		}
		yield return Singleton<MapChoreographer>.Instance.InitMap();
		while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
		{
			yield return null;
		}
		SaveData.Instance.SaveGlobalData();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}

	private void OnNewAdventureScenarioLoaded(AWLaunchMethod launchMethod)
	{
		try
		{
			StartCoroutine(OnNewAdventureScenarioLoadedCoroutine(launchMethod));
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Guildmaster Scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private IEnumerator OnNewAdventureScenarioLoadedCoroutine(AWLaunchMethod launchMethod)
	{
		Debug.Log("OnGuildmasterScenarioLoadedCoroutine");
		while (MapRuleLibraryClient.Instance.MessageQueueLength > 0 || AdventureState.MapState.AdventureStateStarting)
		{
			yield return null;
		}
		OnStartScenarioCallback(launchMethod);
	}

	public void NewAdventureScenarioStart()
	{
		try
		{
			CMapScenarioState currentMapScenarioState = SaveData.Instance.Global.AdventureData.AdventureMapState.CurrentMapScenarioState;
			if (currentMapScenarioState.LoadInitialState && currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				SaveData.Instance.Global.CurrentCustomLevelData = currentMapScenarioState.CustomLevelData;
				SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			}
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnStartScenarioCallback((FFSNetwork.IsOnline && !FFSNetwork.IsHost) ? AWLaunchMethod.join : AWLaunchMethod.create);
			}));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting adventure scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void ResumeSingleScenario()
	{
		StartCoroutine(ResumeSingleScenarioCoroutine());
	}

	private IEnumerator ResumeSingleScenarioCoroutine()
	{
		ShowLoadingScreen();
		yield return null;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		if (SaveData.Instance.Global.ResumeSingleScenario != null)
		{
			SaveData.Instance.Global.ResumeSingleScenario.Load(EGameMode.SingleScenario, isJoiningMPClient: false);
		}
		if (SaveData.Instance.Global.ResumeSingleScenario?.AdventureMapState.CurrentSingleScenario?.ScenarioState != null)
		{
			SaveData.Instance.Global.CurrentCustomLevelData = SaveData.Instance.Global.ResumeSingleScenario.AdventureMapState.CurrentSingleScenario;
			SaveData.Instance.Global.CurrentlyPlayingCustomLevel = true;
			AdventureState.UpdateMapState(SaveData.Instance.Global.ResumeSingleScenario.AdventureMapState);
			SaveData.Instance.StartCustomLevel();
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnCustomLevelLoadedCallback(SaveData.Instance.Global.ResumeSingleScenario.AdventureMapState.CurrentSingleScenario);
			}));
		}
		else
		{
			Debug.LogError("Unable to find resume state for scenario\n");
		}
	}

	public void LoadSingleScenario(PartyAdventureData partyData)
	{
		StartCoroutine(LoadSingleScenarioCoroutine(partyData));
	}

	private IEnumerator LoadSingleScenarioCoroutine(PartyAdventureData partyData)
	{
		ShowLoadingScreen();
		yield return null;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			SaveData.Instance.StartCustomLevel();
			if (partyData.AdventureMapState.CurrentSingleScenario.ScenarioState == null)
			{
				if (partyData.AdventureMapState.CurrentSingleScenario.YMLFile == null || partyData.AdventureMapState.CurrentSingleScenario.YMLFile.Length <= 0)
				{
					throw new Exception("Custom Level Scenario State is null");
				}
				ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.GetScenarioDefinition(partyData.AdventureMapState.CurrentSingleScenario.ScenarioState.ID);
				if (scenarioDefinition == null)
				{
					throw new Exception("Unable to find scenario with filename " + partyData.AdventureMapState.CurrentSingleScenario.YMLFile);
				}
				partyData.AdventureMapState.CurrentSingleScenario.ScenarioState = CMapScenarioState.CreateNewScenario(scenarioDefinition, SharedClient.GlobalRNG.Next(), partyData.AdventureMapState.MapParty.PartyLevel, scenarioDefinition.FileName, GLOOM.LocalizationManager.GetTranslation(scenarioDefinition.ID), (scenarioDefinition.Description != string.Empty) ? GLOOM.LocalizationManager.GetTranslation(scenarioDefinition.Description) : string.Empty, partyData.AdventureMapState.MapParty.SelectedCharacters.Count(), new SharedLibrary.Random(), out var _, out var _, partyData.AdventureMapState.MapParty.ThreatLevel);
			}
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnCustomLevelLoadedCallback(partyData.AdventureMapState.CurrentSingleScenario);
			}));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while loading a custom map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void LoadCustomLevel(CCustomLevelData customLevel, bool isFrontEndTutorial = false, Action onUnloadPreviousSceneCallback = null)
	{
		StartCoroutine(LoadCustomLevelCoroutine(customLevel, isFrontEndTutorial, onUnloadPreviousSceneCallback));
	}

	private IEnumerator LoadCustomLevelCoroutine(CCustomLevelData customLevel, bool isFrontEndTutorial = false, Action onUnloadPreviousSceneCallback = null)
	{
		EGameMode gameMode = (isFrontEndTutorial ? EGameMode.FrontEndTutorial : EGameMode.SingleScenario);
		ShowLoadingScreen();
		yield return null;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			PartyAdventureData partyData = new PartyAdventureData(SharedClient.GlobalRNG.Next(), "Custom Party", EAdventureDifficulty.Normal, StateShared.EHouseRulesFlag.None, PlatformLayer.UserData.IsSignedIn ? new SaveOwner() : new SaveOwner(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty), EGoldMode.PartyGold, DLCRegistry.EDLCKey.None, EEnhancementMode.CharacterPersistent, gameMode, SaveData.Instance.Global?.CurrentModdedRuleset);
			partyData.AdventureMapState.CurrentSingleScenario = customLevel;
			PartyAdventureData partyAdventureData = SaveData.Instance.Global.AllSingleScenarios.SingleOrDefault((PartyAdventureData s) => s.PartyName == "Custom Party" && ((!s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == partyData.Owner.PlatformNetworkAccountID) || ((Application.platform != RuntimePlatform.Switch || s.Owner.PlatformNetworkAccountID.Equals("0")) && s.Owner.PlatformAccountID == partyData.Owner.PlatformAccountID)));
			if (partyAdventureData != null)
			{
				SaveData.Instance.Global.AllSingleScenarios.Remove(partyAdventureData);
			}
			SaveData.Instance.Global.AllSingleScenarios.Add(partyData);
			SaveData.Instance.Global.ResumeSingleScenarioName = "Custom Party";
			SaveData.Instance.Global.CurrentHostAccountID = partyData.Owner.PlatformAccountID;
			SaveData.Instance.Global.CurrentHostNetworkAccountID = partyData.Owner.PlatformNetworkAccountID;
			if (isFrontEndTutorial)
			{
				SaveData.Instance.StartFrontEndTutorialLevel();
			}
			else
			{
				SaveData.Instance.StartCustomLevel();
			}
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnCustomLevelLoadedCallback(customLevel);
			}, delegate
			{
				onUnloadPreviousSceneCallback?.Invoke();
				PersistentData.s_Instance.ClearCardPools();
			}));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while loading a custom map\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void OnCustomLevelLoadedCallback(CCustomLevelData customLevel)
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		try
		{
			if (customLevel.ScenarioState != null && customLevel.ScenarioState.Players.Count > 0)
			{
				Instance.LoadScenarioAssetBundles(customLevel.ScenarioState);
				UnityGameEditorRuntime.LoadScenario(customLevel.ScenarioState);
				SimpleLog.WriteSimpleLogToFile();
			}
			else
			{
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00018", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void LevelEditorStart(ScenarioRuleLibrary.ScenarioState scenarioState = null)
	{
		StartCoroutine(LevelEditorStartCoroutine(scenarioState));
	}

	private IEnumerator LevelEditorStartCoroutine(ScenarioRuleLibrary.ScenarioState scenarioState = null)
	{
		ShowLoadingScreen();
		yield return null;
		yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
		if (PersistentData.s_Instance.FailedLoading)
		{
			Debug.LogError("Unable to load Monster Card YML\n");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
			yield break;
		}
		try
		{
			SaveData.Instance.StartLevelEditor();
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnStartLevelEditorCallback(scenarioState);
			}));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting the level editor\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void OnStartLevelEditorCallback(ScenarioRuleLibrary.ScenarioState scenarioState = null)
	{
		try
		{
			if (scenarioState != null)
			{
				Instance.LoadScenarioAssetBundles();
				SaveData.Instance.Global.CurrentEditorLevelData.ScenarioState = scenarioState;
			}
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			LevelEditorController.s_Instance.StartLevelEditor();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public void AutoTestStart(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false, bool skipMonsterCardGeneration = false)
	{
		if (autoTest == null)
		{
			Debug.LogError("Cannot load null AutoTest");
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00024", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
		}
		else
		{
			StartCoroutine(AutoTestStartCoroutine(autoTest, autoTestState, withoutAutotestFunctionality, skipMonsterCardGeneration));
		}
	}

	private IEnumerator AutoTestStartCoroutine(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality, bool skipMonsterCardGeneration)
	{
		ShowLoadingScreen();
		yield return null;
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			Thread loadGuildmasterYML = new Thread((ThreadStart)delegate
			{
				YML.LoadGuildMaster(DLCRegistry.AllDLCFlag);
			}, 1048576);
			loadGuildmasterYML.Start();
			while (loadGuildmasterYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				Debug.LogError("Unable to load Guildmaster YML");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00001", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
				yield break;
			}
		}
		if (!skipMonsterCardGeneration)
		{
			yield return StartCoroutine(PersistentData.s_Instance.InitMonsterCards());
			if (PersistentData.s_Instance.FailedLoading)
			{
				Debug.LogError("Unable to load Monster Card YML\n");
				GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, LoadMainMenu);
				yield break;
			}
		}
		try
		{
			SaveData.Instance.StartAutoTest(autoTest);
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
			StartCoroutine(LoadSceneCoroutine(ESceneType.Scenario, delegate
			{
				OnStartAutoTestCallback(autoTest, autoTestState, withoutAutotestFunctionality);
			}));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting the Autotest\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	public IEnumerator BulkRunAutoTestStart(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false)
	{
		try
		{
			ShowLoadingScreen();
			SaveData.Instance.StartAutoTest(autoTest);
			UnityGameEditorRuntime.s_DisplayBasicTilesAtRuntime = false;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting the Autotest\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00024", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
		yield return LoadSceneCoroutine(ESceneType.Scenario, delegate
		{
			OnStartAutoTestCallback(autoTest, autoTestState, withoutAutotestFunctionality);
		});
	}

	public void OnStartAutoTestCallback(AutoTestData autoTest, EAutoTestControllerState autoTestState, bool withoutAutotestFunctionality = false)
	{
		try
		{
			Instance.LoadScenarioAssetBundles(autoTest.ScenarioState);
			AutoTestController.s_Instance.LoadIntoAutoTest(autoTest, autoTestState, withoutAutotestFunctionality);
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while starting scenario\n" + ex.Message + "\n" + ex.StackTrace);
			GlobalErrorMessage.ShowMessageDefaultTitle("Could not start scenario, game will now return to Main Menu", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, LoadMainMenu, ex.Message);
		}
	}

	private void ShowDesyncDetectionError(Exception ex)
	{
		List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
		list.Add(new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", delegate
		{
			if (SaveData.Instance.Global.CurrentGameState != EGameState.None)
			{
				UIManager.LoadMainMenu();
			}
			else
			{
				GlobalErrorMessage.CloseErrorMessage();
			}
		}, KeyAction.UI_SUBMIT));
		GlobalErrorMessage.ShowMultiChoiceMessage("ERROR_MULTIPLAYER_00022", list, "GUI_ERROR_TITLE", ex.StackTrace, ex.Message, showErrorReportButton: true, trackDebug: false);
	}

	private IEnumerator LoadRequiredDLCs(DLCRegistry.EDLCKey dlcEnabled)
	{
		if (!PersistentData.s_Instance.CachedDLCs.HasFlag(dlcEnabled))
		{
			yield return Instance.InitAndLoadYML();
			Instance.LoadDlcLanguageUpdate();
			yield return Instance.ReloadGlobalCardAssets(ignoreLoadingScreen: true);
		}
	}

	public IEnumerator WaitForPlayers()
	{
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.WaitingForPlayers);
		while ((FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayers && PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant) || (FFSNetwork.IsHost && PlayerRegistry.AllPlayers.Any((NetworkPlayer w) => w.PlayerID != PlayerRegistry.HostPlayerID && !w.PlayerReadyForAssignment)))
		{
			PlayerRegistry.CheckLoadingFinished();
			yield return null;
		}
		LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
	}

	public void PlayerReadyForAssignment(GameAction action)
	{
		if (!FFSNetwork.IsHost)
		{
			return;
		}
		Task.Run(async delegate
		{
			while (PlayerRegistry.IsProfanityCheckInProcess)
			{
				await Task.Delay(50);
			}
			NetworkPlayer networkPlayer = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == action.PlayerID);
			if (networkPlayer != null)
			{
				networkPlayer.PlayerReadyForAssignment = true;
				if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
				{
					UnityMainThreadDispatcher.Instance().Enqueue(delegate
					{
						Singleton<UIScenarioMultiplayerController>.Instance?.RefreshWaitingNotifications();
					});
				}
				else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
				{
					UnityMainThreadDispatcher.Instance().Enqueue(delegate
					{
						Singleton<UIMapMultiplayerController>.Instance?.RefreshWaitingNotifications();
					});
				}
			}
		});
	}

	public IEnumerator LoadMostRecentSave()
	{
		mostRecentSaveLoaded = false;
		PartyAdventureData mostRecentSave = SaveData.Instance.Global.FindMostRecentSave();
		if (mostRecentSave == null)
		{
			yield break;
		}
		switch (mostRecentSave.GameMode)
		{
		case EGameMode.Campaign:
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				YML.LoadCampaign(mostRecentSave.AdventureMapState.DLCEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				Debug.LogError("Unable to load Campaign YML");
				break;
			}
			CampaignResume(SaveData.Instance.Global.ResumeCampaign, isJoiningMPClient: false);
			mostRecentSaveLoaded = true;
			break;
		}
		case EGameMode.Guildmaster:
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				YML.LoadGuildMaster(mostRecentSave.AdventureMapState.DLCEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				Debug.LogError("Unable to load Guildmaster YML");
				break;
			}
			GuildmasterResume(SaveData.Instance.Global.ResumeAdventure, isJoiningMPClient: false, regenerateMonsterCards: true);
			mostRecentSaveLoaded = true;
			break;
		}
		case EGameMode.SingleScenario:
		case EGameMode.FrontEndTutorial:
		{
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				YML.LoadGuildMaster(mostRecentSave.AdventureMapState.DLCEnabled);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!YMLLoading.LastLoadResult)
			{
				Debug.LogError("Unable to load Guildmaster YML");
				break;
			}
			ResumeSingleScenario();
			mostRecentSaveLoaded = true;
			break;
		}
		}
	}

	public IEnumerator LoadMods()
	{
		Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.ModLoadingWithProgress);
		Instance.ShowLoadingScreen();
		float progressIncrement = 50f / (float)Modding.Mods.Count((GHMod w) => w.MetaData.ModType != GHModMetaData.EModType.Language && w.MetaData.ModType != GHModMetaData.EModType.CustomLevels);
		foreach (GHMod item in Modding.Mods.Where((GHMod w) => w.MetaData.ModType != GHModMetaData.EModType.Language && w.MetaData.ModType != GHModMetaData.EModType.CustomLevels))
		{
			yield return GHModding.ValidateMod(item, null, writeResultsToFile: false);
			Instance.LoadingScreenInstance.IncrementProgressBar(progressIncrement);
		}
		ThreadMessageReceiver threadReceiver = new ThreadMessageReceiver();
		threadReceiver.StartMessageProcessing();
		yield return null;
		Thread loadRulesets = new Thread((ThreadStart)delegate
		{
			Modding.LoadRulesets(new ThreadMessageSender(threadReceiver.QueueMessage));
		}, 1048576);
		loadRulesets.Start();
		while (loadRulesets.IsAlive || threadReceiver.IsBusy)
		{
			yield return null;
		}
		threadReceiver.StopMessageProcessing();
		Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
		Instance.DisableLoadingScreen();
		ModLoadingCompleted = true;
	}

	public void LoadAdventureAssetBundles()
	{
	}

	public void LoadScenarioAssetBundles(ScenarioRuleLibrary.ScenarioState scenarioBeingLoaded = null, CMapState mapStateBeingLoaded = null)
	{
		StartCoroutine(AssetBundleManager.Instance.LoadAllScenarioAssetsBeforeAction(scenarioBeingLoaded, mapStateBeingLoaded));
	}

	private void AdditionalInitAudioController()
	{
		SingletonMonoBehaviour<AudioController>.Instance.LoadAllAuidoClips = PlatformLayer.Setting.UsePreloadAudioClips;
	}

	private void ClearAfterSceneUnloading()
	{
		SingletonMonoBehaviour<AudioController>.Instance.SafeUnloadAllAudioClips();
		Singleton<UIConfirmationBoxManager>.Instance.ResetConfirmationBox();
		_apparanceResourceListLoader.UnloadAll();
		ClearTextMeshPro();
		if (this.UnloadSpecialMemory != null)
		{
			this.UnloadSpecialMemory();
		}
		ClearGraphicRegistry();
		Singleton<SpecialUIProvider>.Instance.Unload();
		Singleton<DebugMenuProvider>.Instance?.ClearMemory();
		Waypoint.s_MovingActor = null;
		Waypoint.s_Waypoints.Clear();
		Waypoint.s_CachedWaypoints.Clear();
		LevelEditorController.s_Instance.ClearDestroyedAllMaps();
		OutlineEffect.ClearTargets();
		UnloadErrorMessage();
	}

	private void ClearTextMeshPro()
	{
		TextMeshProUGUI[] array = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(includeInactive: true);
		foreach (TextMeshProUGUI textMeshProUGUI in array)
		{
			if (textMeshProUGUI != null && textMeshProUGUI.textInfo != null)
			{
				textMeshProUGUI.textInfo.characterInfo = Array.Empty<TMP_CharacterInfo>();
				textMeshProUGUI.textInfo.characterCount = textMeshProUGUI.textInfo.characterInfo.Length;
			}
		}
	}

	public void ClearGraphicRegistry()
	{
		IDictionary dictionary = (IDictionary)typeof(GraphicRegistry).GetField("m_Graphics", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GraphicRegistry.instance);
		List<Canvas> list = new List<Canvas>();
		foreach (DictionaryEntry item in dictionary)
		{
			Canvas canvas = (Canvas)item.Key;
			if (canvas == null || canvas.gameObject == null)
			{
				list.Add(canvas);
				continue;
			}
			RemoveElements(GraphicRegistry.GetGraphicsForCanvas(canvas));
			RemoveElements(GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas));
		}
		foreach (Canvas item2 in list)
		{
			dictionary.Remove(item2);
		}
		static void RemoveElements(IList<Graphic> graphicsForCanvas)
		{
			for (int num = graphicsForCanvas.Count - 1; num >= 0; num--)
			{
				if (graphicsForCanvas[num].IsDestroyed() || graphicsForCanvas[num].gameObject == null)
				{
					graphicsForCanvas.RemoveAt(num);
				}
			}
		}
	}
}
