using System.Collections.Generic;
using Assets.Script.GUI.MainMenu.Modding;

public interface IRuleset
{
	string Name { get; }

	GHRuleset.ERulesetType RulesetType { get; }

	bool IsValid { get; }

	bool IsCompiled { get; }

	List<IMod> GetMods();
}
