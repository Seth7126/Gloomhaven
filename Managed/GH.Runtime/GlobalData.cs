#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Gloomhaven;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using Script.PlatformLayer;
using SteamDataAttribution.Internal;
using UnityEngine;

[Serializable]
public class GlobalData : ISerializable
{
	[Serializable]
	public class KeyBinding
	{
		public static KeyAction[] KeyActions = (KeyAction[])Enum.GetValues(typeof(KeyAction));

		public static KeyCode[] KeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

		public KeyAction Action { get; set; }

		public KeyCode Code { get; set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Action", Action.ToString());
			info.AddValue("Code", Code.ToString());
		}

		public KeyBinding(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					string name = current.Name;
					if (!(name == "Action"))
					{
						if (name == "Code")
						{
							string codeString = info.GetString("Code");
							Code = KeyCodes.SingleOrDefault((KeyCode s) => s.ToString() == codeString);
						}
					}
					else
					{
						string actionString = info.GetString("Action");
						Action = KeyActions.SingleOrDefault((KeyAction s) => s.ToString() == actionString);
					}
				}
				catch (Exception ex)
				{
					LogUtils.LogError("Exception while trying to deserialize KeyBinding entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public KeyBinding(KeyAction action, KeyCode code)
		{
			Action = action;
			Code = code;
		}
	}

	public delegate void SpeedChangedEventHandler();

	public static readonly char[] UnsupportedCharacters = new char[15]
	{
		'_', '/', '*', '.', '"', '\\', '[', ']', ':', ';',
		'|', ',', '<', '>', '?'
	};

	public int partnerID;

	public int MasterVolume;

	public int MusicVolume;

	public int SFXVolume;

	public int StoryVolume = 80;

	public int HapticVolume = 80;

	public int HapticVibration = 80;

	public int MultiplayerPingVolume = 80;

	public int UIVolume = 80;

	public bool MuteAudioInBackground = true;

	public List<KeyBinding> KeyBindings = new List<KeyBinding>();

	public bool DisabledCombatLog;

	public List<CombatLogFilter> DisabledCombatLogFilters = new List<CombatLogFilter>();

	public string QualityLevel;

	public GraphicProfile CustomQualityProfile;

	public int TargetResolutionWidth = -1;

	public int TargetResolutionHeight = -1;

	public bool TargetFullScreenMode = true;

	public int TargetFrameRate = 60;

	public string LastLaunchedPlatform = string.Empty;

	public bool EnableSecondClickHexToConfirm;

	public bool DisableHoverOutlines = true;

	private bool disableAbilityFocusOutlines = true;

	private bool switchSkipAndUndoButtons;

	public List<string> UsersAcceptedEULA = new List<string>();

	public bool EpicLogin;

	private bool m_SpeedUpToggle;

	private bool m_CanSpeedUp;

	private bool crossplayEnabled;

	private string nsaID = string.Empty;

	public bool InvertYAxis;

	public bool HasVibration = true;

	public float StickSensitivity = 1f;

	public bool MPEndOfTurnCompare;

	public StatsDataStorage m_StatsDataStorage = new StatsDataStorage();

	public string CurrentHostAccountID;

	public string CurrentHostNetworkAccountID;

	public string ResumeCampaignName;

	public string ResumeModdedCampaignName;

	public List<PartyAdventureData> AllCampaigns;

	public string ResumeAdventureName;

	public string ResumeModdedAdventureName;

	private List<PartyAdventureData> m_AllAdventures;

	public string ResumeSingleScenarioName;

	public List<PartyAdventureData> AllSingleScenarios;

	public string CurrentModdedRuleset;

	public bool SteamDataSuiteAttributionDone;

	private string m_ModdingDirectory;

	[NonSerialized]
	public CCustomLevelData CurrentCustomLevelData;

	public bool CurrentlyPlayingCustomLevel;

	[NonSerialized]
	public string CurrentFrontEndTutorialID;

	[NonSerialized]
	public string CurrentFrontEndTutorialFilename;

	[NonSerialized]
	public CCustomLevelData CurrentEditorLevelData;

	[NonSerialized]
	public AutoTestData CurrentEditorAutoTestData;

	[NonSerialized]
	public AutoTestData CurrentAutoTestDataCopy;

	[NonSerialized]
	public AutoTestData LastLoadedAutoTestData;

	public const int ResetDataVersion = 2;

	private int m_ResetDataVersion;

	public string NsaID
	{
		get
		{
			return nsaID;
		}
		set
		{
			nsaID = value;
		}
	}

	public bool CanSpeedUp => m_CanSpeedUp;

	public bool CrossplayEnabled
	{
		get
		{
			return crossplayEnabled;
		}
		set
		{
			crossplayEnabled = value;
		}
	}

	public bool DebugSpeedMode { get; set; }

	public List<string> CompletedTutorialIDs { get; set; }

	public bool DisableAbilityFocusOutlines
	{
		get
		{
			return disableAbilityFocusOutlines;
		}
		set
		{
			disableAbilityFocusOutlines = value;
			this.DisableAbilityFocusOutlinesChanged?.Invoke(value);
		}
	}

	public bool SwitchSkipAndUndoButtons
	{
		get
		{
			return switchSkipAndUndoButtons;
		}
		set
		{
			switchSkipAndUndoButtons = value;
			this.SwitchSkipAndUndoButtonsChanged?.Invoke(value);
		}
	}

	public bool SpeedUpToggle
	{
		get
		{
			return m_SpeedUpToggle;
		}
		set
		{
			if (m_SpeedUpToggle != value)
			{
				m_SpeedUpToggle = value;
				this.SpeedUpToggleChanged?.Invoke();
			}
			if (!value)
			{
				StopSpeedUp();
			}
			else if (m_CanSpeedUp)
			{
				StartSpeedUp();
			}
		}
	}

	public Dictionary<KeyAction, KeyCode> Keybinds
	{
		get
		{
			Dictionary<KeyAction, KeyCode> dictionary = new Dictionary<KeyAction, KeyCode>();
			foreach (KeyBinding keyBinding in KeyBindings)
			{
				dictionary.Add(keyBinding.Action, keyBinding.Code);
			}
			return dictionary;
		}
		set
		{
			foreach (KeyValuePair<KeyAction, KeyCode> entry in value)
			{
				KeyBinding keyBinding = KeyBindings.SingleOrDefault((KeyBinding s) => s.Action == entry.Key);
				if (keyBinding != null)
				{
					keyBinding.Code = entry.Value;
				}
				else
				{
					KeyBindings.Add(new KeyBinding(entry.Key, entry.Value));
				}
			}
		}
	}

	public string LastAutotestFolder { get; set; }

	public bool UseCustomLevelDataFolder { get; set; }

	public string CustomLevelDataFolder { get; set; }

	public EGameMode GameMode { get; set; }

	public List<PartyAdventureData> ModdedCampaigns
	{
		get
		{
			if (!CurrentModdedRuleset.IsNullOrEmpty())
			{
				return AllCampaigns?.Where((PartyAdventureData w) => w.RulesetName == CurrentModdedRuleset).ToList();
			}
			return new List<PartyAdventureData>();
		}
	}

	public PartyAdventureData CampaignData
	{
		get
		{
			bool flag = !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.IsNullOrEmpty() && !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.Equals("0");
			PartyAdventureData[] array = ((!CurrentModdedRuleset.IsNullOrEmpty()) ? AllCampaigns?.Where((PartyAdventureData w) => w.RulesetName == CurrentModdedRuleset || w.RulesetName + CurrentHostAccountID == CurrentModdedRuleset).ToArray() : AllCampaigns?.Where((PartyAdventureData w) => !w.IsModded).ToArray());
			if (array == null)
			{
				return null;
			}
			if (flag)
			{
				if (Application.platform == RuntimePlatform.Switch)
				{
					try
					{
						return array.Single((PartyAdventureData s) => s.PartyName == ResumeCampaignName && (s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() || s.Owner.PlatformNetworkAccountID.Equals("0") || s.Owner.PlatformNetworkAccountID == CurrentHostNetworkAccountID));
					}
					catch (Exception)
					{
						return null;
					}
				}
				try
				{
					return array.Single((PartyAdventureData s) => s.PartyName == ResumeCampaignName && !s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == CurrentHostNetworkAccountID);
				}
				catch (Exception)
				{
					return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeCampaignName && s.Owner.PlatformAccountID == CurrentHostAccountID);
				}
			}
			return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeCampaignName && s.Owner.PlatformAccountID == CurrentHostAccountID);
		}
	}

