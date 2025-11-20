using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRemoveActorFromMap : CAbilityTargeting
{
	public CAbilityRemoveActorFromMap()
		: base(EAbilityType.RemoveActorFromMap)
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

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
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
		if (ScenarioManager.Scenario.HasActor(actor) && base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			actor.Health = 0;
			GameState.KillActor(base.TargetingActor, actor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _, this);
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityRemoveActorFromMap(CAbilityRemoveActorFromMap state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
