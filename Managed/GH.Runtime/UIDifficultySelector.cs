using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Locations;
using Photon.Bolt;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

public class UIDifficultySelector : MonoBehaviour
{
	private enum ECommitChangesMode
	{
		OnSelectDifficulty,
		OnHide
	}

	private class DifficultyOption
	{
		private UIOptionEntry m_OptionUI;

		public CAdventureDifficulty Difficulty { get; private set; }

		public DifficultyOption(CAdventureDifficulty difficulty, UIOptionEntry optionUI)
		{
			Difficulty = difficulty;
			m_OptionUI = optionUI;
			optionUI.Init(difficulty.Text);
		}

		public void CheckSelected(EAdventureDifficulty selectedDifficulty)
		{
			m_OptionUI.Enable(Difficulty.ActiveOn.Contains(selectedDifficulty));
		}
	}

	public delegate void DifficultySelectedEventHandler(object sender, DifficultySelectedEventArgs eventArgs);

	public class DifficultySelectedEventArgs : EventArgs
	{
		public EAdventureDifficulty SelectedDifficulty { get; private set; }

		public DifficultySelectedEventArgs(EAdventureDifficulty selectedDifficulty)
		{
			SelectedDifficulty = selectedDifficulty;
		}
	}

	public class DifficultyComparer : IEqualityComparer<CAdventureDifficulty>
	{
		public bool Equals(CAdventureDifficulty x, CAdventureDifficulty y)
		{
			return x?.Text == y?.Text;
		}

		public int GetHashCode(CAdventureDifficulty obj)
		{
			return obj.Text.GetHashCode();
		}
	}

	[SerializeField]
	private UITab m_Friendly;

	[SerializeField]
	private UITab m_Easy;

	[SerializeField]
	private UITab m_Normal;

	[SerializeField]
	private UITab m_Hard;

	[SerializeField]
	private UITab m_Insane;

	[SerializeField]
	private UITab m_Deadly;

	[SerializeField]
	private List<UIOptionEntry> m_DifficultyEntryPool;

	[SerializeField]
	private EGameMode m_GameMode;

	[SerializeField]
	private ECommitChangesMode commitChangesMode = ECommitChangesMode.OnHide;

	[SerializeField]
	private UiNavigationGroup m_uiNavigationGroup;

	private List<DifficultyOption> m_DifficultyEntries;

	public EAdventureDifficulty SelectedDifficulty { get; private set; }

	public event DifficultySelectedEventHandler DifficultySelected;

	private void Awake()
	{
		m_Friendly.onValueChanged.AddListener(OnValueChanged_Friendly);
		m_Easy.onValueChanged.AddListener(OnValueChanged_Easy);
		m_Normal.onValueChanged.AddListener(OnValueChanged_Normal);
		m_Hard.onValueChanged.AddListener(OnValueChanged_Hard);
		m_Insane.onValueChanged.AddListener(OnValueChanged_Insane);
		m_Deadly.onValueChanged.AddListener(OnValueChanged_Deadly);
		m_DifficultyEntries = new List<DifficultyOption>();
	}

	private void SelectNext()
	{
		UITab tab = GetTab(SelectedDifficulty, next: true);
		if (tab.IsInteractable())
		{
			tab.isOn = true;
		}
	}

	private void SelectPrevious()
	{
		UITab tab = GetTab(SelectedDifficulty, next: false);
		if (tab.IsInteractable())
		{
			tab.isOn = true;
		}
	}

	private UITab GetTab(EAdventureDifficulty difficulty, bool next)
	{
		switch (difficulty)
		{
		case EAdventureDifficulty.Friendly:
			if (!next)
			{
				return m_Deadly;
			}
			return m_Easy;
		case EAdventureDifficulty.Easy:
			if (!next)
			{
				return m_Friendly;
			}
			return m_Normal;
		case EAdventureDifficulty.Normal:
			if (!next)
			{
				return m_Easy;
			}
			return m_Hard;
		case EAdventureDifficulty.Hard:
			if (!next)
			{
				return m_Normal;
			}
			return m_Insane;
		case EAdventureDifficulty.Insane:
			if (!next)
			{
				return m_Hard;
			}
			return m_Deadly;
		case EAdventureDifficulty.Deadly:
			if (!next)
			{
				return m_Insane;
			}
			return m_Friendly;
		default:
			return m_Easy;
		}
	}

	public void ClearSelection()
	{
		DisableAllTabs();
	}

