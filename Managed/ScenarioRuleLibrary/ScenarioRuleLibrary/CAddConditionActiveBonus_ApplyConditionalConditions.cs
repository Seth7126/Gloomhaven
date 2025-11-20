using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAddConditionActiveBonus_ApplyConditionalConditions : CBespokeBehaviour
{
	private List<CActor> m_ActorsWithAddedConditions = new List<CActor>();

	public CAddConditionActiveBonus_ApplyConditionalConditions(CActor actor, CAbilityAddCondition ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public void CheckConditions()
	{
		List<CActor> list = (m_ActiveBonusData.IsAura ? m_ActiveBonus.ValidActorsInRangeOfAura : new List<CActor> { m_Actor });
		foreach (CActor item in list)
		{
			if (m_Ability.ActiveBonusData.AbilityData.StartAbilityRequirements.MeetsAbilityRequirements(item, m_Ability) && !m_ActorsWithAddedConditions.Contains(item))
			{
				foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
				{
					item.ApplyCondition(item, positiveCondition.Key, 1, EConditionDecTrigger.ConditionalCondition);
				}
				foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
				{
					item.ApplyCondition(item, negativeCondition.Key, 1, EConditionDecTrigger.ConditionalCondition);
				}
				m_ActorsWithAddedConditions.Add(item);
				m_ActiveBonus.RestrictActiveBonus(item);
				if (!m_ActiveBonus.HasTracker)
				{
					continue;
				}
				foreach (ElementInfusionBoardManager.EElement item2 in m_Ability.ActiveBonusData.Consuming)
				{
					ElementInfusionBoardManager.Consume(item2, item);
				}
				OnBehaviourTriggered();
				m_ActiveBonus.UpdateXPTracker();
				if (m_ActiveBonus.Remaining <= 0)
				{
					Finish();
				}
			}
			else
			{
				if (m_Ability.ActiveBonusData.AbilityData.StartAbilityRequirements.MeetsAbilityRequirements(item, m_Ability) || !m_ActorsWithAddedConditions.Contains(item))
				{
					continue;
				}
				foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition2 in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
				{
					item.RemovePositiveConditionToken(positiveCondition2.Key, EConditionDecTrigger.ConditionalCondition);
				}
				foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition2 in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
				{
					item.RemoveNegativeConditionToken(negativeCondition2.Key, EConditionDecTrigger.ConditionalCondition);
				}
				m_ActorsWithAddedConditions.Remove(item);
			}
		}
		foreach (CActor item3 in m_ActorsWithAddedConditions.Except(list).ToList())
		{
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition3 in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
			{
				item3.RemovePositiveConditionToken(positiveCondition3.Key, EConditionDecTrigger.ConditionalCondition);
			}
			foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition3 in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
			{
				item3.RemoveNegativeConditionToken(negativeCondition3.Key, EConditionDecTrigger.ConditionalCondition);
			}
			m_ActorsWithAddedConditions.Remove(item3);
		}
	}

	public override bool ActiveBonusIsActivatedByTile(CTile tile)
	{
		return m_Ability.ActiveBonusData.AbilityData.StartAbilityRequirements.MeetsAbilityRequirements(tile);
	}

	public override void OnActionEnded(CActor actorEndingAction)
	{
		CheckConditions();
	}

	public override void OnMoved(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes)
	{
		CheckConditions();
	}

	public override void OnMoveStart(CAbilityMove moveAbility)
	{
		CheckConditions();
	}

	public override void OnFinished()
	{
		base.OnFinished();
		foreach (CActor actorsWithAddedCondition in m_ActorsWithAddedConditions)
		{
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.ActiveBonusData.AbilityData.PositiveConditions)
			{
				actorsWithAddedCondition.RemovePositiveConditionToken(positiveCondition.Key, EConditionDecTrigger.ConditionalCondition);
			}
			foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.ActiveBonusData.AbilityData.NegativeConditions)
			{
				actorsWithAddedCondition.RemoveNegativeConditionToken(negativeCondition.Key, EConditionDecTrigger.ConditionalCondition);
			}
		}
		m_ActorsWithAddedConditions.Clear();
	}

	public CAddConditionActiveBonus_ApplyConditionalConditions()
	{
	}

	public CAddConditionActiveBonus_ApplyConditionalConditions(CAddConditionActiveBonus_ApplyConditionalConditions state, ReferenceDictionary references)
		: base(state, references)
	{
		m_ActorsWithAddedConditions = references.Get(state.m_ActorsWithAddedConditions);
		if (m_ActorsWithAddedConditions != null || state.m_ActorsWithAddedConditions == null)
		{
			return;
		}
		m_ActorsWithAddedConditions = new List<CActor>();
		for (int i = 0; i < state.m_ActorsWithAddedConditions.Count; i++)
		{
			CActor cActor = state.m_ActorsWithAddedConditions[i];
			CActor cActor2 = references.Get(cActor);
			if (cActor2 == null && cActor != null)
			{
				CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
				cActor2 = cActor3;
				references.Add(cActor, cActor2);
			}
			m_ActorsWithAddedConditions.Add(cActor2);
		}
		references.Add(state.m_ActorsWithAddedConditions, m_ActorsWithAddedConditions);
	}
}
