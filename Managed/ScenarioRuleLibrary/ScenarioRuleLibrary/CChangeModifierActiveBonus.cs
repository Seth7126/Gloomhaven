using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CChangeModifierActiveBonus : CActiveBonus
{
	public CChangeModifierActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		if (ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.AdjustWhenAttacked)
		{
			if (!(ability is CAbilityChangeModifier ability2))
			{
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus Adjust Attack Modifier");
			}
			m_BespokeBehaviour = new CChangeModifierActiveBonus_AdjustWhenAttacked(actor, baseCard, ability2, this);
		}
	}

	public CChangeModifierActiveBonus()
	{
	}

	public CChangeModifierActiveBonus(CChangeModifierActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
