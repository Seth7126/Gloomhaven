using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityOverrideAugmentAttackType : CAbilityTargeting
{
	public EAttackType OverrideAttackType;

	public CAbilityOverrideAugmentAttackType(EAttackType overrideAttackType)
		: base(EAbilityType.OverrideAugmentAttackType)
	{
		OverrideAttackType = overrideAttackType;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingOverrideAugmentAttackType_MessageData message = new CActorIsApplyingOverrideAugmentAttackType_MessageData(base.AnimOverload, actorApplying)
		{
			m_OverrideAugmentAttackTypeAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			CApplyToActorOverrideAugmentAttackType_MessageData message = new CApplyToActorOverrideAugmentAttackType_MessageData(base.TargetingActor)
			{
				m_OverrideAugmentAttackTypeAbility = this,
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

	public CAbilityOverrideAugmentAttackType()
	{
	}

	public CAbilityOverrideAugmentAttackType(CAbilityOverrideAugmentAttackType state, ReferenceDictionary references)
		: base(state, references)
	{
		OverrideAttackType = state.OverrideAttackType;
	}
}
