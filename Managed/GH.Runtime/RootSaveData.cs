#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using I2.Loc;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

[Serializable]
public class RootSaveData : ISerializable
{
	public enum EReleaseTypes
	{
		None,
		Release,
		OpenBeta,
		ClosedBeta,
		Dev,
		LocalDev,
		InEditor
	}

	public const int RootSaveVersion = 1;

	public const string DefaultSaveFileExtension = ".dat";

	public const string AutotestSaveFileExtension = ".testdat";

	public const string BaseLevelSaveFileExtension = ".lvldat";

	public const string CustomRootFolder = "CustomScenarioSaves";

	public const string CustomScenariosFolder = "CustomScenarios";

	public const string LevelEditorStatesRootFolder = "LevelEditor";

	public const string LevelEditorCustomLVLDATFolder = "LevelEditorScenarios";

	public const string LevelEditorCustomYMLFolder = "YMLScenarios";

	public const string LevelEditorRulebaseYMLFolder = "SingleScenarioYML";

	public const string AutotestRootFolder = "Autotests";

	public const string GuildmasterSavesFolder = "Guildmaster";

	public const string OldGuildmasterSavesFolder = "AdventureV2";

	public const string CampaignSavesFolder = "Campaign";

	public const string ScenarioSavesFolder = "SingleScenarios";

	public const string CrossSaveActivitiesSaveFolder = "CrossSaveActivities";

	public const string CheckpointSavesFolder = "Checkpoints";

	public const string CorruptedSavesFolder = "CorruptedSaves";

	public const string BackupSavesFolder = "BackupSaves";

	public const string RootSaveFileName = "GloomSaven.dat";

	public const string GlobalDataSaveFileName = "GlobalData.dat";

	public const string AutoLogSaveFileName = "GloomAutoLog.dat";

	public const string ModsFolderName = "SteamMods";

	public const string ModdedRulesetsFolderName = "ModdedRulesets";

	public const string ModdingExportDir = "ModdingExport";

	public const string ModdingValidation = "ModValidation";

	public const string MultiplayerLocalization = "MultiplayerLocalization";

	public const string EditorPropertiesSaveFileName = "EditorProperties.dat";

	[NonSerialized]
	private static EReleaseTypes _releaseVersionType;

	public int Version;

	public string CurrentLanguage;

	public static EReleaseTypes ReleaseVersionType
	{
		get
		{
			if (_releaseVersionType == EReleaseTypes.None)
			{
				InitReleaseVersion();
			}
			return _releaseVersionType;
		}
	}

	public static string SaveRootFolder
	{
		get
		{
			switch (ReleaseVersionType)
			{
			case EReleaseTypes.OpenBeta:
				return "GloomSavesOpenBeta";
			case EReleaseTypes.ClosedBeta:
				return "GloomSavesClosedBeta";
			case EReleaseTypes.Dev:
				return "GloomSavesDev";
			case EReleaseTypes.LocalDev:
				return "GloomSavesLocalDev";
			case EReleaseTypes.InEditor:
				return "GloomSavesInEditor";
			case EReleaseTypes.Release:
				return "GloomSaves";
			default:
				Debug.LogError("The ReleaseType has not been set.  Using default save location");
				return "GloomSaves";
			}
		}
	}

	private static string _dataPath
	{
		get
		{
			if (!(SceneController.Instance == null))
			{
				return SceneController.Instance.ApplicationDataPath;
			}
			return Application.dataPath;
		}
	}

	public static string CoreRulebasePath => Path.Combine(Application.streamingAssetsPath, "Rulebase");

