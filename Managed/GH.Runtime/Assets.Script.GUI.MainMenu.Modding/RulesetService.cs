using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Script.GUI.MainMenu.Modding;

internal class RulesetService : IRulesetService
{
	public void CreateRuleset(RulesetDataView rulesetData, Action<IRuleset> onSuccessfullyCreated, Action onFailed)
	{
		if (SceneController.Instance.Modding.Rulesets.Exists((GHRuleset e) => e.Name == rulesetData.Name))
		{
			Debug.LogError("A ruleset with the name " + rulesetData.Name + " already exists");
			onFailed();
			return;
		}
		Validate(rulesetData.Mods, delegate(bool isValid)
		{
			if (!isValid)
			{
				onFailed();
			}
			else
			{
				GHRuleset gHRuleset = new GHRuleset(rulesetData.Name, rulesetData.RulesetType);
				gHRuleset.LinkedModNames.AddRange(rulesetData.Mods.Select((IMod s) => s.Name));
				gHRuleset.Save();
				SceneController.Instance.Modding.Rulesets.Add(gHRuleset);
				onSuccessfullyCreated(gHRuleset);
			}
		});
	}

	public List<IMod> GetCompatibleMods(GHRuleset.ERulesetType type)
	{
		return SceneController.Instance.ModService.GetMods().FindAll((IMod it) => IsModCompatible(type, it));
	}

	private bool IsModCompatible(GHRuleset.ERulesetType rulesetType, IMod mod)
	{
		if (mod.ModType != GHModMetaData.EModType.Global && (mod.ModType != GHModMetaData.EModType.Campaign || rulesetType != GHRuleset.ERulesetType.Campaign))
		{
			if (mod.ModType == GHModMetaData.EModType.Guildmaster)
			{
				return rulesetType == GHRuleset.ERulesetType.Guildmaster;
			}
			return false;
		}
		return true;
	}

	private void Validate(List<IMod> mods, Action<bool> onValidatedCallback)
	{
		CoroutineHelper.RunCoroutine(ValidateMods(mods, onValidatedCallback));
	}

	private IEnumerator ValidateMods(List<IMod> mods, Action<bool> onValidatedCallback)
	{
		foreach (IMod mod in mods)
		{
			yield return GHModding.ValidateMod(ModdingService.GetModFromIMod(mod), null, writeResultsToFile: true);
		}
		onValidatedCallback(mods.All((IMod it) => it.IsValid));
	}

	public void CompileRuleset(IRuleset ruleset, Action<bool> onCompiledCallback)
	{
		GHRuleset rulesetInstance = SceneController.Instance.Modding.Rulesets.FirstOrDefault((GHRuleset s) => s.Name == ruleset.Name);
		CoroutineHelper.RunCoroutine(GHModding.CompileRuleset(rulesetInstance, delegate
		{
			onCompiledCallback?.Invoke(rulesetInstance.IsCompiled);
		}));
	}

	public void DeleteRuleset(IRuleset ruleset)
	{
		try
		{
			GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.FirstOrDefault((GHRuleset s) => s.Name == ruleset.Name);
			if (gHRuleset != null)
			{
				PlatformLayer.FileSystem.RemoveDirectory(gHRuleset.RulesetFolder, recursive: true);
				SceneController.Instance.Modding.Rulesets.Remove(gHRuleset);
			}
			else
			{
				Debug.LogError("Unable to find ruleset with name " + ruleset.Name);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to delete the ruleset.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public List<IRuleset> GetRulesets()
	{
		return SceneController.Instance.Modding.Rulesets.Where((GHRuleset w) => !w.IsMPRuleset).Select((Func<GHRuleset, IRuleset>)((GHRuleset s) => s)).ToList();
	}

	public void UpdateRuleset(IRuleset ruleset, RulesetDataView newData, Action<IRuleset> onSuccessfullyUpdated, Action onFailed)
	{
		GHRuleset rulesetInstance = SceneController.Instance.Modding.Rulesets.FirstOrDefault((GHRuleset s) => s.Name == ruleset.Name);
		if (rulesetInstance != null)
		{
			Validate(newData.Mods, delegate(bool isValid)
			{
				if (isValid)
				{
					rulesetInstance.Update(newData);
					rulesetInstance.Save();
					onSuccessfullyUpdated(rulesetInstance);
				}
				else
				{
					onFailed();
				}
			});
		}
		else
		{
			Debug.LogError("Unable to find ruleset with name " + ruleset.Name);
			onFailed();
		}
	}
}
