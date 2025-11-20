#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Threading;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using Gloomhaven;
using GraphProgress;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.State;
using Newtonsoft.Json;
using Platforms.Activities;
using Platforms.PlatformData;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;
using UnityEngine;

[Serializable]
public class PartyAdventureData : ISerializable
{
	public const int MaxCheckpoints = 10;

	public const int MaxScenarioCheckpointsNonDev = 3;

	public const int MaxConsoleCheckpoints = 1;

	public const string CheckpointsFolder = "Checkpoints";

	public const string ScenarioCheckpointsFolder = "ScenarioCheckpoints";

	public const string ModAppend = "[MOD]";

	public const string CampaignProgressActivityName = "CampaignProgress";

	public const string GuildmasterProgressActivityName = "GuildmasterProgress";

	public const string GuildmasterTravelActivityName = "GuildmasterTravel";

	public const string GuildmasterStoryActivityName = "GuildmasterStory";

	public const string GuildmasterRelicActivityName = "GuildmasterRelic";

	private static string[] _introQuestIDs = new string[10] { "Quest_Story_3A", "Quest_Story_3B", "Quest_Travel_GibbetHill", "Quest_Story_5A", "Quest_Story_5B", "Quest_Story_5C", "Quest_Travel_TheMarches", "Quest_Story_7A", "Quest_Story_7B", "Quest_Relic_ResonantCrystal" };

	private ActivitiesProgressData _previousSaveDependentActivitiesProgressData;

	private ActivitiesProgressData _previousSaveIndependentActivitiesProgressData;

	private bool _activitiesSet;

	[NonSerialized]
	private readonly bool restored;

	public EGameMode GameMode { get; private set; }

	public string PartyName { get; private set; }

	public string DisplayPartyName { get; set; }

	public string PartyNameNoSpaces => PartyName.Replace(" ", "_");

	public int Seed { get; private set; }

	public EAdventureDifficulty Difficulty { get; private set; }

	public StateShared.EHouseRulesFlag HouseRulesSettings { get; private set; }

	public DLCRegistry.EDLCKey DLCEnabled { get; private set; }

	public string AdventureMapStateFilePath { get; set; }

	public string CrossSaveActivitiesSaveFilePath => CrossSaveActivitiesSaveFile;

	public List<string> AdventureMapCheckpointsFilepaths { get; set; }

	public List<string> AdventureMapScenarioCheckpointsFilepaths { get; set; }

	public ClientIndependantValues ClientIndependantValues { get; private set; }

	public bool IsModded
	{
		get
		{
			if (AdventureMapState == null || !AdventureMapState.IsModded || RulesetName == null || !(RulesetName != string.Empty))
			{
				if (RulesetName != null)
				{
					return RulesetName != string.Empty;
				}
				return false;
			}
			return true;
		}
	}

	public string RulesetName { get; set; }

	public string RulesetNameNoSpaces => RulesetName.Replace(" ", "_");

	public DateTime LastSavedTimeStamp { get; private set; }

	public string LastSavedQuestName { get; private set; }

	public int LastSavedPartyGoldAmount { get; private set; }

	public int LastSavedPartyItemAmount { get; private set; }

	public int LastSavedReputation { get; private set; }

	public int LastSavedWealth { get; private set; }

	public List<Tuple<string, int>> LastSavedSelectedCharacterIDs { get; private set; }

	public List<Tuple<string, int, string>> LastSavedSelectedCharacterInfo { get; private set; }

	public bool IsUserIDSet { get; set; }

	public SaveOwner Owner { get; set; }

	public EGoldMode GoldMode { get; set; }

	public string RunSessionID { get; set; }

	public double TimeActiveSecRunLTD { get; set; }

	public CMapState AdventureMapState { get; set; }

	public bool Restored => restored;

	private bool IsSupportActivities => PlatformLayer.IsSupportActivities;

	private IPlatformActivities ActivitiesController => PlatformLayer.Platform.PlatformActivities;

	private ActivitiesProgressData SaveRelatedActivitiesProgressData
	{
		get
		{
			if (!IsSupportActivities)
			{
				return new ActivitiesProgressData();
			}
			return ActivitiesController.GetSaveRelatedProgress();
		}
	}

	private ActivitiesProgressData CrossSaveActivitiesProgressData
	{
		get
		{
			if (!IsSupportActivities)
			{
				return new ActivitiesProgressData();
			}
			return ActivitiesController.GetCrossSaveProgress();
		}
	}

	private string ActivityDifficultyPostfix
	{
		get
		{
			if (AdventureMapState.DifficultySetting == EAdventureDifficulty.None || AdventureMapState.DifficultySetting == EAdventureDifficulty.Normal)
			{
				return string.Empty;
			}
			return AdventureMapState.DifficultySetting.ToString();
		}
	}

	public string PartySaveRoot
	{
		get
		{
			switch (GameMode)
			{
			case EGameMode.Campaign:
				return SaveData.Instance.RootData.CampaignSavePath;
			case EGameMode.Guildmaster:
				return SaveData.Instance.RootData.GuildmasterSavePath;
			case EGameMode.SingleScenario:
			case EGameMode.FrontEndTutorial:
				return SaveData.Instance.RootData.SingleScenarioSavePath;
			default:
				Debug.LogError("Invalid Game Mode is set for PartyAdventureData: " + GameMode);
				return null;
			}
		}
	}

	public string PartySaveFolderOnly
	{
		get
		{
			switch (GameMode)
			{
			case EGameMode.Campaign:
				return "Campaign";
			case EGameMode.Guildmaster:
				return "Guildmaster";
			case EGameMode.SingleScenario:
				return "SingleScenarios";
			default:
				Debug.LogError("Invalid Gamde Mode is set for PartyAdventureData: " + GameMode);
				return null;
			}
		}
	}

	public string PartySaveName
	{
		get
		{
			string text = ((Application.platform == RuntimePlatform.Switch) ? Owner.PlatformNetworkAccountID : Owner.PlatformAccountID);
			if (IsModded)
			{
				return GameMode.ToString() + "_[MOD]" + RulesetNameNoSpaces + "[MOD]_" + PartyNameNoSpaces + "_" + text;
			}
			return GameMode.ToString() + "_" + PartyNameNoSpaces + "_" + text;
		}
	}

	public string PartySaveDir => Path.Combine(PartySaveRoot, PartySaveName);

	public string ScenarioCheckpointFilename => "CP.dat";

	public string PartySaveFileName => PartySaveName + ".dat";

	public string PartyMainSaveFile => Path.Combine(PartySaveDir, PartySaveFileName);

	public string PartyCheckpointDir => Path.Combine(PartySaveDir, "Checkpoints");

	public string PartyScenarioCheckpointDir => Path.Combine(PartySaveDir, "ScenarioCheckpoints");

	public string CrossSaveActivitiesSaveFileName => "CrossSaveActivities.dat";

	public string CrossSaveActivitiesSaveDir => SaveData.Instance.RootData.CrossSaveActivitiesSavePath;

	public string CrossSaveActivitiesSaveFile => Path.Combine(CrossSaveActivitiesSaveDir, CrossSaveActivitiesSaveFileName);

	public string PartySaveDirOldFolder => Path.Combine(SaveData.Instance.RootData.OldGuildmasterSavePath, "NewAdventureMode_" + PartyNameNoSpaces + "_" + Owner.PlatformAccountID);

	public string PartySaveFileNameOldFolder => "NewAdventureMode_" + PartyNameNoSpaces + "_" + Owner.PlatformAccountID + ".dat";

	public string PartyMainSaveFileOldFolder => Path.Combine(PartySaveDir, PartySaveFileName);

	public string PartyCheckpointDirOldFolder => Path.Combine(PartySaveDir, "Checkpoints");

	public string PartySaveDirOld => Path.Combine(SaveData.Instance.RootData.OldGuildmasterSavePath, "NewAdventureMode_" + PartyNameNoSpaces);

	public string PartySaveFileNameOld => "NewAdventureMode_" + PartyNameNoSpaces + ".dat";

	public string PartyMainSaveFileOld => Path.Combine(PartySaveDirOld, PartySaveFileNameOld);

