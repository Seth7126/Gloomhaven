#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using FFSThreads;
using GLOOM;
using UnityEngine;

public class GHModding : IProgress<float>
{
	public const string RulesetMetaDataFileName = "ruleset.mod";

	public const string ModMetaDataFileName = "gloom.mod";

	public const string PreviewFileName = "preview.png";

	public const string LanguagePackCSV = "LanguagePack.csv";

	public const int MaxModSize = 20971520;

	public const string OldMetaDataFileName = "meta.dat";

	public const string LanguagePacksDirectory = "LangPacks";

	public const string LanguageUpdateDirectory = "LangUpdates";

	public const string ModdedYMLDirectory = "ModdedYML";

	public const string ModdedCustomLevels = "ModdedCustomLevels";

	public const string LevelEditorScenarios = "LevelEditorScenarios";

	public const string ScenarioYML = "ScenarioYML";

	public const string CustomResourcesDirectory = "ModdedCustomResources";

	public const string CustomImagesDirectory = "Images";

	public const string c_LanguageExportCSV = "LanguageExport.csv";

	public const string c_LanguageExportCSVConsoles = "LanguageExport_Consoles.csv";

	public const string c_MetaFilename = "meta.dat";

	public const int c_MaxModSize = 20971520;

	public const string c_ModPreviewFile = "preview.png";

	public List<GHRuleset> Rulesets { get; private set; }

	public bool RulesetsLoaded { get; private set; }

	public List<GHMod> Mods { get; private set; }

	public bool ModsLoaded { get; private set; }

	public GHRuleset LevelEditorRuleset { get; set; }

	public GHModding()
	{
		Rulesets = new List<GHRuleset>();
		RulesetsLoaded = false;
		Mods = new List<GHMod>();
		ModsLoaded = false;
	}

	public void LoadRulesets(ThreadMessageSender sender)
	{
		if (!PlatformLayer.FileSystem.ExistsDirectory(RootSaveData.ModdedRulesetsFolder))
		{
			Debug.Log("Creating steam rulesets folder: " + RootSaveData.ModdedRulesetsFolder);
			PlatformLayer.FileSystem.CreateDirectory(RootSaveData.ModdedRulesetsFolder);
		}
		else
		{
			float incrementAmount = 50f / (float)PlatformLayer.FileSystem.GetDirectories(RootSaveData.ModdedRulesetsFolder).Length;
			string[] directories = PlatformLayer.FileSystem.GetDirectories(RootSaveData.ModdedRulesetsFolder);
			foreach (string rulesetDir in directories)
			{
				GHRuleset gHRuleset = LoadRuleset(rulesetDir);
				if (gHRuleset != null && !gHRuleset.IsMPRuleset)
				{
					Rulesets.Add(gHRuleset);
				}
				sender.SendMessage(new ThreadMessage_IncrementProgressBar(incrementAmount));
			}
		}
		RulesetsLoaded = true;
	}

	public void Report(float value)
	{
		SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(value * 100f);
	}

