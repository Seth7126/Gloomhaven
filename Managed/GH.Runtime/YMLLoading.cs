#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GLOOM;
using I2.Loc;
using Ionic.Zip;
using Ionic.Zlib;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Validation;
using SpriteMemoryManagement;
using UnityEngine;

public class YMLLoading
{
	public enum EYMLParserType
	{
		None,
		RemoveYML,
		HeroSummon,
		AttackModifier,
		AttackModifierDeck,
		Character,
		AbilityCard,
		ItemCard,
		Perk,
		Enhancements,
		IconGlossary,
		EnemyCard,
		Enemy,
		MonsterData,
		TreasureTable,
		Rooms,
		ScenarioAbility,
		Scenario,
		Difficulty,
		Achievement,
		RoadEvent,
		VisibilitySphere,
		Temple,
		Village,
		Quest,
		Message,
		Headquarters,
		CityEvent,
		InitialEvents,
		BattleGoal,
		PersonalQuest,
		StoreLocation,
		ItemConfig,
		AbilityCardConfig,
		CharacterConfig,
		MonsterConfig,
		MapConfig,
		CharacterResources
	}

	public static EYMLParserType[] EYMLParserTypes = (EYMLParserType[])Enum.GetValues(typeof(EYMLParserType));

	public const string RulesetExtension = ".ruleset";

	private ZipFile m_GlobalZip;

	private ZipFile m_CampaignZip;

	private ZipFile m_GuildmasterZip;

	private ZipFile m_SharedZip;

	private ZipFile m_SingleScenarios;

	private ZipFile m_ModdedZip;

	private Dictionary<DLCRegistry.EDLCKey, ZipFile> m_DLCGlobalZips = new Dictionary<DLCRegistry.EDLCKey, ZipFile>();

	private Dictionary<DLCRegistry.EDLCKey, ZipFile> m_DLCCampaignZips = new Dictionary<DLCRegistry.EDLCKey, ZipFile>();

	private Dictionary<DLCRegistry.EDLCKey, ZipFile> m_DLCGuildmasterZips = new Dictionary<DLCRegistry.EDLCKey, ZipFile>();

	private Dictionary<DLCRegistry.EDLCKey, ZipFile> m_DLCSharedZips = new Dictionary<DLCRegistry.EDLCKey, ZipFile>();

	private Dictionary<DLCRegistry.EDLCKey, ZipFile> m_DLCSingleScenarioZips = new Dictionary<DLCRegistry.EDLCKey, ZipFile>();

	private List<AbilityCardYMLData> m_ModdedAbilityCards;

	private List<ItemCardYMLData> m_ModdedItemCards;

	private const string AudioItemPrefix = "PlaySound_ScenarioUIEquipmentToggle_";

	private const string Default = "Default";

	public List<ItemConfigUI> ModdedItemConfigs;

	public List<AbilityCardUISkin> ModdedAbilityCardConfigs;

	public List<CharacterConfigUI> ModdedCharacterConfigs;

	public List<Sprite> ModdedMonsterPortraits;

	public List<Sprite> ModdedAvatars;

	public Dictionary<MapChoreographer.ECustomMapAlignment, Texture2D> ModdedMapImages;

	private static Action m_ReturnToMainMenuCompleteCallback;

	public static bool LastLoadResult { get; private set; }

	public static bool InitResult { get; private set; }

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

