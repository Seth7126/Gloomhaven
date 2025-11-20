using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnAttacking(CAbilityAttack attackAbility, CActor target)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : attackAbility.TargetingActor);
		if (!IsValidTarget(m_Ability, target) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonusData.UseTriggerAbilityAsParent)
		{
			cAbility.TargetThisActorAutomatically = target;
			cAbility.ActorsToIgnore.Add(target);
		}
		List<CAbility> nextAbilities = new List<CAbility> { cAbility };
		(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(nextAbilities, cActor, killAfter: false, stackToNextCurrent: true);
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

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking()
	{
	}

	public CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking(CDuringActionAbilityActiveBonus_TrigerAbilityOnAttacking state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
