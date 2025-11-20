using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CShieldActiveBonus_BuffShield : CBespokeBehaviour
{
	public CShieldActiveBonus_BuffShield(CActor actor, CAbilityShield ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (!IsValidTarget(ability, m_Actor) || m_ActiveBonusData.IsToggleBonus)
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(ability.TargetingActor);
		if (m_ActiveBonus.HasTracker && modifiedStrength > 0)
		{
			OnBehaviourTriggered();
			m_ActiveBonus.UpdateXPTracker();
			if (m_ActiveBonus.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	protected override bool IsValidTarget(CAbility ability, CActor target, bool useTargetOriginalType = false)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			bool flag = m_ActiveBonusData.Filter.IsValidTarget(cAbilityAttack.TargetingActor, target, cAbilityAttack.IsTargetedAbility, useTargetOriginalType: false, true, skipUntargetableCheck: true);
			if (target != null && cAbilityAttack != null && flag && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonus.IsValidAttackType(cAbilityAttack))
			{
				return true;
			}
			return false;
		}
		return m_ActiveBonusData.Filter.IsValidTarget(target, target, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true);
	}

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility)
	{
		base.OnPreventDamageTriggered(damagePrevented, damageSource, damagedActor, damagingAbility);
		if (!IsValidTarget(damagingAbility, damagedActor) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(damagedActor))
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(damagedActor);
		if (m_ActiveBonus.HasTracker)
		{
			OnBehaviourTriggered();
			m_ActiveBonus.UpdateXPTracker();
			if (m_ActiveBonus.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (!IsValidTarget(ability, m_Actor) || m_ActiveBonusData.StrengthIsScalar)
		{
			return 0;
		}
		return CheckStatIsBasedOnXType();
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (!IsValidTarget(ability, m_Actor) || !m_ActiveBonusData.StrengthIsScalar)
		{
			return 1;
		}
		return CheckStatIsBasedOnXType();
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		return m_ActiveBonusData.ProcXP;
	}

	public CShieldActiveBonus_BuffShield()
	{
	}

	public CShieldActiveBonus_BuffShield(CShieldActiveBonus_BuffShield state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
