using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddHealActiveBonus_BuffHeal : CBespokeBehaviour
{
	public CAddHealActiveBonus_BuffHeal(CActor actor, CAbilityAddHeal ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnHealApplyToActor(CAbilityHeal healAbility)
	{
		m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
		if (m_Ability.MiscAbilityData?.ConditionsToRemoveFirst != null)
		{
			foreach (CActor item in healAbility.ActorsTargeted)
			{
				CAbilityRemoveConditions.RemoveConditions(healAbility.TargetingActor, item, m_Ability.MiscAbilityData.ConditionsToRemoveFirst, null, m_Ability);
			}
		}
		if (m_ActiveBonusData.Behaviour != CActiveBonus.EActiveBonusBehaviourType.BuffHealPerActor || !m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item2 in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item2, healAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		if (m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public override void OnHealApplyToAction(CAbilityHeal healAbility)
	{
		m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
		if (m_ActiveBonusData.Behaviour != CActiveBonus.EActiveBonusBehaviourType.BuffHealPerAction || !m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, healAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		if (!m_ActiveBonus.ApplyToAction)
		{
			m_ActiveBonus.ApplyToAction = true;
			m_ActiveBonus.UpdateXPTracker();
			if (m_ActiveBonus.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	public override void OnBeingHealed(CAbilityHeal healAbility)
	{
		m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
		if (m_ActiveBonusData.Behaviour != CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal || !m_ActiveBonus.HasTracker)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in m_Ability.ActiveBonusData.Consuming)
		{
			ElementInfusionBoardManager.Consume(item, healAbility.TargetingActor);
		}
		OnBehaviourTriggered();
		m_ActiveBonus.UpdateXPTracker();
		if (m_ActiveBonus.Remaining <= 0)
		{
			Finish();
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		int result = 0;
		bool flag = true;
		if (ability is CAbilityHeal ability2)
		{
			flag = IsValidTarget(ability2, target);
		}
		if (flag && !m_ActiveBonusData.StrengthIsScalar && m_ActiveBonus.HasTracker && (m_ActiveBonusData.Behaviour != CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal || (m_ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal && target == m_Actor)))
		{
			result = CheckStatIsBasedOnXType();
		}
		return result;
	}

	public CAddHealActiveBonus_BuffHeal()
	{
	}

	public CAddHealActiveBonus_BuffHeal(CAddHealActiveBonus_BuffHeal state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
