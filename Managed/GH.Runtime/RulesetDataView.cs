using System.Collections.Generic;
using System.Linq;
using Assets.Script.GUI.MainMenu.Modding;

public class RulesetDataView
{
	private string originalName;

	public List<IMod> originalMods;

	public string Name { get; set; }

	public List<IMod> Mods { get; set; }

	public GHRuleset.ERulesetType RulesetType { get; set; }

	public HashSet<IMod> UnvalidMods { get; set; } = new HashSet<IMod>();

	public bool IsModified
	{
		get
		{
			if (!(Name != originalName) && !Mods.Except(originalMods).Any())
			{
				return originalMods.Except(Mods).Any();
			}
			return true;
		}
	}

	public RulesetDataView(string name, GHRuleset.ERulesetType rulesetType, List<IMod> mods)
	{
		Name = (originalName = name);
		RulesetType = rulesetType;
		Mods = mods;
		originalMods = mods.ToList();
	}

	public RulesetDataView(GHRuleset.ERulesetType rulebaseType)
		: this(string.Empty, rulebaseType, new List<IMod>())
	{
	}

	public RulesetDataView(IRuleset ruleset)
		: this(ruleset.Name, ruleset.RulesetType, ruleset.GetMods().ToList())
	{
	}

	public bool IsValid()
	{
		if (Name.Length == 0 || Mods.Count == 0 || RulesetType == GHRuleset.ERulesetType.None || (UnvalidMods.Count > 0 && Mods.Exists((IMod it) => UnvalidMods.Contains(it))))
		{
			return false;
		}
		return true;
	}

	public void SetUnvalidMods(List<IMod> mods)
	{
		UnvalidMods.Clear();
		UnvalidMods.UnionWith(mods);
	}
}
