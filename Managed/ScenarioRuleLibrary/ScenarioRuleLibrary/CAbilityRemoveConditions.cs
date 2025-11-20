using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRemoveConditions : CAbilityTargeting
{
	public const int STRENGTH_ALL = -1;

	public Dictionary<CCondition.ENegativeCondition, CAbility> OriginalConditions;

	public CAbilityRemoveConditions()
		: base(EAbilityType.RemoveConditions)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (OriginalConditions != null)
		{
			return;
		}
		OriginalConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_NegativeConditions)
		{
			OriginalConditions.Add(negativeCondition.Key, negativeCondition.Value);
		}
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		if (base.Strength != -1)
		{
			FilterConditions(actorsAppliedTo[0]);
		}
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(base.AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			RemoveConditions(base.TargetingActor, actor, m_NegativeConditions.Keys.ToList(), m_PositiveConditions.Keys.ToList(), this, base.AnimOverload);
		}
		return true;
	}

	public static void RemoveConditions(CActor applyingActor, CActor actor, List<CCondition.ENegativeCondition> negativeConditions, List<CCondition.EPositiveCondition> positiveConditions, CAbility ability, string animOverload = "None")
	{
		if (negativeConditions != null && negativeConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition negativeCondition in negativeConditions)
			{
				if (negativeCondition == CCondition.ENegativeCondition.Curse)
				{
					actor.Class.RemoveCurses(actor);
				}
				if (actor.RemoveNegativeConditionToken(negativeCondition))
				{
					CActorIsRemovingCondition_MessageData message = new CActorIsRemovingCondition_MessageData(animOverload, applyingActor)
					{
						m_Ability = ability,
						m_ActorAppliedTo = actor,
						m_NegativeCondition = negativeCondition
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
		}
		if (positiveConditions == null || positiveConditions.Count <= 0)
		{
			return;
		}
		foreach (CCondition.EPositiveCondition positiveCondition in positiveConditions)
		{
			if (positiveCondition == CCondition.EPositiveCondition.Bless)
			{
				actor.Class.RemoveBlesses(actor);
			}
			actor.RemovePositiveConditionToken(positiveCondition);
		}
	}

	protected void FilterConditions(CActor actor)
	{
		m_NegativeConditions.Clear();
		bool flag = false;
		int num = 0;
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> originalCondition in OriginalConditions)
		{
			if (num < base.Strength)
			{
				if (actor.Tokens.HasKey(originalCondition.Key))
				{
					flag = true;
				}
				if (originalCondition.Key == CCondition.ENegativeCondition.Curse && actor.Class.GetCurseCards().Count > 0)
				{
					flag = true;
				}
				if (flag)
				{
					m_NegativeConditions.Add(originalCondition.Key, originalCondition.Value);
					num++;
				}
			}
		}
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityRemoveConditions(CAbilityRemoveConditions state, ReferenceDictionary references)
		: base(state, references)
	{
		OriginalConditions = references.Get(state.OriginalConditions);
		if (OriginalConditions != null || state.OriginalConditions == null)
		{
			return;
		}
		OriginalConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>(state.OriginalConditions.Comparer);
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> originalCondition in state.OriginalConditions)
		{
			CCondition.ENegativeCondition key = originalCondition.Key;
			CAbility cAbility = references.Get(originalCondition.Value);
			if (cAbility == null && originalCondition.Value != null)
			{
				cAbility = new CAbility(originalCondition.Value, references);
				references.Add(originalCondition.Value, cAbility);
			}
			OriginalConditions.Add(key, cAbility);
		}
		references.Add(state.OriginalConditions, OriginalConditions);
	}
}
