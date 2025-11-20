using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CConditionalOverride
{
	public string Name { get; private set; }

	public CAbilityFilterContainer Filter { get; private set; }

	public CAbilityRequirements Requirements { get; private set; }

	public int XP { get; private set; }

	public List<CAbilityOverride> AbilityOverrides { get; private set; }

	public bool Self { get; private set; }

	public CConditionalOverride(string name, CAbilityFilterContainer filter, CAbilityRequirements requirements, int xp, List<CAbilityOverride> abilityOverrides, bool self)
	{
		Name = name;
		Filter = filter;
		Requirements = requirements;
		XP = xp;
		AbilityOverrides = abilityOverrides;
		Self = self;
	}

	public CConditionalOverride Copy()
	{
		return new CConditionalOverride(Name, Filter.Copy(), Requirements.Copy(), XP, AbilityOverrides, Self);
	}

	public CConditionalOverride()
	{
	}

	public CConditionalOverride(CConditionalOverride state, ReferenceDictionary references)
	{
		Name = state.Name;
		Filter = references.Get(state.Filter);
		if (Filter == null && state.Filter != null)
		{
			Filter = new CAbilityFilterContainer(state.Filter, references);
			references.Add(state.Filter, Filter);
		}
		XP = state.XP;
		AbilityOverrides = references.Get(state.AbilityOverrides);
		if (AbilityOverrides == null && state.AbilityOverrides != null)
		{
			AbilityOverrides = new List<CAbilityOverride>();
			for (int i = 0; i < state.AbilityOverrides.Count; i++)
			{
				CAbilityOverride cAbilityOverride = state.AbilityOverrides[i];
				CAbilityOverride cAbilityOverride2 = references.Get(cAbilityOverride);
				if (cAbilityOverride2 == null && cAbilityOverride != null)
				{
					cAbilityOverride2 = new CAbilityOverride(cAbilityOverride, references);
					references.Add(cAbilityOverride, cAbilityOverride2);
				}
				AbilityOverrides.Add(cAbilityOverride2);
			}
			references.Add(state.AbilityOverrides, AbilityOverrides);
		}
		Self = state.Self;
	}
}
