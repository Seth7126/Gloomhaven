using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAttack : CAbility
{
	public enum EAttackState
	{
		None,
		SelectAttackFocus,
		PreActorIsAttacking,
		SelectActiveBonusBuffTarget,
		UpdateAttackFocusAfterAttackEffectInlineSubAbility,
		CheckForDamageSelf,
		DrawingModifiers,
		ActorIsAttacking,
		ActorHasAttacked,
		ActorBeenAttacked,
		CheckForPlayerIdle,
		FinaliseAttack,
		AttackDone,
		AttackBuff,
		AttackBuffDone,
		SelectAttackFocusAdditionalTargets,
		SelectAdditionalTargetsDone,
		SelectAttackFocusAddTargetBuff,
		SelectAttackFocusAddTargetBuffDone
	}

	public static EAttackState[] AttackStates = (EAttackState[])Enum.GetValues(typeof(EAttackState));

	public const int PIERCE_ALL = 99999;

	public const string DEFAULT_ATTACK_NAME = "DefaultAttack";

	private EAttackState m_State;

	private List<CActor> m_AttackingActors = new List<CActor>();

	private List<CActor> m_ActorsToTargetLocked = new List<CActor>();

	private List<int> ActorsToTargetModifiedStrength = new List<int>();

	private bool m_MultiPassAttack;

	private bool m_MultiPassAttackStarted;

	private List<CActor> m_MultiPassActors;

	private bool m_ChainAttack;

	private int m_ChainAttackRange;

	private int m_ChainAttackDamageReduction;

	private CAttackSummary m_AttackSummary;

	private bool m_ProcessedStatusEffects;

	private int m_TotalNumberTargets;

	private bool m_IsDefaultAttack;

	private int m_AreaRangeTileX;

	private int m_AreaRangeTileY;

	private int m_CurrentAttackIndex;

	private int m_CurrentPassAttackIndex;

	private List<CActiveBonus> m_StackedInlineAbilityActiveBonuses = new List<CActiveBonus>();

	private CActiveBonus m_CurrentStackedInlineAbilityActiveBonus;

	private bool m_FirstAttackofAction = true;

	private bool m_ModifiersChecked;

	private bool m_InlineModifiersChecked;

	private List<CAbility> m_ModifierAbilities;

	private List<AttackModifierYMLData> m_PreviousAttackModsForTarget;

	private static int s_AttackValueOverride = int.MaxValue;

	public static string s_AttackModifierCardOverride = null;

	public static int AttackValueOverride => s_AttackValueOverride;

	public EAttackState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public CAttackSummary AttackSummary => m_AttackSummary;

	public List<CActor> AttackingActors => m_AttackingActors;

	public bool MultiPassAttack
	{
		get
		{
			return m_MultiPassAttack;
		}
		set
		{
			m_MultiPassAttack = value;
		}
	}

	public bool ChainAttack
	{
		get
		{
			return m_ChainAttack;
		}
		set
		{
			m_ChainAttack = value;
		}
	}

	public int ChainAttackRange
	{
		get
		{
			return m_ChainAttackRange;
		}
		set
		{
			m_ChainAttackRange = value;
		}
	}

	public int ChainAttackDamageReduction
	{
		get
		{
			return m_ChainAttackDamageReduction;
		}
		set
		{
			m_ChainAttackDamageReduction = value;
		}
	}

	public List<CActor> MultiPassActors => m_MultiPassActors;

	public int Pierce { get; set; }

	public int DamageSelfBeforeAttack { get; set; }

	public bool IsDefaultAttack => m_IsDefaultAttack;

	public int AreaRangeTileX
	{
		get
		{
			return m_AreaRangeTileX;
		}
		set
		{
			m_AreaRangeTileX = value;
		}
	}

	public int AreaRangeTileY
	{
		get
		{
			return m_AreaRangeTileY;
		}
		set
		{
			m_AreaRangeTileY = value;
		}
	}

	public List<CAttackEffect> AttackEffects { get; set; }

	public List<CAbility> ModifierAbilities => m_ModifierAbilities;

	public CAttackSummary.TargetSummary CurrentAttackingTargetSummary { get; set; }

	public bool IsMeleeAttack
	{
		get
		{
			AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.UseParentRangeType == true && base.ParentAbility != null)
			{
				if (base.ParentAbility is CAbilityAttack cAbilityAttack)
				{
					return cAbilityAttack.IsMeleeAttack;
				}
				if (base.ParentAbility is CAbilityMerged cAbilityMerged)
				{
					CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
					if (mergedWithAbility.Range > 1)
					{
						AbilityData.MiscAbilityData miscAbilityData2 = mergedWithAbility.MiscAbilityData;
						if (miscAbilityData2 == null)
						{
							return false;
						}
						return miscAbilityData2.TreatAsMelee == true;
					}
					return true;
				}
				if (base.ParentAbility.Range > 1)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = base.ParentAbility.MiscAbilityData;
					if (miscAbilityData3 == null)
					{
						return false;
					}
					return miscAbilityData3.TreatAsMelee == true;
				}
				return true;
			}
			if (base.AreaEffect != null)
			{
				AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
				if (miscAbilityData4 != null && miscAbilityData4.TreatAsMelee == false)
				{
					return false;
				}
				return base.AreaEffect.Melee;
			}
			if (base.TargetingActor != null && base.TargetingActor.IsOriginalMonsterType && m_Range <= 1)
			{
				if ((base.TargetingActor.Class as CMonsterClass).Range > 1)
				{
					AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
					if (miscAbilityData5 == null)
					{
						return false;
					}
					return miscAbilityData5.TreatAsMelee == true;
				}
				return true;
			}
			if (m_Range > 1)
			{
				AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
				if (miscAbilityData6 == null)
				{
					return false;
				}
				return miscAbilityData6.TreatAsMelee == true;
			}
			return true;
		}
	}

	public bool IsAttackType(EAttackType attackType)
	{
		return attackType switch
		{
			EAttackType.Melee => IsMeleeAttack, 
			EAttackType.Ranged => !IsMeleeAttack, 
			EAttackType.Default => IsDefaultAttack, 
			EAttackType.Control => base.IsControlAbility, 
			_ => true, 
		};
	}

	public List<EAttackType> AttackIsAttackTypes()
	{
		List<EAttackType> list = new List<EAttackType> { EAttackType.Attack };
		if (IsMeleeAttack)
		{
			list.Add(EAttackType.Melee);
		}
		else
		{
			list.Add(EAttackType.Ranged);
		}
		if (IsDefaultAttack)
		{
			list.Add(EAttackType.Default);
		}
		if (base.IsControlAbility)
		{
			list.Add(EAttackType.Control);
		}
		return list;
	}

	public CAbilityAttack(bool multiPassAttack, bool chainAttack, int pierce, bool isDefaultAttack, List<CAttackEffect> attackEffects, int chainAttackRange, int chainAttackDamageReduction, int damageSelfBeforeAttack)
	{
		m_MultiPassAttack = multiPassAttack;
		m_ChainAttack = chainAttack;
		m_ChainAttackRange = chainAttackRange;
		m_ChainAttackDamageReduction = chainAttackDamageReduction;
		Pierce = pierce;
		DamageSelfBeforeAttack = damageSelfBeforeAttack;
		m_IsDefaultAttack = isDefaultAttack;
		AttackEffects = attackEffects ?? new List<CAttackEffect>();
		m_ModifierAbilities = new List<CAbility>();
		m_PreviousAttackModsForTarget = new List<AttackModifierYMLData>();
	}

	public static CAbility CreateDefaultAttack(int strength, int range, int numberOfTargets, bool isMonster, string animationOverload = "", bool isMultipass = true, CAbilityFilterContainer abilityfilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.Attack, strength, useSpecialBaseStat: false, range, numberOfTargets, (abilityfilter == null) ? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy) : abilityfilter, animationOverload, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "DefaultAttack", new List<CEnhancement>(), null, null, isMultipass, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, null, null, EAbilityTargeting.Range, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, new List<EAbilityType>(), new List<EAttackType>(), null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: true);
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		if (targetingActor.Class is CMonsterClass cMonsterClass && cMonsterClass.StatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Attack) && !base.StrengthIsBase)
		{
			SetStatBasedOnX(targetingActor, cMonsterClass.StatIsBasedOnXEntries, base.AbilityFilter);
		}
		if (base.Augment != null && !m_AugmentsAdded)
		{
			targetingActor.AddAugmentOrSong(this, targetingActor);
			m_AugmentsAdded = true;
		}
		targetingActor.AIMoveFocusActors?.RemoveAll((CActor a) => a?.IsDead ?? true);
		targetingActor.AIMoveFocusActors?.RemoveAll((CActor a) => a.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
		List<EAttackType> overrideAttackTypes = new List<EAttackType>();
		foreach (CActiveBonus item2 in targetingActor.FindApplicableActiveBonuses(EAbilityType.OverrideAugmentAttackType))
		{
			if (item2.Ability is CAbilityOverrideAugmentAttackType cAbilityOverrideAugmentAttackType && !overrideAttackTypes.Contains(cAbilityOverrideAugmentAttackType.OverrideAttackType))
			{
				overrideAttackTypes.Add(cAbilityOverrideAugmentAttackType.OverrideAttackType);
			}
		}
		m_AbilityAugments = targetingActor.Augments.Where((CAugment w) => w.AugmentType == CAugment.EAugmentType.Ability && (w.AttackType == EAttackType.Attack || overrideAttackTypes.Contains(EAttackType.Attack) || ((w.AttackType == EAttackType.Melee || overrideAttackTypes.Contains(EAttackType.Melee)) && IsMeleeAttack) || ((w.AttackType == EAttackType.Ranged || overrideAttackTypes.Contains(EAttackType.Ranged)) && !IsMeleeAttack))).ToList();
		m_OverrideAugments = targetingActor.Augments.Where((CAugment w) => w.AugmentType == CAugment.EAugmentType.Override && (w.AttackType == EAttackType.Attack || overrideAttackTypes.Contains(EAttackType.Attack) || ((w.AttackType == EAttackType.Melee || overrideAttackTypes.Contains(EAttackType.Melee)) && IsMeleeAttack) || ((w.AttackType == EAttackType.Ranged || overrideAttackTypes.Contains(EAttackType.Ranged)) && !IsMeleeAttack))).ToList();
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(targetingActor, EAbilityType.Attack);
		list.AddRange(CActiveBonus.FindApplicableActiveBonuses(targetingActor, EAbilityType.AddActiveBonus));
		if (list.Count > 0 && overrideAttackTypes.Count > 0)
		{
			EAttackType attackType = overrideAttackTypes.First();
			for (int num = 0; num < list.Count; num++)
			{
				if (list[num].BaseCard.Name.Equals("ABILITY_CARD_FrozenMind") && list[num].Ability?.Augment != null)
				{
					list[num].Ability.Augment.AttackType = attackType;
					list[num].Ability.ActiveBonusData.AttackType = attackType;
					list[num].Duration = CActiveBonus.EActiveBonusDurationType.Round;
				}
			}
		}
		if ((base.ActiveBonusData == null || base.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA) && !m_AugmentOverridesProcessed && m_OverrideAugments.Count > 0)
		{
			foreach (CAugment overrideAugment in m_OverrideAugments)
			{
				foreach (CAbilityOverride abilityOverride in overrideAugment.AbilityOverrides)
				{
					OverrideAbilityValues(abilityOverride, perform: false);
				}
			}
		}
		m_AugmentOverridesProcessed = true;
		base.Start(targetingActor, filterActor, controllingActor);
		m_AugmentAbilitiesTargetCount = 0;
		m_CurrentAttackIndex = 0;
		m_CurrentPassAttackIndex = 0;
		m_ModifierAbilities = new List<CAbility>();
		m_PreviousAttackModsForTarget = new List<AttackModifierYMLData>();
		m_FirstAttackofAction = true;
		m_ModifiersChecked = false;
		m_InlineModifiersChecked = false;
		m_State = EAttackState.SelectAttackFocus;
		if (m_MultiPassAttack && base.AllTargetsOnMovePath)
		{
			m_MultiPassAttack = false;
		}
		if (m_MultiPassAttack)
		{
			m_MultiPassAttackStarted = false;
		}
		if (base.CanUndoOverride.HasValue)
		{
			m_CanUndo = base.CanUndoOverride.Value;
		}
		else
		{
			CPhase phase = PhaseManager.Phase;
			if (phase != null && phase.Type == CPhase.PhaseType.Action)
			{
				CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
				if (cPhaseAction.PreviousPhaseAbilities != null && cPhaseAction.PreviousPhaseAbilities.Count > 0)
				{
					m_CanUndo = !cPhaseAction.PreviousPhaseAbilities.Last().IsCostAbility;
				}
			}
		}
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (base.UseSubAbilityTargeting)
		{
			AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.IgnoreParentAreaOfEffect.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.IgnoreParentAreaOfEffect.Value)
				{
					base.AreaEffect = null;
					goto IL_049c;
				}
			}
			if (base.AreaEffect == null && base.ParentAbility?.AreaEffect != null)
			{
				base.AreaEffect = base.ParentAbility.AreaEffect.Copy();
			}
			goto IL_049c;
		}
		if (base.IsMergedAbility)
		{
			AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
			if (miscAbilityData3 == null || !miscAbilityData3.IgnoreMergedAbilityTargetSelection.HasValue)
			{
				if (base.ParentAbility != null && base.ParentAbility is CAbilityMerged cAbilityMerged)
				{
					CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
					if (mergedWithAbility != null && base.AllTargetsOnMovePath && mergedWithAbility is CAbilityMove)
					{
						SharedAbilityTargeting.GetValidActorsInRange(this);
						base.TilesInRange.Clear();
						foreach (CActor item3 in base.ValidActorsInRange)
						{
							CTile item = ScenarioManager.Tiles[item3.ArrayIndex.X, item3.ArrayIndex.Y];
							base.TilesInRange.Add(item);
							m_ActorsToTarget.Add(item3);
						}
						if (m_ActorsToTarget.Count <= 0)
						{
							m_CancelAbility = true;
						}
					}
					else if (mergedWithAbility != null && mergedWithAbility.TilesSelected != null && mergedWithAbility.TilesSelected.Count > 0)
					{
						base.TilesInRange.Clear();
						foreach (CTile item4 in mergedWithAbility.TilesSelected)
						{
							AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
							if (miscAbilityData4 != null && miscAbilityData4.UseMergedWithAbilityTiles.HasValue)
							{
								AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
								if (miscAbilityData5 != null && miscAbilityData5.UseMergedWithAbilityTiles.Value)
								{
									base.TilesInRange.Add(item4);
									goto IL_0ba8;
								}
							}
							CActor cActor = ScenarioManager.Scenario.FindActorAt(item4.m_ArrayIndex);
							if (cActor != null)
							{
								m_ActorsToIgnore.Add(cActor);
							}
							base.TilesInRange.AddRange(GameState.GetTilesInRange(item4.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
							goto IL_0ba8;
							IL_0ba8:
							foreach (CActor item5 in GameState.GetActorsInRange(item4.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible))
							{
								if (!m_ValidActorsInRange.Contains(item5))
								{
									m_ValidActorsInRange.Add(item5);
								}
							}
						}
						if (m_ValidActorsInRange.Count <= 0)
						{
							m_CancelAbility = true;
						}
					}
				}
				else
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
				RemoveImmuneActorsFromList(ref m_ActorsToTarget);
				if (m_NumberTargets != -1)
				{
					m_NumberTargets = base.ValidActorsInRange.Count;
				}
				goto IL_0cb5;
			}
		}
		SharedAbilityTargeting.GetValidActorsInRange(this);
		goto IL_0cb5;
		IL_049c:
		if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
		{
			base.TilesInRange = base.InlineSubAbilityTiles.ToList();
			foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
			{
				CActor cActor2 = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
				if (cActor2 != null && !m_ValidActorsInRange.Contains(cActor2) && base.AbilityFilter.IsValidTarget(cActor2, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, false))
				{
					m_ValidActorsInRange.Add(cActor2);
				}
			}
			m_ActorsToTarget = base.ValidActorsInRange.ToList();
		}
		else if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ValidActorsInRange = new List<CActor> { base.TargetingActor };
			m_ActorsToTarget = base.ValidActorsInRange.ToList();
		}
		else if (base.AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = base.ParentAbility.ValidTilesInAreaAffected.ToList();
			base.ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffect);
			m_ActorsToTarget = base.ValidActorsInRange.ToList();
		}
		else
		{
			AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
			if (miscAbilityData6 != null && miscAbilityData6.AllTargetsAdjacentToParentTargets.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData7 = base.MiscAbilityData;
				if (miscAbilityData7 != null && miscAbilityData7.AllTargetsAdjacentToParentTargets.Value)
				{
					m_IsSubAbility = false;
					m_AllTargets = true;
					foreach (CActor item6 in base.ParentAbility.ActorsTargeted)
					{
						if (item6 != null)
						{
							m_ActorsToIgnore.Add(item6);
						}
					}
					base.TilesInRange = GameState.GetTilesInRange(base.ParentAbility.ActorsTargeted[0].ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false);
					m_ValidActorsInRange = GameState.GetActorsInRange(base.ParentAbility.ActorsTargeted[0].ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
					goto IL_0970;
				}
			}
			if (base.AbilityFilter.Equals(base.ParentAbility.AbilityFilter) && base.ParentAbility?.ActorsTargeted != null)
			{
				base.ValidActorsInRange = base.ParentAbility.ActorsTargeted.Where((CActor x) => !x.IsDead).ToList();
				m_ActorsToTarget = m_ValidActorsInRange.ToList();
			}
			else if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
			{
				foreach (CTile item7 in base.ParentAbility.TilesSelected)
				{
					base.TilesInRange.Clear();
					AbilityData.MiscAbilityData miscAbilityData8 = base.MiscAbilityData;
					if (miscAbilityData8 != null && miscAbilityData8.UseParentTiles.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData9 = base.MiscAbilityData;
						if (miscAbilityData9 != null && miscAbilityData9.UseParentTiles.Value)
						{
							base.TilesInRange.AddRange(base.ParentAbility.TilesSelected);
							goto IL_08b4;
						}
					}
					CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item7.m_ArrayIndex);
					if (cActor3 != null)
					{
						m_ActorsToIgnore.Add(cActor3);
					}
					base.TilesInRange.AddRange(GameState.GetTilesInRange(item7.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
					goto IL_08b4;
					IL_08b4:
					foreach (CActor item8 in GameState.GetActorsInRange(item7.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible))
					{
						if (!m_ValidActorsInRange.Contains(item8))
						{
							m_ValidActorsInRange.Add(item8);
						}
					}
				}
			}
			else
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
		}
		goto IL_0970;
		IL_0cb5:
		if (m_NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
		}
		if (base.AreaEffect != null)
		{
			if (base.MiscAbilityData != null && base.MiscAbilityData.ExactRange.HasValue && base.MiscAbilityData.ExactRange.Value)
			{
				m_AreaEffectLocked = true;
			}
			if (targetingActor.Type == CActor.EType.Player)
			{
				m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(base.TargetingActor, ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y], base.AreaEffect, 0f, GameState.GetTilesInRange(targetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true), getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
			}
			else
			{
				UpdateAreaEffect(ScenarioManager.Tiles[m_AreaRangeTileX, m_AreaRangeTileY], base.AreaEffectAngle);
				base.ValidActorsInRange = GameState.GetActorsInRange(targetingActor, filterActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
				RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
				RemoveImmuneActorsFromList(ref m_ActorsToTarget);
			}
		}
		if (!base.AbilityStartListenersInvoked)
		{
			base.AbilityStartListenersInvoked = true;
			if ((base.ActiveBonusData == null || base.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA) && targetingActor.m_OnAttackStartListeners != null)
			{
				targetingActor.m_OnAttackStartListeners(this);
			}
		}
		InitAttackSummary();
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
		return;
		IL_0970:
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		RemoveImmuneActorsFromList(ref m_ActorsToTarget);
		m_NumberTargets = base.ValidActorsInRange.Count;
		if (base.ValidActorsInRange.Count <= 0)
		{
			m_CancelAbility = true;
		}
		goto IL_0cb5;
	}

	public override int ModifiedStrength()
	{
		if (m_AttackSummary != null)
		{
			return m_AttackSummary.ModifiedAttackStrength;
		}
		return m_ModifiedStrength;
	}

	private void PreProcessAttackActor(CActor actorBeingAttacked)
	{
		PreProcessTargetAbility(actorBeingAttacked);
	}

	private CTile DoorPath(CTile targetActorTile, CTile aiMoveFocusActorTile)
	{
		List<CTile> list = new List<CTile>();
		int num = Math.Max(1, m_Range);
		int num2 = ((targetActorTile.m_ArrayIndex.Y - num >= 0) ? (targetActorTile.m_ArrayIndex.Y - num) : 0);
		int num3 = ((targetActorTile.m_ArrayIndex.Y + num < ScenarioManager.Height) ? (targetActorTile.m_ArrayIndex.Y + num) : ScenarioManager.Height);
		int num4 = ((targetActorTile.m_ArrayIndex.X - num >= 0) ? (targetActorTile.m_ArrayIndex.X - num) : 0);
		int num5 = ((targetActorTile.m_ArrayIndex.X + num < ScenarioManager.Width) ? (targetActorTile.m_ArrayIndex.X + num) : ScenarioManager.Width);
		for (int i = num2; i <= num3; i++)
		{
			for (int j = num4; j <= num5; j++)
			{
				if (i < 0 || j < 0 || i >= ScenarioManager.Height || j >= ScenarioManager.Width)
				{
					continue;
				}
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(j, i, ScenarioManager.EAdjacentPosition.ECenter);
				if (adjacentTile != null)
				{
					CObjectDoor obj = (CObjectDoor)adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door);
					if (obj != null && obj.Activated)
					{
						list.Add(adjacentTile);
					}
				}
			}
		}
		CTile cTile = null;
		if (aiMoveFocusActorTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && targetActorTile.FindProp(ScenarioManager.ObjectImportType.Door) == null && aiMoveFocusActorTile.m_HexMap != targetActorTile.m_HexMap)
		{
			bool flag = false;
			foreach (CTile item in list)
			{
				if ((aiMoveFocusActorTile.m_HexMap == item.m_HexMap && targetActorTile.m_HexMap == item.m_Hex2Map) || (aiMoveFocusActorTile.m_HexMap == item.m_Hex2Map && targetActorTile.m_HexMap == item.m_HexMap))
				{
					if (cTile == null)
					{
						cTile = item;
					}
					else
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				cTile = null;
			}
		}
		return cTile;
	}

	private bool IsActorWithinRange(CTile targetActorTile, CTile aiMoveFocusActorTile)
	{
		CTile cTile = DoorPath(targetActorTile, aiMoveFocusActorTile);
		List<Point> list = null;
		bool foundPath;
		if (cTile != null)
		{
			list = ScenarioManager.PathFinder.FindPath(targetActorTile.m_ArrayIndex, cTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
			if (foundPath)
			{
				list.AddRange(ScenarioManager.PathFinder.FindPath(cTile.m_ArrayIndex, aiMoveFocusActorTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath));
			}
		}
		else
		{
			list = ScenarioManager.PathFinder.FindPath(targetActorTile.m_ArrayIndex, aiMoveFocusActorTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		}
		return list.Count <= m_Range;
	}

	private static bool AttackActor(CAbilityAttack ability, bool isMeleeAttack, CActor targetingActor, CActor filterActor, CAttackSummary.TargetSummary attackingTargetSummary, int modifiedStrength, List<CActor> actorsToIgnore, CAreaEffect areaEffect, AbilityData.XpPerTargetData xpPerTargetData, int attackIndex)
	{
		CActor actorToAttack = attackingTargetSummary.ActorToAttack;
		int num = attackingTargetSummary.ActiveBonusStrengthBuffs.Sum();
		ability.ActorsTargeted.Add(actorToAttack);
		targetingActor.ActorsAttackedThisRound.Add(actorToAttack);
		if (attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToTakeFromTargets != null && attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToTakeFromTargets.Count > 0)
		{
			foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToTakeFromTargets)
			{
				if (actorToAttack.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
				{
					actorToAttack.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
					targetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
				}
			}
		}
		if (attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToGiveToTargets != null && attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToGiveToTargets.Count > 0)
		{
			foreach (KeyValuePair<string, int> resourcesToGiveToTarget in attackingTargetSummary.AttackAbilityWithOverrides.ResourcesToGiveToTargets)
			{
				if (targetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
				{
					targetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					actorToAttack.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
				}
			}
		}
		if (ScenarioRuleClient.AttackValueOverride != int.MaxValue)
		{
			modifiedStrength = ScenarioRuleClient.AttackValueOverride;
			ScenarioRuleClient.SetNextAttackValueOverride(int.MaxValue);
		}
		ability.AbilityHasHappened = true;
		if (num > 0)
		{
			CTargetAttackBuff_MessageData cTargetAttackBuff_MessageData = new CTargetAttackBuff_MessageData(targetingActor);
			cTargetAttackBuff_MessageData.m_ActorBeingAttacked = actorToAttack;
			cTargetAttackBuff_MessageData.m_ActorAttacking = targetingActor;
			ScenarioRuleClient.MessageHandler(cTargetAttackBuff_MessageData);
		}
		if (actorToAttack.CalculateShield(ability) > 0)
		{
			CTargetShield_MessageData cTargetShield_MessageData = new CTargetShield_MessageData(targetingActor);
			cTargetShield_MessageData.m_ActorBeingAttacked = actorToAttack;
			cTargetShield_MessageData.m_ActorAttacking = targetingActor;
			ScenarioRuleClient.MessageHandler(cTargetShield_MessageData);
		}
		int health = actorToAttack.Health;
		actorToAttack.m_OnBeingAttackedListeners?.Invoke(ability, modifiedStrength);
		bool actorWasAsleep = actorToAttack.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		EAbilityType damageAbilityType = ability?.AbilityType ?? EAbilityType.None;
		GameState.ActorBeenDamaged(actorToAttack, modifiedStrength, checkIfPlayerCanAvoidDamage: true, targetingActor, ability, damageAbilityType, actorToAttack.CalculateShield(ability));
		if (targetingActor.m_OnAttackFinishedListeners != null)
		{
			targetingActor.m_OnAttackFinishedListeners?.Invoke(ability, actorToAttack, modifiedStrength);
		}
		if (xpPerTargetData != null)
		{
			targetingActor.GainXP(xpPerTargetData.AttackTarget(targetingActor, actorToAttack));
		}
		if (attackingTargetSummary != null)
		{
			foreach (int conditionalOverridesXPBuff in attackingTargetSummary.ConditionalOverridesXPBuffs)
			{
				targetingActor.GainXP(conditionalOverridesXPBuff);
			}
			targetingActor.GainXP(attackingTargetSummary.AttackEffectXP);
			targetingActor.GainXP(attackingTargetSummary.ActiveBonusXPBuffs.Sum());
			if (ability.MiscAbilityData != null && ability.MiscAbilityData.GainXPPerXDamageDealt.HasValue && ability.MiscAbilityData.GainXPPerXDamageDealt.Value > 0)
			{
				targetingActor.GainXP(attackingTargetSummary.FinalAttackStrength / ability.MiscAbilityData.GainXPPerXDamageDealt.Value);
			}
		}
		if (ScenarioManager.Scenario.HasActor(actorToAttack))
		{
			if (GameState.ActorHealthCheck(targetingActor, actorToAttack, out var onDeathAbility, isTrap: false, isTerrain: false, actorWasAsleep, attackingTargetSummary))
			{
				if (actorToAttack.OriginalType != CActor.EType.Player)
				{
					CActorBeenAttacked_MessageData message = new CActorBeenAttacked_MessageData(targetingActor)
					{
						m_AttackingActor = targetingActor,
						m_ActorBeingAttacked = actorToAttack,
						AreaEffect = areaEffect,
						m_ActorOriginalHealth = health,
						m_AttackAbility = ability,
						m_AttackIndex = attackIndex,
						m_ActorWasAsleep = actorWasAsleep
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				actorsToIgnore?.Add(actorToAttack);
			}
			else if (actorToAttack.OriginalType != CActor.EType.Player)
			{
				CActorBeenAttackedAndKilled_MessageData message2 = new CActorBeenAttackedAndKilled_MessageData(targetingActor)
				{
					m_AttackingActor = targetingActor,
					m_ActorBeingAttacked = actorToAttack,
					AreaEffect = areaEffect,
					m_ActorOriginalHealth = health,
					m_AttackAbility = ability,
					m_AttackIndex = attackIndex
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			return onDeathAbility;
		}
		return false;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (!base.ProcessIfDead)
		{
			if (!base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm) || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
				{
					AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
					if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
					{
						goto IL_0097;
					}
				}
				if (!base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
				{
					goto IL_013f;
				}
			}
			goto IL_0097;
		}
		goto IL_013f;
		IL_013f:
		if (m_CancelAbility)
		{
			if (m_IsMergedAbility)
			{
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		bool flag;
		switch (m_State)
		{
		case EAttackState.SelectAttackFocus:
		{
			if (m_AllTargets || m_ActorsToTarget.Count > 0)
			{
				base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
			}
			else
			{
				base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			}
			if (base.UseSubAbilityTargeting)
			{
				if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
				{
					m_State = EAttackState.AttackBuff;
					Perform();
					break;
				}
				if (base.AreaEffect == null)
				{
					base.AreaEffect = new CAreaEffect("SubAbilityAttackArea", IsMeleeAttack, new List<CAreaEffect.CAreaEffectHex>(), new List<CAreaEffect.CAreaEffectHex>());
					m_ValidTilesInAreaEffect = base.ValidActorsInRange.Select((CActor s) => ScenarioManager.GetAdjacentTile(s.ArrayIndex.X, s.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter)).ToList();
				}
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
				}
				m_CanUndo = false;
				if (base.TargetingActor.Type.Equals(CActor.EType.Player))
				{
					CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData2 = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
					cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackingActor = base.TargetingActor;
					cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackAbility = this;
					cActorIsSelectingAttackFocusTargets_MessageData2.m_AttackSummary = AttackSummary.Copy();
					ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData2);
				}
				else
				{
					m_State = EAttackState.PreActorIsAttacking;
					Perform();
				}
				break;
			}
			if (m_IsMergedAbility)
			{
				if (m_AllTargets)
				{
					m_ActorsToTarget = base.ValidActorsInRange.ToList();
				}
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
				}
				if (base.TargetingActor.Type.Equals(CActor.EType.Player))
				{
					CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData3 = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
					cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackingActor = base.TargetingActor;
					cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackAbility = this;
					cActorIsSelectingAttackFocusTargets_MessageData3.m_AttackSummary = AttackSummary.Copy();
					ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData3);
				}
				break;
			}
			AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
			if (miscAbilityData4 != null && miscAbilityData4.AutotriggerAbility == true && base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				m_State = EAttackState.AttackBuff;
				m_ActorsToTarget.AddRange(base.ValidActorsInRange);
				Perform();
				break;
			}
			if (base.TargetingActor.Type != CActor.EType.Player || base.AllTargetsOnMovePath)
			{
				if (base.TargetingActor.Type != CActor.EType.Player && !base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
				{
					if ((base.TargetingActor.Type != CActor.EType.HeroSummon || base.ActiveBonusData == null || !base.ActiveBonusData.IsAura) && base.TargetingActor.AIMoveFocusActors.Count == 0)
					{
						base.TargetingActor.Move(0, jump: false, fly: false, m_Range, allowMove: false, ignoreDifficultTerrain: false, this, firstMove: true);
						if (base.AreaEffect != null && !base.IsSubAbility)
						{
							UpdateAreaEffect(ScenarioManager.Tiles[m_AreaRangeTileX, m_AreaRangeTileY], base.AreaEffectAngle);
							base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor, base.FilterActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
							RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
						}
					}
					base.TargetingActor.AIMoveFocusActors.RemoveAll((CActor x) => x == null || x.IsDead || x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
					base.ValidActorsInRange.RemoveAll((CActor x) => x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
					base.ValidActorsInRange.RemoveAll((CActor x) => x is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.PropHealthDetails != null && cObjectActor.AttachedProp.PropHealthDetails.IgnoredByAIFocus);
					if (ChainAttack)
					{
						if (base.TargetingActor.AIMoveFocusActors.Count <= 0)
						{
							CInvalidAttack_MessageData cInvalidAttack_MessageData = new CInvalidAttack_MessageData(base.TargetingActor);
							cInvalidAttack_MessageData.m_AttackingActor = base.TargetingActor;
							ScenarioRuleClient.MessageHandler(cInvalidAttack_MessageData);
							base.TargetingActor.AIMoveFocusActors?.Clear();
							PhaseManager.NextStep();
							return true;
						}
						CActor cActor4 = base.TargetingActor;
						int num10 = m_Range;
						CTile sourceTile = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
						for (int num11 = 0; num11 < m_TotalNumberTargets; num11++)
						{
							bool flag4 = false;
							foreach (CActor aIMoveFocusActor in base.TargetingActor.AIMoveFocusActors)
							{
								if (aIMoveFocusActor != null)
								{
									CTile targetTile = ScenarioManager.Tiles[aIMoveFocusActor.ArrayIndex.X, aIMoveFocusActor.ArrayIndex.Y];
									if (CActor.HaveLOS(sourceTile, targetTile) && base.ValidActorsInRange.Contains(aIMoveFocusActor) && !m_ActorsToIgnore.Contains(aIMoveFocusActor) && !m_ActorsToTarget.Contains(aIMoveFocusActor))
									{
										m_ActorsToTarget.Add(aIMoveFocusActor);
										cActor4 = aIMoveFocusActor;
										flag4 = true;
										break;
									}
								}
							}
							if (!flag4 || num11 + 1 >= m_TotalNumberTargets)
							{
								break;
							}
							num10 += ChainAttackRange;
							base.TargetingActor.AIMoveFocusActors.Clear();
							base.TargetingActor.Move(0, jump: false, fly: false, num10, allowMove: false, ignoreDifficultTerrain: false, this, firstMove: true);
							base.ValidActorsInRange = GameState.GetActorsInRange(cActor4.ArrayIndex, base.FilterActor, ChainAttackRange, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
						}
					}
					else
					{
						CActor cActor5 = ((base.TargetingActor.AIMoveFocusActors.Count == 0) ? null : base.TargetingActor.AIMoveFocusActors[0]);
						CTile cTile = ((cActor5 == null) ? null : ScenarioManager.Tiles[cActor5.ArrayIndex.X, cActor5.ArrayIndex.Y]);
						CTile cTile2 = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
						CActor item2 = ((cActor5 != null && CActor.HaveLOS(cTile2, cTile) && !m_ActorsToIgnore.Contains(cActor5) && IsActorWithinRange(cTile2, cTile)) ? cActor5 : ((base.ValidActorsInRange.Count == 0) ? null : base.ValidActorsInRange[0]));
						if (!base.ValidActorsInRange.Contains(item2))
						{
							CInvalidAttack_MessageData cInvalidAttack_MessageData2 = new CInvalidAttack_MessageData(base.TargetingActor);
							cInvalidAttack_MessageData2.m_AttackingActor = base.TargetingActor;
							ScenarioRuleClient.MessageHandler(cInvalidAttack_MessageData2);
							base.TargetingActor.AIMoveFocusActors?.Clear();
							PhaseManager.NextStep();
							return true;
						}
						if (m_AllTargets || base.AreaEffect != null)
						{
							m_ActorsToTarget = base.ValidActorsInRange.ToList();
						}
						else
						{
							m_ActorsToTarget.Clear();
							if (base.TargetingActor.AIMoveFocusActors.Count == 0)
							{
								m_ActorsToTarget.Add(item2);
								SimpleLog.AddToSimpleLog("No AIMoveFocusActors - selecting the first actor in ValidActorsInRange list to attack instead");
							}
							else
							{
								foreach (CActor aIMoveFocusActor2 in base.TargetingActor.AIMoveFocusActors)
								{
									if (aIMoveFocusActor2 == null)
									{
										continue;
									}
									cTile = ScenarioManager.Tiles[aIMoveFocusActor2.ArrayIndex.X, aIMoveFocusActor2.ArrayIndex.Y];
									if (CActor.HaveLOS(cTile2, cTile) && !m_ActorsToIgnore.Contains(aIMoveFocusActor2) && IsActorWithinRange(cTile2, cTile))
									{
										AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
										if (miscAbilityData5 != null && miscAbilityData5.TargetOneEnemyWithAllAttacks.HasValue)
										{
											AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
											if (miscAbilityData6 != null && miscAbilityData6.TargetOneEnemyWithAllAttacks.Value)
											{
												m_ActorsToTarget.Clear();
												for (int num12 = 0; num12 < m_TotalNumberTargets; num12++)
												{
													m_ActorsToTarget.Add(aIMoveFocusActor2);
												}
												goto IL_0a49;
											}
										}
										m_ActorsToTarget.Add(aIMoveFocusActor2);
									}
									goto IL_0a49;
									IL_0a49:
									if (m_ActorsToTarget.Count == m_NumberTargetsRemaining)
									{
										break;
									}
								}
							}
						}
					}
				}
				else
				{
					m_ActorsToTarget = base.ValidActorsInRange.ToList();
				}
				if (m_ActorsToTarget.Count <= 0)
				{
					m_CancelAbility = true;
				}
				m_TilesSelected.Clear();
				foreach (CActor item6 in m_ActorsToTarget)
				{
					m_TilesSelected.Add(ScenarioManager.Tiles[item6.ArrayIndex.X, item6.ArrayIndex.Y]);
				}
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
				}
				CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData4 = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
				cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackingActor = base.TargetingActor;
				cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackAbility = this;
				cActorIsSelectingAttackFocusTargets_MessageData4.m_AttackSummary = AttackSummary.Copy();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData4);
				break;
			}
			if (m_AttackSummary != null && !ChainAttack)
			{
				m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
			}
			if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || m_AllTargets)
			{
				m_AllTargets = true;
				m_ActorsToTarget = base.ValidActorsInRange.ToList();
				m_TilesSelected.Clear();
				foreach (CActor item7 in m_ActorsToTarget)
				{
					m_TilesSelected.Add(ScenarioManager.Tiles[item7.ArrayIndex.X, item7.ArrayIndex.Y]);
				}
				CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(base.TargetingActor);
				cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
				cShowSingleTargetActiveBonus_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
			}
			else if (ChainAttack)
			{
				if (m_ActorsToTarget.Count > 0)
				{
					foreach (CActor item8 in m_ActorsToTarget)
					{
						if (!m_ActorsToIgnore.Contains(item8))
						{
							m_ActorsToIgnore.Add(item8);
						}
					}
					List<CActor> actorsInRange2 = GameState.GetActorsInRange(m_ActorsToTarget.Last().ArrayIndex, base.TargetingActor, m_ChainAttackRange, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
					m_ValidActorsInRange.Clear();
					foreach (CActor item9 in actorsInRange2)
					{
						if (!m_ValidActorsInRange.Contains(item9))
						{
							m_ValidActorsInRange.Add(item9);
						}
					}
					RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
					base.TilesInRange = GameState.GetTilesInRange(m_ActorsToTarget.Last(), m_ChainAttackRange, base.Targeting, emptyTilesOnly: false, !IsMeleeAttack);
					foreach (CActor item10 in m_ActorsToTarget)
					{
						CTile item3 = ScenarioManager.Tiles[item10.ArrayIndex.X, item10.ArrayIndex.Y];
						base.TilesInRange.Add(item3);
						m_ActorsToIgnore.Remove(item10);
					}
					ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
					if (m_AttackSummary != null)
					{
						m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
					}
				}
				else
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
					if (m_AttackSummary != null)
					{
						m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
					}
				}
			}
			CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData5 = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
			cActorIsSelectingAttackFocusTargets_MessageData5.m_AttackingActor = base.TargetingActor;
			cActorIsSelectingAttackFocusTargets_MessageData5.m_AttackAbility = this;
			cActorIsSelectingAttackFocusTargets_MessageData5.m_AttackSummary = AttackSummary.Copy();
			ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData5);
			break;
		}
		case EAttackState.PreActorIsAttacking:
			if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				m_State = EAttackState.AttackBuff;
				Perform();
				break;
			}
			if (base.AreaEffect == null)
			{
				if (base.TargetingActor.IsMonsterType && m_MultiPassAttack && (m_NumberTargetsRemaining > 1 || m_AllTargets) && base.ValidActorsInRange.Count > 1)
				{
					if (!m_AllTargets)
					{
						while (m_ActorsToTarget.Count > m_NumberTargetsRemaining)
						{
							m_ActorsToTarget.Remove(m_ActorsToTarget[base.ValidActorsInRange.Count - 1]);
						}
					}
				}
				else
				{
					if (ChainAttack)
					{
						goto IL_0ffb;
					}
					AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.TargetOneEnemyWithAllAttacks.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
						if (miscAbilityData3 != null && miscAbilityData3.TargetOneEnemyWithAllAttacks.Value)
						{
							goto IL_0ffb;
						}
					}
				}
			}
			goto IL_101d;
		case EAttackState.SelectActiveBonusBuffTarget:
			if (base.TargetingActor.Type.Equals(CActor.EType.Player))
			{
				CActorIsSelectingActiveBonusBuffTarget_MessageData cActorIsSelectingActiveBonusBuffTarget_MessageData = new CActorIsSelectingActiveBonusBuffTarget_MessageData(base.TargetingActor);
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_AttackingActor = base.TargetingActor;
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_AttackAbility = this;
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_ActiveBonusName = m_CurrentStackedInlineAbilityActiveBonus.BaseCard.Name;
				cActorIsSelectingActiveBonusBuffTarget_MessageData.m_BaseCardID = m_CurrentStackedInlineAbilityActiveBonus.BaseCard.ID.ToString();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingActiveBonusBuffTarget_MessageData);
			}
			break;
		case EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility:
			if (base.TargetingActor.Type.Equals(CActor.EType.Player))
			{
				m_CanUndo = false;
				if (m_AttackSummary != null)
				{
					m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
				}
				CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(base.TargetingActor);
				cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = base.TargetingActor;
				cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = this;
				cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = m_AttackSummary?.Copy();
				ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
			}
			m_State = EAttackState.CheckForDamageSelf;
			Perform();
			break;
		case EAttackState.CheckForDamageSelf:
			if (m_FirstAttackofAction && DamageSelfBeforeAttack > 0)
			{
				int health = base.TargetingActor.Health;
				bool actorWasAsleep = base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(base.TargetingActor, DamageSelfBeforeAttack, checkIfPlayerCanAvoidDamage: true, base.TargetingActor, this, base.AbilityType);
				if (!GameState.ActorHealthCheck(base.TargetingActor, base.TargetingActor, out var _, isTrap: false, isTerrain: false, actorWasAsleep))
				{
					m_CancelAbility = true;
					if (base.IsMergedAbility)
					{
						PhaseManager.StepComplete();
					}
					else
					{
						PhaseManager.NextStep();
					}
					break;
				}
				CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(base.TargetingActor);
				cActorBeenDamaged_MessageData.m_ActorBeingDamaged = base.TargetingActor;
				cActorBeenDamaged_MessageData.m_DamageAbility = null;
				cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
				cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
				ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
			}
			PhaseManager.StepComplete();
			break;
		case EAttackState.DrawingModifiers:
		{
			List<CActor> list2 = new List<CActor>(m_ActorsToTarget);
			for (int num4 = 0; num4 < m_ActorsToTarget.Count; num4++)
			{
				if (ActorsToTargetModifiedStrength.Count > num4)
				{
					continue;
				}
				CActor cActor = list2[num4];
				foreach (CActor item11 in GameState.GetActorsInRange(cActor, cActor, 1, null, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Ally), null, null, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible))
				{
					CActor cActor2 = item11.ApplyRedirector(this, cActor);
					if (cActor2 != cActor)
					{
						m_ActorsToTarget[num4] = cActor2;
						if (m_AttackSummary != null)
						{
							m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
						}
					}
				}
			}
			if (m_ActorsToTarget.Count <= 0)
			{
				break;
			}
			int targetIndex = 0;
			int num5 = 0;
			for (int num6 = 0; num6 < m_ActorsToTarget.Count; num6++)
			{
				if (ActorsToTargetModifiedStrength.Count > num6)
				{
					targetIndex++;
					continue;
				}
				CActor cActor3 = m_ActorsToTarget[num6];
				if (base.TargetingActor.m_OnAttackingListeners != null)
				{
					base.TargetingActor.m_OnAttackingListeners?.Invoke(this, cActor3);
				}
				if (cActor3.m_OnBeingAttackedPreDamageListeners != null)
				{
					cActor3.m_OnBeingAttackedPreDamageListeners?.Invoke(this);
				}
				foreach (CAttackSummary.TargetSummary target in m_AttackSummary.Targets)
				{
					foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in base.PositiveConditions)
					{
						if (!target.AttackAbilityWithOverrides.PositiveConditions.ContainsKey(positiveCondition.Key))
						{
							target.AttackAbilityWithOverrides.PositiveConditions.Add(positiveCondition.Key, positiveCondition.Value);
						}
					}
					foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in base.NegativeConditions)
					{
						if (!target.AttackAbilityWithOverrides.NegativeConditions.ContainsKey(negativeCondition.Key))
						{
							target.AttackAbilityWithOverrides.NegativeConditions.Add(negativeCondition.Key, negativeCondition.Value);
						}
					}
					if (base.TargetingActor is CHeroSummonActor cHeroSummonActor2 && cHeroSummonActor2.SummonData?.OnAttackConditions != null)
					{
						foreach (CCondition.ENegativeCondition condition in cHeroSummonActor2.SummonData.OnAttackConditions)
						{
							if (!target.AttackAbilityWithOverrides.NegativeConditions.ContainsKey(condition))
							{
								EAbilityType abilityType = CAbility.AbilityTypes.Single((EAbilityType x) => x.ToString() == condition.ToString());
								target.AttackAbilityWithOverrides.NegativeConditions.Add(condition, CAbility.CreateAbility(abilityType, target.AttackAbilityWithOverrides.AbilityFilter, target.AttackAbilityWithOverrides.IsMonsterAbility, target.AttackAbilityWithOverrides.IsTargetedAbility));
							}
						}
					}
					if (!target.AttackersGainDisadvantage)
					{
						continue;
					}
					List<CActiveBonus> list3 = CActiveBonus.FindApplicableActiveBonuses(cActor3, EAbilityType.AttackersGainDisadvantage);
					if (list3.Count > 0)
					{
						foreach (CAttackersGainDisadvantageActiveBonus item12 in list3)
						{
							item12.GainDisadvantageTriggered();
						}
					}
					list3 = CActiveBonus.FindApplicableActiveBonuses(cActor3, EAbilityType.Shield).FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.BespokeBehaviour != null && (x.BespokeBehaviour is CShieldActiveBonus_BuffShield || x.BespokeBehaviour is CShieldActiveBonus_ShieldAndDisadvantage || x.BespokeBehaviour is CShieldActiveBonus_ShieldAndRetaliate)).ToList();
				}
				CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(cActor3, ref targetIndex);
				if (targetSummary == null)
				{
					DLLDebug.LogError("Error: Unable to find target summary for actor " + cActor3.GetPrefabName());
					continue;
				}
				bool flag3 = false;
				if ((targetSummary.AttackersGainDisadvantage || targetSummary.DisadvantageFromAdjacent) && !base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disadvantage))
				{
					base.TargetingActor.Tokens.AddNegativeToken(CCondition.ENegativeCondition.Disadvantage, 0, EConditionDecTrigger.None, base.TargetingActor);
					flag3 = true;
				}
				int addTargetsDrawn = 0;
				targetSummary.DrawAttackModifiers(base.TargetingActor, targetSummary.ActorToAttack, (CAbilityAttack)targetSummary.AttackAbilityWithOverrides, Math.Max(0, targetIndex - 1), out addTargetsDrawn);
				num5 += addTargetsDrawn;
				if (flag3)
				{
					base.TargetingActor.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Disadvantage);
				}
				if (targetSummary.Poison)
				{
					CPoisonTriggered_MessageData message2 = new CPoisonTriggered_MessageData(base.TargetingActor)
					{
						m_PoisonedActor = cActor3,
						m_AttackingActor = base.TargetingActor
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				ActorsToTargetModifiedStrength.Add(targetSummary.FinalAttackStrength);
			}
			foreach (CActor item13 in m_ActorsToTarget)
			{
				if (!m_ActorsToTargetLocked.Contains(item13))
				{
					m_ActorsToTargetLocked.Add(item13);
				}
			}
			if (num5 > 0)
			{
				if (base.AreaEffect != null || base.AreaEffectBackup != null)
				{
					if (base.AreaEffect != null)
					{
						m_AreaEffectBackup = base.AreaEffect;
						base.AreaEffect = null;
						m_ActorsToTarget = new List<CActor>(base.ValidActorsInRange);
						m_ActorsToIgnore = new List<CActor>(base.ValidActorsInRange);
						base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor, base.FilterActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, null, base.AllPossibleTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
						RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
						foreach (CActor item14 in base.ValidActorsInRange)
						{
							AttackSummary.UpdateSingleTargetData(this, item14);
						}
						m_TotalNumberTargets = m_ActorsToTarget.Count;
						m_NumberTargetsRemaining = 0;
					}
				}
				else if (ChainAttack)
				{
					foreach (CActor item15 in m_ActorsToTarget)
					{
						if (!m_ActorsToIgnore.Contains(item15))
						{
							m_ActorsToIgnore.Add(item15);
						}
					}
					List<CActor> actorsInRange = GameState.GetActorsInRange(m_ActorsToTarget.Last().ArrayIndex, base.TargetingActor, m_ChainAttackRange, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
					m_ValidActorsInRange.Clear();
					foreach (CActor item16 in actorsInRange)
					{
						if (!m_ValidActorsInRange.Contains(item16))
						{
							m_ValidActorsInRange.Add(item16);
						}
					}
					RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
					base.TilesInRange = GameState.GetTilesInRange(m_ActorsToTarget.Last(), m_ChainAttackRange, base.Targeting, emptyTilesOnly: false, !IsMeleeAttack);
					foreach (CActor item17 in m_ActorsToTarget)
					{
						CTile item = ScenarioManager.Tiles[item17.ArrayIndex.X, item17.ArrayIndex.Y];
						base.TilesInRange.Add(item);
						m_ActorsToIgnore.Remove(item17);
					}
				}
				if (base.ValidActorsInRange.Any((CActor x) => !m_ActorsToTarget.Contains(x)))
				{
					m_TotalNumberTargets += num5;
					m_NumberTargetsRemaining += num5;
					m_State = EAttackState.SelectAttackFocusAdditionalTargets;
					int targetIndex2 = 0;
					int num7 = base.ValidActorsInRange.Count((CActor x) => !m_ActorsToTarget.Contains(x));
					for (int num8 = 0; num8 < m_ActorsToTargetLocked.Count; num8++)
					{
						CActor actor = m_ActorsToTargetLocked[num8];
						CAttackSummary.TargetSummary targetSummary2 = AttackSummary.FindTarget(actor, ref targetIndex2);
						int num9 = targetSummary2.UsedAttackMods.Count((AttackModifierYMLData x) => x.AddTarget);
						if (num7 <= 0)
						{
							break;
						}
						num7 -= num9;
						targetSummary2.AddedTargetFromAttackModSuccessfully = true;
					}
					Perform();
					break;
				}
			}
			if (m_MultiPassAttack)
			{
				m_MultiPassActors = new List<CActor>(m_ActorsToTarget);
			}
			m_State = EAttackState.ActorIsAttacking;
			Perform();
			break;
		}
		case EAttackState.ActorIsAttacking:
			if (!m_AugmentAbilitiesProcessed && m_AbilityAugments.Count > 0)
			{
				if (m_MultiPassAttack)
				{
					if (m_AugmentAbilitiesNextTarget)
					{
						m_AugmentAbilitiesNextTarget = false;
					}
					else
					{
						if (m_AugmentAbilitiesTargetCount == 0)
						{
							m_AugmentAbilitiesTargetCount = m_MultiPassActors.Count;
						}
						m_AugmentAbilitiesTargetCount--;
						if (m_AugmentAbilitiesTargetCount <= 0)
						{
							m_AugmentAbilitiesProcessed = true;
						}
						m_AugmentAbilitiesNextTarget = true;
						if (!ProcessAbilityAugments())
						{
							m_AugmentAbilitiesProcessed = true;
						}
					}
				}
				else
				{
					m_AugmentAbilitiesProcessed = true;
					bool flag9 = false;
					for (int num15 = 0; num15 < base.ActorsToTarget.Count; num15++)
					{
						flag9 |= ProcessAbilityAugments();
					}
				}
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				if (!m_MultiPassAttack || !m_MultiPassAttackStarted)
				{
					ScenarioRuleClient.FirstAbilityStarted();
				}
			}
			if (m_MultiPassAttack && m_MultiPassActors.Count > 0)
			{
				m_MultiPassAttackStarted = true;
				m_CurrentPassAttackIndex = 0;
				m_AttackingActors.Clear();
				m_AttackingActors.Add(m_MultiPassActors[0]);
				m_MultiPassActors.Remove(m_MultiPassActors[0]);
			}
			else
			{
				m_AttackingActors = m_ActorsToTarget.ToList();
			}
			if (m_AttackingActors.Count > 0)
			{
				CActorIsAttacking_MessageData cActorIsAttacking_MessageData = new CActorIsAttacking_MessageData(base.TargetingActor);
				cActorIsAttacking_MessageData.m_AttackAbility = this;
				cActorIsAttacking_MessageData.m_AttackingActor = base.TargetingActor;
				cActorIsAttacking_MessageData.m_ActorsAttacking = m_AttackingActors.ToList();
				cActorIsAttacking_MessageData.m_AttackSummary = AttackSummary.Copy();
				ScenarioRuleClient.MessageHandler(cActorIsAttacking_MessageData);
			}
			else
			{
				m_State = EAttackState.FinaliseAttack;
				Perform();
			}
			break;
		case EAttackState.ActorHasAttacked:
		{
			bool flag2 = false;
			for (int num3 = 0; num3 < m_AttackingActors.Count; num3++)
			{
				PreProcessAttackActor(m_AttackingActors[num3]);
				if (flag2 || !IsTargetOutsideOfInitialRange(m_AttackingActors[num3]))
				{
					continue;
				}
				foreach (CActiveBonus addRangeActiveBonuse in m_AttackSummary.AddRangeActiveBonuses)
				{
					string text = addRangeActiveBonuse.ReferenceAnimOverload(this, m_AttackingActors[num3]);
					if (!string.IsNullOrEmpty(text))
					{
						base.AnimOverload = text;
						flag2 = true;
					}
				}
			}
			CActorHasAttacked_MessageData message = new CActorHasAttacked_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_AttackAbility = this,
				m_AttackingActor = base.TargetingActor
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		case EAttackState.ActorBeenAttacked:
		{
			m_FirstAttackofAction = false;
			int targetIndex3 = 0;
			for (int num13 = 0; num13 < m_AttackingActors.Count; num13++)
			{
				if (num13 < m_CurrentPassAttackIndex)
				{
					continue;
				}
				m_CurrentAttackIndex++;
				m_CurrentPassAttackIndex++;
				if (ScenarioManager.Scenario.HasActor(base.TargetingActor) || base.ProcessIfDead)
				{
					if (m_AttackingActors[num13].OriginalType == CActor.EType.Player)
					{
						bool actorWasAsleep2 = m_AttackingActors[num13].Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
						CActorBeenAttacked_MessageData message4 = new CActorBeenAttacked_MessageData(base.TargetingActor)
						{
							m_AttackingActor = base.TargetingActor,
							m_ActorBeingAttacked = m_AttackingActors[num13],
							AreaEffect = base.AreaEffect,
							m_ActorOriginalHealth = m_AttackingActors[num13].Health,
							m_AttackAbility = this,
							m_AttackIndex = m_CurrentAttackIndex - 1,
							m_ActorWasAsleep = actorWasAsleep2
						};
						ScenarioRuleClient.MessageHandler(message4);
					}
					CurrentAttackingTargetSummary = m_AttackSummary.FindTarget(m_AttackingActors[num13], ref targetIndex3);
					if (AttackActor(this, IsMeleeAttack, base.TargetingActor, base.FilterActor, CurrentAttackingTargetSummary, ActorsToTargetModifiedStrength[num13], m_ActorsToIgnore, base.AreaEffect, base.XpPerTargetData, m_CurrentAttackIndex - 1))
					{
						return true;
					}
				}
			}
			m_ProcessedStatusEffects = false;
			if (m_MultiPassAttack)
			{
				targetIndex3 = 0;
				CAttackSummary.TargetSummary targetSummary3 = m_AttackSummary.FindTarget(m_AttackingActors[0], ref targetIndex3);
				if (targetSummary3 != null)
				{
					CAbilityAttack cAbilityAttack = targetSummary3.AttackAbilityWithOverrides as CAbilityAttack;
					if (ScenarioManager.Scenario.HasActor(m_AttackingActors[0]))
					{
						CActor cActor7 = ((base.OriginalTargetingActor != null) ? base.OriginalTargetingActor : base.TargetingActor);
						bool num14 = base.SubAbilities == null || base.SubAbilities.Count == 0 || base.SubAbilities.All((CAbility a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull);
						bool flag5 = cAbilityAttack.SubAbilities == null || cAbilityAttack.SubAbilities.Count == 0 || cAbilityAttack.SubAbilities.All((CAbility a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull);
						bool flag6 = cAbilityAttack.ModifierAbilities.Count == 0 || cAbilityAttack.ModifierAbilities.All((CAbility a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull);
						bool flag7 = cAbilityAttack.CurrentOverrides.Count == 0 || cAbilityAttack.CurrentOverrides.All((CAbilityOverride a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull && (a.SubAbilities == null || a.SubAbilities.Count == 0 || a.SubAbilities.All((CAbility b) => b.AbilityType != EAbilityType.Push && b.AbilityType != EAbilityType.Pull)));
						if (num14 && flag5 && flag6 && flag7 && ScenarioManager.Scenario.HasActor(cActor7))
						{
							cActor7.ApplyRetaliateToAttack(m_AttackingActors[0], this);
						}
						if (targetSummary3.AttackAbilityWithOverrides.NegativeConditions.Count > 0)
						{
							targetSummary3.AttackAbilityWithOverrides.ProcessNegativeStatusEffects(m_AttackingActors[0]);
						}
					}
					if (ScenarioManager.Scenario.HasActor(base.TargetingActor) && targetSummary3 != null && targetSummary3.AttackAbilityWithOverrides.PositiveConditions.Count > 0)
					{
						targetSummary3.AttackAbilityWithOverrides.ProcessPositiveStatusEffects(base.TargetingActor);
					}
					if (targetSummary3.Actor.m_OnBeingAttackedPostDamageListeners != null)
					{
						targetSummary3.Actor.m_OnBeingAttackedPostDamageListeners?.Invoke(this);
					}
					CheckModifierAbilities(targetSummary3);
					if (cAbilityAttack.SubAbilities.Count > 0 && cAbilityAttack.SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
					{
						List<CAbility> list4 = new List<CAbility>();
						foreach (CAbility item18 in cAbilityAttack.SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
						{
							if (item18.MiscAbilityData == null || !item18.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item18.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || m_AttackingActors[0].IsDead)
							{
								CAbility cAbility = CAbility.CopyAbility(item18, generateNewID: true, fullCopy: true);
								cAbility.ParentAbility = this;
								cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[m_AttackingActors[0].ArrayIndex.X, m_AttackingActors[0].ArrayIndex.Y]);
								cAbility.TargetThisActorAutomatically = m_AttackingActors[0];
								list4.Add(cAbility);
							}
						}
						m_State = EAttackState.CheckForPlayerIdle;
						(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list4, null, performNow: false, stopPlayerSkipping: false, true);
						m_ProcessedStatusEffects = true;
						return true;
					}
				}
				m_ProcessedStatusEffects = true;
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData2 = new CPlayerWaitForIdle_MessageData(base.TargetingActor);
					cPlayerWaitForIdle_MessageData2.m_Actor = base.TargetingActor;
					ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData2);
					return true;
				}
				m_State = EAttackState.CheckForPlayerIdle;
				Perform();
				break;
			}
			bool flag8 = false;
			flag8 = CheckModifierAbilities(null, nonMultiPassAttack: true);
			if (base.SubAbilities.Count > 0 && base.SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
			{
				List<CAbility> list5 = new List<CAbility>();
				foreach (CAbility item19 in base.SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					foreach (CActor attackingActor in m_AttackingActors)
					{
						if (item19.MiscAbilityData == null || !item19.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item19.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || attackingActor.IsDead)
						{
							CAbility cAbility2 = CAbility.CopyAbility(item19, generateNewID: true, fullCopy: true);
							cAbility2.ParentAbility = this;
							cAbility2.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[attackingActor.ArrayIndex.X, attackingActor.ArrayIndex.Y]);
							cAbility2.TargetThisActorAutomatically = attackingActor;
							list5.Add(cAbility2);
						}
					}
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list5, null, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: true);
				flag8 = true;
			}
			if (flag8)
			{
				m_State = EAttackState.CheckForPlayerIdle;
				CWaitForProgressChoreographer_MessageData message5 = new CWaitForProgressChoreographer_MessageData(base.TargetingActor)
				{
					WaitTickFrame = 10000,
					WaitActor = base.TargetingActor,
					ClearEvents = false
				};
				ScenarioRuleClient.MessageHandler(message5);
				return true;
			}
			m_State = EAttackState.CheckForPlayerIdle;
			Perform();
			break;
		}
		case EAttackState.CheckForPlayerIdle:
			if (m_AttackingActors.Count() > 0 && m_AttackingActors[0] != null)
			{
				CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = new CPlayerWaitForIdle_MessageData(base.TargetingActor);
				cPlayerWaitForIdle_MessageData.m_Actor = m_AttackingActors[0];
				ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData);
				return true;
			}
			PhaseManager.StepComplete();
			break;
		case EAttackState.FinaliseAttack:
		{
			if (m_AttackSummary.Targets.Count > 0)
			{
				AbilityData.MiscAbilityData miscAbilityData7 = base.MiscAbilityData;
				if (miscAbilityData7 != null && miscAbilityData7.TargetOneEnemyWithAllAttacks == true)
				{
					CAttackSummary.TargetSummary targetSummary4 = m_AttackSummary.Targets[0];
					if (targetSummary4.UsedAttackMods.Except(m_PreviousAttackModsForTarget).Any((AttackModifierYMLData y) => y?.IsCurse ?? false))
					{
						foreach (CActiveBonus item20 in from y in CharacterClassManager.FindAllActiveBonuses()
							where y.BespokeBehaviour is CDamageActiveBonus_DamageOnCurseModApplied
							select y)
						{
							(item20.BespokeBehaviour as CDamageActiveBonus_DamageOnCurseModApplied).TriggerAbility(base.TargetingActor);
						}
					}
					m_PreviousAttackModsForTarget = targetSummary4.UsedAttackMods;
				}
				else if (m_AttackSummary.Targets.Any((CAttackSummary.TargetSummary x) => x?.UsedAttackMods != null && x.UsedAttackMods.Any((AttackModifierYMLData y) => y?.IsCurse ?? false)))
				{
					foreach (CActiveBonus item21 in from y in CharacterClassManager.FindAllActiveBonuses()
						where y.BespokeBehaviour is CDamageActiveBonus_DamageOnCurseModApplied
						select y)
					{
						(item21.BespokeBehaviour as CDamageActiveBonus_DamageOnCurseModApplied).TriggerAbility(base.TargetingActor);
					}
				}
			}
			if (!m_MultiPassAttack && m_AttackingActors.Count > 1)
			{
				ActorsToTargetModifiedStrength.RemoveAt(0);
				m_AttackingActors.RemoveAt(0);
				m_State = EAttackState.CheckForPlayerIdle;
				Perform();
				break;
			}
			if (!m_ProcessedStatusEffects)
			{
				List<CAttackSummary.TargetSummary> list6 = new List<CAttackSummary.TargetSummary>();
				list6.AddRange(m_AttackSummary.Targets.Where((CAttackSummary.TargetSummary x) => x.IsTargeted));
				int targetIndex4 = 0;
				foreach (CAttackSummary.TargetSummary item22 in list6)
				{
					if (item22 == null)
					{
						continue;
					}
					if (ScenarioManager.Scenario.HasActor(item22.Actor) || base.ProcessIfDead)
					{
						CActor cActor8 = ((base.OriginalTargetingActor != null) ? base.OriginalTargetingActor : base.TargetingActor);
						CAbilityAttack cAbilityAttack2 = item22.AttackAbilityWithOverrides as CAbilityAttack;
						bool num16 = base.SubAbilities.Count == 0 || base.SubAbilities.All((CAbility a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull);
						bool flag10 = cAbilityAttack2.ModifierAbilities.Count == 0 || cAbilityAttack2.ModifierAbilities.All((CAbility a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull);
						bool flag11 = cAbilityAttack2.CurrentOverrides.Count == 0 || cAbilityAttack2.CurrentOverrides.All((CAbilityOverride a) => a.AbilityType != EAbilityType.Push && a.AbilityType != EAbilityType.Pull && (a.SubAbilities == null || a.SubAbilities.Count == 0 || a.SubAbilities.All((CAbility b) => b.AbilityType != EAbilityType.Push && b.AbilityType != EAbilityType.Pull)));
						if (num16 && flag10 && flag11 && ScenarioManager.Scenario.HasActor(cActor8))
						{
							cActor8.ApplyRetaliateToAttack(item22.Actor, this);
						}
						if (item22.ItemOverrideAbility != null)
						{
							CAttackSummary.TargetSummary targetSummary5 = item22.ItemOverrideAbility.AttackSummary.FindTarget(item22.Actor, ref targetIndex4);
							if (targetSummary5 != null && targetSummary5.ItemOverrideAbility.NegativeConditions.Count > 0)
							{
								targetSummary5.ItemOverrideAbility.ProcessNegativeStatusEffects(item22.Actor);
							}
						}
						else if (item22.AttackAbilityWithOverrides.NegativeConditions.Count > 0)
						{
							item22.AttackAbilityWithOverrides.ProcessNegativeStatusEffects(item22.Actor);
						}
						if (item22.Actor.m_OnBeingAttackedPostDamageListeners != null)
						{
							item22.Actor.m_OnBeingAttackedPostDamageListeners?.Invoke(this);
						}
					}
					if (!ScenarioManager.Scenario.HasActor(base.TargetingActor))
					{
						continue;
					}
					if (item22.ItemOverrideAbility != null)
					{
						CAttackSummary.TargetSummary targetSummary6 = item22.ItemOverrideAbility.AttackSummary.FindTarget(item22.Actor, ref targetIndex4);
						if (targetSummary6 != null && targetSummary6.ItemOverrideAbility.PositiveConditions.Count > 0)
						{
							targetSummary6.ItemOverrideAbility.ProcessPositiveStatusEffects(base.TargetingActor);
						}
					}
					else if (item22 != null && item22.AttackAbilityWithOverrides.PositiveConditions.Count > 0)
					{
						item22.AttackAbilityWithOverrides.ProcessPositiveStatusEffects(base.TargetingActor);
					}
				}
				CheckModifierAbilities();
			}
			if (m_MultiPassAttack && (!base.TargetingActor.IsDead || base.ProcessIfDead))
			{
				ActorsToTargetModifiedStrength.RemoveAt(0);
				int targetIndex5 = 0;
				CAttackSummary.TargetSummary item5 = m_AttackSummary.FindTarget(m_AttackingActors[0], ref targetIndex5);
				AttackSummary.Targets.Remove(item5);
				while (m_MultiPassActors.Count > 0 && !ScenarioManager.Scenario.HasActor(m_MultiPassActors[0]))
				{
					m_MultiPassActors.Remove(m_MultiPassActors[0]);
				}
				if (m_MultiPassActors.Count > 0)
				{
					m_State = EAttackState.DrawingModifiers;
					CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData3 = new CPlayerWaitForIdle_MessageData(base.TargetingActor);
					cPlayerWaitForIdle_MessageData3.m_Actor = base.TargetingActor;
					ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData3);
					return true;
				}
			}
			if (base.AreaEffect == null && m_AreaEffectBackup != null)
			{
				base.AreaEffect = m_AreaEffectBackup;
			}
			if (base.TargetingActor.m_OnAttackAbilityFinishedListeners != null)
			{
				base.TargetingActor.m_OnAttackAbilityFinishedListeners?.Invoke(this);
			}
			CAttackDone_MessageData cAttackDone_MessageData = new CAttackDone_MessageData(base.TargetingActor);
			cAttackDone_MessageData.m_AttackingActor = base.TargetingActor;
			ScenarioRuleClient.MessageHandler(cAttackDone_MessageData);
			base.TargetingActor.AIMoveFocusActors?.Clear();
			if (base.IsMergedAbility)
			{
				PhaseManager.StepComplete();
			}
			else if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.CurrentPhaseAbility.m_Ability != this)
			{
				m_State = EAttackState.AttackDone;
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		case EAttackState.AttackBuff:
		{
			ScenarioRuleClient.FirstAbilityStarted();
			base.AbilityHasHappened = true;
			foreach (CActor item23 in m_ActorsToTarget)
			{
				ApplyToActor(item23);
			}
			CAttackBuff_MessageData message3 = new CAttackBuff_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_AttackAbility = this
			};
			ScenarioRuleClient.MessageHandler(message3);
			if (base.AnimOverload.Contains("None"))
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		case EAttackState.SelectAttackFocusAdditionalTargets:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				if (ChainAttack)
				{
					foreach (CActor item24 in m_ActorsToTarget)
					{
						if (!m_ActorsToIgnore.Contains(item24))
						{
							m_ActorsToIgnore.Add(item24);
						}
					}
					List<CActor> actorList = GameState.GetActorsInRange(m_ActorsToTarget.Last().ArrayIndex, base.TargetingActor, m_ChainAttackRange, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
					RemoveImmuneActorsFromList(ref actorList);
					m_ValidActorsInRange.Clear();
					foreach (CActor item25 in actorList)
					{
						if (!m_ValidActorsInRange.Contains(item25))
						{
							m_ValidActorsInRange.Add(item25);
						}
					}
					base.TilesInRange = GameState.GetTilesInRange(m_ActorsToTarget.Last(), m_ChainAttackRange, base.Targeting, emptyTilesOnly: false, !IsMeleeAttack);
					foreach (CActor item26 in m_ActorsToTarget)
					{
						CTile item4 = ScenarioManager.Tiles[item26.ArrayIndex.X, item26.ArrayIndex.Y];
						base.TilesInRange.Add(item4);
						m_ActorsToIgnore.Remove(item26);
					}
					ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
					foreach (CActor item27 in m_ValidActorsInRange)
					{
						if (!m_ActorsToTargetLocked.Contains(item27))
						{
							m_AttackSummary.UpdateSingleTargetData(this, item27);
						}
					}
				}
				CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData6 = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
				cActorIsSelectingAttackFocusTargets_MessageData6.m_AttackingActor = base.TargetingActor;
				cActorIsSelectingAttackFocusTargets_MessageData6.m_AttackAbility = this;
				cActorIsSelectingAttackFocusTargets_MessageData6.m_AttackSummary = AttackSummary.Copy();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData6);
			}
			else
			{
				CActor cActor6 = ((base.TargetingActor.AIMoveFocusActors.Count == 0) ? null : base.TargetingActor.AIMoveFocusActors[0]);
				CTile cTile3 = ((cActor6 == null) ? null : ScenarioManager.Tiles[cActor6.ArrayIndex.X, cActor6.ArrayIndex.Y]);
				CTile cTile4 = ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y];
				foreach (CActor aIMoveFocusActor3 in base.TargetingActor.AIMoveFocusActors)
				{
					if (aIMoveFocusActor3 != null && !m_ActorsToTarget.Contains(aIMoveFocusActor3))
					{
						cTile3 = ScenarioManager.Tiles[aIMoveFocusActor3.ArrayIndex.X, aIMoveFocusActor3.ArrayIndex.Y];
						if (CActor.HaveLOS(cTile4, cTile3) && !m_ActorsToIgnore.Contains(aIMoveFocusActor3) && IsActorWithinRange(cTile4, cTile3))
						{
							m_ActorsToTarget.Add(aIMoveFocusActor3);
							AttackSummary.UpdateSingleTargetData(this, aIMoveFocusActor3);
						}
						if (m_ActorsToTarget.Count == m_NumberTargetsRemaining)
						{
							break;
						}
					}
				}
				m_State = EAttackState.DrawingModifiers;
				Perform();
			}
			return true;
		case EAttackState.SelectAdditionalTargetsDone:
			m_State = EAttackState.DrawingModifiers;
			if (ChainAttack)
			{
				foreach (CActor item28 in m_ActorsToTarget)
				{
					if (!m_ActorsToTargetLocked.Contains(item28))
					{
						m_AttackSummary.UpdateSingleTargetData(this, item28);
					}
				}
			}
			Perform();
			return true;
		case EAttackState.SelectAttackFocusAddTargetBuff:
		{
			if (m_AttackSummary != null && !ChainAttack)
			{
				m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, useBothLists: true);
			}
			CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
			cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor = base.TargetingActor;
			cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility = this;
			cActorIsSelectingAttackFocusTargets_MessageData.m_AttackSummary = AttackSummary.Copy();
			ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData);
			return true;
		}
		case EAttackState.SelectAttackFocusAddTargetBuffDone:
			if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				m_State = EAttackState.AttackBuff;
			}
			else
			{
				m_State = EAttackState.PreActorIsAttacking;
			}
			Perform();
			return true;
		case EAttackState.AttackDone:
			{
				PhaseManager.NextStep();
				return true;
			}
			IL_0ffb:
			if (AttackSummary != null)
			{
				AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
			}
			goto IL_101d;
			IL_101d:
			for (int num = m_ActiveSingleTargetItems.Count - 1; num >= 0; num--)
			{
				CItem cItem = m_ActiveSingleTargetItems[num];
				if (cItem != null && cItem.SingleTarget == null)
				{
					base.TargetingActor.Inventory.DeselectItem(cItem);
					ToggleSingleTargetItem(cItem);
				}
			}
			for (int num2 = m_ActiveSingleTargetActiveBonuses.Count - 1; num2 >= 0; num2--)
			{
				CActiveBonus cActiveBonus = m_ActiveSingleTargetActiveBonuses[num2];
				if (cActiveBonus != null && cActiveBonus.SingleTarget == null)
				{
					cActiveBonus.ToggleActiveBonus(null, base.TargetingActor);
				}
			}
			m_CanUndo = false;
			if (base.TargetingActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.SummonData.AttackInfuse.HasValue)
			{
				ElementInfusionBoardManager.Infuse((base.TargetingActor as CHeroSummonActor).SummonData.AttackInfuse.Value, base.TargetingActor);
			}
			else if (base.MiscAbilityData?.OnAttackInfuse != null)
			{
				ElementInfusionBoardManager.Infuse(base.MiscAbilityData.OnAttackInfuse, base.TargetingActor);
			}
			if (base.TargetingActor.m_OnPreActorIsAttackingListeners != null)
			{
				base.TargetingActor.m_OnPreActorIsAttackingListeners(this);
			}
			flag = false;
			if (base.TargetingActor.Type.Equals(CActor.EType.Player) && m_ActorsToTarget.Count > 0)
			{
				List<CActiveBonus> list = (from w in CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.Attack)
					where w.Ability.Augment == null
					select w).ToList();
				if (list.Count > 0)
				{
					foreach (CActiveBonus item29 in list)
					{
						if (item29.StackActiveBonusInlineAbility(m_ActorsToTarget[0], this))
						{
							if (m_AttackSummary != null)
							{
								m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: true);
							}
							flag = true;
							m_StackedInlineAbilityActiveBonuses.Add(item29);
						}
					}
				}
			}
			if (m_ActorsToTarget.Count <= 0)
			{
				m_CancelAbility = true;
			}
			if (!flag)
			{
				if (base.IsMergedAbility)
				{
					m_State = EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility;
					PhaseManager.StepComplete();
				}
				else
				{
					m_State = EAttackState.CheckForDamageSelf;
					Perform();
				}
			}
			else
			{
				m_CurrentStackedInlineAbilityActiveBonus = m_StackedInlineAbilityActiveBonuses[0];
				if (m_ActorsToTarget.Count > 1)
				{
					m_State = EAttackState.SelectActiveBonusBuffTarget;
				}
				else
				{
					m_State = EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility;
				}
			}
			break;
		}
		return false;
		IL_0097:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				CPlayerIsStunned_MessageData message6 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message6);
			}
			else if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message7 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message7);
			}
			else
			{
				CPlayerIsDisarmed_MessageData message8 = new CPlayerIsDisarmed_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message8);
				if (base.IsMergedAbility)
				{
					m_CancelAbility = true;
					PhaseManager.StepComplete();
				}
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public static bool TargetsOnPath(CAbilityAttack attackAbility, List<CTile> optionalTileList, out List<CActor> targetActors)
	{
		targetActors = new List<CActor>();
		if (optionalTileList != null)
		{
			for (int i = 0; i < optionalTileList.Count; i++)
			{
				bool foundPath = false;
				List<Point> list = new List<Point>();
				list = ((i != 0) ? ScenarioManager.PathFinder.FindPath(optionalTileList[i - 1].m_ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath) : ScenarioManager.PathFinder.FindPath(attackAbility.TargetingActor.ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath));
				if (foundPath)
				{
					foreach (Point item in list)
					{
						List<CActor> actorsOnTile = GameState.GetActorsOnTile(ScenarioManager.Tiles[item.X, item.Y], attackAbility.TargetingActor, attackAbility.AbilityFilter, attackAbility.ActorsToIgnore, attackAbility.IsTargetedAbility, false);
						targetActors.AddRange(actorsOnTile);
					}
					continue;
				}
				return false;
			}
			targetActors = targetActors.Distinct().ToList();
			return true;
		}
		return false;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm) && !base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && !base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
		{
			if (m_State.Equals(EAttackState.SelectActiveBonusBuffTarget))
			{
				if (CheckSelectedActiveBonusBuffTarget(selectedTile))
				{
					flag = true;
				}
			}
			else
			{
				if (base.AreaEffect != null)
				{
					UpdateAreaEffect(selectedTile, base.AreaEffectAngle);
					bool flag2 = false;
					List<CActor> validActorsInArea = GetValidActorsInArea(base.ValidTilesInAreaEffectedIncludingBlocked);
					if (validActorsInArea.Count > 0)
					{
						flag2 = true;
						m_AreaEffectLocked = true;
						base.ValidActorsInRange = validActorsInArea;
					}
					if (!flag2)
					{
						base.TileSelected(selectedTile, optionalTileList);
						LogEvent(ESESubTypeAbility.AbilityTileSelected);
						return;
					}
				}
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
				if (TargetsOnPath(this, optionalTileList, out var targetActors))
				{
					m_ActorsToTarget.Clear();
					foreach (CActor item in targetActors)
					{
						if (!CAbility.ImmuneToAbility(item, this) && CActor.HaveLOS(ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y], ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y]))
						{
							m_ActorsToTarget.Add(item);
						}
					}
					if (m_ActorsToTarget.Count > 0)
					{
						m_State = EAttackState.DrawingModifiers;
						flag = true;
					}
				}
				else if (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAdditionalTargets || (m_State == EAttackState.SelectAttackFocusAddTargetBuff && m_AreaEffectBackup == null))
				{
					if (SharedAbilityTargeting.TileSelected(this, selectedTile, (m_State == EAttackState.SelectAttackFocus) ? base.AreaEffect : null, base.TargetingActor, base.FilterActor, m_Range, ref m_ValidActorsInRange, m_ActorsToIgnore, base.AbilityFilter, m_TotalNumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, base.IsTargetedAbility))
					{
						if (m_State == EAttackState.SelectAttackFocus && base.AreaEffect != null && m_AttackSummary.ActiveBonusAddTargetBuff != 0)
						{
							if (m_AttackSummary != null)
							{
								m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
							}
							if (m_NumberTargetsRemaining > 1)
							{
								m_NumberTargetsRemaining--;
							}
							m_AreaEffectBackup = base.AreaEffect;
							base.AreaEffect = null;
							m_State = EAttackState.SelectAttackFocusAddTargetBuff;
							m_ActorsToTarget = new List<CActor>(base.ValidActorsInRange);
							List<CActor> list = new List<CActor>(base.ValidActorsInRange);
							list.AddRange(m_ActorsToIgnore);
							list = list.Distinct().ToList();
							base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor, base.FilterActor, m_Range, list, base.AbilityFilter, null, base.AllPossibleTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
							RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
							m_ActorsToTarget.Union(base.ValidActorsInRange);
							foreach (CActor item2 in base.ValidActorsInRange)
							{
								m_AttackSummary.UpdateSingleTargetData(this, item2);
							}
							CActorIsSelectingAttackFocusTargets_MessageData cActorIsSelectingAttackFocusTargets_MessageData = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor);
							cActorIsSelectingAttackFocusTargets_MessageData.m_AttackingActor = base.TargetingActor;
							cActorIsSelectingAttackFocusTargets_MessageData.m_AttackAbility = this;
							cActorIsSelectingAttackFocusTargets_MessageData.m_AttackSummary = AttackSummary.Copy();
							ScenarioRuleClient.MessageHandler(cActorIsSelectingAttackFocusTargets_MessageData);
						}
						else
						{
							if (base.AreaEffect != null)
							{
								m_ActorsToTarget = base.ValidActorsInRange.ToList();
							}
							if (m_State == EAttackState.SelectAttackFocus && m_AttackSummary != null)
							{
								m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, useActorsToTarget: false, ChainAttack);
							}
						}
					}
					else if (ChainAttack)
					{
						flag = true;
					}
					else if (base.UseSubAbilityTargeting)
					{
						PhaseManager.NextStep();
					}
				}
				else if (m_State == EAttackState.SelectAttackFocusAddTargetBuff && m_NumberTargetsRemaining > 0 && GameState.GetTilesInRange(base.TargetingActor.ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true).Contains(selectedTile) && actorOnTile != null)
				{
					m_NumberTargetsRemaining--;
					m_ActorsToTarget.Add(actorOnTile);
					if (m_AttackSummary != null)
					{
						foreach (CActor item3 in m_ActorsToTarget)
						{
							m_AttackSummary.UpdateSingleTargetData(this, item3);
						}
					}
				}
			}
			if (flag)
			{
				Perform();
			}
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (m_State.Equals(EAttackState.SelectActiveBonusBuffTarget))
		{
			if (CheckSelectedActiveBonusBuffTarget(selectedTile))
			{
				flag = true;
			}
		}
		else if ((base.AreaEffect != null || base.AreaEffectBackup != null) && m_AreaEffectLocked && m_State != EAttackState.SelectAttackFocusAdditionalTargets)
		{
			if (SharedAbilityTargeting.TileDeselected(this, selectedTile, m_TotalNumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
			{
				if (base.AreaEffectBackup != null)
				{
					base.AreaEffect = m_AreaEffectBackup;
					m_AreaEffectBackup = null;
					m_State = EAttackState.SelectAttackFocus;
					m_ActorsToIgnore.Clear();
					m_NumberTargetsRemaining = m_NumberTargets + m_AttackSummary.ActiveBonusAddTargetBuff;
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				m_AreaEffectLocked = false;
				SharedAbilityTargeting.GetValidActorsInRange(this);
				flag = true;
				if (m_ActorsToTarget.Count <= 0)
				{
					base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				}
			}
		}
		else if (!base.AllTargets)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex);
			if (cActor != null && !m_ActorsToTargetLocked.Contains(cActor))
			{
				bool flag2 = false;
				if (ChainAttack)
				{
					int num = m_ActorsToTarget.IndexOf(cActor);
					for (int num2 = m_ActorsToTarget.Count - 1; num2 >= num; num2--)
					{
						CTile selectedTile2 = ScenarioManager.Tiles[m_ActorsToTarget[num2].ArrayIndex.X, m_ActorsToTarget[num2].ArrayIndex.Y];
						flag2 |= SharedAbilityTargeting.TileDeselected(this, selectedTile2, m_TotalNumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected);
					}
				}
				else
				{
					CAreaEffect cAreaEffect = null;
					if (m_State == EAttackState.SelectAttackFocusAdditionalTargets && base.AreaEffectBackup != null)
					{
						cAreaEffect = base.AreaEffectBackup;
						m_AreaEffectBackup = null;
					}
					flag2 |= SharedAbilityTargeting.TileDeselected(this, selectedTile, m_TotalNumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected);
					if (cAreaEffect != null)
					{
						m_AreaEffectBackup = cAreaEffect;
					}
				}
				if (flag2)
				{
					flag = true;
					if (m_ActorsToTarget.Count <= 0)
					{
						base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
					}
				}
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	private bool CheckSelectedActiveBonusBuffTarget(CTile selectedTile)
	{
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
		if (base.ValidActorsInRange.Contains(actorOnTile) && m_CurrentStackedInlineAbilityActiveBonus != null)
		{
			m_CurrentStackedInlineAbilityActiveBonus.UpdateActiveBonusInlineAbilityTarget(actorOnTile);
			m_StackedInlineAbilityActiveBonuses.Remove(m_CurrentStackedInlineAbilityActiveBonus);
			if (m_StackedInlineAbilityActiveBonuses.Count > 0)
			{
				m_CurrentStackedInlineAbilityActiveBonus = m_StackedInlineAbilityActiveBonuses[0];
			}
			else
			{
				m_State = EAttackState.UpdateAttackFocusAfterAttackEffectInlineSubAbility;
			}
			return true;
		}
		return false;
	}

	public void UpdateAllTargetsOnAttackPath(List<CTile> optionalTileList)
	{
		if (!TargetsOnPath(this, optionalTileList, out var targetActors))
		{
			return;
		}
		m_ActorsToTarget.Clear();
		foreach (CActor item in targetActors)
		{
			if (!CAbility.ImmuneToAbility(item, this) && CActor.HaveLOS(ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y], ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y]))
			{
				m_ActorsToTarget.Add(item);
			}
		}
		base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction);
		CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(base.TargetingActor);
		cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
		cShowSingleTargetActiveBonus_MessageData.m_Ability = this;
		ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		m_ActorsToTargetLocked.Clear();
		if (base.AreaEffect != null && m_State == EAttackState.SelectAttackFocus)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else if (m_AreaEffectBackup != null && m_State == EAttackState.SelectAttackFocusAddTargetBuff)
		{
			base.AreaEffect = m_AreaEffectBackup;
			m_AreaEffectBackup = null;
			m_State = EAttackState.SelectAttackFocus;
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (m_ActiveSingleTargetItems.Count > 0 || m_ActiveSingleTargetActiveBonuses.Count > 0)
		{
			for (int num = m_ActiveSingleTargetItems.Count - 1; num >= 0; num--)
			{
				CItem cItem = m_ActiveSingleTargetItems[num];
				if (cItem != null)
				{
					base.TargetingActor.Inventory.DeselectItem(cItem);
				}
			}
			m_ActiveSingleTargetItems.Clear();
			for (int num2 = m_ActiveSingleTargetActiveBonuses.Count - 1; num2 >= 0; num2--)
			{
				m_ActiveSingleTargetActiveBonuses[num2]?.ToggleActiveBonus(null, base.TargetingActor);
			}
			m_ActiveSingleTargetActiveBonuses.Clear();
			Restart();
			Perform();
		}
		if (m_AttackSummary != null)
		{
			m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
			base.TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			CActorIsSelectingAttackFocusTargets_MessageData message = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor)
			{
				m_AttackingActor = base.TargetingActor,
				m_AttackAbility = this,
				m_AttackSummary = AttackSummary.Copy()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override bool CanClearTargets()
	{
		if (base.UseSubAbilityTargeting)
		{
			return false;
		}
		if (m_State != EAttackState.SelectAttackFocus)
		{
			return m_State == EAttackState.SelectAttackFocusAddTargetBuff;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.UseSubAbilityTargeting)
		{
			return false;
		}
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EAttackState.SelectAttackFocus && m_State != EAttackState.SelectAttackFocusAddTargetBuff && m_State != EAttackState.SelectActiveBonusBuffTarget)
			{
				return m_State == EAttackState.SelectAttackFocusAdditionalTargets;
			}
			return true;
		}
		return false;
	}

	public override bool RequiresWaypointSelection()
	{
		if (base.AllTargetsOnAttackPath && (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAddTargetBuff || m_State == EAttackState.SelectAttackFocusAdditionalTargets))
		{
			if (base.ActiveBonusData != null)
			{
				if (base.ActiveBonusData != null)
				{
					return base.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		if (m_State == EAttackState.SelectAttackFocus || m_State == EAttackState.SelectAttackFocusAddTargetBuff)
		{
			if (base.ActiveBonusData != null)
			{
				if (base.ActiveBonusData != null)
				{
					return base.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool MaxTargetsSelected()
	{
		if (base.AreaEffect != null || base.AreaEffectBackup != null)
		{
			if (m_State == EAttackState.SelectAttackFocusAddTargetBuff || m_State == EAttackState.SelectAttackFocusAdditionalTargets)
			{
				return m_NumberTargetsRemaining <= 0;
			}
			return m_AreaEffectLocked;
		}
		return m_NumberTargetsRemaining <= 0;
	}

	public override void ToggleSingleTargetItem(CItem item)
	{
		CActor singleTarget = item.SingleTarget;
		base.ToggleSingleTargetItem(item);
		if (singleTarget != null && !m_ActiveSingleTargetItems.Contains(item))
		{
			for (int num = m_AttackSummary.Targets.Count - 1; num >= 0; num--)
			{
				CAttackSummary.TargetSummary targetSummary = m_AttackSummary.Targets[num];
				if (targetSummary.Actor == singleTarget)
				{
					m_AttackSummary.Targets.Remove(targetSummary);
					break;
				}
			}
			m_AttackSummary.UpdateSingleTargetData(this, singleTarget);
			ApplySingleTargetActiveBonus(singleTarget);
			ApplySingleTargetItem(singleTarget);
		}
		if (m_State == EAttackState.SelectAttackFocus)
		{
			CActorIsSelectingAttackFocusTargets_MessageData message = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor)
			{
				m_AttackingActor = base.TargetingActor,
				m_AttackAbility = this,
				m_AttackSummary = AttackSummary.Copy()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override void ToggleSingleTargetActiveBonus(CActiveBonus activeBonus)
	{
		CActor singleTarget = activeBonus.SingleTarget;
		base.ToggleSingleTargetActiveBonus(activeBonus);
		if (singleTarget != null && !m_ActiveSingleTargetActiveBonuses.Contains(activeBonus))
		{
			for (int num = m_AttackSummary.Targets.Count - 1; num >= 0; num--)
			{
				CAttackSummary.TargetSummary targetSummary = m_AttackSummary.Targets[num];
				if (targetSummary.Actor == singleTarget)
				{
					m_AttackSummary.Targets.Remove(targetSummary);
					break;
				}
			}
			m_AttackSummary.UpdateSingleTargetData(this, singleTarget);
			ApplySingleTargetActiveBonus(singleTarget);
			ApplySingleTargetItem(singleTarget);
		}
		if (m_State == EAttackState.SelectAttackFocus)
		{
			CActorIsSelectingAttackFocusTargets_MessageData message = new CActorIsSelectingAttackFocusTargets_MessageData(base.TargetingActor)
			{
				m_AttackingActor = base.TargetingActor,
				m_AttackAbility = this,
				m_AttackSummary = AttackSummary.Copy()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (base.ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, base.TargetingActor);
			}
			else if (cBaseCard != null)
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

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState && m_State != EAttackState.AttackDone && m_State != EAttackState.AttackBuffDone)
		{
			m_State++;
		}
		if (m_State != EAttackState.AttackDone)
		{
			return m_State == EAttackState.AttackBuffDone;
		}
		return true;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void Restart()
	{
		base.Restart();
		if (base.UseSubAbilityTargeting)
		{
			if (base.ParentAbility != null)
			{
				base.ValidActorsInRange = base.ParentAbility.ActorsTargeted;
			}
		}
		else
		{
			if (base.IsMergedAbility)
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || !miscAbilityData.IgnoreMergedAbilityTargetSelection.HasValue)
				{
					m_State = EAttackState.SelectAttackFocus;
					m_CancelAbility = false;
					if (m_ActorsToTarget != null)
					{
						m_ActorsToTarget.Clear();
					}
					ActorsToTargetModifiedStrength.Clear();
					if (base.ParentAbility != null && base.ParentAbility is CAbilityMerged cAbilityMerged)
					{
						CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
						if (mergedWithAbility != null && base.AllTargetsOnMovePath && mergedWithAbility is CAbilityMove)
						{
							SharedAbilityTargeting.GetValidActorsInRange(this);
							base.TilesInRange.Clear();
							for (int num = base.ValidActorsInRange.Count - 1; num >= 0; num--)
							{
								CActor cActor = base.ValidActorsInRange[num];
								if (!base.ActorsTargeted.Contains(cActor))
								{
									CTile item = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
									base.TilesInRange.Add(item);
									m_ActorsToTarget.Add(cActor);
								}
								else
								{
									base.ValidActorsInRange.Remove(cActor);
								}
							}
							m_CurrentAttackIndex = 0;
							m_CurrentPassAttackIndex = 0;
							if (m_ActorsToTarget.Count <= 0)
							{
								m_CancelAbility = true;
							}
						}
						else if (mergedWithAbility != null && mergedWithAbility.TilesSelected != null && mergedWithAbility.TilesSelected.Count > 0)
						{
							base.TilesInRange.Clear();
							foreach (CTile item2 in mergedWithAbility.TilesSelected)
							{
								AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
								if (miscAbilityData2 != null && miscAbilityData2.UseMergedWithAbilityTiles.HasValue)
								{
									AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
									if (miscAbilityData3 != null && miscAbilityData3.UseMergedWithAbilityTiles.Value)
									{
										base.TilesInRange.Add(item2);
										goto IL_025f;
									}
								}
								CActor cActor2 = ScenarioManager.Scenario.FindActorAt(item2.m_ArrayIndex);
								if (cActor2 != null)
								{
									m_ActorsToIgnore.Add(cActor2);
								}
								base.TilesInRange.AddRange(GameState.GetTilesInRange(item2.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
								goto IL_025f;
								IL_025f:
								foreach (CActor item3 in GameState.GetActorsInRange(item2.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible))
								{
									if (!m_ValidActorsInRange.Contains(item3))
									{
										m_ValidActorsInRange.Add(item3);
									}
								}
							}
							if (m_ValidActorsInRange.Count <= 0)
							{
								m_CancelAbility = true;
							}
						}
					}
					goto IL_0522;
				}
			}
			AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
			if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
				if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.Value)
				{
					base.TilesInRange.Clear();
					m_ValidActorsInRange = new List<CActor>();
					for (int i = 0; i < base.ParentAbility.ActorsTargeted.Count; i++)
					{
						CActor cActor3 = base.ParentAbility.ActorsTargeted[i];
						if (cActor3 != null)
						{
							m_ActorsToIgnore.Add(cActor3);
							base.TilesInRange.AddRange(from x in GameState.GetTilesInRange(cActor3.ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false)
								where !base.TilesInRange.Contains(x)
								select x);
							m_ValidActorsInRange.AddRange(from x in GameState.GetActorsInRange(cActor3.ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible)
								where !m_ValidActorsInRange.Contains(x)
								select x);
						}
					}
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
					goto IL_0522;
				}
			}
			m_ActorsToTarget.Clear();
			m_TilesSelected.Clear();
			foreach (CItem activeSingleTargetItem in m_ActiveSingleTargetItems)
			{
				base.TargetingActor.Inventory.DeselectItem(activeSingleTargetItem);
			}
			m_ActiveSingleTargetItems.Clear();
			if (m_State == EAttackState.SelectAttackFocusAddTargetBuff)
			{
				base.AreaEffect = m_AreaEffectBackup;
				m_AreaEffectBackup = null;
				m_State = EAttackState.SelectAttackFocus;
			}
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		goto IL_0522;
		IL_0522:
		if (m_NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
			m_NumberTargets = base.ValidActorsInRange.Count;
		}
		else
		{
			m_AllTargets = false;
		}
		if (ChainAttack)
		{
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, base.Range, base.Targeting, emptyTilesOnly: false, !IsMeleeAttack);
		}
		if (base.TargetingActor.m_OnAttackStartListeners != null)
		{
			base.TargetingActor.m_OnAttackStartListeners(this);
		}
		InitAttackSummary();
	}

	private void InitAttackSummary()
	{
		m_AttackSummary = new CAttackSummary(this);
		m_ModifiedStrength = m_AttackSummary.ModifiedAttackStrength;
		m_NumberTargetsRemaining = m_NumberTargets + m_AttackSummary.ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		bool isMeleeAttack = IsMeleeAttack;
		m_Range = m_OriginalRange + m_AttackSummary.ActiveBonusAddRangeBuff;
		if (m_AttackSummary.ActiveBonusAddRangeBuff != 0)
		{
			if (isMeleeAttack)
			{
				if (m_OriginalRange >= m_AttackSummary.ActiveBonusAddRangeBuff + 1)
				{
					m_Range = m_OriginalRange;
				}
				if (base.MiscAbilityData == null)
				{
					base.MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				base.MiscAbilityData.TreatAsMelee = true;
			}
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (m_Range < 0 && base.TargetingActor.Type != CActor.EType.Player)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).EnemyConsume();
		}
		m_TotalNumberTargets = m_NumberTargetsRemaining;
	}

	public override void ApplySingleTargetItem(CActor target)
	{
		if (m_AttackSummary != null && target != null && m_ValidActorsInRange.Contains(target))
		{
			bool flag = false;
			int targetIndex = 0;
			CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(target, ref targetIndex);
			if (targetSummary != null)
			{
				CAbilityAttack cAbilityAttack = null;
				cAbilityAttack = ((targetSummary.ItemOverrideAbility == null) ? (CAbility.CopyAbility(this, generateNewID: false, fullCopy: true) as CAbilityAttack) : (CAbility.CopyAbility(targetSummary.ItemOverrideAbility, generateNewID: false, fullCopy: true) as CAbilityAttack));
				cAbilityAttack.m_AugmentsAdded = m_AugmentsAdded;
				cAbilityAttack.m_AugmentOverridesProcessed = m_AugmentOverridesProcessed;
				cAbilityAttack.m_AugmentAbilitiesProcessed = m_AugmentAbilitiesProcessed;
				cAbilityAttack.m_SongOverridesProcessed = m_SongOverridesProcessed;
				cAbilityAttack.m_SongAbilitiesProcessed = m_SongAbilitiesProcessed;
				cAbilityAttack.AbilityStartListenersInvoked = base.AbilityStartListenersInvoked;
				List<CItem> list = m_ActiveSingleTargetItems.Where((CItem x) => x.SingleTarget == null || x.SingleTarget == target).ToList();
				for (int num = list.Count - 1; num >= 0; num--)
				{
					CItem cItem = list[num];
					if (!cItem.YMLData.Data.CompareAbility.CompareAbility(cAbilityAttack, target))
					{
						list.Remove(cItem);
					}
				}
				foreach (CItem item in list)
				{
					item.SingleTarget = target;
					if (item?.YMLData.Data?.Overrides == null || item.YMLData.Data.Overrides.Count <= 0)
					{
						continue;
					}
					foreach (CAbilityOverride @override in item.YMLData.Data.Overrides)
					{
						flag = true;
						cAbilityAttack.OverrideAbilityValues(@override, perform: false);
						if (@override.SubAbilities == null)
						{
							continue;
						}
						CPhaseAction cPhaseAction = PhaseManager.CurrentPhase as CPhaseAction;
						foreach (CAbility subAbility in @override.SubAbilities)
						{
							if (!cPhaseAction.RemainingPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == subAbility.Name))
							{
								CAbility cAbility = CAbility.CopyAbility(subAbility, generateNewID: false);
								cAbility.ParentAbility = cPhaseAction.CurrentPhaseAbility.m_Ability;
								cAbility.TargetThisActorAutomatically = target;
								cPhaseAction.RemainingPhaseAbilities.Add(new CPhaseAction.CPhaseAbility(cAbility, cPhaseAction.CurrentPhaseAbility.TargetingActor, item, null, item.ID));
							}
						}
					}
				}
				if (flag)
				{
					cAbilityAttack.Start(base.TargetingActor, base.FilterActor);
					targetIndex = 0;
					CAttackSummary.TargetSummary targetSummary2 = cAbilityAttack.m_AttackSummary.FindTarget(target, ref targetIndex);
					if (targetSummary != null && targetSummary2 != null)
					{
						targetSummary2.ItemOverrideAbility = cAbilityAttack;
						m_AttackSummary.Targets.Remove(targetSummary);
						m_AttackSummary.Targets.Add(targetSummary2);
					}
				}
				LogEvent(ESESubTypeAbility.AbilityApplySingleTargetItem);
			}
		}
		if (m_ActorsToTarget.Count > 0)
		{
			base.TargetingActor.Inventory.HighlightUsableItems(this, target, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
		}
	}

	public override void ApplySingleTargetActiveBonus(CActor target)
	{
		if (m_AttackSummary != null && target != null && base.ActorsToTarget.Contains(target))
		{
			bool flag = false;
			int targetIndex = 0;
			CAttackSummary.TargetSummary targetSummary = m_AttackSummary.FindTarget(target, ref targetIndex);
			if (targetSummary != null)
			{
				CAbilityAttack cAbilityAttack = null;
				cAbilityAttack = ((targetSummary.ItemOverrideAbility == null) ? (CAbility.CopyAbility(this, generateNewID: false, m_ActiveSingleTargetActiveBonuses.Any((CActiveBonus x) => x.FullCopy)) as CAbilityAttack) : (CAbility.CopyAbility(targetSummary.ItemOverrideAbility, generateNewID: false) as CAbilityAttack));
				cAbilityAttack.m_AugmentsAdded = m_AugmentsAdded;
				cAbilityAttack.m_AugmentOverridesProcessed = m_AugmentOverridesProcessed;
				cAbilityAttack.m_AugmentAbilitiesProcessed = m_AugmentAbilitiesProcessed;
				cAbilityAttack.m_SongOverridesProcessed = m_SongOverridesProcessed;
				cAbilityAttack.m_SongAbilitiesProcessed = m_SongAbilitiesProcessed;
				cAbilityAttack.AbilityStartListenersInvoked = base.AbilityStartListenersInvoked;
				foreach (CActiveBonus item in m_ActiveSingleTargetActiveBonuses.Where((CActiveBonus x) => x.SingleTarget == null || x.SingleTarget == target))
				{
					flag = true;
					item.SingleTarget = target;
					if (item.Ability.Song != null)
					{
						foreach (CSong.SongEffect songEffect in item.Ability.Song.SongEffects)
						{
							songEffect.OnSongEffectToggled(cAbilityAttack, toggledOn: true);
						}
					}
					cAbilityAttack.ActiveBonusToggled(base.TargetingActor, item);
					if (item.BespokeBehaviour != null)
					{
						item.BespokeBehaviour.OnActiveBonusToggled(cAbilityAttack, toggledOn: true);
					}
				}
				if (flag)
				{
					cAbilityAttack.Start(base.TargetingActor, base.FilterActor);
					targetIndex = 0;
					CAttackSummary.TargetSummary targetSummary2 = cAbilityAttack.m_AttackSummary.FindTarget(target, ref targetIndex);
					if (targetSummary != null && targetSummary2 != null)
					{
						targetSummary2.ItemOverrideAbility = cAbilityAttack;
						m_AttackSummary.Targets.Remove(targetSummary);
						m_AttackSummary.Targets.Add(targetSummary2);
					}
				}
				LogEvent(ESESubTypeAbility.AbilityApplySingleTargetActiveBonus);
			}
		}
		if (m_ActorsToTarget.Count > 0)
		{
			base.TargetingActor.Inventory.HighlightUsableItems(this, target, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
		}
	}

	public override void ActiveBonusToggled(CActor actor, CActiveBonus activeBonus)
	{
		if (m_AttackSummary != null)
		{
			if (CurrentAttackingTargetSummary != null)
			{
				m_AttackSummary.RefreshingTargetDataForActiveBonusToggled(this, actor, CurrentAttackingTargetSummary);
			}
			else if (actor.Equals(base.TargetingActor) && CanApplyActiveBonusTogglesTo())
			{
				m_AttackSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses, m_AreaEffectLocked);
			}
			else
			{
				m_AttackSummary.RefreshingTargetDataForMultipassAttack(this, actor);
			}
			CUpdateAttackFocusAfterAttackEffectInlineSubAbility cUpdateAttackFocusAfterAttackEffectInlineSubAbility = new CUpdateAttackFocusAfterAttackEffectInlineSubAbility(base.TargetingActor);
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackingActor = base.TargetingActor;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackAbility = this;
			cUpdateAttackFocusAfterAttackEffectInlineSubAbility.m_AttackSummary = m_AttackSummary?.Copy();
			ScenarioRuleClient.MessageHandler(cUpdateAttackFocusAfterAttackEffectInlineSubAbility);
		}
	}

	public override void OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null)
	{
		if (m_ChainAttack && abilityOverride.Range.HasValue)
		{
			if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
			{
				m_ChainAttackRange = abilityOverride.Range.Value;
			}
			else
			{
				m_ChainAttackRange += abilityOverride.Range.Value;
			}
		}
		base.OverrideAbilityValues(abilityOverride, perform, item, filter);
	}

	public override void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		if (m_ChainAttack && abilityOverride.Range.HasValue)
		{
			if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
			{
				m_ChainAttackRange = abilityOverride.OriginalAbility.Range;
			}
			else
			{
				m_ChainAttackRange -= abilityOverride.Range.Value;
			}
		}
		base.UndoOverride(abilityOverride, perform, item);
	}

	public override string GetDescription()
	{
		if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			if (m_Range > 0 || m_NumberTargets > 0)
			{
				return "AttackBuff (" + m_Strength + ") R: " + m_Range + " N: " + m_NumberTargets;
			}
			return "AttackBuff (" + m_Strength + ")";
		}
		if (m_Range > 0 || m_NumberTargets > 0)
		{
			return "Attack(" + m_Strength + ") R: " + m_Range + " N: " + m_NumberTargets + GetTargetAbilityDescription() + GetAreaEffectDescription();
		}
		return "Attack(" + m_Strength + ")" + GetTargetAbilityDescription() + GetAreaEffectDescription();
	}

	private bool ProcessAbilityAugments()
	{
		List<CAbility> list = new List<CAbility>();
		foreach (CAugment abilityAugment in m_AbilityAugments)
		{
			list.AddRange(abilityAugment.Abilities);
		}
		if (list.Count > 0)
		{
			(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(list, base.TargetingActor, killAfter: false, stackToNextCurrent: true);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			return true;
		}
		return false;
	}

	public static void SetNextAttackValueOverride(int value)
	{
		s_AttackValueOverride = value;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		_ = 2;
		bool isSummon = false;
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		int targets = 1;
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityAttack(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, State, AttackEffects.ToList(), base.IsTargetedAbility, base.AreaEffect != null, targets, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens.ToList(), base.TargetingActor?.Tokens.CheckNegativeTokens.ToList(), "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null, "", IsDefaultAttack, base.AbilityHasHappened));
	}

	public bool HasPassedState(EAttackState attackState)
	{
		return m_State > attackState;
	}

	public override bool HasCondition(CCondition.ENegativeCondition condition)
	{
		if (base.HasCondition(condition))
		{
			return true;
		}
		if (base.TargetingActor != null)
		{
			foreach (CActiveBonus item in (from w in CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.Attack)
				where w.Ability.Augment == null
				select w).ToList())
			{
				if (item.Ability.NegativeConditions.ContainsKey(condition))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool HasCondition(CCondition.EPositiveCondition condition)
	{
		if (base.HasCondition(condition))
		{
			return true;
		}
		if (base.TargetingActor != null)
		{
			foreach (CActiveBonus item in (from w in CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.Attack)
				where w.Ability.Augment == null
				select w).ToList())
			{
				if (item.Ability.PositiveConditions.ContainsKey(condition))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsTargetOutsideOfInitialRange(CActor targetActor)
	{
		bool foundPath;
		List<Point> list = ScenarioManager.PathFinder.FindPath(base.TargetingActor.ArrayIndex, targetActor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out foundPath);
		if (m_AttackSummary.ActiveBonusAddRangeBuff != 0 && list.Count > base.Range - m_AttackSummary.ActiveBonusAddRangeBuff)
		{
			return true;
		}
		return false;
	}

	public bool CheckModifierAbilities(CAttackSummary.TargetSummary tSummary = null, bool nonMultiPassAttack = false)
	{
		if (!m_ModifiersChecked || !m_InlineModifiersChecked)
		{
			bool result = false;
			base.TargetingActor.Inventory.HandleUsedItems();
			if (tSummary != null)
			{
				if (tSummary.AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack && cAbilityAttack.ModifierAbilities.Count > 0)
				{
					if (ScenarioManager.Scenario.HasActor(base.TargetingActor))
					{
						ProcessModifierAbilities(cAbilityAttack.ModifierAbilities, nonMultiPassAttack);
					}
					cAbilityAttack.ModifierAbilities.Clear();
					result = true;
				}
			}
			else
			{
				List<CAttackSummary.TargetSummary> list = new List<CAttackSummary.TargetSummary>();
				list.AddRange(m_AttackSummary.Targets.Where((CAttackSummary.TargetSummary x) => x.IsTargeted));
				foreach (CAttackSummary.TargetSummary item in list)
				{
					if (item != null && item.AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack2 && cAbilityAttack2.ModifierAbilities.Count > 0)
					{
						if (ScenarioManager.Scenario.HasActor(base.TargetingActor))
						{
							ProcessModifierAbilities(cAbilityAttack2.ModifierAbilities, nonMultiPassAttack);
						}
						cAbilityAttack2.ModifierAbilities.Clear();
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	private void ProcessModifierAbilities(List<CAbility> modifierAbilities, bool nonMultiPassAttack)
	{
		foreach (CAbility modifierAbility in modifierAbilities)
		{
			switch (modifierAbility.AbilityType)
			{
			case EAbilityType.Infuse:
			{
				CAbilityInfuse cAbilityInfuse = modifierAbility as CAbilityInfuse;
				cAbilityInfuse.DoInfuse();
				DLLDebug.LogInfo("Infusing " + string.Join(" ,", cAbilityInfuse.ElementsToInfuse.Select((ElementInfusionBoardManager.EElement s) => s.ToString())) + " from attack modifier cards");
				break;
			}
			case EAbilityType.Heal:
			case EAbilityType.Damage:
			case EAbilityType.Shield:
			case EAbilityType.RefreshItemCards:
			case EAbilityType.Push:
			case EAbilityType.Pull:
				if (!m_InlineModifiersChecked)
				{
					modifierAbility.ParentAbility = this;
					m_State = EAttackState.CheckForPlayerIdle;
					(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(new List<CAbility> { modifierAbility }, null, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: true, null, nonMultiPassAttack);
				}
				break;
			default:
				DLLDebug.LogError("Unsupported attack modifier ability type being processed");
				break;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		s_AttackValueOverride = int.MaxValue;
		s_AttackModifierCardOverride = null;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count > 0;
	}

	public CAbilityAttack()
	{
	}

	public CAbilityAttack(CAbilityAttack state, ReferenceDictionary references)
		: base(state, references)
	{
		Pierce = state.Pierce;
		DamageSelfBeforeAttack = state.DamageSelfBeforeAttack;
		AttackEffects = references.Get(state.AttackEffects);
		if (AttackEffects == null && state.AttackEffects != null)
		{
			AttackEffects = new List<CAttackEffect>();
			for (int i = 0; i < state.AttackEffects.Count; i++)
			{
				CAttackEffect cAttackEffect = state.AttackEffects[i];
				CAttackEffect cAttackEffect2 = references.Get(cAttackEffect);
				if (cAttackEffect2 == null && cAttackEffect != null)
				{
					cAttackEffect2 = new CAttackEffect(cAttackEffect, references);
					references.Add(cAttackEffect, cAttackEffect2);
				}
				AttackEffects.Add(cAttackEffect2);
			}
			references.Add(state.AttackEffects, AttackEffects);
		}
		m_State = state.m_State;
		m_AttackingActors = references.Get(state.m_AttackingActors);
		if (m_AttackingActors == null && state.m_AttackingActors != null)
		{
			m_AttackingActors = new List<CActor>();
			for (int j = 0; j < state.m_AttackingActors.Count; j++)
			{
				CActor cActor = state.m_AttackingActors[j];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				m_AttackingActors.Add(cActor2);
			}
			references.Add(state.m_AttackingActors, m_AttackingActors);
		}
		m_ActorsToTargetLocked = references.Get(state.m_ActorsToTargetLocked);
		if (m_ActorsToTargetLocked == null && state.m_ActorsToTargetLocked != null)
		{
			m_ActorsToTargetLocked = new List<CActor>();
			for (int k = 0; k < state.m_ActorsToTargetLocked.Count; k++)
			{
				CActor cActor4 = state.m_ActorsToTargetLocked[k];
				CActor cActor5 = references.Get(cActor4);
				if (cActor5 == null && cActor4 != null)
				{
					CActor cActor3 = ((cActor4 is CObjectActor state6) ? new CObjectActor(state6, references) : ((cActor4 is CEnemyActor state7) ? new CEnemyActor(state7, references) : ((cActor4 is CHeroSummonActor state8) ? new CHeroSummonActor(state8, references) : ((!(cActor4 is CPlayerActor state9)) ? new CActor(cActor4, references) : new CPlayerActor(state9, references)))));
					cActor5 = cActor3;
					references.Add(cActor4, cActor5);
				}
				m_ActorsToTargetLocked.Add(cActor5);
			}
			references.Add(state.m_ActorsToTargetLocked, m_ActorsToTargetLocked);
		}
		ActorsToTargetModifiedStrength = references.Get(state.ActorsToTargetModifiedStrength);
		if (ActorsToTargetModifiedStrength == null && state.ActorsToTargetModifiedStrength != null)
		{
			ActorsToTargetModifiedStrength = new List<int>();
			for (int l = 0; l < state.ActorsToTargetModifiedStrength.Count; l++)
			{
				int item = state.ActorsToTargetModifiedStrength[l];
				ActorsToTargetModifiedStrength.Add(item);
			}
			references.Add(state.ActorsToTargetModifiedStrength, ActorsToTargetModifiedStrength);
		}
		m_MultiPassAttack = state.m_MultiPassAttack;
		m_MultiPassAttackStarted = state.m_MultiPassAttackStarted;
		m_MultiPassActors = references.Get(state.m_MultiPassActors);
		if (m_MultiPassActors == null && state.m_MultiPassActors != null)
		{
			m_MultiPassActors = new List<CActor>();
			for (int m = 0; m < state.m_MultiPassActors.Count; m++)
			{
				CActor cActor6 = state.m_MultiPassActors[m];
				CActor cActor7 = references.Get(cActor6);
				if (cActor7 == null && cActor6 != null)
				{
					CActor cActor3 = ((cActor6 is CObjectActor state10) ? new CObjectActor(state10, references) : ((cActor6 is CEnemyActor state11) ? new CEnemyActor(state11, references) : ((cActor6 is CHeroSummonActor state12) ? new CHeroSummonActor(state12, references) : ((!(cActor6 is CPlayerActor state13)) ? new CActor(cActor6, references) : new CPlayerActor(state13, references)))));
					cActor7 = cActor3;
					references.Add(cActor6, cActor7);
				}
				m_MultiPassActors.Add(cActor7);
			}
			references.Add(state.m_MultiPassActors, m_MultiPassActors);
		}
		m_ChainAttack = state.m_ChainAttack;
		m_ChainAttackRange = state.m_ChainAttackRange;
		m_ChainAttackDamageReduction = state.m_ChainAttackDamageReduction;
		m_ProcessedStatusEffects = state.m_ProcessedStatusEffects;
		m_TotalNumberTargets = state.m_TotalNumberTargets;
		m_IsDefaultAttack = state.m_IsDefaultAttack;
		m_AreaRangeTileX = state.m_AreaRangeTileX;
		m_AreaRangeTileY = state.m_AreaRangeTileY;
		m_CurrentAttackIndex = state.m_CurrentAttackIndex;
		m_CurrentPassAttackIndex = state.m_CurrentPassAttackIndex;
		m_StackedInlineAbilityActiveBonuses = references.Get(state.m_StackedInlineAbilityActiveBonuses);
		if (m_StackedInlineAbilityActiveBonuses == null && state.m_StackedInlineAbilityActiveBonuses != null)
		{
			m_StackedInlineAbilityActiveBonuses = new List<CActiveBonus>();
			for (int n = 0; n < state.m_StackedInlineAbilityActiveBonuses.Count; n++)
			{
				CActiveBonus cActiveBonus = state.m_StackedInlineAbilityActiveBonuses[n];
				CActiveBonus cActiveBonus2 = references.Get(cActiveBonus);
				if (cActiveBonus2 == null && cActiveBonus != null)
				{
					CActiveBonus cActiveBonus3 = ((cActiveBonus is CAddConditionActiveBonus state14) ? new CAddConditionActiveBonus(state14, references) : ((cActiveBonus is CAddHealActiveBonus state15) ? new CAddHealActiveBonus(state15, references) : ((cActiveBonus is CAddRangeActiveBonus state16) ? new CAddRangeActiveBonus(state16, references) : ((cActiveBonus is CAddTargetActiveBonus state17) ? new CAddTargetActiveBonus(state17, references) : ((cActiveBonus is CAdjustInitiativeActiveBonus state18) ? new CAdjustInitiativeActiveBonus(state18, references) : ((cActiveBonus is CAdvantageActiveBonus state19) ? new CAdvantageActiveBonus(state19, references) : ((cActiveBonus is CAttackActiveBonus state20) ? new CAttackActiveBonus(state20, references) : ((cActiveBonus is CAttackersGainDisadvantageActiveBonus state21) ? new CAttackersGainDisadvantageActiveBonus(state21, references) : ((cActiveBonus is CChangeCharacterModelActiveBonus state22) ? new CChangeCharacterModelActiveBonus(state22, references) : ((cActiveBonus is CChangeConditionActiveBonus state23) ? new CChangeConditionActiveBonus(state23, references) : ((cActiveBonus is CChangeModifierActiveBonus state24) ? new CChangeModifierActiveBonus(state24, references) : ((cActiveBonus is CChooseAbilityActiveBonus state25) ? new CChooseAbilityActiveBonus(state25, references) : ((cActiveBonus is CDamageActiveBonus state26) ? new CDamageActiveBonus(state26, references) : ((cActiveBonus is CDisableCardActionActiveBonus state27) ? new CDisableCardActionActiveBonus(state27, references) : ((cActiveBonus is CDuringActionAbilityActiveBonus state28) ? new CDuringActionAbilityActiveBonus(state28, references) : ((cActiveBonus is CDuringTurnAbilityActiveBonus state29) ? new CDuringTurnAbilityActiveBonus(state29, references) : ((cActiveBonus is CEndActionAbilityActiveBonus state30) ? new CEndActionAbilityActiveBonus(state30, references) : ((cActiveBonus is CEndRoundAbilityActiveBonus state31) ? new CEndRoundAbilityActiveBonus(state31, references) : ((cActiveBonus is CEndTurnAbilityActiveBonus state32) ? new CEndTurnAbilityActiveBonus(state32, references) : ((cActiveBonus is CForgoActionsForCompanionActiveBonus state33) ? new CForgoActionsForCompanionActiveBonus(state33, references) : ((cActiveBonus is CHealthReductionActiveBonus state34) ? new CHealthReductionActiveBonus(state34, references) : ((cActiveBonus is CImmunityActiveBonus state35) ? new CImmunityActiveBonus(state35, references) : ((cActiveBonus is CInfuseActiveBonus state36) ? new CInfuseActiveBonus(state36, references) : ((cActiveBonus is CInvulnerabilityActiveBonus state37) ? new CInvulnerabilityActiveBonus(state37, references) : ((cActiveBonus is CItemLockActiveBonus state38) ? new CItemLockActiveBonus(state38, references) : ((cActiveBonus is CLootActiveBonus state39) ? new CLootActiveBonus(state39, references) : ((cActiveBonus is CMoveActiveBonus state40) ? new CMoveActiveBonus(state40, references) : ((cActiveBonus is COverhealActiveBonus state41) ? new COverhealActiveBonus(state41, references) : ((cActiveBonus is COverrideAbilityTypeActiveBonus state42) ? new COverrideAbilityTypeActiveBonus(state42, references) : ((cActiveBonus is CPierceInvulnerabilityActiveBonus state43) ? new CPierceInvulnerabilityActiveBonus(state43, references) : ((cActiveBonus is CPreventDamageActiveBonus state44) ? new CPreventDamageActiveBonus(state44, references) : ((cActiveBonus is CRedirectActiveBonus state45) ? new CRedirectActiveBonus(state45, references) : ((cActiveBonus is CRetaliateActiveBonus state46) ? new CRetaliateActiveBonus(state46, references) : ((cActiveBonus is CShieldActiveBonus state47) ? new CShieldActiveBonus(state47, references) : ((cActiveBonus is CStartActionAbilityActiveBonus state48) ? new CStartActionAbilityActiveBonus(state48, references) : ((cActiveBonus is CStartRoundAbilityActiveBonus state49) ? new CStartRoundAbilityActiveBonus(state49, references) : ((cActiveBonus is CStartTurnAbilityActiveBonus state50) ? new CStartTurnAbilityActiveBonus(state50, references) : ((cActiveBonus is CSummonActiveBonus state51) ? new CSummonActiveBonus(state51, references) : ((!(cActiveBonus is CUntargetableActiveBonus state52)) ? new CActiveBonus(cActiveBonus, references) : new CUntargetableActiveBonus(state52, references))))))))))))))))))))))))))))))))))))))));
					cActiveBonus2 = cActiveBonus3;
					references.Add(cActiveBonus, cActiveBonus2);
				}
				m_StackedInlineAbilityActiveBonuses.Add(cActiveBonus2);
			}
			references.Add(state.m_StackedInlineAbilityActiveBonuses, m_StackedInlineAbilityActiveBonuses);
		}
		m_FirstAttackofAction = state.m_FirstAttackofAction;
		m_ModifiersChecked = state.m_ModifiersChecked;
		m_InlineModifiersChecked = state.m_InlineModifiersChecked;
		m_ModifierAbilities = references.Get(state.m_ModifierAbilities);
		if (m_ModifierAbilities == null && state.m_ModifierAbilities != null)
		{
			m_ModifierAbilities = new List<CAbility>();
			for (int num = 0; num < state.m_ModifierAbilities.Count; num++)
			{
				CAbility cAbility = state.m_ModifierAbilities[num];
				CAbility cAbility2 = references.Get(cAbility);
				if (cAbility2 == null && cAbility != null)
				{
					CAbility cAbility3 = ((cAbility is CAbilityBlockHealing state53) ? new CAbilityBlockHealing(state53, references) : ((cAbility is CAbilityNeutralizeShield state54) ? new CAbilityNeutralizeShield(state54, references) : ((cAbility is CAbilityAdvantage state55) ? new CAbilityAdvantage(state55, references) : ((cAbility is CAbilityBless state56) ? new CAbilityBless(state56, references) : ((cAbility is CAbilityCurse state57) ? new CAbilityCurse(state57, references) : ((cAbility is CAbilityDisarm state58) ? new CAbilityDisarm(state58, references) : ((cAbility is CAbilityImmobilize state59) ? new CAbilityImmobilize(state59, references) : ((cAbility is CAbilityImmovable state60) ? new CAbilityImmovable(state60, references) : ((cAbility is CAbilityInvisible state61) ? new CAbilityInvisible(state61, references) : ((cAbility is CAbilityMuddle state62) ? new CAbilityMuddle(state62, references) : ((cAbility is CAbilityOverheal state63) ? new CAbilityOverheal(state63, references) : ((cAbility is CAbilityPoison state64) ? new CAbilityPoison(state64, references) : ((cAbility is CAbilitySleep state65) ? new CAbilitySleep(state65, references) : ((cAbility is CAbilityStopFlying state66) ? new CAbilityStopFlying(state66, references) : ((cAbility is CAbilityStrengthen state67) ? new CAbilityStrengthen(state67, references) : ((cAbility is CAbilityStun state68) ? new CAbilityStun(state68, references) : ((cAbility is CAbilityWound state69) ? new CAbilityWound(state69, references) : ((cAbility is CAbilityChooseAbility state70) ? new CAbilityChooseAbility(state70, references) : ((cAbility is CAbilityAddActiveBonus state71) ? new CAbilityAddActiveBonus(state71, references) : ((cAbility is CAbilityAddAugment state72) ? new CAbilityAddAugment(state72, references) : ((cAbility is CAbilityAddCondition state73) ? new CAbilityAddCondition(state73, references) : ((cAbility is CAbilityAddDoom state74) ? new CAbilityAddDoom(state74, references) : ((cAbility is CAbilityAddDoomSlots state75) ? new CAbilityAddDoomSlots(state75, references) : ((cAbility is CAbilityAddHeal state76) ? new CAbilityAddHeal(state76, references) : ((cAbility is CAbilityAddRange state77) ? new CAbilityAddRange(state77, references) : ((cAbility is CAbilityAddSong state78) ? new CAbilityAddSong(state78, references) : ((cAbility is CAbilityAddTarget state79) ? new CAbilityAddTarget(state79, references) : ((cAbility is CAbilityAdjustInitiative state80) ? new CAbilityAdjustInitiative(state80, references) : ((cAbility is CAbilityAttackersGainDisadvantage state81) ? new CAbilityAttackersGainDisadvantage(state81, references) : ((cAbility is CAbilityChangeAllegiance state82) ? new CAbilityChangeAllegiance(state82, references) : ((cAbility is CAbilityChangeCharacterModel state83) ? new CAbilityChangeCharacterModel(state83, references) : ((cAbility is CAbilityChoose state84) ? new CAbilityChoose(state84, references) : ((cAbility is CAbilityConsume state85) ? new CAbilityConsume(state85, references) : ((cAbility is CAbilityConsumeItemCards state86) ? new CAbilityConsumeItemCards(state86, references) : ((cAbility is CAbilityControlActor state87) ? new CAbilityControlActor(state87, references) : ((cAbility is CAbilityDisableCardAction state88) ? new CAbilityDisableCardAction(state88, references) : ((cAbility is CAbilityDiscardCards state89) ? new CAbilityDiscardCards(state89, references) : ((cAbility is CAbilityExtraTurn state90) ? new CAbilityExtraTurn(state90, references) : ((cAbility is CAbilityForgoActionsForCompanion state91) ? new CAbilityForgoActionsForCompanion(state91, references) : ((cAbility is CAbilityGiveSupplyCard state92) ? new CAbilityGiveSupplyCard(state92, references) : ((cAbility is CAbilityHeal state93) ? new CAbilityHeal(state93, references) : ((cAbility is CAbilityHealthReduction state94) ? new CAbilityHealthReduction(state94, references) : ((cAbility is CAbilityImmunityTo state95) ? new CAbilityImmunityTo(state95, references) : ((cAbility is CAbilityImprovedShortRest state96) ? new CAbilityImprovedShortRest(state96, references) : ((cAbility is CAbilityIncreaseCardLimit state97) ? new CAbilityIncreaseCardLimit(state97, references) : ((cAbility is CAbilityInvulnerability state98) ? new CAbilityInvulnerability(state98, references) : ((cAbility is CAbilityItemLock state99) ? new CAbilityItemLock(state99, references) : ((cAbility is CAbilityKill state100) ? new CAbilityKill(state100, references) : ((cAbility is CAbilityLoseCards state101) ? new CAbilityLoseCards(state101, references) : ((cAbility is CAbilityLoseGoalChestReward state102) ? new CAbilityLoseGoalChestReward(state102, references) : ((cAbility is CAbilityNullTargeting state103) ? new CAbilityNullTargeting(state103, references) : ((cAbility is CAbilityOverrideAugmentAttackType state104) ? new CAbilityOverrideAugmentAttackType(state104, references) : ((cAbility is CAbilityPierceInvulnerability state105) ? new CAbilityPierceInvulnerability(state105, references) : ((cAbility is CAbilityPlaySong state106) ? new CAbilityPlaySong(state106, references) : ((cAbility is CAbilityPreventDamage state107) ? new CAbilityPreventDamage(state107, references) : ((cAbility is CAbilityRecoverDiscardedCards state108) ? new CAbilityRecoverDiscardedCards(state108, references) : ((cAbility is CAbilityRecoverLostCards state109) ? new CAbilityRecoverLostCards(state109, references) : ((cAbility is CAbilityRedirect state110) ? new CAbilityRedirect(state110, references) : ((cAbility is CAbilityRefreshItemCards state111) ? new CAbilityRefreshItemCards(state111, references) : ((cAbility is CAbilityRemoveActorFromMap state112) ? new CAbilityRemoveActorFromMap(state112, references) : ((cAbility is CAbilityRemoveConditions state113) ? new CAbilityRemoveConditions(state113, references) : ((cAbility is CAbilityRetaliate state114) ? new CAbilityRetaliate(state114, references) : ((cAbility is CAbilityShield state115) ? new CAbilityShield(state115, references) : ((cAbility is CAbilityShuffleModifierDeck state116) ? new CAbilityShuffleModifierDeck(state116, references) : ((cAbility is CAbilityTransferDooms state117) ? new CAbilityTransferDooms(state117, references) : ((cAbility is CAbilityUntargetable state118) ? new CAbilityUntargetable(state118, references) : ((cAbility is CAbilityCondition state119) ? new CAbilityCondition(state119, references) : ((cAbility is CAbilityMergedCreateAttack state120) ? new CAbilityMergedCreateAttack(state120, references) : ((cAbility is CAbilityMergedDestroyAttack state121) ? new CAbilityMergedDestroyAttack(state121, references) : ((cAbility is CAbilityMergedDisarmTrapDestroyObstacle state122) ? new CAbilityMergedDisarmTrapDestroyObstacle(state122, references) : ((cAbility is CAbilityMergedKillCreate state123) ? new CAbilityMergedKillCreate(state123, references) : ((cAbility is CAbilityMergedMoveAttack state124) ? new CAbilityMergedMoveAttack(state124, references) : ((cAbility is CAbilityMergedMoveObstacleAttack state125) ? new CAbilityMergedMoveObstacleAttack(state125, references) : ((cAbility is CAbilityActivateSpawner state126) ? new CAbilityActivateSpawner(state126, references) : ((cAbility is CAbilityAddModifierToMonsterDeck state127) ? new CAbilityAddModifierToMonsterDeck(state127, references) : ((cAbility is CAbilityAttack state128) ? new CAbilityAttack(state128, references) : ((cAbility is CAbilityChangeCondition state129) ? new CAbilityChangeCondition(state129, references) : ((cAbility is CAbilityChangeModifier state130) ? new CAbilityChangeModifier(state130, references) : ((cAbility is CAbilityConsumeElement state131) ? new CAbilityConsumeElement(state131, references) : ((cAbility is CAbilityCreate state132) ? new CAbilityCreate(state132, references) : ((cAbility is CAbilityDamage state133) ? new CAbilityDamage(state133, references) : ((cAbility is CAbilityDeactivateSpawner state134) ? new CAbilityDeactivateSpawner(state134, references) : ((cAbility is CAbilityDestroyObstacle state135) ? new CAbilityDestroyObstacle(state135, references) : ((cAbility is CAbilityDisarmTrap state136) ? new CAbilityDisarmTrap(state136, references) : ((cAbility is CAbilityFear state137) ? new CAbilityFear(state137, references) : ((cAbility is CAbilityInfuse state138) ? new CAbilityInfuse(state138, references) : ((cAbility is CAbilityLoot state139) ? new CAbilityLoot(state139, references) : ((cAbility is CAbilityMove state140) ? new CAbilityMove(state140, references) : ((cAbility is CAbilityMoveObstacle state141) ? new CAbilityMoveObstacle(state141, references) : ((cAbility is CAbilityMoveTrap state142) ? new CAbilityMoveTrap(state142, references) : ((cAbility is CAbilityNull state143) ? new CAbilityNull(state143, references) : ((cAbility is CAbilityNullHex state144) ? new CAbilityNullHex(state144, references) : ((cAbility is CAbilityPull state145) ? new CAbilityPull(state145, references) : ((cAbility is CAbilityPush state146) ? new CAbilityPush(state146, references) : ((cAbility is CAbilityRecoverResources state147) ? new CAbilityRecoverResources(state147, references) : ((cAbility is CAbilityRedistributeDamage state148) ? new CAbilityRedistributeDamage(state148, references) : ((cAbility is CAbilityRevive state149) ? new CAbilityRevive(state149, references) : ((cAbility is CAbilitySummon state150) ? new CAbilitySummon(state150, references) : ((cAbility is CAbilitySwap state151) ? new CAbilitySwap(state151, references) : ((cAbility is CAbilityTargeting state152) ? new CAbilityTargeting(state152, references) : ((cAbility is CAbilityTeleport state153) ? new CAbilityTeleport(state153, references) : ((cAbility is CAbilityTrap state154) ? new CAbilityTrap(state154, references) : ((cAbility is CAbilityXP state155) ? new CAbilityXP(state155, references) : ((!(cAbility is CAbilityMerged state156)) ? new CAbility(cAbility, references) : new CAbilityMerged(state156, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility2 = cAbility3;
					references.Add(cAbility, cAbility2);
				}
				m_ModifierAbilities.Add(cAbility2);
			}
			references.Add(state.m_ModifierAbilities, m_ModifierAbilities);
		}
		m_PreviousAttackModsForTarget = references.Get(state.m_PreviousAttackModsForTarget);
		if (m_PreviousAttackModsForTarget != null || state.m_PreviousAttackModsForTarget == null)
		{
			return;
		}
		m_PreviousAttackModsForTarget = new List<AttackModifierYMLData>();
		for (int num2 = 0; num2 < state.m_PreviousAttackModsForTarget.Count; num2++)
		{
			AttackModifierYMLData attackModifierYMLData = state.m_PreviousAttackModsForTarget[num2];
			AttackModifierYMLData attackModifierYMLData2 = references.Get(attackModifierYMLData);
			if (attackModifierYMLData2 == null && attackModifierYMLData != null)
			{
				attackModifierYMLData2 = new AttackModifierYMLData(attackModifierYMLData, references);
				references.Add(attackModifierYMLData, attackModifierYMLData2);
			}
			m_PreviousAttackModsForTarget.Add(attackModifierYMLData2);
		}
		references.Add(state.m_PreviousAttackModsForTarget, m_PreviousAttackModsForTarget);
	}
}
