using System.Collections.Generic;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAttackersGainDisadvantage : CAbilityTargeting
{
	public CAbilityAttackersGainDisadvantage()
		: base(EAbilityType.AttackersGainDisadvantage)
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
			CAttackersGainDisadvantage_MessageData cAttackersGainDisadvantage_MessageData = new CAttackersGainDisadvantage_MessageData(base.AnimOverload, base.TargetingActor);
			cAttackersGainDisadvantage_MessageData.m_AttackersGainDisadvantageAbility = this;
			cAttackersGainDisadvantage_MessageData.m_ActorAppliedTo = actor;
			ScenarioRuleClient.MessageHandler(cAttackersGainDisadvantage_MessageData);
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

	public CAbilityAttackersGainDisadvantage(CAbilityAttackersGainDisadvantage state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
