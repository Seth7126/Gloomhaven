using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityStopFlying : CAbilityCondition
{
	public CAbilityStopFlying()
		: base(EAbilityType.StopFlying, int.MaxValue, EConditionDecTrigger.Never)
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(EAbilityType.StopFlying, ESESubTypeAbility.ActorIsApplying, base.DecrementTrigger, base.Duration, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", (actorsAppliedTo.Count > 0) ? actorsAppliedTo[0].Type : CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
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
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCondition(EAbilityType.StopFlying, ESESubTypeAbility.ApplyToActor, base.DecrementTrigger, base.Duration, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, actor.Class.ID, actor.Type, actedOnIsSummon, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens));
			base.AbilityHasHappened = true;
			actor.ApplyCondition(base.TargetingActor, CCondition.ENegativeCondition.StopFlying);
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(actor);
			}
			CTile cTile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (CAbilityFilter.IsValidTile(cTile, CAbilityFilter.EFilterTile.Obstacle))
			{
				CTile propTile = null;
				List<CTile> list = new List<CTile>();
				for (int num = 0; num < 5; num++)
				{
					List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(cTile, num + 1);
					List<CTile> list2 = new List<CTile>();
					foreach (CTile item in allUnblockedTilesFromOrigin)
					{
						if (!ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Blocked && ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable && CAbilityFilter.IsValidTile(item, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: true) && CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile) == null)
						{
							list2.Add(item);
						}
					}
					if (list2.Count > 0)
					{
						list.AddRange(list2);
						break;
					}
				}
				if (list.Count > 0)
				{
					CTile cTile2 = list[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list.Count)];
					actor.ArrayIndex = cTile2.m_ArrayIndex;
					CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(base.TargetingActor);
					cActorHasPushed_MessageData.m_PushAbility = null;
					cActorHasPushed_MessageData.m_ActorBeingPushed = actor;
					ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
					int trapDamage = ScenarioManager.Scenario.SLTE.TrapDamage;
					int health = actor.Health;
					bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					GameState.ActorBeenDamaged(actor, trapDamage, checkIfPlayerCanAvoidDamage: true, base.TargetingActor, null, base.AbilityType, 0, isTrapDamage: true);
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
			}
			else
			{
				for (int num2 = cTile.m_Props.Count - 1; num2 >= 0; num2--)
				{
					CObjectProp cObjectProp = cTile.m_Props[num2];
					if (cObjectProp.WillActivationDamageActor(actor) && (!actor.IgnoreHazardousTerrain || (cObjectProp.ObjectType != ScenarioManager.ObjectImportType.TerrainHotCoals && cObjectProp.ObjectType != ScenarioManager.ObjectImportType.TerrainThorns)))
					{
						cObjectProp.AutomaticActivate(actor);
					}
				}
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public CAbilityStopFlying(CAbilityStopFlying state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
