using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityStun : CAbilityCondition
{
	public CAbilityStun(int duration, EConditionDecTrigger decTrigger)
		: base(EAbilityType.Stun, duration, decTrigger)
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(EAbilityType.Stun, ESESubTypeAbility.ActorIsApplying, base.DecrementTrigger, base.Duration, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", (actorsAppliedTo.Count > 0) ? actorsAppliedTo[0].Type : CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
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
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(EAbilityType.Stun, ESESubTypeAbility.ApplyToActor, base.DecrementTrigger, base.Duration, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, actor.Class.ID, actor.Type, actedOnIsSummon, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens));
			base.AbilityHasHappened = true;
			AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.AlreadyHasConditionDamageInstead.HasValue && actor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				int health = actor.Health;
				bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(actor, base.MiscAbilityData.AlreadyHasConditionDamageInstead.Value, checkIfPlayerCanAvoidDamage: true, base.TargetingActor, this, EAbilityType.Damage);
				if ((actor.Type != CActor.EType.Player || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					CActorBeenDamaged_MessageData message = new CActorBeenDamaged_MessageData(actor)
					{
						m_ActorBeingDamaged = actor,
						m_DamageAbility = null,
						m_ActorOriginalHealth = health,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else
			{
				actor.ApplyCondition(base.TargetingActor, CCondition.ENegativeCondition.Stun, base.Duration, base.DecrementTrigger);
				if (m_NegativeConditions.Count > 0)
				{
					ProcessNegativeStatusEffects(actor);
				}
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public CAbilityStun()
	{
	}

	public CAbilityStun(CAbilityStun state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
