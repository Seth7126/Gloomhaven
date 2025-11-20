#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFSNet;
using GLOOM;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow), typeof(ControllerInputArea))]
public class UILoadoutManager : Singleton<UILoadoutManager>
{
	private const string GUIHotkeyEnterDungeon = "Consoles/GUI_HOTKEY_ENTER_DUNGEON";

	[SerializeField]
	private Button confirmationButton;

	[SerializeField]
	private GameObject warningConfirm;

	[SerializeField]
	private string enterDungeonAudioItem = "PlaySound_UIEndTurn";

	[SerializeField]
	private UILoadoutQuestWindow questInfo;

	[SerializeField]
	private GameObject focusMask;

	[SerializeField]
	private LongConfirmHandler _confirmHandler;

	[SerializeField]
	private Hotkey _longConfirmHotKey;

	[SerializeField]
	private HelpBox _helpBox;

	private ControllerInputArea _controllerArea;

	private UIWindow _window;

	private CQuestState _quest;

	private CMapParty _party;

	private Action _confirmationCallback;

	private SimpleKeyActionHandlerBlocker _confirmButtonActiveBlocker;

	public bool IsOpen => _window.IsOpen;

	public UnityEvent OnOpen => _window.onShown;

	public event Action OnShortPressed;

	protected override void Awake()
	{
		base.Awake();
		_window = GetComponent<UIWindow>();
		_controllerArea = GetComponent<ControllerInputArea>();
		if (!InputManager.GamePadInUse)
		{
			confirmationButton.onClick.AddListener(ConfirmEnterScenario);
		}
		else
		{
			_longConfirmHotKey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		_window.onShown.AddListener(OnWindowShown);
		_window.onHidden.AddListener(OnHidden);
		_confirmButtonActiveBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnLongConfirmPressed).AddBlocker(_confirmButtonActiveBlocker).AddBlocker(new UIWindowOpenKeyActionBlocker(_window)));
		}
		SetActiveSinglePlayerLongConfirmButton(active: false);
	}

	private void SetActiveSinglePlayerLongConfirmButton(bool active)
	{
		if (InputManager.GamePadInUse)
		{
			_confirmHandler.SetActiveLongConfirmButton(active);
		}
		confirmationButton.gameObject.SetActive(active);
		_confirmButtonActiveBlocker.SetBlock(!active);
	}

	public void SetActiveConfirmationButton(bool value)
	{
		value &= CanShowConfirmationButton();
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.ToggleVisibility(value);
		}
		else
		{
			SetActiveSinglePlayerLongConfirmButton(value);
		}
	}

	private bool CanShowConfirmationButton()
	{
		NewPartyDisplayUI partyDisplay = NewPartyDisplayUI.PartyDisplay;
		if (partyDisplay.IsOpen && partyDisplay.ActiveDisplay != NewPartyDisplayUI.DisplayType.BATTLE_GOALS)
		{
			return partyDisplay.ActiveDisplay == NewPartyDisplayUI.DisplayType.NONE;
		}
		return true;
	}

	protected override void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			_longConfirmHotKey.Deinitialize();
		}
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnPartyPanelToggled;
		NewPartyDisplayUI.PartyDisplay.OnAbilityDeckUpdated -= RefreshRequirements;
		if (_quest != null)
		{
			_quest.OnBattleGoalsUpdated -= OnUpdateBattleGoal;
		}
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnLongConfirmPressed);
		}
		confirmationButton.onClick.RemoveAllListeners();
		base.OnDestroy();
	}

	public bool InLoadout()
	{
		return _window.IsOpen;
	}

	public void ConfirmEnterScenario()
	{
		if (CheckRequirements(forceShowWarning: true))
		{
			AudioControllerUtils.PlaySound(enterDungeonAudioItem);
			_confirmationCallback?.Invoke();
		}
		else
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
		}
	}

	private void OnLongConfirmPressed()
	{
		if (_confirmHandler.isActiveAndEnabled)
		{
			_confirmHandler.Pressed(ConfirmEnterScenario, OnShortPress);
		}
	}

	private void OnShortPress()
	{
		this.OnShortPressed?.Invoke();
	}

	public bool CheckRequirements(bool forceShowWarning = false)
	{
		List<CMapCharacter> list;
		if (FFSNetwork.IsOnline)
		{
			list = _party.SelectedCharacters.Where((CMapCharacter it) => it.IsUnderMyControl && (!_quest.IsSoloScenario || _quest.SoloScenarioCharacterID == it.CharacterID)).ToList();
		}
		else
		{
			if (_quest.ScenarioState.IsTutorialOrIntroScenario)
			{
				return OnSuccess();
			}
			list = ((!_quest.IsSoloScenario) ? _party.SelectedCharacters : _party.SelectedCharacters.Where((CMapCharacter it) => _quest.SoloScenarioCharacterID == it.CharacterID)).ToList();
		}
		List<CMapCharacter> list2 = list.FindAll((CMapCharacter it) => it.HandAbilityCardIDs.Count != it.MaxCards);
		List<string> list3 = (_quest.IsSoloScenario ? null : (from it in list
			where _quest.GetChosenBattleGoal(it.CharacterID) == null
			select it.CharacterID).ToList());
		if (list2.Count == 0 && list3.IsNullOrEmpty())
		{
			return OnSuccess();
		}
		if (list2.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder(LocalizationManager.GetTranslation(list2[0].CharacterYMLData.LocKey));
			for (int num = 1; num < list2.Count; num++)
			{
				stringBuilder.AppendFormat((num == list2.Count - 1) ? (" " + LocalizationManager.GetTranslation("AND") + " {0}") : ", {0}", LocalizationManager.GetTranslation(list2[num].CharacterYMLData.LocKey));
			}
			string text = string.Format(LocalizationManager.GetTranslation((list2.Count == 1) ? "GUI_WRONG_ABILITY_DECK_CHARACTER" : "GUI_WRONG_ABILITY_DECK_CHARACTERS"), stringBuilder);
			_helpBox.ShowTranslated(text, null, HelpBox.FormatTarget.ALL);
		}
		else if (!list3.IsNullOrEmpty())
		{
			string term = ((!InputManager.GamePadInUse) ? "GUI_BATTLE_GOALS_TIP_CONTROLLER" : "Consoles/GUI_BATTLE_GOALS_TIP_CONTROLLER");
			string localizedTip = LocalizationManager.GetTranslation(term);
			_helpBox.ShowControllerOrKeyboardTip(LocalizationManager.GetTranslation("GUI_BATTLE_GOALS_TIP"), () => Singleton<InputManager>.Instance.LocalizeControls(localizedTip), LocalizationManager.GetTranslation("GUI_BATTLE_GOALS"));
		}
		NewPartyDisplayUI.PartyDisplay.ShowCardsWarning(list2, warningConfirm.activeSelf);
		NewPartyDisplayUI.PartyDisplay.ShowBattleGoalWarning(list3, warningConfirm.activeSelf);
		if (warningConfirm.activeSelf)
		{
			if (!InputManager.GamePadInUse || forceShowWarning)
			{
				_helpBox.HighlightWarning();
			}
		}
		else
		{
			warningConfirm.SetActive(value: true);
		}
		return OnFailure();
		bool OnFailure()
		{
			SetActiveConfirmationButton(value: false);
			return false;
		}
		bool OnSuccess()
		{
			HideConfirmWarning();
			CheckMultiplayerHelp();
			SetActiveConfirmationButton(value: true);
			return true;
		}
	}

	private void CheckMultiplayerHelp()
	{
		if (FFSNetwork.IsOnline)
		{
			_helpBox.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_ENTER_SCENARIO");
		}
	}

	public void HideConfirmWarning()
	{
		_helpBox.Hide();
		warningConfirm.SetActive(value: false);
		NewPartyDisplayUI.PartyDisplay.HideCardsWarning();
		NewPartyDisplayUI.PartyDisplay.HideBattleGoalWarning();
	}

	public void EnterLoadout(CMapParty party, CQuestState quest, Action callback)
	{
		ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.Loadout);
		ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.Loadout);
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.loadout_screen);
		StopAllCoroutines();
		_quest = quest;
		_party = party;
		_confirmationCallback = callback;
		if (InputManager.GamePadInUse)
		{
			SetActiveConfirmationButton(value: false);
		}
		Singleton<UIQuestPopupManager>.Instance.Hide(quest);
		SetFocused(focused: true);
		_window.Show();
		questInfo.Show(quest.Quest, MPSyncState);
		NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
		NewPartyDisplayUI.PartyDisplay.HideLevelUpUI();
		if (quest.IsSoloScenario)
		{
			NewPartyDisplayUI.PartyDisplay.EnableBattleGoalsSelection(quest.SoloScenarioCharacterID, interactable: false, LocalizationManager.GetTranslation("GUI_BG_UNAVAILABLE_IN_SOLO"));
		}
		else
		{
			NewPartyDisplayUI.PartyDisplay.EnableBattleGoalsSelection(enable: true);
		}
	}

	private void MPSyncState()
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIMultiplayerLockOverlay>.Instance.ShowLock(this, "GUI_WAIT_PLAYERS_CONFIRM_TIP", blur: false);
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapLoadoutScreen);
			Singleton<UIReadyToggle>.Instance.Initialize(show: false, null, null, delegate
			{
				Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(this);
				if (FFSNetwork.IsHost)
				{
					FFSNetwork.Manager.HostingEndedEvent.RemoveListener(EnableLoadoutInteraction);
				}
				EnableLoadoutInteraction();
			}, Singleton<UIMapMultiplayerController>.Instance.UpdateReadyPlayer, null, null, "GUI_READY", "GUI_UNREADY", "PlaySound_UIMultiPlayerReady", bringToFront: false, null, null, validateReadyUpOnPlayerLeft: true, UIReadyToggle.EReadyUpType.Player);
			Singleton<UIMapMultiplayerController>.Instance.ShowLoadoutSyncMultiplayer();
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
			if (FFSNetwork.IsHost)
			{
				FFSNetwork.Manager.HostingEndedEvent.RemoveListener(EnableLoadoutInteraction);
				FFSNetwork.Manager.HostingEndedEvent.AddListener(EnableLoadoutInteraction);
				FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
				FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
			}
		}
		else
		{
			EnableLoadoutInteraction();
		}
	}

	private void EnableLoadoutInteraction()
	{
		Debug.LogGUI("EnableLoadoutInteraction");
		_quest.ScenarioState.CheckForNonSerializedInitialScenario();
		Singleton<UIQuestPopupManager>.Instance.ShowQuest(_quest);
		ClearEvents();
		if (_quest.IsSoloScenario)
		{
			DisableSoloCharacters(_quest.SoloScenarioCharacterID);
			NewPartyDisplayUI.PartyDisplay.OnShown.AddListener(NewPartyDisplayUI.PartyDisplay.SelectSoloCharacter);
		}
		else
		{
			NewPartyDisplayUI.PartyDisplay.OnShown.AddListener(AutoselectCharacter);
		}
		NewPartyDisplayUI.PartyDisplay.Show(this);
		NewPartyDisplayUI.PartyDisplay.OnOpenedPanel += OnPartyPanelToggled;
		NewPartyDisplayUI.PartyDisplay.EnableCharacterSelection(enable: false, this);
		NewPartyDisplayUI.PartyDisplay.EnableAssignPlayer(enable: false, this);
		NewPartyDisplayUI.PartyDisplay.EnableResetLevel(enableReset: false);
		NewPartyDisplayUI.PartyDisplay.DisableAssignToEmptySlot();
		NewPartyDisplayUI.PartyDisplay.ShowNewClassNotification(show: false);
		NewPartyDisplayUI.PartyDisplay.OnAbilityDeckUpdated += RefreshRequirements;
		_quest.OnBattleGoalsUpdated += OnUpdateBattleGoal;
		MultiplayerStartup();
		RefreshRequirements();
		if (!(Singleton<UINavigation>.Instance.StateMachine.CurrentState is BattleGoalPickerState) && !(Singleton<UINavigation>.Instance.StateMachine.CurrentState is LevelMessageState))
		{
			if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<LoadoutState>())
			{
				Singleton<UIQuestPopupManager>.Instance.TryFocusQuest();
			}
			else
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.Loadout);
			}
		}
	}

	private void DisableSoloCharacters(string soloScenarioCharacterID)
	{
		if (!soloScenarioCharacterID.IsNullOrEmpty())
		{
			NewPartyDisplayUI.PartyDisplay.HideSlots(_party.SelectedCharacters.Where((CMapCharacter it) => it.CharacterID != soloScenarioCharacterID).ToList());
		}
	}

	private void AutoselectCharacter()
	{
		CMapCharacter cMapCharacter = _party.SelectedCharacters.FirstOrDefault((CMapCharacter it) => (!FFSNetwork.IsOnline || it.IsUnderMyControl) && _quest.GetChosenBattleGoal(it.CharacterID) == null);
		if (cMapCharacter != null)
		{
			string characterID = cMapCharacter.CharacterID;
			NewPartyDisplayUI.PartyDisplay.SelectCharacterById(characterID);
			NewPartyDisplayUI.PartyDisplay.OpenBattleGoalPanel(characterID);
		}
	}

	private void OnDisable()
	{
		OnHidden();
	}

	private void OnWindowShown()
	{
		string translation = LocalizationManager.GetTranslation("Consoles/GUI_HOTKEY_ENTER_DUNGEON");
		if (FFSNetwork.IsHost)
		{
			SetMultiplayerButtonLabelGamepad(translation);
		}
	}

	private void SetMultiplayerButtonLabelGamepad(string label)
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<UIReadyToggle>.Instance.SetLabelTextGamepad(label);
		}
	}

	private void OnHidden()
	{
		ClearEvents();
	}

	private void ClearEvents()
	{
		if (NewPartyDisplayUI.PartyDisplay != null)
		{
			NewPartyDisplayUI.PartyDisplay.OnShown.RemoveListener(AutoselectCharacter);
			NewPartyDisplayUI.PartyDisplay.OnShown.AddListener(NewPartyDisplayUI.PartyDisplay.SelectSoloCharacter);
			NewPartyDisplayUI.PartyDisplay.OnOpenedPanel -= OnPartyPanelToggled;
			NewPartyDisplayUI.PartyDisplay.OnAbilityDeckUpdated -= RefreshRequirements;
		}
		if (_quest != null)
		{
			_quest.OnBattleGoalsUpdated -= OnUpdateBattleGoal;
		}
	}

	private void OnPartyPanelToggled(bool visible, NewPartyDisplayUI.DisplayType display)
	{
		SetFocused(!visible);
		RefreshRequirements();
	}

	private void OnUpdateBattleGoal(string characterID)
	{
		if (!FFSNetwork.IsOnline || _party.SelectedCharacters.Any((CMapCharacter it) => it.CharacterID == characterID && it.IsUnderMyControl))
		{
			RefreshRequirements();
		}
	}

	private void SetFocused(bool focused)
	{
		focusMask.SetActive(!focused);
		if (focused)
		{
			if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<LoadoutState>())
			{
				Singleton<UIQuestPopupManager>.Instance.TryFocusQuest();
			}
			else
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.Loadout);
			}
		}
	}

	private void MultiplayerStartup()
	{
		if (FFSNetwork.IsOnline)
		{
			if (FFSNetwork.IsHost)
			{
				FFSNetwork.Manager.HostingEndedEvent.RemoveListener(EnableLoadoutInteraction);
				FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
				FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
			}
			ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.MapLoadoutScreen);
			if (FFSNetwork.IsClient)
			{
				ControllableRegistry.AllControllables.Where((NetworkControllable w) => !(w.ControllableObject is BenchedCharacter)).ToList().ForEach(delegate(NetworkControllable x)
				{
					x.ApplyState();
				});
			}
			MPConfirmEnterScenario();
		}
		else
		{
			FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
			FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		}
	}

	private void MPConfirmEnterScenario()
	{
		Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
		Singleton<UIReadyToggle>.Instance.Initialize(show: true, delegate
		{
			Singleton<APartyDisplayUI>.Instance.CloseWindows();
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			if (InputManager.GamePadInUse)
			{
				NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
			}
		}, delegate
		{
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			if (InputManager.GamePadInUse)
			{
				NewPartyDisplayUI.PartyDisplay.TabInput.Register();
			}
		}, delegate
		{
			if (FFSNetwork.IsHost)
			{
				ConfirmEnterScenario();
			}
		}, delegate(NetworkPlayer player, bool ready)
		{
			Singleton<UIMapMultiplayerController>.Instance.UpdateReadyPlayer(player, ready);
			bool flag = ready && Singleton<UIReadyToggle>.Instance.PlayersReady.Count == PlayerRegistry.Participants.Count;
			Singleton<UIReadyToggle>.Instance.SetInteractable(!flag);
			if (flag)
			{
				Singleton<UIReadyToggle>.Instance.CancelProgress();
			}
		}, OnShortPress, () => CheckRequirements(), "GUI_LOADOUT_ENTER_SCENARIO", "GUI_CANCEL", "PlaySound_UIMultiPlayerReady", bringToFront: false, delegate
		{
			Singleton<APartyDisplayUI>.Instance.CloseWindows();
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
		}, delegate
		{
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
		});
		Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
		Singleton<UIMapMultiplayerController>.Instance.ShowLoadoutMultiplayer();
	}

	public void RefreshRequirements()
	{
		CheckRequirements();
	}

	private void OnSwitchedToMultiplayer()
	{
		FFSNetwork.Manager.HostingStartedEvent.RemoveListener(OnSwitchedToMultiplayer);
		MPConfirmEnterScenario();
		HideConfirmWarning();
		RefreshRequirements();
		SetActiveSinglePlayerLongConfirmButton(active: false);
		FFSNetwork.Manager.HostingEndedEvent.AddListener(OnSwitchedToSinglePlayer);
	}

	private void OnSwitchedToSinglePlayer()
	{
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(OnSwitchedToSinglePlayer);
		FFSNetwork.Manager.HostingEndedEvent.RemoveListener(EnableLoadoutInteraction);
		if (SaveData.Instance.Global.GameMode != EGameMode.MainMenu)
		{
			NewPartyDisplayUI.PartyDisplay.BattleGoalWindow.SelectionAllowed = true;
			Singleton<UIReadyToggle>.Instance.Reset();
			Singleton<UIMapMultiplayerController>.Instance.HideMultiplayer();
			Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(this);
			HideConfirmWarning();
			RefreshRequirements();
			Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			FFSNetwork.Manager.HostingStartedEvent.AddListener(OnSwitchedToMultiplayer);
		}
	}
}
