using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CShieldActiveBonus_TargetedShield : CBespokeBehaviour
{
	public CShieldActiveBonus_TargetedShield(CActor actor, CAbilityShield ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnBeingAttacked(CAbilityAttack ability, int modifiedStrength)
	{
		if (!IsValidTarget(ability, m_Actor) || m_ActiveBonusData.IsToggleBonus)
		{
			return;
		}
		m_ActiveBonus.RestrictActiveBonus(m_Actor);
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

	public override void OnPreventDamageTriggered(int damagePrevented, CActor damageSource, CActor damagedActor, CAbility damagingAbility)
	{
		base.OnPreventDamageTriggered(damagePrevented, damageSource, damagedActor, damagingAbility);
		if (!IsValidTarget(damagingAbility, damagedActor))
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
		if (m_ActiveBonusData.StrengthIsScalar)
		{
			return 0;
		}
		return m_Strength;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (!m_ActiveBonusData.StrengthIsScalar)
		{
			return 1;
		}
		return m_Strength;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		return m_ActiveBonusData.ProcXP;
	}

	public CShieldActiveBonus_TargetedShield()
	{
	}

	public CShieldActiveBonus_TargetedShield(CShieldActiveBonus_TargetedShield state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
