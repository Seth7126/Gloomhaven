using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAddCondition : CAbilityTargeting
{
	public CAbilityAddCondition()
		: base(EAbilityType.AddCondition)
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
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList)
	{
		if (base.ActiveBonusData != null && base.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.AddConditionUntilDamaged)
		{
			actorList.RemoveAll((CActor x) => base.ActiveBonusData.AbilityData.PositiveConditions.Values.Any((CAbility y) => CAbility.ImmuneToAbility(x, y)));
			actorList.RemoveAll((CActor x) => base.ActiveBonusData.AbilityData.NegativeConditions.Values.Any((CAbility y) => CAbility.ImmuneToAbility(x, y)));
		}
		base.RemoveImmuneActorsFromList(ref actorList);
	}

	public CAbilityAddCondition(CAbilityAddCondition state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
