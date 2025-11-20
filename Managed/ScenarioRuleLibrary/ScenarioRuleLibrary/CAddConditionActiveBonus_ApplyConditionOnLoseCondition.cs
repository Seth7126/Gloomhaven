using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus_ApplyConditionOnLoseCondition : CBespokeBehaviour
{
	public CAddConditionActiveBonus_ApplyConditionOnLoseCondition(CActor actor, CAbilityAddCondition ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnNegativeConditionEndedOnActor(CCondition.ENegativeCondition negativeConditionType, CActor target)
	{
		if (!IsValidNegativeConditionType(negativeConditionType))
		{
			return;
		}
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> item in m_Ability.ActiveBonusData.AbilityData?.PositiveConditions)
		{
			target.ApplyCondition(m_Actor, item.Key, 1, EConditionDecTrigger.Turns);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> item2 in m_Ability.ActiveBonusData.AbilityData?.NegativeConditions)
		{
			target.ApplyCondition(m_Actor, item2.Key, 1, EConditionDecTrigger.Turns);
		}
		m_ActiveBonus.RestrictActiveBonus(target);
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item3 in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item3, m_Actor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		if (m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public override void OnPositiveConditionEndedOnActor(CCondition.EPositiveCondition positiveConditionType, CActor target)
	{
		if (!IsValidPositiveConditionType(positiveConditionType))
		{
			return;
		}
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> item in m_Ability.ActiveBonusData.AbilityData?.PositiveConditions)
		{
			target.ApplyCondition(m_Actor, item.Key, 1, EConditionDecTrigger.Turns);
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> item2 in m_Ability.ActiveBonusData.AbilityData?.NegativeConditions)
		{
			target.ApplyCondition(m_Actor, item2.Key, 1, EConditionDecTrigger.Turns);
		}
		m_ActiveBonus.RestrictActiveBonus(target);
		if (!m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item3 in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item3, m_Actor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		if (m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public CAddConditionActiveBonus_ApplyConditionOnLoseCondition()
	{
	}

	public CAddConditionActiveBonus_ApplyConditionOnLoseCondition(CAddConditionActiveBonus_ApplyConditionOnLoseCondition state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
