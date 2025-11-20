#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FFSThreads;
using GLOOM;
using I2.Loc;
using Ionic.Zip;
using JetBrains.Annotations;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using Utilities;

public class GHMod
{
	public const string LanguageUpdateFileName = "LangUpdate.csv";

	private const string EmptyLangCSV = "Key,English [en-GB]\r\n";

	public GHModMetaData MetaData { get; private set; }

	public string LocalPath { get; private set; }

	public float Rating { get; set; }

	public bool IsLocalMod { get; private set; }

	public bool IsValid { get; private set; }

	public string LastValidationResultsFile { get; private set; }

	public string LangPacksDirectory => Path.Combine(LocalPath, "LangPacks");

	public string LangUpdateDirectory => Path.Combine(LocalPath, "LangUpdates");

	public string LangUpdateFilePath => Path.Combine(LangUpdateDirectory, "LangUpdate.csv");

	public string ModdedYMLDirectory => Path.Combine(LocalPath, "ModdedYML");

	public string CustomLevelsDirectory => Path.Combine(LocalPath, "ModdedCustomLevels");

	public string CustomYMLScenariosDirectory => Path.Combine(LocalPath, "ModdedCustomLevels", "ScenarioYML");

	public string CustomImagesDirectory => Path.Combine(LocalPath, "ModdedCustomResources", "Images");

	public string[] LanguageCSVs
	{
		get
		{
			if (PlatformLayer.FileSystem.ExistsDirectory(LangPacksDirectory))
			{
				return PlatformLayer.FileSystem.GetFiles(LangPacksDirectory, "*.csv");
			}
			return new string[0];
		}
	}

	public string[] ModdedYMLFiles
	{
		get
		{
			List<string> list = new List<string>();
			if (PlatformLayer.FileSystem.ExistsDirectory(ModdedYMLDirectory))
			{
				list.AddRange(PlatformLayer.FileSystem.GetFiles(ModdedYMLDirectory, "*.yml", SearchOption.AllDirectories));
			}
			if (PlatformLayer.FileSystem.ExistsDirectory(CustomYMLScenariosDirectory))
			{
				list.AddRange(PlatformLayer.FileSystem.GetFiles(CustomYMLScenariosDirectory, "*.yml", SearchOption.AllDirectories));
			}
			return list.ToArray();
		}
	}

	public string[] CustomLevelFiles
	{
		get
		{
			if (PlatformLayer.FileSystem.ExistsDirectory(CustomLevelsDirectory))
			{
				return PlatformLayer.FileSystem.GetFiles(CustomLevelsDirectory, "*.lvldat", SearchOption.AllDirectories);
			}
			return new string[0];
		}
	}

	public string[] CustomImageFiles
	{
		get
		{
			if (PlatformLayer.FileSystem.ExistsDirectory(CustomImagesDirectory))
			{
				return PlatformLayer.FileSystem.GetFiles(CustomImagesDirectory, "*.png", SearchOption.AllDirectories);
			}
			return new string[0];
		}
	}

	public bool Compile()
	{
		return true;
	}

	public GHMod(GHModMetaData metaData, string localPath, float rating, bool isLocalMod)
	{
		MetaData = metaData;
		LocalPath = localPath;
		Rating = rating;
		IsLocalMod = isLocalMod;
		IsValid = !isLocalMod;
	}

	public GHMod(GHModMetaData metaData, string localPath)
	{
		MetaData = metaData;
		LocalPath = localPath;
		Rating = -1f;
		IsLocalMod = true;
		IsValid = false;
	}

	public bool CompileMod()
	{
		return true;
	}

