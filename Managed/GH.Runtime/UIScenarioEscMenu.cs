using System.Collections;
using Code.State;
using FFSNet;
using GLOOM;
using MapRuleLibrary.MapState;
using MapRuleLibrary.State;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.Utils;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIScenarioEscMenu : ESCMenu
{
	[SerializeField]
	private UIMainMenuOption quitDungeonButton;

	[SerializeField]
	private UIMainMenuOption restartRoundButton;

	[SerializeField]
	private UIMainMenuOption restartTurnButton;

	[SerializeField]
	private UIMainMenuOption skipMissionButton;

	[SerializeField]
	private UIMainMenuOption skipTutorialButton;

	[SerializeField]
	private UIMainMenuOption loadLevelEditorButton;

	private Coroutine multiplayerCoroutine;

	private Coroutine multiplayerButtonCoroutine;

	private UiNavigationBlocker _blocker = new UiNavigationBlocker("UIScenarioEscMenu");

	private IStateFilter _previousStateFilter = new StateFilterByType(typeof(GamepadDisconnectionBoxState), typeof(UiMultiplayerConfirmationBoxState), typeof(SelectInputDeviceBoxState)).InverseFilter();

	public static UIScenarioEscMenu ScenarioInstance => Singleton<ESCMenu>.Instance as UIScenarioEscMenu;

	public bool IsVisible { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		quitDungeonButton.Init(QuitDungeon);
		restartRoundButton.Init(QueueRestartRound, delegate
		{
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(restartRoundButton.gameObject);
			}
		});
		restartTurnButton.Init(RestartTurn, delegate
		{
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(restartTurnButton.gameObject);
			}
		});
		skipMissionButton.Init(SkipMission, delegate
		{
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(skipMissionButton.gameObject);
			}
		});
		skipTutorialButton.Init(SkipTutorial, delegate
		{
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(skipTutorialButton.gameObject);
			}
		});
		loadLevelEditorButton.Init(LoadLevelEditor);
		myWindow.enabled = false;
	}

	protected override void OnDestroy()
	{
		restartRoundButton = null;
		quitDungeonButton = null;
		restartTurnButton = null;
		skipMissionButton = null;
		skipTutorialButton = null;
		loadLevelEditorButton = null;
		base.OnDestroy();
	}

	private void Start()
	{
		TransitionManager.s_Instance.RegisterCallbackOnTransitionFinished(delegate
		{
			myWindow.enabled = true;
		});
	}

	protected void OnEnable()
	{
		controllerArea.OnFocused.AddListener(SwitchToEscMenuState);
	}

	protected void OnDisable()
	{
		controllerArea.OnFocused.RemoveListener(SwitchToEscMenuState);
	}

	private void SwitchToEscMenuState()
	{
		if (!Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<ScenarioEscMenuState>())
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.ScenarioEscMenu);
			IsVisible = true;
		}
	}

	private void Update()
	{
		if (myWindow.IsOpen)
		{
			UpdateButtonStates();
		}
	}

	private void UpdateButtonStates()
	{
		if (!SaveData.Instance.IsSavingThreadActive)
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				goto IL_0059;
			}
			if (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection)
			{
				bool num;
				if (FFSNetwork.IsOnline)
				{
					if (Choreographer.s_Choreographer.CurrentActor == null)
					{
						goto IL_0078;
					}
					num = Choreographer.s_Choreographer.CurrentActor.IsUnderMyControl;
				}
				else
				{
					num = Choreographer.s_Choreographer.CurrentActor is CPlayerActor;
				}
				if (num)
				{
					goto IL_0059;
				}
			}
		}
		goto IL_0078;
		IL_0059:
		int num2 = ((Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown) ? 1 : 0);
		goto IL_0079;
		IL_0078:
		num2 = 0;
		goto IL_0079;
		IL_0079:
		bool flag = (byte)num2 != 0;
		int num3;
		if (SaveData.Instance.Global.GameMode != EGameMode.FrontEndTutorial)
		{
			SaveData instance = SaveData.Instance;
			bool? obj;
			if ((object)instance == null)
			{
				obj = null;
			}
			else
			{
				GlobalData global = instance.Global;
				if (global == null)
				{
					obj = null;
				}
				else
				{
					PartyAdventureData currentAdventureData = global.CurrentAdventureData;
					if (currentAdventureData == null)
					{
						obj = null;
					}
					else
					{
						CMapState adventureMapState = currentAdventureData.AdventureMapState;
						if (adventureMapState == null)
						{
							obj = null;
						}
						else
						{
							CMapScenarioState currentMapScenarioState = adventureMapState.CurrentMapScenarioState;
							obj = ((currentMapScenarioState != null) ? new bool?(!currentMapScenarioState.IsTutorialOrIntroScenario) : ((bool?)null));
						}
					}
				}
			}
			num3 = ((obj ?? true) ? 1 : 0);
		}
		else
		{
			num3 = 0;
		}
		bool flag2 = (byte)num3 != 0;
		bool flag3 = !FFSNetwork.IsOnline || !FFSNetwork.IsHost || (PlayerRegistry.JoiningPlayers != null && PlayerRegistry.JoiningPlayers.Count <= 0 && PlayerRegistry.ConnectingUsers != null && PlayerRegistry.ConnectingUsers.Count <= 0);
		bool flag4 = !FFSNetwork.IsOnline || FFSNetwork.IsHost || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest;
		bool flag5 = FFSNetwork.IsOnline && PlayerRegistry.WaitForOtherPlayersFullLoaded;
		bool flag6 = true;
		if (flag && flag2 && flag3 && flag4 && flag6 && !flag5)
		{
			restartRoundButton.IsInteractable = true;
			restartRoundButton.SetTooltip(enable: false);
		}
		else
		{
			restartRoundButton.IsInteractable = false;
			if (!flag4)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_MP_HOST_TOOLTIP"));
			}
			else if (!flag3)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_WHILE_PLAYERS_JOINING_MP_TOOLTIP"));
			}
			else if (!flag)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_PHASE_TOOLTIP"));
			}
			else if (!flag2)
			{
				restartRoundButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_RESTART_ROUND_GAMEMODE_TOOLTIP"));
			}
			else
			{
				restartRoundButton.SetTooltip(enable: true);
			}
		}
		bool flag7 = !FFSNetwork.IsOnline || FFSNetwork.IsHost;
		if (flag && flag3 && flag7 && !flag5)
		{
			quitDungeonButton.IsInteractable = true;
			quitDungeonButton.SetTooltip(enable: false);
			return;
		}
		quitDungeonButton.IsInteractable = false;
		if (!flag7)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_MP_HOST_TOOLTIP"));
		}
		else if (!flag3)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_WHILE_PLAYERS_JOINING_MP_TOOLTIP"));
		}
		else if (!flag)
		{
			quitDungeonButton.SetTooltip(enable: true, LocalizationManager.GetTranslation("GUI_ABANDON_QUEST_PHASE_TOOLTIP"));
		}
		else
		{
			quitDungeonButton.SetTooltip(enable: true);
		}
	}

	protected override void SetFocused(bool isFocused)
	{
		base.SetFocused(isFocused);
		if (!IsDestroying)
		{
			quitDungeonButton.SetFocused(isFocused);
			restartRoundButton.SetFocused(isFocused);
			restartTurnButton.SetFocused(isFocused);
			skipMissionButton.SetFocused(isFocused);
			skipTutorialButton.SetFocused(isFocused);
			loadLevelEditorButton.SetFocused(isFocused);
		}
	}

	protected override void OnShow()
	{
		Singleton<UINavigation>.Instance.StateMachine.SetFilter(new StateFilterByTagType<MainStateTag>());
		base.OnShow();
		quitDungeonButton.gameObject.SetActive(SaveData.Instance?.Global != null);
		skipMissionButton.gameObject.SetActive(SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.IsTutorialOrIntroScenario);
		skipTutorialButton.gameObject.SetActive(SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.CurrentMapScenarioState != null && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.IsStartingScenario);
		loadLevelEditorButton.gameObject.SetActive(value: false);
		UpdateButtonStates();
		StartTrackMultiplayerButtonState();
		DisableCameraInput();
	}

	protected override void OnHide()
	{
		Singleton<UINavigation>.Instance.StateMachine.RemoveFilter();
		IsVisible = false;
		SwitchToPreviousState();
		StopMultiplayerButtonCoroutine();
		EnableCameraInput();
		base.OnHide();
	}

	private void DisableCameraInput()
	{
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	private void EnableCameraInput()
	{
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
	}

	private void SwitchToPreviousState()
	{
		if (!IsDestroying && Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<ScenarioEscMenuState>())
		{
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_previousStateFilter, useSimplifiedFilter: false);
		}
	}

	private void StartTrackMultiplayerButtonState()
	{
		if (multiplayerButtonCoroutine == null)
		{
			multiplayerButtonCoroutine = StartCoroutine(CheckMultiplayerButtonState());
		}
	}

	private void StopMultiplayerButtonCoroutine()
	{
		if (multiplayerButtonCoroutine != null)
		{
			StopCoroutine(multiplayerButtonCoroutine);
		}
		multiplayerButtonCoroutine = null;
	}

	private IEnumerator CheckMultiplayerButtonState()
	{
		do
		{
			RefreshMultiplayerButton();
			yield return null;
		}
		while (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown);
		multiplayerButtonCoroutine = null;
	}

	protected override bool CheckMultiplayerButton(out string tooltip)
	{
		if (!base.CheckMultiplayerButton(out tooltip))
		{
			return false;
		}
		if (Singleton<UIResultsManager>.Instance != null && Singleton<UIResultsManager>.Instance.IsShown)
		{
			tooltip = null;
			return false;
		}
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest && !FFSNetwork.IsOnline)
		{
			tooltip = LocalizationManager.GetTranslation("GUI_START_MULTIPLAYER_WRONG_PHASE_TOOLTIP");
			return false;
		}
		tooltip = null;
		return true;
	}

	public override void LoadMainMenu(bool skipConfirmation = false)
	{
		base.LoadMainMenu(AutoTestController.s_AutoTestCurrentlyLoaded || skipConfirmation);
	}

	private void LoadLevelEditor()
	{
		switch (LevelEditorController.s_Instance.CurrentState)
		{
		case LevelEditorController.ELevelEditorState.PreviewingLoadOwnParty:
		case LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel:
			SaveData.Instance.LoadCustomLevelFromData(SaveData.Instance.Global.CurrentEditorLevelData, LevelEditorController.ELevelEditorState.Editing);
			SceneController.Instance.LevelEditorStart();
			break;
		case LevelEditorController.ELevelEditorState.Idle:
		case LevelEditorController.ELevelEditorState.Editing:
			break;
		}
	}

	protected override void OnLoadMainMenuConfirmed()
	{
		if (LevelEventsController.s_EventsControllerActive)
		{
			Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
		}
		base.OnLoadMainMenuConfirmed();
	}

	public void QuitDungeon()
	{
		myWindow.Hide(instant: true);
		Main.Unpause3DWorld(forceUnpause: true);
		_blocker.Block();
		if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster || SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_SCENARIO"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_ABANDON_ADVENTURE_SCENARIO"), delegate
			{
				Choreographer.s_Choreographer.AbandonScenario();
				_blocker.Unblock();
			}, delegate
			{
				_blocker.Unblock();
			}, null, null, showHeader: true, enableSoftlockReport: false, NavigationOperation.ToPreviousNonMenuState, resetAfterAction: true);
			return;
		}
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_EXIT_SCENARIO"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_ABANDON_SINGLE_SCENARIO"), delegate
		{
			if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor || SaveData.Instance.Global.CurrentlyPlayingCustomLevel)
			{
				LoadMainMenu(skipConfirmation: true);
			}
			else
			{
				Choreographer.s_Choreographer.AbandonScenario();
			}
			_blocker.Unblock();
		}, delegate
		{
			_blocker.Unblock();
		}, null, null, showHeader: true, enableSoftlockReport: false, NavigationOperation.ToPreviousNonMenuState, resetAfterAction: true);
	}

	public void RestartScenario()
	{
		if (LevelEventsController.s_EventsControllerActive)
		{
			Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
		}
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_MESSAGE"), delegate
		{
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: false);
			SimpleLog.AddToSimpleLog("User restarted the Scenario");
			SceneController.Instance.RestartScenarioFromInitial();
		}, delegate
		{
			restartRoundButton.Deselect();
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
		}, null, null, showHeader: true, enableSoftlockReport: false, null, resetAfterAction: true);
	}

	public void QueueRestartRound()
	{
		if (SaveData.Instance.IsSavingThreadActive)
		{
			return;
		}
		if (LevelEventsController.s_EventsControllerActive)
		{
			Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
		}
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_RESTART_MESSAGE"), delegate
		{
			restartRoundButton.Deselect();
			if (Singleton<UIResultsManager>.Instance.IsShown)
			{
				Singleton<UIResultsManager>.Instance.Hide();
				if (FFSNetwork.IsOnline)
				{
					Synchronizer.SendSideAction(GameActionType.RestartRound);
				}
				Choreographer.s_Choreographer.RestartRound();
			}
			else
			{
				if (FFSNetwork.IsOnline)
				{
					Synchronizer.SendSideAction(GameActionType.RestartRoundMessage);
				}
				Singleton<StoryController>.Instance.Clear();
				Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
				ScenarioRuleClient.MessageHandler(new CRestartRound_MessageData());
			}
		}, delegate
		{
			restartRoundButton.Deselect();
			Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: true);
		}, null, null, showHeader: true, enableSoftlockReport: false, null, resetAfterAction: true);
	}

	public void RestartTurn()
	{
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_RESTART_TURN"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_GUI_RESTART_TURN"), delegate
		{
			SimpleLog.AddToSimpleLog("User restarted the turn");
		}, delegate
		{
			restartTurnButton.Deselect();
		}, null, null, showHeader: true, enableSoftlockReport: false, null, resetAfterAction: true);
	}

	public void SkipMission()
	{
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_SKIP_MISSION"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_SKIP_MISSION"), delegate
		{
			Choreographer.s_Choreographer.EndGame(EResult.Win);
		}, delegate
		{
			skipMissionButton.Deselect();
		}, null, null, showHeader: true, enableSoftlockReport: false, null, resetAfterAction: true);
	}

	public void SkipTutorial()
	{
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_SKIP_FTUE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_SKIP_FTUE"), delegate
		{
			Choreographer.s_Choreographer.EndGameAndSkipTutorial();
		}, delegate
		{
			skipTutorialButton.Deselect();
		}, null, null, showHeader: true, enableSoftlockReport: false, null, resetAfterAction: true);
	}

	public void Focus()
	{
		controllerArea.Focus();
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.ScenarioEscMenu);
	}
}
