#define ENABLE_LOGS
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AsmodeeNet.Utils;
using JetBrains.Annotations;
using Platforms;
using Platforms.Utils;
using PlayEveryWare.EpicOnlineServices;
using SM.Utils;
using Script.Misc;
using Script.PlatformLayer;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformLayer : MonoBehaviour, IPlatformLayer
{
	public const string STANDALONE = "Standalone";

	public const string GAME_CORE = "GameCore";

	public const string EPIC_GAMES_STORE = "EpicGamesStore";

	public const string XBOX = "Xbox";

	public const string STEAM = "Steam";

	public const string GOG_GALAXY = "GoGGalaxy";

	public const string PLAY_STATION_4 = "PlayStation4";

	public const string PLAY_STATION_5 = "PlayStation5";

	public const string NINTENDO_SWITCH = "NintendoSwitch";

	public static PlatformLayer Instance;

	[SerializeField]
	private PlatformFileSystem m_FileSystem;

	[SerializeField]
	private PlatformNetworking m_Networking;

	[SerializeField]
	private PlatformUserData m_UserData;

	[SerializeField]
	private PlatformStats m_Stats;

	[SerializeField]
	private PlatformModding m_Modding;

	[SerializeField]
	private PlatformDLC m_DLC;

	[SerializeField]
	private PlatformSetting[] m_PlatformSettings;

	[SerializeField]
	private PlatformMessage m_PlatformMessage;

	[SerializeField]
	private PlatformStreamingInstall m_StreamingInstall;

	[SerializeField]
	private ProfanityFilter m_ProfanityFilter;

	[SerializeField]
	private PlayerPlatformImageController _playerPlatformImageController;

	private GameProvider _gameProvider;

	private IPlatform _currentPlatform;

	private IPlatformBoost _platformBoost;

	private PlatformSetting _actualPlatformSetting;

	public const int MaxNumberOfDigitsAllowed = 6;

	[Header("Epic Store Specific")]
	public GameObject EOSManagerPrefab;

	private EOSManager Manager;

	private const string c_SteamNotAvailable = "Steam is not available or active on this platform !";

	private const string c_ModuleName = "PlatformLayer";

	private const uint c_SteamAppId = 780290u;

	public static bool EOSInitialised { get; protected set; }

	public static bool Initialised { get; protected set; }

	public static PlatformFileSystem FileSystem => Instance?.m_FileSystem;

	public static PlatformNetworking Networking => Instance?.m_Networking;

	public static PlatformUserData UserData => Instance?.m_UserData;

	public static PlatformStats Stats => Instance?.m_Stats;

	public static PlatformModding Modding => Instance?.m_Modding;

	public static PlatformDLC DLC => Instance?.m_DLC;

	public static GameProvider GameProvider => Instance?._gameProvider;

	public static PlatformSetting Setting => Instance?.GetPlatformSetting();

	public static PlatformMessage Message => Instance?.m_PlatformMessage;

	public static PlatformStreamingInstall StreamingInstall => Instance?.m_StreamingInstall;

	public static ProfanityFilter ProfanityFilter => Instance?.m_ProfanityFilter;

	public PlayerPlatformImageController PlayerPlatformImageController => _playerPlatformImageController;

	public static IPlatformBoost Boost => Instance._platformBoost;

	public static IPlatform Platform => Instance?._currentPlatform;

	public static bool IsSupportActivities => Platform.IsSupportActivities;

	public static bool IsSupportStreamingInstall => Platform.IsSupportStreamingInstall;

	[CanBeNull]
	public StartupLanguage StartupLanguage { get; private set; }

	public bool IsConsole { get; }

	public ControllerType DefaultControllerType { get; } = ControllerType.MouseKeyboard;

	public bool IsDelayedInit { get; }

	public string PlatformID => "Steam";

	public uint SteamAppId => SteamClient.AppId;

	public string SessionTicket => BitConverter.ToString(SteamUser.GetAuthSessionTicket().Data).Replace("-", string.Empty);

	public bool IsValid => SteamClient.IsValid;

	public void EOSInitialise()
	{
		EOSManager component = UnityEngine.Object.Instantiate(EOSManagerPrefab, base.transform).GetComponent<EOSManager>();
		if (component != null)
		{
			EOSManager.Instance.Init(component);
		}
		StartCoroutine(InitialiseOtherPlatformEndpoints());
	}

	private IEnumerator InitialiseOtherPlatformEndpoints()
	{
		UserData.StartLoginFlow();
		while (UserData.CurrentEOSAuthStatus != PlatformUserData.EPlatformAuthStatus.Authorised)
		{
			yield return null;
		}
		EOSInitialised = true;
	}

	public void InitialisePlatformLayer()
	{
		Instance = this;
		_gameProvider = new GameProvider();
		bool initHydra = true;
		bool initEntitlements = true;
		_currentPlatform = PlatformConstructor.BuildPlatform(GameProvider, initHydra, initEntitlements, initPros: false);
		InputSystem.onDeviceChange += InputSystemOnDeviceChange;
		SetDeviceSpecificQualitySettings();
		Initialize(_currentPlatform);
		m_FileSystem.Initialize(_currentPlatform);
		m_Modding.Initialize(_currentPlatform);
		m_DLC.Initialize(_currentPlatform);
		m_PlatformMessage.Initialize(_currentPlatform);
		m_StreamingInstall.Initialize(_currentPlatform);
		m_ProfanityFilter.Initialize();
		UnityEngine.Object.DontDestroyOnLoad(this);
		if (IsConsole)
		{
			_currentPlatform.OnApplicationSuspend += ApplicationSuspend;
			_currentPlatform.OnApplicationResume += ApplicationResume;
		}
	}

	private void ApplicationResume(Action callback)
	{
		LogUtils.LogError("ApplicationResume...");
		_gameProvider.AppFlowInformer.OnResume();
		callback?.Invoke();
	}

	private void ApplicationSuspend(Action callback)
	{
		LogUtils.LogError("ApplicationSuspend...");
		_gameProvider.AppFlowInformer.OnSuspend();
		callback?.Invoke();
	}

	[UsedImplicitly]
	private async void OnDestroy()
	{
		InputSystem.onDeviceChange -= InputSystemOnDeviceChange;
		if (IsConsole && _currentPlatform != null)
		{
			_currentPlatform.OnApplicationSuspend -= ApplicationSuspend;
			_currentPlatform.OnApplicationResume -= ApplicationResume;
		}
		Shutdown();
		if (_currentPlatform != null)
		{
			await _currentPlatform.DisposeAsync();
		}
		_currentPlatform = null;
	}

	private PlatformSetting GetPlatformSetting()
	{
		if ((bool)_actualPlatformSetting)
		{
			return _actualPlatformSetting;
		}
		_actualPlatformSetting = m_PlatformSettings.Where((PlatformSetting x) => x.Platform == GetCurrentPlatform()).FirstOrDefault();
		if (_actualPlatformSetting == null)
		{
			_actualPlatformSetting = m_PlatformSettings.Where((PlatformSetting x) => x.Platform == DeviceType.Standalone).First();
		}
		return _actualPlatformSetting;
	}

	private void SetDeviceSpecificQualitySettings()
	{
		Debug.Log("[PlatformLayer.cs] Platform was detected as " + PlatformID);
		Debug.Log("[PlatformLayer.cs] Game is running on " + Setting.Platform);
		Debug.Log("[PlatformLayer.cs] Enabling QualitySettings named " + Setting.GetQualitySettingsName());
		QualitySettings.SetQualityLevel(Setting.GetQualitySettingsIndex(), applyExpensiveChanges: true);
	}

	public static bool MatchesCurrentPlatform(string platformId)
	{
		if (Instance.PlatformID == platformId)
		{
			return true;
		}
		if (Instance.GetCurrentPlatform() == DeviceType.Standalone)
		{
			switch (platformId)
			{
			case "Standalone":
			case "GoGGalaxy":
			case "Steam":
			case "EpicGamesStore":
				return true;
			}
		}
		return false;
	}

	private void InputSystemOnDeviceChange(InputDevice inputDevice, InputDeviceChange changeType)
	{
		if (changeType == InputDeviceChange.Disconnected)
		{
			_gameProvider.ShowJoystickDisconnectionMessage();
		}
		if (changeType == InputDeviceChange.Reconnected)
		{
			_gameProvider.HideJoystickDisconnectionMessage();
		}
		if (changeType == InputDeviceChange.Added && inputDevice is Gamepad && Singleton<InputManager>.Instance != null && !InputManager.GamePadInUse)
		{
			_gameProvider.ShowJoystickConnectionMessage();
		}
	}

	private void Update()
	{
		if (Initialised && IsValid)
		{
			SteamClient.RunCallbacks();
		}
	}

	public void Initialize(IPlatform platform)
	{
		try
		{
			Init(780290u);
		}
		catch (Exception ex)
		{
			AsmoLogger.Error("PlatformLayer", "Couldn't instantiate a Steam client: " + ex.Message);
		}
		if (IsValid)
		{
			AsmoLogger.Debug("PlatformLayer", "Steam client created");
		}
		else
		{
			Dispose();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(EOSManagerPrefab, base.transform);
		Manager = gameObject.GetComponent<EOSManager>();
		if (Manager != null)
		{
			EOSManager.Instance.Init(Manager);
		}
		Initialised = true;
	}

	public void Shutdown()
	{
		Networking?.Shutdown();
		Dispose();
	}

	public bool IsPlatformPartnerIDCorrect(int partnerID)
	{
		return true;
	}

	public void Init(uint steamGameId)
	{
		AsmoLogger.Debug("PlatformLayer", "Initializing Steam client");
		try
		{
			SteamClient.Init(steamGameId);
		}
		catch (Exception ex)
		{
			AsmoLogger.Error("PlatformLayer", "Couldn't initialize a Steam client: " + ex.Message);
		}
		DeleteSteamAppID();
		if (SteamClient.RestartAppIfNecessary(steamGameId))
		{
			Application.Quit();
		}
		else if (!IsValid)
		{
			SteamClient.Shutdown();
			Debug.LogError("Steam it's not running. Will quit application now.");
			Application.Quit(-1);
		}
	}

	private async Task<byte[]> GetSteamAppTicket()
	{
		return await SteamUser.RequestEncryptedAppTicketAsync();
	}

	public void Dispose()
	{
		SteamClient.Shutdown();
	}

	private void ExistOrThrow()
	{
		if (IsValid)
		{
			return;
		}
		AsmoLogger.Error("PlatformLayer", "Steam is not available or active on this platform !");
		throw new NullReferenceException("Steam is not available or active on this platform !");
	}

	private void DeleteSteamAppID()
	{
		if (!Application.isEditor && File.Exists("steam_appid.txt"))
		{
			try
			{
				File.Delete("steam_appid.txt");
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
			}
			if (File.Exists("steam_appid.txt"))
			{
				Debug.LogError("Cannot delete steam_appid.txt. Quitting...");
				Application.Quit();
			}
		}
	}

	public DeviceType GetCurrentPlatform()
	{
		return DeviceType.Standalone;
	}

	public string GetSystemLanguage()
	{
		return Platform.GetSystemLanguage();
	}
}
