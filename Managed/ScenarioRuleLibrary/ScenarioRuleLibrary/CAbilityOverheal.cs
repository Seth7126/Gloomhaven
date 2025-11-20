using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityOverheal : CAbilityCondition
{
	public int OverhealAmount = 35;

	public CAbilityOverheal(int overhealAmount, int duration, EConditionDecTrigger decTrigger)
		: base(EAbilityType.Overheal, duration, decTrigger)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		bool isSummon = false;
		if (actorApplying.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actorApplying.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(EAbilityType.Overheal, ESESubTypeAbility.ActorIsApplying, base.DecrementTrigger, base.Duration, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", (actorsAppliedTo.Count > 0) ? actorsAppliedTo[0].Type : CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
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
			bool isSummon = false;
			CActor targetingActor = base.TargetingActor;
			if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid);
				if (cEnemyActor != null)
				{
					isSummon = cEnemyActor.IsSummon;
				}
			}
			bool actedOnIsSummon = false;
			if (actor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
				if (cEnemyActor2 != null)
				{
					actedOnIsSummon = cEnemyActor2.IsSummon;
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(EAbilityType.Overheal, ESESubTypeAbility.ApplyToActor, base.DecrementTrigger, base.Duration, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, actor.Class.ID, actor.Type, actedOnIsSummon, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens));
			base.AbilityHasHappened = true;
			base.TargetingActor.FindCardWithAbility(this).AddActiveBonus(this, actor, base.TargetingActor);
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityOverheal()
	{
	}

	public CAbilityOverheal(CAbilityOverheal state, ReferenceDictionary references)
		: base(state, references)
	{
		OverhealAmount = state.OverhealAmount;
	}
}
