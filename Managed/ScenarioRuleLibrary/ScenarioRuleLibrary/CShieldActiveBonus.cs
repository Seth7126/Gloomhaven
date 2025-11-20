using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CShieldActiveBonus : CActiveBonus
{
	public CShieldActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		switch (ability.ActiveBonusData.Behaviour)
		{
		case EActiveBonusBehaviourType.ShieldIncomingAttacks:
			if (ability is CAbilityShield ability4)
			{
				m_BespokeBehaviour = new CShieldActiveBonus_BuffShield(actor, ability4, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
		case EActiveBonusBehaviourType.ShieldAndRetaliate:
			if (ability is CAbilityShield ability3)
			{
				m_BespokeBehaviour = new CShieldActiveBonus_ShieldAndRetaliate(actor, ability3, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
		case EActiveBonusBehaviourType.ShieldTargetedAttacks:
			if (ability is CAbilityShield ability5)
			{
				m_BespokeBehaviour = new CShieldActiveBonus_TargetedShield(actor, ability5, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
		case EActiveBonusBehaviourType.ShieldAndDisadvantage:
			if (ability is CAbilityShield ability2)
			{
				m_BespokeBehaviour = new CShieldActiveBonus_ShieldAndDisadvantage(actor, ability2, this);
				break;
			}
			throw new Exception("Invalid ability type " + ability.AbilityType.ToString() + " for Shield active bonus");
		}
	}

	public override void TriggerPreventDamage(int damagePrevented, CActor damageSource, CActor actorDamaged, CAbility damagingAbility)
	{
		if (IsActiveBonusToggledAndNotRestricted(actorDamaged))
		{
			actorDamaged.m_OnPreventDamageListeners?.Invoke(damagePrevented, damageSource, actorDamaged, damagingAbility);
		}
	}

	public CShieldActiveBonus()
	{
	}

	public CShieldActiveBonus(CShieldActiveBonus state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
