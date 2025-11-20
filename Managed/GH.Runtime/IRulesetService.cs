using System;
using System.Collections.Generic;
using Assets.Script.GUI.MainMenu.Modding;

public interface IRulesetService
{
	List<IRuleset> GetRulesets();

	void UpdateRuleset(IRuleset ruleset, RulesetDataView newData, Action<IRuleset> onSuccessfullyUpdated, Action onFailed);

	void DeleteRuleset(IRuleset ruleset);

	void CreateRuleset(RulesetDataView rulesetData, Action<IRuleset> onSuccessfullyCreated, Action onFailed);

	List<IMod> GetCompatibleMods(GHRuleset.ERulesetType type);

	void CompileRuleset(IRuleset ruleset, Action<bool> onCompiledCallback);
}
