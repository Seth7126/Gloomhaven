#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using GLOOM;
using I2.Loc;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.YML.Quest;
using Photon.Bolt;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;
using UnityEngine;
using UnityEngine.UI;

public class UIMapMultiplayerController : Singleton<UIMapMultiplayerController>
{
	[SerializeField]
	private UIReadyTrackerBar trackerBar;

	[SerializeField]
	private Button cancelQuestButton;

	private MapLocation hostSelectedLocation;

	private EReadyUpToggleStates m_State;

	private UIGuildmasterConfirmActionPresenter GuildmasterConfirmAction => Singleton<UIGuildmasterHUD>.Instance.ConfirmActionPresenter;

	public CQuestState HostSelectedQuest => hostSelectedLocation?.LocationQuest;

	public bool IsReadyRewards => m_State == EReadyUpToggleStates.Reward;

	public bool IsReadyTownRecords => m_State == EReadyUpToggleStates.TownRecords;

	protected override void Awake()
	{
		base.Awake();
		cancelQuestButton.onClick.AddListener(Cancel);
		cancelQuestButton.gameObject.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
		cancelQuestButton.onClick.RemoveAllListeners();
		base.OnDestroy();
	}

	protected void OnEnable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	protected void OnDisable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	public void ShowMapMultiplayer()
	{
		RefreshWaitingNotifications();
		if (InQuestSelectionPhase())
		{
			if (FFSNetwork.IsHost)
			{
				Singleton<AdventureMapUIManager>.Instance.RefreshTravelOptions();
				if (Singleton<AdventureMapUIManager>.Instance.LocationToTravel != null)
				{
					OnSelectedLocation();
				}
				RefreshHostOptions();
			}
			else
			{
				Singleton<UIGuildmasterHUD>.Instance.EnableCityEncounter(this, enable: false);
			}
		}
		ValidateCharactersAssignedToSlots();
	}

	public void RefreshHostOptions()
	{
		if (FFSNetwork.IsHost)
		{
			bool enable = PlayerRegistry.AllPlayers.Count > 1 && PlayerRegistry.AllPlayers.All((NetworkPlayer x) => x.IsParticipant) && PlayerRegistry.JoiningPlayers.Count == 0;
			Singleton<UIGuildmasterHUD>.Instance.EnableCityEncounter(this, enable);
			Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		}
	}

	public void ShowLoadoutMultiplayer()
	{
		RefreshWaitingNotifications();
		ShowCharactersTrackers();
	}

	public void ShowLoadoutSyncMultiplayer()
	{
		RefreshWaitingNotifications();
		ShowPlayersTrackers();
	}

