using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRedirect : CAbilityTargeting
{
	public CAbilityRedirect()
		: base(EAbilityType.Redirect)
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
			CRedirect_MessageData cRedirect_MessageData = new CRedirect_MessageData(base.AnimOverload, null);
			cRedirect_MessageData.m_RedirectAbility = this;
			cRedirect_MessageData.m_ActorAppliedTo = actor;
			ScenarioRuleClient.MessageHandler(cRedirect_MessageData);
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

	public CAbilityRedirect(CAbilityRedirect state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
