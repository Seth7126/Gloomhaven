#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Permissions;
using Assets.Script.GUI.MainMenu.Modding;
using ScenarioRuleLibrary.YML;

[Serializable]
public class GHRuleset : ISerializable, IRuleset
{
	public enum ERulesetType
	{
		None,
		Campaign,
		Guildmaster
	}

	private const string c_RulesetCompileFolderName = "Compiled";

	public static ERulesetType[] RulesetTypes = (ERulesetType[])Enum.GetValues(typeof(ERulesetType));

	public string Name { get; set; }

	public ERulesetType RulesetType { get; private set; }

	public List<string> LinkedModNames { get; private set; }

	public string CompiledHash { get; set; }

	public bool IsValid { get; private set; }

	public List<string> CompiledAbilityCards { get; private set; }

	public List<string> CompiledItemCards { get; private set; }

	public bool IsMPRuleset { get; set; }

	public bool IsCompiled => PlatformLayer.FileSystem.ExistsFile(RulesetCompiledZip);

	public GHMod[] LinkedMods => SceneController.Instance.Modding.Mods.Where((GHMod w) => LinkedModNames.Contains(w.MetaData.Name)).ToArray();

	public List<string> LinkedCSV { get; set; }

	public string RulesetFolder => Path.Combine(RootSaveData.ModdedRulesetsFolder, Name);

	public string RulesetCompileFolder => Path.Combine(RulesetFolder, "Compiled");

	public string RulesetCompiledZip => Path.Combine(RulesetCompileFolder, Name + ".zip");

	public string RulesetMetaDataPath => Path.Combine(RulesetFolder, "ruleset.mod");

	public List<IMod> GetMods()
	{
		return ModdingService.WrapData(LinkedMods.ToList());
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("RulesetType", RulesetType.ToString());
		info.AddValue("LinkedModNames", LinkedModNames);
		info.AddValue("CompiledHash", CompiledHash);
		info.AddValue("CompiledAbilityCards", CompiledAbilityCards);
		info.AddValue("CompiledItemCards", CompiledItemCards);
		info.AddValue("LinkedCSV", LinkedCSV);
		info.AddValue("IsMPRuleset", IsMPRuleset);
	}

