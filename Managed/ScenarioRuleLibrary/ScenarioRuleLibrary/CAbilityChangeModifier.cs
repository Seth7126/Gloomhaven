using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityChangeModifier : CAbility
{
	public CAbilityChangeModifier()
	{
	}

	public CAbilityChangeModifier(CAbilityChangeModifier state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