	public static string RootSaveFolder => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder);

	public static string PlayerLogPath => Path.Combine(SaveData.Instance.PersistentDataPath, "Player.log");

	public static string TempPlayerLogPath => Path.Combine(SaveData.Instance.PersistentDataPath, "TempPlayer.log");

	public static string SimpleLogPath => Path.Combine(SaveData.Instance.PersistentDataPath, "Simple.log");

	public static string SimpleLogPrevPath => Path.Combine(SaveData.Instance.PersistentDataPath, "Simple-prev.log");

	public static string PlayerLogPathFromHost => Path.Combine(SaveData.Instance.PersistentDataPath, "HostPlayer.log");

	public static string SimpleLogPathFromHost => Path.Combine(SaveData.Instance.PersistentDataPath, "HostSimple.log");

	public static string ScreenCaptureImagePath => Path.Combine(SaveData.Instance.PersistentDataPath, "ErrorSceenCap.png");

	public static string RootSaveFile => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "GloomSaven.dat");

	public static string EditorPropertiesSaveFile => Path.Combine(_dataPath, "..", "EditorProperties.dat");

	public static string LevelEditorCustomLevels => Path.Combine(SaveData.Instance.PersistentDataPath, "LevelEditor");

	public static string LevelEditorCustomLVLDAT => Path.Combine(LevelEditorCustomLevels, "LevelEditorScenarios");

	public static string LevelEditorCustomYML => Path.Combine(LevelEditorCustomLevels, "YMLScenarios");

	public static string RulebaseCustomLevelEditorScenarios => Path.Combine(CoreRulebasePath, "CustomScenarios", "LevelEditorScenarios");

	public static string RulebaseCustomYMLScenarios => Path.Combine(CoreRulebasePath, "CustomScenarios", "SingleScenarioYML");

	public static string ModsFolder => Path.Combine(SaveData.Instance.Global.ModdingDirectory, "SteamMods");

	public static string ModdedRulesetsFolder => Path.Combine(SaveData.Instance.Global.ModdingDirectory, "ModdedRulesets");

	public static string ModdingExportFolder => Path.Combine(SaveData.Instance.PersistentDataPath, "ModdingExport");

	public static string ModValidationFolder => Path.Combine(SaveData.Instance.Global.ModdingDirectory, "ModValidation");

	public static string MultiplayerLocalizationFolder => Path.Combine(SaveData.Instance.PersistentDataPath, "MultiplayerLocalization");

	public static string AutotestRuntimePath => Path.Combine(SaveData.Instance.PersistentDataPath, "Autotests");

	public static string AutotestEditorPath
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(_dataPath, "..", "Autotests"));
			}
			return Path.GetFullPath(Path.Combine(_dataPath, "..", "..", "Autotests"));
		}
	}

	public static string AutoTestPath
	{
		get
		{
			if (!PlatformLayer.FileSystem.ExistsDirectory(AutotestRuntimePath))
			{
				Debug.Log("Creating AutotestRuntimePath directory: " + AutotestRuntimePath);
				PlatformLayer.FileSystem.CreateDirectory(AutotestRuntimePath);
			}
			return AutotestRuntimePath;
		}
	}

	public string GlobalSaveFile => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "GlobalData.dat");

	public string PreviousGlobalSaveFile => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "Core.dat");

	public string CustomRootPath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "CustomScenarioSaves");

	public string JSONEventLogPath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "EventLog.JSON");

	public string CorruptedSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "CorruptedSaves");

	public string PreDLCBackupSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "BackupSaves");

	public string CampaignSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "Campaign");

	public string GuildmasterSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "Guildmaster");

	public string OldGuildmasterSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "AdventureV2");

	public string SingleScenarioSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "SingleScenarios");

	public string CrossSaveActivitiesSavePath => Path.Combine(SaveData.Instance.PersistentDataPath, SaveRootFolder, "CrossSaveActivities");

	public string CurrentlyUsedSavePath
	{
		get
		{
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Guildmaster:
				return SaveData.Instance.Global.AdventureData.AdventureMapStateFilePath;
			case EGameMode.Campaign:
				return SaveData.Instance.Global.CampaignData.AdventureMapStateFilePath;
			case EGameMode.SingleScenario:
			{
				CCustomLevelData currentCustomLevelData = SaveData.Instance.Global.CurrentCustomLevelData;
				foreach (FileInfo availableFile in SaveData.Instance.LevelEditorDataManager.AvailableFiles)
				{
					if (SaveData.Instance.LevelEditorDataManager.CheckIfLevelIsFromWorkshop(currentCustomLevelData, availableFile))
					{
						ModMetadata modMetadataForCustomLevel = SaveData.Instance.LevelEditorDataManager.GetModMetadataForCustomLevel(currentCustomLevelData);
						if (modMetadataForCustomLevel != null)
						{
							return SaveData.Instance.LevelEditorDataManager.WorkshopLevelSaveFilePath(currentCustomLevelData.Name, modMetadataForCustomLevel.PublishedFileId.ToString());
						}
						return null;
					}
				}
				return SaveData.Instance.LevelEditorDataManager.LevelDataSaveFilePath(currentCustomLevelData.Name);
			}
			default:
				return null;
			}
		}
	}

	public static string DLCPackageFolder(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(CoreRulebasePath, "DLC", GloomUtility.GetEnumCategory(dlcEnum));
	}

	public static string GetModValidationLogPath(string modName)
	{
		return Path.Combine(ModValidationFolder, "ModValidation_" + modName + "_" + DateTime.Now.ToString("MM_dd_yyy_HH_mm_ss") + ".txt");
	}

	public static string GetRulesetValidationLogPath(string rulesetName)
	{
		return Path.Combine(ModValidationFolder, "RulesetValidation_" + rulesetName + "_" + DateTime.Now.ToString("MM_dd_yyy_HH_mm_ss") + ".txt");
	}

	public static string SimpleLogFromClientPath(string clientName)
	{
		return Path.Combine(SaveData.Instance.PersistentDataPath, "ClientSimple-" + clientName + ".log");
	}

	public static string PlayerLogFromClientPath(string clientName)
	{
		return Path.Combine(SaveData.Instance.PersistentDataPath, "ClientPlayer-" + clientName + ".log");
	}

	public static void CleanClientSimpleLogs()
	{
		try
		{
			string[] files = PlatformLayer.FileSystem.GetFiles(SaveData.Instance.PersistentDataPath, "ClientSimple-*.log");
			foreach (string path in files)
			{
				PlatformLayer.FileSystem.RemoveFile(path);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to delete client simple logs.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void CleanClientPlayerLogs()
	{
		try
		{
			string[] files = PlatformLayer.FileSystem.GetFiles(SaveData.Instance.PersistentDataPath, "ClientPlayer-*.log");
			foreach (string path in files)
			{
				PlatformLayer.FileSystem.RemoveFile(path);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception trying to delete client player logs.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static string[] GetAllClientSimpleLogs()
	{
		List<string> list = new List<string>();
		string[] files = PlatformLayer.FileSystem.GetFiles(SaveData.Instance.PersistentDataPath, "ClientSimple-*.log");
		foreach (string item in files)
		{
			list.Add(item);
		}
		return list.ToArray();
	}

	public static string[] GetAllClientPlayerLogs()
	{
		List<string> list = new List<string>();
		string[] files = PlatformLayer.FileSystem.GetFiles(SaveData.Instance.PersistentDataPath, "ClientPlayer-*.log");
		foreach (string item in files)
		{
			list.Add(item);
		}
		return list.ToArray();
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Version", Version);
		info.AddValue("CurrentLanguage", CurrentLanguage);
	}

	public RootSaveData(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "Version"))
				{
					if (name == "CurrentLanguage")
					{
						CurrentLanguage = info.GetString("CurrentLanguage");
					}
				}
				else
				{
					Version = info.GetInt32("Version");
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize RootSaveData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (Version != 1)
		{
			Debug.Log("Root Save data version has been bumped.  Current root data will be updated.");
			Version = 1;
			CurrentLanguage = LocalizationManager.CurrentLanguage;
		}
	}

	public string GetFilePathForParty(EGameMode gameMode, string partyName, string hostAccountID)
	{
		return gameMode switch
		{
			EGameMode.Campaign => Path.Combine(CampaignSavePath, EGameMode.Campaign.ToString() + "_" + partyName.Replace(" ", "_") + "_" + hostAccountID, EGameMode.Campaign.ToString() + "_" + partyName.Replace(" ", "_") + "_" + hostAccountID + ".dat"), 
			EGameMode.Guildmaster => Path.Combine(GuildmasterSavePath, EGameMode.Guildmaster.ToString() + "_" + partyName.Replace(" ", "_") + "_" + hostAccountID, EGameMode.Guildmaster.ToString() + "_" + partyName.Replace(" ", "_") + "_" + hostAccountID + ".dat"), 
			_ => null, 
		};
	}

	public string GetDefaultFilePathForCustomLevel(string levelName, string workshopID = "")
	{
		if (workshopID.IsNullOrEmpty())
		{
			return SaveData.Instance.LevelEditorDataManager.LevelDataSaveFilePath(levelName);
		}
		return SaveData.Instance.LevelEditorDataManager.WorkshopLevelSaveFilePath(levelName, workshopID);
	}

	public RootSaveData()
	{
	}

	public static void InitReleaseVersion()
	{
		string version = Application.version;
		if (version.Contains("-dev"))
		{
			_releaseVersionType = EReleaseTypes.Dev;
		}
		else if (version.Contains("-obeta"))
		{
			_releaseVersionType = EReleaseTypes.OpenBeta;
		}
		else if (version.Contains("-cbeta"))
		{
			_releaseVersionType = EReleaseTypes.ClosedBeta;
		}
		else if (version == "0.0.0.0")
		{
			_releaseVersionType = EReleaseTypes.LocalDev;
		}
		else
		{
			_releaseVersionType = EReleaseTypes.Release;
		}
	}

	public static RootSaveData CreateRootSaveData()
	{
		string text = Application.systemLanguage.ToString();
		Debug.Log($"[RootSaveData] Creating RootSaveData. System Language: {text}. IsConsole: {PlatformLayer.Instance.IsConsole}");
		return new RootSaveData
		{
			Version = 1,
			CurrentLanguage = (PlatformLayer.Instance.IsConsole ? text : LocalizationManager.CurrentLanguage)
		};
	}
}
