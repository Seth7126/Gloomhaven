using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAddAugment : CAbilityTargeting
{
	public CAbilityAddAugment()
		: base(EAbilityType.AddAugment)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingAddAugment_MessageData message = new CActorIsApplyingAddAugment_MessageData(base.AnimOverload, actorApplying)
		{
			m_AddAugmentAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			CApplyToActorAddAugment_MessageData message = new CApplyToActorAddAugment_MessageData(base.TargetingActor)
			{
				m_AddAugmentAbility = this,
				m_Target = actor
			};
			ScenarioRuleClient.MessageHandler(message);
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (cBaseCard != null)
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

	public CAbilityAddAugment(CAbilityAddAugment state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
