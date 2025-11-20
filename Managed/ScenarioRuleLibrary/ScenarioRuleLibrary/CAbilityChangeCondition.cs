using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityChangeCondition : CAbility
{
	public CAbilityChangeCondition()
	{
	}

	public CAbilityChangeCondition(CAbilityChangeCondition state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
