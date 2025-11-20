#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using Code.State;
using GLOOM.MainMenu.Credits;
using I2.Loc;
using JetBrains.Annotations;
using Platforms.Social;
using SM.Gamepad;
using SM.Utils;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using TMPro;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainMenuUIManager : MonoBehaviour
{
	public TextMeshProUGUI Version;

	public GameObject VersionGO;

	public GameObject MainMenuCamera;

	public GameObject notificationManager;

	[SerializeField]
	private PrimaryUserData _primaryUserData;

	[SerializeField]
	private CanvasGroup canvasGroupInteraction;

	private HashSet<object> requestDisableInteraction = new HashSet<object>();

	[Header("Screen Components")]
	public InitialInteractionScreen initialInteractionScreen;

	public EULAScreen eulaScreen;

	public UIMainOptionsMenu mainMenu;

	public CustomPartySetup CustomPartySetupScreen;

	public LevelEditorMainMenu LevelEditorMainMenuScreen;

	public UIMainMenuModeSelection ModeSelectionScreen;

	[Header("Gamepad")]
	[SerializeField]
	private Hotkey _aboutHotKey;

	[SerializeField]
	private UICreditsWindow _uiCreditsWindow;

	[SerializeField]
	private UIMenuOption _tutorialUIMenuOption;

	[SerializeField]
	private GameObject _logo;

	private static Action m_OnLoadingCompleteCallback;

	public static bool JoinSessionCommandLineProcessed;

	private bool m_Initialised;

	private ConditionalHandlerBlocker _conditionalCreditsWindowBlocker;

	public static MainMenuUIManager Instance { get; set; }

	public bool Initialised => m_Initialised;

	public UIMenuOption TutorialUIMenuOption => _tutorialUIMenuOption;

	public GameObject Logo => _logo;

	public PrimaryUserData PrimaryUserData => _primaryUserData;

	[UsedImplicitly]
	private void Awake()
	{
		m_Initialised = false;
		Instance = this;
		PlatformLayer.GameProvider.MainMenuLoaded();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		initialInteractionScreen.OnSuccessfulSignIn -= InitialiseMainMenu;
		Instance = null;
	}

	[UsedImplicitly]
	private void Start()
	{
		Debug.Log("BRYNN: MainMenuUIManager Start() Start" + DateTime.Now);
		StartCoroutine(InitializeWhenFullyloaded());
		Debug.Log("BRYNN: MainMenuUIManager Start() End" + DateTime.Now);
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Debug.Log("BRYNN: MainMenuUIManager OnEnable Start " + DateTime.Now);
		OnLanguageChanged();
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
		ObjectPool.ClearEnhancements();
		Debug.Log("BRYNN: MainMenuUIManager OnEnable End " + DateTime.Now);
		SubscribeOnGamepadEvents();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.None);
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
			UnsubscribeOnGamepadEvents();
		}
	}

	private void ShowHotkey()
	{
		_aboutHotKey.Initialize(Singleton<UINavigation>.Instance.Input);
	}

	private void HideHotkey()
	{
		_aboutHotKey.Deinitialize();
		_aboutHotKey.DisplayHotkey(active: false);
	}

	public void RequestDisableInteraction(bool disable, object request)
	{
		if (disable)
		{
			requestDisableInteraction.Add(request);
		}
		else
		{
			requestDisableInteraction.Remove(request);
		}
		canvasGroupInteraction.blocksRaycasts = requestDisableInteraction.Count == 0;
	}

	public static void SetLoadingCompleteCallback(Action callbackToSet)
	{
		m_OnLoadingCompleteCallback = callbackToSet;
	}

	private IEnumerator InitializeWhenFullyloaded()
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			Debug.Log("BRYNN: MainMenuUIManager InitializeWhenFullyloaded Start " + DateTime.Now);
			while (!PersistentData.s_Instance.IsDataLoaded)
			{
				yield return null;
			}
			Debug.Log("BRYNN: MainMenuUIManager InitializeWhenFullyloaded DataIsLoaded " + DateTime.Now);
		}
		yield return null;
		InitializeUI();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		m_OnLoadingCompleteCallback?.Invoke();
		m_OnLoadingCompleteCallback = null;
		Debug.Log("BRYNN: MainMenuUIManager InitializeWhenFullyloaded End " + DateTime.Now);
	}

	private void SubscribeOnGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			IState state = Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.InitialInteractionScreen);
			IState state2 = Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.HouseRules);
			IState state3 = Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.EULAScreen);
			IState state4 = Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.SelectDLC);
			IState state5 = Singleton<UINavigation>.Instance.StateMachine.GetState(MainStateTag.DropdownOptionSelect);
			_conditionalCreditsWindowBlocker = new ConditionalHandlerBlocker(InfoBlockFunctor);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_INFO, OnInfoClicked, ShowHotkey, HideHotkey).AddBlocker(_conditionalCreditsWindowBlocker).AddBlocker(new StateActionHandlerBlocker(new HashSet<IState> { state, state2, state3, state4, state5 })));
			UICreditsWindow uiCreditsWindow = _uiCreditsWindow;
			uiCreditsWindow.OnActivityChanged = (Action<bool>)Delegate.Combine(uiCreditsWindow.OnActivityChanged, new Action<bool>(CreditsWindowOnActivityChanged));
		}
	}

	private void CreditsWindowOnActivityChanged(bool active)
	{
		_conditionalCreditsWindowBlocker.StateChangedHandler();
	}

	private bool InfoBlockFunctor()
	{
		return _uiCreditsWindow.IsActive;
	}

	private void UnsubscribeOnGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			if (Singleton<KeyActionHandlerController>.IsInitialized)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_INFO, OnInfoClicked);
			}
			UICreditsWindow uiCreditsWindow = _uiCreditsWindow;
			uiCreditsWindow.OnActivityChanged = (Action<bool>)Delegate.Remove(uiCreditsWindow.OnActivityChanged, new Action<bool>(CreditsWindowOnActivityChanged));
		}
	}

	private void OnInfoClicked()
	{
		Singleton<EarlyAccessNotificationManager>.Instance.ShowEarlyAccessNotification();
	}

	private void InitializeUI()
	{
		SceneController.Instance.MainSceneCamera.SetActive(value: false);
		MainMenuCamera.SetActive(value: true);
		if (InputManager.GamePadInUse && !PlatformLayer.UserData.IsSignedIn && PlatformLayer.Instance.IsConsole)
		{
			initialInteractionScreen.OnSuccessfulSignIn += OnSignIn;
			initialInteractionScreen.Show();
			m_Initialised = true;
		}
		else if (!SaveData.Instance.Global.UsersAcceptedEULA.Contains(PlatformLayer.UserData.PlatformAccountID))
		{
			eulaScreen.Show(InitialiseMainMenu);
			m_Initialised = true;
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MainOptions);
			InitialiseMainMenu();
		}
	}

	private void OnSignIn()
	{
		initialInteractionScreen.OnSuccessfulSignIn -= OnSignIn;
		if (!SaveData.Instance.Global.UsersAcceptedEULA.Contains(PlatformLayer.UserData.PlatformAccountID))
		{
			eulaScreen.Show(InitialiseMainMenu);
			return;
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MainOptions);
		InitialiseMainMenu();
	}

	private void InitialiseMainMenu()
	{
		notificationManager.SetActive(value: true);
		if (Singleton<PromotionDLCManager>.Instance != null)
		{
			Singleton<PromotionDLCManager>.Instance.InitPromotionDlcSlots();
			Singleton<PromotionDLCManager>.Instance.Show();
		}
		if (PlatformLayer.Instance.IsDelayedInit)
		{
			PrimaryUserData.Init();
			PrimaryUserData.Show();
		}
		else
		{
			PlatformLayer.Platform.PlatformSocial.RegisterInviteEvent();
		}
		LogUtils.Log($"InitialiseMainMenu {PlatformLayer.Networking.PlatformInvitesSupported} {PlatformLayer.Networking.HasInvitePending}");
		bool ePICInvitesSupported = PlatformLayer.Networking.EPICInvitesSupported;
		bool hasEPICInvitePending = PlatformLayer.Networking.HasEPICInvitePending;
		if ((PlatformLayer.Networking.PlatformInvitesSupported || ePICInvitesSupported) && (PlatformLayer.Networking.HasInvitePending || hasEPICInvitePending || PlatformLayer.Networking.CheckCommandLineForPendingInvite()))
		{
			LogUtils.Log("InitialiseMainMenu HasPendingInvite");
			PlatformLayer.Networking.CheckNetworkAvailabilityAsync(delegate(bool isConnected)
			{
				LogUtils.Log($"InitialiseMainMenu CheckNetworkAvailabilityAsync completed {isConnected}");
				if (isConnected)
				{
					PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Multiplayer, delegate(bool isMultiplayerValid)
					{
						if (isMultiplayerValid)
						{
							TriggerJoinMultiplayer();
						}
						else
						{
							PlatformLayer.Networking.LeaveSession(null);
							mainMenu.Show();
						}
						m_Initialised = true;
					}, PrivilegePlatform.AllExceptSwitch);
				}
				else
				{
					PlatformLayer.Networking.LeaveSession(null);
					mainMenu.Show();
				}
			});
		}
		else
		{
			mainMenu.Show();
			m_Initialised = true;
		}
	}

	private void OnLanguageChanged()
	{
		if (MF.NeedToShowVersion())
		{
			VersionGO.SetActive(value: true);
			MF.SetVersion(Version);
		}
		else
		{
			VersionGO.SetActive(value: false);
		}
	}

	private void TriggerJoinMultiplayer()
	{
		mainMenu.OpenJoinMultiplayer();
	}
}
