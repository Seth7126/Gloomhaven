#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using GLOOM;
using Newtonsoft.Json;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;

[Serializable]
public class LevelEditorDataManager
{
	public enum ELevelFileSortType
	{
		Filename,
		DateModified,
		Extension
	}

	public const string cCustomLevelFolderPrefix = "CustomLevel_";

	public const string cWorkshopLevelFolderPrefix = "Workshop_";

	public const string cCustomLevelPreviewImageDir = "CustomLevelPreviewImages";

	public const string cCustomLevelPreviewImageFileName = "preview.png";

	public Dictionary<ulong, string> AvailableWorkshopLevels;

	public List<string> LevelEditorFileNames { get; private set; }

	public List<string> YMLFileNames { get; private set; }

	public List<FileInfo> AvailableFiles { get; private set; }

	public Dictionary<CCustomLevelData, ModMetadata> CustomLevelMetadata { get; private set; }

	public static string DefaultLevelThumbnailPath => Path.Combine(Application.streamingAssetsPath, "CustomLevelPreviewImages", "preview.png");

	public string LevelDataSaveFilePath(string levelName, bool useCustom = false)
	{
		GHRuleset gHRuleset = SceneController.Instance.Modding?.LevelEditorRuleset;
		if (gHRuleset != null)
		{
			return Path.Combine(gHRuleset.LinkedMods[0].CustomLevelsDirectory, levelName + ".lvldat");
		}
		if (PlatformLayer.FileSystem.ExistsDirectory(RootSaveData.ModsFolder))
		{
			string[] files = PlatformLayer.FileSystem.GetFiles(RootSaveData.ModsFolder, levelName + ".lvldat", SearchOption.AllDirectories);
			if (files.Length == 1)
			{
				return files[0];
			}
			if (files.Length > 1)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_LEVEL_EDITOR_MULTIPLE_LEVELS_SAME_NAME", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
				return Path.Combine(RootSaveData.LevelEditorCustomLVLDAT, "CustomLevel_" + levelName, levelName + "_Copy.lvldat");
			}
		}
		if (!useCustom)
		{
			return Path.Combine(RootSaveData.LevelEditorCustomLVLDAT, "CustomLevel_" + levelName, levelName + ".lvldat");
		}
		return Path.Combine(SaveData.Instance.Global.CustomLevelDataFolder, levelName + ".lvldat");
	}

	public string LevelDataMetaFilePath(string levelName)
	{
		string[] files = PlatformLayer.FileSystem.GetFiles(RootSaveData.ModsFolder, levelName + ".lvldat", SearchOption.AllDirectories);
		if (files.Length == 1)
		{
			return Path.Combine(Path.GetDirectoryName(files[0]), "meta.dat");
		}
		if (files.Length > 1)
		{
			return Path.Combine(RootSaveData.LevelEditorCustomLVLDAT, "CustomLevel_" + levelName, "meta.dat_Copy");
		}
		return Path.Combine(RootSaveData.LevelEditorCustomLVLDAT, "CustomLevel_" + levelName, "meta.dat");
	}

	public string WorkshopLevelSaveFilePath(string levelName, string workshopId)
	{
		return Path.Combine(RootSaveData.LevelEditorCustomLVLDAT, "Workshop_" + workshopId + "_" + levelName, levelName + ".lvldat");
	}

	public bool TrySetCustomLevelDataPath(string pathToSet, out string errorLog)
	{
		if (string.IsNullOrEmpty(pathToSet))
		{
			errorLog = "Cannot set Null path";
			return false;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(pathToSet);
		if (directoryInfo.Exists)
		{
			bool flag = true;
			AuthorizationRuleCollection accessRules = Directory.GetAccessControl(pathToSet).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
			if (directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
			{
				flag = false;
			}
			else
			{
				foreach (FileSystemAccessRule item in accessRules)
				{
					if (item.FileSystemRights.HasFlag(FileSystemRights.Write) && item.AccessControlType == AccessControlType.Deny)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				SaveData.Instance.Global.CustomLevelDataFolder = pathToSet;
				errorLog = string.Empty;
				return true;
			}
			errorLog = "Write access is denied to that folder, please choose another one.";
			return false;
		}
		errorLog = "Path does not exist, create it first";
		return false;
	}

	public LevelEditorDataManager()
	{
		LevelEditorFileNames = new List<string>();
		YMLFileNames = new List<string>();
		AvailableFiles = new List<FileInfo>();
		AvailableWorkshopLevels = new Dictionary<ulong, string>();
		CustomLevelMetadata = new Dictionary<CCustomLevelData, ModMetadata>();
	}

	public void DetermineAvailableFilesFromLoadFolder(bool useCustomLoadFolder = false, bool useModdedFolder = false)
	{
		LevelEditorFileNames.Clear();
		YMLFileNames.Clear();
		AvailableFiles.Clear();
		if (!useCustomLoadFolder && !useModdedFolder)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(RootSaveData.RulebaseCustomLevelEditorScenarios);
			if (directoryInfo.Exists)
			{
				AddFiles(directoryInfo.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
			DirectoryInfo directoryInfo2 = new DirectoryInfo(RootSaveData.RulebaseCustomYMLScenarios);
			if (directoryInfo2.Exists)
			{
				AddFiles(directoryInfo2.GetFiles("*.yml", SearchOption.AllDirectories), isYML: true);
			}
			DirectoryInfo directoryInfo3 = new DirectoryInfo(RootSaveData.LevelEditorCustomLVLDAT);
			if (directoryInfo3.Exists)
			{
				AddFiles(directoryInfo3.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
			DirectoryInfo directoryInfo4 = new DirectoryInfo(RootSaveData.LevelEditorCustomYML);
			if (directoryInfo4.Exists)
			{
				AddFiles(directoryInfo4.GetFiles("*.yml", SearchOption.AllDirectories), isYML: true);
			}
		}
		else if (!string.IsNullOrEmpty(SaveData.Instance.Global.CustomLevelDataFolder))
		{
			DirectoryInfo directoryInfo5 = new DirectoryInfo(SaveData.Instance.Global.CustomLevelDataFolder);
			if (directoryInfo5.Exists)
			{
				AddFiles(directoryInfo5.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
		}
		else if (SceneController.Instance.Modding?.LevelEditorRuleset != null)
		{
			DirectoryInfo directoryInfo6 = new DirectoryInfo(SceneController.Instance.Modding.LevelEditorRuleset.LinkedMods[0].CustomLevelsDirectory);
			if (directoryInfo6.Exists)
			{
				AddFiles(directoryInfo6.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
			}
		}
		if (SceneController.Instance.Modding == null || useModdedFolder)
		{
			return;
		}
		foreach (GHMod item in SceneController.Instance.Modding.Mods.Where((GHMod it) => it.IsValid && it.MetaData.ModType == GHModMetaData.EModType.CustomLevels))
		{
			DirectoryInfo directoryInfo7 = new DirectoryInfo(item.CustomLevelsDirectory);
			if (directoryInfo7.Exists)
			{
				AddFiles(directoryInfo7.GetFiles("*.lvldat", SearchOption.AllDirectories), isYML: false);
				AddFiles(directoryInfo7.GetFiles("*.yml", SearchOption.AllDirectories), isYML: true);
			}
		}
	}

	private void AddFiles(FileInfo[] files, bool isYML)
	{
		if (files.Length != 0)
		{
			AvailableFiles.AddRange(files);
			string[] array = files.Select((FileInfo s) => s.FullName).ToArray();
			try
			{
				Array.Sort(array, new AlphanumComparatorFast());
			}
			catch
			{
			}
			if (isYML)
			{
				YMLFileNames.AddRange(array.ToList());
			}
			else
			{
				LevelEditorFileNames.AddRange(array.ToList());
			}
		}
	}

	public void SortAvailableFiles(ELevelFileSortType sortType, bool ascending)
	{
		if (AvailableFiles == null || AvailableFiles.Count == 0)
		{
			return;
		}
		switch (sortType)
		{
		case ELevelFileSortType.Filename:
			AvailableFiles.Sort((FileInfo x, FileInfo y) => (!ascending) ? y.Name.CompareTo(x.Name) : x.Name.CompareTo(y.Name));
			break;
		case ELevelFileSortType.DateModified:
			AvailableFiles.Sort((FileInfo x, FileInfo y) => (!ascending) ? DateTime.Compare(y.LastWriteTime, x.LastWriteTime) : DateTime.Compare(x.LastWriteTime, y.LastWriteTime));
			break;
		case ELevelFileSortType.Extension:
			AvailableFiles.Sort((FileInfo x, FileInfo y) => (!ascending) ? y.Extension.CompareTo(x.Extension) : x.Extension.CompareTo(y.Extension));
			break;
		}
	}

	public bool SaveLevelEditorBaseData(CCustomLevelData baseLevelData, string levelName, bool saveReadableJson = false, bool usingCustomLevelFolder = false)
	{
		try
		{
			string text = SceneController.Instance.Modding?.LevelEditorRuleset?.LinkedMods.First().CustomLevelsDirectory;
			if (!usingCustomLevelFolder && text == null)
			{
				if (!CustomLevelMetadata.ContainsKey(baseLevelData))
				{
					CustomLevelMetadata.Add(baseLevelData, new ModMetadata());
				}
				ModMetadata modMetadataForCustomLevel = GetModMetadataForCustomLevel(baseLevelData);
				modMetadataForCustomLevel.Type = ModMetadata.ModType.Level;
				string path = LevelDataMetaFilePath(levelName);
				string directoryName = Path.GetDirectoryName(path);
				if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(directoryName))
				{
					PlatformLayer.FileSystem.CreateDirectory(directoryName);
				}
				using MemoryStream memoryStream = new MemoryStream();
				new BinaryFormatter().Serialize(memoryStream, modMetadataForCustomLevel);
				PlatformLayer.FileSystem.WriteFile(memoryStream.ToArray(), path);
			}
			string path2 = LevelDataSaveFilePath(levelName, usingCustomLevelFolder);
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				new BinaryFormatter().Serialize(memoryStream2, baseLevelData);
				PlatformLayer.FileSystem.WriteFile(memoryStream2.ToArray(), path2);
			}
			if (!usingCustomLevelFolder && text == null)
			{
				string text2 = Path.Combine(Path.GetDirectoryName(path2), "preview.png");
				if (!PlatformLayer.FileSystem.ExistsFile(text2) && PlatformLayer.FileSystem.ExistsFile(DefaultLevelThumbnailPath))
				{
					PlatformLayer.FileSystem.CopyFile(DefaultLevelThumbnailPath, text2, overwrite: false);
				}
			}
			if (saveReadableJson)
			{
				using MemoryStream memoryStream3 = new MemoryStream();
				string graph = JsonConvert.SerializeObject(baseLevelData, Formatting.Indented);
				new BinaryFormatter().Serialize(memoryStream3, graph);
				PlatformLayer.FileSystem.WriteFile(memoryStream3.ToArray(), LevelDataSaveFilePath("JSON_" + levelName, usingCustomLevelFolder));
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to save LevelEditorBase data.\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public CCustomLevelData GetLevelDataForFile(FileInfo file)
	{
		string fullPath = Path.GetFullPath(file.FullName);
		if (PlatformLayer.FileSystem.ExistsFile(fullPath))
		{
			if (file.Extension == ".lvldat")
			{
				try
				{
					CCustomLevelData cCustomLevelData = null;
					using (MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(fullPath)))
					{
						SaveData.Instance.CurrentFileBeingDeserialized = fullPath;
						cCustomLevelData = new BinaryFormatter
						{
							Binder = new SerializationBinding()
						}.Deserialize(serializationStream) as CCustomLevelData;
					}
					ModMetadata customLevelMetaDataForLevelFile = GetCustomLevelMetaDataForLevelFile(file);
					if (customLevelMetaDataForLevelFile != null)
					{
						if (!CustomLevelMetadata.ContainsKey(cCustomLevelData))
						{
							CustomLevelMetadata.Add(cCustomLevelData, customLevelMetaDataForLevelFile);
						}
						else
						{
							CustomLevelMetadata[cCustomLevelData] = customLevelMetaDataForLevelFile;
						}
					}
					return cCustomLevelData;
				}
				catch (Exception ex)
				{
					Debug.LogError("Unable to get LevelData object from path.\n" + ex.Message + "\n" + ex.StackTrace);
				}
			}
			else if (file.Extension == ".yml")
			{
				ScenarioDefinition scenarioDefinition = ScenarioRuleClient.SRLYML.Scenarios.SingleOrDefault((ScenarioDefinition s) => Path.GetFileName(s.FileName) == file.Name);
				if (scenarioDefinition != null)
				{
					return new CCustomLevelData(LocalizationManager.GetTranslation(scenarioDefinition.ID), scenarioDefinition);
				}
				Debug.LogError("Unable to find Custom Scenario " + file.Name);
			}
			else
			{
				Debug.LogError("Invlid file extension for custom file '" + file.Extension + "'");
			}
		}
		return null;
	}

	public CCustomLevelData GetLevelDataForFile(string filePath)
	{
		FileInfo levelFile = new FileInfo(filePath);
		if (PlatformLayer.FileSystem.ExistsFile(filePath))
		{
			try
			{
				CCustomLevelData cCustomLevelData = null;
				using (MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(filePath)))
				{
					SaveData.Instance.CurrentFileBeingDeserialized = filePath;
					cCustomLevelData = new BinaryFormatter
					{
						Binder = new SerializationBinding()
					}.Deserialize(serializationStream) as CCustomLevelData;
				}
				ModMetadata customLevelMetaDataForLevelFile = GetCustomLevelMetaDataForLevelFile(levelFile);
				if (customLevelMetaDataForLevelFile != null)
				{
					if (!CustomLevelMetadata.ContainsKey(cCustomLevelData))
					{
						CustomLevelMetadata.Add(cCustomLevelData, customLevelMetaDataForLevelFile);
					}
					else
					{
						CustomLevelMetadata[cCustomLevelData] = customLevelMetaDataForLevelFile;
					}
				}
				return cCustomLevelData;
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to get LevelData object from path.\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		return null;
	}

	public ModMetadata GetCustomLevelMetaDataForLevelFile(FileInfo levelFile)
	{
		string text = Path.Combine(levelFile.DirectoryName, "meta.dat");
		if (PlatformLayer.FileSystem.ExistsFile(text))
		{
			using (MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(text)))
			{
				SaveData.Instance.CurrentFileBeingDeserialized = text;
				return new BinaryFormatter
				{
					Binder = new SerializationBinding()
				}.Deserialize(serializationStream) as ModMetadata;
			}
		}
		return null;
	}

	public void UpdateWorkshopLevel(CCustomLevelData levelToUpdate)
	{
		try
		{
			ModMetadata modMetadataForCustomLevel = GetModMetadataForCustomLevel(levelToUpdate);
			if (modMetadataForCustomLevel == null)
			{
				Debug.LogWarning("Cannot find meta data of level to update");
				return;
			}
			if (!AvailableWorkshopLevels.ContainsKey(modMetadataForCustomLevel.PublishedFileId))
			{
				Debug.LogWarning("Cannot find workshop level to update");
				return;
			}
			string directoryName = Path.GetDirectoryName(WorkshopLevelSaveFilePath(levelToUpdate.Name, modMetadataForCustomLevel.PublishedFileId.ToString()));
			if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(directoryName))
			{
				Debug.Log("Creating local workshop directory: " + directoryName);
				PlatformLayer.FileSystem.CreateDirectory(directoryName);
			}
			FileInfo[] files = new DirectoryInfo(AvailableWorkshopLevels[modMetadataForCustomLevel.PublishedFileId]).GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				PlatformLayer.FileSystem.CopyFile(fileInfo.FullName, Path.Combine(directoryName, fileInfo.Name), overwrite: true);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to update custom workshop level:\n" + ex.Message);
		}
	}

	public bool CheckIfLevelIsFromWorkshop(CCustomLevelData levelToCheck, FileInfo levelsFile)
	{
		try
		{
			ModMetadata modMetadataForCustomLevel = GetModMetadataForCustomLevel(levelToCheck);
			if (modMetadataForCustomLevel == null)
			{
				Debug.LogWarning("Failed to find associated Level Meta data when trying to check if level is from workshop");
				return false;
			}
			FileInfo fileInfo = new FileInfo(WorkshopLevelSaveFilePath(levelToCheck.Name, modMetadataForCustomLevel.PublishedFileId.ToString()));
			string name = levelsFile.Directory.Name;
			return fileInfo.Directory.Name == name;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to check if custom workshop level is from teh steam workshop:\n" + ex.Message);
			return false;
		}
	}

	public bool CheckIfWorkshopLevelNeedUpdate(CCustomLevelData levelToCheck)
	{
		try
		{
			ModMetadata modMetadataForCustomLevel = GetModMetadataForCustomLevel(levelToCheck);
			if (modMetadataForCustomLevel == null)
			{
				Debug.LogWarning("Cannot find meta data of level to check for update");
				return false;
			}
			if (!AvailableWorkshopLevels.ContainsKey(modMetadataForCustomLevel.PublishedFileId))
			{
				return false;
			}
			FileInfo levelFile = new DirectoryInfo(AvailableWorkshopLevels[modMetadataForCustomLevel.PublishedFileId]).GetFiles("*.lvldat", SearchOption.AllDirectories)[0];
			return GetCustomLevelMetaDataForLevelFile(levelFile).Version > modMetadataForCustomLevel.Version;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to check if custom workshop level needs update:\n" + ex.Message);
			return false;
		}
	}

	public ModMetadata GetModMetadataForCustomLevel(CCustomLevelData levelToGetMetaDataFor)
	{
		if (levelToGetMetaDataFor == null)
		{
			return null;
		}
		if (CustomLevelMetadata.ContainsKey(levelToGetMetaDataFor))
		{
			return CustomLevelMetadata[levelToGetMetaDataFor];
		}
		return null;
	}
}
