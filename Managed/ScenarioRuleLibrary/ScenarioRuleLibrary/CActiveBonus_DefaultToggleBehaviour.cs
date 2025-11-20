using StateCodeGenerator;

namespace ScenarioRuleLibrary;

internal class CActiveBonus_DefaultToggleBehaviour : CBespokeBehaviour
{
	public CActiveBonus_DefaultToggleBehaviour(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		if (attackAbility != null && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnConditionApplyToActor(CAbility conditionAbility, CActor target)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(conditionAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(conditionAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnHealApplyToAction(CAbilityHeal healAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(healAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnHealApplyToActor(CAbilityHeal healAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(healAbility.TargetingActor))
		{
			m_ActiveBonus.RestrictActiveBonus(healAbility.TargetingActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnPreventDamageTriggered(int preventedDamage, CActor damageSource, CActor damagedActor, CAbility damagingAbility)
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(damagedActor))
		{
			m_ActiveBonus.RestrictActiveBonus(damagedActor);
			OnBehaviourTriggered();
		}
	}

	public override void OnRetaliate()
	{
		if (m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(m_Actor))
		{
			m_ActiveBonus.RestrictActiveBonus(m_Actor);
			OnBehaviourTriggered();
		}
	}

	public CActiveBonus_DefaultToggleBehaviour()
	{
	}

	public CActiveBonus_DefaultToggleBehaviour(CActiveBonus_DefaultToggleBehaviour state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
