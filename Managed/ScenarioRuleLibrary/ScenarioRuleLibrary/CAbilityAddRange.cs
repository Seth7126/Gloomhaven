using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAddRange : CAbilityTargeting
{
	public CAbilityAddRange()
		: base(EAbilityType.AddRange)
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
			CAddRange_MessageData cAddRange_MessageData = new CAddRange_MessageData(base.AnimOverload, base.TargetingActor);
			cAddRange_MessageData.m_AddRangeAbility = this;
			cAddRange_MessageData.m_ActorAppliedTo = actor;
			ScenarioRuleClient.MessageHandler(cAddRange_MessageData);
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (cBaseCard != null)
			{
				cBaseCard.AddActiveBonus(this, actor, base.TargetingActor);
			}
			else
			{
				DLLDebug.LogError("Error: Unable to find base ability card for ability " + base.Name);
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

	public CAbilityAddRange(CAbilityAddRange state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