	public static GHMod CreateNewMod(GHModMetaData metaData)
	{
		try
		{
			if (!PlatformLayer.FileSystem.ExistsDirectory(RootSaveData.ModsFolder))
			{
				Debug.Log("Creating Steam Mods directory: " + RootSaveData.ModsFolder);
				PlatformLayer.FileSystem.CreateDirectory(RootSaveData.ModsFolder);
			}
			string text = Path.Combine(RootSaveData.ModsFolder, metaData.Name);
			if (PlatformLayer.FileSystem.ExistsDirectory(text))
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_MOD_ALREADY_EXISTS", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
				return null;
			}
			Debug.Log("Creating mod directory: " + text);
			PlatformLayer.FileSystem.CreateDirectory(text);
			GHMod gHMod = new GHMod(metaData, text);
			Debug.Log("Creating mod YML/CustomLevels/LangUpdates directories");
			switch (metaData.ModType)
			{
			case GHModMetaData.EModType.Guildmaster:
			case GHModMetaData.EModType.Campaign:
			case GHModMetaData.EModType.Global:
				PlatformLayer.FileSystem.CreateDirectory(gHMod.ModdedYMLDirectory);
				PlatformLayer.FileSystem.CreateDirectory(gHMod.CustomLevelsDirectory);
				PlatformLayer.FileSystem.CreateDirectory(gHMod.CustomYMLScenariosDirectory);
				PlatformLayer.FileSystem.CreateDirectory(gHMod.LangUpdateDirectory);
				PlatformLayer.FileSystem.CreateDirectory(gHMod.CustomImagesDirectory);
				PlatformLayer.FileSystem.FileWriteAllText(gHMod.LangUpdateFilePath, "Key,English [en-GB]\r\n");
				if (gHMod.MetaData.Save(text))
				{
					return gHMod;
				}
				return null;
			case GHModMetaData.EModType.CustomLevels:
				PlatformLayer.FileSystem.CreateDirectory(gHMod.CustomLevelsDirectory);
				PlatformLayer.FileSystem.CreateDirectory(gHMod.CustomYMLScenariosDirectory);
				if (gHMod.MetaData.Save(text))
				{
					return gHMod;
				}
				return null;
			case GHModMetaData.EModType.Language:
			{
				PlatformLayer.FileSystem.CreateDirectory(gHMod.LangPacksDirectory);
				IList<string> list = ExportLanguageCSV(gHMod.LangPacksDirectory, PlatformLayer.FileSystem);
				if (list != null)
				{
					gHMod.MetaData.AppliedFiles.AddRange(list);
					if (gHMod.MetaData.Save(text))
					{
						return gHMod;
					}
					return null;
				}
				return null;
			}
			default:
				throw new Exception("Unsupported mod type " + metaData.ModType);
			}
		}
		catch (Exception ex)
		{
			try
			{
				string path = Path.Combine(RootSaveData.ModsFolder, metaData.Name);
				if (PlatformLayer.FileSystem.ExistsDirectory(path))
				{
					PlatformLayer.FileSystem.RemoveDirectory(path, recursive: true);
				}
			}
			catch
			{
			}
			Debug.LogError("Exception while trying to create new mod.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_ERROR_CREATING_MOD", "GUI_OK", ex.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, ex.Message);
			return null;
		}
	}

	public static void ChangeModdingDirectory(string pathToChangeTo, ThreadMessageSender sender)
	{
		try
		{
			Debug.Log("Changing modding directory: " + pathToChangeTo);
			if (!PlatformLayer.FileSystem.ExistsDirectory(Path.Combine(pathToChangeTo, "SteamMods")))
			{
				PlatformLayer.FileSystem.CreateDirectory(Path.Combine(pathToChangeTo, "SteamMods"));
			}
			if (!PlatformLayer.FileSystem.ExistsDirectory(Path.Combine(pathToChangeTo, "ModdedRulesets")))
			{
				PlatformLayer.FileSystem.CreateDirectory(Path.Combine(pathToChangeTo, "ModdedRulesets"));
			}
			if (!PlatformLayer.FileSystem.ExistsDirectory(Path.Combine(pathToChangeTo, "ModValidation")))
			{
				PlatformLayer.FileSystem.CreateDirectory(Path.Combine(pathToChangeTo, "ModValidation"));
			}
			if (GloomUtility.MoveFolder(RootSaveData.ModsFolder, Path.Combine(pathToChangeTo, "SteamMods"), sender) && GloomUtility.MoveFolder(RootSaveData.ModdedRulesetsFolder, Path.Combine(pathToChangeTo, "ModdedRulesets"), sender) && GloomUtility.MoveFolder(RootSaveData.ModValidationFolder, Path.Combine(pathToChangeTo, "ModValidation"), sender))
			{
				SaveData.Instance.Global.ModdingDirectory = pathToChangeTo;
			}
			else
			{
				sender.SendMessage(new ThreadMessage_ShowErrorMessage("GUI_MODDING_CHANGE_DIRECTORY_FAILED"));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occured in ChangeModdingDirectory.\nException: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	[CanBeNull]
	public static IList<string> ExportLanguageCSV(string pathToExportTo, IFileService fileService)
	{
		List<string> list = new List<string>();
		if (fileService.ExistsDirectory(pathToExportTo))
		{
			string text = Path.Combine(pathToExportTo, "LanguageExport.csv");
			try
			{
				string content = I2.Loc.LocalizationManager.Sources[0].Export_CSV2(new string[4] { "English [en-GB]", "French", "German", "Spanish" });
				string text2 = I2.Loc.LocalizationManager.Sources.FirstOrDefault((LanguageSourceData x) => x.GetCategories().Contains("Consoles"))?.Export_CSV2(new string[4] { "English [en-GB]", "French", "German", "Spanish" });
				fileService.FileWriteAllText(text, content);
				if (text2 != null)
				{
					string path = Path.Combine(pathToExportTo, "LanguageExport_Consoles.csv");
					fileService.FileWriteAllText(path, text2);
					list.Add(text2);
				}
				list.Add(text);
				return list;
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred in ExportCurrentLanguageData.\nException: " + ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}
		Debug.LogError("Path " + pathToExportTo + " does not exist.  Unable to export Language CSV");
		return null;
	}

	public static bool ExportYML(string pathToExportTo, GHRuleset.ERulesetType rulesetType)
	{
		if (PlatformLayer.FileSystem.ExistsDirectory(pathToExportTo))
		{
			try
			{
				bool result = true;
				if (PlatformLayer.FileSystem.ExistsFile(SceneController.Instance.YML.GlobalRulesetZip))
				{
					ZipFile.Read(SceneController.Instance.YML.GlobalRulesetZip).ExtractAll(Path.Combine(pathToExportTo, "Global"));
				}
				else
				{
					Debug.LogError("Failed to extract Global YML");
					result = false;
				}
				if (PlatformLayer.FileSystem.ExistsFile(SceneController.Instance.YML.SharedRulesetZip))
				{
					ZipFile.Read(SceneController.Instance.YML.SharedRulesetZip).ExtractAll(Path.Combine(pathToExportTo, "Shared"));
				}
				else
				{
					Debug.LogError("Failed to extract Shared YML");
					result = false;
				}
				if (rulesetType == GHRuleset.ERulesetType.Guildmaster || rulesetType == GHRuleset.ERulesetType.None)
				{
					if (PlatformLayer.FileSystem.ExistsFile(SceneController.Instance.YML.GuildmasterRulesetZip))
					{
						ZipFile.Read(SceneController.Instance.YML.GuildmasterRulesetZip).ExtractAll(Path.Combine(pathToExportTo, "Guildmaster"));
					}
					else
					{
						Debug.LogError("Failed to extract Guildmaster YML");
						result = false;
					}
				}
				if (rulesetType == GHRuleset.ERulesetType.Campaign || rulesetType == GHRuleset.ERulesetType.None)
				{
					if (PlatformLayer.FileSystem.ExistsFile(SceneController.Instance.YML.CampaignRulesetZip))
					{
						ZipFile.Read(SceneController.Instance.YML.CampaignRulesetZip).ExtractAll(Path.Combine(pathToExportTo, "Campaign"));
					}
					else
					{
						Debug.LogError("Failed to extract Campaign YML");
						result = false;
					}
				}
				foreach (DLCRegistry.EDLCKey item in DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey w) => w != DLCRegistry.EDLCKey.None))
				{
					if (PlatformLayer.DLC.CanPlayDLC(item))
					{
						string text = SceneController.Instance.YML.DLCGlobalRulesetZip(item);
						if (PlatformLayer.FileSystem.ExistsFile(text))
						{
							ZipFile.Read(text).ExtractAll(Path.Combine(pathToExportTo, GloomUtility.GetEnumCategory(item), "Global"));
						}
						else
						{
							Debug.LogError("Failed to extract DLC Global YML");
							result = false;
						}
						string text2 = SceneController.Instance.YML.DLCSharedRulesetZip(item);
						if (PlatformLayer.FileSystem.ExistsFile(text2))
						{
							ZipFile.Read(text2).ExtractAll(Path.Combine(pathToExportTo, GloomUtility.GetEnumCategory(item), "Shared"));
						}
						else
						{
							Debug.LogError("Failed to extract DLC Shared YML");
							result = false;
						}
						if (rulesetType == GHRuleset.ERulesetType.Guildmaster || rulesetType == GHRuleset.ERulesetType.None)
						{
							string text3 = SceneController.Instance.YML.DLCGuildmasterRulesetZip(item);
							if (PlatformLayer.FileSystem.ExistsFile(text3))
							{
								ZipFile.Read(text3).ExtractAll(Path.Combine(pathToExportTo, GloomUtility.GetEnumCategory(item), "Guildmaster"));
							}
							else
							{
								Debug.LogError("Failed to extract DLC Guildmaster YML");
								result = false;
							}
						}
						if (rulesetType == GHRuleset.ERulesetType.Campaign || rulesetType == GHRuleset.ERulesetType.None)
						{
							string text4 = SceneController.Instance.YML.DLCCampaignRulesetZip(item);
							if (PlatformLayer.FileSystem.ExistsFile(text4))
							{
								ZipFile.Read(text4).ExtractAll(Path.Combine(pathToExportTo, GloomUtility.GetEnumCategory(item), "Campaign"));
							}
							else
							{
								Debug.LogError("Failed to extract DLC Campaign YML");
								result = false;
							}
						}
						string text5 = SceneController.Instance.YML.DLCLanguageCSVPackage(item);
						if (PlatformLayer.FileSystem.ExistsFile(text5))
						{
							PlatformLayer.FileSystem.CreateDirectory(Path.Combine(pathToExportTo, GloomUtility.GetEnumCategory(item), "LangUpdates"));
							PlatformLayer.FileSystem.CopyFile(text5, Path.Combine(pathToExportTo, GloomUtility.GetEnumCategory(item), "LangUpdates", "LangUpdate.csv"), overwrite: true);
						}
						else
						{
							Debug.LogError("Failed to extract DLC lang updates");
							result = false;
						}
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogError("An exception occurred in ExportYML.\nException: " + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		Debug.LogError("Path " + pathToExportTo + " does not exist.  Unable to export YML");
		return false;
	}

	public bool Validate(bool writeResultsToFile)
	{
		MetaData.AppliedFiles.Clear();
		ScenarioRuleClient.SRLYML.ResetModdedData();
		MapRuleLibraryClient.MRLYML.ResetModdedData();
		if (writeResultsToFile)
		{
			LastValidationResultsFile = RootSaveData.GetModValidationLogPath(MetaData.Name);
			SceneController.Instance.YML.WriteValidationToFile(LastValidationResultsFile, "Starting validation for mod: " + MetaData.Name);
		}
		else
		{
			LastValidationResultsFile = null;
		}
		bool flag;
		switch (MetaData.ModType)
		{
		case GHModMetaData.EModType.Guildmaster:
		case GHModMetaData.EModType.Campaign:
		case GHModMetaData.EModType.Global:
		{
			bool flag2 = false;
			if (ModdedYMLFiles.Length != 0)
			{
				ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.Global;
				MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.Global;
				flag2 = SceneController.Instance.YML.LoadMod(this, LastValidationResultsFile);
			}
			Tuple<bool, string[]> tuple2 = ValidateModCustomLevels(LastValidationResultsFile);
			Tuple<bool, string> tuple3 = ValidateModLanguageSheet(LastValidationResultsFile);
			flag = flag2 && tuple2.Item1 && tuple3.Item1;
			if (flag)
			{
				MetaData.AppliedFiles.AddRange(ModdedYMLFiles);
				if (tuple2.Item2.Length != 0)
				{
					MetaData.AppliedFiles.AddRange(tuple2.Item2);
				}
				if (tuple3.Item2 != null)
				{
					MetaData.AppliedFiles.Add(tuple3.Item2);
				}
			}
			else if (ModdedYMLFiles.Length == 0 && writeResultsToFile)
			{
				SceneController.Instance.YML.WriteValidationToFile(LastValidationResultsFile, "No YML files included in mod");
			}
			break;
		}
		case GHModMetaData.EModType.CustomLevels:
		{
			Tuple<bool, string[]> tuple4 = ValidateModCustomLevels(LastValidationResultsFile);
			flag = tuple4.Item1 && tuple4.Item2.Length != 0;
			if (flag)
			{
				MetaData.AppliedFiles.AddRange(tuple4.Item2);
			}
			else if (tuple4.Item2.Length == 0 && writeResultsToFile)
			{
				SceneController.Instance.YML.WriteValidationToFile(LastValidationResultsFile, "No Custom Level files included in Custom Level mod");
			}
			break;
		}
		case GHModMetaData.EModType.Language:
		{
			Tuple<bool, string[]> tuple = ValidateLanguagePacks(LastValidationResultsFile);
			flag = tuple.Item1 && tuple.Item2.Length != 0;
			if (flag)
			{
				MetaData.AppliedFiles.AddRange(tuple.Item2);
			}
			else if (tuple.Item2.Length == 0 && writeResultsToFile)
			{
				SceneController.Instance.YML.WriteValidationToFile(LastValidationResultsFile, "No csv files included in Language mod");
			}
			break;
		}
		default:
			Debug.LogError("Invalid ModType " + MetaData.ModType);
			flag = false;
			break;
		}
		IsValid = flag;
		return flag;
	}

	private Tuple<bool, string[]> ValidateModCustomLevels(string validationErrorFile)
	{
		bool item = true;
		List<string> list = new List<string>();
		string[] customLevelFiles = CustomLevelFiles;
		foreach (string text in customLevelFiles)
		{
			try
			{
				using (MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(text)))
				{
					new BinaryFormatter().Deserialize(serializationStream);
				}
				list.Add(text);
			}
			catch (Exception ex)
			{
				item = false;
				string text2 = "Unable to get Custom Level Data object from path " + text + ".";
				Debug.LogError(text2 + "\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.YML.WriteValidationToFile(validationErrorFile, text2);
			}
		}
		return new Tuple<bool, string[]>(item, list.ToArray());
	}

	private Tuple<bool, string> ValidateModLanguageSheet(string validationErrorFile)
	{
		if (PlatformLayer.FileSystem.ExistsFile(LangUpdateFilePath))
		{
			LanguageSourceData languageSourceData = GLOOM.LocalizationManager.GenerateLanguageSourceSource();
			string text = File.ReadAllText(LangUpdateFilePath);
			if (text != "Key,English [en-GB]\r\n")
			{
				string text2 = languageSourceData.Import_CSV(string.Empty, text, eSpreadsheetUpdateMode.Merge);
				if (text2 != string.Empty)
				{
					SceneController.Instance.YML.WriteValidationToFile(validationErrorFile, text2);
					return new Tuple<bool, string>(item1: false, null);
				}
				return new Tuple<bool, string>(item1: true, LangUpdateFilePath);
			}
		}
		return new Tuple<bool, string>(item1: true, null);
	}

	private Tuple<bool, string[]> ValidateLanguagePacks(string validationErrorFile)
	{
		bool item = true;
		List<string> list = new List<string>();
		string[] languageCSVs = LanguageCSVs;
		foreach (string text in languageCSVs)
		{
			string text2 = GLOOM.LocalizationManager.GenerateLanguageSourceSource().Import_CSV(string.Empty, PlatformLayer.FileSystem.FileReadAllText(text), eSpreadsheetUpdateMode.Merge);
			if (text2 != string.Empty)
			{
				SceneController.Instance.YML.WriteValidationToFile(validationErrorFile, text2);
				item = false;
			}
			else
			{
				list.Add(text);
			}
		}
		return new Tuple<bool, string[]>(item, list.ToArray());
	}
}
