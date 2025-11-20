using System.Collections.Generic;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityImmunityTo : CAbilityTargeting
{
	public List<EAbilityType> ImmuneToAbilityTypes { get; set; }

	public List<EAttackType> ImmuneToAttackTypes { get; set; }

	public CAbilityImmunityTo(List<EAbilityType> immuneToAbilityTypes, List<EAttackType> immuneToAttackTypes)
		: base(EAbilityType.ImmunityTo)
	{
		ImmuneToAbilityTypes = immuneToAbilityTypes;
		ImmuneToAttackTypes = immuneToAttackTypes;
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
			((CImmunityActiveBonus)base.TargetingActor.FindCardWithAbility(this).AddActiveBonus(this, actor, base.TargetingActor)).RemoveConditionsFromAffectedActors();
			base.AbilityHasHappened = true;
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityImmunityTo()
	{
	}

	public CAbilityImmunityTo(CAbilityImmunityTo state, ReferenceDictionary references)
		: base(state, references)
	{
		ImmuneToAbilityTypes = references.Get(state.ImmuneToAbilityTypes);
		if (ImmuneToAbilityTypes == null && state.ImmuneToAbilityTypes != null)
		{
			ImmuneToAbilityTypes = new List<EAbilityType>();
			for (int i = 0; i < state.ImmuneToAbilityTypes.Count; i++)
			{
				EAbilityType item = state.ImmuneToAbilityTypes[i];
				ImmuneToAbilityTypes.Add(item);
			}
			references.Add(state.ImmuneToAbilityTypes, ImmuneToAbilityTypes);
		}
		ImmuneToAttackTypes = references.Get(state.ImmuneToAttackTypes);
		if (ImmuneToAttackTypes == null && state.ImmuneToAttackTypes != null)
		{
			ImmuneToAttackTypes = new List<EAttackType>();
			for (int j = 0; j < state.ImmuneToAttackTypes.Count; j++)
			{
				EAttackType item2 = state.ImmuneToAttackTypes[j];
				ImmuneToAttackTypes.Add(item2);
			}
			references.Add(state.ImmuneToAttackTypes, ImmuneToAttackTypes);
		}
	}
}
