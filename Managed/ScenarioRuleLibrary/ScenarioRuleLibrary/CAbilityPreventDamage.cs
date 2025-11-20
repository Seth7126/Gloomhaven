using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityPreventDamage : CAbilityTargeting
{
	private bool m_AttackSourcesOnly;

	public bool AttackSourcesOnly
	{
		get
		{
			return m_AttackSourcesOnly;
		}
		set
		{
			m_AttackSourcesOnly = value;
		}
	}

	public CAbilityPreventDamage(bool attackSourceOnly)
		: base(EAbilityType.PreventDamage)
	{
		m_AttackSourcesOnly = attackSourceOnly;
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
			base.TargetingActor.FindCardWithAbility(this).AddActiveBonus(this, base.TargetingActor, base.TargetingActor);
			CPreventDamage_MessageData message = new CPreventDamage_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_PreventDamageAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
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

	public CAbilityPreventDamage()
	{
	}

	public CAbilityPreventDamage(CAbilityPreventDamage state, ReferenceDictionary references)
		: base(state, references)
	{
		m_AttackSourcesOnly = state.m_AttackSourcesOnly;
	}
}
