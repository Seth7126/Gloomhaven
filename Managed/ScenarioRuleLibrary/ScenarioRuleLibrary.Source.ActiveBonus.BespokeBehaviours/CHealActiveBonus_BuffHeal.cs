using StateCodeGenerator;

namespace ScenarioRuleLibrary.Source.ActiveBonus.BespokeBehaviours;

internal class CHealActiveBonus_BuffHeal : CBespokeBehaviour
{
	public CHealActiveBonus_BuffHeal(CActor actor, CAbilityMove ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public CHealActiveBonus_BuffHeal()
	{
	}

	public CHealActiveBonus_BuffHeal(CHealActiveBonus_BuffHeal state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
