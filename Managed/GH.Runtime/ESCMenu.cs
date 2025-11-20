using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using GLOO.Introduction;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.State;
using Platforms.Social;
using Script.GUI.IngameMenu.EscMenuVoiceChat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ESCMenu : Singleton<ESCMenu>
{
	[SerializeField]
	protected ControllerInputArea controllerArea;

	[SerializeField]
	protected ToggleGroup toggleGroup;

	[SerializeField]
	protected UIMainMenuOption continueButton;

	[SerializeField]
	protected UIMainMenuOption mainMenuButton;

	[SerializeField]
	protected UIMainMenuOption multiplayerButton;

	[SerializeField]
	protected UIMainMenuOption exitButton;

	[SerializeField]
	protected UIMainMenuOption compendiumButton;

	[SerializeField]
	protected UIMainMenuOption optionsButton;

	[SerializeField]
	protected GameObject onlineCheckbox;

	[SerializeField]
	protected LeanTweenGUIAnimator _leanTweenGUIAnimator;

	[SerializeField]
	[TextArea]
	private string multiplayerLockedTooltipFormat = "<sprite name=\"Lock_Icon_White\"><indent=30><font=\"MarcellusSC-Regular SDF\"><color=#EACF8CFF>{0}</color></font>\n{1}";

	[SerializeField]
	[TextArea]
	private string multiplayerCrossplayLockedTooltipFormat = "<font=\"MarcellusSC-Regular SDF\"><color=#EACF8CFF>{0}</color></font>\n{1}";

	protected UIWindow myWindow;

	protected EscMenuVoiceChatController _voiceChatController;

	private HashSet<Component> requestDisable;

	private UIWindow.EscapeKeyAction defaultEscapeOption;

	private const string FadeInBackgroundInformationText = "Fade In background";

	protected bool IsDestroying;

	public EscMenuVoiceChatController VoiceChatController => _voiceChatController;

	public bool IsOpen => myWindow.IsOpen;

	public UnityEvent OnShown => myWindow.onShown;

	public UnityEvent OnHidden => myWindow.onHidden;

	public event Action BeforeMainMenuLoadingStarted;

	public event Action<bool> EscMenuStateChanged;

	protected override void Awake()
	{
		base.Awake();
		_voiceChatController = Singleton<SpecialUIProvider>.Instance.EscMenuVoiceChatController;
		requestDisable = new HashSet<Component>();
		myWindow = GetComponent<UIWindow>();
		defaultEscapeOption = myWindow.escapeKeyAction;
		myWindow.onTransitionBegin.AddListener(OnTransitionBegin);
		continueButton.Init(Hide);
		mainMenuButton.Init(delegate
		{
			LoadMainMenu();
		}, delegate
		{
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
			}
			SetFocused(isFocused: true);
		});
		if (PlatformLayer.Instance.IsConsole)
		{
			exitButton.gameObject.SetActive(value: false);
		}
		else
		{
			exitButton.Init(ExitGame, delegate
			{
				if (controllerArea.IsFocused)
				{
					EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
				}
				SetFocused(isFocused: true);
			});
		}
		compendiumButton.Init(delegate
		{
			UIWindow component = Singleton<SpecialUIProvider>.Instance.CompendiumUIObject.GetComponent<UIWindow>();
			component.onHidden.RemoveListener(OnHideOptionWindow);
			component.onHidden.AddListener(OnHideOptionWindow);
			component.Show();
		}, delegate
		{
			UIWindow component = Singleton<SpecialUIProvider>.Instance.CompendiumUIObject.GetComponent<UIWindow>();
			component.onHidden.RemoveListener(OnHideOptionWindow);
			component.Hide();
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(compendiumButton.gameObject);
			}
		});
		optionsButton.Init(delegate
		{
			SetFocused(isFocused: false);
			Singleton<UIOptionsWindow>.Instance.Show(optionsButton.transform as RectTransform, optionsButton.Deselect);
		}, delegate
		{
			Singleton<UIOptionsWindow>.Instance.Hide();
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(optionsButton.gameObject);
			}
			SetFocused(isFocused: true);
		});
		multiplayerButton.Init(delegate
		{
			PlatformLayer.Networking.CheckNetworkAvailabilityAsync(delegate(bool isConnected)
			{
				if (isConnected)
				{
					PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Multiplayer, delegate(bool isMultiplayerValid)
					{
						if (isMultiplayerValid)
						{
							SetFocused(isFocused: false);
							Singleton<UIMultiplayerEscSubmenu>.Instance.OnHide.RemoveListener(OnHideOptionWindow);
							Singleton<UIMultiplayerEscSubmenu>.Instance.OnHide.AddListener(OnHideOptionWindow);
							Singleton<UIMultiplayerEscSubmenu>.Instance.Show();
						}
						else
						{
							multiplayerButton.Deselect();
						}
					}, PrivilegePlatform.AllExceptSwitch);
				}
				else
				{
					multiplayerButton.Deselect();
				}
			});
		}, delegate
		{
			Singleton<UIMultiplayerEscSubmenu>.Instance.OnHide.RemoveListener(OnHideOptionWindow);
			Singleton<UIMultiplayerEscSubmenu>.Instance.Hide();
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(multiplayerButton.gameObject);
			}
			SetFocused(isFocused: true);
		});
		controllerArea.OnFocused.AddListener(OnControllerAreaFocused);
		myWindow.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Shown)
			{
				controllerArea.Focus();
			}
		});
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		OnControllerTypeChanged(Singleton<InputManager>.Instance.CurrentControllerType);
		InputManager.RegisterToOnPressed(KeyAction.UI_PAUSE, ShowUIWindow);
		if (PlatformLayer.Setting.SimplifiedUISettings.DisableUIBlur)
		{
			LeanTweenGuiAnimationSettingFade leanTweenGuiAnimationSettingFade = (LeanTweenGuiAnimationSettingFade)_leanTweenGUIAnimator.GetSettings().FirstOrDefault((LeanTweenGUIAnimationSetting x) => x.Information == "Fade In background");
			if (leanTweenGuiAnimationSettingFade != null)
			{
				leanTweenGuiAnimationSettingFade.ToValue = 1f;
			}
		}
	}

	protected override void OnDestroy()
	{
		IsDestroying = true;
		if (!CoreApplication.IsQuitting)
		{
			OnHide();
			InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
			InputManager.UnregisterToOnPressed(KeyAction.UI_PAUSE, ShowUIWindow);
			_voiceChatController = null;
			multiplayerButton.Deselect();
			compendiumButton.Deselect();
			optionsButton.Deselect();
			base.OnDestroy();
		}
	}

	private void ShowUIWindow()
	{
		myWindow.Show();
	}

	private void OnControllerTypeChanged(ControllerType obj)
	{
		if (InputManager.GamePadInUse)
		{
			myWindow.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		}
		else
		{
			myWindow.escapeKeyAction = UIWindow.EscapeKeyAction.Toggle;
		}
	}

	private void OnControllerAreaFocused()
	{
		if (myWindow.IsOpen)
		{
			toggleGroup.SetAllTogglesOff();
		}
		else
		{
			myWindow.Show();
		}
	}

	private void OnHideOptionWindow()
	{
		toggleGroup.SetAllTogglesOff();
	}

	protected virtual void SetFocused(bool isFocused)
	{
		continueButton.SetFocused(isFocused);
		multiplayerButton.SetFocused(isFocused);
		compendiumButton.SetFocused(isFocused);
		exitButton.SetFocused(isFocused);
		optionsButton.SetFocused(isFocused);
		mainMenuButton.SetFocused(isFocused);
	}

	protected void OnTransitionBegin(UIWindow window, UIWindow.VisualState state, bool instant)
	{
		if (state == UIWindow.VisualState.Shown)
		{
			OnShow();
		}
		else
		{
			OnHide();
		}
	}

	protected virtual void OnShow()
	{
		InputManager.RequestDisableInput(this, KeyAction.HIGHLIGHT);
		this.EscMenuStateChanged?.Invoke(obj: true);
		SetFocused(isFocused: true);
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.pause);
		if (!FFSNetwork.IsOnline)
		{
			OnSwitchedToSinglePlayer();
		}
		else
		{
			OnSwitchedToMultiplayer();
		}
		RefreshMultiplayerButton();
		_voiceChatController?.OnShow();
		Singleton<UIReadyToggle>.Instance.CanBeToggled = false;
	}

	protected void RefreshMultiplayerButton()
	{
		string tooltip;
		bool isInteractable = CheckMultiplayerButton(out tooltip);
		multiplayerButton.IsInteractable = isInteractable;
		multiplayerButton.SetTooltip(tooltip.IsNOTNullOrEmpty(), tooltip);
	}

	protected virtual bool CheckMultiplayerButton(out string tooltip)
	{
		CMapState mapState = AdventureState.MapState;
		if (mapState != null)
		{
			CQuestState inProgressQuestState = mapState.InProgressQuestState;
			if (inProgressQuestState != null && inProgressQuestState.IsSoloScenario)
			{
				tooltip = null;
				return false;
			}
		}
		if (AdventureState.MapState != null && (AdventureState.MapState?.HeadquartersState == null || AdventureState.MapState.HeadquartersState.MultiplayerUnlocked))
		{
			if (PlatformLayer.UserData.IsSignedIn)
			{
				if (!PlatformLayer.Instance.IsConsole && SaveData.Instance.Global.CrossplayEnabled && SceneController.Instance.Modding != null && SaveData.Instance.Global.CurrentModdedRuleset != null)
				{
					tooltip = string.Format(multiplayerCrossplayLockedTooltipFormat, LocalizationManager.GetTranslation("GUI_MULTIPLAYER"), LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_CROSSPLAY_LOCKED_TOOLTIP"));
					return false;
				}
				tooltip = null;
				return true;
			}
			tooltip = null;
			return false;
		}
		tooltip = string.Format(multiplayerLockedTooltipFormat, LocalizationManager.GetTranslation("GUI_MULTIPLAYER"), string.Format("<color=#{1}>{0}</color>", LocalizationManager.GetTranslation("GUI_MULTIPLAYER_LOCKED_TOOLTIP"), UIInfoTools.Instance.warningColor.ToHex()));
		return false;
	}

	protected virtual void OnSwitchedToMultiplayer()
	{
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
		onlineCheckbox.SetActive(value: true);
	}

	protected virtual void OnSwitchedToSinglePlayer()
	{
		FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		onlineCheckbox.SetActive(value: false);
	}

	protected virtual void OnHide()
	{
		InputManager.RequestEnableInput(this, KeyAction.HIGHLIGHT);
		this.EscMenuStateChanged?.Invoke(obj: false);
		toggleGroup.SetAllTogglesOff();
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		controllerArea.Unfocus();
		_voiceChatController?.OnHide();
		multiplayerButton.SetTooltip(enable: false);
		if (LevelMessageUILayoutGroup.IsShown)
		{
			if (Singleton<UIIntroductionManager>.Instance != null)
			{
				Singleton<UIIntroductionManager>.Instance.LayoutGroup.EnableShownAreas();
			}
			else if (LevelMessagesUIHandler.s_Instance != null)
			{
				LevelMessagesUIHandler.s_Instance.LevelMessageBoxLayoutGroup.EnableShownAreas();
			}
		}
		if (Singleton<UIReadyToggle>.IsInitialized)
		{
			Singleton<UIReadyToggle>.Instance.CanBeToggled = true;
		}
	}

	public void Hide()
	{
		myWindow.Hide();
	}

	public virtual void LoadMainMenu(bool skipConfirmation = false)
	{
		if (AutoTestController.s_AutoLogPlaybackInProgress)
		{
			AutoTestController.s_Instance.EndAutotestsWithoutEval();
		}
		Main.Unpause3DWorld(forceUnpause: true);
		if (skipConfirmation)
		{
			OnLoadMainMenuConfirmed();
			return;
		}
		bool flag = FFSNetwork.IsOnline && RootSaveData.ReleaseVersionType != RootSaveData.EReleaseTypes.Release;
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			UIConfirmationBoxManager mainMenuInstance = UIConfirmationBoxManager.MainMenuInstance;
			string translation = LocalizationManager.GetTranslation("GUI_CONFIRMATION_RETURN_MAINMENU");
			string translation2 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_RETURN_MAINMENU_MAP");
			UnityAction onActionConfirmed = OnLoadMainMenuConfirmed;
			UnityAction onActionCancelled = mainMenuButton.Deselect;
			bool enableSoftlockReport = flag;
			mainMenuInstance.ShowGenericConfirmation(translation, translation2, onActionConfirmed, onActionCancelled, null, null, null, showHeader: true, enableSoftlockReport, null, resetAfterAction: true);
			return;
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			LevelEditorController s_Instance = LevelEditorController.s_Instance;
			if ((object)s_Instance != null && !s_Instance.IsEditing)
			{
				UIConfirmationBoxManager mainMenuInstance2 = UIConfirmationBoxManager.MainMenuInstance;
				string translation3 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_RETURN_MAINMENU");
				string translation4 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_RETURN_MAINMENU_SCENARIO");
				UnityAction onActionConfirmed2 = OnLoadMainMenuConfirmed;
				UnityAction onActionCancelled2 = mainMenuButton.Deselect;
				bool enableSoftlockReport = flag;
				mainMenuInstance2.ShowGenericConfirmation(translation3, translation4, onActionConfirmed2, onActionCancelled2, null, null, null, showHeader: true, enableSoftlockReport, null, resetAfterAction: true);
				return;
			}
		}
		OnLoadMainMenuConfirmed();
	}

	protected virtual void OnLoadMainMenuConfirmed()
	{
		this.BeforeMainMenuLoadingStarted?.Invoke();
		myWindow.Hide(instant: true);
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if ((object)s_Instance == null || !s_Instance.IsEditing)
		{
			LevelEditorController s_Instance2 = LevelEditorController.s_Instance;
			if ((object)s_Instance2 == null || !s_Instance2.IsPreviewingLevel)
			{
				goto IL_0053;
			}
		}
		LevelEditorController.s_Instance?.ExitToMainMenuThroughUI();
		goto IL_0053;
		IL_0053:
		UIManager.LoadMainMenu();
	}

	public void ExitGame()
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			UIConfirmationBoxManager mainMenuInstance = UIConfirmationBoxManager.MainMenuInstance;
			string translation = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_TITLE");
			string translation2 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_MAP");
			UnityAction onActionConfirmed = Application.Quit;
			UnityAction onActionCancelled = exitButton.Deselect;
			bool isOnline = FFSNetwork.IsOnline;
			mainMenuInstance.ShowGenericConfirmation(translation, translation2, onActionConfirmed, onActionCancelled, null, null, null, showHeader: true, isOnline, null, resetAfterAction: true);
			return;
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			LevelEditorController s_Instance = LevelEditorController.s_Instance;
			if ((object)s_Instance != null && !s_Instance.IsEditing)
			{
				UIConfirmationBoxManager mainMenuInstance2 = UIConfirmationBoxManager.MainMenuInstance;
				string translation3 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_TITLE");
				string translation4 = LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_GAME_SCENARIO");
				UnityAction onActionConfirmed2 = Application.Quit;
				UnityAction onActionCancelled2 = exitButton.Deselect;
				bool isOnline = FFSNetwork.IsOnline;
				mainMenuInstance2.ShowGenericConfirmation(translation3, translation4, onActionConfirmed2, onActionCancelled2, null, null, null, showHeader: true, isOnline, null, resetAfterAction: true);
				return;
			}
		}
		Application.Quit();
	}

	public void EnableDisplay(bool enable, Component request)
	{
		if (!enable)
		{
			requestDisable.Add(request);
		}
		else
		{
			requestDisable.Remove(request);
		}
		myWindow.escapeKeyAction = ((requestDisable.Count == 0) ? defaultEscapeOption : UIWindow.EscapeKeyAction.None);
	}
}
