#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.Quest;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINewAdventureResultsManager : UIResultsManager
{
	[SerializeField]
	private ExtendedButton _returnButton;

	[SerializeField]
	private HeaderHighlightText header;

	[SerializeField]
	private GameObject buttonsContainer;

	[SerializeField]
	private GameObject resultsPanel;

	[SerializeField]
	private GameObject statsPanel;

	[SerializeField]
	private GUIAnimator showStatsAnimation;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[Header("Achievements")]
	[SerializeField]
	private Transform partyAchievementHolder;

	[SerializeField]
	private UIScenarioAchievement partyAchievementPrefab;

	[Header("Accomplishements")]
	[SerializeField]
	private UIScenarioAccomplishmentsList accomplishments;

	[Header("Battle goals")]
	[SerializeField]
	private UIScenarioBattleGoalResult battleGoalResultSlot;

	protected List<UIScenarioBattleGoalResult> battleGoalResults = new List<UIScenarioBattleGoalResult>();

	[SerializeField]
	private Hotkey _returnHotkey;

	private Action<UnityAction> onFinishedResultAnimation;

	public static UINewAdventureResultsManager Implementation => (UINewAdventureResultsManager)Singleton<UIResultsManager>.Instance;

	private void Start()
	{
		base.gameObject.SetActive(value: true);
		myWindow.onTransitionComplete.AddListener(delegate(UIWindow w, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Shown)
			{
				header.Show(delegate
				{
					if (onFinishedResultAnimation == null)
					{
						ContinueShowStats();
					}
					else
					{
						onFinishedResultAnimation(ContinueShowStats);
					}
				});
			}
		});
		showStatsAnimation.OnAnimationFinished.AddListener(delegate
		{
			DisableStatsPanelBlocker();
			statsPanel.SetActive(value: true);
			Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.EndResults);
			InputManager.RequestEnableInput(this, KeyAction.UI_PAUSE);
			OnFinishedOpenAnimations();
		});
	}

	private void ContinueShowStats()
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			showStatsAnimation.Play();
		}
		else if (!m_InCustomLevelMode && m_Retry != ERetryOptionType.Only)
		{
			showStatsAnimation.Play();
		}
		else
		{
			OnFinishedOpenAnimations();
		}
	}

	private void OnFinishedOpenAnimations()
	{
		buttonsContainer.SetActive(value: true);
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
		}
	}

	public void Show(float delay = 0f, EndGameCallback onButtonClicked = null, EResult result = EResult.None, ERetryOptionType retryOption = ERetryOptionType.None, UnityAction retryCallback = null, List<ScenarioAchievementProgress> progressedAchievements = null, Action<UnityAction> onFinishedResultAnimation = null)
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.Reset();
		}
		base.Show(delay, onButtonClicked, result, retryOption, retryCallback, progressedAchievements);
		this.onFinishedResultAnimation = onFinishedResultAnimation;
		buttonsContainer.SetActive(value: false);
		controllerArea.Enable();
		if (InputManager.GamePadInUse)
		{
			UpdateHotkeyLabel();
		}
		else
		{
			UpdateReturnButtonText();
		}
	}

	public override void Show(float delay = 0f, EndGameCallback onButtonClicked = null, EResult result = EResult.None, ERetryOptionType retryOption = ERetryOptionType.None, UnityAction retryCallback = null, List<ScenarioAchievementProgress> progressedAchievements = null)
	{
		Show(delay, onButtonClicked, result, retryOption, retryCallback, progressedAchievements);
	}

	protected override void Show()
	{
		Hotkeys.Instance.SetActiveState(state: false);
		base.Show();
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			ShowNewAdventureModeResults();
		}
		else if (m_InCustomLevelMode || m_Retry == ERetryOptionType.Only)
		{
			resultsPanel.SetActive(value: true);
			statsPanel.SetActive(value: false);
			ActiveStatsPanelBlocker();
			SetResult(m_Result);
		}
		else
		{
			ShowNewAdventureModeResults();
		}
		if (InputManager.GamePadInUse)
		{
			UpdateHotkeyLabel();
		}
		else
		{
			UpdateReturnButtonText();
		}
		InitiativeTrack.Instance.ToggleSortingOrder(value: false);
	}

	private void UpdateHotkeyLabel()
	{
		if (AdventureState.MapState == null)
		{
			_returnHotkey.ExpectedEvent = "ReturnToMenu";
		}
		else
		{
			_returnHotkey.ExpectedEvent = "ReturnToMap";
		}
		_returnHotkey.UpdateHotkeyLabel();
	}

	private void UpdateReturnButtonText()
	{
		string textLanguageKey = ((AdventureState.MapState != null) ? "CONSOLES/GUI_HOTKEY_RETURN_TO_MAP" : "CONSOLES/GUI_HOTKEY_RETURN_TO_MENU");
		_returnButton.TextLanguageKey = textLanguageKey;
	}

	protected override void Retry()
	{
		CloseResultsPanel();
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsHost)
			{
				Synchronizer.SendGameAction(GameActionType.RetryScenario, ActionPhaseType.ScenarioEnded);
			}
			ShowReadyUpRetryToggles(autoReadyUp: true);
		}
		else
		{
			base.Retry();
		}
	}

	protected override void ReturnToMap()
	{
		CloseResultsPanel();
		if (FFSNetwork.IsOnline && m_Retry == ERetryOptionType.Additional)
		{
			if (FFSNetwork.IsHost)
			{
				Synchronizer.SendGameAction(GameActionType.ExitScenario, ActionPhaseType.ScenarioEnded);
			}
			ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT", autoReadyUp: true);
		}
		else
		{
			base.ReturnToMap();
		}
	}

	private void CloseResultsPanel()
	{
		UnregisterOptions();
		Singleton<UINavigation>.Instance.StateMachine.ToPreviousStateIfPossible();
	}

	public void ProxyRetryScenario(GameAction gameAction)
	{
		if (FFSNetwork.IsClient)
		{
			ShowReadyUpRetryToggles();
			Singleton<UIReadyToggle>.Instance.SetInteractable(buttonsContainer.activeSelf);
		}
	}

	public void ProxyExitScenario(GameAction gameAction)
	{
		if (FFSNetwork.IsClient)
		{
			ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT");
			Singleton<UIReadyToggle>.Instance.SetInteractable(buttonsContainer.activeSelf);
		}
	}

	private void SetResult(EResult result)
	{
		DisableStatsPanelBlocker();
		switch (result)
		{
		case EResult.Win:
			retryOption.SetActive(active: false);
			AudioController.Play("PlaySound_STING_Victory");
			header.SetHighlgihtText("GUI_RESULTS_WIN");
			if (FFSNetwork.IsOnline)
			{
				ShowReadyUpToggles("GUI_RESULT_WIN_EXIT");
			}
			break;
		case EResult.Lose:
			retryOption.SetActive(m_Retry != ERetryOptionType.None);
			returnToMapOption.SetActive(m_Retry != ERetryOptionType.Only);
			AudioController.Play("PlaySound_STING_Defeat");
			header.SetWarningText("GUI_RESULTS_LOSE");
			if (!FFSNetwork.IsOnline)
			{
				break;
			}
			if (m_Retry == ERetryOptionType.Additional)
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioEnded);
				if (FFSNetwork.IsClient)
				{
					ShowClientWaitForHostMessage();
					HideResultOptions();
				}
			}
			else
			{
				ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT");
			}
			break;
		case EResult.Resign:
			retryOption.SetActive(m_Retry != ERetryOptionType.None);
			AudioController.Play("PlaySound_STING_Defeat");
			header.SetWarningText("GUI_RESULTS_LOSE");
			if (FFSNetwork.IsOnline)
			{
				ShowReadyUpToggles("GUI_RESULT_LOSE_EXIT");
			}
			break;
		}
	}

	private void ShowReadyUpToggles(string buttonText, bool autoReadyUp = false)
	{
		ShowReadyUpToggles(buttonText, delegate
		{
			m_EndGameCallback?.Invoke(m_Result);
		}, autoReadyUp);
	}

	private void ShowReadyUpRetryToggles(bool autoReadyUp = false)
	{
		ShowReadyUpToggles("gui_retry", m_OnRetry.Invoke, autoReadyUp);
	}

	private void ShowClientWaitForHostMessage()
	{
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_MULTIPLAYER_TIP_TITLE");
		string translation2 = LocalizationManager.GetTranslation("Consoles/GUI_WAIT_FOR_HOST_TIP");
		Singleton<HelpBox>.Instance.ShowTranslated(translation2, translation);
		Singleton<HelpBox>.Instance.BringToFront();
	}

	private void HideResultOptions()
	{
		returnToMapOption.SetActive(active: false);
		retryOption.SetActive(active: false);
	}

	private void ShowReadyUpToggles(string buttonText, Action callback, bool autoReadyUp = false)
	{
		if (FFSNetwork.IsClient)
		{
			ShowClientWaitForHostMessage();
		}
		HideResultOptions();
		ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ScenarioEnded);
		bool isParticipant = PlayerRegistry.MyPlayer.IsParticipant;
		bool isClient = FFSNetwork.IsClient;
		bool show = isParticipant && !isClient;
		bool bringToFront = isParticipant;
		Singleton<UIReadyToggle>.Instance.Initialize(show, delegate
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
		}, delegate
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
		}, delegate
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
			Singleton<HelpBox>.Instance.Hide();
			ActionProcessor.SetState(ActionProcessorStateType.Halted);
			callback();
		}, delegate(NetworkPlayer player, bool isReady)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.UpdateReadyPlayer(player, isReady);
		}, null, null, buttonText, buttonText, "PlaySound_UIMultiPlayerReady", bringToFront, null, null, validateReadyUpOnPlayerLeft: false, UIReadyToggle.EReadyUpType.Player, EReadyUpToggleStates.NotSet, controllerArea.Id);
		Singleton<UIReadyToggle>.Instance.SetLabelTextGamepad(LocalizationManager.GetTranslation(buttonText));
		if (autoReadyUp || isClient)
		{
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
		}
		if (InputManager.GamePadInUse)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.HideReadyTracker();
		}
		else if (!isClient)
		{
			Singleton<UIScenarioMultiplayerController>.Instance.ShowReadyTracker(bringToFront: true, showExhausted: true);
		}
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
	}

	private void OnControllerChanged(NetworkControllable controllable, NetworkPlayer oldcontroller, NetworkPlayer newcontroller)
	{
		if (Singleton<UIReadyToggle>.Instance.IsVisible != PlayerRegistry.MyPlayer.IsParticipant)
		{
			Singleton<UIReadyToggle>.Instance.ToggleVisibility(PlayerRegistry.MyPlayer.IsParticipant, bringToFront: true);
		}
	}

	public void ShowNewAdventureModeResults()
	{
		resultsPanel.SetActive(value: true);
		statsPanel.SetActive(value: false);
		ActiveStatsPanelBlocker();
		SetResult(m_Result);
		CQuestState questState = AdventureState.MapState?.InProgressQuestState;
		CMapParty cMapParty = AdventureState.MapState?.MapParty;
		if (AdventureState.MapState != null)
		{
			if (m_Result == EResult.Win)
			{
				questState?.CheckBattleGoals();
			}
			string soloID = "";
			CQuestState cQuestState = questState;
			if (cQuestState != null && cQuestState.Quest?.QuestCharacterRequirements?.Count > 0)
			{
				foreach (QuestYML.CQuestCharacterRequirement questCharacterRequirement in questState.Quest.QuestCharacterRequirements)
				{
					if (questCharacterRequirement.RequiredCharacterCount == 1)
					{
						soloID = questCharacterRequirement.RequiredCharacterID;
					}
				}
			}
			AdventureState.MapState.SoloID = soloID;
			AdventureState.MapState.CheckTrophyAchievements(new CScenarioEnded_AchievementTrigger());
			UIScenarioAccomplishmentsList uIScenarioAccomplishmentsList = accomplishments;
			StatsDataStorage statsDataStorage = SaveData.Instance.Global.m_StatsDataStorage;
			CQuestState cQuestState2 = questState;
			uIScenarioAccomplishmentsList.ShowStats(statsDataStorage, (cQuestState2 != null && cQuestState2.IsSoloScenario) ? cMapParty.SelectedCharacters.Where((CMapCharacter it) => it.CharacterID == questState.SoloScenarioCharacterID).ToList() : cMapParty.SelectedCharacters.ToList(), cMapParty.LastScenarioStats, m_Result == EResult.Win, soloID);
			cMapParty.LastScenarioStats.Clear();
		}
		else
		{
			Debug.Log("Skip showing accomplishments cause AdventureState.MapState it's null");
		}
		if (questState != null && !questState.IsSoloScenario)
		{
			partyAchievementHolder.gameObject.SetActive(value: true);
			List<Tuple<string, CBattleGoalState>> battleGoals = questState.ChosenBattleGoalStates;
			HelperTools.NormalizePool(ref battleGoalResults, battleGoalResultSlot.gameObject, partyAchievementHolder, battleGoals.Count);
			int i;
			for (i = 0; i < battleGoals.Count; i++)
			{
				CMapCharacter cMapCharacter = cMapParty.SelectedCharacters.FirstOrDefault((CMapCharacter it) => it.CharacterID == battleGoals[i].Item1);
				if (cMapCharacter == null)
				{
					Debug.LogErrorFormat("Not found battle goal characterId {0} in the party selected characters", battleGoals[i].Item1);
					battleGoalResults[i].gameObject.SetActive(value: false);
				}
				else
				{
					battleGoalResults[i].SetBattleGoal(battleGoals[i].Item2, cMapCharacter, m_Result != EResult.Lose);
				}
			}
		}
		List<ScenarioAchievementProgress> list = null;
		if (m_ProgressedAchievements != null)
		{
			list = (from it in m_ProgressedAchievements
				where it.achievement.Achievement.AchievementType != EAchievementType.Trophy && it.HasProgressed()
				orderby (float)it.achievement.AchievementConditionState.CurrentProgress / (float)it.achievement.AchievementConditionState.TotalConditionsAndTargets descending, it.achievement.Achievement.AchievementOrderId ?? it.achievement.ID
				select it).ToList();
		}
		if (list.IsNullOrEmpty() && battleGoalResults.IsNullOrEmpty())
		{
			partyAchievementHolder.gameObject.SetActive(value: false);
			return;
		}
		HelperTools.NormalizePool(ref partyAchievements, partyAchievementPrefab.gameObject, partyAchievementHolder, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			partyAchievements[num].SetAchievement(list[num]);
		}
	}

	public override void Hide()
	{
		base.Hide();
		OnHidden();
	}

	private void OnHidden()
	{
		if (InitiativeTrack.Instance != null)
		{
			InitiativeTrack.Instance.ToggleSortingOrder(value: true);
		}
		Hotkeys.Instance.SetActiveState(state: true);
		header.Hide();
		showStatsAnimation.Stop(goToEnd: false);
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		controllerArea.Destroy();
	}

	protected override void OnDestroy()
	{
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		OnHidden();
		base.OnDestroy();
	}

	public void ActiveStatsPanelBlocker()
	{
		OnStatsPanelStateChanged(active: true);
	}

	public void DisableStatsPanelBlocker()
	{
		OnStatsPanelStateChanged(active: false);
	}

	private void OnStatsPanelStateChanged(bool active)
	{
		returnToMapOption.HandleStatsPanelStateChanged(active);
		retryOption.HandleStatsPanelStateChanged(active);
	}
}
