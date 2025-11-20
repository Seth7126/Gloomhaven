using System;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityCondition : CAbilityTargeting
{
	public static EConditionDecTrigger[] ConditionDecTriggers = (EConditionDecTrigger[])Enum.GetValues(typeof(EConditionDecTrigger));

	public EAbilityType thisability;

	public int Duration { get; set; }

	public EConditionDecTrigger DecrementTrigger { get; set; }

	public CAbilityCondition(EAbilityType abilityType, int duration, EConditionDecTrigger decTrigger)
		: base(abilityType)
	{
		thisability = abilityType;
		Duration = duration;
		DecrementTrigger = decTrigger;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (base.TargetingActor.Type == CActor.EType.Player && base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !IsPositive() && !base.TargetingActor.Immunities.Contains(base.AbilityType))
		{
			m_CanSkip = false;
		}
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.AddCondition, CActiveBonus.EActiveBonusBehaviourType.MultiplyAddedConditions))
		{
			if (base.Strength == 0)
			{
				base.Strength = 1;
			}
			base.Strength += item.ReferenceStrength(this, base.TargetingActor);
		}
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			if (base.AbilityType != EAbilityType.Sleep && actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				actor.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep);
				CActorAwakened_MessageData message = new CActorAwakened_MessageData(actor);
				ScenarioRuleClient.MessageHandler(message);
			}
			return true;
		}
		return false;
	}

	public bool AbilityTypeEqualsEnhancement(EEnhancement enhancementType)
	{
		return enhancementType switch
		{
			EEnhancement.Bless => base.AbilityType == EAbilityType.Bless, 
			EEnhancement.Curse => base.AbilityType == EAbilityType.Curse, 
			EEnhancement.Disarm => base.AbilityType == EAbilityType.Disarm, 
			EEnhancement.Immobilize => base.AbilityType == EAbilityType.Immobilize, 
			EEnhancement.Muddle => base.AbilityType == EAbilityType.Muddle, 
			EEnhancement.Poison => base.AbilityType == EAbilityType.Poison, 
			EEnhancement.Strengthen => base.AbilityType == EAbilityType.Strengthen, 
			EEnhancement.Wound => base.AbilityType == EAbilityType.Wound, 
			_ => false, 
		};
	}

	public CAbilityCondition()
	{
	}

	public CAbilityCondition(CAbilityCondition state, ReferenceDictionary references)
		: base(state, references)
	{
		Duration = state.Duration;
		DecrementTrigger = state.DecrementTrigger;
		thisability = state.thisability;
	}
}
