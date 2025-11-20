using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnBeingAttackedPostDamage(CAbilityAttack attackAbility)
	{
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!IsValidAbilityType(attackAbility) || !m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) || !m_ActiveBonus.RequirementsMet(cActor))
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (cAbility.IsInlineSubAbility)
		{
			cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[attackAbility.TargetingActor.ArrayIndex.X, attackAbility.TargetingActor.ArrayIndex.Y]);
		}
		List<CAbility> list = new List<CAbility> { cAbility };
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.StackNextAbilities(list, cActor, killAfter: false, stackToNextCurrent: true);
		}
		else
		{
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard);
		}
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

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked(CDuringActionAbilityActiveBonus_TriggerAbilityOnAttacked state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
