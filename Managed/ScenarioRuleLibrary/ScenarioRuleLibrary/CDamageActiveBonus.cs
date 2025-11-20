using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDamageActiveBonus : CActiveBonus
{
	public CDamageActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.DamageOnCurseModApplied:
			if (ability is CAbilityDamage ability2)
			{
				m_BespokeBehaviour = new CDamageActiveBonus_DamageOnCurseModApplied(actor, ability2, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Active Bonus DamageOnCurseModApplied");
		case EActiveBonusBehaviourType.DamageOnReceivedDamage:
			m_BespokeBehaviour = new CDamageActiveBonus_DamageOnReceivedDamage(caster, ability, this);
			break;
		}
	}

	public CDamageActiveBonus()
	{
	}

	public CDamageActiveBonus(CDamageActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
