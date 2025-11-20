using System.Collections.Generic;
using ScenarioRuleLibrary;

public class EnhancementLine
{
	public EEnhancementLine Line;

	public CAbility Ability;

	public List<EnhancementButtonBase> EnhancementSlots;

	public EnhancementLine(EEnhancementLine line, CAbility ability, List<EnhancementButtonBase> enhancements)
	{
		Line = line;
		EnhancementSlots = enhancements;
		Ability = ability;
	}
}
