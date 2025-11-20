using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath : CBespokeBehaviour
{
	private CAbilityAddActiveBonus AbilityAddActiveBonus;

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		AbilityAddActiveBonus = ability as CAbilityAddActiveBonus;
	}

	public override void OnDeath(CActor actor, CAbility causeOfDeathAbility)
	{
		CActor actorToTriggerOn = (m_ActiveBonusData.TriggerOnCaster ? m_ActiveBonus.Caster : actor);
		if (!m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(actor) || !m_ActiveBonus.RequirementsMet(actor))
		{
			return;
		}
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		CAbility cAbility = CAbility.CopyAbility(AbilityAddActiveBonus.AddAbility, generateNewID: false);
		if (m_ActiveBonusData.UseTriggerAbilityAsParent)
		{
			if (causeOfDeathAbility != null)
			{
				cAbility.ParentAbility = causeOfDeathAbility;
			}
			else
			{
				cAbility.TargetThisActorAutomatically = actor;
			}
		}
		if (cAbility.IsInlineSubAbility)
		{
			cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]);
		}
		List<CAbility> list = new List<CAbility> { cAbility };
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			if (m_Ability.Targeting == CAbility.EAbilityTargeting.All && m_Ability.NumberTargets == -1)
			{
				m_Ability.TargetingActor = m_Actor;
				foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x != actorToTriggerOn))
				{
					if (IsValidTarget(m_Ability, item))
					{
						if (item.IsDead)
						{
							GameState.OverrideCurrentActorForOneAction(item, null, killActorAfterAction: true);
							cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
						}
						else
						{
							cPhaseAction.StackNextAbilities(list, item);
						}
					}
				}
			}
			else if (actorToTriggerOn.IsDead)
			{
				GameState.OverrideCurrentActorForOneAction(actorToTriggerOn, null, killActorAfterAction: true);
				cPhaseAction.StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
			}
			else
			{
				cPhaseAction.StackNextAbilities(list, actorToTriggerOn);
			}
		}
		else if (m_Ability.Targeting == CAbility.EAbilityTargeting.All && m_Ability.NumberTargets == -1)
		{
			m_Ability.TargetingActor = m_Actor;
			foreach (CActor item2 in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x != actorToTriggerOn))
			{
				if (!IsValidTarget(m_Ability, item2) || item2.IsDead)
				{
					continue;
				}
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2)
				{
					cPhaseAction2.StackNextAbilities(list, item2);
					continue;
				}
				if (GameState.InternalCurrentActor != item2)
				{
					GameState.OverrideCurrentActorForOneAction(item2);
				}
				PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true);
			}
		}
		else
		{
			if (GameState.InternalCurrentActor != actorToTriggerOn)
			{
				GameState.OverrideCurrentActorForOneAction(actorToTriggerOn);
			}
			PhaseManager.StartAbilities(list, m_ActiveBonus.BaseCard, fullCopy: true);
		}
		m_ActiveBonus.RestrictActiveBonus(actorToTriggerOn);
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

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath()
	{
	}

	public CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath(CDuringActionAbilityActiveBonus_TriggerAbilityOnDeath state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
