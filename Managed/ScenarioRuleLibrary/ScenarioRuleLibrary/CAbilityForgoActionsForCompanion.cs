using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityForgoActionsForCompanion : CAbilityTargeting
{
	public CAbility ForgoTopActionAbility { get; set; }

	public CAbility ForgoBottomActionAbility { get; set; }

	public CAbilityForgoActionsForCompanion(CAbility forgoTopActionAbility, CAbility forgoBottomActionAbility)
		: base(EAbilityType.ForgoActionsForCompanion)
	{
		ForgoTopActionAbility = forgoTopActionAbility;
		ForgoBottomActionAbility = forgoBottomActionAbility;
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

	public CAbilityForgoActionsForCompanion()
	{
	}

	public CAbilityForgoActionsForCompanion(CAbilityForgoActionsForCompanion state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
