using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace Assets.Script.GUI.MainMenu.Modding;

public class ModdingService : IModdingService, IRulesetModService
{
	public List<IMod> GetMods()
	{
		return WrapData(SceneController.Instance.Modding.Mods);
	}

	public static List<IMod> WrapData(List<GHMod> mods)
	{
		return mods.ConvertAll((Converter<GHMod, IMod>)((GHMod it) => new GHModWrapper(it)));
	}

	public IMod CreateMod(ModDataView modData)
	{
		try
		{
			if (SceneController.Instance.Modding.Mods.Exists((GHMod e) => e.MetaData.Name == modData.Name))
			{
				Debug.LogError("A mod with the name " + modData.Name + " already exists");
				return null;
			}
			GHMod gHMod = GHMod.CreateNewMod(new GHModMetaData(modData.Name, modData.Description, modData.ModType, modData.Thumbnail));
			if (gHMod != null)
			{
				SceneController.Instance.Modding.Mods.Add(gHMod);
				return new GHModWrapper(gHMod);
			}
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception running CreateMod.\n" + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}

	public void UploadMod(IMod modData)
	{
		if (SceneController.Instance.Modding.Mods.SingleOrDefault((GHMod s) => s.MetaData.Name == modData.Name) != null && PlatformLayer.Modding.ModdingSupported)
		{
			PlatformLayer.Modding.UploadMod(modData.Name);
		}
	}

	public void EditMod(IMod modData)
	{
		GHMod gHMod = SceneController.Instance.Modding.Mods.SingleOrDefault((GHMod s) => s.MetaData.Name == modData.Name);
		if (gHMod != null && gHMod.IsLocalMod)
		{
			Application.OpenURL(gHMod.LocalPath);
		}
	}

	public void OpenSteam()
	{
		Application.OpenURL("steam://url/SteamWorkshopPage/780290");
	}

	public void ValidateMod(IMod mod, Action onValidatedCallback)
	{
		CoroutineHelper.RunCoroutine(GHModding.ValidateMod(GetModFromIMod(mod), onValidatedCallback, writeResultsToFile: true));
	}

	public void ViewModErrors(IMod modData)
	{
		if (File.Exists(modData.LastValidationResultsFile))
		{
			Application.OpenURL(modData.LastValidationResultsFile);
		}
		else
		{
			Debug.LogError("File does not exist: " + modData.LastValidationResultsFile);
		}
	}

	public static GHMod GetModFromIMod(IMod mod)
	{
		return SceneController.Instance.Modding.Mods.SingleOrDefault((GHMod s) => s.MetaData.Name == mod.Name);
	}

	public void CreateDummyRuleset(IMod modData)
	{
		GHMod modFromIMod = GetModFromIMod(modData);
		GHRuleset.ERulesetType type = ((modFromIMod.MetaData.ModType != GHModMetaData.EModType.Guildmaster) ? GHRuleset.ERulesetType.Campaign : GHRuleset.ERulesetType.Guildmaster);
		GHRuleset gHRuleset = new GHRuleset(modFromIMod.MetaData.Name + "_LEVEL_EDITOR", type);
		gHRuleset.LinkedModNames.Add(modFromIMod.MetaData.Name);
		gHRuleset.Save();
		SceneController.Instance.Modding.Rulesets.Add(gHRuleset);
		SceneController.Instance.Modding.LevelEditorRuleset = gHRuleset;
	}

	public void LoadAndDeleteDummyRuleset(GHRuleset ruleset)
	{
		ScenarioRuleClient.SRLYML.YMLMode = CSRLYML.EYMLMode.ModdedRuleset;
		SceneController.Instance.YML.LoadRulesetZip(ruleset);
		LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Mod;
		ScenarioRuleClient.LoadData();
		GloomUtility.DeleteFolder(ruleset.RulesetFolder, topFolder: true);
	}
}
