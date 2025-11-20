using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus_AddConditionUntilDamaged : CBespokeBehaviour
{
	public CAddConditionActiveBonus_AddConditionUntilDamaged(CActor actor, CAbilityAddCondition ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
		{
			m_Actor.ApplyCondition(m_ActiveBonus.Caster, positiveCondition.Key, 1, EConditionDecTrigger.ConditionalCondition);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
		{
			m_Actor.ApplyCondition(m_ActiveBonus.Caster, negativeCondition.Key, 1, EConditionDecTrigger.ConditionalCondition);
		}
	}

	private void RemoveConditions()
	{
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
		{
			m_Actor.RemovePositiveConditionToken(positiveCondition.Key, EConditionDecTrigger.ConditionalCondition);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
		{
			m_Actor.RemoveNegativeConditionToken(negativeCondition.Key, EConditionDecTrigger.ConditionalCondition);
		}
	}

	public override void OnDamaged(CActor actor)
	{
		base.OnDamaged(actor);
		RemoveConditions();
		if (m_ActiveBonus.HasTracker)
		{
			foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
			{
				ElementInfusionBoardManager.Consume(item, m_Actor);
			}
			OnBehaviourTriggered();
			m_ActiveBonus.UpdateXPTracker();
			if (m_ActiveBonus.Remaining <= 0)
			{
				Finish();
			}
		}
		else
		{
			Finish();
		}
	}

	public override void OnFinished()
	{
		base.OnFinished();
		RemoveConditions();
	}

	public CAddConditionActiveBonus_AddConditionUntilDamaged()
	{
	}

	public CAddConditionActiveBonus_AddConditionUntilDamaged(CAddConditionActiveBonus_AddConditionUntilDamaged state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
