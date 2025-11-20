using System;
using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CImmunityActiveBonus : CActiveBonus
{
	public CImmunityActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
	}

	public void RemoveConditionsFromAffectedActors()
	{
		List<CActor> list = new List<CActor>();
		list.AddRange(base.ValidActorsInRangeOfAura);
		if (!list.Contains(base.Actor) && (!base.IsAura || (base.AuraFilter != null && base.AuraFilter.IsValidTarget(base.Actor, base.Caster, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			list.Add(base.Actor);
		}
		foreach (CActor item in list)
		{
			List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>(base.Ability.NegativeConditions.Keys);
			List<CCondition.EPositiveCondition> list3 = new List<CCondition.EPositiveCondition>(base.Ability.PositiveConditions.Keys);
			if (base.Ability is CAbilityImmunityTo cAbilityImmunityTo)
			{
				foreach (CAbility.EAbilityType immuneToAbilityType in cAbilityImmunityTo.ImmuneToAbilityTypes)
				{
					if (Enum.TryParse<CCondition.ENegativeCondition>(immuneToAbilityType.ToString(), ignoreCase: false, out var result) && !list2.Contains(result))
					{
						list2.Add(result);
					}
					if (Enum.TryParse<CCondition.EPositiveCondition>(immuneToAbilityType.ToString(), ignoreCase: false, out var result2) && !list3.Contains(result2))
					{
						list3.Add(result2);
					}
				}
			}
			foreach (CCondition.ENegativeCondition item2 in list2)
			{
				if (item2 != CCondition.ENegativeCondition.Curse && item.RemoveNegativeConditionToken(item2))
				{
					CActorIsRemovingCondition_MessageData message = new CActorIsRemovingCondition_MessageData("", item)
					{
						m_Ability = base.Ability,
						m_ActorAppliedTo = item,
						m_NegativeCondition = item2
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			foreach (CCondition.EPositiveCondition item3 in list3)
			{
				if (item3 != CCondition.EPositiveCondition.Bless)
				{
					item.RemovePositiveConditionToken(item3);
				}
			}
		}
	}

	public CImmunityActiveBonus()
	{
	}

	public CImmunityActiveBonus(CImmunityActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
