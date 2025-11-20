using System.Collections.Generic;
using AStar;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRetaliate : CAbilityTargeting
{
	public const int RETALIATE_RANGE_ANY = 99999;

	private int m_RetaliateRange;

	public int RetaliateRange
	{
		get
		{
			return m_RetaliateRange;
		}
		set
		{
			m_RetaliateRange = value;
		}
	}

	public CAbilityRetaliate(int retaliateRange)
		: base(EAbilityType.Retaliate)
	{
		m_RetaliateRange = retaliateRange;
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
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			CRetaliate_MessageData cRetaliate_MessageData = new CRetaliate_MessageData(base.AnimOverload, base.TargetingActor);
			cRetaliate_MessageData.m_RetaliateAbility = this;
			cRetaliate_MessageData.m_ActorAppliedTo = actor;
			ScenarioRuleClient.MessageHandler(cRetaliate_MessageData);
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (base.ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, base.TargetingActor);
			}
			else if (cBaseCard != null)
			{
				cBaseCard.AddActiveBonus(this, actor, base.TargetingActor);
			}
			else
			{
				DLLDebug.LogError("Unable to find base ability card for ability " + base.Name);
			}
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

	public static void SingleShotRetaliate(int strength, int retaliateRange, CActor actorAttacking, CActor actorBeingAttacked, CAbility damageAbility)
	{
		if (actorAttacking != null && actorBeingAttacked != null && damageAbility != null && strength > 0 && retaliateRange > 0)
		{
			bool foundPath;
			List<Point> list = ScenarioManager.PathFinder.FindPath(actorAttacking.ArrayIndex, actorBeingAttacked.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
			if (foundPath && list.Count <= retaliateRange)
			{
				CAbilityRetaliate retaliateAbility = new CAbilityRetaliate(retaliateRange)
				{
					Strength = strength
				};
				CRetaliate_MessageData message = new CRetaliate_MessageData("", actorBeingAttacked)
				{
					m_RetaliateAbility = retaliateAbility,
					m_ActorAppliedTo = actorBeingAttacked
				};
				ScenarioRuleClient.MessageHandler(message);
				actorAttacking.ApplyRetaliateToAttack(actorBeingAttacked, damageAbility, strength);
			}
		}
	}

	public CAbilityRetaliate()
	{
	}

	public CAbilityRetaliate(CAbilityRetaliate state, ReferenceDictionary references)
		: base(state, references)
	{
		m_RetaliateRange = state.m_RetaliateRange;
	}
}