	public PartyAdventureData ResumeCampaign
	{
		get
		{
			bool flag = !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.IsNullOrEmpty() && !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.Equals("0");
			PartyAdventureData[] array = ((!CurrentModdedRuleset.IsNullOrEmpty()) ? AllCampaigns?.Where((PartyAdventureData w) => w.RulesetName == CurrentModdedRuleset).ToArray() : AllCampaigns?.Where((PartyAdventureData w) => !w.IsModded).ToArray());
			if (array == null)
			{
				return null;
			}
			if (flag)
			{
				if (Application.platform == RuntimePlatform.Switch)
				{
					try
					{
						return array.Single((PartyAdventureData s) => s.PartyName == ResumeCampaignName && (s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() || s.Owner.PlatformNetworkAccountID.Equals("0") || s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID));
					}
					catch (Exception)
					{
						return null;
					}
				}
				try
				{
					return array.Single((PartyAdventureData s) => s.PartyName == ResumeCampaignName && !s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID);
				}
				catch (Exception)
				{
					return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeCampaignName && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
				}
			}
			return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeCampaignName && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
		}
	}

	public PartyAdventureData[] AllAdventures
	{
		get
		{
			if (string.IsNullOrEmpty(CurrentModdedRuleset))
			{
				return m_AllAdventures?.Where((PartyAdventureData w) => !w.IsModded).ToArray();
			}
			return m_AllAdventures?.Where((PartyAdventureData w) => w.RulesetName == CurrentModdedRuleset).ToArray();
		}
	}

