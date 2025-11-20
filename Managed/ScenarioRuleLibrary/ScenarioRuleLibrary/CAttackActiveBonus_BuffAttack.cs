using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAttackActiveBonus_BuffAttack : CBespokeBehaviour
{
	private bool m_TriggeredOnAttack;

	private bool isBuffed;

	public CAttackActiveBonus_BuffAttack(CActor actor, CAbilityAttack ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		m_TriggeredOnAttack = false;
	}

	public override void OnActiveBonusToggled(CAbility currentAbility, bool toggledOn)
	{
		if (currentAbility is CAbilityAttack cAbilityAttack)
		{
			if (cAbilityAttack.AttackSummary != null)
			{
				cAbilityAttack.AttackSummary.UpdateTargetData(cAbilityAttack, cAbilityAttack.ActiveSingleTargetItems, cAbilityAttack.ActiveSingleTargetActiveBonuses, cAbilityAttack.AreaEffectLocked);
			}
			CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(m_Actor);
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = m_Actor;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = cAbilityAttack;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = cAbilityAttack.AttackSummary?.Copy();
			ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
		}
	}

	public override void OnAttackStart(CAbilityAttack attackAbility)
	{
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		if (!IsValidTarget(attackAbility, target) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(attackAbility.TargetingActor))
		{
			return;
		}
		bool flag = isBuffed;
		isBuffed |= CalculateBuffs(attackAbility, target, out var _, out var _, out var _);
		isBuffed |= m_ActiveBonusData.StrengthIsScalar;
		isBuffed |= m_ActiveBonusData.AbilityStrengthIsScalar;
		isBuffed |= ReferenceAdvantage(attackAbility, target);
		if (m_Ability.NegativeConditions != null && m_Ability.NegativeConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition negCondition in m_Ability.NegativeConditions.Keys)
			{
				if (!attackAbility.NegativeConditions.ContainsKey(negCondition) && negCondition != CCondition.ENegativeCondition.NA)
				{
					CAbility.EAbilityType abilityType = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == negCondition.ToString());
					attackAbility.NegativeConditions.Add(negCondition, CAbility.CreateAbility(abilityType, attackAbility.AbilityFilter, isMonster: false, attackAbility.IsTargetedAbility));
					isBuffed = true;
				}
			}
		}
		if (m_Ability.PositiveConditions != null && m_Ability.PositiveConditions.Count > 0)
		{
			foreach (CCondition.EPositiveCondition posCondition in m_Ability.PositiveConditions.Keys)
			{
				if (!attackAbility.PositiveConditions.ContainsKey(posCondition) && posCondition != CCondition.EPositiveCondition.NA)
				{
					CAbility.EAbilityType abilityType2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == posCondition.ToString());
					attackAbility.PositiveConditions.Add(posCondition, CAbility.CreateAbility(abilityType2, attackAbility.AbilityFilter, isMonster: false, attackAbility.IsTargetedAbility));
					isBuffed = true;
				}
			}
		}
		if (isBuffed)
		{
			m_ActiveBonus.RestrictActiveBonus(attackAbility.TargetingActor);
			m_TriggeredOnAttack = true;
			if (m_ActiveBonus.HasTracker && (!AppliedThisAction || !m_Ability.ActiveBonusData.EntireAction))
			{
				if (m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
				{
					foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
					{
						attackEffect.ResetStackedSubAbilityForBuffOnAttack();
					}
				}
				m_ActiveBonus.UpdateXPTracker();
				if (m_ActiveBonus.Remaining <= 0)
				{
					Finish();
				}
			}
			AppliedThisAction = true;
		}
		isBuffed = flag;
	}

	public override void OnAttackAbilityFinished(CAbilityAttack attackAbility)
	{
		if (m_TriggeredOnAttack || (m_ActiveBonusData.IsToggleBonus && m_ActiveBonus.ToggledBonus))
		{
			OnBehaviourTriggered();
			m_TriggeredOnAttack = false;
		}
	}

	public override int ReferenceStrength(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack && !m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _);
			return strength;
		}
		return 0;
	}

	public override int ReferenceStrengthScalar(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack && m_ActiveBonusData.StrengthIsScalar && !m_ActiveBonusData.AbilityStrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceAbilityStrengthScalar(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack && m_ActiveBonusData.AbilityStrengthIsScalar && !m_ActiveBonusData.StrengthIsScalar && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var strength, out var _, out var _);
			return strength;
		}
		return 1;
	}

	public override int ReferenceXP(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var _, out var xp, out var _);
			return xp;
		}
		return 0;
	}

	public int ReferencePierce(CAbility ability, CActor target)
	{
		if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target))
		{
			CalculateBuffs(ability, target, out var _, out var _, out var pierce);
			return pierce;
		}
		return 0;
	}

	public bool ReferenceAdvantage(CAbility ability, CActor target)
	{
		AbilityData.MiscAbilityData miscAbilityData = m_Ability.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.AttackHasAdvantage == true)
		{
			if (ability.AbilityType == CAbility.EAbilityType.Attack && IsValidTarget(ability, target) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(ability.TargetingActor))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool CalculateBuffs(CAbility ability, CActor target, out int strength, out int xp, out int pierce)
	{
		CAbilityAttack attackAbility = ability as CAbilityAttack;
		strength = CheckStatIsBasedOnXType(ability);
		xp = m_XP;
		pierce = 0;
		if (target != null && m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
		{
			foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
			{
				attackEffect.GetBonus(target, attackAbility, out var attackBuff, out var xpBuff);
				strength += attackBuff;
				xp += xpBuff;
				pierce += attackEffect.Pierce;
			}
		}
		if (strength == 0 && xp == 0)
		{
			return pierce != 0;
		}
		return true;
	}

	public override bool StackActiveBonusInlineAbility(CActor target, CAbility ability)
	{
		CAbilityAttack attackAbility = ability as CAbilityAttack;
		List<CAbility> list = new List<CAbility>();
		if (m_ActiveBonusData.AbilityData != null)
		{
			list.Add(m_ActiveBonusData.AbilityData);
		}
		if (list.Count > 0)
		{
			foreach (CAbility item in list)
			{
				item.StackedAttackEffectAbility = true;
			}
		}
		if (m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack && cAbilityAttack.AttackEffects.Count > 0)
		{
			bool result = false;
			{
				foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
				{
					if (attackEffect.StackAbilityForBonus(target, attackAbility, list))
					{
						result = true;
					}
				}
				return result;
			}
		}
		return false;
	}

	public override void UpdateActiveBonusInlineAbilityTarget(CActor target)
	{
		if (!(m_Ability is CAbilityAttack { AttackEffects: not null } cAbilityAttack) || cAbilityAttack.AttackEffects.Count <= 0)
		{
			return;
		}
		foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
		{
			attackEffect.UpdateStackedAbilityForBonusTarget(target);
		}
	}

	public CAttackActiveBonus_BuffAttack()
	{
	}

	public CAttackActiveBonus_BuffAttack(CAttackActiveBonus_BuffAttack state, ReferenceDictionary references)
		: base(state, references)
	{
		m_TriggeredOnAttack = state.m_TriggeredOnAttack;
		isBuffed = state.isBuffed;
	}
}
