using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddRangeActiveBonus : CActiveBonus
{
	public CAddRangeActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? iD, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, iD, remaining)
	{
		if (ability.ActiveBonusData.Behaviour == EActiveBonusBehaviourType.BuffRange)
		{
			if (!(ability is CAbilityAddRange ability2))
			{
				throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Range active bonus");
			}
			m_BespokeBehaviour = new CAddRangeActiveBonus_BuffRange(actor, ability2, this);
		}
	}

	public CAddRangeActiveBonus()
	{
	}

	public CAddRangeActiveBonus(CAddRangeActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