	public PartyAdventureData AdventureData
	{
		get
		{
			bool flag = !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.IsNullOrEmpty() && !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.Equals("0");
			PartyAdventureData[] array = ((!CurrentModdedRuleset.IsNullOrEmpty()) ? m_AllAdventures?.Where((PartyAdventureData w) => w.RulesetName == CurrentModdedRuleset || w.RulesetName + CurrentHostAccountID == CurrentModdedRuleset).ToArray() : m_AllAdventures?.Where((PartyAdventureData w) => !w.IsModded).ToArray());
			if (array == null)
			{
				return null;
			}
			if (flag)
			{
				if (Application.platform == RuntimePlatform.Switch)
				{
					try
					{
						return array.Single((PartyAdventureData s) => s.PartyName == ResumeAdventureName && (s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() || s.Owner.PlatformNetworkAccountID.Equals("0") || s.Owner.PlatformNetworkAccountID == CurrentHostNetworkAccountID));
					}
					catch (Exception)
					{
						return null;
					}
				}
				try
				{
					return array.Single((PartyAdventureData s) => s.PartyName == ResumeAdventureName && !s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == CurrentHostNetworkAccountID);
				}
				catch (Exception)
				{
					return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeAdventureName && s.Owner.PlatformAccountID == CurrentHostAccountID);
				}
			}
			return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeAdventureName && s.Owner.PlatformAccountID == CurrentHostAccountID);
		}
	}

	public PartyAdventureData ResumeAdventure
	{
		get
		{
			bool flag = !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.IsNullOrEmpty() && !PlatformLayer.UserData.PlatformNetworkAccountPlayerID.Equals("0");
			PartyAdventureData[] array = ((!CurrentModdedRuleset.IsNullOrEmpty()) ? m_AllAdventures?.Where((PartyAdventureData w) => w.RulesetName == CurrentModdedRuleset).ToArray() : m_AllAdventures?.Where((PartyAdventureData w) => !w.IsModded).ToArray());
			if (array == null)
			{
				return null;
			}
			if (flag)
			{
				if (Application.platform == RuntimePlatform.Switch)
				{
					try
					{
						return array.Single((PartyAdventureData s) => s.PartyName == ResumeAdventureName && (s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() || s.Owner.PlatformNetworkAccountID.Equals("0") || s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID));
					}
					catch (Exception)
					{
						return null;
					}
				}
				try
				{
					return array.Single((PartyAdventureData s) => s.PartyName == ResumeAdventureName && !s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID);
				}
				catch (Exception)
				{
					return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeAdventureName && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
				}
			}
			return array.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeAdventureName && s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
		}
	}

	public PartyAdventureData SingleScenarioData
	{
		get
		{
			if (Application.platform == RuntimePlatform.Switch)
			{
				return AllSingleScenarios?.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeSingleScenarioName && (s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() || s.Owner.PlatformNetworkAccountID.Equals("0") || s.Owner.PlatformNetworkAccountID == CurrentHostNetworkAccountID));
			}
			return AllSingleScenarios?.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeSingleScenarioName && ((!s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == CurrentHostNetworkAccountID) || s.Owner.PlatformAccountID == CurrentHostAccountID));
		}
	}

	public PartyAdventureData ResumeSingleScenario
	{
		get
		{
			if (Application.platform == RuntimePlatform.Switch)
			{
				return AllSingleScenarios?.SingleOrDefault((PartyAdventureData s) => s.PartyName == ResumeSingleScenarioName && (s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() || s.Owner.PlatformNetworkAccountID.Equals("0") || s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID));
			}
			return AllSingleScenarios?.SingleOrDefault((PartyAdventureData s) => (s.PartyName == ResumeSingleScenarioName && !s.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && !s.Owner.PlatformNetworkAccountID.Equals("0") && s.Owner.PlatformNetworkAccountID == PlatformLayer.UserData.PlatformNetworkAccountPlayerID) || s.Owner.PlatformAccountID == PlatformLayer.UserData.PlatformAccountID);
		}
	}

	public PartyAdventureData CurrentAdventureData
	{
		get
		{
			switch (GameMode)
			{
			case EGameMode.Campaign:
				return CampaignData;
			case EGameMode.Guildmaster:
				return AdventureData;
			case EGameMode.SingleScenario:
			case EGameMode.FrontEndTutorial:
				return SingleScenarioData;
			default:
				return null;
			}
		}
	}

	public List<GHRuleset> Rulesets { get; private set; }

	public string ModdingDirectory
	{
		get
		{
			if (PlatformLayer.FileSystem.ExistsDirectory(m_ModdingDirectory))
			{
				return m_ModdingDirectory;
			}
			m_ModdingDirectory = SaveData.Instance.PersistentDataPath;
			return m_ModdingDirectory;
		}
		set
		{
			m_ModdingDirectory = value;
		}
	}

	public DLCRegistry.EDLCKey DLCInstalledRecord { get; private set; }

	public static List<string> CompletedCharacters => new List<string>
	{
		"BruteID", "CragheartID", "ScoundrelID", "SpellweaverID", "TinkererID", "MindthiefID", "ElementalistID", "SoothsingerID", "SunkeeperID", "BerserkerID",
		"NightshroudID", "BeastTyrantID", "SummonerID", "QuartermasterID"
	};

	public EGameState CurrentGameState
	{
		get
		{
			if (GameMode == EGameMode.Campaign)
			{
				if (AdventureState.MapState.IsInScenarioPhase)
				{
					return EGameState.Scenario;
				}
				return EGameState.Map;
			}
			if (GameMode == EGameMode.LevelEditor && CurrentEditorLevelData != null)
			{
				return EGameState.Scenario;
			}
			if (GameMode == EGameMode.Guildmaster && AdventureState.MapState != null)
			{
				if (AdventureState.MapState.IsInScenarioPhase)
				{
					return EGameState.Scenario;
				}
				return EGameState.Map;
			}
			if (GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.Autotest || GameMode == EGameMode.FrontEndTutorial)
			{
				return EGameState.Scenario;
			}
			return EGameState.None;
		}
	}

	public event SpeedChangedEventHandler GameSpeedChanged;

	public event SpeedChangedEventHandler SpeedUpToggleChanged;

	public event Action<bool> DisableAbilityFocusOutlinesChanged;

	public event Action<bool> SwitchSkipAndUndoButtonsChanged;

	public void InvokeGameSpeedChanged()
	{
		this.GameSpeedChanged?.Invoke();
	}

	public bool IsJotlDlcSaveExists()
	{
		if (!AllCampaigns.Any((PartyAdventureData campaign) => campaign.DLCEnabled.HasFlag(DLCRegistry.EDLCKey.DLC1)))
		{
			return AllAdventures.Any((PartyAdventureData adventure) => adventure.DLCEnabled.HasFlag(DLCRegistry.EDLCKey.DLC1));
		}
		return true;
	}

	public void StopSpeedUp()
	{
		if (!DebugSpeedMode)
		{
			m_CanSpeedUp = false;
			if (TimeManager.DefaultTimeScale != 1f)
			{
				TimeManager.DefaultTimeScale = 1f;
				this.GameSpeedChanged?.Invoke();
			}
		}
	}

	public void StartSpeedUp()
	{
		if (!DebugSpeedMode)
		{
			m_CanSpeedUp = true;
			if (SpeedUpToggle && TimeManager.DefaultTimeScale != SceneController.Instance.GameSpeedIncreaseAmount)
			{
				TimeManager.DefaultTimeScale = SceneController.Instance.GameSpeedIncreaseAmount;
				this.GameSpeedChanged?.Invoke();
			}
		}
	}

	public void AddAdventureSave(PartyAdventureData data)
	{
		if (m_AllAdventures != null && !m_AllAdventures.Contains(data))
		{
			m_AllAdventures.Add(data);
		}
	}

	public void RemoveAdventureSave(PartyAdventureData data)
	{
		if (m_AllAdventures != null && m_AllAdventures.Contains(data))
		{
			m_AllAdventures.Remove(data);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PartnerID", partnerID);
		info.AddValue("MasterVolume", MasterVolume);
		info.AddValue("MusicVolume", MusicVolume);
		info.AddValue("SFXVolume", SFXVolume);
		info.AddValue("StoryVolume", StoryVolume);
		info.AddValue("UIVolume", UIVolume);
		info.AddValue("MuteAudioInBackground", MuteAudioInBackground);
		info.AddValue("MultiplayerPingVolume", MultiplayerPingVolume);
		info.AddValue("HapticVolume", HapticVolume);
		info.AddValue("HapticVibration", HapticVibration);
		info.AddValue("KeyBindings", KeyBindings);
		info.AddValue("DisabledCombatLogFilters", DisabledCombatLogFilters);
		info.AddValue("DisabledCombatLog", DisabledCombatLog);
		info.AddValue("CustomQualityProfile", CustomQualityProfile);
		info.AddValue("QualityLevel", QualityLevel);
		info.AddValue("TargetResolutionWidth", TargetResolutionWidth);
		info.AddValue("TargetResolutionHeight", TargetResolutionHeight);
		info.AddValue("TargetFullScreenMode", TargetFullScreenMode);
		info.AddValue("TargetFrameRate", TargetFrameRate);
		info.AddValue("LastLaunchedPlatform", LastLaunchedPlatform);
		info.AddValue("EnableSecondClickHexToConfirm", EnableSecondClickHexToConfirm);
		info.AddValue("SwitchSkipAndUndoButtons", switchSkipAndUndoButtons);
		info.AddValue("DisableAbilityFocusOutlines", disableAbilityFocusOutlines);
		info.AddValue("DisableHoverOutlines", DisableHoverOutlines);
		info.AddValue("EpicLogin", EpicLogin);
		info.AddValue("StatsDataStorage", m_StatsDataStorage);
		info.AddValue("ResetDataVersion", m_ResetDataVersion);
		info.AddValue("SteamDataSuiteAttributionDone", SteamDataSuiteAttributionDone);
		info.AddValue("ResumeCampaignName", ResumeCampaignName);
		info.AddValue("ResumeModdedCampaignName", ResumeModdedCampaignName);
		info.AddValue("AllCampaigns", AllCampaigns);
		info.AddValue("ResumeAdventureName", ResumeAdventureName);
		info.AddValue("ResumeModdedAdventureName", ResumeModdedAdventureName);
		info.AddValue("AllAdventures", m_AllAdventures);
		info.AddValue("ResumeSingleScenarioName", ResumeSingleScenarioName);
		info.AddValue("AllSingleScenarios", AllSingleScenarios);
		info.AddValue("LastAutotestFolder", LastAutotestFolder);
		info.AddValue("UseCustomLevelDataFolder", UseCustomLevelDataFolder);
		info.AddValue("CustomLevelDataFolder", CustomLevelDataFolder);
		info.AddValue("MPEndOfTurnCompare", MPEndOfTurnCompare);
		info.AddValue("Rulesets", Rulesets);
		info.AddValue("SpeedUpToggle", m_SpeedUpToggle);
		info.AddValue("CustomModdingDirectory", m_ModdingDirectory);
		info.AddValue("CompletedTutorialIDs", CompletedTutorialIDs);
		info.AddValue("DLCInstalledRecord", DLCInstalledRecord);
		info.AddValue("CrossplayEnabled", crossplayEnabled);
		info.AddValue("UsersAcceptedEULA", UsersAcceptedEULA);
		if (Application.platform == RuntimePlatform.Switch)
		{
			info.AddValue("NSAID", nsaID);
		}
		info.AddValue("InvertYAxis", InvertYAxis);
		info.AddValue("Vibration", HasVibration);
		info.AddValue("StickSensitivity", StickSensitivity);
	}

	private GlobalData(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "PartnerID":
					partnerID = info.GetInt32("PartnerID");
					break;
				case "CrossplayEnabled":
					crossplayEnabled = info.GetBoolean("CrossplayEnabled");
					break;
				case "MasterVolume":
					MasterVolume = info.GetInt32("MasterVolume");
					break;
				case "MusicVolume":
					MusicVolume = info.GetInt32("MusicVolume");
					break;
				case "SFXVolume":
					SFXVolume = info.GetInt32("SFXVolume");
					break;
				case "StoryVolume":
					StoryVolume = info.GetInt32("StoryVolume");
					break;
				case "UIVolume":
					UIVolume = info.GetInt32("UIVolume");
					break;
				case "MultiplayerPingVolume":
					MultiplayerPingVolume = info.GetInt32("MultiplayerPingVolume");
					break;
				case "HapticVolume":
					HapticVolume = info.GetInt32("HapticVolume");
					break;
				case "HapticVibration":
					HapticVibration = info.GetInt32("HapticVibration");
					break;
				case "MuteAudioInBackground":
					MuteAudioInBackground = info.GetBoolean("MuteAudioInBackground");
					break;
				case "KeyBindings":
					KeyBindings = (List<KeyBinding>)info.GetValue("KeyBindings", typeof(List<KeyBinding>));
					break;
				case "DisabledCombatLogFilters":
					DisabledCombatLogFilters = ((List<CombatLogFilter>)info.GetValue("DisabledCombatLogFilters", typeof(List<CombatLogFilter>))) ?? new List<CombatLogFilter>();
					break;
				case "DisabledCombatLog":
					DisabledCombatLog = info.GetBoolean("DisabledCombatLog");
					break;
				case "CustomQualityProfile":
					CustomQualityProfile = (GraphicProfile)info.GetValue("CustomQualityProfile", typeof(GraphicProfile));
					break;
				case "QualityLevel":
					QualityLevel = info.GetString("QualityLevel");
					break;
				case "TargetResolutionHeight":
					TargetResolutionHeight = info.GetInt32("TargetResolutionHeight");
					break;
				case "TargetResolutionWidth":
					TargetResolutionWidth = info.GetInt32("TargetResolutionWidth");
					break;
				case "TargetFullScreenMode":
					TargetFullScreenMode = info.GetBoolean("TargetFullScreenMode");
					break;
				case "TargetFrameRate":
					TargetFrameRate = info.GetInt32("TargetFrameRate");
					break;
				case "LastLaunchedPlatform":
					LastLaunchedPlatform = info.GetString("LastLaunchedPlatform");
					break;
				case "EnableSecondClickHexToConfirm":
					EnableSecondClickHexToConfirm = info.GetBoolean("EnableSecondClickHexToConfirm");
					break;
				case "SwitchSkipAndUndoButtons":
					switchSkipAndUndoButtons = info.GetBoolean("SwitchSkipAndUndoButtons");
					break;
				case "DisableAbilityFocusOutlines":
					disableAbilityFocusOutlines = info.GetBoolean("DisableAbilityFocusOutlines");
					break;
				case "EpicLogin":
					EpicLogin = info.GetBoolean("EpicLogin");
					break;
				case "DisableHoverOutlines":
					DisableHoverOutlines = info.GetBoolean("DisableHoverOutlines");
					break;
				case "StatsDataStorage":
					m_StatsDataStorage = (StatsDataStorage)info.GetValue("StatsDataStorage", typeof(StatsDataStorage));
					break;
				case "ResetDataVersion":
					m_ResetDataVersion = info.GetInt32("ResetDataVersion");
					break;
				case "SteamDataSuiteAttributionDone":
					SteamDataSuiteAttributionDone = info.GetBoolean("SteamDataSuiteAttributionDone");
					break;
				case "ResumeCampaignName":
					ResumeCampaignName = info.GetString("ResumeCampaignName");
					break;
				case "ResumeModdedCampaignName":
					ResumeModdedCampaignName = info.GetString("ResumeModdedCampaignName");
					break;
				case "AllCampaigns":
					AllCampaigns = (List<PartyAdventureData>)info.GetValue("AllCampaigns", typeof(List<PartyAdventureData>));
					break;
				case "ResumeAdventureName":
					ResumeAdventureName = info.GetString("ResumeAdventureName");
					break;
				case "ResumeModdedAdventureName":
					ResumeModdedAdventureName = info.GetString("ResumeModdedAdventureName");
					break;
				case "AllAdventures":
					m_AllAdventures = (List<PartyAdventureData>)info.GetValue("AllAdventures", typeof(List<PartyAdventureData>));
					break;
				case "ResumeSingleScenarioName":
					ResumeSingleScenarioName = info.GetString("ResumeSingleScenarioName");
					break;
				case "AllSingleScenarios":
					AllSingleScenarios = (List<PartyAdventureData>)info.GetValue("AllSingleScenarios", typeof(List<PartyAdventureData>));
					break;
				case "LastAutotestFolder":
					LastAutotestFolder = info.GetString("LastAutotestFolder");
					break;
				case "UseCustomLevelDataFolder":
					UseCustomLevelDataFolder = info.GetBoolean("UseCustomLevelDataFolder");
					break;
				case "CustomLevelDataFolder":
					CustomLevelDataFolder = info.GetString("CustomLevelDataFolder");
					break;
				case "MPEndOfTurnCompare":
					MPEndOfTurnCompare = info.GetBoolean("MPEndOfTurnCompare");
					break;
				case "Rulesets":
					Rulesets = (List<GHRuleset>)info.GetValue("Rulesets", typeof(List<GHRuleset>));
					break;
				case "SpeedUpToggle":
					m_SpeedUpToggle = info.GetBoolean("SpeedUpToggle");
					break;
				case "CustomModdingDirectory":
					m_ModdingDirectory = info.GetString("CustomModdingDirectory");
					break;
				case "CompletedTutorialIDs":
					CompletedTutorialIDs = (List<string>)info.GetValue("CompletedTutorialIDs", typeof(List<string>));
					break;
				case "DLCInstalledRecord":
					DLCInstalledRecord = (DLCRegistry.EDLCKey)info.GetValue("DLCInstalledRecord", typeof(DLCRegistry.EDLCKey));
					break;
				case "UsersAcceptedEULA":
					UsersAcceptedEULA = ((List<string>)info.GetValue("UsersAcceptedEULA", typeof(List<string>))) ?? new List<string>();
					break;
				case "NSAID":
					nsaID = info.GetString("NSAID");
					break;
				case "InvertYAxis":
					InvertYAxis = info.GetBoolean("InvertYAxis");
					break;
				case "HasVibration":
					HasVibration = info.GetBoolean("HasVibration");
					break;
				case "StickSensitivity":
					StickSensitivity = (float)info.GetDouble("StickSensitivity");
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize GlobalData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (Rulesets == null)
		{
			Rulesets = new List<GHRuleset>();
		}
		if (!SteamDataSuiteAttributionDone)
		{
			SteamDataSuiteAttributionDone = true;
		}
		if (AllCampaigns == null)
		{
			AllCampaigns = new List<PartyAdventureData>();
		}
		if (m_AllAdventures == null)
		{
			m_AllAdventures = new List<PartyAdventureData>();
		}
		if (AllSingleScenarios == null)
		{
			AllSingleScenarios = new List<PartyAdventureData>();
		}
		if (m_ModdingDirectory == null || !PlatformLayer.Modding.ModdingSupported || !PlatformLayer.FileSystem.ExistsDirectory(m_ModdingDirectory))
		{
			m_ModdingDirectory = SaveData.Instance.PersistentDataPath;
		}
		if (CompletedTutorialIDs == null)
		{
			CompletedTutorialIDs = new List<string>();
		}
		ValidateAllAdventures();
	}

	private void UpdateOldSaveWithUserID(PartyAdventureData partyData)
	{
		partyData.Owner = new SaveOwner();
		if (PlatformLayer.FileSystem.ExistsFile(partyData.PartyMainSaveFileOld))
		{
			PlatformLayer.FileSystem.MoveFile(partyData.PartyMainSaveFileOld, Path.Combine(partyData.PartySaveDirOld, partyData.PartySaveFileName));
			partyData.AdventureMapStateFilePath = partyData.PartyMainSaveFile;
		}
		foreach (string adventureMapCheckpointsFilepath in partyData.AdventureMapCheckpointsFilepaths)
		{
			if (PlatformLayer.FileSystem.ExistsFile(adventureMapCheckpointsFilepath))
			{
				PlatformLayer.FileSystem.MoveFile(adventureMapCheckpointsFilepath, Path.Combine(partyData.PartyCheckpointDirOld, SaveData.NextAvailableFilename(Path.Combine(partyData.PartyCheckpointDirOld, partyData.PartySaveFileName))));
			}
		}
		if (PlatformLayer.FileSystem.ExistsDirectory(partyData.PartySaveDirOld))
		{
			PlatformLayer.FileSystem.MoveFile(partyData.PartySaveDirOld, partyData.PartySaveDir);
		}
		partyData.IsUserIDSet = true;
	}

	public void ValidateAllAdventures()
	{
		if (PlatformLayer.FileSystem.ExistsDirectory(SaveData.Instance.RootData.OldGuildmasterSavePath))
		{
			try
			{
				string[] directories = PlatformLayer.FileSystem.GetDirectories(SaveData.Instance.RootData.OldGuildmasterSavePath);
				foreach (string text in directories)
				{
					string path = Path.Combine(text, "Checkpoints");
					string[] files;
					if (PlatformLayer.FileSystem.ExistsDirectory(path))
					{
						files = PlatformLayer.FileSystem.GetFiles(path);
						foreach (string text2 in files)
						{
							string text3 = text2.ReplaceLastOccurrence("NewAdventureMode", "Guildmaster");
							if (Path.GetFileName(text2) != Path.GetFileName(text3))
							{
								PlatformLayer.FileSystem.MoveFile(text2, text3);
							}
						}
					}
					files = PlatformLayer.FileSystem.GetFiles(text);
					foreach (string text4 in files)
					{
						string text5 = text4.ReplaceLastOccurrence("NewAdventureMode", "Guildmaster");
						if (Path.GetFileName(text4) != Path.GetFileName(text5))
						{
							PlatformLayer.FileSystem.MoveFile(text4, text5);
						}
					}
					string text6 = text.ReplaceLastOccurrence("NewAdventureMode", "Guildmaster");
					if (text != text6)
					{
						PlatformLayer.FileSystem.MoveDirectory(text, text6);
					}
				}
				if (PlatformLayer.FileSystem.ExistsDirectory(SaveData.Instance.RootData.GuildmasterSavePath))
				{
					GloomUtility.CopyAll(SaveData.Instance.RootData.OldGuildmasterSavePath, SaveData.Instance.RootData.GuildmasterSavePath);
					PlatformLayer.FileSystem.RemoveDirectory(SaveData.Instance.RootData.OldGuildmasterSavePath, recursive: true);
				}
				else
				{
					PlatformLayer.FileSystem.MoveDirectory(SaveData.Instance.RootData.OldGuildmasterSavePath, SaveData.Instance.RootData.GuildmasterSavePath);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred attempting to update old save data.\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		foreach (PartyAdventureData allAdventure in m_AllAdventures)
		{
			if (!allAdventure.IsUserIDSet || allAdventure.Owner == null)
			{
				UpdateOldSaveWithUserID(allAdventure);
			}
		}
		foreach (PartyAdventureData allCampaign in AllCampaigns)
		{
			if (!allCampaign.IsUserIDSet || allCampaign.Owner == null)
			{
				UpdateOldSaveWithUserID(allCampaign);
			}
		}
		for (int num = m_AllAdventures.Count - 1; num >= 0; num--)
		{
			PartyAdventureData adventure = m_AllAdventures[num];
			if (m_AllAdventures.Count((PartyAdventureData x) => x.PartyNameNoSpaces == adventure.PartyNameNoSpaces && ((!x.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && x.Owner.PlatformNetworkAccountID == adventure.Owner.PlatformNetworkAccountID) || ((Application.platform != RuntimePlatform.Switch || x.Owner.PlatformNetworkAccountID.Equals("0")) && x.Owner.PlatformAccountID == adventure.Owner.PlatformAccountID))) > 1)
			{
				m_AllAdventures.Remove(adventure);
			}
		}
		for (int num2 = AllCampaigns.Count - 1; num2 >= 0; num2--)
		{
			PartyAdventureData adventure2 = AllCampaigns[num2];
			if (AllCampaigns.Count((PartyAdventureData x) => x.PartyNameNoSpaces == adventure2.PartyNameNoSpaces && ((!x.Owner.PlatformNetworkAccountID.IsNullOrEmpty() && x.Owner.PlatformNetworkAccountID == adventure2.Owner.PlatformNetworkAccountID) || ((Application.platform != RuntimePlatform.Switch || x.Owner.PlatformNetworkAccountID.Equals("0")) && x.Owner.PlatformAccountID == adventure2.Owner.PlatformAccountID))) > 1)
			{
				AllCampaigns.Remove(adventure2);
			}
		}
		for (int num3 = m_AllAdventures.Count - 1; num3 >= 0; num3--)
		{
			PartyAdventureData partyAdventureData = m_AllAdventures[num3];
			if (!PlatformLayer.FileSystem.ExistsFile(m_AllAdventures[num3].PartyMainSaveFile))
			{
				m_AllAdventures.Remove(partyAdventureData);
			}
			else if (partyAdventureData.AdventureMapStateFilePath != m_AllAdventures[num3].PartyMainSaveFile)
			{
				partyAdventureData.AdventureMapStateFilePath = m_AllAdventures[num3].PartyMainSaveFile;
				partyAdventureData.RefreshCheckpoints();
			}
		}
		for (int num4 = AllCampaigns.Count - 1; num4 >= 0; num4--)
		{
			PartyAdventureData partyAdventureData2 = AllCampaigns[num4];
			if (!PlatformLayer.FileSystem.ExistsFile(AllCampaigns[num4].PartyMainSaveFile))
			{
				AllCampaigns.Remove(partyAdventureData2);
			}
			else if (partyAdventureData2.AdventureMapStateFilePath != AllCampaigns[num4].PartyMainSaveFile)
			{
				partyAdventureData2.AdventureMapStateFilePath = AllCampaigns[num4].PartyMainSaveFile;
				partyAdventureData2.RefreshCheckpoints();
			}
		}
	}

	public IEnumerator ValidateSaves(EGameMode gameMode, List<string> restoredSaves = null)
	{
		SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(5f);
		string path;
		switch (gameMode)
		{
		case EGameMode.Campaign:
			path = SaveData.Instance.RootData.CampaignSavePath;
			if (MapRuleLibraryClient.MRLYML.MapMode != ScenarioManager.EDLLMode.Campaign)
			{
				SceneController.Instance.YML.LoadCampaign(DLCRegistry.AllDLCFlag);
			}
			break;
		case EGameMode.Guildmaster:
			path = SaveData.Instance.RootData.GuildmasterSavePath;
			if (MapRuleLibraryClient.MRLYML.MapMode != ScenarioManager.EDLLMode.Guildmaster)
			{
				SceneController.Instance.YML.LoadGuildMaster(DLCRegistry.AllDLCFlag);
			}
			break;
		default:
			Debug.LogError("Invalid game mode for ValidateSaves");
			yield break;
		}
		if (PlatformLayer.FileSystem.ExistsDirectory(path))
		{
			string[] directoryEntries = PlatformLayer.FileSystem.GetDirectories(path);
			for (int i = 0; i < directoryEntries.Length; i++)
			{
				Tuple<string, string, bool, string> saveNameInfo = GetSaveNameInfo(directoryEntries[i]);
				SceneController.Instance.LoadingScreenInstance.UpdateProgressBar((float)i / (float)directoryEntries.Length * 100f);
				yield return RecoverSave(saveNameInfo.Item1, saveNameInfo.Item2, saveNameInfo.Item4, saveNameInfo.Item3, directoryEntries[i], gameMode, restoredSaves);
			}
		}
	}

	public void DeserializeMapStateAndValidateOnLoad(EGameMode gameMode, PartyAdventureData partyData)
	{
		try
		{
			if (partyData.AdventureMapState == null)
			{
				partyData.Load(gameMode, isJoiningMPClient: false);
			}
			if (partyData.AdventureMapState != null && partyData.AdventureMapState.SaveOwner == null)
			{
				partyData.AdventureMapState.SetSaveOwner(partyData.Owner.ConvertToMapStateSaveOwner());
				partyData.Save();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to update MapStateFile\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private Tuple<string, string, bool, string> GetSaveNameInfo(string saveName)
	{
		if (saveName.Contains("\\"))
		{
			saveName = saveName.Substring(saveName.LastIndexOf("\\") + 1);
		}
		if (saveName.Contains("//"))
		{
			saveName = saveName.Substring(saveName.LastIndexOf("/") + 1);
		}
		string[] array = saveName.Split('_');
		string text = string.Empty;
		string text2 = string.Empty;
		bool flag = false;
		for (int i = 1; i < array.Length - 1; i++)
		{
			string text3 = array[i];
			string[] array2 = text3.Split(new string[1] { "[MOD]" }, StringSplitOptions.None);
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] == "" && text3 != "")
				{
					flag = !flag;
				}
				else if (flag)
				{
					if (text2 != null)
					{
						text2 += " ";
					}
					text2 += array2[j];
				}
				else
				{
					text += array[i];
					if (i < array.Length - 2)
					{
						text += " ";
					}
				}
			}
		}
		return new Tuple<string, string, bool, string>(text, array[^1], item3: false, text2);
	}

	private IEnumerator RecoverSave(string partyName, string accountID, string customRuleset, bool updateOldSave, string directoryName, EGameMode gameMode, List<string> restoredSaves)
	{
		PartyAdventureData partyAdventureData;
		switch (gameMode)
		{
		case EGameMode.Campaign:
			partyAdventureData = AllCampaigns.SingleOrDefault((PartyAdventureData x) => x.PartyName == partyName && ((Application.platform == RuntimePlatform.Switch) ? x.Owner.PlatformNetworkAccountID : x.Owner.PlatformAccountID) == accountID && x.RulesetName == customRuleset);
			break;
		case EGameMode.Guildmaster:
			partyAdventureData = m_AllAdventures.SingleOrDefault((PartyAdventureData x) => x.PartyName == partyName && ((Application.platform == RuntimePlatform.Switch) ? x.Owner.PlatformNetworkAccountID : x.Owner.PlatformAccountID) == accountID && x.RulesetName == customRuleset);
			break;
		default:
			Debug.LogError("Invalid game mode for RecoverSave");
			yield break;
		}
		if (partyAdventureData == null)
		{
			string[] fileEntries = PlatformLayer.FileSystem.GetFiles(directoryName, "*.dat");
			if (fileEntries.Length == 1)
			{
				PartyAdventureData newPartyAdventureData = new PartyAdventureData(fileEntries[0], new SaveOwner(), gameMode, partyName, customRuleset);
				if (restoredSaves != null && newPartyAdventureData.Restored)
				{
					restoredSaves.Add(newPartyAdventureData.PartySaveFileName);
				}
				if (updateOldSave)
				{
					UpdateOldSaveWithUserID(newPartyAdventureData);
				}
				string text = ((Application.platform == RuntimePlatform.Switch) ? newPartyAdventureData.Owner.PlatformNetworkAccountID : newPartyAdventureData.Owner.PlatformAccountID);
				if (newPartyAdventureData.AdventureMapState.SaveOwner != null && accountID != text)
				{
					bool isMaskingInProcess = true;
					SaveOwner saveOwner = new SaveOwner(newPartyAdventureData.AdventureMapState.SaveOwner);
					saveOwner.MaskBadWordsInUsername(delegate
					{
						isMaskingInProcess = false;
					});
					while (isMaskingInProcess)
					{
						yield return null;
					}
					newPartyAdventureData = new PartyAdventureData(fileEntries[0], saveOwner, gameMode, partyName, customRuleset);
				}
				switch (gameMode)
				{
				case EGameMode.Campaign:
				{
					PartyAdventureData partyAdventureData3 = ((Application.platform != RuntimePlatform.Switch) ? AllCampaigns.Find((PartyAdventureData x) => x.PartyName == newPartyAdventureData.PartyName && x.Owner.PlatformAccountID == newPartyAdventureData.Owner.PlatformAccountID) : AllCampaigns.Find((PartyAdventureData x) => x.PartyName == newPartyAdventureData.PartyName && x.Owner.PlatformNetworkAccountID == newPartyAdventureData.Owner.PlatformNetworkAccountID));
					if (partyAdventureData3 == null)
					{
						AllCampaigns.Add(newPartyAdventureData);
					}
					else if (partyAdventureData3.LastSavedSelectedCharacterIDs.Count != newPartyAdventureData.LastSavedSelectedCharacterIDs.Count)
					{
						partyAdventureData3.SetLastSavedSelectedCharacterIDs(newPartyAdventureData.LastSavedSelectedCharacterIDs);
					}
					break;
				}
				case EGameMode.Guildmaster:
				{
					PartyAdventureData partyAdventureData2 = ((Application.platform != RuntimePlatform.Switch) ? m_AllAdventures.Find((PartyAdventureData x) => x.PartyName == newPartyAdventureData.PartyName && x.Owner.PlatformAccountID == newPartyAdventureData.Owner.PlatformAccountID) : m_AllAdventures.Find((PartyAdventureData x) => x.PartyName == newPartyAdventureData.PartyName && x.Owner.PlatformNetworkAccountID == newPartyAdventureData.Owner.PlatformNetworkAccountID));
					if (partyAdventureData2 == null)
					{
						m_AllAdventures.Add(newPartyAdventureData);
					}
					else if (partyAdventureData2.LastSavedSelectedCharacterIDs.Count != newPartyAdventureData.LastSavedSelectedCharacterIDs.Count)
					{
						partyAdventureData2.SetLastSavedSelectedCharacterIDs(newPartyAdventureData.LastSavedSelectedCharacterIDs);
					}
					break;
				}
				default:
					Debug.LogError("Invalid game mode for RecoverSaves");
					yield break;
				}
				newPartyAdventureData.Save();
				while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
				{
					yield return null;
				}
				if (newPartyAdventureData.PartySaveDir.ParsePath() != directoryName && PlatformLayer.FileSystem.ExistsDirectory(directoryName))
				{
					Debug.Log("Deleting old directory " + directoryName + "\nNew directory for save is " + newPartyAdventureData.PartySaveDir);
					PlatformLayer.FileSystem.RemoveDirectory(directoryName, recursive: true);
				}
			}
			else if (fileEntries.Length != 0)
			{
				Debug.LogError("There are multiple save files in the directory " + directoryName + ".  Each save directory must have only one save file inside it.  Save State could not be recovered.");
			}
		}
		else if (updateOldSave)
		{
			UpdateOldSaveWithUserID(partyAdventureData);
		}
	}

	public GlobalData()
	{
		MasterVolume = 80;
		MusicVolume = 80;
		SFXVolume = 80;
		StoryVolume = 80;
		UIVolume = 80;
		MultiplayerPingVolume = 80;
		HapticVolume = 80;
		HapticVibration = 80;
		MuteAudioInBackground = true;
		KeyBindings = new List<KeyBinding>();
		DisabledCombatLogFilters = new List<CombatLogFilter>();
		DisabledCombatLog = false;
		CurrentCustomLevelData = null;
		CurrentAutoTestDataCopy = null;
		GameMode = EGameMode.MainMenu;
		SteamDataSuiteAttributionDone = false;
		AllCampaigns = new List<PartyAdventureData>();
		m_AllAdventures = new List<PartyAdventureData>();
		AllSingleScenarios = new List<PartyAdventureData>();
		m_ResetDataVersion = 2;
		QualityLevel = "Fastest";
		TargetResolutionWidth = -1;
		TargetResolutionHeight = -1;
		TargetFullScreenMode = true;
		TargetFrameRate = 60;
		LastLaunchedPlatform = string.Empty;
		Rulesets = new List<GHRuleset>();
		m_ModdingDirectory = SaveData.Instance.PersistentDataPath;
		CompletedTutorialIDs = new List<string>();
		InvertYAxis = false;
		HasVibration = true;
		StickSensitivity = 1f;
	}

	public void Save()
	{
	}

	public void Load()
	{
		if (!SteamDataSuiteAttributionDone)
		{
			SteamDataAttribution.Internal.SteamDataAttribution.OnRuntimeMethodLoad();
		}
	}

	public void CloseSave(EGameMode mode)
	{
		switch (mode)
		{
		case EGameMode.Campaign:
		case EGameMode.Guildmaster:
		case EGameMode.SingleScenario:
		case EGameMode.FrontEndTutorial:
			SaveData.Instance.SaveCurrentAdventureData();
			CurrentCustomLevelData = null;
			break;
		default:
			Debug.LogError("Unable to close save file.  Invalid game mode " + mode);
			break;
		}
	}

	public void CloseCustomLevel()
	{
		CurrentlyPlayingCustomLevel = false;
		CurrentCustomLevelData = null;
	}

	public bool IsCorrectVersion()
	{
		if (m_ResetDataVersion < 2)
		{
			return false;
		}
		return true;
	}

	public PartyAdventureData FindMostRecentSave()
	{
		try
		{
			List<PartyAdventureData> list = new List<PartyAdventureData>();
			if (ResumeAdventure != null)
			{
				list.Add(ResumeAdventure);
			}
			if (ResumeCampaign != null)
			{
				list.Add(ResumeCampaign);
			}
			if (ResumeSingleScenario != null)
			{
				list.Add(ResumeSingleScenario);
			}
			return list.OrderByDescending((PartyAdventureData o) => o.LastSavedTimeStamp).FirstOrDefault();
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during FindMostRecentSave.\n" + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}

	public void ChangeDLCInstalledState(DLCRegistry.EDLCKey dlcToChange, bool stateToSet)
	{
		if (stateToSet)
		{
			DLCInstalledRecord |= dlcToChange;
		}
		else
		{
			DLCInstalledRecord &= ~dlcToChange;
		}
	}
}
