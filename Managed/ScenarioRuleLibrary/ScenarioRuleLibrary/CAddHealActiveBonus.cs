using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddHealActiveBonus : CActiveBonus
{
	public CAddHealActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.BuffHealPerActor:
		case EActiveBonusBehaviourType.BuffHealPerAction:
		case EActiveBonusBehaviourType.BuffIncomingHeal:
			if (ability is CAbilityAddHeal ability2)
			{
				m_BespokeBehaviour = new CAddHealActiveBonus_BuffHeal(actor, ability2, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Add Heal active bonus");
		case EActiveBonusBehaviourType.HealOnHealingReceived:
			m_BespokeBehaviour = new CHealActiveBonus_HealOnHealReceived(actor, ability, this);
			break;
		case EActiveBonusBehaviourType.HealOnDamageReceived:
			m_BespokeBehaviour = new CHealActiveBonus_HealOnDamageReceived(actor, ability, this);
			break;
		}
	}

	public CAddHealActiveBonus()
	{
	}

	public CAddHealActiveBonus(CAddHealActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