	public string PartyCheckpointDirOld => Path.Combine(PartySaveDirOld, "Checkpoints");

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("GameMode", GameMode);
		info.AddValue("PartyName", PartyName);
		info.AddValue("DisplayPartyName", DisplayPartyName);
		info.AddValue("Seed", Seed);
		info.AddValue("Difficulty", Difficulty);
		info.AddValue("HouseRulesSettings", HouseRulesSettings);
		info.AddValue("AdventureMapStateFilePath", AdventureMapStateFilePath);
		info.AddValue("AdventureMapCheckpointsFilepaths", AdventureMapCheckpointsFilepaths);
		info.AddValue("AdventureMapScenarioCheckpointsFilepaths", AdventureMapScenarioCheckpointsFilepaths);
		info.AddValue("ClientIndependantValues", ClientIndependantValues);
		info.AddValue("LastSavedTimeStamp", LastSavedTimeStamp);
		info.AddValue("LastSavedQuestName", LastSavedQuestName);
		info.AddValue("LastSavedPartyGoldAmount", LastSavedPartyGoldAmount);
		info.AddValue("LastSavedPartyItemAmount", LastSavedPartyItemAmount);
		info.AddValue("LastSavedSelectedCharacterIDs", LastSavedSelectedCharacterIDs);
		info.AddValue("RulesetName", RulesetName);
		info.AddValue("IsUserIDSet", IsUserIDSet);
		info.AddValue("Owner", Owner);
		info.AddValue("RunSessionID", RunSessionID);
		info.AddValue("TimeActiveSecRunLTD", TimeActiveSecRunLTD);
		info.AddValue("LastSavedReputation", LastSavedReputation);
		info.AddValue("LastSavedWealth", LastSavedWealth);
		info.AddValue("GoldMode", GoldMode);
		info.AddValue("DLCEnabled", DLCEnabled);
		info.AddValue("LastSavedSelectedCharacterInfo", LastSavedSelectedCharacterInfo);
	}

	public PartyAdventureData(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "GameMode":
					GameMode = (EGameMode)info.GetValue("GameMode", typeof(EGameMode));
					break;
				case "PartyName":
					PartyName = info.GetString("PartyName");
					break;
				case "DisplayPartyName":
					DisplayPartyName = info.GetString("DisplayPartyName");
					if (DisplayPartyName == null)
					{
						DisplayPartyName = PartyName;
					}
					break;
				case "Seed":
					Seed = info.GetInt32("Seed");
					break;
				case "Difficulty":
					Difficulty = (EAdventureDifficulty)info.GetValue("Difficulty", typeof(EAdventureDifficulty));
					break;
				case "HouseRulesSettings":
					HouseRulesSettings = (StateShared.EHouseRulesFlag)info.GetValue("HouseRulesSettings", typeof(StateShared.EHouseRulesFlag));
					break;
				case "AdventureMapStateFilePath":
					AdventureMapStateFilePath = info.GetString("AdventureMapStateFilePath");
					break;
				case "AdventureMapCheckpointsFilepaths":
					AdventureMapCheckpointsFilepaths = (List<string>)info.GetValue("AdventureMapCheckpointsFilepaths", typeof(List<string>));
					break;
				case "AdventureMapScenarioCheckpointsFilepaths":
					AdventureMapScenarioCheckpointsFilepaths = (List<string>)info.GetValue("AdventureMapScenarioCheckpointsFilepaths", typeof(List<string>));
					break;
				case "ClientIndependantValues":
					ClientIndependantValues = (ClientIndependantValues)info.GetValue("ClientIndependantValues", typeof(ClientIndependantValues));
					break;
				case "LastSavedTimeStamp":
					LastSavedTimeStamp = (DateTime)info.GetValue("LastSavedTimeStamp", typeof(DateTime));
					break;
				case "LastSavedQuestName":
					LastSavedQuestName = info.GetString("LastSavedQuestName");
					break;
				case "LastSavedPartyGoldAmount":
					LastSavedPartyGoldAmount = info.GetInt32("LastSavedPartyGoldAmount");
					break;
				case "LastSavedPartyItemAmount":
					LastSavedPartyItemAmount = info.GetInt32("LastSavedPartyItemAmount");
					break;
				case "LastSavedWealth":
					LastSavedWealth = info.GetInt32("LastSavedWealth");
					break;
				case "LastSavedReputation":
					LastSavedReputation = info.GetInt32("LastSavedReputation");
					break;
				case "LastSavedSelectedCharacterIDs":
					LastSavedSelectedCharacterIDs = (List<Tuple<string, int>>)info.GetValue("LastSavedSelectedCharacterIDs", typeof(List<Tuple<string, int>>));
					break;
				case "LastSavedSelectedCharacterInfo":
					LastSavedSelectedCharacterInfo = (List<Tuple<string, int, string>>)info.GetValue("LastSavedSelectedCharacterInfo", typeof(List<Tuple<string, int, string>>));
					break;
				case "RulesetName":
					RulesetName = info.GetString("RulesetName");
					break;
				case "IsUserIDSet":
					IsUserIDSet = info.GetBoolean("IsUserIDSet");
					break;
				case "Owner":
					Owner = (SaveOwner)info.GetValue("Owner", typeof(SaveOwner));
					break;
				case "RunSessionID":
					RunSessionID = info.GetString("RunSessionID");
					break;
				case "TimeActiveSecRunLTD":
					TimeActiveSecRunLTD = info.GetDouble("TimeActiveSecRunLTD");
					break;
				case "GoldMode":
					GoldMode = (EGoldMode)info.GetValue("GoldMode", typeof(EGoldMode));
					break;
				case "DLCEnabled":
					DLCEnabled = (DLCRegistry.EDLCKey)info.GetValue("DLCEnabled", typeof(DLCRegistry.EDLCKey));
					break;
				case "LastSavedSelectedCharacters":
				{
					List<Tuple<ECharacter, int>> obj = (List<Tuple<ECharacter, int>>)info.GetValue("LastSavedSelectedCharacters", typeof(List<Tuple<ECharacter, int>>));
					LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>();
					foreach (Tuple<ECharacter, int> item in obj)
					{
						LastSavedSelectedCharacterIDs.Add(new Tuple<string, int>(item.Item1.ToString() + "ID", item.Item2));
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize PartyAdventureData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (GameMode == EGameMode.None)
		{
			GameMode = EGameMode.Guildmaster;
		}
		if (RunSessionID == null)
		{
			RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		}
		if (LastSavedSelectedCharacterIDs == null)
		{
			LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>();
		}
		if (LastSavedSelectedCharacterInfo == null)
		{
			LastSavedSelectedCharacterInfo = new List<Tuple<string, int, string>>();
		}
		if (ClientIndependantValues == null)
		{
			ClientIndependantValues = new ClientIndependantValues();
		}
		if (AdventureMapScenarioCheckpointsFilepaths == null)
		{
			AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		}
		if (GoldMode == EGoldMode.None)
		{
			GoldMode = EGoldMode.PartyGold;
		}
		RefreshCheckpoints();
	}

	public PartyAdventureData(int seed, string partyName, EAdventureDifficulty difficulty, StateShared.EHouseRulesFlag houseRulesSettings, SaveOwner owner, EGoldMode goldMode, DLCRegistry.EDLCKey dlcEnabled, EEnhancementMode enhancementMode, EGameMode gameMode, string rulesetName)
	{
		PartyName = partyName;
		DisplayPartyName = partyName;
		Seed = seed;
		Difficulty = difficulty;
		HouseRulesSettings = houseRulesSettings;
		LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>();
		LastSavedSelectedCharacterInfo = new List<Tuple<string, int, string>>();
		IsUserIDSet = true;
		Owner = owner;
		ClientIndependantValues = new ClientIndependantValues();
		GameMode = gameMode;
		RulesetName = rulesetName;
		GoldMode = goldMode;
		DLCEnabled = dlcEnabled;
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		Debug.Log("[PartyAdventureData] Constructor called! Is about to reset saves with bool save = false!");
		ResetSave(goldMode, enhancementMode, save: false);
	}

	public PartyAdventureData(string filePath, string partyName, SaveOwner owner, EGameMode gameMode, string rulesetName)
	{
		AdventureMapState = LoadMapStateFromFile(filePath, out var _);
		SyncPlatformActivitiesProgress(AdventureMapState);
		PartyName = partyName;
		DisplayPartyName = partyName;
		Seed = AdventureMapState.Seed;
		Difficulty = AdventureMapState.DifficultySetting;
		HouseRulesSettings = AdventureMapState.HouseRulesSetting;
		LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>();
		LastSavedSelectedCharacterInfo = new List<Tuple<string, int, string>>();
		IsUserIDSet = true;
		Owner = owner;
		ClientIndependantValues = new ClientIndependantValues();
		GameMode = gameMode;
		RulesetName = rulesetName;
		DLCEnabled = AdventureMapState.DLCEnabled;
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		AdventureMapStateFilePath = PartyMainSaveFile;
		AdventureMapCheckpointsFilepaths = new List<string>();
		AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		UpdateLoadSlotData();
	}

	public PartyAdventureData(CMapState mapState, string partyName, SaveOwner owner, EGameMode gameMode, string rulesetName)
	{
		AdventureMapState = mapState;
		SyncPlatformActivitiesProgress(AdventureMapState);
		PartyName = partyName;
		DisplayPartyName = partyName;
		Seed = AdventureMapState.Seed;
		Difficulty = AdventureMapState.DifficultySetting;
		HouseRulesSettings = AdventureMapState.HouseRulesSetting;
		LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>();
		LastSavedSelectedCharacterInfo = new List<Tuple<string, int, string>>();
		IsUserIDSet = true;
		Owner = owner;
		ClientIndependantValues = new ClientIndependantValues();
		GameMode = gameMode;
		RulesetName = rulesetName;
		DLCEnabled = AdventureMapState.DLCEnabled;
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		AdventureMapStateFilePath = PartyMainSaveFile;
		AdventureMapCheckpointsFilepaths = new List<string>();
		AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		UpdateLoadSlotData();
	}

	public PartyAdventureData(string filePath, SaveOwner owner, EGameMode gameMode, string existingPartyName, string rulesetName)
	{
		PartyName = existingPartyName;
		DisplayPartyName = existingPartyName;
		GameMode = gameMode;
		Owner = owner;
		AdventureMapCheckpointsFilepaths = new List<string>();
		AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>();
		LastSavedSelectedCharacterInfo = new List<Tuple<string, int, string>>();
		ClientIndependantValues = new ClientIndependantValues();
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		IsUserIDSet = true;
		RulesetName = rulesetName;
		restored = RefreshMapStateFromFile(filePath);
		AdventureMapState.ValidateQuestStates();
		AdventureMapStateFilePath = PartyMainSaveFile;
		Seed = AdventureMapState.Seed;
		Difficulty = AdventureMapState.DifficultySetting;
		HouseRulesSettings = AdventureMapState.HouseRulesSetting;
		DLCEnabled = AdventureMapState.DLCEnabled;
		UpdateLoadSlotData();
	}

	public PartyAdventureData()
	{
	}

	public void Save(Action onSaveDone = null, byte[] data = null)
	{
		if (IsPossibleToMakeDeepClone(out var deepClone))
		{
			SaveData.Instance.SaveQueue.EnqueueWriteOperation(delegate(byte[] bytes, Action action)
			{
				ExecuteSaveAsync(bytes, deepClone, action);
			}, data, onSaveDone);
		}
		else
		{
			onSaveDone?.Invoke();
		}
	}

	public void SkipIntroActivities()
	{
		SkipIntroActivitiesInternal();
	}

	[Obsolete("Use GetSerializeMapStateAsync() instead of this method")]
	private byte[] GetSerializedMapState()
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map && AdventureMapState.CurrentMapPhaseType.In(EMapPhaseType.Moving, EMapPhaseType.RoadEvent))
		{
			Debug.LogError("----------------- Skipping writing save as player is not at map phase that supports saving");
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream();
		if (IsSupportActivities)
		{
			AdventureMapState.ActivitiesProgressData = ActivitiesController.GetSaveRelatedProgress();
		}
		new BinaryFormatter().Serialize(memoryStream, AdventureMapState);
		return memoryStream.ToArray();
	}

	private bool IsPossibleToMakeDeepClone(out CMapState deepClone)
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map && AdventureMapState.CurrentMapPhaseType.In(EMapPhaseType.Moving, EMapPhaseType.RoadEvent))
		{
			Debug.LogError("----------------- Skipping writing save as player is not at map phase that supports saving");
			deepClone = null;
			return false;
		}
		if (IsSupportActivities)
		{
			AdventureMapState.ActivitiesProgressData = ActivitiesController.GetSaveRelatedProgress();
		}
		deepClone = new CMapState(AdventureMapState, new ReferenceDictionary());
		return true;
	}

	private void GetSerializedMapStateAsync(CMapState copy, Action<byte[]> onComplete)
	{
		ThreadPool.QueueUserWorkItem(delegate(object state)
		{
			SerializeObject(state, onComplete);
		}, copy);
	}

	private void SerializeObject(object state, Action<byte[]> onComplete)
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, state);
		byte[] result = memoryStream.ToArray();
		SceneController.Instance.Updater.ExecuteInMainThread(delegate
		{
			onComplete?.Invoke(result);
		});
	}

	private void ExecuteSaveAsync(byte[] data, CMapState copy, Action onSaveDone = null)
	{
		if (data == null)
		{
			GetSerializedMapStateAsync(copy, delegate(byte[] byteData)
			{
				ExecuteSave(byteData, onSaveDone);
			});
		}
		else
		{
			ExecuteSave(data, onSaveDone);
		}
	}

	private void ExecuteSave(byte[] data, Action onSaveDone = null)
	{
		ClientIndependantValues.UpdateItemIsNewDictionary(AdventureMapState.GetAllUnlockedItems);
		ClientIndependantValues.UpdateAchievementIsNewDictionary(AdventureMapState.MapParty.Achievements);
		ClientIndependantValues.UpdateQuestIsNewDictionary(AdventureMapState.AllQuests);
		SaveMapStateToFile(data, AdventureMapStateFilePath, updateLoadSlotData: true, delegate
		{
			if (IsSupportActivities)
			{
				ActivitiesProgressData crossSaveActivitiesProgressData = CrossSaveActivitiesProgressData;
				ActivitiesProgressData saveRelatedActivitiesProgressData = SaveRelatedActivitiesProgressData;
				UpdateActivitiesInfo(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
				AdventureMapState.ActivitiesProgressData = saveRelatedActivitiesProgressData;
			}
			onSaveDone?.Invoke();
		});
	}

	private void UpdateActivitiesInfo(ActivitiesProgressData activitiesSaveDependentData, ActivitiesProgressData activitiesSaveIndependentData)
	{
		if (!IsSupportActivities)
		{
			return;
		}
		if (!_activitiesSet)
		{
			LogUtils.Log("Activities was not set and should not be updated");
			return;
		}
		LogUtils.Log("Updating activities info...");
		if (activitiesSaveDependentData == null || activitiesSaveIndependentData == null)
		{
			LogUtils.Log($"Some activities pack is null: activitiesSaveDependentData - {activitiesSaveDependentData}; activitiesSaveIndependentData - {activitiesSaveIndependentData};");
			return;
		}
		LogActivitiesData(activitiesSaveDependentData, activitiesSaveIndependentData);
		if (ActivitiesProgressData.Equals(_previousSaveDependentActivitiesProgressData, activitiesSaveDependentData) && ActivitiesProgressData.Equals(_previousSaveIndependentActivitiesProgressData, activitiesSaveIndependentData))
		{
			LogUtils.Log("Same activities info already send");
			return;
		}
		LogUtils.Log("Updating activities view...");
		PlatformLayer.Platform.PlatformActivities.UpdateAvailableActivitiesView(activitiesSaveDependentData, activitiesSaveIndependentData);
		if (!ActivitiesProgressData.Equals(_previousSaveDependentActivitiesProgressData, activitiesSaveDependentData))
		{
			PlatformLayer.Platform.PlatformActivities.UpdateStartedActivitiesView(activitiesSaveDependentData);
			PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(activitiesSaveDependentData);
		}
		LogUtils.Log("Activities info view updated");
		PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(activitiesSaveDependentData);
		PlatformLayer.Platform.PlatformActivities.SetCrossSaveProgress(activitiesSaveIndependentData);
		_previousSaveDependentActivitiesProgressData = activitiesSaveDependentData;
		_previousSaveIndependentActivitiesProgressData = activitiesSaveIndependentData;
		LogUtils.Log("Activities info updated");
	}

	public void RefreshCheckpoints(string overridePartyCheckpointsDir = null, string overrideScenarioCheckpointsDir = null)
	{
		LogUtils.Log("RefreshCheckpoints");
		AdventureMapCheckpointsFilepaths = new List<string>();
		string text = ((overridePartyCheckpointsDir != null) ? overridePartyCheckpointsDir : PartyCheckpointDir);
		string path = ((overrideScenarioCheckpointsDir != null) ? overrideScenarioCheckpointsDir : PartyScenarioCheckpointDir);
		if (PlatformLayer.FileSystem.ExistsDirectory(text))
		{
			List<string> list = null;
			if (PlatformLayer.Instance.IsConsole)
			{
				list = new List<string>();
				string[] files = PlatformLayer.FileSystem.GetFiles(text);
				list.AddRange(files);
				LogUtils.Log($"Found checkpoints: {files.Length} in {text}");
			}
			else
			{
				list = (from f in new DirectoryInfo(text).GetFileSystemInfos()
					orderby f.LastWriteTime
					select f.FullName).ToList();
			}
			foreach (string item in list)
			{
				AdventureMapCheckpointsFilepaths.Add(item);
			}
			LogUtils.Log($"{PlatformLayer.Instance.PlatformID}: refrereshed {list.Count} checkpoints");
		}
		AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		if (!PlatformLayer.FileSystem.ExistsDirectory(path))
		{
			return;
		}
		List<string> list2 = null;
		if (PlatformLayer.Instance.IsConsole)
		{
			list2 = new List<string>();
			list2.AddRange(PlatformLayer.FileSystem.GetFiles(path));
		}
		else
		{
			list2 = (from f in new DirectoryInfo(path).GetFileSystemInfos()
				orderby f.LastWriteTime
				select f.FullName).ToList();
		}
		foreach (string item2 in list2)
		{
			AdventureMapScenarioCheckpointsFilepaths.Add(item2);
		}
		LogUtils.Log($"{PlatformLayer.Instance.PlatformID}: refrereshed {list2.Count} scenarioCheckpoints");
	}

	public void EnqueueSaveCheckpoint(Action onDone = null)
	{
		if (IsPossibleToMakeDeepClone(out var deepClone))
		{
			SaveData.Instance.SaveQueue.EnqueueWriteOperation(delegate(byte[] bytes, Action action)
			{
				SaveCheckpointAsync(bytes, deepClone, action);
			}, null, onDone);
		}
		else
		{
			onDone?.Invoke();
		}
	}

	private void SaveCheckpointAsync(byte[] data, CMapState copy, Action onDone)
	{
		GetSerializedMapStateAsync(copy, delegate(byte[] serializedData)
		{
			SaveCheckpoint(serializedData, onDone);
		});
	}

	private void SaveCheckpoint(byte[] data, Action onDone)
	{
		LogUtils.Log($"SavedCheckpoint count: {AdventureMapCheckpointsFilepaths.Count}");
		LogAnyTypeOfCheckpoints(AdventureMapCheckpointsFilepaths);
		int num = (PlatformLayer.Instance.IsConsole ? 1 : 10);
		while (AdventureMapCheckpointsFilepaths.Count > num)
		{
			if (PlatformLayer.FileSystem.ExistsFile(AdventureMapCheckpointsFilepaths[0]))
			{
				PlatformLayer.FileSystem.RemoveFile(AdventureMapCheckpointsFilepaths[0]);
			}
			AdventureMapCheckpointsFilepaths.RemoveAt(0);
		}
		if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(PartyCheckpointDir))
		{
			Debug.Log("Creating PartyCheckpointDir directory: " + PartyCheckpointDir);
			PlatformLayer.FileSystem.CreateDirectory(PartyCheckpointDir);
		}
		string[] files = PlatformLayer.FileSystem.GetFiles(PartyCheckpointDir);
		LogUtils.Log("Checkpoints in PartyCheckpointDir:");
		LogAnyTypeOfCheckpoints(files.ToList());
		string text = SaveData.NextAvailableFilename(Path.Combine(PartyCheckpointDir, ScenarioCheckpointFilename));
		LogUtils.Log("Create checkpoint: " + text);
		AdventureMapCheckpointsFilepaths.Add(text);
		SaveMapStateToFile(data, AdventureMapCheckpointsFilepaths.Last(), updateLoadSlotData: false, onDone);
	}

	private void SaveScenarioCheckpointAsync(byte[] data, CMapState copy, Action onSaveDone = null)
	{
		if (data == null)
		{
			GetSerializedMapStateAsync(copy, delegate(byte[] serializedData)
			{
				SaveScenarioCheckpoint(serializedData, onSaveDone);
			});
		}
		else
		{
			SaveScenarioCheckpoint(data, onSaveDone);
		}
	}

	public void SaveScenarioCheckpoint(byte[] data, Action OnSaveDone = null)
	{
		LogUtils.Log($"SavedScenarioCheckpoint count: {AdventureMapScenarioCheckpointsFilepaths.Count}");
		LogAnyTypeOfCheckpoints(AdventureMapScenarioCheckpointsFilepaths);
		int num = 0;
		num = (PlatformLayer.Instance.IsConsole ? 1 : 3);
		while (AdventureMapScenarioCheckpointsFilepaths.Count > num)
		{
			if (PlatformLayer.FileSystem.ExistsFile(AdventureMapScenarioCheckpointsFilepaths[0]))
			{
				PlatformLayer.FileSystem.RemoveFile(AdventureMapScenarioCheckpointsFilepaths[0]);
				LogUtils.Log("File deleted: " + AdventureMapScenarioCheckpointsFilepaths[0]);
			}
			AdventureMapScenarioCheckpointsFilepaths.RemoveAt(0);
		}
		if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(PartyScenarioCheckpointDir))
		{
			Debug.Log("Creating PartyScenarioCheckpointDir directory: " + PartyScenarioCheckpointDir);
			PlatformLayer.FileSystem.CreateDirectory(PartyScenarioCheckpointDir);
		}
		string[] files = PlatformLayer.FileSystem.GetFiles(PartyScenarioCheckpointDir);
		LogUtils.Log("ScenarioCheckpoints in PartyScenarioCheckpointDir:");
		LogAnyTypeOfCheckpoints(files.ToList());
		string text = SaveData.NextAvailableFilename(Path.Combine(PartyScenarioCheckpointDir, ScenarioCheckpointFilename));
		LogUtils.Log("Create scenario checkpoint: " + text);
		AdventureMapScenarioCheckpointsFilepaths.Add(text);
		SaveMapStateToFile(data, AdventureMapScenarioCheckpointsFilepaths.Last(), updateLoadSlotData: false, OnSaveDone);
	}

	private void LogAnyTypeOfCheckpoints(List<string> checkpointsPaths)
	{
		string text = string.Empty;
		foreach (string checkpointsPath in checkpointsPaths)
		{
			text = text + checkpointsPath + "\n";
		}
		LogUtils.Log(text);
	}

	public bool RefreshMapStateFromFile(string filePath, Action onDone = null)
	{
		bool result = false;
		if (filePath == null)
		{
			Debug.LogError("Null filepath sent to RefreshMapStateFromFile");
		}
		else if (!PlatformLayer.FileSystem.ExistsFile(filePath))
		{
			Debug.LogWarning("MapState filePath does not exist. " + filePath);
		}
		else
		{
			AdventureMapState = LoadMapStateFromFile(filePath, out var _);
		}
		if (AdventureMapState == null)
		{
			result = true;
			string overridePartyCheckpointsDir = Path.Combine(Path.GetDirectoryName(filePath), "Checkpoints");
			string overrideScenarioCheckpointsDir = Path.Combine(Path.GetDirectoryName(filePath), "ScenarioCheckpoints");
			RefreshCheckpoints(overridePartyCheckpointsDir, overrideScenarioCheckpointsDir);
			AdventureMapState = LoadMostRecentWorkingCheckpoint();
		}
		SyncPlatformActivitiesProgress(AdventureMapState);
		if (AdventureMapState == null)
		{
			throw new Exception("Unable to load MapState file " + filePath + " and no valid checkpoints could be loaded to recover it");
		}
		onDone?.Invoke();
		return result;
	}

	public void SetupActivitiesForNewCampaign()
	{
		SetupCampaignActivities(forNewCampaign: true);
	}

	public void SetupActivitiesForResumedCampaign()
	{
		SetupCampaignActivities();
	}

	public void SetupActivitiesForNewGuildmaster(bool skipIntro)
	{
		SetupGuildmasterActivities(forNewGame: true, skipIntro);
	}

	public void SetupActivitiesForResumedGuildmaster()
	{
		SetupGuildmasterActivities();
	}

	public void ResetCampaignActivities()
	{
		CampaignProgressGraph.Instance.QuestCompleteEvent -= OnCampaignQuestComplete;
		CampaignProgressGraph.Instance.QuestBlockedEvent -= OnCampaignQuestBlocked;
	}

	private void SaveMapStateToFile(byte[] bf, string filePath, bool updateLoadSlotData = false, Action onSaveDone = null)
	{
		if (!SaveData.Instance.IsSavingThreadActive && Thread.CurrentThread != SceneController.Instance.MainThread)
		{
			Debug.LogError("Attempting to call SaveMapStateToFile from a thread that is not the main thread.  This is not allowed.\nMainThread: " + SceneController.Instance.MainThread.Name.ToString() + "\nCurrentThread: " + Thread.CurrentThread.Name.ToString());
			return;
		}
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		try
		{
			if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(PartySaveDir))
			{
				Debug.Log("Creating PartySaveDir directory: " + PartySaveDir);
				PlatformLayer.FileSystem.CreateDirectory(PartySaveDir);
			}
			PlatformLayer.FileSystem.WriteFileAsync(bf, filePath, delegate(OperationResult result, string description)
			{
				OnFileWriteDone(result, description, updateLoadSlotData, onSaveDone);
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to save Map State data.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00014", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OnFileWriteDone(OperationResult result, string description, bool updateLoadSlotData, Action OnSaveDone)
	{
		Debug.Log($"[SAVE] Saving mapstate to filepath done. result: {result} description: {description}. ");
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.HideProgress();
		}
		if (result == OperationResult.Success)
		{
			if (updateLoadSlotData)
			{
				UpdateLoadSlotData();
			}
		}
		else if (!PlatformLayer.UserData.IsSignedIn)
		{
			LogUtils.LogError("User signed out!");
		}
		else if (result != OperationResult.NotEnoughSpace)
		{
			UnityMainThreadDispatcher.Instance().Enqueue(delegate
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00014", "GUI_ERROR_MAIN_MENU_BUTTON", $"{result} \n {description}", UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu);
			});
		}
		OnSaveDone?.Invoke();
	}

	private void UpdateLoadSlotData()
	{
		LastSavedTimeStamp = DateTime.Now;
		LastSavedQuestName = ((AdventureMapState.InProgressQuestState?.Quest != null) ? AdventureMapState.InProgressQuestState.Quest.LocalisedNameKey : "");
		LastSavedPartyGoldAmount = ((AdventureMapState.GoldMode == EGoldMode.CharacterGold) ? AdventureMapState.MapParty.SelectedCharacters.Sum((CMapCharacter it) => it.CharacterGold) : AdventureMapState.MapParty.PartyGold);
		LastSavedPartyItemAmount = AdventureMapState.MapParty.CheckUnboundItems.Count;
		Difficulty = AdventureMapState.DifficultySetting;
		HouseRulesSettings = AdventureMapState.HouseRulesSetting;
		LastSavedReputation = (AdventureMapState.IsCampaign ? AdventureMapState.MapParty.Reputation : 0);
		LastSavedWealth = (AdventureMapState.IsCampaign ? AdventureMapState.MapParty.ProsperityLevel : 0);
		GoldMode = AdventureMapState.GoldMode;
		DLCEnabled = AdventureMapState.DLCEnabled;
		UpdateLastSavedSelectedCharacterIDs();
		UpdateLastSavedSelectedCharacterGold();
	}

	public void SetLastSavedSelectedCharacterIDs(List<Tuple<string, int>> ids)
	{
		LastSavedSelectedCharacterIDs = new List<Tuple<string, int>>(ids);
	}

	public void UpdateLastSavedSelectedCharacterIDs()
	{
		LastSavedSelectedCharacterIDs.Clear();
		if (AdventureState.MapState != null && (AdventureMapState.IsPlayingTutorial || AdventureMapState.IsInScenarioPhase))
		{
			if (AdventureMapState.IsPlayingTutorial)
			{
				foreach (PlayerState player in AdventureMapState.CurrentMapScenarioState.CustomLevelData.ScenarioState.Players)
				{
					LastSavedSelectedCharacterIDs.Add(new Tuple<string, int>(player.ClassID, player.Level));
				}
				return;
			}
			{
				foreach (PlayerState player2 in AdventureMapState.CurrentMapScenarioState.CurrentState.Players)
				{
					LastSavedSelectedCharacterIDs.Add(new Tuple<string, int>(player2.ClassID, player2.Level));
				}
				return;
			}
		}
		foreach (CMapCharacter selectedCharacter in AdventureMapState.MapParty.SelectedCharacters)
		{
			LastSavedSelectedCharacterIDs.Add(new Tuple<string, int>(selectedCharacter.CharacterID, selectedCharacter.Level));
		}
	}

	public void UpdateLastSavedSelectedCharacterGold()
	{
		LastSavedSelectedCharacterInfo.Clear();
		if ((AdventureState.MapState == null || AdventureState.MapState.GoldMode != EGoldMode.CharacterGold) && (AdventureMapState == null || AdventureMapState.GoldMode != EGoldMode.CharacterGold || AdventureMapState.MapParty == null))
		{
			return;
		}
		foreach (CMapCharacter selectedCharacter in AdventureMapState.MapParty.SelectedCharacters)
		{
			LastSavedSelectedCharacterInfo.Add(new Tuple<string, int, string>(selectedCharacter.CharacterID, selectedCharacter.CharacterGold, selectedCharacter.DisplayCharacterName));
		}
	}

	public void Load(EGameMode mode, bool isJoiningMPClient)
	{
		Debug.Log("[PartyAdventureData] Load() called!");
		AdventureMapState = LoadMapStateFromFile(AdventureMapStateFilePath, out var gameClosedWhileSaving);
		SyncPlatformActivitiesProgress(AdventureMapState);
		if (AdventureMapState == null)
		{
			if (!PlatformLayer.FileSystem.ExistsFile(AdventureMapStateFilePath))
			{
				AdventureMapState = null;
				if (IsSupportActivities)
				{
					PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
				}
				switch (mode)
				{
				case EGameMode.Campaign:
					if (SaveData.Instance.Global != null && SaveData.Instance.Global.AllCampaigns.Contains(this))
					{
						SaveData.Instance.Global.AllCampaigns.Remove(this);
					}
					break;
				case EGameMode.Guildmaster:
					if (SaveData.Instance.Global != null && SaveData.Instance.Global.AllAdventures.Contains(this))
					{
						SaveData.Instance.Global.RemoveAdventureSave(this);
					}
					break;
				}
				SaveData.Instance.SaveGlobalData();
				return;
			}
			MoveMapStateFileToCorruptedSaveFolder(mode);
			if (!isJoiningMPClient)
			{
				AdventureMapState = LoadMostRecentWorkingCheckpoint();
			}
			if (AdventureMapState == null)
			{
				Debug.LogError("[PartyAdventureData] No valid checkpoint map state file found");
				List<ErrorMessage.LabelAction> list = new List<ErrorMessage.LabelAction>();
				list.Add(new ErrorMessage.LabelAction("GUI_ERROR_RESET_SAVEFILE_BUTTON", ResetSave, KeyAction.UI_SUBMIT));
				list.Add(new ErrorMessage.LabelAction("GUI_ERROR_EXIT_GAME_BUTTON", Application.Quit, KeyAction.UI_CANCEL));
				SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_SAVEDATA_00012", Environment.StackTrace, list);
				return;
			}
			if (gameClosedWhileSaving)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00021", "GUI_ERROR_CONTINUE_BUTTON", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide);
			}
			else
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00013", "GUI_ERROR_CONTINUE_BUTTON", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide);
			}
			Debug.LogWarning("Reverted to checkpoint");
			Save();
		}
		SimpleLog.AddToSimpleLog("MapRNG (Load PartyAdventureData): " + AdventureMapState.PeekMapRNG);
		if (IsSupportActivities)
		{
			if (mode == EGameMode.Campaign)
			{
				SetupCampaignActivities();
			}
			else
			{
				SetupGuildmasterActivities();
			}
		}
		foreach (CItem item in AdventureMapState.GetAllUnlockedItems)
		{
			try
			{
				ClientIndependantValues.CIVKeyValuePair cIVKeyValuePair = ClientIndependantValues.CIVItemIsNewDictionary.SingleOrDefault((ClientIndependantValues.CIVKeyValuePair s) => s.Key == item.ItemGuid);
				if (cIVKeyValuePair != null)
				{
					item.IsNew = cIVKeyValuePair.Value;
				}
			}
			catch
			{
				Debug.LogError("Duplicate Item GUIDs exist in CIVItemIsNewDictionary.  ItemGUID = " + item.ItemGuid);
			}
		}
		foreach (CPartyAchievement partyAchievement in AdventureMapState.MapParty.Achievements)
		{
			try
			{
				ClientIndependantValues.CIVKeyValuePair cIVKeyValuePair2 = ClientIndependantValues.CIVAchievementIsNewDictionary.SingleOrDefault((ClientIndependantValues.CIVKeyValuePair s) => s.Key == partyAchievement.ID);
				if (cIVKeyValuePair2 != null)
				{
					partyAchievement.IsNew = cIVKeyValuePair2.Value;
				}
			}
			catch
			{
				Debug.LogError("Duplicate PartyAchievement Names exist in CIVAchievementIsNewDictionary.  PartyAchievement Name = " + partyAchievement.ID);
			}
		}
		foreach (CQuestState quest in AdventureMapState.AllQuests)
		{
			try
			{
				ClientIndependantValues.CIVKeyValuePair cIVKeyValuePair3 = ClientIndependantValues.CIVQuestIsNewDictionary.SingleOrDefault((ClientIndependantValues.CIVKeyValuePair s) => s.Key == quest.ID);
				if (cIVKeyValuePair3 != null)
				{
					quest.IsNew = cIVKeyValuePair3.Value;
				}
			}
			catch
			{
				Debug.LogError("Duplicate Quest IDs exist in CIVQuestIsNewDictionary.  Quest ID = " + quest.ID);
			}
		}
	}

	public void WinScenario(CQuestState questStateWon)
	{
		if (AdventureMapState.IsCampaign)
		{
			AdventureMapState.JustCompletedLocationState = questStateWon.ScenarioState;
			if (questStateWon.LinkedQuestState != null && questStateWon.LinkedQuestState.QuestState != CQuestState.EQuestState.Blocked)
			{
				AdventureMapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.AtLinkedScenario));
			}
			else
			{
				AdventureMapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InHQ));
			}
			if (IsSupportActivities && !questStateWon.IsDLCQuest)
			{
				CampaignProgressGraph.Instance.TryCompleteQuest(questStateWon.ID);
			}
		}
		else
		{
			if (questStateWon != null)
			{
				OnGuildmasterQuestComplete(questStateWon.ID);
			}
			AdventureMapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InHQ));
		}
		Save();
	}

	public void LoseScenario(bool reset = true)
	{
		if (reset)
		{
			AdventureMapState.InProgressQuestState?.ResetQuest();
		}
		AdventureMapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InHQ));
		Save();
	}

	public void RetryScenario()
	{
		AdventureMapState.CurrentMapScenarioState.RegenerateMapScenario(AdventureMapState.CurrentMapScenarioState.IsStartingScenario ? 1 : AdventureMapState.InProgressQuestState.Quest.Chapter);
		AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
		if (AdventureMapState.CurrentMapScenarioState != null)
		{
			AdventureMapState.SetNextMapPhase(new CMapPhase(EMapPhaseType.InScenario));
		}
		Save();
	}

	private CMapState LoadMostRecentWorkingCheckpoint()
	{
		CMapState cMapState = null;
		List<string> removedCheckpoints = new List<string>();
		bool gameClosedWhileSaving;
		for (int num = AdventureMapCheckpointsFilepaths.Count - 1; num >= 0; num--)
		{
			string text = AdventureMapCheckpointsFilepaths[num];
			cMapState = LoadMapStateFromFile(text, out gameClosedWhileSaving, deleteOnError: true);
			if (cMapState != null)
			{
				break;
			}
			removedCheckpoints.Add(text);
		}
		AdventureMapCheckpointsFilepaths.RemoveAll((string c) => removedCheckpoints.Contains(c));
		if (cMapState == null)
		{
			for (int num2 = AdventureMapScenarioCheckpointsFilepaths.Count - 1; num2 >= 0; num2--)
			{
				string text2 = AdventureMapScenarioCheckpointsFilepaths[num2];
				cMapState = LoadMapStateFromFile(text2, out gameClosedWhileSaving, deleteOnError: true);
				if (cMapState != null)
				{
					break;
				}
				removedCheckpoints.Add(text2);
			}
			AdventureMapScenarioCheckpointsFilepaths.RemoveAll((string c) => removedCheckpoints.Contains(c));
		}
		return cMapState;
	}

	public static string ConvertMapStateToJSON(string filePath)
	{
		CMapState cMapState = null;
		cMapState = ((filePath != null) ? LoadMapStateFromFile(filePath, out var _) : SaveData.Instance.Global.FindMostRecentSave().AdventureMapState);
		if (cMapState != null)
		{
			return JsonConvert.SerializeObject(cMapState, Formatting.Indented);
		}
		return string.Empty;
	}

	public static string ConvertScenarioStateToJSON(ScenarioState state)
	{
		if (state != null)
		{
			return JsonConvert.SerializeObject(state, Formatting.Indented);
		}
		return string.Empty;
	}

	public static CMapState LoadMapStateFromFile(string filePath, out bool gameClosedWhileSaving, bool deleteOnError = false)
	{
		gameClosedWhileSaving = false;
		if (PlatformLayer.FileSystem.ExistsFile(filePath))
		{
			try
			{
				using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(filePath));
				SaveData.Instance.CurrentFileBeingDeserialized = filePath;
				return new BinaryFormatter
				{
					Binder = new SerializationBinding()
				}.Deserialize(serializationStream) as CMapState;
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("End of Stream encountered before parsing was completed"))
				{
					gameClosedWhileSaving = true;
				}
				Debug.LogError("Map state file could not be deserialized" + Debug.GetExceptionText(ex));
				if (deleteOnError)
				{
					PlatformLayer.FileSystem.RemoveFile(filePath);
				}
				return null;
			}
		}
		Debug.LogWarning("Map state file path doesn't exist");
		return null;
	}

	public void MoveMapStateFileToCorruptedSaveFolder(EGameMode mode)
	{
		if (PlatformLayer.FileSystem.ExistsFile(AdventureMapStateFilePath))
		{
			MoveMapStateToCorruptedSaveFolderInternal(PartySaveFileName, AdventureMapStateFilePath);
		}
		AdventureMapState = null;
		if (IsSupportActivities)
		{
			PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
		}
		if (LoadMostRecentWorkingCheckpoint() != null)
		{
			return;
		}
		switch (mode)
		{
		case EGameMode.Campaign:
			if (SaveData.Instance.Global != null && SaveData.Instance.Global.AllCampaigns.Contains(this))
			{
				SaveData.Instance.Global.AllCampaigns.Remove(this);
			}
			break;
		case EGameMode.Guildmaster:
			if (SaveData.Instance.Global != null && SaveData.Instance.Global.AllAdventures.Contains(this))
			{
				SaveData.Instance.Global.RemoveAdventureSave(this);
			}
			break;
		}
	}

	public static void MoveMapStateToCorruptedSaveFolderInternal(string partySaveFileName, string mapStateFilePath)
	{
		string corruptedSavePath = SaveData.Instance.RootData.CorruptedSavePath;
		if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(corruptedSavePath))
		{
			Debug.Log("Creating CorruptedSavePath directory: " + corruptedSavePath);
			PlatformLayer.FileSystem.CreateDirectory(corruptedSavePath);
		}
		if (PlatformLayer.FileSystem.ExistsFile(Path.Combine(corruptedSavePath, partySaveFileName)))
		{
			PlatformLayer.FileSystem.RemoveFile(Path.Combine(corruptedSavePath, partySaveFileName));
		}
		PlatformLayer.FileSystem.MoveFile(mapStateFilePath, Path.Combine(corruptedSavePath, partySaveFileName));
	}

	public void CopyMapStateFileToBackupFolder(EGameMode mode)
	{
		if (PlatformLayer.FileSystem.ExistsFile(AdventureMapStateFilePath))
		{
			string preDLCBackupSavePath = SaveData.Instance.RootData.PreDLCBackupSavePath;
			if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(preDLCBackupSavePath))
			{
				Debug.Log("Creating BackupSave directory: " + preDLCBackupSavePath);
				PlatformLayer.FileSystem.CreateDirectory(preDLCBackupSavePath);
			}
			string text = Path.Combine(preDLCBackupSavePath, PartySaveFileName);
			int num = 0;
			while (PlatformLayer.FileSystem.ExistsFile(text))
			{
				num++;
				text = Path.Combine(preDLCBackupSavePath, PartySaveName + "_" + num + ".dat");
			}
			PlatformLayer.FileSystem.CopyFile(AdventureMapStateFilePath, text, overwrite: false);
		}
	}

	public void DeleteMapStateFiles()
	{
		Debug.Log("[PartyAdventureData] DeleteMapStateFiles() called.");
		if (PlatformLayer.FileSystem.ExistsFile(AdventureMapStateFilePath))
		{
			Debug.Log("[PartyAdventureData] DeleteMapStateFiles() AdventureMapStateFilePath RemoveFile(" + AdventureMapStateFilePath + ").");
			PlatformLayer.FileSystem.RemoveFile(AdventureMapStateFilePath);
		}
		foreach (string adventureMapCheckpointsFilepath in AdventureMapCheckpointsFilepaths)
		{
			if (PlatformLayer.FileSystem.ExistsFile(adventureMapCheckpointsFilepath))
			{
				Debug.Log("[PartyAdventureData] DeleteMapStateFiles() checkpointPath RemoveFile(" + adventureMapCheckpointsFilepath + ").");
				PlatformLayer.FileSystem.RemoveFile(adventureMapCheckpointsFilepath);
			}
		}
		foreach (string adventureMapScenarioCheckpointsFilepath in AdventureMapScenarioCheckpointsFilepaths)
		{
			if (PlatformLayer.FileSystem.ExistsFile(adventureMapScenarioCheckpointsFilepath))
			{
				Debug.Log("[PartyAdventureData] DeleteMapStateFiles() scenarioCheckpointPath RemoveFile(" + adventureMapScenarioCheckpointsFilepath + ").");
				PlatformLayer.FileSystem.RemoveFile(adventureMapScenarioCheckpointsFilepath);
			}
		}
	}

	public void ResetSave()
	{
		Debug.Log("[PartyAdventureData] ResetSave() called!");
		EGoldMode goldMode = EGoldMode.PartyGold;
		EEnhancementMode enhancementMode = EEnhancementMode.CharacterPersistent;
		bool gameClosedWhileSaving;
		if (AdventureMapState != null)
		{
			goldMode = AdventureMapState.GoldMode;
			enhancementMode = AdventureMapState.EnhancementMode;
		}
		else if (PlatformLayer.FileSystem.ExistsFile(AdventureMapStateFilePath))
		{
			AdventureMapState = LoadMapStateFromFile(AdventureMapStateFilePath, out gameClosedWhileSaving, deleteOnError: true);
			SyncPlatformActivitiesProgress(AdventureMapState);
			goldMode = AdventureMapState.GoldMode;
			enhancementMode = AdventureMapState.EnhancementMode;
		}
		else if (PlatformLayer.FileSystem.ExistsFile(PartySaveFileName))
		{
			AdventureMapState = LoadMapStateFromFile(PartySaveFileName, out gameClosedWhileSaving, deleteOnError: true);
			SyncPlatformActivitiesProgress(AdventureMapState);
			goldMode = AdventureMapState.GoldMode;
			enhancementMode = AdventureMapState.EnhancementMode;
		}
		ResetSave(goldMode, enhancementMode);
	}

	public void ResetSave(EGoldMode goldMode, EEnhancementMode enhancementMode, bool save = true)
	{
		Debug.Log("[PartyAdventureData] ResetSave(EGoldMode:" + goldMode.ToString() + ", EEnchacementMode:" + enhancementMode.ToString() + ", bool save:" + save + ") called!");
		AdventureMapStateFilePath = PartyMainSaveFile;
		AdventureMapCheckpointsFilepaths = new List<string>();
		AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		DeleteMapStateFiles();
		AdventureMapState = new CMapState(Seed, Difficulty, HouseRulesSettings, goldMode, enhancementMode, DLCEnabled, IsModded, Owner.ConvertToMapStateSaveOwner(), ConvertGameModeToDLLMode(GameMode), GameMode != EGameMode.Campaign);
		if (IsSupportActivities)
		{
			PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(AdventureMapState.ActivitiesProgressData);
		}
		if (save)
		{
			Save();
		}
	}

	private ScenarioManager.EDLLMode ConvertGameModeToDLLMode(EGameMode gameMode)
	{
		switch (gameMode)
		{
		case EGameMode.Campaign:
			return ScenarioManager.EDLLMode.Campaign;
		case EGameMode.Guildmaster:
			return ScenarioManager.EDLLMode.Guildmaster;
		case EGameMode.LevelEditor:
		case EGameMode.SingleScenario:
		case EGameMode.Autotest:
		case EGameMode.FrontEndTutorial:
			return ScenarioManager.EDLLMode.CustomScenario;
		default:
			return ScenarioManager.EDLLMode.None;
		}
	}

	public void UpdateSavePaths()
	{
		if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(PartySaveDir))
		{
			Debug.Log("Creating PartySaveDir directory: " + PartySaveDir);
			PlatformLayer.FileSystem.CreateDirectory(PartySaveDir);
		}
		if (PlatformLayer.FileSystem.ExistsFile(PartyMainSaveFile))
		{
			PlatformLayer.FileSystem.RemoveFile(PartyMainSaveFile);
		}
		PlatformLayer.FileSystem.CopyFile(AdventureMapStateFilePath, PartyMainSaveFile, overwrite: false);
		AdventureMapStateFilePath = PartyMainSaveFile;
		AdventureMapState = LoadMapStateFromFile(AdventureMapStateFilePath, out var _);
		SyncPlatformActivitiesProgress(AdventureMapState);
		AdventureMapCheckpointsFilepaths = new List<string>();
		AdventureMapScenarioCheckpointsFilepaths = new List<string>();
		Save();
	}

	public void ScrapeStats(EResult result)
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign || SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
		{
			SaveData.Instance.Global.m_StatsDataStorage.ScrapeEventLog(result, endScenario: true);
		}
	}

	public void EndCurrentScenario(EResult result)
	{
		try
		{
			if (AdventureMapState.CurrentMapScenarioState != null)
			{
				if (result == EResult.Win || result == EResult.Lose || result == EResult.Resign)
				{
					if (AdventureMapState.IsCampaign && !AdventureMapState.CurrentMapScenarioState.ID.ToUpper().Contains("JOTL") && !AdventureMapState.CurrentMapScenarioState.ID.ToUpper().Contains("_SOLO_"))
					{
						AdventureMapState.CanDrawCityEvent = true;
					}
					foreach (CPlayerActor playerActor in ScenarioManager.Scenario.AllPlayers)
					{
						int num = playerActor.Gold;
						if (playerActor.CompanionSummon != null)
						{
							num += playerActor.CompanionSummon.Gold;
						}
						CMapCharacter cMapCharacter = AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == playerActor.CharacterClass.ID);
						if (cMapCharacter != null)
						{
							if (AdventureMapState.GoldMode == EGoldMode.CharacterGold)
							{
								cMapCharacter.ModifyGold(num, useGoldModifier: true);
							}
							cMapCharacter.GainEXP(playerActor.XP, AdventureMapState.Difficulty.HasXPModifier ? AdventureMapState.Difficulty.XPModifier : 1f);
							cMapCharacter.RemoveMapConditions(RewardCondition.EConditionMapDuration.NextScenario);
						}
						if (AdventureMapState.GoldMode == EGoldMode.PartyGold)
						{
							AdventureMapState.MapParty.ModifyPartyGold(num, useGoldModifier: true);
						}
						if (AdventureMapState.InProgressQuestState != null && AdventureMapState.InProgressQuestState.IsSoloScenario && AdventureMapState.InProgressQuestState.SoloScenarioCharacterID == cMapCharacter?.CharacterID)
						{
							AdventureMapState.InProgressQuestState.SoloScenarioGoldGained = num;
							AdventureMapState.InProgressQuestState.SoloScenarioXPGained = playerActor.XP;
						}
					}
					AdventureMapState.MapParty.ResetItems();
					if (AdventureMapState.InProgressQuestState != null)
					{
						foreach (CBattleGoalState item in AdventureMapState.InProgressQuestState.ChosenBattleGoalStates.Select((Tuple<string, CBattleGoalState> x) => x.Item2).ToList())
						{
							item.ResetBattleGoalProgress();
						}
					}
				}
				if (result == EResult.Win)
				{
					if (ScenarioManager.CurrentScenarioState.GoalChestRewards.Count > 0 && AdventureMapState.InProgressQuestState.QuestState < CQuestState.EQuestState.Completed)
					{
						foreach (Tuple<string, RewardGroup> goalChestReward in ScenarioManager.CurrentScenarioState.GoalChestRewards)
						{
							AdventureMapState.ApplyRewards(goalChestReward.Item2.Rewards, goalChestReward.Item1);
						}
					}
					AdventureMapState.CurrentMapScenarioState.RemoveAssociatedDataFromMapState();
					if (AdventureMapState.IsPlayingTutorial)
					{
						AdventureMapState.CompleteTutorial();
					}
					else if (AdventureMapState.CurrentMapPhaseType == EMapPhaseType.InScenario)
					{
						AdventureMapState.InProgressQuestState.UpdateQuestCompletion();
					}
					else if (AdventureMapState.CurrentMapPhaseType == EMapPhaseType.InScenarioDebugMode)
					{
						AdventureMapState.InProgressQuestState.ResetQuest();
					}
				}
			}
			ScenarioRuleClient.Reset();
			Choreographer.s_Choreographer.CheckAchievements();
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred at PartyAdventureData.EndCurrentScenario\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_PARTYADVENTUREDATA_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public IEnumerator UpdateScenarioCheckpointAsync(ScenarioState currentState)
	{
		while (DelayedDropSMB.DelayedDropsAreInProgress())
		{
			yield return null;
		}
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
		{
			yield break;
		}
		if (Singleton<AutoSaveProgress>.Instance != null)
		{
			Singleton<AutoSaveProgress>.Instance.ShowProgress();
		}
		yield return null;
		try
		{
			SaveData.Instance.IsSavingData = true;
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
			case EGameMode.Guildmaster:
			{
				SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.UpdateAutoSaveState(currentState);
				if (IsPossibleToMakeDeepClone(out var deepClone))
				{
					SaveData.Instance.SaveQueue.EnqueueWriteOperation(delegate(byte[] bytes, Action action)
					{
						SaveScenarioCheckpointAsync(bytes, deepClone, action);
					}, null);
				}
				break;
			}
			case EGameMode.SingleScenario:
			case EGameMode.FrontEndTutorial:
				SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario.ScenarioState = currentState;
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Scenario Checkpoint.  Exception: " + ex.Message + "\n" + ex.StackTrace);
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
			StartStateCompare();
			yield break;
		}
		yield return null;
		try
		{
			SaveData.Instance.SaveCurrentAdventureData(delegate
			{
				if (Singleton<AutoSaveProgress>.Instance != null)
				{
					Singleton<AutoSaveProgress>.Instance.HideProgress();
				}
				StartStateCompare();
			});
		}
		catch (Exception ex2)
		{
			Debug.LogError("Failed to save Scenario Checkpoint.  Exception: " + ex2.Message + "\n" + ex2.StackTrace);
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
			StartStateCompare();
		}
	}

	public void UpdateScenarioCheckpoint(ScenarioState currentState, Action OnSaveDone)
	{
		while (DelayedDropSMB.DelayedDropsAreInProgress())
		{
			Thread.Sleep(10);
		}
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
		{
			return;
		}
		try
		{
			SaveData.Instance.IsSavingData = true;
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
			case EGameMode.Guildmaster:
			{
				SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.UpdateAutoSaveState(currentState);
				if (IsPossibleToMakeDeepClone(out var deepClone))
				{
					SaveData.Instance.SaveQueue.EnqueueWriteOperation(delegate(byte[] bytes, Action action)
					{
						SaveScenarioCheckpointAsync(bytes, deepClone, action);
					}, null);
				}
				break;
			}
			case EGameMode.SingleScenario:
			case EGameMode.FrontEndTutorial:
				SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario.ScenarioState = currentState;
				break;
			case EGameMode.LevelEditor:
			case EGameMode.Autotest:
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Scenario Checkpoint.  Exception: " + ex.Message + "\n" + ex.StackTrace);
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
			StartStateCompare();
			return;
		}
		try
		{
			if (IsPossibleToMakeDeepClone(out var deepClone2))
			{
				GetSerializedMapStateAsync(deepClone2, delegate(byte[] serializedData)
				{
					SaveData.Instance.SaveCurrentAdventureData(delegate
					{
						if (Singleton<AutoSaveProgress>.Instance != null)
						{
							Singleton<AutoSaveProgress>.Instance.HideProgress();
						}
						StartStateCompare();
						OnSaveDone?.Invoke();
					}, serializedData);
				});
			}
			else
			{
				OnSaveDone?.Invoke();
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Failed to save Scenario Checkpoint.  Exception: " + ex2.Message + "\n" + ex2.StackTrace);
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
			StartStateCompare();
		}
	}

	private void StartStateCompare()
	{
		if (Choreographer.s_Choreographer != null && !Choreographer.s_Choreographer.IsFirstTurnPlaying && FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant)
		{
			if (SaveData.Instance.Global.MPEndOfTurnCompare)
			{
				Choreographer.s_Choreographer.StartMPEndOfRoundCompare();
			}
			else
			{
				SaveData.Instance.IsSavingData = false;
			}
		}
		else
		{
			SaveData.Instance.IsSavingData = false;
		}
	}

	private void SetupCampaignActivities(bool forNewCampaign = false)
	{
		LogUtils.Log("Setting activities info for campaign...");
		bool flag = false;
		foreach (PartyAdventureData allCampaign in SaveData.Instance.Global.AllCampaigns)
		{
			allCampaign.ResetCampaignActivities();
		}
		ActivitiesProgressData activitiesProgressData = AdventureMapState.ActivitiesProgressData;
		if (activitiesProgressData == null)
		{
			activitiesProgressData = new ActivitiesProgressData();
			if (!forNewCampaign)
			{
				flag = true;
			}
		}
		ActivitiesProgressData crossSaveProgress = new ActivitiesProgressData();
		string[] visibilityFilters = new string[1] { "Campaign" };
		PlatformLayer.Platform.PlatformActivities.SetVisibilityFilters(visibilityFilters);
		PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(activitiesProgressData);
		PlatformLayer.Platform.PlatformActivities.SetCrossSaveProgress(crossSaveProgress);
		if (flag)
		{
			string id = "CampaignProgress";
			PlatformLayer.Platform.PlatformActivities.MakeAvailable(id);
			PlatformLayer.Platform.PlatformActivities.Start(id);
			CampaignProgressGraph.Instance.QuestCompleteEvent += OnCampaignQuestComplete;
			CampaignProgressGraph.Instance.QuestBlockedEvent += OnCampaignQuestBlocked;
			List<CQuestState> allCompletedQuests = AdventureMapState.AllCompletedQuests;
			foreach (CQuestState item in allCompletedQuests)
			{
				_ = item;
				foreach (CQuestState item2 in allCompletedQuests)
				{
					if (!item2.IsDLCQuest)
					{
						CampaignProgressGraph.Instance.TryCompleteQuest(item2.ID);
					}
				}
			}
		}
		else
		{
			if (forNewCampaign)
			{
				string id2 = "CampaignProgress";
				PlatformLayer.Platform.PlatformActivities.MakeAvailable(id2);
				PlatformLayer.Platform.PlatformActivities.Start(id2);
			}
			else
			{
				foreach (int campaignCompleteActivityQuestsId in AdventureMapState.CampaignCompleteActivityQuestsIds)
				{
					CampaignProgressGraph.Instance.TryCompleteQuest(campaignCompleteActivityQuestsId);
				}
			}
			CampaignProgressGraph.Instance.QuestCompleteEvent += OnCampaignQuestComplete;
			CampaignProgressGraph.Instance.QuestBlockedEvent += OnCampaignQuestBlocked;
		}
		ActivitiesProgressData saveRelatedActivitiesProgressData = SaveRelatedActivitiesProgressData;
		ActivitiesProgressData crossSaveActivitiesProgressData = CrossSaveActivitiesProgressData;
		LogActivitiesData(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
		LogUtils.Log("Updating activities view for campaign...");
		PlatformLayer.Platform.PlatformActivities.ClearActivitiesProgressView();
		PlatformLayer.Platform.PlatformActivities.UpdateAvailableActivitiesView(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
		if (forNewCampaign || flag)
		{
			PlatformLayer.Platform.PlatformActivities.UpdateStartedActivitiesView(saveRelatedActivitiesProgressData);
			PlatformLayer.Platform.PlatformActivities.UpdateStartedActivitiesView(crossSaveActivitiesProgressData);
		}
		PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(saveRelatedActivitiesProgressData);
		PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(crossSaveActivitiesProgressData);
		if (!forNewCampaign)
		{
			PlatformLayer.Platform.PlatformActivities.UpdateResumedActivitiesView(saveRelatedActivitiesProgressData);
			PlatformLayer.Platform.PlatformActivities.UpdateResumedActivitiesView(crossSaveActivitiesProgressData);
		}
		LogUtils.Log("Activities view for campaign updated");
		PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(saveRelatedActivitiesProgressData);
		PlatformLayer.Platform.PlatformActivities.SetCrossSaveProgress(crossSaveActivitiesProgressData);
		AdventureMapState.ActivitiesProgressData = saveRelatedActivitiesProgressData;
		_previousSaveDependentActivitiesProgressData = saveRelatedActivitiesProgressData;
		_previousSaveIndependentActivitiesProgressData = crossSaveActivitiesProgressData;
		_activitiesSet = true;
		LogUtils.Log("Activities info for campaign set");
	}

	private void OnCampaignQuestComplete(int questID, string questStringID)
	{
		if (ActivitiesController.IsExists(questStringID) && !AdventureMapState.CampaignCompleteActivityQuestsIds.Contains(questID))
		{
			AdventureMapState.CampaignCompleteActivityQuestsIds.Add(questID);
			MarkActivityAsComplete(questStringID);
			if (IsFinalCampaignQuest(questID))
			{
				MarkAllSkippedActivitiesAsComplete();
				EndProgressActivity("CampaignProgress");
				ActivitiesProgressData saveRelatedActivitiesProgressData = SaveRelatedActivitiesProgressData;
				ActivitiesProgressData crossSaveActivitiesProgressData = CrossSaveActivitiesProgressData;
				LogActivitiesData(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
				LogUtils.Log("Updating activities view on progress complete...");
				PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(saveRelatedActivitiesProgressData);
				PlatformLayer.Platform.PlatformActivities.UpdateAvailableActivitiesView(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
				PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(saveRelatedActivitiesProgressData);
			}
		}
	}

	private void OnCampaignQuestBlocked(int questID, string questStringID)
	{
		if (ActivitiesController.IsExists(questStringID))
		{
			MarkActivityAsComplete(questStringID);
			if (IsFinalCampaignQuest(questID))
			{
				MarkAllSkippedActivitiesAsComplete();
			}
		}
	}

	private bool IsFinalCampaignQuest(int questID)
	{
		return questID == 51;
	}

	private void MarkActivityAsComplete(string activityID)
	{
		ActivitiesController.MakeAvailable(activityID);
		ActivitiesController.Start(activityID);
		ActivitiesController.End(activityID, ActivityResult.Completed);
		ActivitiesController.MakeUnavailable(activityID);
	}

	private void EndProgressActivity(string progressActivityID)
	{
		ActivitiesController.End(progressActivityID, ActivityResult.Completed);
		ActivitiesController.MakeUnavailable(progressActivityID);
	}

	private void MarkAllSkippedActivitiesAsComplete()
	{
		foreach (QuestVertex mainQuestVertex in CampaignProgressGraph.Instance.MainQuestVertices)
		{
			if (mainQuestVertex.QuestType != QuestType.Completed && mainQuestVertex.QuestType != QuestType.Blocked)
			{
				string questId = mainQuestVertex.QuestId;
				MarkActivityAsComplete(questId);
			}
		}
	}

	private void SetupGuildmasterActivities(bool forNewGame = false, bool skipIntro = false)
	{
		LogUtils.Log("Setting activities info for guildmaster...");
		bool flag = false;
		ActivitiesProgressData activitiesProgressData = AdventureMapState.ActivitiesProgressData;
		if (activitiesProgressData == null)
		{
			LogUtils.Log("There is NO activities data in save => creating new");
			activitiesProgressData = new ActivitiesProgressData();
			if (!forNewGame)
			{
				flag = true;
			}
		}
		ActivitiesProgressData crossSaveProgress = new ActivitiesProgressData();
		string[] visibilityFilters = new string[1] { "Guildmaster" };
		PlatformLayer.Platform.PlatformActivities.SetVisibilityFilters(visibilityFilters);
		PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(activitiesProgressData);
		PlatformLayer.Platform.PlatformActivities.SetCrossSaveProgress(crossSaveProgress);
		if (forNewGame || flag)
		{
			string id = "GuildmasterProgress";
			PlatformLayer.Platform.PlatformActivities.MakeAvailable(id);
			PlatformLayer.Platform.PlatformActivities.Start(id);
			string id2 = "GuildmasterTravel";
			PlatformLayer.Platform.PlatformActivities.MakeAvailable(id2);
			PlatformLayer.Platform.PlatformActivities.Start(id2);
			string id3 = "GuildmasterStory";
			PlatformLayer.Platform.PlatformActivities.MakeAvailable(id3);
			PlatformLayer.Platform.PlatformActivities.Start(id3);
			string id4 = "GuildmasterRelic";
			PlatformLayer.Platform.PlatformActivities.MakeAvailable(id4);
			PlatformLayer.Platform.PlatformActivities.Start(id4);
			if (flag)
			{
				if (AdventureMapState.AllCompletedQuests != null)
				{
					foreach (CQuestState allCompletedQuest in AdventureMapState.AllCompletedQuests)
					{
						OnGuildmasterQuestComplete(allCompletedQuest.ID);
					}
				}
			}
			else if (skipIntro)
			{
				SkipIntroActivitiesInternal();
			}
		}
		ActivitiesProgressData saveRelatedActivitiesProgressData = SaveRelatedActivitiesProgressData;
		ActivitiesProgressData crossSaveActivitiesProgressData = CrossSaveActivitiesProgressData;
		LogActivitiesData(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
		LogUtils.Log("Updating activities view for guildmaster...");
		PlatformLayer.Platform.PlatformActivities.ClearActivitiesProgressView();
		PlatformLayer.Platform.PlatformActivities.UpdateAvailableActivitiesView(saveRelatedActivitiesProgressData, crossSaveActivitiesProgressData);
		if (forNewGame || flag)
		{
			PlatformLayer.Platform.PlatformActivities.UpdateStartedActivitiesView(saveRelatedActivitiesProgressData);
			PlatformLayer.Platform.PlatformActivities.UpdateStartedActivitiesView(crossSaveActivitiesProgressData);
		}
		PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(saveRelatedActivitiesProgressData);
		PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(crossSaveActivitiesProgressData);
		if (!forNewGame)
		{
			PlatformLayer.Platform.PlatformActivities.UpdateResumedActivitiesView(saveRelatedActivitiesProgressData);
			PlatformLayer.Platform.PlatformActivities.UpdateResumedActivitiesView(crossSaveActivitiesProgressData);
		}
		LogUtils.Log("Activities view for guildmaster updated");
		PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(saveRelatedActivitiesProgressData);
		PlatformLayer.Platform.PlatformActivities.SetCrossSaveProgress(crossSaveActivitiesProgressData);
		AdventureMapState.ActivitiesProgressData = saveRelatedActivitiesProgressData;
		_previousSaveDependentActivitiesProgressData = saveRelatedActivitiesProgressData;
		_previousSaveIndependentActivitiesProgressData = crossSaveActivitiesProgressData;
		_activitiesSet = true;
		LogUtils.Log("Activities info for guildmaster set");
	}

	private void SkipIntroActivitiesInternal()
	{
		string[] introQuestIDs = _introQuestIDs;
		foreach (string questFullID in introQuestIDs)
		{
			OnGuildmasterQuestComplete(questFullID);
		}
	}

	private void LogActivitiesData(ActivitiesProgressData activitiesSaveDependentData, ActivitiesProgressData activitiesSaveIndependentData)
	{
		string text = "Available activities: ";
		foreach (string item in activitiesSaveDependentData.Available)
		{
			text = string.Concat(text, "\n" + item);
		}
		foreach (string item2 in activitiesSaveIndependentData.Available)
		{
			text = string.Concat(text, "\n" + item2);
		}
		text += "\n^^^\n";
		text += "Active activities:";
		foreach (string item3 in activitiesSaveDependentData.Active)
		{
			text = string.Concat(text, "\n" + item3);
		}
		foreach (string item4 in activitiesSaveIndependentData.Active)
		{
			text = string.Concat(text, "\n" + item4);
		}
		text += "\n^^^\n";
		text += "Ended activities:";
		foreach (KeyValuePair<string, List<ActivityResult>> item5 in activitiesSaveDependentData.Ended)
		{
			text += $"\n{item5}";
		}
		foreach (KeyValuePair<string, List<ActivityResult>> item6 in activitiesSaveIndependentData.Ended)
		{
			text += $"\n{item6}";
		}
		text += "\n^^^\n";
		text += "NeedsTobeSendAsStarted activities:";
		foreach (string item7 in activitiesSaveDependentData.NeedsTobeSendAsStarted)
		{
			text = string.Concat(text, "\n" + item7);
		}
		foreach (string item8 in activitiesSaveIndependentData.NeedsTobeSendAsStarted)
		{
			text = string.Concat(text, "\n" + item8);
		}
		text += "\n^^^\n";
		text += "NeedsToBeSendAsEnded activities:";
		foreach (KeyValuePair<string, List<ActivityResult>> item9 in activitiesSaveDependentData.NeedsToBeSendAsEnded)
		{
			text += $"\n{item9}";
		}
		foreach (KeyValuePair<string, List<ActivityResult>> item10 in activitiesSaveIndependentData.NeedsToBeSendAsEnded)
		{
			text += $"\n{item10}";
		}
		text += "\n^^^\n";
		text += "ActiveIncludeFilterTags activities:";
		foreach (string activeIncludeFilterTag in activitiesSaveDependentData.ActiveIncludeFilterTags)
		{
			text = string.Concat(text, "\n" + activeIncludeFilterTag);
		}
		foreach (string activeIncludeFilterTag2 in activitiesSaveIndependentData.ActiveIncludeFilterTags)
		{
			text = string.Concat(text, "\n" + activeIncludeFilterTag2);
		}
		text += "\n^^^\n";
		LogUtils.Log(text);
	}

	private bool WasNotEnded(string activityID, ActivitiesProgressData activitiesSaveDependentData)
	{
		return !activitiesSaveDependentData.Ended.ContainsKey(activityID) && !activitiesSaveDependentData.EndedWithScore.ContainsKey(activityID) && !activitiesSaveDependentData.NeedsToBeSendAsEnded.ContainsKey(activityID) && !activitiesSaveDependentData.NeedsToBeSendAsEndedWithScore.ContainsKey(activityID);
	}

	private void OnGuildmasterQuestComplete(string questFullID)
	{
		if (!IsSupportActivities)
		{
			return;
		}
		string text = ToShortActivityID(questFullID);
		LogUtils.Log("[PartyAdventureData] Guildmaster quest complete: full ID - " + questFullID + "; short ID -" + text + ";");
		string text2 = text;
		if (!ActivitiesController.IsExists(text2))
		{
			return;
		}
		MarkActivityAsComplete(text2);
		string text3 = text + "_P";
		if (ActivitiesController.IsExists(text3))
		{
			MarkActivityAsComplete(text3);
			bool num = ActivitiesController.AllChildrenComplete("GuildmasterTravel");
			bool flag = ActivitiesController.AllChildrenComplete("GuildmasterStory");
			bool flag2 = ActivitiesController.AllChildrenComplete("GuildmasterRelic");
			ActivitiesProgressData saveRelatedActivitiesProgressData = SaveRelatedActivitiesProgressData;
			bool flag3 = WasNotEnded("GuildmasterTravel", saveRelatedActivitiesProgressData);
			bool flag4 = WasNotEnded("GuildmasterStory", saveRelatedActivitiesProgressData);
			bool flag5 = WasNotEnded("GuildmasterRelic", saveRelatedActivitiesProgressData);
			bool flag6 = WasNotEnded("GuildmasterProgress", saveRelatedActivitiesProgressData);
			bool flag7 = num && flag3;
			bool flag8 = flag && flag4;
			bool flag9 = flag2 && flag5;
			bool num2 = num && flag && flag2 && flag6;
			if (flag7)
			{
				EndProgressActivity("GuildmasterTravel");
			}
			if (flag8)
			{
				EndProgressActivity("GuildmasterStory");
			}
			if (flag9)
			{
				EndProgressActivity("GuildmasterRelic");
			}
			if (num2)
			{
				EndProgressActivity("GuildmasterProgress");
			}
			if (flag7 || flag8 || flag9)
			{
				LogUtils.Log("Updating activities view on progress complete...");
				ActivitiesProgressData saveRelatedActivitiesProgressData2 = SaveRelatedActivitiesProgressData;
				ActivitiesProgressData crossSaveActivitiesProgressData = CrossSaveActivitiesProgressData;
				LogActivitiesData(saveRelatedActivitiesProgressData2, crossSaveActivitiesProgressData);
				PlatformLayer.Platform.PlatformActivities.UpdateEndedActivitiesView(saveRelatedActivitiesProgressData2);
				PlatformLayer.Platform.PlatformActivities.UpdateAvailableActivitiesView(saveRelatedActivitiesProgressData2, crossSaveActivitiesProgressData);
				PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(saveRelatedActivitiesProgressData2);
			}
		}
	}

	private string ToShortActivityID(string guildmasterQuestFullID)
	{
		string text = string.Empty;
		if (guildmasterQuestFullID.Contains("Quest_Relic_"))
		{
			text = "R_" + guildmasterQuestFullID.Remove(0, "Quest_Relic_".Length);
		}
		if (guildmasterQuestFullID.Contains("Quest_Story_"))
		{
			text = "S_" + guildmasterQuestFullID.Remove(0, "Quest_Story_".Length);
		}
		if (guildmasterQuestFullID.Contains("Quest_Travel_"))
		{
			text = "T_" + guildmasterQuestFullID.Remove(0, "Quest_Travel_".Length);
		}
		if (text.Equals("S_Berserker_1"))
		{
			text = "S_Berseker_1";
		}
		return text;
	}

	private string WithDifficultyPostfix(string baseID)
	{
		if (string.IsNullOrEmpty(ActivityDifficultyPostfix))
		{
			return baseID;
		}
		return baseID + "_" + ActivityDifficultyPostfix;
	}

	private void SyncPlatformActivitiesProgress(CMapState mapState)
	{
		if (IsSupportActivities)
		{
			if (mapState != null && mapState.ActivitiesProgressData != null)
			{
				PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(mapState.ActivitiesProgressData);
			}
			else
			{
				PlatformLayer.Platform.PlatformActivities.SetSaveRelatedProgress(new ActivitiesProgressData());
			}
		}
	}
}
