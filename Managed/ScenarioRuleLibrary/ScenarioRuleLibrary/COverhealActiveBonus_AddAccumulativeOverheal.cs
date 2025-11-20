using System;
using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class COverhealActiveBonus_AddAccumulativeOverheal : CBespokeBehaviour
{
	public int CurrentAddedOverheal;

	public COverhealActiveBonus_AddAccumulativeOverheal(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		UpdateCurrentOverhealStrength();
	}

	public void UpdateCurrentOverhealStrength()
	{
		CalculateBuffs(m_Ability, m_Actor, out var strength);
		CurrentAddedOverheal = strength;
		GetAccumulativeOverhealAndUpdate();
	}

	public void GetAccumulativeOverhealAndUpdate(bool excludeSelf = false)
	{
		int num = 0;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(m_Actor, m_Ability.AbilityType, m_Ability.ActiveBonusData.Behaviour);
		list.RemoveAll((CActiveBonus b) => b.Finishing() || b.Finished() || (excludeSelf && b == m_ActiveBonus));
		List<COverhealActiveBonus_AddAccumulativeOverheal> list2 = list.Select((CActiveBonus a) => a.BespokeBehaviour as COverhealActiveBonus_AddAccumulativeOverheal).ToList();
		if (!excludeSelf && !list2.Contains(this))
		{
			list2.Add(this);
		}
		num = list2.Sum((COverhealActiveBonus_AddAccumulativeOverheal b) => b.CurrentAddedOverheal);
		if (m_Actor.AccumulativeOverheal != num)
		{
			bool num2 = m_ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.AddAccumulativeOverhealAndHealIfMax && m_Actor.AccumulativeOverheal < num && m_Actor.Health == m_Actor.MaxHealth;
			m_Actor.AccumulativeOverheal = num;
			if (num2)
			{
				m_Actor.Health = m_Actor.MaxHealth;
			}
			else
			{
				m_Actor.Health = Math.Min(m_Actor.Health, m_Actor.MaxHealth);
			}
			CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(m_Actor, m_Actor, m_Actor.Health);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Overheal && !m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength);
			return strength;
		}
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Overheal && m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength);
			return strength;
		}
		return 1;
	}

	public override int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Overheal && m_ActiveBonusData.AbilityStrengthIsScalar && !m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength);
			return strength;
		}
		return 1;
	}

	private bool CalculateBuffs(CAbility ability, CActor target, out int strength)
	{
		strength = CheckStatIsBasedOnXType();
		return strength != 0;
	}

	public COverhealActiveBonus_AddAccumulativeOverheal()
	{
	}

	public COverhealActiveBonus_AddAccumulativeOverheal(COverhealActiveBonus_AddAccumulativeOverheal state, ReferenceDictionary references)
		: base(state, references)
	{
		CurrentAddedOverheal = state.CurrentAddedOverheal;
	}
}
