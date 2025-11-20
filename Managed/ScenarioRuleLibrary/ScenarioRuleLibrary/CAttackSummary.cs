using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class CAttackSummary
{
	public class ConsumeData
	{
		public int BuffAmount { get; private set; }

		public List<ElementInfusionBoardManager.EElement> Elements { get; private set; }

		public ConsumeData(int buffAmount, List<ElementInfusionBoardManager.EElement> elements)
		{
			BuffAmount = buffAmount;
			Elements = elements;
		}
	}

	public class TargetSummary
	{
		private List<AttackModifierYMLData> m_UsedAttackMods;

		private List<AttackModifierYMLData> m_NotUsedAttackMods;

		private int m_FinalAttackStrength;

		private int m_PreShieldAttackStrength;

		private int m_InitialAbilityAttackStrength;

		private List<SEventAttackModifier.OverrideBuffData> m_eventOverrideBuffData;

		public CAttackSummary Parent { get; private set; }

		public CActor Actor { get; private set; }

		public CActor RedirectedToActor { get; set; }

		public int Shield { get; private set; }

		public int Pierce { get; private set; }

		public int ShieldMinusPierce { get; private set; }

		public bool Poison { get; private set; }

		public bool Invulnerable { get; private set; }

		public CAbilityAttack ItemOverrideAbility { get; set; }

		public int FinalAttackStrength
		{
			get
			{
				if (m_FinalAttackStrength < 0)
				{
					return 0;
				}
				return m_FinalAttackStrength;
			}
		}

		public CAbility AttackAbilityWithOverrides { get; set; }

		public bool IsTargeted { get; set; }

		public bool DisadvantageFromAdjacent { get; private set; }

		public bool AttackersGainDisadvantage { get; private set; }

		public EAdvantageStatuses OverallAdvantage { get; set; }

		public List<CActiveBonus> ShieldActiveBonuses { get; private set; }

		public int ActiveBonusShieldBuff { get; private set; }

		public List<int> ConditionalOverridesStrengthBuffs { get; private set; }

		public List<int> ConditionalOverridesXPBuffs { get; private set; }

		public List<int> ItemStrengthBuffs { get; private set; }

		public int AttackEffectBuff { get; private set; }

		public int AttackEffectPierce { get; private set; }

		public int AttackEffectXP { get; private set; }

		public int AttackScalar { get; private set; }

		public int AttackAbilityScalar { get; private set; }

		public int AttackModifierCardsStrength { get; set; }

		public List<AttackModifierYMLData> UsedAttackMods
		{
			get
			{
				return m_UsedAttackMods;
			}
			set
			{
				m_UsedAttackMods = value;
			}
		}

		public List<AttackModifierYMLData> NotUsedAttackMods
		{
			get
			{
				return m_NotUsedAttackMods;
			}
			set
			{
				m_NotUsedAttackMods = value;
			}
		}

		public bool AddedTargetFromAttackModSuccessfully { get; set; }

		public List<int> ActiveBonusStrengthBuffs { get; private set; }

		public List<int> ActiveBonusXPBuffs { get; private set; }

		public List<CActiveBonus> ThisTargetAttackActiveBonuses { get; private set; }

		public CActor ActorToAttack
		{
			get
			{
				if (RedirectedToActor == null)
				{
					return Actor;
				}
				return RedirectedToActor;
			}
		}

		public TargetSummary(CAttackSummary parent, CActor target, CAbility ability)
		{
			Parent = parent;
			Actor = target;
			IsTargeted = false;
			AttackAbilityWithOverrides = CAbility.CopyAbility(ability, generateNewID: false, fullCopy: true);
			AttackAbilityWithOverrides.ConditionalOverrides.Clear();
			ConditionalOverridesStrengthBuffs = new List<int>();
			ConditionalOverridesXPBuffs = new List<int>();
			ItemStrengthBuffs = new List<int>();
			UsedAttackMods = new List<AttackModifierYMLData>();
			m_eventOverrideBuffData = new List<SEventAttackModifier.OverrideBuffData>();
			m_InitialAbilityAttackStrength = ability.Strength;
			if (ability is CAbilityAttack { ChainAttack: not false } cAbilityAttack)
			{
				int num = 0;
				num = ((!cAbilityAttack.ActorsToTarget.Contains(target)) ? cAbilityAttack.ActorsToTarget.Count : cAbilityAttack.ActorsToTarget.IndexOf(target));
				m_InitialAbilityAttackStrength -= cAbilityAttack.ChainAttackDamageReduction * num;
			}
			ActiveBonusStrengthBuffs = new List<int>();
			ActiveBonusXPBuffs = new List<int>();
			ThisTargetAttackActiveBonuses = new List<CActiveBonus>();
			if (parent.AttackActiveBonuses != null)
			{
				ThisTargetAttackActiveBonuses.AddRange(parent.AttackActiveBonuses);
			}
			ThisTargetAttackActiveBonuses.AddRange((from x in CActiveBonus.FindApplicableActiveBonuses(target, CAbility.EAbilityType.Attack)
				where x.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffIncomingAttacks
				select x).ToList());
			if (ThisTargetAttackActiveBonuses != null)
			{
				int num2 = 0;
				foreach (CActiveBonus thisTargetAttackActiveBonuse in ThisTargetAttackActiveBonuses)
				{
					CActor target2 = ((thisTargetAttackActiveBonuse.Ability.ActiveBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.BuffIncomingAttacks) ? ability.TargetingActor : target);
					int num3 = thisTargetAttackActiveBonuse.ReferenceStrength(ability, target2);
					if (thisTargetAttackActiveBonuse.Ability.ActiveBonusData.AttackType == CAbility.EAttackType.Default)
					{
						num2 = Math.Max(num2, num3);
					}
					else if (num3 != 0)
					{
						ActiveBonusStrengthBuffs.Add(num3);
					}
					int num4 = thisTargetAttackActiveBonuse.ReferenceXP(ability, target2);
					if (num4 != 0)
					{
						ActiveBonusXPBuffs.Add(num4);
					}
					m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.ActiveBonus, thisTargetAttackActiveBonuse.BaseCard.Name, num3, thisTargetAttackActiveBonuse.Ability.NegativeConditions.Keys.ToList(), thisTargetAttackActiveBonuse.Ability.PositiveConditions.Keys.ToList(), thisTargetAttackActiveBonuse.Actor.Class.ID, thisTargetAttackActiveBonuse.BaseCard.CardType != CBaseCard.ECardType.ScenarioModifier));
				}
				if (num2 > 0)
				{
					ActiveBonusStrengthBuffs.Add(num2);
				}
			}
			foreach (CConditionalOverride conditionalOverride in ability.ConditionalOverrides)
			{
				CAbility ability2 = ability;
				if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
				{
					ability2 = ability.ParentAbility;
				}
				else if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
				{
					ability2 = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last().m_Ability;
				}
				if (conditionalOverride.Self || !conditionalOverride.Filter.IsValidTarget(target, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) || !conditionalOverride.Requirements.MeetsAbilityRequirements(ability.TargetingActor, ability2))
				{
					continue;
				}
				int strength = AttackAbilityWithOverrides.Strength;
				List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
				List<CCondition.EPositiveCondition> list2 = new List<CCondition.EPositiveCondition>();
				foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
				{
					AttackAbilityWithOverrides.OverrideAbilityValues(abilityOverride, perform: false, null, conditionalOverride.Filter);
					AbilityData.MiscAbilityData miscAbilityData = abilityOverride.MiscAbilityData;
					if (miscAbilityData != null && miscAbilityData.AddTargetPropertyToStrength == CActor.EOverrideLookupProperty.Shield)
					{
						int num5 = ((abilityOverride.MiscAbilityData == null || !abilityOverride.MiscAbilityData.AddTargetPropertyToStrengthMultiplier.HasValue) ? 1 : abilityOverride.MiscAbilityData.AddTargetPropertyToStrengthMultiplier.Value);
						AttackAbilityWithOverrides.Strength += target.CalculateShield(ability) * num5;
					}
					if (abilityOverride.NegativeConditions != null)
					{
						list.AddRange(abilityOverride.NegativeConditions);
					}
					if (abilityOverride.PositiveConditions != null)
					{
						list2.AddRange(abilityOverride.PositiveConditions);
					}
				}
				if (AttackAbilityWithOverrides.Strength - strength > 0)
				{
					ConditionalOverridesStrengthBuffs.Add(AttackAbilityWithOverrides.Strength - strength);
				}
				m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Conditional, ability.AbilityBaseCard.Name, AttackAbilityWithOverrides.Strength - strength, list, list2));
				if (conditionalOverride.XP > 0)
				{
					ConditionalOverridesXPBuffs.Add(conditionalOverride.XP);
				}
			}
			foreach (CItem item in ability.ActiveSingleTargetItems.Where((CItem x) => x.SingleTarget == target))
			{
				int strength2 = AttackAbilityWithOverrides.Strength;
				List<CCondition.ENegativeCondition> list3 = new List<CCondition.ENegativeCondition>();
				List<CCondition.EPositiveCondition> list4 = new List<CCondition.EPositiveCondition>();
				foreach (CAbilityOverride @override in item.YMLData.Data.Overrides)
				{
					AttackAbilityWithOverrides.OverrideAbilityValues(@override, perform: false);
					if (@override.NegativeConditions != null)
					{
						list3.AddRange(@override.NegativeConditions);
					}
					if (@override.PositiveConditions != null)
					{
						list4.AddRange(@override.PositiveConditions);
					}
				}
				if (AttackAbilityWithOverrides.Strength - strength2 > 0)
				{
					ItemStrengthBuffs.Add(AttackAbilityWithOverrides.Strength - strength2);
				}
				m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item, item.Name, AttackAbilityWithOverrides.Strength - strength2, list3, list4));
			}
			foreach (CActiveBonus item2 in ability.ActiveSingleTargetActiveBonuses.Where((CActiveBonus x) => x.SingleTarget == target))
			{
				int strength3 = AttackAbilityWithOverrides.Strength;
				List<CCondition.ENegativeCondition> overrideNegativeConditions = new List<CCondition.ENegativeCondition>();
				List<CCondition.EPositiveCondition> overridePositiveConditions = new List<CCondition.EPositiveCondition>();
				if (item2.Ability.Song != null)
				{
					foreach (CSong.SongEffect songEffect in item2.Ability.Song.SongEffects)
					{
						songEffect.OnSongEffectToggled(AttackAbilityWithOverrides, toggledOn: true);
					}
				}
				if (AttackAbilityWithOverrides.Strength - strength3 > 0)
				{
					ItemStrengthBuffs.Add(AttackAbilityWithOverrides.Strength - strength3);
				}
				m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item, item2.BaseCard.Name, AttackAbilityWithOverrides.Strength - strength3, overrideNegativeConditions, overridePositiveConditions));
			}
			foreach (CItem activeOverrideItem in ability.ActiveOverrideItems)
			{
				int num6 = 0;
				List<CCondition.ENegativeCondition> list5 = new List<CCondition.ENegativeCondition>();
				List<CCondition.EPositiveCondition> list6 = new List<CCondition.EPositiveCondition>();
				foreach (CAbilityOverride override2 in activeOverrideItem.YMLData.Data.Overrides)
				{
					if (override2.Strength.HasValue)
					{
						num6 += override2.Strength.Value;
					}
					if (override2.NegativeConditions != null)
					{
						list5.AddRange(override2.NegativeConditions);
					}
					if (override2.PositiveConditions != null)
					{
						list6.AddRange(override2.PositiveConditions);
					}
				}
				m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item, activeOverrideItem.Name, num6, list5, list6));
			}
			m_PreShieldAttackStrength = ability.Strength;
			AttackAbilityScalar = 0;
			foreach (CActiveBonus attackActiveBonuse in parent.AttackActiveBonuses)
			{
				int num7 = attackActiveBonuse.ReferenceAbilityStrengthScalar(ability, target);
				int num8 = m_PreShieldAttackStrength * num7 - m_PreShieldAttackStrength;
				AttackAbilityScalar += ((num7 > 1) ? num7 : 0);
				if (num8 > 0)
				{
					ActiveBonusStrengthBuffs.Add(num8);
				}
			}
			m_PreShieldAttackStrength += ActiveBonusStrengthBuffs.Sum() + ConditionalOverridesStrengthBuffs.Sum() + ItemStrengthBuffs.Sum();
			if (ability is CAbilityAttack { ChainAttack: not false } cAbilityAttack2)
			{
				int num9 = 0;
				num9 = ((!cAbilityAttack2.ActorsToTarget.Contains(target)) ? cAbilityAttack2.ActorsToTarget.Count : cAbilityAttack2.ActorsToTarget.IndexOf(target));
				m_PreShieldAttackStrength -= cAbilityAttack2.ChainAttackDamageReduction * num9;
			}
			CAbility attackAbilityWithOverrides = AttackAbilityWithOverrides;
			CAbilityAttack attackAbility = attackAbilityWithOverrides as CAbilityAttack;
			if (attackAbility != null)
			{
				AttackEffectBuff = 0;
				AttackEffectXP = 0;
				AttackEffectPierce = 0;
				foreach (CAttackEffect attackEffect in attackAbility.AttackEffects)
				{
					attackEffect.GetBonus(target, attackAbility, out var attackBuff, out var xpBuff);
					AttackEffectBuff += attackBuff;
					AttackEffectXP += xpBuff;
					AttackEffectPierce += attackEffect.Pierce;
				}
				foreach (CAttackActiveBonus_BuffAttack item3 in from x in ThisTargetAttackActiveBonuses
					select x.BespokeBehaviour into y
					where y is CAttackActiveBonus_BuffAttack
					select y)
				{
					AttackEffectPierce += item3.ReferencePierce(attackAbility, target);
				}
				foreach (CAttackActiveBonus_BuffIncomingAttacks item4 in from x in ThisTargetAttackActiveBonuses
					select x.BespokeBehaviour into y
					where y is CAttackActiveBonus_BuffIncomingAttacks
					select y)
				{
					AttackEffectPierce += item4.ReferencePierce(attackAbility, target);
				}
				m_PreShieldAttackStrength += AttackEffectBuff;
				CActor cActor = ((attackAbility.OriginalTargetingActor != null) ? attackAbility.OriginalTargetingActor : attackAbility.TargetingActor);
				DisadvantageFromAdjacent = !attackAbility.IsMeleeAttack && ScenarioManager.IsTileAdjacent(cActor.ArrayIndex.X, cActor.ArrayIndex.Y, Actor.ArrayIndex.X, Actor.ArrayIndex.Y);
				bool num10 = CActiveBonus.FindApplicableActiveBonuses(Actor, CAbility.EAbilityType.Attack).Any(delegate(CActiveBonus x)
				{
					if (x.BespokeBehaviour is CAttackActiveBonus_BuffIncomingAttacks)
					{
						AbilityData.MiscAbilityData miscAbilityData4 = x.Ability.MiscAbilityData;
						if (miscAbilityData4 == null)
						{
							return false;
						}
						return miscAbilityData4.AttackHasAdvantage == true;
					}
					return false;
				}) | ThisTargetAttackActiveBonuses.Any((CActiveBonus x) => x.BespokeBehaviour is CAttackActiveBonus_BuffAttack cAttackActiveBonus_BuffAttack2 && cAttackActiveBonus_BuffAttack2.ReferenceAdvantage(attackAbility, target));
				AttackersGainDisadvantage = (Actor is CEnemyActor cEnemyActor && cEnemyActor.MonsterClass.AttackersGainDisadv) || CActiveBonus.FindApplicableActiveBonuses(Actor, CAbility.EAbilityType.AttackersGainDisadvantage).Count > 0 || CActiveBonus.FindApplicableActiveBonuses(Actor, CAbility.EAbilityType.Shield).FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.BespokeBehaviour != null && x.BespokeBehaviour is CShieldActiveBonus_ShieldAndDisadvantage).Count > 0;
				AttackersGainDisadvantage = AttackersGainDisadvantage || AreThereApplicableActiveBonuses(cActor, ability);
				int num11;
				if (!num10)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = attackAbility.MiscAbilityData;
					num11 = ((miscAbilityData2 != null && miscAbilityData2.AttackHasAdvantage == true) ? 1 : 0);
				}
				else
				{
					num11 = 1;
				}
				bool addAdvantage = (byte)num11 != 0;
				int num12;
				if (!DisadvantageFromAdjacent && !AttackersGainDisadvantage)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = attackAbility.MiscAbilityData;
					num12 = ((miscAbilityData3 != null && miscAbilityData3.AttackHasDisadvantage == true) ? 1 : 0);
				}
				else
				{
					num12 = 1;
				}
				bool addDisadvantage = (byte)num12 != 0;
				OverallAdvantage = cActor.GetAdvantageStatus(addAdvantage, addDisadvantage);
				Poison = Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison);
				if (Poison)
				{
					m_PreShieldAttackStrength++;
				}
				AttackScalar = 0;
				foreach (CActiveBonus attackActiveBonuse2 in parent.AttackActiveBonuses)
				{
					int num13 = attackActiveBonuse2.ReferenceStrengthScalar(ability, target);
					int num14 = m_PreShieldAttackStrength * num13 - m_PreShieldAttackStrength;
					AttackScalar += ((num13 > 1) ? num13 : 0);
					if (num14 > 0)
					{
						ActiveBonusStrengthBuffs.Add(num14);
						m_PreShieldAttackStrength += num14;
					}
				}
				Shield = Actor.BaseShield();
				int num15 = target.CalculateShield(ability);
				ActiveBonusShieldBuff = num15 - Shield;
				Shield += ActiveBonusShieldBuff;
				Pierce = attackAbility.Pierce + AttackEffectPierce;
				if (Pierce >= 99999)
				{
					ShieldMinusPierce = 0;
				}
				else
				{
					ShieldMinusPierce = Math.Max(0, Shield - Pierce);
				}
			}
			Invulnerable = Actor.Invulnerable && !ability.TargetingActor.PierceInvulnerability;
			m_FinalAttackStrength = ((!Invulnerable) ? Math.Max(0, m_PreShieldAttackStrength - ShieldMinusPierce) : 0);
			static bool AreThereApplicableActiveBonuses(CActor attackingActor, CAbility ability3)
			{
				List<CActiveBonus> source = CActiveBonus.FindApplicableActiveBonuses(attackingActor, CAbility.EAbilityType.Attack);
				source = source.Where(delegate(CActiveBonus b)
				{
					CAbility ability4 = b.Ability;
					return ability4 != null && ability4.MiscAbilityData?.AttackHasDisadvantage == true;
				}).ToList();
				HandleVoidWardenControlAbilityWithWardDisadvantageAura(ability3, source);
				return source.Count > 0;
				static void HandleVoidWardenControlAbilityWithWardDisadvantageAura(CAbility cAbility, List<CActiveBonus> applicableBonuses)
				{
					bool num16 = cAbility.IsControlAbility && cAbility.AbilityBaseCard is CBaseAbilityCard cBaseAbilityCard && cBaseAbilityCard.ClassID == "VoidwardenID";
					CActiveBonus cActiveBonus = applicableBonuses.FirstOrDefault((CActiveBonus bonus) => bonus.Ability.AbilityBaseCard is CBaseAbilityCard { ClassID: "WardID" } && bonus.Ability.Name == "ADisadvantageAura");
					if (num16 && cActiveBonus != null)
					{
						applicableBonuses.Remove(cActiveBonus);
					}
				}
			}
		}

		public void DrawAttackModifiers(CActor actor, CActor currentTarget, CAbilityAttack attackAbility, int attackIndex, out int addTargetsDrawn)
		{
			SimpleLog.AddToSimpleLog("Drawing attack modifiers. OverallAdvantage status is: " + OverallAdvantage);
			m_FinalAttackStrength = GameState.DrawAndApplyAttackModifierCards(attackAbility, actor, currentTarget, m_PreShieldAttackStrength, OverallAdvantage, out m_UsedAttackMods, out m_NotUsedAttackMods, out var pierce, out var attackModifierEvent, out addTargetsDrawn, attackIndex);
			AttackModifierCardsStrength = FinalAttackStrength - m_PreShieldAttackStrength;
			int pierce2 = Pierce;
			Pierce += pierce;
			if (Pierce >= 99999)
			{
				ShieldMinusPierce = 0;
			}
			else
			{
				ShieldMinusPierce = Math.Max(0, Shield - Pierce);
			}
			m_FinalAttackStrength = ((!Invulnerable) ? Math.Max(0, m_FinalAttackStrength - ShieldMinusPierce) : 0);
			IsTargeted = true;
			Actor.IncomingAttackDamage = FinalAttackStrength;
			attackModifierEvent.TargetActorShield = Shield;
			attackModifierEvent.DrawingActorPierce = pierce2;
			attackModifierEvent.FinalPierce = Pierce;
			attackModifierEvent.FinalAttackStrength = m_FinalAttackStrength;
			attackModifierEvent.AttackAbilityAttackStrength = m_InitialAbilityAttackStrength;
			attackModifierEvent.OverrideBuffs = m_eventOverrideBuffData;
			Poison = Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison);
			attackModifierEvent.TargetActorWasPoisoned = Poison;
			attackModifierEvent.TargetActorWasInvulnerable = Invulnerable;
			SEventLogMessageHandler.AddEventLogMessage(attackModifierEvent);
		}

		public void OverrideFinalAttackStrength(int overrideStrength)
		{
			m_FinalAttackStrength = overrideStrength;
		}
	}

	public int ModifiedAttackStrength { get; private set; }

	public CAbilityAttack AttackAbility { get; private set; }

	public List<TargetSummary> Targets { get; private set; }

	public List<CActiveBonus> AttackActiveBonuses { get; private set; }

	public List<CActiveBonus> AddTargetActiveBonuses { get; private set; }

	public List<CActiveBonus> AddRangeActiveBonuses { get; private set; }

	public int ActiveBonusAddTargetBuff { get; private set; }

	public int ActiveBonusAddRangeBuff { get; private set; }

	public List<ConsumeData> StrengthBuffConsumes { get; private set; }

	public int ConsumeStrengthBuff { get; private set; }

	public int ConsumeRangeBuff { get; private set; }

	public int ConsumeTargetsBuff { get; private set; }

	public int ItemOverrideStrengthBuff { get; private set; }

	public CAttackSummary()
	{
	}

	public CAttackSummary(CAbility ability)
	{
		DLLDebug.LogInfo("[ATTACK SUMMARY] Created new AttackSummary for Ability Type:" + ability.AbilityType);
		if (ability is CAbilityAttack attackAbility)
		{
			AttackAbility = attackAbility;
		}
		ModifiedAttackStrength = ability.Strength;
		StrengthBuffConsumes = new List<ConsumeData>();
		foreach (CActionAugmentation currentActionSelectedAugmentation in GameState.CurrentActionSelectedAugmentations)
		{
			bool flag = false;
			ConsumeStrengthBuff = 0;
			ConsumeRangeBuff = 0;
			ConsumeTargetsBuff = 0;
			foreach (CActionAugmentationOp augmentationOp in currentActionSelectedAugmentation.AugmentationOps)
			{
				if (augmentationOp.ParentAbilityName == ability.Name && augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
				{
					if (augmentationOp.AbilityOverride.Strength.HasValue)
					{
						ConsumeStrengthBuff += augmentationOp.AbilityOverride.Strength.Value;
						flag = true;
					}
					if (augmentationOp.AbilityOverride.Range.HasValue)
					{
						ConsumeRangeBuff += augmentationOp.AbilityOverride.Range.Value;
					}
					if (augmentationOp.AbilityOverride.NumberOfTargets.HasValue)
					{
						ConsumeTargetsBuff += augmentationOp.AbilityOverride.NumberOfTargets.Value;
					}
				}
			}
			if (flag)
			{
				StrengthBuffConsumes.Add(new ConsumeData(ConsumeStrengthBuff, currentActionSelectedAugmentation.Elements));
			}
		}
		AttackActiveBonuses = (from w in CActiveBonus.FindApplicableActiveBonuses(ability.TargetingActor, CAbility.EAbilityType.Attack)
			where w.Ability.Augment == null && w.RequirementsMet(ability.TargetingActor)
			select w).ToList();
		AddTargetActiveBonuses = (from w in CActiveBonus.FindApplicableActiveBonuses(ability.TargetingActor, CAbility.EAbilityType.AddTarget)
			where w.Ability.Augment == null && w.RequirementsMet(ability.TargetingActor)
			select w).ToList();
		AddRangeActiveBonuses = (from w in CActiveBonus.FindApplicableActiveBonuses(ability.TargetingActor, CAbility.EAbilityType.AddRange)
			where w.Ability.Augment == null && w.RequirementsMet(ability.TargetingActor)
			select w).ToList();
		ActiveBonusAddTargetBuff = 0;
		if (AddTargetActiveBonuses != null)
		{
			ActiveBonusAddTargetBuff = AddTargetActiveBonuses.Sum((CActiveBonus x) => x.ReferenceStrength(ability, null));
		}
		ActiveBonusAddRangeBuff = 0;
		if (AddRangeActiveBonuses != null)
		{
			ActiveBonusAddRangeBuff = AddRangeActiveBonuses.Sum((CActiveBonus x) => x.ReferenceStrength(ability, null));
		}
		foreach (CItem activeOverrideItem in ability.ActiveOverrideItems)
		{
			ItemOverrideStrengthBuff = 0;
			foreach (CAbilityOverride @override in activeOverrideItem.YMLData.Data.Overrides)
			{
				if (@override.Strength.HasValue)
				{
					ItemOverrideStrengthBuff += @override.Strength.Value;
				}
			}
		}
		UpdateTargetData(ability, null, null);
	}

	public CAttackSummary Copy()
	{
		DLLDebug.LogInfo("[ATTACK SUMMARY] Copying Attack Summary");
		return new CAttackSummary
		{
			ModifiedAttackStrength = ModifiedAttackStrength,
			AttackAbility = AttackAbility,
			Targets = Targets.ToList(),
			AttackActiveBonuses = AttackActiveBonuses.ToList(),
			AddTargetActiveBonuses = AddTargetActiveBonuses.ToList(),
			AddRangeActiveBonuses = AddRangeActiveBonuses.ToList(),
			ActiveBonusAddTargetBuff = ActiveBonusAddTargetBuff,
			ActiveBonusAddRangeBuff = ActiveBonusAddRangeBuff,
			StrengthBuffConsumes = StrengthBuffConsumes.ToList(),
			ConsumeStrengthBuff = ConsumeStrengthBuff,
			ConsumeRangeBuff = ConsumeRangeBuff,
			ConsumeTargetsBuff = ConsumeTargetsBuff,
			ItemOverrideStrengthBuff = ItemOverrideStrengthBuff
		};
	}

	public void UpdateTargetData(CAbility ability, List<CItem> activeSingleTargetItems, List<CActiveBonus> activeSingleTargetBonuses, bool useActorsToTarget = false, bool useBothLists = false)
	{
		DLLDebug.LogInfo("[ATTACK SUMMARY] Updating Target Summary Data");
		if (Targets == null)
		{
			Targets = new List<TargetSummary>();
		}
		else
		{
			Targets.Clear();
			DLLDebug.LogInfo("[ATTACK SUMMARY] Cleared all Target Summarys");
		}
		List<CActor> list = (useBothLists ? ability.ActorsToTarget.Union(ability.ValidActorsInRange).ToList() : (useActorsToTarget ? ability.ActorsToTarget : ability.ValidActorsInRange));
		foreach (CActor item in list)
		{
			Targets.Add(new TargetSummary(this, item, ability));
			DLLDebug.LogInfo("[ATTACK SUMMARY] Created new Target Summary for Actor Guid: " + item.ActorGuid);
			ability.ApplySingleTargetActiveBonus(item);
			ability.ApplySingleTargetItem(item);
		}
		if (!(ability is CAbilityAttack { MultiPassAttack: not false, MultiPassActors: not null } cAbilityAttack))
		{
			return;
		}
		foreach (CActor multiPassActor in cAbilityAttack.MultiPassActors)
		{
			if (!list.Contains(multiPassActor))
			{
				Targets.Add(new TargetSummary(this, multiPassActor, ability));
				DLLDebug.LogInfo("[ATTACK SUMMARY] Created new Target Summary for Actor Guid: " + multiPassActor.ActorGuid);
				ability.ApplySingleTargetActiveBonus(multiPassActor);
				ability.ApplySingleTargetItem(multiPassActor);
			}
		}
	}

	public void UpdateSingleTargetData(CAbilityAttack attackAbility, CActor actor)
	{
		DLLDebug.LogInfo("[ATTACK SUMMARY] Updating Single Target Summary Data");
		int targetIndex = 0;
		if (FindTarget(actor, ref targetIndex) == null)
		{
			Targets.Add(new TargetSummary(this, actor, attackAbility));
			DLLDebug.LogInfo("[ATTACK SUMMARY] Created new Target Summary for Actor Guid: " + actor.ActorGuid);
		}
	}

	public void RefreshingTargetDataForMultipassAttack(CAbilityAttack attackAbility, CActor actor)
	{
		DLLDebug.LogInfo("[ATTACK SUMMARY] Refreshing Target Summary data for multipass attack for Actor Guid: " + actor.ActorGuid);
		int num = Targets.FindIndex((TargetSummary x) => x.Actor == actor);
		if (num > -1)
		{
			bool isTargeted = Targets[num].IsTargeted;
			Targets[num] = new TargetSummary(this, actor, attackAbility)
			{
				ItemOverrideAbility = Targets[num].ItemOverrideAbility
			};
			Targets[num].IsTargeted = isTargeted;
		}
	}

	public void RefreshingTargetDataForActiveBonusToggled(CAbilityAttack attackAbility, CActor actor, TargetSummary currentAttackingTargetSummary)
	{
		DLLDebug.LogInfo("[ATTACK SUMMARY] Refreshing Target Summary data for active bonus toggled for Actor Guid: " + actor.ActorGuid);
		int num = Targets.FindIndex((TargetSummary x) => x == currentAttackingTargetSummary);
		if (num > -1)
		{
			Targets[num] = new TargetSummary(this, Targets[num].Actor, attackAbility)
			{
				ItemOverrideAbility = Targets[num].ItemOverrideAbility,
				RedirectedToActor = Targets[num].RedirectedToActor,
				IsTargeted = Targets[num].IsTargeted,
				UsedAttackMods = Targets[num].UsedAttackMods,
				NotUsedAttackMods = Targets[num].NotUsedAttackMods,
				AttackModifierCardsStrength = Targets[num].AttackModifierCardsStrength
			};
		}
		attackAbility.CurrentAttackingTargetSummary = Targets[num];
	}

	public TargetSummary FindTarget(CActor actor, ref int targetIndex)
	{
		if (Targets.Count((TargetSummary x) => x.Actor.ActorGuid == actor.ActorGuid) > 1)
		{
			if (targetIndex < Targets.Count)
			{
				int? num = Targets.Skip(targetIndex).Select((TargetSummary summary, int index) => new { summary, index }).First(x => x.summary.Actor.ActorGuid == actor.ActorGuid)
					.index;
				if (num.HasValue)
				{
					targetIndex += num.Value;
					if (targetIndex < Targets.Count)
					{
						TargetSummary result = Targets[targetIndex];
						targetIndex++;
						return result;
					}
				}
			}
			return null;
		}
		targetIndex++;
		return Targets.SingleOrDefault((TargetSummary x) => x.Actor.ActorGuid == actor.ActorGuid);
	}

	public void OverrideModifiedStrength(int overrideStrength)
	{
		ModifiedAttackStrength = overrideStrength;
		foreach (TargetSummary target in Targets)
		{
			target.OverrideFinalAttackStrength(overrideStrength);
		}
	}
}
