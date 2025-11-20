using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAttackActiveBonus : CActiveBonus
{
	public CAttackActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.BuffAttack:
			if (ability is CAbilityAttack ability3)
			{
				m_BespokeBehaviour = new CAttackActiveBonus_BuffAttack(actor, ability3, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus " + ability.ActiveBonusData.Behaviour);
		case EActiveBonusBehaviourType.BuffIncomingAttacks:
			if (ability is CAbilityAttack ability2)
			{
				m_BespokeBehaviour = new CAttackActiveBonus_BuffIncomingAttacks(actor, ability2, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus " + ability.ActiveBonusData.Behaviour);
		}
	}

	public CAttackActiveBonus()
	{
	}

	public CAttackActiveBonus(CAttackActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