	public string RulebaseFolder
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(_dataPath, "..", "Rulebase"));
			}
			return Path.Combine(_dataPath, "Rulebase");
		}
	}

	public string RulebaseZipFolder
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(_dataPath, "StreamingAssets", "Rulebase"));
			}
			return Path.Combine(_dataPath, "Rulebase");
		}
	}

	public string GlobalRuleset => Path.Combine(RulebaseFolder, "Global");

	public string GlobalRulesetZip => Path.Combine(RulebaseZipFolder, "Global.ruleset");

	public string CampaignRuleset => Path.Combine(RulebaseFolder, "Campaign");

	public string CampaignRulesetZip => Path.Combine(RulebaseZipFolder, "Campaign.ruleset");

	public string GuildmasterRuleset => Path.Combine(RulebaseFolder, "Guildmaster");

	public string GuildmasterRulesetZip => Path.Combine(RulebaseZipFolder, "Guildmaster.ruleset");

	public string SharedRuleset => Path.Combine(RulebaseFolder, "Shared");

	public string SharedRulesetZip => Path.Combine(RulebaseZipFolder, "Shared.ruleset");

	public string ModdedRulesets => Path.Combine(RulebaseFolder, "Mods");

	public string CustomScenariosRuleset => Path.Combine(RulebaseFolder, "CustomScenarios");

	public string CustomScenariosRulesetZip => Path.Combine(RulebaseZipFolder, "CustomScenarios.ruleset");

	public string GuildmasterAutoCompleteFileName => "GuildmasterAutoComplete.yml";

	public string GuildmasterAutoCompletePath
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(_dataPath, "StreamingAssets", GuildmasterAutoCompleteFileName));
			}
			return Path.Combine(_dataPath, GuildmasterAutoCompleteFileName);
		}
	}

	public string CampaignAutoCompleteFileName => "CampaignAutoComplete.yml";

	public string CampaignAutoCompletePath
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(_dataPath, "StreamingAssets", CampaignAutoCompleteFileName));
			}
			return Path.Combine(_dataPath, CampaignAutoCompleteFileName);
		}
	}

	public bool IsUnloading { get; private set; }

	public static LanguageSourceData ModdedLangSource { get; private set; }

	public string[] ModdedRulesetPaths { get; private set; }

	public string DLCGlobalRuleset(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlcEnum), "Global");
	}

	public string DLCGlobalRulesetZip(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RootSaveData.DLCPackageFolder(dlcEnum), GloomUtility.GetEnumCategory(dlcEnum) + "_Global.ruleset");
	}

	public string DLCCampaignRuleset(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlcEnum), "Campaign");
	}

	public string DLCCampaignRulesetZip(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RootSaveData.DLCPackageFolder(dlcEnum), GloomUtility.GetEnumCategory(dlcEnum) + "_Campaign.ruleset");
	}

	public string DLCGuildmasterRuleset(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlcEnum), "Guildmaster");
	}

	public string DLCGuildmasterRulesetZip(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RootSaveData.DLCPackageFolder(dlcEnum), GloomUtility.GetEnumCategory(dlcEnum) + "_Guildmaster.ruleset");
	}

	public string DLCSharedRuleset(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlcEnum), "Shared");
	}

	public string DLCSharedRulesetZip(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RootSaveData.DLCPackageFolder(dlcEnum), GloomUtility.GetEnumCategory(dlcEnum) + "_Shared.ruleset");
	}

	public string DLCCustomScenariosRuleset(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlcEnum), "CustomScenarios");
	}

	public string DLCCustomScenariosRulesetZip(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RootSaveData.DLCPackageFolder(dlcEnum), GloomUtility.GetEnumCategory(dlcEnum) + "_CustomScenarios.ruleset");
	}

	public string DLCLanguageCSV(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlcEnum), "LangUpdates", "LangUpdate.csv");
	}

	public string DLCLanguageCSVPackage(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(RootSaveData.DLCPackageFolder(dlcEnum), "LangUpdate.csv");
	}

	public IEnumerator GenerateModdedAbilityCards(List<string> moddedCards)
	{
		LastLoadResult = false;
		bool loadedSuccessfully = true;
		if (m_ModdedAbilityCards == null)
		{
			m_ModdedAbilityCards = new List<AbilityCardYMLData>();
		}
		if (moddedCards != null && moddedCards.Count > 0)
		{
			foreach (AbilityCardYMLData abilityCardYML in ScenarioRuleClient.SRLYML.AbilityCards.Where((AbilityCardYMLData w) => moddedCards.Contains(Path.GetFileName(w.FileName))))
			{
				bool cardCreated = false;
				GameObject newAbilityCard = null;
				AbilityCardUI cardUI = null;
				try
				{
					ObjectPool.RemoveAbilityCard(abilityCardYML.ID);
					cardCreated = PersistentData.CreateAbilityCard1(abilityCardYML, out newAbilityCard, out cardUI);
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception during GenerateModdedAbilityCards.\n" + ex.Message + "\n" + ex.StackTrace);
					loadedSuccessfully = false;
				}
				yield return null;
				if (cardCreated)
				{
					try
					{
						PersistentData.CreateAbilityCard2(abilityCardYML, newAbilityCard, cardUI);
						m_ModdedAbilityCards.Add(abilityCardYML);
					}
					catch (Exception ex2)
					{
						Debug.LogError("Exception during GenerateModdedAbilityCards.\n" + ex2.Message + "\n" + ex2.StackTrace);
						loadedSuccessfully = false;
					}
					yield return null;
				}
				else
				{
					loadedSuccessfully = false;
				}
			}
		}
		LastLoadResult = loadedSuccessfully;
	}

	public IEnumerator GenerateModdedItemCards(List<string> moddedCards)
	{
		LastLoadResult = false;
		bool loadedSuccessfully = true;
		if (m_ModdedItemCards == null)
		{
			m_ModdedItemCards = new List<ItemCardYMLData>();
		}
		if (moddedCards != null && moddedCards.Count > 0)
		{
			foreach (ItemCardYMLData itemCardYML in ScenarioRuleClient.SRLYML.ItemCards.Where((ItemCardYMLData w) => moddedCards.Contains(Path.GetFileName(w.FileName))))
			{
				bool cardCreated = false;
				GameObject newItemCard = null;
				ItemCardUI cardUI = null;
				try
				{
					ObjectPool.RemoveItemCard(itemCardYML.ID);
					cardCreated = PersistentData.CreateItemCard1(itemCardYML, out newItemCard, out cardUI);
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception during GenerateModdedItemCards.\n" + ex.Message + "\n" + ex.StackTrace);
					loadedSuccessfully = false;
				}
				yield return null;
				if (cardCreated)
				{
					try
					{
						PersistentData.CreateItemCard2(itemCardYML, newItemCard, cardUI);
						m_ModdedItemCards.Add(itemCardYML);
					}
					catch (Exception ex2)
					{
						Debug.LogError("Exception during GenerateModdedItemCards.\n" + ex2.Message + "\n" + ex2.StackTrace);
						loadedSuccessfully = false;
					}
					yield return null;
				}
				else
				{
					loadedSuccessfully = false;
				}
			}
		}
		LastLoadResult = loadedSuccessfully;
	}

	public bool CompileRuleset(string rulesetPath, string zipPath, List<string> includeList = null)
	{
		bool flag = true;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(zipPath);
		string text = Path.Combine(RulebaseZipFolder, "GuildmasterRaw");
		string text2 = Path.Combine(RulebaseZipFolder, "CampaignRaw");
		string text3 = Path.Combine(RulebaseZipFolder, "CommonRaw");
		List<string> obj = new List<string> { text, text2, text3 };
		Directory.CreateDirectory(text);
		Directory.CreateDirectory(text2);
		Directory.CreateDirectory(text3);
		foreach (string item in obj)
		{
			Directory.CreateDirectory(Path.Combine(item, EYMLParserType.Enemy.ToString()));
			Directory.CreateDirectory(Path.Combine(item, EYMLParserType.Scenario.ToString()));
		}
		try
		{
			using ZipFile zipFile = new ZipFile();
			if (File.Exists(zipPath))
			{
				File.Delete(zipPath);
			}
			if (includeList != null && !includeList.Contains(Path.GetFileName(zipPath)))
			{
				Debug.LogWarning("Not included " + zipPath);
				return true;
			}
			if (Directory.Exists(rulesetPath))
			{
				string[] files = Directory.GetFiles(rulesetPath, "*.yml", SearchOption.AllDirectories);
				foreach (string file in files)
				{
					string parserLine;
					using (StreamReader streamReader = new StreamReader(file))
					{
						parserLine = streamReader.ReadLine();
					}
					EYMLParserType parserType = GetParserType(parserLine, file);
					if (parserType == EYMLParserType.Enemy || parserType == EYMLParserType.Scenario)
					{
						string destFileName = Path.Combine(text3, parserType.ToString(), Path.GetFileName(file));
						if (fileNameWithoutExtension.Contains("Campaign"))
						{
							destFileName = Path.Combine(text2, parserType.ToString(), Path.GetFileName(file));
						}
						else if (fileNameWithoutExtension.Contains("Guildmaster"))
						{
							destFileName = Path.Combine(text, parserType.ToString(), Path.GetFileName(file));
						}
						File.Copy(file, destFileName, overwrite: true);
					}
					if (parserType != EYMLParserType.None)
					{
						if (includeList == null || includeList.Any((string a) => file.Replace("\\", "/").Contains(a)))
						{
							string safeFileName = GetSafeFileName(zipFile, file, parserType.ToString());
							if (file != safeFileName)
							{
								File.Move(file, safeFileName);
							}
							zipFile.AddFile(safeFileName, parserType.ToString());
						}
						else
						{
							Debug.LogWarning("Not included file: " + file);
						}
					}
					else
					{
						Debug.LogError("Failed to compile ruleset at: " + rulesetPath + "\nThe Parser type for file " + file + " is not valid");
						flag = false;
					}
				}
				files = Directory.GetFiles(rulesetPath, "*.lvldat", SearchOption.AllDirectories);
				foreach (string file2 in files)
				{
					if (includeList == null || includeList.Any((string a) => file2.Replace("\\", "/").Contains(a)))
					{
						string safeFileName2 = GetSafeFileName(zipFile, file2, "CustomLevels");
						if (file2 != safeFileName2)
						{
							File.Move(file2, safeFileName2);
						}
						zipFile.AddFile(safeFileName2, "CustomLevels");
					}
					else
					{
						Debug.LogWarning("Not included file: " + file2);
					}
				}
			}
			else
			{
				Debug.LogWarning("Directory " + rulesetPath + " is not exists.");
			}
			if (flag)
			{
				zipFile.CompressionLevel = CompressionLevel.BestSpeed;
				if (!Directory.Exists(Path.GetDirectoryName(zipPath)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
				}
				zipFile.Save(zipPath);
				Debug.Log($"Zip completed: {zipPath} {File.Exists(zipPath)}");
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while compiling a ruleset.\n" + ex.Message + "\n" + ex.StackTrace);
			flag = false;
		}
		return flag;
	}

	public bool CopyLangUpdatesToDLCPackage(string csvPath, string outputPath)
	{
		bool result = true;
		try
		{
			if (File.Exists(outputPath))
			{
				File.Delete(outputPath);
			}
			if (!File.Exists(csvPath))
			{
				result = false;
			}
			else
			{
				File.Copy(csvPath, outputPath);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while copy a LangUpdate csv.\n" + ex.Message + "\n" + ex.StackTrace);
			result = false;
		}
		return result;
	}

	public void Init(bool usingCardViewerTool = false)
	{
		try
		{
			m_GlobalZip = ZipFile.Read(GlobalRulesetZip);
			m_CampaignZip = ZipFile.Read(CampaignRulesetZip);
			m_GuildmasterZip = ZipFile.Read(GuildmasterRulesetZip);
			m_SharedZip = ZipFile.Read(SharedRulesetZip);
			m_SingleScenarios = ZipFile.Read(CustomScenariosRulesetZip);
			bool flag = SaveData.Instance.Global == null || SaveData.Instance.Global.IsJotlDlcSaveExists();
			foreach (DLCRegistry.EDLCKey item in DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey w) => w != DLCRegistry.EDLCKey.None && w != DLCRegistry.EDLCKey.DLC3))
			{
				if (usingCardViewerTool || PlatformLayer.DLC.UserInstalledDLC(item) || flag)
				{
					Debug.Log("Loading DLC YML rulesets for " + item);
					string text = DLCGlobalRulesetZip(item);
					if (File.Exists(text))
					{
						m_DLCGlobalZips[item] = ZipFile.Read(text);
					}
					else
					{
						Debug.LogError("Failed to load DLC ruleset: " + text);
					}
					string text2 = DLCCampaignRulesetZip(item);
					if (File.Exists(text2))
					{
						m_DLCCampaignZips[item] = ZipFile.Read(text2);
					}
					else
					{
						Debug.LogError("Failed to load DLC ruleset: " + text2);
					}
					string text3 = DLCGuildmasterRulesetZip(item);
					if (File.Exists(text3))
					{
						m_DLCGuildmasterZips[item] = ZipFile.Read(text3);
					}
					else
					{
						Debug.LogError("Failed to load DLC ruleset: " + text3);
					}
					string text4 = DLCSharedRulesetZip(item);
					if (File.Exists(text4))
					{
						m_DLCSharedZips[item] = ZipFile.Read(text4);
					}
					else
					{
						Debug.LogError("Failed to load DLC ruleset: " + text4);
					}
					string text5 = DLCCustomScenariosRulesetZip(item);
					if (File.Exists(text5))
					{
						m_DLCSingleScenarioZips[item] = ZipFile.Read(text5);
					}
					else
					{
						Debug.LogError("Failed to load DLC ruleset: " + text5);
					}
				}
			}
			if (Directory.Exists(ModdedRulesets))
			{
				ModdedRulesetPaths = Directory.GetFiles(ModdedRulesets, "*.ruleset", SearchOption.AllDirectories);
			}
			InitResult = true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to initialise YML\n" + ex.Message + "\n" + ex.StackTrace);
			InitResult = false;
		}
	}

	public void Close()
	{
		if (m_GlobalZip != null)
		{
			m_GlobalZip.Dispose();
			m_GlobalZip = null;
		}
		if (m_SharedZip != null)
		{
			m_SharedZip.Dispose();
			m_SharedZip = null;
		}
		if (m_CampaignZip != null)
		{
			m_CampaignZip.Dispose();
			m_CampaignZip = null;
		}
		if (m_GuildmasterZip != null)
		{
			m_GuildmasterZip.Dispose();
			m_GuildmasterZip = null;
		}
		if (m_SingleScenarios != null)
		{
			m_SingleScenarios.Dispose();
			m_SingleScenarios = null;
		}
		if (m_ModdedZip != null)
		{
			m_ModdedZip.Dispose();
			m_ModdedZip = null;
		}
		foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCGlobalZip in m_DLCGlobalZips)
		{
			dLCGlobalZip.Value.Dispose();
		}
		m_DLCGlobalZips.Clear();
		foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCCampaignZip in m_DLCCampaignZips)
		{
			dLCCampaignZip.Value.Dispose();
		}
		m_DLCCampaignZips.Clear();
		foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCGuildmasterZip in m_DLCGuildmasterZips)
		{
			dLCGuildmasterZip.Value.Dispose();
		}
		m_DLCGuildmasterZips.Clear();
		foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCSharedZip in m_DLCSharedZips)
		{
			dLCSharedZip.Value.Dispose();
		}
		m_DLCSharedZips.Clear();
		foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCSingleScenarioZip in m_DLCSingleScenarioZips)
		{
			dLCSingleScenarioZip.Value.Dispose();
		}
		m_DLCSingleScenarioZips.Clear();
	}

	public bool LoadGlobal()
	{
		ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.Global;
		MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.Global;
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.None;
		LastLoadResult = ProcessZipFile(m_GlobalZip, CSRLYML.EYMLMode.Global);
		if (!LastLoadResult)
		{
			Debug.LogError("Failed to load Global YML data");
		}
		else if (m_DLCGlobalZips.Count > 0)
		{
			foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCGlobalZip in m_DLCGlobalZips)
			{
				Debug.Log("Loading YML data for DLC " + dLCGlobalZip.Key);
				if (dLCGlobalZip.Value != null)
				{
					LastLoadResult = ProcessZipFile(dLCGlobalZip.Value, CSRLYML.EYMLMode.Global);
					if (!LastLoadResult)
					{
						Debug.LogError("Failed to load YML data for DLC " + dLCGlobalZip.Key);
					}
				}
			}
		}
		return LastLoadResult;
	}

	public bool LoadSingleScenarios(ScenarioManager.EDLLMode mode, string modPath = null)
	{
		LastLoadResult = false;
		if (SceneController.Instance.Modding?.LevelEditorRuleset != null)
		{
			mode = ScenarioManager.EDLLMode.Mod;
		}
		bool flag;
		switch (mode)
		{
		case ScenarioManager.EDLLMode.Campaign:
			if (LoadCampaign(PlatformLayer.DLC.OwnedDLCAsFlag()))
			{
				flag = ProcessZipFile(m_SingleScenarios, CSRLYML.EYMLMode.StandardRuleset);
				break;
			}
			Debug.LogError("Failed to load Campaign Data");
			flag = false;
			break;
		case ScenarioManager.EDLLMode.Guildmaster:
			if (LoadGuildMaster(PlatformLayer.DLC.OwnedDLCAsFlag()))
			{
				flag = ProcessZipFile(m_SingleScenarios, CSRLYML.EYMLMode.StandardRuleset);
				break;
			}
			Debug.LogError("Failed to load Guildmaster Data");
			flag = false;
			break;
		case ScenarioManager.EDLLMode.Mod:
			flag = true;
			break;
		default:
			Debug.LogError("Invalid mode " + mode.ToString() + " sent to load single scenarios");
			flag = false;
			break;
		}
		if (m_DLCSingleScenarioZips.Count > 0)
		{
			foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCSingleScenarioZip in m_DLCSingleScenarioZips)
			{
				if (dLCSingleScenarioZip.Value != null)
				{
					flag = ProcessZipFile(dLCSingleScenarioZip.Value, CSRLYML.EYMLMode.StandardRuleset);
				}
			}
		}
		if (flag)
		{
			try
			{
				ScenarioRuleClient.LoadData();
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception when loading ScenarioRuleClient data.\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		LastLoadResult = flag;
		return flag;
	}

	public bool LoadGuildMaster(DLCRegistry.EDLCKey dlcToLoad)
	{
		LastLoadResult = false;
		if (ScenarioRuleClient.SRLYML.YMLMode != CSRLYML.EYMLMode.Global || MapRuleLibraryClient.MRLYML.YMLMode != CSRLYML.EYMLMode.Global)
		{
			Unload(regenCards: true);
		}
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Guildmaster;
		ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.StandardRuleset;
		MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.StandardRuleset;
		bool flag = ProcessZipFile(m_SharedZip, CSRLYML.EYMLMode.StandardRuleset);
		if (flag && m_DLCSharedZips.Count > 0)
		{
			foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCSharedZip in m_DLCSharedZips)
			{
				if (dLCSharedZip.Value != null && dlcToLoad.HasFlag(dLCSharedZip.Key))
				{
					flag = ProcessZipFile(dLCSharedZip.Value, CSRLYML.EYMLMode.StandardRuleset);
				}
			}
		}
		if (flag)
		{
			flag = ProcessZipFile(m_GuildmasterZip, CSRLYML.EYMLMode.StandardRuleset);
			if (flag && m_DLCGuildmasterZips.Count > 0)
			{
				foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCGuildmasterZip in m_DLCGuildmasterZips)
				{
					if (dLCGuildmasterZip.Value != null && dlcToLoad.HasFlag(dLCGuildmasterZip.Key))
					{
						flag = ProcessZipFile(dLCGuildmasterZip.Value, CSRLYML.EYMLMode.StandardRuleset);
					}
				}
			}
			if (flag)
			{
				if (!ScenarioRuleClient.SRLYML.Validate())
				{
					flag = false;
				}
				if (!MapRuleLibraryClient.MRLYML.Validate())
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			try
			{
				ScenarioRuleClient.LoadData();
				MapRuleLibraryClient.Instance.Reset();
				MapRuleLibraryClient.Instance.Start();
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception when loading ScenarioRuleClient data.\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		LastLoadResult = flag;
		return flag;
	}

	public bool LoadCampaign(DLCRegistry.EDLCKey dlcToLoad)
	{
		LastLoadResult = false;
		if (ScenarioRuleClient.SRLYML.YMLMode != CSRLYML.EYMLMode.Global || MapRuleLibraryClient.MRLYML.YMLMode != CSRLYML.EYMLMode.Global)
		{
			Unload(regenCards: true);
		}
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.StandardRuleset;
		MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.StandardRuleset;
		bool flag = ProcessZipFile(m_SharedZip, CSRLYML.EYMLMode.StandardRuleset);
		if (flag && m_DLCSharedZips.Count > 0)
		{
			foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCSharedZip in m_DLCSharedZips)
			{
				if (dLCSharedZip.Value != null && dlcToLoad.HasFlag(dLCSharedZip.Key))
				{
					flag = ProcessZipFile(dLCSharedZip.Value, CSRLYML.EYMLMode.StandardRuleset);
				}
			}
		}
		if (flag)
		{
			flag = ProcessZipFile(m_CampaignZip, CSRLYML.EYMLMode.StandardRuleset);
			if (flag && m_DLCCampaignZips.Count > 0)
			{
				foreach (KeyValuePair<DLCRegistry.EDLCKey, ZipFile> dLCCampaignZip in m_DLCCampaignZips)
				{
					if (dLCCampaignZip.Value != null && dlcToLoad.HasFlag(dLCCampaignZip.Key))
					{
						flag = ProcessZipFile(dLCCampaignZip.Value, CSRLYML.EYMLMode.StandardRuleset);
					}
				}
			}
			if (flag)
			{
				if (!ScenarioRuleClient.SRLYML.Validate())
				{
					flag = false;
				}
				if (!MapRuleLibraryClient.MRLYML.Validate())
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			try
			{
				ScenarioRuleClient.LoadData();
				MapRuleLibraryClient.Instance.Reset();
				MapRuleLibraryClient.Instance.Start();
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception when loading ScenarioRuleClient data.\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		LastLoadResult = flag;
		return flag;
	}

	public bool LoadMod(GHMod mod, string writeErrorsToFile)
	{
		LastLoadResult = false;
		try
		{
			if (ScenarioRuleClient.SRLYML.YMLMode != CSRLYML.EYMLMode.Global || MapRuleLibraryClient.MRLYML.YMLMode != CSRLYML.EYMLMode.Global)
			{
				Unload(regenCards: true);
			}
			ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.ModdedRuleset;
			MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.ModdedRuleset;
			bool flag = ProcessZipFile(m_GlobalZip, CSRLYML.EYMLMode.ModdedRuleset);
			if (flag)
			{
				if (mod.MetaData.ModType == GHModMetaData.EModType.Campaign)
				{
					MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
					flag = ProcessZipFile(m_SharedZip, CSRLYML.EYMLMode.ModdedRuleset);
					if (flag)
					{
						flag = ProcessZipFile(m_CampaignZip, CSRLYML.EYMLMode.ModdedRuleset);
					}
				}
				else if (mod.MetaData.ModType == GHModMetaData.EModType.Guildmaster)
				{
					MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Guildmaster;
					flag = ProcessZipFile(m_SharedZip, CSRLYML.EYMLMode.ModdedRuleset);
					if (flag)
					{
						flag = ProcessZipFile(m_GuildmasterZip, CSRLYML.EYMLMode.ModdedRuleset);
					}
				}
				else if (mod.MetaData.ModType != GHModMetaData.EModType.Global)
				{
					flag = false;
				}
			}
			if (flag)
			{
				flag = ProcessMod(mod, CSRLYML.EYMLMode.ModdedRuleset, writeErrorsToFile);
			}
			LastLoadResult = flag;
			return flag;
		}
		catch (Exception ex)
		{
			string text = "Exception running LoadMod.\n" + ex.Message + "\n" + ex.StackTrace;
			Debug.LogError(text);
			if (writeErrorsToFile != null)
			{
				WriteValidationToFile(writeErrorsToFile, text);
			}
			LastLoadResult = false;
			return false;
		}
	}

	private bool LoadModdedRuleset(GHRuleset ruleset)
	{
		Debug.Log("[YML] Attempting to load modded ruleset " + ruleset.Name + " containing mods: " + ruleset.LinkedModNames.ToStringPretty());
		LastLoadResult = false;
		if (ScenarioRuleClient.SRLYML.YMLMode != CSRLYML.EYMLMode.Global || MapRuleLibraryClient.MRLYML.YMLMode != CSRLYML.EYMLMode.Global)
		{
			Unload(regenCards: true);
		}
		if (ruleset.RulesetType == GHRuleset.ERulesetType.Campaign)
		{
			MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		}
		else if (ruleset.RulesetType == GHRuleset.ERulesetType.Guildmaster)
		{
			MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Guildmaster;
		}
		ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.ModdedRuleset;
		MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.ModdedRuleset;
		m_ModdedZip = ZipFile.Read(ruleset.RulesetCompiledZip);
		bool flag = ProcessZipFile(m_ModdedZip, CSRLYML.EYMLMode.ModdedRuleset);
		if (flag)
		{
			if (!ScenarioRuleClient.SRLYML.Validate())
			{
				flag = false;
			}
			if (!MapRuleLibraryClient.MRLYML.Validate())
			{
				flag = false;
			}
		}
		if (flag)
		{
			Debug.Log("[YML] Successfully loaded modded ruleset " + ruleset.Name + " containing mods: " + ruleset.LinkedModNames.ToStringPretty());
			try
			{
				ScenarioRuleClient.LoadData();
				MapRuleLibraryClient.Instance.Reset();
				MapRuleLibraryClient.Instance.Start();
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception when loading ScenarioRuleClient data.\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		LastLoadResult = flag;
		return flag;
	}

	public IEnumerator LoadRulesetZip(GHRuleset ruleset)
	{
		yield return null;
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			SceneController.Instance.ShowLoadingScreen();
			yield return null;
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				LoadModdedRuleset(ruleset);
			});
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			if (!LastLoadResult)
			{
				Debug.LogError("Unable to load Modded Ruleset '" + ruleset.Name + "' YML");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
				yield break;
			}
			try
			{
				string[] array = ((ruleset.LinkedCSV != null) ? ruleset.LinkedCSV.ToArray() : (from s in ruleset.LinkedMods
					where s.MetaData.AppliedFiles.Contains(s.LangUpdateFilePath)
					select s.LangUpdateFilePath).ToArray());
				if (array != null && array.Length != 0)
				{
					LanguageSourceData languageSourceData = GLOOM.LocalizationManager.GenerateLanguageSourceSource();
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (languageSourceData.Import_CSV(string.Empty, File.ReadAllText(text), eSpreadsheetUpdateMode.Merge) != string.Empty)
						{
							throw new Exception("Error loading modded CSV file " + text);
						}
					}
					ModdedLangSource = languageSourceData;
					I2.Loc.LocalizationManager.Sources.Insert(0, languageSourceData);
					I2.Loc.LocalizationManager.LocalizeAll(Force: true);
				}
			}
			catch (Exception ex)
			{
				LastLoadResult = false;
				Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
				yield break;
			}
			ModdedAvatars = new List<Sprite>();
			GenerateModdedItemConfigs(ruleset);
			GenerateModdedAbilityCardConfigs(ruleset);
			GenerateModdedCharacterConfigs(ruleset);
			GenerateModdedMonsterConfigs(ruleset);
			GenerateModdedMapImages(ruleset);
			yield return SceneController.Instance.YML.GenerateModdedAbilityCards(ruleset.CompiledAbilityCards);
			yield return SceneController.Instance.YML.GenerateModdedItemCards(ruleset.CompiledItemCards);
			if (!LastLoadResult)
			{
				LastLoadResult = false;
				Debug.LogError("Unable to load Modded Ruleset '" + ruleset.Name + "' AbilityCards");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
			}
		}
		else
		{
			LastLoadResult = false;
			Debug.LogError("Unable to load Modded Ruleset '" + ruleset.Name + "' YML.  Invalid YML Mode: " + ScenarioRuleClient.SRLYML.YMLMode);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
		}
	}

	private void GenerateModdedItemConfigs(GHRuleset ruleset)
	{
		ModdedItemConfigs = new List<ItemConfigUI>();
		try
		{
			foreach (ItemConfigYMLData itemConfig in ScenarioRuleClient.SRLYML.ItemConfigs)
			{
				ItemConfigUI itemConfigUI = (ItemConfigUI)ScriptableObject.CreateInstance("ItemConfigUI");
				GHMod[] linkedMods = ruleset.LinkedMods;
				foreach (GHMod gHMod in linkedMods)
				{
					if (!itemConfigUI.BackgroundImage.IsHaveSprite())
					{
						itemConfigUI.BackgroundImage.SetSpriteInsteadAddressable(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, itemConfig.Background)));
					}
					itemConfigUI.miniIcon = LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, itemConfig.Icon), itemConfigUI.miniIcon);
				}
				itemConfigUI.itemName = itemConfig.ItemName;
				itemConfigUI.toggleAudioItem = "PlaySound_ScenarioUIEquipmentToggle_" + itemConfig.Audio;
				itemConfigUI.previewEffect = new PreviewEffectInfo();
				ModdedItemConfigs.Add(itemConfigUI);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	private void GenerateModdedAbilityCardConfigs(GHRuleset ruleset)
	{
		ModdedAbilityCardConfigs = new List<AbilityCardUISkin>();
		UIInfoTools instance = UIInfoTools.Instance;
		try
		{
			foreach (AbilityCardConfigYMLData abilityCardConfig in ScenarioRuleClient.SRLYML.AbilityCardConfigs)
			{
				AbilityCardUISkin abilityCardUISkin = new AbilityCardUISkin();
				GHMod[] linkedMods = ruleset.LinkedMods;
				foreach (GHMod gHMod in linkedMods)
				{
					abilityCardUISkin.TitleSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.TitleSprite) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.TitleSprite).cardSkin.TitleSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.TitleSprite))));
					abilityCardUISkin.TopActionRegularSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionRegular) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionRegular).cardSkin.TopActionRegularSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.TopActionRegular))));
					abilityCardUISkin.TopActionHighlightSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionHighlight).cardSkin.TopActionHighlightSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.TopActionHighlight))));
					abilityCardUISkin.TopActionSelectedSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionSelected) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionSelected).cardSkin.TopActionSelectedSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.TopActionSelected))));
					abilityCardUISkin.TopActionDisabledSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionDisabled) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.TopActionDisabled).cardSkin.TopActionDisabledSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.TopActionDisabled))));
					abilityCardUISkin.BottomActionRegularSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionRegular) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionRegular).cardSkin.BottomActionRegularSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.BottomActionRegular))));
					abilityCardUISkin.BottomActionSelectedSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionSelected) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionSelected).cardSkin.BottomActionSelectedSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.BottomActionSelected))));
					abilityCardUISkin.BottomActionDisabledSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionDisabled) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionDisabled).cardSkin.BottomActionDisabledSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.BottomActionDisabled))));
					abilityCardUISkin.BottomActionHighlightSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.BottomActionHighlight).cardSkin.BottomActionHighlightSprite : new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.BottomActionHighlight))));
					abilityCardUISkin.defaultBottomActionDisabledSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultBottomActionDisabled) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultBottomActionDisabled).cardSkin.defaultBottomActionDisabledSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.DefaultBottomActionDisabled), abilityCardUISkin.defaultBottomActionDisabledSprite));
					abilityCardUISkin.defaultBottomActionHighlightSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultBottomActionHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultBottomActionHighlight).cardSkin.defaultBottomActionHighlightSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.DefaultBottomActionHighlight), abilityCardUISkin.defaultBottomActionHighlightSprite));
					abilityCardUISkin.defaultBottomActionRegularSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultBottomActionRegular) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultBottomActionRegular).cardSkin.defaultBottomActionRegularSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.DefaultBottomActionRegular), abilityCardUISkin.defaultBottomActionRegularSprite));
					abilityCardUISkin.defaultTopActionRegularSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultTopActionRegular) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultTopActionRegular).cardSkin.defaultTopActionRegularSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.DefaultTopActionRegular), abilityCardUISkin.defaultTopActionRegularSprite));
					abilityCardUISkin.defaultTopActionHighlightSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultTopActionHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultTopActionHighlight).cardSkin.defaultTopActionHighlightSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.DefaultTopActionHighlight), abilityCardUISkin.defaultTopActionHighlightSprite));
					abilityCardUISkin.defaultTopActionDisabledSprite = (instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultTopActionDisabled) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultTopActionDisabled).cardSkin.defaultTopActionDisabledSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.DefaultTopActionDisabled), abilityCardUISkin.defaultTopActionDisabledSprite));
					abilityCardUISkin.regularPreviewBackground = (instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewRegular) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewRegular).cardSkin.regularPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewRegular), abilityCardUISkin.regularPreviewBackground));
					abilityCardUISkin.regularHighlightedPreviewBackground = (instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewHighlight).cardSkin.regularHighlightedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewHighlight), abilityCardUISkin.regularHighlightedPreviewBackground));
					abilityCardUISkin.selectedHighlightedPreviewBackground = (instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewSelectedHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewSelectedHighlight).cardSkin.selectedHighlightedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewSelectedHighlight), abilityCardUISkin.selectedHighlightedPreviewBackground));
					abilityCardUISkin.selectedPreviewBackground = (instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewSelected) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewSelected).cardSkin.selectedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewSelected), abilityCardUISkin.selectedPreviewBackground));
					abilityCardUISkin.activePreviewBackground = (instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewActive) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewActive).cardSkin.activePreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewActive), abilityCardUISkin.activePreviewBackground));
					abilityCardUISkin.activeHighlightedPreviewBackground = (instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewActiveHighlight) ? instance.GetCharacterConfigUIFromString(abilityCardConfig.PreviewActiveHighlight).cardSkin.activeHighlightedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewActiveHighlight), abilityCardUISkin.activeHighlightedPreviewBackground));
					abilityCardUISkin.longRestActionRegularSprite = ((abilityCardConfig.LongRestRegular == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestActionRegularSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.LongRestRegular), abilityCardUISkin.longRestActionRegularSprite));
					abilityCardUISkin.longRestActionHighlightSprite = ((abilityCardConfig.LongRestHighlight == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestActionHighlightSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.LongRestHighlight), abilityCardUISkin.longRestActionHighlightSprite));
					abilityCardUISkin.longRestActionSelectedSprite = ((abilityCardConfig.LongRestSelected == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestActionSelectedSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.LongRestSelected), abilityCardUISkin.longRestActionSelectedSprite));
					abilityCardUISkin.longRestActionDisabledSprite = ((abilityCardConfig.LongRestHighlight == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestActionDisabledSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.LongRestDisabled), abilityCardUISkin.longRestActionDisabledSprite));
					abilityCardUISkin.discardedPreviewBackground = ((abilityCardConfig.PreviewDiscarded == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.discardedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewDiscarded), abilityCardUISkin.discardedPreviewBackground));
					abilityCardUISkin.lostPreviewBackground = ((abilityCardConfig.PreviewLost == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.lostPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLost), abilityCardUISkin.lostPreviewBackground));
					abilityCardUISkin.permalostPreviewBackground = ((abilityCardConfig.PreviewPermalost == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.permalostPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewPermalost), abilityCardUISkin.permalostPreviewBackground));
					abilityCardUISkin.longRestPreviewBackground = ((abilityCardConfig.PreviewLongRest == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLongRest), abilityCardUISkin.longRestPreviewBackground));
					abilityCardUISkin.longRestDiscardedPreviewBackground = ((abilityCardConfig.PreviewLongRestDiscarded == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestDiscardedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLongRestDiscarded), abilityCardUISkin.longRestDiscardedPreviewBackground));
					abilityCardUISkin.longRestHighlightedPreviewBackground = ((abilityCardConfig.PreviewLongRestHighlight == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestHighlightedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLongRestHighlight), abilityCardUISkin.longRestHighlightedPreviewBackground));
					abilityCardUISkin.longRestLostPreviewBackground = ((abilityCardConfig.PreviewLongRestLost == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestLostPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLongRestLost), abilityCardUISkin.longRestLostPreviewBackground));
					abilityCardUISkin.longRestSelectedPreviewBackground = ((abilityCardConfig.PreviewLongRestSelected == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestSelectedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLongRestSelected), abilityCardUISkin.longRestSelectedPreviewBackground));
					abilityCardUISkin.longRestSelectedHighlightedPreviewBackground = ((abilityCardConfig.PreviewLongRestSelectedHighlight == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.longRestSelectedHighlightedPreviewBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.PreviewLongRestSelectedHighlight), abilityCardUISkin.longRestSelectedHighlightedPreviewBackground));
					abilityCardUISkin.buttonsHolderSprite = ((abilityCardConfig.ButtonHolder == "Default") ? instance.GetCharacterConfigUIFromString(abilityCardConfig.DefaultCharacter).cardSkin.buttonsHolderSprite : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, abilityCardConfig.ButtonHolder), abilityCardUISkin.buttonsHolderSprite));
				}
				ColorUtility.TryParseHtmlString(abilityCardConfig.PreviewRegularTextColor, out var color);
				abilityCardUISkin.previewRegularTextColor = color;
				ColorUtility.TryParseHtmlString(abilityCardConfig.PreviewActiveTextColor, out color);
				abilityCardUISkin.previewActiveTextColor = color;
				ColorUtility.TryParseHtmlString(abilityCardConfig.PreviewSelectedTextColor, out color);
				abilityCardUISkin.previewSelectedTextColor = color;
				ColorUtility.TryParseHtmlString(abilityCardConfig.PreviewDiscardedTextColor, out color);
				abilityCardUISkin.previewDiscardedTextColor = color;
				ColorUtility.TryParseHtmlString(abilityCardConfig.PreviewLostTextColor, out color);
				abilityCardUISkin.previewLostTextColor = color;
				ColorUtility.TryParseHtmlString(abilityCardConfig.PreviewPermalostTextColor, out color);
				abilityCardUISkin.previewPermalostTextColor = color;
				ColorUtility.TryParseHtmlString(abilityCardConfig.InitiativeColor, out color);
				abilityCardUISkin.initiativeColor = color;
				abilityCardUISkin.previewDiscardedTextOpacity = abilityCardConfig.PreviewDiscardedTextOpacity;
				abilityCardUISkin.previewLostTextOpacity = abilityCardConfig.PreviewLostTextOpacity;
				abilityCardUISkin.ID = abilityCardConfig.ID;
				ModdedAbilityCardConfigs.Add(abilityCardUISkin);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	private void GenerateModdedCharacterConfigs(GHRuleset ruleset)
	{
		ModdedCharacterConfigs = new List<CharacterConfigUI>();
		UIInfoTools instance = UIInfoTools.Instance;
		try
		{
			foreach (CharacterConfigYMLData configData in ScenarioRuleClient.SRLYML.CharacterConfigs)
			{
				CharacterConfigUI characterConfigUI = (CharacterConfigUI)ScriptableObject.CreateInstance("CharacterConfigUI");
				CharacterConfigUI characterConfigUI2 = instance.GetCharacterConfigUI(configData.Model);
				characterConfigUI.questConfig = new QuestTypeConfigUI();
				characterConfigUI.tabIconConfig = new CharacterTabSkin();
				Sprite sprite = null;
				GHMod[] linkedMods = ruleset.LinkedMods;
				foreach (GHMod gHMod in linkedMods)
				{
					characterConfigUI.IconClass = (instance.GetCharacterConfigUIFromString(configData.Icon) ? instance.GetCharacterConfigUIFromString(configData.Icon).IconClass : ((characterConfigUI.IconClass == null) ? new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.Icon))) : characterConfigUI.IconClass));
					characterConfigUI.IconClassHighlight = (instance.GetCharacterConfigUIFromString(configData.IconHighlight) ? instance.GetCharacterConfigUIFromString(configData.IconHighlight).IconClassHighlight : ((characterConfigUI.IconClassHighlight == null) ? new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.IconHighlight))) : characterConfigUI.IconClassHighlight));
					characterConfigUI.IconClassGold = (instance.GetCharacterConfigUIFromString(configData.IconGold) ? instance.GetCharacterConfigUIFromString(configData.IconGold).IconClassGold : ((characterConfigUI.IconClassGold == null) ? new ReferenceToSprite(LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.IconGold))) : characterConfigUI.IconClassGold));
					characterConfigUI.newAdventurePortrait = (instance.GetCharacterConfigUIFromString(configData.NewAdventurePortrait) ? instance.GetCharacterConfigUIFromString(configData.NewAdventurePortrait).newAdventurePortrait : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.NewAdventurePortrait), characterConfigUI.newAdventurePortrait));
					characterConfigUI.newHighlightAdventurePortrait = (instance.GetCharacterConfigUIFromString(configData.NewAdventurePortraitHighlight) ? instance.GetCharacterConfigUIFromString(configData.NewAdventurePortraitHighlight).newHighlightAdventurePortrait : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.NewAdventurePortraitHighlight), characterConfigUI.newHighlightAdventurePortrait));
					characterConfigUI.questConfig.campaingRewardIcon = (instance.GetCharacterConfigUIFromString(configData.CampaignRewardIcon) ? instance.GetCharacterConfigUIFromString(configData.CampaignRewardIcon).questConfig.campaingRewardIcon : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.CampaignRewardIcon), characterConfigUI.questConfig.campaingRewardIcon));
					characterConfigUI.questConfig.marker = (instance.GetCharacterConfigUIFromString(configData.MapMarker) ? instance.GetCharacterConfigUIFromString(configData.MapMarker).questConfig.marker : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.MapMarker), characterConfigUI.questConfig.marker));
					characterConfigUI.scenarioPortrait = (instance.GetCharacterConfigUIFromString(configData.ScenarioPortrait) ? instance.GetCharacterConfigUIFromString(configData.ScenarioPortrait).scenarioPortrait : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.ScenarioPortrait), characterConfigUI.scenarioPortrait));
					characterConfigUI.scenarioPreviewInfoPortrait = (instance.GetCharacterConfigUIFromString(configData.ScenarioPreviewPortrait) ? instance.GetCharacterConfigUIFromString(configData.ScenarioPreviewPortrait).scenarioPreviewInfoPortrait : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.ScenarioPreviewPortrait), characterConfigUI.scenarioPreviewInfoPortrait));
					characterConfigUI.initativeExtensionBackground = (instance.GetCharacterConfigUIFromString(configData.InitiativeBackground) ? instance.GetCharacterConfigUIFromString(configData.InitiativeBackground).initativeExtensionBackground : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.InitiativeBackground), characterConfigUI.initativeExtensionBackground));
					characterConfigUI.tabIconConfig.normalIcon = (instance.GetCharacterConfigUIFromString(configData.TabIcon) ? instance.GetCharacterConfigUIFromString(configData.TabIcon).tabIconConfig.normalIcon : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.TabIcon), characterConfigUI.tabIconConfig.normalIcon));
					characterConfigUI.tabIconConfig.selectedIcon = (instance.GetCharacterConfigUIFromString(configData.TabIconSelected) ? instance.GetCharacterConfigUIFromString(configData.TabIconSelected).tabIconConfig.selectedIcon : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.TabIconSelected), characterConfigUI.tabIconConfig.selectedIcon));
					characterConfigUI.activeAbility = (instance.GetCharacterConfigUIFromString(configData.ActiveAbilityIcon) ? instance.GetCharacterConfigUIFromString(configData.ActiveAbilityIcon).activeAbility : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.ActiveAbilityIcon), characterConfigUI.activeAbility));
					characterConfigUI.assemblyPortrait = (instance.GetCharacterConfigUIFromString(configData.AssemblyPortrait) ? instance.GetCharacterConfigUIFromString(configData.AssemblyPortrait).assemblyPortrait : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.AssemblyPortrait), characterConfigUI.assemblyPortrait));
					characterConfigUI.distributionPortrait = (instance.GetCharacterConfigUIFromString(configData.DistributionPortrait) ? instance.GetCharacterConfigUIFromString(configData.DistributionPortrait).distributionPortrait : LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.DistributionPortrait), characterConfigUI.distributionPortrait));
					sprite = LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, configData.Avatar), sprite);
				}
				ColorUtility.TryParseHtmlString(configData.Color, out var color);
				characterConfigUI.questConfig.color = color;
				characterConfigUI.cardSkin = ModdedAbilityCardConfigs.Single((AbilityCardUISkin s) => s.ID == configData.CardConfig);
				characterConfigUI.gender = characterConfigUI2.gender;
				characterConfigUI.alternativeSkins = characterConfigUI2.alternativeSkins;
				characterConfigUI.questConfig.guildmasterRewardIcon = characterConfigUI2.questConfig.guildmasterRewardIcon;
				characterConfigUI.customAssemblyCharacter3DPosition = characterConfigUI2.customAssemblyCharacter3DPosition;
				characterConfigUI.guildmasterAssemblyCharacterPlacement = characterConfigUI2.guildmasterAssemblyCharacterPlacement;
				characterConfigUI.campaignAssemblyCharacterPlacement = characterConfigUI2.campaignAssemblyCharacterPlacement;
				characterConfigUI.assemblyCharacter3DCompanions = characterConfigUI2.assemblyCharacter3DCompanions;
				characterConfigUI.rosterPortraitOffset = characterConfigUI2.rosterPortraitOffset;
				characterConfigUI.hasToReveal = characterConfigUI2.hasToReveal;
				characterConfigUI.ID = configData.ID;
				characterConfigUI.character = configData.Model;
				sprite.name = configData.Avatar;
				ModdedCharacterConfigs.Add(characterConfigUI);
				ModdedAvatars.Add(sprite);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	private void GenerateModdedMonsterConfigs(GHRuleset ruleset)
	{
		ModdedMonsterPortraits = new List<Sprite>();
		try
		{
			foreach (MonsterConfigYMLData monsterConfig in ScenarioRuleClient.SRLYML.MonsterConfigs)
			{
				Sprite sprite = null;
				Sprite sprite2 = null;
				GHMod[] linkedMods = ruleset.LinkedMods;
				foreach (GHMod gHMod in linkedMods)
				{
					sprite = LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, monsterConfig.Avatar), sprite);
					sprite2 = LoadSprite(Path.Combine(gHMod.CustomImagesDirectory, monsterConfig.Portrait), sprite2);
				}
				sprite.name = monsterConfig.Avatar;
				sprite2.name = monsterConfig.Portrait;
				ModdedMonsterPortraits.Add(sprite2);
				ModdedAvatars.Add(sprite);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	private void GenerateModdedMapImages(GHRuleset ruleset)
	{
		ModdedMapImages = new Dictionary<MapChoreographer.ECustomMapAlignment, Texture2D>();
		try
		{
			MapConfigYMLData mapConfigYMLData = ScenarioRuleClient.SRLYML.MapConfigs.FirstOrDefault();
			if (mapConfigYMLData != null)
			{
				Texture2D texture2D = null;
				Texture2D texture2D2 = null;
				Texture2D texture2D3 = null;
				Texture2D texture2D4 = null;
				GHMod[] linkedMods = ruleset.LinkedMods;
				foreach (GHMod gHMod in linkedMods)
				{
					texture2D = ((texture2D == null) ? GloomUtility.LoadTGA(Path.Combine(gHMod.CustomImagesDirectory, mapConfigYMLData.NorthWest)) : texture2D);
					texture2D2 = ((texture2D2 == null) ? GloomUtility.LoadTGA(Path.Combine(gHMod.CustomImagesDirectory, mapConfigYMLData.NorthEast)) : texture2D2);
					texture2D3 = ((texture2D3 == null) ? GloomUtility.LoadTGA(Path.Combine(gHMod.CustomImagesDirectory, mapConfigYMLData.SouthWest)) : texture2D3);
					texture2D4 = ((texture2D4 == null) ? GloomUtility.LoadTGA(Path.Combine(gHMod.CustomImagesDirectory, mapConfigYMLData.SouthEast)) : texture2D4);
				}
				ModdedMapImages.Add(MapChoreographer.ECustomMapAlignment.NorthWest, texture2D);
				ModdedMapImages.Add(MapChoreographer.ECustomMapAlignment.NorthEast, texture2D2);
				ModdedMapImages.Add(MapChoreographer.ECustomMapAlignment.SouthWest, texture2D3);
				ModdedMapImages.Add(MapChoreographer.ECustomMapAlignment.SouthEast, texture2D4);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	public Sprite LoadSprite(string path, Sprite sprite = null)
	{
		if (sprite != null)
		{
			return sprite;
		}
		Texture2D texture2D = LoadTexture2D(path);
		if (texture2D == null)
		{
			return null;
		}
		return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
	}

	public Texture2D LoadTexture2D(string path)
	{
		Texture2D texture2D = null;
		if (File.Exists(path))
		{
			byte[] data = File.ReadAllBytes(path);
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
		}
		return texture2D;
	}

	public void ReturnToMainMenu()
	{
		IsUnloading = true;
		CoroutineHelper.RunCoroutine(ReturnToMainMenuCoroutine());
	}

	public IEnumerator ReturnToMainMenuCoroutine()
	{
		SceneController.Instance.ShowLoadingScreen();
		yield return null;
		Unload(regenCards: false);
		yield return RegenCardsCoroutine();
		AsyncOperation asyncOp = Resources.UnloadUnusedAssets();
		while (!asyncOp.isDone)
		{
			yield return null;
		}
		IsUnloading = false;
		SceneController.Instance.DisableLoadingScreen();
		m_ReturnToMainMenuCompleteCallback?.Invoke();
		m_ReturnToMainMenuCompleteCallback = null;
	}

	public static void SetReturnToMainMenuCompleteCallback(Action callbackToSet)
	{
		m_ReturnToMainMenuCompleteCallback = callbackToSet;
	}

	public void Unload(bool regenCards)
	{
		SceneController.Instance.SelectingPersonalQuest = false;
		SceneController.Instance.BusyProcessingResults = false;
		SceneController.Instance.RetiringCharacter = false;
		SceneController.Instance.CheckingLockedContent = false;
		UIAdventurePartyAssemblyWindow.IsChangingCharacter = false;
		SaveData.Instance.Global.StopSpeedUp();
		SaveData.Instance.Global.CurrentModdedRuleset = null;
		if (ModdedLangSource != null && I2.Loc.LocalizationManager.Sources.Contains(ModdedLangSource))
		{
			I2.Loc.LocalizationManager.Sources.Remove(ModdedLangSource);
			ModdedLangSource = null;
		}
		if (m_ModdedZip != null)
		{
			m_ModdedZip.Dispose();
			m_ModdedZip = null;
		}
		ScenarioRuleClient.SRLYML.UnloadRuleset();
		ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.Global;
		MapRuleLibraryClient.MRLYML.UnloadRuleset();
		MapRuleLibraryClient.MRLYML.YMLMode = CSRLYML.EYMLMode.Global;
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.None;
		if (ObjectPool.instance != null)
		{
			ObjectPool.ClearAllMonsterCards();
		}
		ScenarioManager.CurrentScenarioState = null;
		MapRuleLibraryClient.Instance.Reset();
		if (regenCards)
		{
			CoroutineHelper.RunCoroutine(RegenCardsCoroutine());
		}
	}

	public IEnumerator RegenCardsCoroutine()
	{
		yield return RegenerateModdedAbilityCards();
		yield return RegenerateModdedItemCards();
	}

	private IEnumerator RegenerateModdedAbilityCards()
	{
		if (m_ModdedAbilityCards == null)
		{
			yield break;
		}
		SceneController.Instance.ShowLoadingScreen();
		foreach (AbilityCardYMLData abilityCardYML in m_ModdedAbilityCards)
		{
			ObjectPool.RemoveAbilityCard(abilityCardYML.ID);
			AbilityCardYMLData abilityCardYMLData = ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData s) => s.ID == abilityCardYML.ID);
			GameObject newAbilityCard = null;
			AbilityCardUI cardUI = null;
			if (abilityCardYMLData == null)
			{
				continue;
			}
			bool cardCreated;
			try
			{
				cardCreated = PersistentData.CreateAbilityCard1(abilityCardYML, out newAbilityCard, out cardUI);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to unload Modded Ruleset\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
				yield break;
			}
			yield return null;
			if (cardCreated)
			{
				try
				{
					PersistentData.CreateAbilityCard2(abilityCardYML, newAbilityCard, cardUI);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Unable to unload Modded Ruleset\n" + ex2.Message + "\n" + ex2.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, SceneController.Instance.LoadMainMenu, ex2.Message);
					yield break;
				}
				yield return null;
			}
		}
		m_ModdedAbilityCards = null;
		SceneController.Instance.DisableLoadingScreen();
	}

	private IEnumerator RegenerateModdedItemCards()
	{
		if (m_ModdedItemCards == null)
		{
			yield break;
		}
		SceneController.Instance.ShowLoadingScreen();
		foreach (ItemCardYMLData itemCardYML in m_ModdedItemCards)
		{
			ObjectPool.RemoveItemCard(itemCardYML.ID);
			ItemCardYMLData itemCardYMLData = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData s) => s.ID == itemCardYML.ID);
			GameObject newItemCard = null;
			ItemCardUI cardUI = null;
			if (itemCardYMLData == null)
			{
				continue;
			}
			bool cardCreated;
			try
			{
				cardCreated = PersistentData.CreateItemCard1(itemCardYML, out newItemCard, out cardUI);
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to unload Modded Ruleset\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
				yield break;
			}
			yield return null;
			if (cardCreated)
			{
				try
				{
					PersistentData.CreateItemCard2(itemCardYML, newItemCard, cardUI);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Unable to unload Modded Ruleset\n" + ex2.Message + "\n" + ex2.StackTrace);
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, SceneController.Instance.LoadMainMenu, ex2.Message);
					yield break;
				}
				yield return null;
			}
		}
		m_ModdedItemCards = null;
		SceneController.Instance.DisableLoadingScreen();
	}

	private bool ProcessMod(GHMod mod, CSRLYML.EYMLMode ymlMode, string writeErrorsToFile)
	{
		bool flag = true;
		DateTime now = DateTime.Now;
		Debug.Log("[YML] Starting parse for Mod " + mod.MetaData.Name);
		flag = GetYMLParserTypes(mod.ModdedYMLFiles, out var filesByParser, writeErrorsToFile);
		SharedClient.ValidationRecord.HideLogErrorMessage = true;
		if (flag)
		{
			int num = 0;
			foreach (EYMLParserType item in EYMLParserTypes.Where((EYMLParserType w) => w != EYMLParserType.None))
			{
				if (!filesByParser.ContainsKey(item.ToString()))
				{
					continue;
				}
				foreach (string item2 in filesByParser[item.ToString()])
				{
					num++;
					using MemoryStream stream = new MemoryStream(File.ReadAllBytes(item2));
					if (!ProcessYMLFile(stream, ymlMode, item2, writeErrorsToFile))
					{
						flag = false;
					}
				}
			}
			string[] customLevelFiles = mod.CustomLevelFiles;
			foreach (string file in customLevelFiles)
			{
				num++;
				if (!ProcessCustomLevel(file, ymlMode))
				{
					flag = false;
				}
			}
			if (flag)
			{
				int num3 = mod.ModdedYMLFiles.Length + mod.CustomLevelFiles.Length;
				if (num != num3)
				{
					string text = "Invalid entries exist within the mod " + mod.MetaData.Name + ".  Entry count: " + num3 + "  Process Entries Count: " + num;
					Debug.LogError(text);
					if (writeErrorsToFile != null)
					{
						WriteValidationToFile(writeErrorsToFile, text);
					}
					flag = false;
				}
				Debug.Log("[YML] Finished parse for Mod " + mod.MetaData.Name + ".  Duration: " + (DateTime.Now - now).TotalSeconds);
			}
		}
		SharedClient.ValidationRecord.HideLogErrorMessage = false;
		return flag;
	}

	private bool ProcessZipFile(ZipFile zipFile, CSRLYML.EYMLMode ymlMode)
	{
		bool result = true;
		DateTime now = DateTime.Now;
		Debug.Log("[YML] Starting parse for " + zipFile.Name);
		int num = 0;
		foreach (EYMLParserType parserType in EYMLParserTypes.Where((EYMLParserType w) => w != EYMLParserType.None))
		{
			foreach (ZipEntry item in zipFile.Where((ZipEntry w) => Path.GetExtension(w.FileName) == ".yml" && w.FileName.Split('/')[0] == parserType.ToString()))
			{
				num++;
				if (!ProcessZipEntry(item, ymlMode))
				{
					result = false;
				}
			}
		}
		foreach (ZipEntry item2 in zipFile.Where((ZipEntry w) => Path.GetExtension(w.FileName) == ".lvldat"))
		{
			num++;
			if (!ProcessCustomLevel(item2, ymlMode))
			{
				result = false;
			}
		}
		if (num != zipFile.Count)
		{
			Debug.LogError("Invalid entries exist within the ruleset " + zipFile.Name + ".  Entry count: " + zipFile.Count + "  Process Entries Count: " + num);
			result = false;
		}
		Debug.Log("[YML] Finished parse for " + zipFile.Name + "  Duration: " + (DateTime.Now - now).TotalSeconds);
		return result;
	}

	private bool ProcessCustomLevel(ZipEntry entry, CSRLYML.EYMLMode ymlMode)
	{
		if (ymlMode == CSRLYML.EYMLMode.None)
		{
			Debug.LogError("Invalid YML mode set for SRL ruleset processing!");
			return false;
		}
		bool result = true;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			entry.Extract(memoryStream);
			using StreamReader streamReader = new StreamReader(memoryStream);
			streamReader.DiscardBufferedData();
			streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
			switch (ymlMode)
			{
			case CSRLYML.EYMLMode.Global:
				ScenarioRuleClient.SRLYML.GlobalData.CustomLevels.Add(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(entry.FileName), memoryStream.ToArray()));
				break;
			case CSRLYML.EYMLMode.StandardRuleset:
				ScenarioRuleClient.SRLYML.RulesetData.CustomLevels.Add(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(entry.FileName), memoryStream.ToArray()));
				break;
			case CSRLYML.EYMLMode.ModdedRuleset:
				ScenarioRuleClient.SRLYML.ModdedData.CustomLevels.Add(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(entry.FileName), memoryStream.ToArray()));
				break;
			default:
				Debug.LogError("Invalid YML mode for Custom Level " + ymlMode);
				result = false;
				break;
			}
		}
		return result;
	}

	private bool ProcessCustomLevel(string file, CSRLYML.EYMLMode ymlMode)
	{
		if (ymlMode == CSRLYML.EYMLMode.None)
		{
			Debug.LogError("Invalid YML mode set for SRL ruleset processing!");
			return false;
		}
		bool result = true;
		switch (ymlMode)
		{
		case CSRLYML.EYMLMode.Global:
			ScenarioRuleClient.SRLYML.GlobalData.CustomLevels.Add(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(file), File.ReadAllBytes(file)));
			break;
		case CSRLYML.EYMLMode.StandardRuleset:
			ScenarioRuleClient.SRLYML.RulesetData.CustomLevels.Add(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(file), File.ReadAllBytes(file)));
			break;
		case CSRLYML.EYMLMode.ModdedRuleset:
			ScenarioRuleClient.SRLYML.ModdedData.CustomLevels.Add(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(file), File.ReadAllBytes(file)));
			break;
		default:
			Debug.LogError("Invalid YML mode for Custom Level " + ymlMode);
			result = false;
			break;
		}
		return result;
	}

	private bool ProcessAutoComplete(ScenarioManager.EDLLMode mode, CSRLYML.EYMLMode ymlMode)
	{
		if (ymlMode == CSRLYML.EYMLMode.None)
		{
			Debug.LogError("Invalid YML mode set for SRL ruleset processing!");
			return false;
		}
		bool result = true;
		string fileName = ((mode == ScenarioManager.EDLLMode.Guildmaster) ? GuildmasterAutoCompletePath : CampaignAutoCompletePath);
		switch (ymlMode)
		{
		case CSRLYML.EYMLMode.Global:
			if (!MapRuleLibraryClient.MRLYML.GlobalData.AutoCompletes.ProcessFile(fileName))
			{
				result = false;
				ShowYMLErrors(fileName, null);
			}
			break;
		case CSRLYML.EYMLMode.StandardRuleset:
			if (!MapRuleLibraryClient.MRLYML.RulesetData.AutoCompletes.ProcessFile(fileName))
			{
				result = false;
				ShowYMLErrors(fileName, null);
			}
			break;
		case CSRLYML.EYMLMode.ModdedRuleset:
			if (!MapRuleLibraryClient.MRLYML.ModdedData.AutoCompletes.ProcessFile(fileName))
			{
				result = false;
				ShowYMLErrors(fileName, null);
			}
			break;
		default:
			Debug.LogError("Invalid YML mode for Custom Level " + ymlMode);
			result = false;
			break;
		}
		return result;
	}

	private bool ProcessZipEntry(ZipEntry entry, CSRLYML.EYMLMode ymlMode)
	{
		try
		{
			if (ymlMode == CSRLYML.EYMLMode.None)
			{
				Debug.LogError("Invalid YML mode set for SRL ruleset processing!");
				return false;
			}
			bool result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				entry.Extract(memoryStream);
				result = ProcessYMLFile(memoryStream, ymlMode, entry.FileName);
				memoryStream.Close();
			}
			return result;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception running ProcessZipEntry.\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public bool ProcessYMLFile(MemoryStream stream, CSRLYML.EYMLMode ymlMode, string fileName, string writeErrorsToFile = null)
	{
		try
		{
			bool result = true;
			using (StreamReader streamReader = new StreamReader(stream))
			{
				try
				{
					streamReader.DiscardBufferedData();
					streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
					string text = streamReader.ReadLine();
					switch (GetParserType(text, fileName))
					{
					case EYMLParserType.None:
					{
						string text2 = "No parser specified for file " + fileName;
						Debug.LogError(text2);
						if (writeErrorsToFile != null)
						{
							WriteValidationToFile(writeErrorsToFile, text2);
						}
						result = false;
						break;
					}
					case EYMLParserType.HeroSummon:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.HeroSummons.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.HeroSummons.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.HeroSummons.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Character:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.Characters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.Characters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.Characters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.AbilityCard:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.AbilityCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.AbilityCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.AbilityCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.ItemCard:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.ItemCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.ItemCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.ItemCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Perk:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.Perks.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.Perks.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.Perks.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Enhancements:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.Enhancements.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.Enhancements.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.Enhancements.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.IconGlossary:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.IconGlossary.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.IconGlossary.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.IconGlossary.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.AttackModifier:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.AttackModifiers.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.AttackModifiers.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.AttackModifiers.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.AttackModifierDeck:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.AttackModifierDecks.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.AttackModifierDecks.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.AttackModifierDecks.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.EnemyCard:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.MonsterCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.MonsterCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.MonsterCards.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Enemy:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.Monsters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.Monsters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.Monsters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.TreasureTable:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.TreasureTables.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.TreasureTables.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.TreasureTables.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Scenario:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.Scenarios.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.Scenarios.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.Scenarios.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.MonsterData:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.MonsterData.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.MonsterData.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.MonsterData.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Rooms:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.ScenarioRooms.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.ScenarioRooms.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.ScenarioRooms.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.ScenarioAbility:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.ScenarioAbilities.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.ScenarioAbilities.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.ScenarioAbilities.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.CharacterResources:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.CharacterResources.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.CharacterResources.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.CharacterResources.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.ItemConfig:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.ItemConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.ItemConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.ItemConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.AbilityCardConfig:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.AbilityCardConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.AbilityCardConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.AbilityCardConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.CharacterConfig:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.CharacterConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.CharacterConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.CharacterConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.MonsterConfig:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.MonsterConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.MonsterConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.MonsterConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.MapConfig:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!ScenarioRuleClient.SRLYML.GlobalData.MapConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!ScenarioRuleClient.SRLYML.RulesetData.MapConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!ScenarioRuleClient.SRLYML.ModdedData.MapConfigs.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Difficulty:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.Difficulty.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.Difficulty.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.Difficulty.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Achievement:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.Achievements.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.Achievements.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.Achievements.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.BattleGoal:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.BattleGoals.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.BattleGoals.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.BattleGoals.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.PersonalQuest:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.PersonalQuests.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.PersonalQuests.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.PersonalQuests.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.RoadEvent:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.RoadEvents.ProcessFile(streamReader, fileName, "RoadEvent"))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.RoadEvents.ProcessFile(streamReader, fileName, "RoadEvent"))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.RoadEvents.ProcessFile(streamReader, fileName, "RoadEvent"))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.CityEvent:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.CityEvents.ProcessFile(streamReader, fileName, "CityEvent"))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.CityEvents.ProcessFile(streamReader, fileName, "CityEvent"))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.CityEvents.ProcessFile(streamReader, fileName, "CityEvent"))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.InitialEvents:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.InitialEvents.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.InitialEvents.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.InitialEvents.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.VisibilitySphere:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.VisibilitySpheres.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.VisibilitySpheres.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.VisibilitySpheres.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Temple:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.Temples.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.Temples.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.Temples.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Village:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.Villages.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.Villages.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.Villages.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.StoreLocation:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.StoreLocations.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.StoreLocations.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.StoreLocations.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Quest:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.Quests.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.Quests.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.Quests.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Message:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.MapMessages.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.MapMessages.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.MapMessages.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					case EYMLParserType.Headquarters:
						switch (ymlMode)
						{
						case CSRLYML.EYMLMode.Global:
							if (!MapRuleLibraryClient.MRLYML.GlobalData.Headquarters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.StandardRuleset:
							if (!MapRuleLibraryClient.MRLYML.RulesetData.Headquarters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						case CSRLYML.EYMLMode.ModdedRuleset:
							if (!MapRuleLibraryClient.MRLYML.ModdedData.Headquarters.ProcessFile(streamReader, fileName))
							{
								result = false;
								ShowYMLErrors(fileName, writeErrorsToFile);
							}
							break;
						}
						break;
					default:
						result = false;
						Debug.LogError("Invalid parser type " + text + " for Global Data in file " + fileName);
						break;
					case EYMLParserType.RemoveYML:
						break;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception running ProcessYMLFile Internal.\n" + ex.Message + "\n" + ex.StackTrace);
				}
				finally
				{
					streamReader.Close();
				}
			}
			return result;
		}
		catch (Exception ex2)
		{
			Debug.LogError("Exception running ProcessYMLFile.\n" + ex2.Message + "\n" + ex2.StackTrace);
			return false;
		}
	}

	private void ShowYMLErrors(string fileName, string writeErrorsToFile)
	{
		string text = "Failed to parse file " + fileName + "\n" + string.Join("\n", SharedClient.ValidationRecord.RecordedFailures.Select((YMLValidationRecord.CYMLValidationFailure s) => s.Message));
		Debug.LogError(text);
		if (writeErrorsToFile != null)
		{
			WriteValidationToFile(writeErrorsToFile, text);
		}
		SharedClient.ValidationRecord.RecordedFailures.Clear();
	}

	public EYMLParserType GetParserType(string parserLine, string fileName, string writeErrorsToFile = null)
	{
		try
		{
			string parser = parserLine.Split(':')[1].Trim();
			return EYMLParserTypes.SingleOrDefault((EYMLParserType s) => s.ToString() == parser);
		}
		catch
		{
			string text = "Invalid Parser " + parserLine + " in file " + fileName;
			Debug.LogError(text);
			if (writeErrorsToFile != null)
			{
				WriteValidationToFile(writeErrorsToFile, text);
			}
			return EYMLParserType.None;
		}
	}

	private string GetSafeFileName(ZipFile zip, string file, string zipFolder)
	{
		int num = 0;
		string text = file;
		while (zip.EntryFileNames.Contains(zipFolder + "/" + Path.GetFileName(text)))
		{
			num++;
			text = file.Insert(file.LastIndexOf("."), num.ToString());
		}
		return text;
	}

	public void WriteValidationToFile(string filePath, string appendText)
	{
		try
		{
			File.AppendAllText(filePath, appendText + "\n");
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception running YMLLoading.WriteValidationToFile.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public bool GetYMLParserTypes(string[] ymlFiles, out Dictionary<string, List<string>> filesByParser, string writeErrorsToFile = null)
	{
		filesByParser = new Dictionary<string, List<string>>();
		foreach (string text in ymlFiles)
		{
			string text2 = GetParserType(File.ReadLines(text).First(), text).ToString();
			if (text2 == "None")
			{
				if (writeErrorsToFile != null)
				{
					WriteValidationToFile(writeErrorsToFile, "Invalid parser type in file " + text);
				}
				return false;
			}
			if (filesByParser.ContainsKey(text2))
			{
				filesByParser[text2].Add(text);
				continue;
			}
			filesByParser[text2] = new List<string> { text };
		}
		return true;
	}
}
