using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnKill : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnKill(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnKill(CActor target, CActor actor, CActor.ECauseOfDeath causeOfDeath, CAbility causeOfDeathAbility, bool onActorTurn)
	{
		bool flag = target != null && actor != null && !m_ActiveBonus.Finishing() && !m_ActiveBonus.Finished() && m_ActiveBonusData.Filter.IsValidTarget(target, actor, m_Ability.IsTargetedAbility, useTargetOriginalType: false, m_Ability.MiscAbilityData?.CanTargetInvisible);
		CActor cActor = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : m_Actor);
		if (!(m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(cActor) && flag) || !m_ActiveBonus.RequirementsMet(cActor) || causeOfDeath == CActor.ECauseOfDeath.SummonActiveBonusCancelled)
		{
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonusData.UseTriggerAbilityAsParent && causeOfDeathAbility != null)
		{
			cAbility.ParentAbility = causeOfDeathAbility;
		}
		if (cAbility.IsInlineSubAbility)
		{
			cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
		}
		List<CAbility> list = new List<CAbility> { cAbility };
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.StackNextAbilities(list, cActor);
		}
		else
		{
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard);
		}
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

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnKill()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnKill(CDuringActionAbilityActiveBonus_TriggerAbilityOnKill state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
