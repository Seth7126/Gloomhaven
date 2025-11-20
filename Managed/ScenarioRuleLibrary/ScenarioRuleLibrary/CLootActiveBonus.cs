using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CLootActiveBonus : CActiveBonus
{
	public CLootActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		if (ability is CAbilityLoot ability2)
		{
			switch (ability.ActiveBonusData.Behaviour)
			{
			case EActiveBonusBehaviourType.BuffLoot:
				m_BespokeBehaviour = new CLootActiveBonus_BuffLoot(actor, ability2, this);
				break;
			case EActiveBonusBehaviourType.LimitLoot:
				m_BespokeBehaviour = new CLootActiveBonus_LimitLoot(actor, ability2, this);
				break;
			}
			return;
		}
		throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus LootBuff");
	}

	public CLootActiveBonus()
	{
	}

	public CLootActiveBonus(CLootActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
