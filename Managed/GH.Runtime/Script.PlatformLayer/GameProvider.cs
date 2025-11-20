using Platforms;
using Platforms.Activities;
using Platforms.Epic;
using Platforms.GameCore;
using Platforms.Generic;
using Platforms.PS4;
using Platforms.PS5;
using Platforms.PSShared;
using Platforms.ProsOrHydra;
using Platforms.Steam;

namespace Script.PlatformLayer;

public class GameProvider : IGameProvider, IProjectSpecificDependenciesGeneric, IProjectSpecificDependenciesPS4, IProjectSpecificDependenciesPSShared, IProjectSpecificDependenciesPS5, IProjectSpecificDependenciesGameCore, IProjectSpecificDependenciesSteam, IProjectSpecificDependenciesEpic
{
	private readonly DefaultSaveDataParamsBuilderPSShared _saveDataParamsBuilder = new DefaultSaveDataParamsBuilderPSShared();

	private readonly SampleNpToolkitSettingsProvider _sampleNpToolkitSettings = new SampleNpToolkitSettingsProvider();

	private readonly SamplePsnSettingsProvider _samplePsnSettings = new SamplePsnSettingsProvider();

	private readonly GHActivitiesProvider _activitiesProvider = new GHActivitiesProvider();

	private readonly SampleGameIntentReceiver _sampleGameIntentReceiver = new SampleGameIntentReceiver();

	private readonly GHEntitlementsProvider _ghEntitlementsProvider = new GHEntitlementsProvider();

	private PlayerOnlineProvider _playerOnlineProvider;

	private readonly SampleHydraAnalyticsSettingsProvider _sampleHydraAnalyticsSettingsSettingsProvider = new SampleHydraAnalyticsSettingsProvider();

	private readonly IAppFlowInformer _appFlowInformer = new AppFlowInformerService();

	public ISaveDataParamsBuilderPSShared SaveDataParamsBuilderPSShared => _saveDataParamsBuilder;

	public INpToolkitSettingsProvider NpToolkitSettingsProvider => _sampleNpToolkitSettings;

	public IEntitlementsProviderPS4 EntitlementsProviderPS4 => _ghEntitlementsProvider;

	public IHydraProsSettingsProviderPS4 HydraProsSettingsProviderPS4 => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IEntitlementsProviderPS5 EntitlementsProviderPS5 => _ghEntitlementsProvider;

	public IHydraProsSettingsProviderPS5 HydraProsSettingsProviderPS5 => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IHydraProsSettingsProviderPS4 HydraAnalyticsSettingsProviderPS5 => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IHydraProsSettingsProviderPS5 HydraAnalyticsSettingsProviderPS4 => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IProsHydraSettingsProviderGameCore HydraAnalyticsSettingsProviderGeneric => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IProsHydraSettingsProviderGeneric HydraAnalyticsSettingsSettingsProviderGameCore => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IHydraSettingsProviderSteam HydraSettingsProviderSteam => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IHydraSettingsProviderEpic HydraSettingsProviderEpic => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IPsnSettingsProvider PsnSettingsProvider => _samplePsnSettings;

	public IActivitiesProvider ActivitiesProvider => _activitiesProvider;

	public IGameIntentReceiver GameIntentReceiver => _sampleGameIntentReceiver;

	public IAppFlowInformer AppFlowInformer => _appFlowInformer;

	public IProsSettingsProvider ProsSettingsProvider => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IHydraSettingsProvider HydraSettingsProvider => _sampleHydraAnalyticsSettingsSettingsProvider;

	public bool PlayerOnline
	{
		get
		{
			if (_playerOnlineProvider != null)
			{
				return _playerOnlineProvider.IsPlayerOnline;
			}
			return false;
		}
		set
		{
		}
	}

	public bool CrossPlayTurnedOn => FFSNetwork.Behaviour.IsCrossplaySession();

	public IProsHydraSettingsProviderGeneric ProsHydraSettingsProviderGeneric => _sampleHydraAnalyticsSettingsSettingsProvider;

	public IProsHydraSettingsProviderGameCore ProsHydraSettingsSettingsProviderGameCore => _sampleHydraAnalyticsSettingsSettingsProvider;

	public bool IsSignInUIRequired { get; private set; }

	public bool IsShowSignOutMessage { get; private set; }

	public bool GamepadDisconnected { get; private set; }

	public void ReturnToInitialInteractiveScreen(bool isSignInUIRequired = false)
	{
		IsSignInUIRequired = isSignInUIRequired;
		IsShowSignOutMessage = !isSignInUIRequired;
		global::PlatformLayer.Platform.UserManagement.ResetCurrentUser();
		UIManager.LoadMainMenuAfterSceneLoaded();
	}

	public bool IsUserActive(Platforms.IPlatformUserData userData)
	{
		return false;
	}

	public void ShowJoystickDisconnectionMessage()
	{
		GamepadDisconnected = true;
		if (!(Singleton<InputManager>.Instance == null))
		{
			if (Singleton<InputManager>.Instance.IsPCVersion() && InputManager.GamePadInUse && !global::PlatformLayer.Instance.IsConsole && !Singleton<InputManager>.Instance.IsPCAndGamepadVersion())
			{
				InputManager.UpdateMouseInputEnabled(value: true);
				Singleton<SelectInputDeviceBox>.Instance.Active(SelectInputDeviceBox.ESelectInputReason.DisconnectGamepad);
			}
			else if (global::PlatformLayer.Instance.IsConsole && !Singleton<GamepadDisconnectionBox>.Instance.Window.IsOpen)
			{
				Singleton<GamepadDisconnectionBox>.Instance.Activate(pauseGame: true);
			}
			else
			{
				GamepadDisconnected = false;
			}
		}
	}

	public void HideJoystickDisconnectionMessage()
	{
		GamepadDisconnected = false;
		if (!(Singleton<InputManager>.Instance == null))
		{
			if (Singleton<InputManager>.Instance.IsPCVersion() && InputManager.GamePadInUse && !global::PlatformLayer.Instance.IsConsole)
			{
				InputManager.UpdateMouseInputEnabled(value: false);
				Singleton<SelectInputDeviceBox>.Instance.Disable();
			}
			if (global::PlatformLayer.Instance.IsConsole && Singleton<GamepadDisconnectionBox>.Instance.Window.IsOpen)
			{
				Singleton<GamepadDisconnectionBox>.Instance.Deactivate();
			}
		}
	}

	public void ShowJoystickConnectionMessage()
	{
		if (!(Singleton<InputManager>.Instance == null) && Singleton<InputManager>.Instance.IsPlayerSelectInputDevice && Singleton<InputManager>.Instance.IsPCVersion() && !InputManager.GamePadInUse)
		{
			Singleton<GamepadConnectionBox>.Instance.Activate();
		}
	}

	private bool GetPlayerOnlineProvider()
	{
		if (_playerOnlineProvider == null && CoroutineHelper.instance != null)
		{
			_playerOnlineProvider = new PlayerOnlineProvider();
			return true;
		}
		return _playerOnlineProvider != null;
	}

	public void MainMenuLoaded()
	{
		if (GetPlayerOnlineProvider())
		{
			_playerOnlineProvider.ResetMenuLoading();
		}
	}

	public void MainMenuLoadingStarted()
	{
		if (GetPlayerOnlineProvider())
		{
			_playerOnlineProvider.SetMenuLoading();
		}
	}
}