	public GHRuleset(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "RulesetType":
					RulesetType = RulesetTypes.SingleOrDefault((ERulesetType s) => s.ToString() == info.GetString("RulesetType"));
					break;
				case "LinkedModNames":
					LinkedModNames = (List<string>)info.GetValue("LinkedModNames", typeof(List<string>));
					break;
				case "CompiledHash":
					CompiledHash = info.GetString("CompiledHash");
					break;
				case "CompiledAbilityCards":
					CompiledAbilityCards = (List<string>)info.GetValue("CompiledAbilityCards", typeof(List<string>));
					break;
				case "CompiledItemCards":
					CompiledItemCards = (List<string>)info.GetValue("CompiledItemCards", typeof(List<string>));
					break;
				case "LinkedCSV":
					LinkedCSV = (List<string>)info.GetValue("LinkedCSV", typeof(List<string>));
					break;
				case "IsMPRuleset":
					IsMPRuleset = info.GetBoolean("IsMPRuleset");
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to deserialize GHRuleset entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public GHRuleset(string name, ERulesetType type)
	{
		Name = name;
		RulesetType = type;
		LinkedModNames = new List<string>();
	}

	public bool Save()
	{
		try
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.ShowProgress();
			}
			if (PlatformLayer.Instance.PlatformID != "GameCore" && !PlatformLayer.FileSystem.ExistsDirectory(RulesetFolder))
			{
				Debug.Log("Creating RulesetFolder: " + RulesetFolder);
				PlatformLayer.FileSystem.CreateDirectory(RulesetFolder);
			}
			using MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, this);
			PlatformLayer.FileSystem.WriteFile(memoryStream.ToArray(), RulesetMetaDataPath);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while trying to save Ruleset.\n" + ex.Message + "\n" + ex.StackTrace);
			try
			{
				if (PlatformLayer.FileSystem.ExistsDirectory(RulesetMetaDataPath))
				{
					PlatformLayer.FileSystem.RemoveFile(RulesetMetaDataPath);
				}
			}
			catch
			{
			}
			return false;
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
		return true;
	}

	public void Update(RulesetDataView newData)
	{
		GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.FirstOrDefault((GHRuleset f) => f.Name == newData.Name);
		if (gHRuleset == null)
		{
			Name = newData.Name;
		}
		else if (gHRuleset != this)
		{
			Debug.LogError("Unable to update ruleset name.  Another ruleset with the name " + newData.Name + " already exists");
		}
		if (newData.RulesetType != RulesetType)
		{
			Debug.LogError("It is not possible to update Ruleset Type after the ruleset is created.");
		}
		LinkedModNames = newData.Mods.Select((IMod s) => s.Name).ToList();
	}

	public bool CompileRuleset()
	{
		try
		{
			if (LinkedModNames.Count == 0)
			{
				Debug.LogError("Unable to compile ruleset " + Name + " as it contains no linked mods");
				return false;
			}
			RemoveYML removeYML = new RemoveYML();
			List<string> list = new List<string>();
			GHMod[] linkedMods = LinkedMods;
			foreach (GHMod gHMod in linkedMods)
			{
				gHMod.Validate(writeResultsToFile: false);
				string[] files = PlatformLayer.FileSystem.GetFiles(gHMod.ModdedYMLDirectory, "*.yml", SearchOption.AllDirectories);
				foreach (string text in files)
				{
					using StreamReader streamReader = new StreamReader(text);
					string parserLine = streamReader.ReadLine();
					if (SceneController.Instance.YML.GetParserType(parserLine, text) == YMLLoading.EYMLParserType.RemoveYML)
					{
						removeYML.ProcessFile(streamReader, text);
					}
				}
			}
			if (LinkedMods.Any((GHMod a) => !a.IsValid))
			{
				Debug.LogError("Unable to compile ruleset " + Name + " as one or more mods included are not valid.");
				return false;
			}
			Debug.Log("Creating RulesetCompfileFolder: " + RulesetCompileFolder);
			if (PlatformLayer.FileSystem.ExistsDirectory(RulesetCompileFolder))
			{
				PlatformLayer.FileSystem.RemoveDirectory(RulesetCompileFolder, recursive: true);
			}
			PlatformLayer.FileSystem.CreateDirectory(RulesetCompileFolder);
			CompiledAbilityCards = new List<string>();
			CompiledItemCards = new List<string>();
			CompiledHash = null;
			foreach (RemoveYMLData item in removeYML.LoadedYML)
			{
				list.AddRange(item.FilesToRemove);
				list.Add(Path.GetFileName(item.FileName));
			}
			if (GHMod.ExportYML(RulesetCompileFolder, RulesetType))
			{
				List<string> list2 = new List<string>();
				linkedMods = LinkedMods;
				foreach (GHMod gHMod2 in linkedMods)
				{
					string text2 = Path.Combine(RulesetCompileFolder, "Z", gHMod2.MetaData.Name);
					PlatformLayer.FileSystem.CreateDirectory(text2);
					foreach (string item2 in gHMod2.MetaData.AppliedFiles.Where((string w) => w.EndsWith(".yml") || w.EndsWith(".lvldat")))
					{
						if (item2.EndsWith(".yml"))
						{
							list2.Add(item2);
						}
						PlatformLayer.FileSystem.CopyFile(item2, Path.Combine(text2, Path.GetFileName(item2)), overwrite: false);
					}
				}
				foreach (string item3 in list)
				{
					string[] files = PlatformLayer.FileSystem.GetFiles(RulesetCompileFolder, item3, SearchOption.AllDirectories);
					foreach (string path in files)
					{
						PlatformLayer.FileSystem.RemoveFile(path);
					}
				}
				if (SceneController.Instance.YML.GetYMLParserTypes(list2.ToArray(), out var filesByParser))
				{
					if (filesByParser.ContainsKey(YMLLoading.EYMLParserType.AbilityCard.ToString()))
					{
						CompiledAbilityCards.AddRange(filesByParser[YMLLoading.EYMLParserType.AbilityCard.ToString()].Select((string s) => Path.GetFileName(s)));
					}
					if (filesByParser.ContainsKey(YMLLoading.EYMLParserType.ItemCard.ToString()))
					{
						CompiledItemCards.AddRange(filesByParser[YMLLoading.EYMLParserType.ItemCard.ToString()].Select((string s) => Path.GetFileName(s)));
					}
					if (SceneController.Instance.YML.CompileRuleset(RulesetCompileFolder, RulesetCompiledZip))
					{
						CompiledHash = GetRulesetHash();
						if (CompiledHash != string.Empty)
						{
							return Save();
						}
					}
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception running CompileRuleset.\n" + ex.Message + "\n" + ex.StackTrace);
			if (PlatformLayer.FileSystem.ExistsDirectory(RulesetCompileFolder))
			{
				try
				{
					PlatformLayer.FileSystem.RemoveDirectory(RulesetCompileFolder, recursive: true);
				}
				catch
				{
				}
			}
			CompiledAbilityCards = new List<string>();
			CompiledItemCards = new List<string>();
			CompiledHash = null;
			return false;
		}
	}

	public string GetRulesetHash()
	{
		if (PlatformLayer.FileSystem.ExistsFile(RulesetCompiledZip))
		{
			using (MD5 mD = MD5.Create())
			{
				using MemoryStream inputStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(RulesetCompiledZip));
				return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", string.Empty).ToLowerInvariant();
			}
		}
		return string.Empty;
	}
}
