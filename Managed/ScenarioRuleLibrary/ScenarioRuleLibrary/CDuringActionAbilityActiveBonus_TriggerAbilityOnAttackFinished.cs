using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAttackFinished(CAbilityAttack attackAbility, CActor target, int damageDealt)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : attackAbility.TargetingActor);
		if (!IsValidTarget(attackAbility, target) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !RequiredDamageDone(damageDealt))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonusData.UseTriggerAbilityAsParent && attackAbility != null)
		{
			cAbility.ParentAbility = attackAbility;
		}
		List<CAbility> nextAbilities = new List<CAbility> { cAbility };
		(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, attackAbility.TargetingActor, killAfter: false, stackToNextCurrent: true);
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		m_ActiveBonus.RestrictActiveBonus(cActor);
		OnBehaviourTriggered();
		if (m_ActiveBonus.HasTracker)
		{
			m_ActiveBonus.UpdateXPTracker();
			if (m_ActiveBonus.Remaining <= 0)
			{
				Finish();
			}
		}
	}

	public bool RequiredDamageDone(int damageDealt)
	{
		if (m_ActiveBonus.NeedsDamage && damageDealt < 1)
		{
			return false;
		}
		return true;
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished(CDuringActionAbilityActiveBonus_TriggerAbilityOnAttackFinished state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
