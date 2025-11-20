using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CRetaliateActiveBonus_BuffRetaliate : CBespokeBehaviour
{
	public CRetaliateActiveBonus_BuffRetaliate(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public CRetaliateActiveBonus_BuffRetaliate()
	{
	}

	public CRetaliateActiveBonus_BuffRetaliate(CRetaliateActiveBonus_BuffRetaliate state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
