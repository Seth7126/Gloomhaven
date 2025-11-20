using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityBlockHealing : CAbilityCondition
{
	public CAbilityBlockHealing(int duration, EConditionDecTrigger decTrigger)
		: base(EAbilityType.BlockHealing, duration, decTrigger)
	{
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
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public CAbilityBlockHealing()
	{
	}

	public CAbilityBlockHealing(CAbilityBlockHealing state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
