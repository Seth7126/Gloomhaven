using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CPreventDamageActiveBonus_BA_085 : CBespokeBehaviour
{
	public CPreventDamageActiveBonus_BA_085(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public CPreventDamageActiveBonus_BA_085()
	{
	}

	public CPreventDamageActiveBonus_BA_085(CPreventDamageActiveBonus_BA_085 state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
