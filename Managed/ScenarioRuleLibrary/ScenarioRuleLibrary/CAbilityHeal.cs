using System;
using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityHeal : CAbilityTargeting
{
	public class HealAbilityData
	{
		public bool IgnoreTokens;

		public List<CCondition.EPositiveCondition> PositiveConditionsToAddIfHealRemovesPoison;

		public List<CCondition.ENegativeCondition> NegativeConditionsToAddIfHealRemovesPoison;

		public CAbilityFilterContainer ApplyConditionsOnRemovingPoisonFilter;

		public List<CCondition.EPositiveCondition> PositiveConditionsToAddIfHealRemovesWound;

		public List<CCondition.ENegativeCondition> NegativeConditionsToAddIfHealRemovesWound;

		public CAbilityFilterContainer ApplyConditionsOnRemovingWoundFilter;

		public HealAbilityData()
		{
			IgnoreTokens = false;
			PositiveConditionsToAddIfHealRemovesPoison = new List<CCondition.EPositiveCondition>();
			NegativeConditionsToAddIfHealRemovesPoison = new List<CCondition.ENegativeCondition>();
			ApplyConditionsOnRemovingPoisonFilter = CAbilityFilterContainer.CreateDefaultFilter();
			PositiveConditionsToAddIfHealRemovesWound = new List<CCondition.EPositiveCondition>();
			NegativeConditionsToAddIfHealRemovesWound = new List<CCondition.ENegativeCondition>();
			ApplyConditionsOnRemovingWoundFilter = CAbilityFilterContainer.CreateDefaultFilter();
		}

		public HealAbilityData Copy()
		{
			return new HealAbilityData
			{
				IgnoreTokens = IgnoreTokens,
				PositiveConditionsToAddIfHealRemovesPoison = PositiveConditionsToAddIfHealRemovesPoison.ToList(),
				NegativeConditionsToAddIfHealRemovesPoison = NegativeConditionsToAddIfHealRemovesPoison.ToList(),
				ApplyConditionsOnRemovingPoisonFilter = ApplyConditionsOnRemovingPoisonFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesWound = PositiveConditionsToAddIfHealRemovesWound.ToList(),
				NegativeConditionsToAddIfHealRemovesWound = NegativeConditionsToAddIfHealRemovesWound.ToList(),
				ApplyConditionsOnRemovingWoundFilter = ApplyConditionsOnRemovingWoundFilter.Copy()
			};
		}

		public HealAbilityData Merge(HealAbilityData dataToMerge)
		{
			return new HealAbilityData
			{
				IgnoreTokens = (IgnoreTokens || dataToMerge.IgnoreTokens),
				PositiveConditionsToAddIfHealRemovesPoison = PositiveConditionsToAddIfHealRemovesPoison.Concat(dataToMerge.PositiveConditionsToAddIfHealRemovesPoison).ToList(),
				NegativeConditionsToAddIfHealRemovesPoison = NegativeConditionsToAddIfHealRemovesPoison.Concat(dataToMerge.NegativeConditionsToAddIfHealRemovesPoison).ToList(),
				ApplyConditionsOnRemovingPoisonFilter = dataToMerge.ApplyConditionsOnRemovingPoisonFilter.Copy(),
				PositiveConditionsToAddIfHealRemovesWound = PositiveConditionsToAddIfHealRemovesWound.Concat(dataToMerge.PositiveConditionsToAddIfHealRemovesWound).ToList(),
				NegativeConditionsToAddIfHealRemovesWound = NegativeConditionsToAddIfHealRemovesWound.Concat(dataToMerge.NegativeConditionsToAddIfHealRemovesWound).ToList(),
				ApplyConditionsOnRemovingWoundFilter = dataToMerge.ApplyConditionsOnRemovingWoundFilter.Copy()
			};
		}

		public static HealAbilityData DefaultHealData()
		{
			return new HealAbilityData();
		}
	}

	public HealAbilityData HealData { get; set; }

	public CAbilityHeal(HealAbilityData healData)
		: base(EAbilityType.Heal)
	{
		if (healData == null)
		{
			healData = HealAbilityData.DefaultHealData();
		}
		HealData = healData;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.AddHeal))
		{
			CAbility ability = item.Ability;
			if (ability == null || ability.ActiveBonusData?.Behaviour != CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal)
			{
				m_ModifiedStrength += item.ReferenceStrength(this, base.TargetingActor);
			}
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
	}

	public override bool PreProcessBeforeActorApplies()
	{
		if (base.TargetingActor.Type != CActor.EType.Player)
		{
			bool flag = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				if (item.Health < item.MaxHealth || item.Tokens.HasKey(CCondition.ENegativeCondition.Wound) || item.Tokens.HasKey(CCondition.ENegativeCondition.Poison))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				m_CancelAbility = true;
			}
		}
		base.PreProcessBeforeActorApplies();
		return true;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		Dictionary<CActor, int> dictionary = new Dictionary<CActor, int>();
		foreach (CActor item in actorsAppliedTo)
		{
			foreach (CActiveBonus item2 in CActiveBonus.FindApplicableActiveBonuses(item, EAbilityType.AddHeal))
			{
				CAbility ability = item2.Ability;
				if (ability != null && ability.ActiveBonusData?.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal)
				{
					m_Strength += item2.ReferenceStrength(this, item);
				}
			}
			if (base.MiscAbilityData != null && base.MiscAbilityData.HealPercentageOfHealth.HasValue)
			{
				dictionary.Add(item, m_Strength + (int)Math.Floor((float)item.OriginalMaxHealth * base.MiscAbilityData.HealPercentageOfHealth.Value));
			}
			else
			{
				dictionary.Add(item, m_Strength);
			}
		}
		CActorIsHealing_MessageData message = new CActorIsHealing_MessageData(base.AnimOverload, actorApplying)
		{
			m_ActorsHealedAndHealStrength = dictionary,
			m_HealAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (actorApplying.Type != CActor.EType.Player)
		{
			validActorsInRange.Sort((CActor x, CActor y) => (100 * (y.MaxHealth - y.Health) + ((y.Tokens.HasKey(CCondition.ENegativeCondition.Wound) || y.Tokens.HasKey(CCondition.ENegativeCondition.Poison)) ? 1 : 0)).CompareTo(100 * (x.MaxHealth - x.Health) + ((x.Tokens.HasKey(CCondition.ENegativeCondition.Wound) || x.Tokens.HasKey(CCondition.ENegativeCondition.Poison)) ? 1 : 0)));
		}
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (ScenarioManager.Scenario.HasActor(actor) && CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.BlockHealing).Count == 0 && base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			int health = actor.Health;
			if (base.TargetingActor.m_OnHealApplyToActorListeners != null)
			{
				base.TargetingActor.m_OnHealApplyToActorListeners?.Invoke(this);
			}
			actor.m_OnBeingHealedListeners?.Invoke(this);
			m_ModifiedStrength = m_Strength;
			if (base.MiscAbilityData != null && base.MiscAbilityData.HealPercentageOfHealth.HasValue)
			{
				m_ModifiedStrength += (int)Math.Floor((float)actor.OriginalMaxHealth * base.MiscAbilityData.HealPercentageOfHealth.Value);
			}
			actor.Healed(m_ModifiedStrength, base.TargetingActor, out var woundRemoved, out var poisonRemoved, out var healedAmount, HealData.IgnoreTokens);
			if (poisonRemoved)
			{
				if (HealData.PositiveConditionsToAddIfHealRemovesPoison.Count > 0 && HealData.ApplyConditionsOnRemovingPoisonFilter.IsValidTarget(actor, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					foreach (CCondition.EPositiveCondition condition in HealData.PositiveConditionsToAddIfHealRemovesPoison)
					{
						CAbility cAbility = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), base.TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility.Start(base.TargetingActor, base.FilterActor);
						((CAbilityTargeting)cAbility).ApplyToActor(actor);
					}
				}
				if (HealData.NegativeConditionsToAddIfHealRemovesPoison.Count > 0 && HealData.ApplyConditionsOnRemovingWoundFilter.IsValidTarget(actor, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					foreach (CCondition.ENegativeCondition condition2 in HealData.NegativeConditionsToAddIfHealRemovesPoison)
					{
						CAbility cAbility2 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition2.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), base.TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility2.Start(base.TargetingActor, base.FilterActor);
						((CAbilityTargeting)cAbility2).ApplyToActor(actor);
					}
				}
			}
			if (woundRemoved)
			{
				if (HealData.PositiveConditionsToAddIfHealRemovesWound.Count > 0)
				{
					foreach (CCondition.EPositiveCondition condition3 in HealData.PositiveConditionsToAddIfHealRemovesWound)
					{
						CAbility cAbility3 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition3.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), base.TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility3.Start(base.TargetingActor, base.FilterActor);
						((CAbilityTargeting)cAbility3).ApplyToActor(actor);
					}
				}
				if (HealData.NegativeConditionsToAddIfHealRemovesWound.Count > 0)
				{
					foreach (CCondition.ENegativeCondition condition4 in HealData.NegativeConditionsToAddIfHealRemovesWound)
					{
						CAbility cAbility4 = CAbility.CreateAbility(CAbility.AbilityTypes.SingleOrDefault((EAbilityType x) => x.ToString() == condition4.ToString()), CAbilityFilterContainer.CreateDefaultFilter(), base.TargetingActor.IsOriginalMonsterType, isTargetedAbility: true, 1);
						cAbility4.Start(base.TargetingActor, base.FilterActor);
						((CAbilityTargeting)cAbility4).ApplyToActor(actor);
					}
				}
			}
			CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(base.TargetingActor, actor, (!poisonRemoved) ? healedAmount : 0, health, poisonRemoved, woundRemoved);
			ScenarioRuleClient.MessageHandler(message);
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

	public override void Restart()
	{
		base.Restart();
		m_ModifiedStrength = m_Strength;
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.AddHeal))
		{
			CAbility ability = item.Ability;
			if (ability == null || ability.ActiveBonusData?.Behaviour != CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal)
			{
				m_ModifiedStrength += item.ReferenceStrength(this, base.TargetingActor);
			}
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
	}

	public override void AbilityEnded()
	{
		if (m_AbilityHasHappened)
		{
			base.TargetingActor.m_OnHealApplyToActionListeners?.Invoke(this);
		}
		base.AbilityEnded();
	}

	public CAbilityHeal()
	{
	}

	public CAbilityHeal(CAbilityHeal state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