	private GHRuleset LoadRuleset(string rulesetDir)
	{
		try
		{
			if (PlatformLayer.FileSystem.ExistsDirectory(rulesetDir))
			{
				string[] files = PlatformLayer.FileSystem.GetFiles(rulesetDir, "ruleset.mod");
				if (files.Length == 1)
				{
					using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(files[0]));
					return new BinaryFormatter().Deserialize(serializationStream) as GHRuleset;
				}
				Debug.LogError("Unable to locate Ruleset meta file at " + Path.Combine(rulesetDir, "ruleset.mod") + ".");
			}
			else
			{
				Debug.LogError("Directory " + rulesetDir + " does not exist!  Unable to load this ruleset.");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while running LoadRuleset.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return null;
	}

	public GHMod LoadMod(string modDirectory, string modName, float rating, bool isLocalMod)
	{
		try
		{
			string text = Path.Combine(modDirectory, "gloom.mod");
			if (PlatformLayer.FileSystem.ExistsFile(text))
			{
				using MemoryStream serializationStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(text));
				SaveData.Instance.CurrentFileBeingDeserialized = text;
				return new GHMod(new BinaryFormatter
				{
					Binder = new SerializationBinding()
				}.Deserialize(serializationStream) as GHModMetaData, modDirectory, rating, isLocalMod);
			}
			string text2 = Path.Combine(modDirectory, "meta.dat");
			string path = Path.Combine(modDirectory, "LangPacks");
			if (PlatformLayer.FileSystem.ExistsFile(text2) && PlatformLayer.FileSystem.ExistsDirectory(path) && PlatformLayer.FileSystem.GetFiles(path, "*.csv").Length != 0)
			{
				using (MemoryStream serializationStream2 = new MemoryStream(PlatformLayer.FileSystem.ReadFile(text2)))
				{
					SaveData.Instance.CurrentFileBeingDeserialized = text2;
					ModMetadata oldMetaData = new BinaryFormatter
					{
						Binder = new SerializationBinding()
					}.Deserialize(serializationStream2) as ModMetadata;
					Texture2D texture2D = new Texture2D(2, 2);
					string text3 = Path.Combine(modDirectory, "preview.png");
					try
					{
						byte[] data = PlatformLayer.FileSystem.ReadFile(text3);
						texture2D.LoadImage(data);
					}
					catch
					{
						Debug.LogError("Unable to load Mod thumbnail at path " + text3);
					}
					return new GHMod(new GHModMetaData(oldMetaData, modName, texture2D), modDirectory, rating, isLocalMod);
				}
			}
			Debug.LogError("Mod " + modName + " is no longer supported");
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load mod at path " + modDirectory + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return null;
	}

	public IEnumerator LoadLanguageMods()
	{
		foreach (GHMod item in Mods.Where((GHMod w) => w.MetaData.ModType == GHModMetaData.EModType.Language))
		{
			Debug.Log("[MODDING] Loading LanguageMod ModName: " + item.MetaData.Name);
			string[] languageCSVs = item.LanguageCSVs;
			for (int num = 0; num < languageCSVs.Length; num++)
			{
				LocalizationManager.AddCSVSource(languageCSVs[num]);
				yield return null;
			}
			yield return null;
		}
	}

	public static IEnumerator ExportModdableContent(Action onSuccess, Action onFailed)
	{
		bool exportSuccessful = false;
		Thread exportYML = new Thread((ThreadStart)delegate
		{
			exportSuccessful = ExportModdableContentInternal();
		});
		exportYML.Start();
		while (exportYML.IsAlive)
		{
			yield return null;
		}
		if (exportSuccessful)
		{
			onSuccess?.Invoke();
			yield break;
		}
		Debug.LogError("Failed to Export moddable content");
		onFailed?.Invoke();
	}

	private static bool ExportModdableContentInternal()
	{
		try
		{
			bool result = true;
			Debug.Log("Exporting moddable content internal");
			if (PlatformLayer.FileSystem.ExistsDirectory(RootSaveData.ModdingExportFolder))
			{
				PlatformLayer.FileSystem.RemoveDirectory(RootSaveData.ModdingExportFolder, recursive: true);
			}
			PlatformLayer.FileSystem.CreateDirectory(RootSaveData.ModdingExportFolder);
			if (GHMod.ExportLanguageCSV(RootSaveData.ModdingExportFolder, PlatformLayer.FileSystem) == null)
			{
				result = false;
			}
			if (!GHMod.ExportYML(RootSaveData.ModdingExportFolder, GHRuleset.ERulesetType.None))
			{
				result = false;
			}
			return result;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred in ExportModdableContent.\nException: " + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public static IEnumerator ValidateMod(GHMod mod, Action onValidatedCallback, bool writeResultsToFile)
	{
		Thread validateMod = new Thread((ThreadStart)delegate
		{
			mod.Validate(writeResultsToFile);
		});
		validateMod.Start();
		while (validateMod.IsAlive)
		{
			yield return null;
		}
		SceneController.Instance.YML.Unload(regenCards: true);
		yield return SceneController.Instance.YML.RegenCardsCoroutine();
		onValidatedCallback?.Invoke();
	}

	public static IEnumerator CompileRuleset(GHRuleset ruleset, Action onCompiledCallback)
	{
		Thread compileRuleset = new Thread((ThreadStart)delegate
		{
			ruleset.CompileRuleset();
		});
		compileRuleset.Start();
		while (compileRuleset.IsAlive)
		{
			yield return null;
		}
		onCompiledCallback?.Invoke();
	}
}