	public void ShowRewardsMultiplayer(Action onAllPlayersReady)
	{
		m_State = EReadyUpToggleStates.Reward;
		Singleton<UIGuildmasterHUD>.Instance.Hide(this);
		Singleton<UIMultiplayerLockOverlay>.Instance.ShowLock(EReadyUpToggleStates.Reward, "GUI_WAIT_PLAYERS_CONFIRM_TIP", blur: false);
		ShowPlayersTrackers();
		Singleton<UIReadyToggle>.Instance.Initialize(show: true, delegate
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: false);
		}, delegate
		{
			Singleton<UIReadyToggle>.Instance.SetInteractable(interactable: true);
		}, delegate
		{
			HideRewardsMultiplayer();
			onAllPlayersReady();
		}, delegate(NetworkPlayer player, bool isReady)
		{
			UpdateReadyTracker(player, isReady);
		}, null, null, "GUI_READY", "GUI_CANCEL", "PlaySound_UIMultiPlayerReady", bringToFront: true, null, null, validateReadyUpOnPlayerLeft: true, UIReadyToggle.EReadyUpType.Player, EReadyUpToggleStates.Reward);
		CoroutineHelper.RunDelayedAction(0.5f, delegate
		{
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
		});
	}

	public void HideRewardsMultiplayer()
	{
		Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(EReadyUpToggleStates.Reward);
		if (m_State == EReadyUpToggleStates.Reward)
		{
			m_State = EReadyUpToggleStates.NotSet;
			trackerBar.HideTrackers();
			Singleton<UIGuildmasterHUD>.Instance.Show(this);
		}
	}

	public void ShowRetirementMultiplayer()
	{
		Singleton<UIMultiplayerLockOverlay>.Instance.ShowLock(EReadyUpToggleStates.Retirement, "GUI_WAIT_PLAYERS_CONFIRM_TIP", blur: false);
		m_State = EReadyUpToggleStates.Retirement;
		ShowPlayersTrackers();
	}

	public void HideRetirementMultiplayer()
	{
		Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(EReadyUpToggleStates.Retirement);
		if (m_State == EReadyUpToggleStates.Retirement)
		{
			m_State = EReadyUpToggleStates.NotSet;
			trackerBar.HideTrackers();
		}
	}

	public void HideMultiplayer()
	{
		UIMultiplayerNotifications.HideAllMultiplayerNotification();
		if (InQuestSelectionPhase())
		{
			ToggleReadyUpUI(show: false, m_State);
			Singleton<AdventureMapUIManager>.Instance.RefreshTravelOptions();
			Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
			InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
			GuildmasterConfirmAction.ClearAll();
			cancelQuestButton.gameObject.SetActive(value: false);
			Singleton<UIGuildmasterHUD>.Instance.EnableCityEncounter(this, enable: true);
		}
		else
		{
			trackerBar.HideTrackers();
		}
		Singleton<UIMultiplayerLockOverlay>.Instance.ResetLock();
		Singleton<UIGuildmasterHUD>.Instance.Show(this);
	}

	public void OnSelectedLocation()
	{
		if (!FFSNetwork.IsOnline || !FFSNetwork.IsHost || !InQuestSelectionPhase())
		{
			return;
		}
		if (Singleton<AdventureMapUIManager>.Instance.CheckTravel())
		{
			if (PlayerRegistry.AllPlayers.Count <= 1)
			{
				Singleton<HelpBox>.Instance.Show("Consoles/GUI_SELECT_QUEST_HOST_ERROR_TIP", null, null, HelpBox.FormatTarget.ALL);
			}
			else
			{
				Singleton<HelpBox>.Instance.Show("GUI_SELECT_QUEST_TIP", "GUI_SELECT_QUEST");
			}
		}
		Debug.Log($"Number of players: {PlayerRegistry.AllPlayers.Count}");
		MapChoreographer.InitReadyToggle(EReadyUpToggleStates.Quests);
		ToggleReadyUpUI(show: true, EReadyUpToggleStates.Quests);
		Singleton<MapChoreographer>.Instance.DetermineHostToggleInteractability();
		cancelQuestButton.gameObject.SetActive(value: true);
	}

	public void OnSelectedCityEvent()
	{
		if (FFSNetwork.IsOnline && InQuestSelectionPhase())
		{
			MapChoreographer.InitReadyToggle(EReadyUpToggleStates.CityEvents);
			ToggleReadyUpUI(show: true, EReadyUpToggleStates.CityEvents);
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
		}
	}

	public void OnSelectedTownRecords()
	{
		if (FFSNetwork.IsOnline && InQuestSelectionPhase())
		{
			Debug.LogGUI("OnSelectedTownRecords");
			Singleton<UIReadyToggle>.Instance.Initialize(show: true, delegate
			{
				ConfirmTownRecord();
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				OnReady(ready: false);
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, delegate
			{
				ResetUI();
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.TownRecords);
			}, delegate(NetworkPlayer player, bool isReady)
			{
				UpdateReadyPlayer(player, isReady);
			}, null, null, "GUI_READY_TOWN_RECORDS", "GUI_CANCEL_TOWN_RECORDS", "PlaySound_UIMultiQuestSelected", bringToFront: true, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = false;
			}, delegate
			{
				Singleton<APartyDisplayUI>.Instance.EnableInteraction = true;
			}, validateReadyUpOnPlayerLeft: true, UIReadyToggle.EReadyUpType.Participant, EReadyUpToggleStates.TownRecords);
			Singleton<MapChoreographer>.Instance.DeterminePlayerToggleInteractability();
			ToggleReadyUpUI(show: true, EReadyUpToggleStates.TownRecords);
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: true);
		}
	}

	private bool InQuestSelectionPhase()
	{
		if (AdventureState.MapState.CurrentMapPhaseType != EMapPhaseType.InHQ)
		{
			return AdventureState.MapState.CurrentMapPhaseType == EMapPhaseType.AtLinkedScenario;
		}
		return true;
	}

	public void ConfirmSelectedLocation()
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost && InQuestSelectionPhase())
		{
			OnReady(ready: true);
			IProtocolToken supplementaryDataToken = new LocationToken(Singleton<AdventureMapUIManager>.Instance.LocationToTravel.Location.ID);
			Synchronizer.SendGameAction(GameActionType.SelectQuest, ActionPhaseType.MapHQ, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
	}

	public void ConfirmCityEvent()
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost && InQuestSelectionPhase())
		{
			OnReady(ready: true);
			Synchronizer.SendGameAction(GameActionType.SelectCityEvent, ActionPhaseType.MapHQ);
		}
	}

	public void ConfirmTownRecord()
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost && InQuestSelectionPhase())
		{
			OnReady(ready: true);
			Synchronizer.SendGameAction(GameActionType.SelectTownRecords, ActionPhaseType.MapHQ);
		}
	}

	public void OnDeselectedQuest()
	{
		if (FFSNetwork.IsOnline)
		{
			cancelQuestButton.gameObject.SetActive(value: false);
			ToggleReadyUpUI(show: false, EReadyUpToggleStates.Quests);
		}
	}

	public void UpdateReadyPlayer(NetworkPlayer player, bool isReady)
	{
		if (InQuestSelectionPhase())
		{
			if (!player.IsClient && FFSNetwork.IsClient)
			{
				if (isReady)
				{
					UpdateReadyTracker(player, isReady: true);
				}
				else if (m_State == EReadyUpToggleStates.CityEvents)
				{
					ProxyHostCancelledCityEvent();
				}
				else if (m_State == EReadyUpToggleStates.TownRecords)
				{
					ProxyHostCancelledTownRecords();
				}
				else
				{
					ProxyHostCancelledSelectedLocation();
				}
			}
			else
			{
				UpdateReadyTracker(player, isReady);
			}
		}
		else
		{
			UpdateReadyTracker(player, isReady);
		}
	}

	public void UpdateReadyTracker(NetworkPlayer player, bool isReady)
	{
		trackerBar.RefreshReady(player, isReady);
	}

	public void OnReady(bool ready)
	{
		Debug.LogGUI("OnReady (lock options) " + ready + " " + PlayerRegistry.MyPlayer?.Username);
		Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(ready, this);
		if (ready)
		{
			InputManager.RequestDisableInput(this, EKeyActionTag.AreaShortcuts);
		}
		else
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
		}
		if (ready)
		{
			cancelQuestButton.gameObject.SetActive(value: false);
			if (m_State == EReadyUpToggleStates.Quests)
			{
				Singleton<HelpBox>.Instance.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_QUEST");
			}
			else if (m_State == EReadyUpToggleStates.CityEvents)
			{
				Singleton<HelpBox>.Instance.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_CITY_EVENT");
			}
			else if (m_State == EReadyUpToggleStates.TownRecords)
			{
				Singleton<HelpBox>.Instance.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_TOWN_RECORDS");
			}
		}
		else
		{
			cancelQuestButton.gameObject.SetActive(value: true);
			Singleton<HelpBox>.Instance.Hide();
			if (FFSNetwork.IsClient)
			{
				Cancel();
			}
			else if (m_State == EReadyUpToggleStates.CityEvents)
			{
				CancelPreviewCityEvent();
			}
			else if (m_State == EReadyUpToggleStates.TownRecords)
			{
				CancelPreviewTownRecords();
			}
		}
		if (m_State == EReadyUpToggleStates.Quests)
		{
			Singleton<AdventureMapUIManager>.Instance.RefreshTravelOptions();
		}
	}

	private void ShowCharactersTrackers()
	{
		trackerBar.ShowCharactersTrackers(AdventureState.MapState.MapParty.SelectedCharacters.ToList().ConvertAll((CMapCharacter it) => it.CharacterID));
	}

	private void ShowPlayersTrackers()
	{
		trackerBar.ShowPlayersTrackers(PlayerRegistry.AllPlayers.Select((NetworkPlayer it) => it.PlayerID.ToString()).ToList(), (string id) => PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.PlayerID.ToString() == id));
		trackerBar.RefreshReady();
	}

	public void ToggleReadyUpUI(bool show, EReadyUpToggleStates state)
	{
		m_State = state;
		Singleton<UIReadyToggle>.Instance.ToggleVisibility(show);
		if (show)
		{
			ShowCharactersTrackers();
			trackerBar.RefreshReady();
			Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: false);
			if (!InQuestSelectionPhase() || !FFSNetwork.IsClient)
			{
				return;
			}
			switch (state)
			{
			case EReadyUpToggleStates.Quests:
				if (Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer))
				{
					Singleton<HelpBox>.Instance.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_QUEST");
				}
				else
				{
					Singleton<HelpBox>.Instance.Show("GUI_ACCEPT_QUEST_TIP", "GUI_ACCEPT_QUEST");
				}
				break;
			case EReadyUpToggleStates.CityEvents:
				if (Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer))
				{
					Singleton<HelpBox>.Instance.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_CITY_EVENT");
				}
				else
				{
					Singleton<HelpBox>.Instance.Show("GUI_ACCEPT_CITY_EVENT_TIP", "GUI_ACCEPT_CITY_EVENT");
				}
				break;
			case EReadyUpToggleStates.TownRecords:
				if (Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer))
				{
					Singleton<HelpBox>.Instance.Show("GUI_WAIT_PLAYERS_CONFIRM_TIP", "GUI_WAIT_PLAYERS_CONFIRM_TOWN_RECORDS");
				}
				else
				{
					Singleton<HelpBox>.Instance.Show("GUI_ACCEPT_CITY_EVENT_TIP", "GUI_ACCEPT_TOWN_RECORDS");
				}
				break;
			}
		}
		else
		{
			Singleton<HelpBox>.Instance.Hide();
			trackerBar.HideTrackers();
			cancelQuestButton.gameObject.SetActive(value: false);
			if (Singleton<UIGuildmasterHUD>.Instance != null)
			{
				Singleton<UIGuildmasterHUD>.Instance.EnableHeadquartersOptions(this, enableOptions: true);
			}
		}
	}

	public void UpdateCharacterController(NetworkControllable controllable, NetworkPlayer newController)
	{
		string characterID = null;
		string characterName = null;
		if (AdventureState.MapState.IsCampaign)
		{
			CMapCharacter mapCharacterWithCharacterNameHash = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(controllable.ID);
			if (mapCharacterWithCharacterNameHash != null)
			{
				characterName = mapCharacterWithCharacterNameHash.CharacterName;
				characterID = mapCharacterWithCharacterNameHash.CharacterID;
			}
		}
		else
		{
			characterID = CharacterClassManager.GetCharacterIDFromModelInstanceID(controllable.ID);
		}
		trackerBar.RefreshReady(characterID);
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characterID);
		if (!(controllable.ControllableObject is BenchedCharacter))
		{
			UIMultiplayerNotifications.ShowPlayerControlsCharacter(newController, GLOOM.LocalizationManager.GetTranslation(cCharacterClass.LocKey));
		}
		RefreshWaitingNotifications();
		NewPartyDisplayUI.PartyDisplay.RefreshCharacterOwner(characterID, characterName, newController);
		ValidateCharactersAssignedToSlots();
	}

	public void UpdateSlotAssignedToPlayer(NetworkPlayer player, int slot)
	{
		NewPartyDisplayUI.PartyDisplay.CharacterSlots[slot].OnControlAssigned(player);
		ValidateCharactersAssignedToSlots();
	}

	public void UpdateSlotRemovedFromPlayer(NetworkPlayer player, int slot)
	{
		ValidateCharactersAssignedToSlots();
	}

	public void OnPlayerJoined(NetworkPlayer player)
	{
		UIMultiplayerNotifications.ShowPlayerJoined(player);
		List<string> list = new List<string>();
		foreach (NetworkControllable myParticipatingControllable in player.MyParticipatingControllables)
		{
			string item = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(myParticipatingControllable.ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(myParticipatingControllable.ID));
			list.Add(item);
		}
		NewPartyDisplayUI.PartyDisplay.UpdateConnectionStatus(list);
		RefreshWaitingNotifications();
		ValidateCharactersAssignedToSlots();
	}

	public void ValidateCharactersAssignedToSlots()
	{
		if (PlayerRegistry.MyPlayer == null || PlayerRegistry.MyPlayer.AssignedSlots.Count == 0 || PlayerRegistry.MyPlayer.MyParticipatingControllables.Count == 0)
		{
			Singleton<UIGuildmasterHUD>.Instance.Hide(this);
			UIMultiplayerNotifications.HideWaitingAssignAllMySlots();
			if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode.In(EGuildmasterMode.Enchantress, EGuildmasterMode.Merchant, EGuildmasterMode.Temple, EGuildmasterMode.Trainer))
			{
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
			}
		}
		else if (FFSNetwork.IsClient)
		{
			if (PlayerRegistry.MyPlayer.MyParticipatingControllables.Count != PlayerRegistry.MyPlayer.AssignedSlots.Count)
			{
				Singleton<UIGuildmasterHUD>.Instance.Hide(this);
				UIMultiplayerNotifications.ShowWaitingAssignAllMySlots();
				if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode.In(EGuildmasterMode.Enchantress, EGuildmasterMode.Merchant, EGuildmasterMode.Temple, EGuildmasterMode.Trainer))
				{
					Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
				}
			}
			else
			{
				Singleton<UIGuildmasterHUD>.Instance.Show(this);
				UIMultiplayerNotifications.HideWaitingAssignAllMySlots();
			}
		}
		else
		{
			Singleton<UIGuildmasterHUD>.Instance.Show(this);
			if (PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.IsClient && it.MyParticipatingControllables.Count != it.AssignedSlots.Count))
			{
				UIMultiplayerNotifications.ShowWaitingAssignAllMySlots();
			}
			else
			{
				UIMultiplayerNotifications.HideWaitingAssignAllMySlots();
			}
		}
	}

	public void OnPlayerLeft(NetworkPlayer player)
	{
		UIMultiplayerNotifications.ShowPlayerDisconnected(player, showEndSessionOption: false);
		RefreshWaitingNotifications();
		trackerBar.RemoveTracker(player.PlayerID.ToString());
		if (ActionProcessor.CurrentPhase == ActionPhaseType.MapLoadoutScreen && FFSNetwork.IsHost && Singleton<UIReadyToggle>.Instance.ToggledOn && Singleton<UIReadyToggle>.Instance.ReadyUpType != UIReadyToggle.EReadyUpType.Player)
		{
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
		}
		NewPartyDisplayUI.PartyDisplay.UpdateConnectionStatus();
	}

	public void RemoveControllable(NetworkControllable controllable)
	{
		string character = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(controllable.ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(controllable.ID));
		trackerBar.RemoveTracker(character);
		RefreshWaitingNotifications();
	}

	public void OnPartyChanged()
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost && InQuestSelectionPhase() && Singleton<AdventureMapUIManager>.Instance.LocationToTravel != null && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.HostPlayer) && Singleton<AdventureMapUIManager>.Instance.CheckTravel())
		{
			Singleton<HelpBox>.Instance.Show("GUI_SELECT_QUEST_TIP", "GUI_SELECT_QUEST");
		}
	}

	public void RefreshWaitingNotifications()
	{
		if (SceneController.Instance.SelectingPersonalQuest || SceneController.Instance.RetiringCharacter)
		{
			return;
		}
		bool flag;
		if (ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && !SceneController.Instance.BusyProcessingResults && ((FFSNetwork.IsHost && (PlayerRegistry.JoiningPlayers.Count > 0 || PlayerRegistry.AllPlayers.Any((NetworkPlayer w) => w.PlayerID != PlayerRegistry.HostPlayerID && !w.PlayerReadyForAssignment))) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining)))
		{
			flag = true;
			UIMultiplayerNotifications.ShowWaitingPlayersJoining();
			if (PlayerRegistry.MyPlayer.IsCreatingCharacter)
			{
				Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(this);
			}
			else
			{
				Singleton<UIMultiplayerLockOverlay>.Instance.ShowLock(this, null);
				Hotkeys.Instance.SetActiveState(state: false);
			}
		}
		else
		{
			flag = false;
			UIMultiplayerNotifications.HideWaitingPlayersJoining();
			Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(this);
			Hotkeys.Instance.SetActiveState(state: true);
		}
		if (!flag && PlayerRegistry.AllPlayers.Count > 1 && PlayerRegistry.AllPlayers.Exists((NetworkPlayer x) => !x.IsParticipant))
		{
			UIMultiplayerNotifications.ShowWaitingPlayersCharacterAssigned(PlayerRegistry.AllPlayers.Where((NetworkPlayer x) => !x.IsParticipant).ToList());
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingPlayersCharacterAssigned();
		}
		if (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count == 1)
		{
			UIMultiplayerNotifications.ShowWaitingPlayersJoin();
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingPlayersJoin();
		}
	}

	private void Cancel()
	{
		if (m_State == EReadyUpToggleStates.CityEvents)
		{
			CancelPreviewCityEvent();
		}
		else if (m_State == EReadyUpToggleStates.TownRecords)
		{
			CancelPreviewTownRecords();
		}
		else
		{
			CancelPreviewedQuest();
		}
	}

	private void CancelPreviewCityEvent()
	{
		cancelQuestButton.gameObject.SetActive(value: false);
		ToggleReadyUpUI(show: false, EReadyUpToggleStates.CityEvents);
	}

	private void CancelPreviewTownRecords()
	{
		cancelQuestButton.gameObject.SetActive(value: false);
		ToggleReadyUpUI(show: false, EReadyUpToggleStates.NotSet);
	}

	private void CancelPreviewedQuest(bool force = false)
	{
		if (FFSNetwork.IsHost)
		{
			Singleton<AdventureMapUIManager>.Instance.DeselectCurrentMapLocation();
			return;
		}
		if (hostSelectedLocation != null && (force || IsShowingHostQuestToClient()))
		{
			hostSelectedLocation.ForceHighlight(force: false);
			hostSelectedLocation.Deselect();
		}
		ToggleReadyUpUI(show: false, EReadyUpToggleStates.Quests);
	}

	private void PreviewQuest(bool isCancellable = true)
	{
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.WorldMap && Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.City)
		{
			if (AdventureState.MapState.IsCampaign && hostSelectedLocation.LocationQuest.Quest.Type == EQuestType.City)
			{
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.City);
			}
			else
			{
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
			}
		}
		hostSelectedLocation.ForceHighlight(force: true);
		hostSelectedLocation.Select(isHighlighted: false);
		ToggleReadyUpUI(show: true, EReadyUpToggleStates.Quests);
		Singleton<MapChoreographer>.Instance.DeterminePlayerToggleInteractability();
		cancelQuestButton.gameObject.SetActive(isCancellable);
	}

	public bool IsShowingHostQuestToClient()
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
		{
			return Singleton<UIReadyToggle>.Instance.IsVisible;
		}
		return false;
	}

	public bool HasConfirmedQuest()
	{
		if (FFSNetwork.IsOnline && Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer))
		{
			return Singleton<UIReadyToggle>.Instance.IsVisible;
		}
		return false;
	}

	public void ClearHostSelectedQuest(bool force = false)
	{
		if (FFSNetwork.IsClient)
		{
			GuildmasterConfirmAction.HideQuestSelectedAction();
			CancelPreviewedQuest(force);
			Singleton<AdventureMapUIManager>.Instance.OnCancelTravelButtonClick();
			hostSelectedLocation = null;
		}
	}

	public void ClearHostSelectedCityEvent()
	{
		if (FFSNetwork.IsClient && m_State == EReadyUpToggleStates.CityEvents)
		{
			GuildmasterConfirmAction.HideCityEncounterAction();
			Singleton<UIGuildmasterHUD>.Instance.EnableCityEncounter(this, enable: false);
			CancelPreviewCityEvent();
		}
	}

	public void ClearHostSelectedTownRecords()
	{
		if (FFSNetwork.IsClient && m_State == EReadyUpToggleStates.TownRecords)
		{
			CancelPreviewTownRecords();
			Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		}
	}

	public void ResetUI()
	{
		ClearHostSelectedQuest(force: true);
		ClearHostSelectedCityEvent();
		ClearHostSelectedTownRecords();
		ToggleReadyUpUI(show: false, EReadyUpToggleStates.NotSet);
		Singleton<AdventureMapUIManager>.Instance.RefreshTravelOptions();
		Singleton<AdventureMapUIManager>.Instance.LockOptionsInteraction(locked: false, this);
		InputManager.RequestEnableInput(this, EKeyActionTag.AreaShortcuts);
	}

	private void ProxyHostCancelledSelectedLocation()
	{
		if (FFSNetwork.IsClient)
		{
			FFSNet.Console.LogInfo("Host cancelled selected quest");
			if (Singleton<UIReadyToggle>.Instance.ToggledOn)
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
			ClearHostSelectedQuest();
		}
	}

	public void ProxyHostSelectedLocation(MapLocation location)
	{
		if (!FFSNetwork.IsClient)
		{
			return;
		}
		UIMultiplayerNotifications.ShowSelectedQuest();
		if (hostSelectedLocation != null)
		{
			ProxyHostCancelledSelectedLocation();
		}
		hostSelectedLocation = location;
		if (!Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption())
		{
			GuildmasterConfirmAction.ShowQuestSelectedAction(location.LocationQuest, delegate
			{
				PreviewQuest();
			});
		}
		else
		{
			GuildmasterConfirmAction.HideQuestSelectedAction();
			PreviewQuest(isCancellable: false);
		}
	}

	public void ProxyHostSelectedCityEvent()
	{
		if (FFSNetwork.IsClient)
		{
			m_State = EReadyUpToggleStates.CityEvents;
			GuildmasterConfirmAction.ShowCityEncounterAction(delegate
			{
				Singleton<UIGuildmasterHUD>.Instance.OpenCityEncounter();
			});
			Singleton<UIGuildmasterHUD>.Instance.EnableCityEncounter(this, enable: true);
		}
	}

	private void ProxyHostCancelledCityEvent()
	{
		if (FFSNetwork.IsClient)
		{
			FFSNet.Console.LogInfo("Host cancelled city event");
			if (Singleton<UIReadyToggle>.Instance.ToggledOn)
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
			UIMultiplayerNotifications.HideSelectedCityEvent();
			ClearHostSelectedCityEvent();
		}
	}

	public void ProxyHostSelectedTownRecords()
	{
		if (FFSNetwork.IsClient)
		{
			m_State = EReadyUpToggleStates.TownRecords;
			UIMultiplayerNotifications.ShowSelectedTownRecords();
			Singleton<UIGuildmasterHUD>.Instance.RefreshUnlockedOptions();
		}
	}

	private void ProxyHostCancelledTownRecords()
	{
		if (FFSNetwork.IsClient)
		{
			FFSNet.Console.LogInfo("Host cancelled town records");
			if (Singleton<UIReadyToggle>.Instance.ToggledOn)
			{
				Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			}
			UIMultiplayerNotifications.HideSelectedTownRecords();
			ClearHostSelectedTownRecords();
		}
	}

	public bool CanCancelSelectedLocation(MapLocation locationToDeselect)
	{
		if (!FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			return true;
		}
		if (Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption() && hostSelectedLocation == locationToDeselect)
		{
			return false;
		}
		return true;
	}

	public ICallbackPromise ConfirmRetirement(CMapCharacter character, NetworkPlayer player, bool isOptional = true)
	{
		Debug.Log("ConfirmRetirement " + character.CharacterID + " " + m_State);
		if (m_State != EReadyUpToggleStates.NotSet && m_State != EReadyUpToggleStates.Retirement)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(GLOOM.LocalizationManager.GetTranslation("GUI_RETIREMENT"), GLOOM.LocalizationManager.GetTranslation("GUI_RETIREMENT_CANCEL_SELECTED_QUEST_CONFIRMATION"), "GUI_CLOSE");
			Singleton<UIReadyToggle>.Instance.ReadyUp(toggledOn: false, autoValidateUnreadying: true);
			Cancel();
		}
		m_State = EReadyUpToggleStates.Retirement;
		CallbackPromise callbackPromise;
		if (isOptional && !character.IsUnderMyControl)
		{
			Singleton<UIPersonalQuestResultManager>.Instance.ShowOtherPlayerCompletedQuestNotification(character);
			callbackPromise = new CallbackPromise();
			GuildmasterConfirmAction.ShowCharacterRetiredAction(character, player, callbackPromise.Resolve);
		}
		else
		{
			callbackPromise = CallbackPromise.Resolved();
		}
		return callbackPromise.Then(delegate
		{
			GuildmasterConfirmAction.HideCharacterRetiredAction();
			if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.WorldMap && Singleton<UIGuildmasterHUD>.Instance.CurrentMode != EGuildmasterMode.City)
			{
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
			}
			return Singleton<UIPersonalQuestResultManager>.Instance.PlayerConfirmRetirement(character, new PersonalQuestDTO(character.PersonalQuest));
		});
	}

	private void OnLanguageChanged()
	{
		RefreshWaitingNotifications();
	}
}
