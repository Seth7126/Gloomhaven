using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityKill : CAbilityTargeting
{
	public CAbilityKill()
		: base(EAbilityType.Kill)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (base.AreaEffect != null)
		{
			base.TilesInRange = GameState.GetTilesInRange(targetActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true);
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(base.TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], base.AreaEffect, 0f, GameState.GetTilesInRange(targetActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true), getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
		}
		if (!base.UseSubAbilityTargeting)
		{
			return;
		}
		for (int num = base.ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor = base.ValidActorsInRange[num];
			if (cActor != null && cActor.IsDead)
			{
				base.ValidActorsInRange.Remove(cActor);
			}
		}
	}

	public override bool Perform()
	{
		CRefreshWorldSpaceStarHexDisplay_MessageData message = new CRefreshWorldSpaceStarHexDisplay_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		return base.Perform();
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsKilling_MessageData message = new CActorIsKilling_MessageData(base.AnimOverload, actorApplying)
		{
			m_ActorsAppliedTo = actorsAppliedTo,
			m_KillAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (ScenarioManager.Scenario.HasActor(actor) && base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			actor.Health = 0;
			bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
			AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.NoGoldDrop == true)
			{
				actor.NoGoldDrop = true;
			}
			GameState.KillActor(base.TargetingActor, actor, CActor.ECauseOfDeath.KillAbility, out var startedOnDeathAbility, this, actorWasAsleep);
			if (startedOnDeathAbility)
			{
				m_SkipWaitForProgressChoreographer = true;
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList)
	{
		actorList.RemoveAll((CActor x) => x is CObjectActor);
		if (!base.TargetingActor.PierceInvulnerability)
		{
			actorList.RemoveAll((CActor x) => x.Invulnerable);
		}
		base.RemoveImmuneActorsFromList(ref actorList);
	}

	public CAbilityKill(CAbilityKill state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