	private void OnEnable()
	{
		if (m_GameMode == EGameMode.None && (SaveData.Instance.Global.GameMode == EGameMode.Campaign || SaveData.Instance.Global.GameMode == EGameMode.Guildmaster))
		{
			m_GameMode = SaveData.Instance.Global.GameMode;
		}
		TryInitialize(GetDifficulties());
		if (!InputManager.GamePadInUse)
		{
			UITab tabByDifficulty = GetTabByDifficulty(SelectedDifficulty);
			if (!(tabByDifficulty == null))
			{
				SetAllTogglesIsOnToFalse();
				tabByDifficulty.isOn = true;
			}
		}
	}

	private void TryInitialize(List<CAdventureDifficulty> difficulties)
	{
		if (difficulties != null)
		{
			DetermineToggleInteractability();
			if (FFSNetwork.IsHost)
			{
				PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
				PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
				PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(DetermineToggleInteractability));
			}
			m_DifficultyEntries.Clear();
			HelperTools.NormalizePool(ref m_DifficultyEntryPool, m_DifficultyEntryPool[0].gameObject, m_DifficultyEntryPool[0].transform.parent, difficulties.Count);
			for (int i = 0; i < difficulties.Count; i++)
			{
				m_DifficultyEntries.Add(new DifficultyOption(difficulties[i], m_DifficultyEntryPool[i]));
			}
			if ((SaveData.Instance.Global.GameMode == EGameMode.Guildmaster || SaveData.Instance.Global.GameMode == EGameMode.Campaign) && SaveData.Instance.Global.CurrentGameState.In(EGameState.Map, EGameState.Scenario))
			{
				SelectedDifficulty = AdventureState.MapState.DifficultySetting;
				DisableAllTabs();
				ToggleDifficulty(SelectedDifficulty);
				return;
			}
			SelectedDifficulty = EAdventureDifficulty.Easy;
			m_Friendly.SetValue(value: false);
			m_Easy.SetValue(value: true);
			m_Normal.SetValue(value: false);
			m_Hard.SetValue(value: false);
			m_Insane.SetValue(value: false);
			m_Deadly.SetValue(value: false);
			SetDifficulty(EAdventureDifficulty.Easy, networkAction: false);
		}
	}

	private List<CAdventureDifficulty> GetDifficulties()
	{
		return MapRuleLibraryClient.MRLYML?.Difficulty?.SingleOrDefault((DifficultyYMLData s) => s.GameMode == m_GameMode.ToString())?.DifficultySettings?.Distinct(new DifficultyComparer()).ToList();
	}

	public void ToggleDifficulty(EAdventureDifficulty difficulty)
	{
		if (m_DifficultyEntries.Count <= 0)
		{
			TryInitialize(GetDifficulties());
		}
		switch (difficulty)
		{
		case EAdventureDifficulty.Friendly:
			m_Easy.SetIsOnWithoutNotify(value: true);
			SetDifficulty(EAdventureDifficulty.Friendly, networkAction: false);
			break;
		case EAdventureDifficulty.Easy:
			m_Easy.SetIsOnWithoutNotify(value: true);
			SetDifficulty(EAdventureDifficulty.Easy, networkAction: false);
			break;
		case EAdventureDifficulty.Normal:
			m_Normal.SetIsOnWithoutNotify(value: true);
			SetDifficulty(EAdventureDifficulty.Normal, networkAction: false);
			break;
		case EAdventureDifficulty.Hard:
			m_Hard.SetIsOnWithoutNotify(value: true);
			SetDifficulty(EAdventureDifficulty.Hard, networkAction: false);
			break;
		case EAdventureDifficulty.Insane:
			m_Insane.SetIsOnWithoutNotify(value: true);
			SetDifficulty(EAdventureDifficulty.Insane, networkAction: false);
			break;
		case EAdventureDifficulty.Deadly:
			m_Deadly.SetIsOnWithoutNotify(value: true);
			SetDifficulty(EAdventureDifficulty.Deadly, networkAction: false);
			break;
		}
	}

	private void OnDisable()
	{
		DisableAllTabs();
		if (FFSNetwork.IsHost)
		{
			PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
			PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
			PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(DetermineToggleInteractability));
			if (FFSNetwork.IsOnline && commitChangesMode == ECommitChangesMode.OnHide && CanChangeDifficulty())
			{
				Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.PickDifficulty, ActionProcessor.CurrentPhase, disableAutoReplication: false, 0, (int)SelectedDifficulty);
			}
		}
	}

	private bool CanChangeDifficulty()
	{
		if (AdventureState.MapState != null && AdventureState.MapState.DifficultySetting == SelectedDifficulty)
		{
			return false;
		}
		if (Singleton<MapChoreographer>.Instance != null && !Singleton<MapChoreographer>.Instance.PartyAtHQ)
		{
			return false;
		}
		return true;
	}

	private void OnValueChanged_Friendly(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Friendly);
		}
	}

	private void OnValueChanged_Easy(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Easy);
		}
	}

	private void OnValueChanged_Normal(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Normal);
		}
	}

	private void OnValueChanged_Hard(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Hard);
		}
	}

	private void OnValueChanged_Insane(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Insane);
		}
	}

	private void OnValueChanged_Deadly(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Deadly);
		}
	}

	private void SetDifficulty(EAdventureDifficulty difficultySetting, bool networkAction = true)
	{
		SelectedDifficulty = difficultySetting;
		foreach (DifficultyOption difficultyEntry in m_DifficultyEntries)
		{
			difficultyEntry.CheckSelected(SelectedDifficulty);
		}
		SetUiNavigation(difficultySetting);
		if (CanChangeDifficulty())
		{
			if (!FFSNetwork.IsOnline)
			{
				this.DifficultySelected?.Invoke(this, new DifficultySelectedEventArgs(SelectedDifficulty));
			}
			else if (FFSNetwork.IsHost && networkAction && commitChangesMode == ECommitChangesMode.OnSelectDifficulty)
			{
				Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.PickDifficulty, ActionPhaseType.MapHQ, disableAutoReplication: false, 0, (int)SelectedDifficulty);
			}
		}
	}

	private void SetUiNavigation(EAdventureDifficulty difficultySetting)
	{
		UITab tabByDifficulty = GetTabByDifficulty(difficultySetting);
		if (!(tabByDifficulty == null))
		{
			UINavigationSelectable component = tabByDifficulty.GetComponent<UINavigationSelectable>();
			if (!(component == null) && !(m_uiNavigationGroup == null))
			{
				m_uiNavigationGroup.SetDefaultElementToSelect(component);
			}
		}
	}

	private UITab GetTabByDifficulty(EAdventureDifficulty difficultySetting)
	{
		return difficultySetting switch
		{
			EAdventureDifficulty.Friendly => m_Friendly, 
			EAdventureDifficulty.Easy => m_Easy, 
			EAdventureDifficulty.Normal => m_Normal, 
			EAdventureDifficulty.Hard => m_Hard, 
			EAdventureDifficulty.Insane => m_Insane, 
			EAdventureDifficulty.Deadly => m_Deadly, 
			_ => null, 
		};
	}

	private void DisableAllTabs()
	{
		m_Easy.group.SetAllTogglesOff(sendCallback: false);
		m_Friendly.group.SetAllTogglesOff(sendCallback: false);
		m_Normal.group.SetAllTogglesOff(sendCallback: false);
		m_Hard.group.SetAllTogglesOff(sendCallback: false);
		m_Deadly.group.SetAllTogglesOff(sendCallback: false);
		m_Insane.group.SetAllTogglesOff(sendCallback: false);
	}

	private void SetAllTogglesIsOnToFalse()
	{
		m_Easy.isOn = false;
		m_Friendly.isOn = false;
		m_Normal.isOn = false;
		m_Hard.isOn = false;
		m_Deadly.isOn = false;
		m_Insane.isOn = false;
	}

	private void DetermineToggleInteractability()
	{
		bool flag = (SaveData.Instance.Global.CurrentGameState == EGameState.None || (Singleton<MapChoreographer>.Instance != null && Singleton<MapChoreographer>.Instance.PartyAtHQ)) && ((!FFSNetwork.IsHost) ? (!FFSNetwork.IsOnline) : (PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.ConnectingUsers.Count == 0));
		UITab easy = m_Easy;
		UITab normal = m_Normal;
		UITab hard = m_Hard;
		UITab insane = m_Insane;
		UITab friendly = m_Friendly;
		bool flag2 = (m_Deadly.interactable = flag);
		bool flag4 = (friendly.interactable = flag2);
		bool flag6 = (insane.interactable = flag4);
		bool flag8 = (hard.interactable = flag6);
		bool interactable = (normal.interactable = flag8);
		easy.interactable = interactable;
	}

	public static void ClientSetDifficulty(GameAction action)
	{
		EAdventureDifficulty supplementaryDataIDMin = (EAdventureDifficulty)action.SupplementaryDataIDMin;
		AdventureState.MapState.ChangeDifficulty(supplementaryDataIDMin);
		Singleton<MapChoreographer>.Instance.RegenerateAllMapScenarios(rerollQuestRewards: true);
		Singleton<UIQuestPopupManager>.Instance.ResetQuestPopups();
		SaveData.Instance.Global.CurrentAdventureData.Save();
		FFSNet.Console.LogInfo("Difficulty switched to " + supplementaryDataIDMin);
	}

	private void DetermineToggleInteractability(BoltConnection connection)
	{
		DetermineToggleInteractability();
	}

	private void DetermineToggleInteractability(NetworkPlayer player)
	{
		DetermineToggleInteractability();
	}
}
