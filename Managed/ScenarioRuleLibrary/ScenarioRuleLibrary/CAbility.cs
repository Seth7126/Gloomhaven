using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[DebuggerDisplay("{Name}")]
public class CAbility
{
	public enum EAbilityType
	{
		None,
		Move,
		Attack,
		Heal,
		Damage,
		Loot,
		PreventDamage,
		Shield,
		Retaliate,
		Trap,
		Create,
		RecoverLostCards,
		Invisible,
		Poison,
		Wound,
		Muddle,
		Immobilize,
		Disarm,
		Curse,
		Stun,
		Strengthen,
		Bless,
		Consume,
		Infuse,
		RefreshItemCards,
		RecoverDiscardedCards,
		Summon,
		Advantage,
		Teleport,
		Kill,
		DisarmTrap,
		DestroyObstacle,
		Redirect,
		AddTarget,
		Push,
		Pull,
		AddHeal,
		AddRange,
		ControlActor,
		AddAugment,
		MergedCreateAttack,
		MergedDestroyObstacleAttack,
		MergedMoveAttack,
		MoveObstacle,
		MergedMoveObstacleAttack,
		MergedDisarmTrapDestroyObstacles,
		MergedKillCreate,
		AttackersGainDisadvantage,
		AddCondition,
		AddSong,
		PlaySong,
		AddActiveBonus,
		OverrideAugmentAttackType,
		Null,
		ImmunityTo,
		ConsumeElement,
		ChooseAbility,
		LoseCards,
		IncreaseCardLimit,
		Augment,
		StopFlying,
		ExtraTurn,
		ImprovedShortRest,
		Overheal,
		GainXP,
		AdjustInitiative,
		Swap,
		RedistributeDamage,
		ForgoActionsForCompanion,
		ChangeModifier,
		Immovable,
		ChangeCondition,
		RemoveConditions,
		ShuffleModifierDeck,
		DiscardCards,
		NullTargeting,
		AddDoom,
		AddDoomSlots,
		TransferDooms,
		Cure,
		Choose,
		GiveSupplyCard,
		LongRest,
		ShortRest,
		Fear,
		DeactivateSpawner,
		RemoveActorFromMap,
		ConsumeItemCards,
		Invulnerability,
		PierceInvulnerability,
		Sleep,
		ActivateSpawner,
		Untargetable,
		ItemLock,
		LoseGoalChestReward,
		AddModifierToMonsterDeck,
		RecoverResources,
		MoveTrap,
		BlockHealing,
		NeutralizeShield,
		ChangeCharacterModel,
		NullHex,
		HealthReduction,
		ChangeAllegiance,
		Revive,
		DisableCardAction
	}

	public enum EAttackType
	{
		None,
		Attack,
		Melee,
		Ranged,
		Default,
		Control,
		ControlledByCaster,
		Augment
	}

	public enum EAbilityStatType
	{
		None,
		Strength,
		Range,
		NumberOfTargets
	}

	public enum EAbilityTargeting
	{
		None,
		Range,
		Room,
		All,
		AllConnectedRooms
	}

	public enum EStatIsBasedOnXType
	{
		None,
		HexesMovedThisTurn,
		HexesMovedThisAction,
		HexesMovedByParentAbility,
		DamageInflictedThisTurn,
		DamageInflictedThisAction,
		DamageInflictedByParent,
		ObstaclesDestroyedThisTurn,
		ObstaclesDestroyedThisAction,
		ObstaclesDestroyedByParent,
		ObstaclesToBeDestroyedByMergedAbility,
		TargetAdjacentEnemies,
		TargetAdjacentAllies,
		CasterAdjacentEnemies,
		CasterAdjacentAllies,
		DamageInflictedByParentOnLastTarget,
		LostCardCount,
		PercentageOfCurrentHP,
		HPDifference,
		TargetsDamagedByPreviousAttackThisTurn,
		TargetsDamagedByPreviousAttackThisAction,
		TargetsDamagedByPreviousDamageAbilityThisTurn,
		TargetsDamagedByPreviousDamageAbilityThisAction,
		HalfTargetsDamagedByPreviousDamageAbilityThisAction,
		ActorCount,
		DeadActorCount,
		ScenarioLevel,
		TargetHPDifference,
		TargetDamageTaken,
		TargetDistanceRangeDiff,
		InitialPlayerCharacterCount,
		XAddedToCharactersTimesLevel,
		CharactersTimesLevelPlusX,
		XAddedToYTimesLevel,
		XPlusCharactersPlusLevel,
		XPlusLevel,
		XAddedToLevelTimesCharactersOverY,
		LevelAddedToXTimesCharacters,
		XAddedToInitialPlayerCharacterCount,
		XTimesInitialPlayerCharacterCountPlusLevelMinusY,
		XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound,
		CharactersPlusLevelOverX,
		TargetAdjacentEnemiesOfTarget,
		TargetAdjacentAlliesOfTarget,
		CasterAdjacentEnemiesOfTarget,
		CasterAdjacentAlliesOfTarget,
		TargetAdjacentEnemiesOfCaster,
		TargetAdjacentAlliesOfCaster,
		CasterAdjacentEnemiesOfCaster,
		CasterAdjacentAlliesOfCaster,
		XPlusCharactersPlusLevelTimesY,
		TargetAdjacentActors,
		XTimesInitialPlayerCharacterCountMinusY,
		XAddedToLevelThenTimesCharactersOverY,
		LevelTimesXTimesHexesMovedThisTurnInDefinedRoom,
		LevelTimesXPlusY,
		ActorsTargetedByParentAbility,
		ActorsKilledByParentAbility,
		TargetAdjacentWalls,
		CasterAdjacentWalls,
		TargetAdjacentValidTiles,
		CasterAdjacentValidTiles,
		ExcessDamageInflictedByParentOnLastTargetKilled,
		ActorsKilledThisAction,
		ActorsKilledThisTurn,
		ActorsKilledThisRound,
		ActorsKilledThisScenario,
		CasterShieldValue,
		TargetShieldValue,
		XTimesActorCountMinusY,
		DamageInflictedByPreviousAbility,
		InitialPlayerCharacterCountMinusYTimesX,
		DamageActuallyTakenByParent,
		TargetsActuallyDamagedByPreviousAttackThisTurn
	}

	public enum EStatIsBasedOnXRoundingType
	{
		None,
		RoundOff,
		ToFloor,
		ToCeil
	}

	public static List<EStatIsBasedOnXType> StatIsXTypesToIgnoreIfAddTo = new List<EStatIsBasedOnXType> { EStatIsBasedOnXType.HexesMovedThisTurn };

	public static List<EStatIsBasedOnXType> StatIsBasedOnXTypesForBaseStats = new List<EStatIsBasedOnXType>
	{
		EStatIsBasedOnXType.InitialPlayerCharacterCount,
		EStatIsBasedOnXType.XAddedToCharactersTimesLevel,
		EStatIsBasedOnXType.CharactersTimesLevelPlusX,
		EStatIsBasedOnXType.XAddedToYTimesLevel,
		EStatIsBasedOnXType.XPlusCharactersPlusLevel,
		EStatIsBasedOnXType.XPlusLevel,
		EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY,
		EStatIsBasedOnXType.LevelAddedToXTimesCharacters,
		EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount,
		EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY,
		EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound,
		EStatIsBasedOnXType.CharactersPlusLevelOverX,
		EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY,
		EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY,
		EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY,
		EStatIsBasedOnXType.LevelTimesXPlusY,
		EStatIsBasedOnXType.InitialPlayerCharacterCountMinusYTimesX
	};

	public static EAttackType[] AttackTypes = (EAttackType[])Enum.GetValues(typeof(EAttackType));

	public static EAbilityTargeting[] AbilityTargetingTypes = (EAbilityTargeting[])Enum.GetValues(typeof(EAbilityTargeting));

	public static EAbilityType[] AbilityTypes = (EAbilityType[])Enum.GetValues(typeof(EAbilityType));

	public static EStatIsBasedOnXType[] StatIsBasedOnXTypes = (EStatIsBasedOnXType[])Enum.GetValues(typeof(EStatIsBasedOnXType));

	public static EAbilityStatType[] AbilityStatTypes = (EAbilityStatType[])Enum.GetValues(typeof(EAbilityStatType));

	public static Dictionary<EAbilityType, EAbilityStatType> AbilityDisplayValues = GetAbilityDisplayValues();

	public static EStatIsBasedOnXRoundingType[] RoundingTypes = (EStatIsBasedOnXRoundingType[])Enum.GetValues(typeof(EStatIsBasedOnXRoundingType));

	protected List<CActor> m_ValidActorsInRange = new List<CActor>();

	protected bool m_IsSubAbility;

	protected bool m_IsMergedAbility;

	protected bool m_IsModifierAbility;

	protected int m_Strength;

	protected int m_ModifiedStrength;

	protected int m_Range;

	protected int m_OriginalRange;

	protected int m_NumberTargets;

	protected int m_OriginalTargetCount;

	protected List<CActor> m_ActorsToIgnore = new List<CActor>();

	protected List<CEnhancement> m_AbilityEnhancements;

	protected Dictionary<CCondition.EPositiveCondition, CAbility> m_PositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>();

	protected Dictionary<CCondition.ENegativeCondition, CAbility> m_NegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();

	protected List<CTile> m_TilesSelected = new List<CTile>();

	protected AbilityData.XpPerTargetData m_XpPerTargetData;

	protected bool m_CanUndo = true;

	protected bool m_CanSkip = true;

	protected List<CActor> m_ActorsToTarget;

	protected int m_NumberTargetsRemaining;

	protected int m_UndoNumberTargetsRemaining;

	protected bool m_ProcessIfDead;

	protected bool m_AllTargets;

	protected bool m_CancelAbility;

	protected bool m_AbilityHasHappened;

	protected bool m_AbilityStartComplete;

	protected Dictionary<CAbilityOverride, CItem> m_CurrentItemOverrides = new Dictionary<CAbilityOverride, CItem>();

	private List<CAbilityOverride> m_CurrentOverrides = new List<CAbilityOverride>();

	protected List<CItem> m_ActiveSingleTargetItems = new List<CItem>();

	protected List<CActiveBonus> m_ActiveSingleTargetActiveBonuses = new List<CActiveBonus>();

	protected List<CTile> m_ValidTilesInAreaEffect = new List<CTile>();

	protected List<CTile> m_ValidTilesInAreaEffectIncludingBlocked = new List<CTile>();

	protected List<CTile> m_InlineSubAbilityTiles = new List<CTile>();

	protected CAreaEffect m_AreaEffectBackup;

	protected bool m_AreaEffectLocked;

	protected float m_AreaEffectAngle;

	protected bool m_IsScenarioModifierAbility;

	protected bool m_AugmentsAdded;

	protected bool m_AugmentAbilitiesProcessed;

	protected int m_AugmentAbilitiesTargetCount;

	protected bool m_AugmentAbilitiesNextTarget;

	protected bool m_AugmentOverridesProcessed;

	protected List<CAugment> m_AbilityAugments = new List<CAugment>();

	protected List<CAugment> m_OverrideAugments = new List<CAugment>();

	protected bool m_SongAbilitiesProcessed;

	protected int m_SongAbilitiesTargetCount;

	protected bool m_SongAbilitiesNextTarget;

	protected bool m_SongOverridesProcessed;

	protected List<CSong.SongEffect> m_AbilitySongs = new List<CSong.SongEffect>();

	protected List<CSong.SongEffect> m_OverrideSongs = new List<CSong.SongEffect>();

	private bool m_IsControlAbility;

	private List<ElementInfusionBoardManager.EElement> m_InfuseElements = new List<ElementInfusionBoardManager.EElement>();

	public Guid ID { get; private set; }

	public string Name { get; private set; }

	public EAbilityType AbilityType { get; private set; }

	public string AnimOverload { get; set; }

	public int Strength
	{
		get
		{
			return m_Strength;
		}
		set
		{
			m_Strength = value;
		}
	}

	public int Range
	{
		get
		{
			return m_Range;
		}
		set
		{
			m_Range = value;
		}
	}

	public int NumberTargets
	{
		get
		{
			return m_NumberTargets;
		}
		set
		{
			m_NumberTargets = value;
		}
	}

	public int OriginalTargetCount
	{
		get
		{
			return m_OriginalTargetCount;
		}
		set
		{
			m_OriginalTargetCount = value;
		}
	}

	public CAbilityFilterContainer AbilityFilter { get; set; }

	public CAbilityFilter.EFilterTile TileFilter { get; set; }

	public EAbilityTargeting Targeting { get; set; }

	public bool ProcessIfDead
	{
		get
		{
			return m_ProcessIfDead;
		}
		set
		{
			m_ProcessIfDead = value;
		}
	}

	public AbilityData.XpPerTargetData XpPerTargetData => m_XpPerTargetData;

	public bool AllTargetsOnMovePath { get; private set; }

	public bool AllTargetsOnMovePathSameStartAndEnd { get; private set; }

	public bool AllTargetsOnAttackPath { get; private set; }

	public bool IsTargetedAbility { get; private set; }

	public float SpawnDelay { get; private set; }

	public List<CConditionalOverride> ConditionalOverrides { get; private set; }

	public List<CConditionalOverride> ActiveConditionalOverrides { get; private set; }

	public CAbilityRequirements StartAbilityRequirements { get; private set; }

	public int AbilityXP { get; set; }

	public AbilityData.MiscAbilityData MiscAbilityData { get; set; }

	public bool SkipAnim { get; private set; }

	public bool IsItemAbility { get; set; }

	public bool UseSpecialBaseStat { get; private set; }

	public string PreviewEffectId { get; private set; }

	public string PreviewEffectText { get; private set; }

	public string HelpBoxTooltipLocKey { get; private set; }

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; set; }

	public CAreaEffect AreaEffect { get; protected set; }

	public float AreaEffectAngle
	{
		get
		{
			return m_AreaEffectAngle;
		}
		set
		{
			m_AreaEffectAngle = value;
		}
	}

	public bool AreaEffectLocked => m_AreaEffectLocked;

	public string AreaEffectYMLString { get; private set; }

	public string AreaEffectLayoutOverrideYMLString { get; private set; }

	public AbilityData.ActiveBonusData ActiveBonusData { get; private set; }

	public ActiveBonusLayout ActiveBonusYML { get; set; }

	public List<CEnhancement> AbilityEnhancements => m_AbilityEnhancements;

	public bool EnhancementsApplied { get; private set; }

	public Dictionary<CCondition.EPositiveCondition, CAbility> PositiveConditions
	{
		get
		{
			return m_PositiveConditions;
		}
		set
		{
			m_PositiveConditions = value;
		}
	}

	public Dictionary<CCondition.ENegativeCondition, CAbility> NegativeConditions
	{
		get
		{
			return m_NegativeConditions;
		}
		set
		{
			m_NegativeConditions = value;
		}
	}

	public Dictionary<string, int> ResourcesToAddOnAbilityEnd { get; set; }

	public Dictionary<string, int> ResourcesToTakeFromTargets { get; set; }

	public Dictionary<string, int> ResourcesToGiveToTargets { get; set; }

	public CAugment Augment { get; private set; }

	public CSong Song { get; private set; }

	public bool IsConsumeAbility { get; private set; }

	public string ParentName { get; private set; }

	public bool IsSubAbility
	{
		get
		{
			return m_IsSubAbility;
		}
		set
		{
			m_IsSubAbility = value;
		}
	}

	public bool UseSubAbilityTargeting
	{
		get
		{
			if (m_IsSubAbility)
			{
				if (MiscAbilityData == null || !MiscAbilityData.TreatAsNonSubAbility.HasValue)
				{
					return true;
				}
				return !MiscAbilityData.TreatAsNonSubAbility.Value;
			}
			return false;
		}
	}

	public bool IsInlineSubAbility { get; set; }

	public List<CAbility> SubAbilities { get; private set; }

	public bool IsMergedAbility => m_IsMergedAbility;

	public bool IsModifierAbility => m_IsModifierAbility;

	public bool AddAttackBaseStat { get; private set; }

	public bool StrengthIsBase { get; private set; }

	public bool RangeIsBase { get; private set; }

	public bool TargetIsBase { get; private set; }

	public bool IsMonsterAbility { get; private set; }

	public bool TargetsSet { get; private set; }

	public string AbilityText { get; private set; }

	public bool AbilityTextOnly { get; private set; }

	public bool ShowRange { get; private set; }

	public bool ShowTarget { get; private set; }

	public bool ShowArea { get; private set; }

	public bool OnDeath { get; set; }

	public bool StackedAttackEffectAbility { get; set; }

	public bool IsUpdating { get; protected set; }

	public List<CActor> ValidActorsInRange
	{
		get
		{
			return m_ValidActorsInRange;
		}
		set
		{
			m_ValidActorsInRange = value;
		}
	}

	public CActor TargetingActor { get; set; }

	public CActor OriginalTargetingActor { get; set; }

	public CActor FilterActor { get; set; }

	public CActor ControllingActor { get; private set; }

	public List<CTile> TilesSelected => m_TilesSelected;

	public List<CActor> ActorsToTarget => m_ActorsToTarget;

	public int NumberTargetsRemaining => m_NumberTargetsRemaining;

	public List<CTile> TilesInRange { get; set; }

	public bool CanUndo => m_CanUndo;

	public bool IsControlAbility
	{
		get
		{
			return m_IsControlAbility;
		}
		set
		{
			m_IsControlAbility = value;
		}
	}

	public bool CanSkip => m_CanSkip;

	public List<CTile> AllPossibleTilesInAreaEffect { get; set; }

	public List<CActor> ActorsToIgnore
	{
		get
		{
			return m_ActorsToIgnore;
		}
		set
		{
			m_ActorsToIgnore = value;
		}
	}

	public bool AllTargets => m_AllTargets;

	public List<CItem> ActiveSingleTargetItems => m_ActiveSingleTargetItems;

	public List<CActiveBonus> ActiveSingleTargetActiveBonuses => m_ActiveSingleTargetActiveBonuses;

	public List<CItem> ActiveOverrideItems => m_CurrentItemOverrides.Values.ToList();

	public List<CTile> ValidTilesInAreaAffected => m_ValidTilesInAreaEffect;

	public List<CTile> ValidTilesInAreaEffectedIncludingBlocked => m_ValidTilesInAreaEffectIncludingBlocked;

	public bool AbilityHasHappened
	{
		get
		{
			return m_AbilityHasHappened;
		}
		set
		{
			m_AbilityHasHappened = value;
		}
	}

	public bool AbilityHasBeenCancelled => m_CancelAbility;

	public bool AugmentsAdded => m_AugmentsAdded;

	public List<CActor> ActorsTargeted { get; protected set; }

	public CAreaEffect AreaEffectBackup => m_AreaEffectBackup;

	public int DamageInflictedByAbility { get; set; }

	public int DamageActuallyTakenByAbility { get; set; }

	public bool? CanUndoOverride { get; set; }

	public int DamageInflictedByAbilityOnLastTarget { get; set; }

	public int ExcessDamageInflictedOnLastTargetKilled { get; set; }

	public bool AppliedEnhancements { get; set; }

	public bool AbilityStartListenersInvoked { get; set; }

	public CAbility ParentAbility { get; set; }

	public List<CTile> InlineSubAbilityTiles
	{
		get
		{
			return m_InlineSubAbilityTiles;
		}
		set
		{
			m_InlineSubAbilityTiles = value;
		}
	}

	public CActor TargetThisActorAutomatically { get; set; }

	public List<CAbilityOverride> CurrentOverrides => m_CurrentOverrides;

	public CBaseCard AbilityBaseCard => TargetingActor?.FindCardWithAbility(this);

	public CBaseCard ParentAbilityBaseCard { get; set; }

	public bool IsScenarioModifierAbility
	{
		get
		{
			return m_IsScenarioModifierAbility;
		}
		set
		{
			m_IsScenarioModifierAbility = value;
		}
	}

	public List<CEnhancement> ActiveEnhancements => m_AbilityEnhancements.Where((CEnhancement w) => w.Enhancement != EEnhancement.NoEnhancement).ToList();

	public List<CEnhancement> AvailableEnhancementSlots => m_AbilityEnhancements.Where((CEnhancement w) => w.Enhancement == EEnhancement.NoEnhancement).ToList();

	public virtual bool Perform()
	{
		return false;
	}

	public virtual void Update()
	{
	}

	public virtual void PerformImmediate()
	{
	}

	public virtual void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		CFinishedProcessingTileSelected_MessageData message = new CFinishedProcessingTileSelected_MessageData
		{
			m_SelectedTile = selectedTile,
			m_OptionalTileList = optionalTileList,
			m_Ability = this,
			m_AreaEffectLocked = m_AreaEffectLocked
		};
		ScenarioRuleClient.MessageHandler(message, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
	}

	public virtual void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		CFinishedProcessingTileDeselected_MessageData message = new CFinishedProcessingTileDeselected_MessageData
		{
			m_SelectedTile = selectedTile,
			m_OptionalTileList = optionalTileList,
			m_Ability = this,
			m_CanUndo = m_CanUndo,
			m_AreaEffectLocked = m_AreaEffectLocked
		};
		ScenarioRuleClient.MessageHandler(message, ScenarioRuleClient.s_WorkThread != Thread.CurrentThread);
	}

	public virtual bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		fullAbilityRestart = true;
		return true;
	}

	public virtual void AbilityPassStep()
	{
	}

	public virtual void AbilityEnded()
	{
		if (m_AbilityHasHappened)
		{
			TargetingActor.LastAbilityPerformed = this;
			TargetingActor.AbilityTypesPerformedThisTurn.Add(AbilityType);
			TargetingActor.AbilityTypesPerformedThisAction.Add(AbilityType);
			if (ActiveBonusData != null && ActiveBonusData.OverrideAsSong)
			{
				TargetingActor.AbilityTypesPerformedThisTurn.Add(EAbilityType.PlaySong);
				TargetingActor.AbilityTypesPerformedThisAction.Add(EAbilityType.PlaySong);
			}
			if (ControllingActor != null)
			{
				ControllingActor.GainXP(AbilityXP);
				CAbilityRequirements startAbilityRequirements = StartAbilityRequirements;
				if (startAbilityRequirements != null && startAbilityRequirements.XP > 0)
				{
					ControllingActor.GainXP(StartAbilityRequirements.XP);
				}
				if (ActiveConditionalOverrides.Count > 0)
				{
					foreach (CConditionalOverride activeConditionalOverride in ActiveConditionalOverrides)
					{
						if (activeConditionalOverride.XP > 0)
						{
							ControllingActor.GainXP(activeConditionalOverride.XP);
						}
					}
				}
			}
			else
			{
				TargetingActor.GainXP(AbilityXP);
				CAbilityRequirements startAbilityRequirements2 = StartAbilityRequirements;
				if (startAbilityRequirements2 != null && startAbilityRequirements2.XP > 0)
				{
					TargetingActor.GainXP(StartAbilityRequirements.XP);
				}
				if (ActiveConditionalOverrides.Count > 0)
				{
					foreach (CConditionalOverride activeConditionalOverride2 in ActiveConditionalOverrides)
					{
						if (activeConditionalOverride2.XP > 0)
						{
							TargetingActor.GainXP(activeConditionalOverride2.XP);
						}
					}
				}
			}
			if (m_InfuseElements.Count > 0)
			{
				List<CAbility> list = new List<CAbility>();
				CActor cActor = ((ControllingActor != null) ? ControllingActor : ((TargetingActor is CHeroSummonActor cHeroSummonActor) ? cHeroSummonActor.Summoner : TargetingActor));
				CBaseCard parentBaseCard = cActor.FindCardWithAbility(this);
				list.Add(CreateAbility(EAbilityType.Infuse, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), IsMonsterAbility, isTargetedAbility: false, 0, 1, 1, 1, EConditionDecTrigger.Turns, m_InfuseElements, showElementPicker: true, parentBaseCard));
				(PhaseManager.CurrentPhase as CPhaseAction).StackNextAbilities(list, cActor, killAfter: false, stackToNextCurrent: false, copyCurrentActionID: true);
			}
			if (ResourcesToAddOnAbilityEnd != null && ResourcesToAddOnAbilityEnd.Count > 0 && TargetingActor is CPlayerActor cPlayerActor)
			{
				foreach (KeyValuePair<string, int> item in ResourcesToAddOnAbilityEnd)
				{
					cPlayerActor.AddCharacterResource(item.Key, item.Value);
				}
			}
		}
		CAbilityEndUpdateCombatLog_MessageData message = new CAbilityEndUpdateCombatLog_MessageData(GameState.InternalCurrentActor)
		{
			m_AbilityType = AbilityType,
			m_Ability = this
		};
		ScenarioRuleClient.MessageHandler(message);
		if (MiscAbilityData != null && MiscAbilityData.SetHPTo.HasValue && MiscAbilityData.SetHPTo.Value > 0 && MiscAbilityData.SetHPTo.Value <= GameState.InternalCurrentActor.OriginalMaxHealth)
		{
			GameState.InternalCurrentActor.ApplyImmediateDamage(GameState.InternalCurrentActor.Health - MiscAbilityData.SetHPTo.Value, cannotPrevent: true);
		}
		AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.ExhaustSelf == true)
		{
			GameState.InternalCurrentActor.ExhaustAfterAction = true;
		}
		CActiveBonus.RefreshAllAuraActiveBonuses();
		CActiveBonus.RefreshOverhealActiveBonuses();
		TargetingActor.CalculateAttackStrengthForUI();
		if (TargetingActor.m_OnAbilityEndedListeners != null)
		{
			TargetingActor.m_OnAbilityEndedListeners(this);
		}
		TargetingActor.ClearCharacterSpecialMechanicsCache(clearAugments: true, clearSongs: true);
		if (!AbilityHasHappened)
		{
			return;
		}
		foreach (CScenarioModifier item2 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where(delegate(CScenarioModifier m)
		{
			if (m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.AfterAbility && (m.AfterAbilityTypes == null || m.AfterAbilityTypes.Contains(AbilityType)))
			{
				CAbilityAttack attackAbility = this as CAbilityAttack;
				if (attackAbility == null)
				{
					return true;
				}
				if (m.AfterAttackTypes == null)
				{
					return true;
				}
				return m.AfterAttackTypes.Any((EAttackType x) => attackAbility.IsAttackType(x));
			}
			return false;
		}))
		{
			item2.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
		}
	}

	public virtual void LogEvent(ESESubTypeAbility subTypeAbility)
	{
	}

	public virtual string GetDescription()
	{
		return "";
	}

	public virtual void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		TargetingActor = targetingActor;
		FilterActor = filterActor;
		ControllingActor = controllingActor;
		m_ActorsToTarget = new List<CActor>();
		ActorsTargeted = new List<CActor>();
		m_TilesSelected = new List<CTile>();
		TilesInRange = new List<CTile>();
		if (ActiveBonusData == null || ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			ProcessSongOverridesAndAbilities(TargetingActor, CSong.ESongActivationType.AbilityStart);
		}
		CheckForScenarioModifiersToTrigger();
		if (targetingActor != null && targetingActor.Class is CMonsterClass cMonsterClass && cMonsterClass.CurrentMonsterStat.SpecialBaseStats.ContainsKey(AbilityType))
		{
			m_Strength = cMonsterClass.CurrentMonsterStat.SpecialBaseStats[AbilityType];
		}
		if (!AppliedEnhancements)
		{
			foreach (CEnhancement abilityEnhancement in AbilityEnhancements)
			{
				ApplyEnhancement(abilityEnhancement);
			}
			AppliedEnhancements = true;
		}
		foreach (CScenarioModifierAddConditionsToAbilities item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.AddConditionsToAbilities))
		{
			if (!item.ShouldAddConditions(TargetingActor, AbilityType))
			{
				continue;
			}
			foreach (CCondition.EPositiveCondition posCondition in item.PositiveConditions)
			{
				EAbilityType abilityType = AbilityTypes.Single((EAbilityType x) => x.ToString() == posCondition.ToString());
				if (!m_PositiveConditions.ContainsKey(posCondition))
				{
					m_PositiveConditions.Add(posCondition, CreateAbility(abilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
			}
			if (CScenarioModifier.IgnoreNegativeScenarioEffects(TargetingActor))
			{
				continue;
			}
			foreach (CCondition.ENegativeCondition negativeCondition in item.NegativeConditions)
			{
				EAbilityType abilityType2 = AbilityTypes.Single((EAbilityType x) => x.ToString() == negativeCondition.ToString());
				if (!m_NegativeConditions.ContainsKey(negativeCondition))
				{
					m_NegativeConditions.Add(negativeCondition, CreateAbility(abilityType2, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
			}
		}
		AbilityFilter.IsValidTarget(TargetingActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible);
		SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
		m_ModifiedStrength = m_Strength;
		if (m_ActorsToIgnore == null)
		{
			m_ActorsToIgnore = new List<CActor>();
		}
		if (MiscAbilityData?.IgnorePreviousAbilityTargets != null)
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.IgnorePreviousAbilityTargets.Count > 0 && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				foreach (string abilityName in MiscAbilityData?.IgnorePreviousAbilityTargets)
				{
					CPhaseAction.CPhaseAbility cPhaseAbility = cPhaseAction.PreviousPhaseAbilities.FirstOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == abilityName);
					if (cPhaseAbility != null)
					{
						m_ActorsToIgnore.AddRange(cPhaseAbility.m_Ability.ActorsTargeted);
					}
				}
				m_ActorsToIgnore = m_ActorsToIgnore.Distinct().ToList();
			}
		}
		ValidTilesInAreaAffected.Clear();
		if (StartAbilityRequirements != null)
		{
			if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.ThisAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, this))
				{
					m_CancelAbility = true;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, ParentAbility))
				{
					m_CancelAbility = true;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
			{
				CPhaseAction.CPhaseAbility cPhaseAbility2 = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last();
				if (!StartAbilityRequirements.MeetsAbilityRequirements(targetingActor, cPhaseAbility2.m_Ability))
				{
					m_CancelAbility = true;
				}
			}
		}
		if (ConditionalOverrides != null)
		{
			ActiveConditionalOverrides = new List<CConditionalOverride>();
			foreach (CConditionalOverride conditionalOverride in ConditionalOverrides)
			{
				CAbility ability = this;
				if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
				{
					ability = ParentAbility;
				}
				else if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
				{
					ability = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last().m_Ability;
				}
				if (!conditionalOverride.Self || !conditionalOverride.Filter.IsValidTarget(TargetingActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible) || !conditionalOverride.Requirements.MeetsAbilityRequirements(TargetingActor, ability))
				{
					continue;
				}
				foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
				{
					OverrideAbilityValues(abilityOverride, perform: false, null, conditionalOverride.Filter);
				}
				ActiveConditionalOverrides.Add(conditionalOverride);
			}
		}
		m_OriginalTargetCount = m_NumberTargets;
		m_OriginalRange = m_Range;
		AbilityHasHappened = false;
		m_AreaEffectLocked = false;
		if (!AbilityStartListenersInvoked)
		{
			AbilityStartListenersInvoked = true;
			TargetingActor.m_OnAbilityStartedListeners?.Invoke(this);
		}
	}

	public void SetCancelAbilityFlag(bool cancel)
	{
		m_CancelAbility = cancel;
	}

	public void ProcessSongOverridesAndAbilities(CActor targetingActor, CSong.ESongActivationType songActivationType, bool temporaryOverrides = false)
	{
		if (m_SongAbilitiesProcessed && m_SongOverridesProcessed && songActivationType != CSong.ESongActivationType.ActionStart)
		{
			return;
		}
		m_AbilitySongs = new List<CSong.SongEffect>();
		m_OverrideSongs = new List<CSong.SongEffect>();
		foreach (CActiveBonus item in CActiveBonus.FindAllApplicableSongActiveBonuses(targetingActor))
		{
			if ((item.Ability.ActiveBonusData.IsToggleBonus && targetingActor.Type == CActor.EType.Player) || item.IsRestricted(targetingActor))
			{
				continue;
			}
			foreach (CSong.SongEffect songEffect in item.Ability.Song.SongEffects)
			{
				bool flag = false;
				bool flag2 = false;
				CAbility cAbility = null;
				if (songEffect.SongActivationType != songActivationType)
				{
					continue;
				}
				if (songEffect.SongEffectType == CSong.ESongEffectType.Ability)
				{
					if (this is CAbilityMerged cAbilityMerged)
					{
						foreach (CAbility mergedAbility in cAbilityMerged.MergedAbilities)
						{
							if (songEffect.AbilityType == mergedAbility.AbilityType)
							{
								flag = true;
								cAbility = mergedAbility;
								break;
							}
						}
					}
					else if ((ParentAbility == null || !(ParentAbility is CAbilityMerged)) && songEffect.AbilityType == AbilityType)
					{
						flag = true;
						cAbility = this;
					}
				}
				else if (songEffect.AbilityType == AbilityType)
				{
					flag = true;
					cAbility = this;
				}
				if (flag)
				{
					if (cAbility is CAbilityAttack cAbilityAttack)
					{
						if (cAbilityAttack.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA && (songEffect.AttackType == EAttackType.Attack || (songEffect.AttackType == EAttackType.Melee && cAbilityAttack.IsMeleeAttack) || (songEffect.AttackType == EAttackType.Ranged && !cAbilityAttack.IsMeleeAttack)))
						{
							flag2 = true;
						}
					}
					else
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					if (!temporaryOverrides)
					{
						item.RestrictActiveBonus(targetingActor);
					}
					if (songEffect.SongEffectType == CSong.ESongEffectType.Ability)
					{
						m_AbilitySongs.Add(songEffect);
					}
					else
					{
						m_OverrideSongs.Add(songEffect);
					}
				}
			}
		}
		if ((!m_SongOverridesProcessed || songActivationType == CSong.ESongActivationType.ActionStart) && m_OverrideSongs.Count > 0)
		{
			foreach (CSong.SongEffect overrideSong in m_OverrideSongs)
			{
				foreach (CAbilityOverride abilityOverride in overrideSong.AbilityOverrides)
				{
					OverrideAbilityValues(abilityOverride, perform: false);
				}
			}
		}
		if ((!m_SongAbilitiesProcessed || songActivationType == CSong.ESongActivationType.ActionStart) && m_AbilitySongs.Count > 0 && !targetingActor.IsUsingOnDeathAbility && !temporaryOverrides)
		{
			List<CAbility> list = new List<CAbility>();
			foreach (CSong.SongEffect abilitySong in m_AbilitySongs)
			{
				list.AddRange(abilitySong.Abilities);
			}
			if (list.Count > 0)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, null, stopPlayerUndo: false, null, ignorePerformNow: true);
				if (GameState.OverridingCurrentActor && GameState.InternalCurrentActor != TargetingActor)
				{
					GameState.OverrideCurrentActorForOneAction(TargetingActor);
				}
			}
		}
		if (songActivationType != CSong.ESongActivationType.ActionStart)
		{
			m_SongOverridesProcessed = true;
			m_SongAbilitiesProcessed = true;
		}
	}

	public void CheckForScenarioModifiersToTrigger()
	{
		if (MiscAbilityData?.TriggeredScenarioModifiers == null)
		{
			return;
		}
		int i;
		for (i = 0; i < MiscAbilityData?.TriggeredScenarioModifiers.Count; i++)
		{
			CScenarioModifier cScenarioModifier = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FirstOrDefault((CScenarioModifier m) => m.EventIdentifier == MiscAbilityData?.TriggeredScenarioModifiers[i]);
			if (cScenarioModifier != null)
			{
				List<CActor> list = new List<CActor>();
				if (MiscAbilityData.TriggerScenarioModifierOnSelfOnly.HasValue && MiscAbilityData.TriggerScenarioModifierOnSelfOnly.Value)
				{
					list.Add(TargetingActor);
				}
				else
				{
					list.AddRange(ScenarioManager.Scenario.AllActors);
				}
				for (int num = 0; num < list.Count; num++)
				{
					cScenarioModifier.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, list[num], ScenarioManager.CurrentScenarioState.Players.Count);
				}
			}
		}
	}

	public void ApplyEnhancement(CEnhancement enhancement)
	{
		switch (enhancement.Enhancement)
		{
		case EEnhancement.Area:
			if (AreaEffect != null)
			{
				AreaEffect.EnhancementHexes[enhancement.EnhancementSlot].m_Enabled = true;
			}
			break;
		case EEnhancement.PlusMove:
			if (this is CAbilityMove)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusAttack:
			if (this is CAbilityAttack)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusRange:
			m_Range++;
			break;
		case EEnhancement.PlusShield:
			if (this is CAbilityShield)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusPush:
			if (this is CAbilityPush)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusPull:
			if (this is CAbilityPull)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusPierce:
			if (this is CAbilityAttack cAbilityAttack)
			{
				cAbilityAttack.Pierce++;
			}
			break;
		case EEnhancement.PlusRetaliate:
			if (this is CAbilityRetaliate)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusRetaliateRange:
			if (this is CAbilityRetaliate cAbilityRetaliate)
			{
				cAbilityRetaliate.RetaliateRange++;
			}
			break;
		case EEnhancement.PlusHeal:
			if (this is CAbilityHeal)
			{
				m_Strength++;
			}
			break;
		case EEnhancement.PlusTarget:
			m_NumberTargets++;
			break;
		case EEnhancement.Poison:
		case EEnhancement.Wound:
		case EEnhancement.Muddle:
		case EEnhancement.Immobilize:
		case EEnhancement.Disarm:
			try
			{
				EAbilityType abilityType4 = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
				CCondition.ENegativeCondition key4 = CCondition.NegativeConditions.Single((CCondition.ENegativeCondition s) => s.ToString() == enhancement.Enhancement.ToString());
				if (!m_NegativeConditions.ContainsKey(key4))
				{
					m_NegativeConditions.Add(key4, CreateAbility(abilityType4, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
				break;
			}
			catch
			{
				DLLDebug.LogError("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				break;
			}
		case EEnhancement.Curse:
			try
			{
				CCondition.ENegativeCondition key3 = CCondition.ENegativeCondition.Curse;
				if (AbilityType == EAbilityType.Curse)
				{
					if (Strength == 0)
					{
						Strength = 1;
					}
					Strength++;
				}
				else if (m_NegativeConditions.ContainsKey(key3))
				{
					if (m_NegativeConditions[key3].Strength == 0)
					{
						m_NegativeConditions[key3].Strength++;
					}
					m_NegativeConditions[key3].Strength++;
				}
				else
				{
					EAbilityType abilityType3 = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
					m_NegativeConditions.Add(key3, CreateAbility(abilityType3, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
				break;
			}
			catch
			{
				DLLDebug.LogError("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				break;
			}
		case EEnhancement.Strengthen:
			try
			{
				EAbilityType abilityType2 = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
				CCondition.EPositiveCondition key2 = CCondition.PositiveConditions.Single((CCondition.EPositiveCondition s) => s.ToString() == enhancement.Enhancement.ToString());
				if (!m_PositiveConditions.ContainsKey(key2))
				{
					m_PositiveConditions.Add(key2, CreateAbility(abilityType2, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
				break;
			}
			catch
			{
				DLLDebug.LogError("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				break;
			}
		case EEnhancement.Bless:
			try
			{
				CCondition.EPositiveCondition key = CCondition.EPositiveCondition.Bless;
				if (AbilityType == EAbilityType.Bless)
				{
					if (Strength == 0)
					{
						Strength = 1;
					}
					Strength++;
				}
				else if (m_PositiveConditions.ContainsKey(key))
				{
					if (m_PositiveConditions[key].Strength == 0)
					{
						m_PositiveConditions[key].Strength++;
					}
					m_PositiveConditions[key].Strength++;
				}
				else
				{
					EAbilityType abilityType = AbilityTypes.Single((EAbilityType x) => x.ToString() == enhancement.Enhancement.ToString());
					m_PositiveConditions.Add(key, CreateAbility(abilityType, AbilityFilter, IsMonsterAbility, IsTargetedAbility));
				}
				break;
			}
			catch
			{
				DLLDebug.LogError("Condition " + enhancement.Enhancement.ToString() + " could not be found in EAbilityType enum.");
				break;
			}
		case EEnhancement.Jump:
			if (this is CAbilityMove cAbilityMove)
			{
				cAbilityMove.Jump = true;
			}
			break;
		case EEnhancement.Fire:
		case EEnhancement.Ice:
		case EEnhancement.Air:
		case EEnhancement.Earth:
		case EEnhancement.Light:
		case EEnhancement.Dark:
		case EEnhancement.AnyElement:
		{
			ElementInfusionBoardManager.EElement item;
			try
			{
				item = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement s) => s.ToString() == enhancement.Enhancement.ToString());
			}
			catch
			{
				item = ElementInfusionBoardManager.EElement.Any;
			}
			m_InfuseElements.Add(item);
			break;
		}
		case EEnhancement.SummonAttack:
			if (this is CAbilitySummon cAbilitySummon4)
			{
				cAbilitySummon4.SelectedSummonYMLData.AttackEnhancementBonus++;
			}
			break;
		case EEnhancement.SummonHP:
			if (this is CAbilitySummon cAbilitySummon3)
			{
				cAbilitySummon3.SelectedSummonYMLData.HPEnhancementBonus++;
			}
			break;
		case EEnhancement.SummonMove:
			if (this is CAbilitySummon cAbilitySummon2)
			{
				cAbilitySummon2.SelectedSummonYMLData.MoveEnhancementBonus++;
			}
			break;
		case EEnhancement.SummonRange:
			if (this is CAbilitySummon cAbilitySummon)
			{
				cAbilitySummon.SelectedSummonYMLData.RangeEnhancementBonus++;
			}
			break;
		}
	}

	public virtual int ModifiedStrength()
	{
		return m_ModifiedStrength;
	}

	public virtual bool HasCondition(CCondition.ENegativeCondition condition)
	{
		if (condition == CCondition.ENegativeCondition.NA)
		{
			return false;
		}
		if (condition.ToString() == AbilityType.ToString())
		{
			return true;
		}
		if (Augment != null && Augment.AugmentType == CAugment.EAugmentType.Bonus && Augment.Abilities.Count > 0)
		{
			CBaseCard cBaseCard = TargetingActor.FindCardWithAbility(this);
			if (cBaseCard != null)
			{
				foreach (CAbility ability in Augment.Abilities)
				{
					CActiveBonus cActiveBonus = cBaseCard.ActiveBonuses.Find((CActiveBonus x) => x.Ability.ID == ability.ID && x.Actor == TargetingActor);
					if (cActiveBonus != null && ability.ActiveBonusData.IsToggleBonus && cActiveBonus.ToggledBonus && ability.NegativeConditions.ContainsKey(condition))
					{
						return true;
					}
				}
			}
		}
		if (!NegativeConditions.ContainsKey(condition) && !SubAbilities.Exists((CAbility it) => it.HasCondition(condition)))
		{
			if (AbilityType == EAbilityType.AddCondition && ActiveBonusData.AbilityData != null)
			{
				return ActiveBonusData.AbilityData.NegativeConditions.Any((KeyValuePair<CCondition.ENegativeCondition, CAbility> it) => it.Key == condition);
			}
			return false;
		}
		return true;
	}

	public virtual bool HasCondition(CCondition.EPositiveCondition condition)
	{
		if (condition == CCondition.EPositiveCondition.NA)
		{
			return false;
		}
		if (condition.ToString() == AbilityType.ToString())
		{
			return true;
		}
		if (!PositiveConditions.ContainsKey(condition))
		{
			return SubAbilities.Exists((CAbility it) => it.HasCondition(condition));
		}
		return true;
	}

	public bool HaveActiveItemsCondition(CCondition.ENegativeCondition condition, CActor appliedToActor = null)
	{
		if (appliedToActor != null && this is CAbilityAttack)
		{
			_ = ((CAbilityAttack)this).AttackSummary;
			return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == appliedToActor && it.HasCondition(condition));
		}
		return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == null && it.HasCondition(condition));
	}

	public bool HaveActiveItemsCondition(CCondition.EPositiveCondition condition, CActor appliedToActor = null)
	{
		if (appliedToActor != null && this is CAbilityAttack)
		{
			_ = ((CAbilityAttack)this).AttackSummary;
			return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == appliedToActor && it.HasCondition(condition));
		}
		return ActiveSingleTargetItems.Exists((CItem it) => it.SingleTarget == null && it.HasCondition(condition));
	}

	public static CAbility CreateAbility(EAbilityType abilityType, CAbilityFilterContainer abilityFilter, bool isMonster, bool isTargetedAbility, int strength = 0, int range = 1, int numberTargets = 1, int conditionDuration = 1, EConditionDecTrigger conditionDecTrigger = EConditionDecTrigger.Turns, List<ElementInfusionBoardManager.EElement> elementsToInfuse = null, bool showElementPicker = false, CBaseCard parentBaseCard = null, bool isModifierAbility = false, List<CItem.EItemSlot> slotsToRefresh = null, List<CItem.EItemSlotState> slotStatesToRefresh = null, bool isSub = false, bool isInline = false)
	{
		EAbilityType abilityType2 = abilityType;
		string empty = string.Empty;
		List<CAbility> subAbilities = new List<CAbility>();
		AbilityData.ActiveBonusData activeBonusData = new AbilityData.ActiveBonusData();
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string name = abilityType.ToString();
		List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
		string empty4 = string.Empty;
		string empty5 = string.Empty;
		List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
		CAbilityRequirements startAbilityConditions = new CAbilityRequirements();
		string empty6 = string.Empty;
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>();
		string empty7 = string.Empty;
		bool showElementPicker2 = showElementPicker;
		bool isModifierAbility2 = isModifierAbility;
		return CreateAbility(abilityType2, strength, useSpecialBaseStat: false, range, numberTargets, abilityFilter, empty, subAbilities, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, activeBonusData, null, empty2, empty3, name, abilityEnhancements, null, null, multiPassAttack: true, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, empty4, textOnly: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, null, null, EAbilityTargeting.Range, elementsToInfuse, isMonster, empty5, null, isSub, isInline, 0, 1, isTargetedAbility, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, conditionalOverrides, startAbilityConditions, 0, empty6, null, skipAnim: false, conditionDuration, conditionDecTrigger, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, statIsBasedOnXEntries, 0, empty7, slotsToRefresh, slotStatesToRefresh, null, EAttackType.None, new List<CAbility>(), new List<EAbilityType>(), null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, new List<EAbilityType>(), new List<EAttackType>(), null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker2, isModifierAbility2, parentBaseCard);
	}

	private static List<CAbility> CopySubAbilities(List<CAbility> subAbilities)
	{
		List<CAbility> list = new List<CAbility>();
		foreach (CAbility subAbility in subAbilities)
		{
			CAbility cAbility = CopyAbility(subAbility, generateNewID: false);
			CAbility parentAbility = cAbility.ParentAbility;
			if (parentAbility != null)
			{
				if (cAbility.AbilityFilter == null && parentAbility.AbilityFilter != null)
				{
					cAbility.AbilityFilter = parentAbility.AbilityFilter.Copy();
				}
				if (cAbility.AreaEffect == null && parentAbility.AreaEffect != null)
				{
					cAbility.AreaEffect = parentAbility.AreaEffect.Copy();
				}
			}
			list.Add(cAbility);
		}
		return list;
	}

	public static CAbility CopyAbility(CAbility ability, bool generateNewID, bool fullCopy = false, bool copyCurrentOverrides = false)
	{
		if (ability == null)
		{
			return null;
		}
		List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
		List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>();
		foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in ability.PositiveConditions)
		{
			if (positiveCondition.Key == CCondition.EPositiveCondition.Bless && positiveCondition.Value.Strength > 0)
			{
				for (int i = 0; i < positiveCondition.Value.Strength; i++)
				{
					list.Add(positiveCondition.Key);
				}
			}
			else
			{
				list.Add(positiveCondition.Key);
			}
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in ability.NegativeConditions)
		{
			if (negativeCondition.Key == CCondition.ENegativeCondition.Curse && negativeCondition.Value.Strength > 0)
			{
				for (int j = 0; j < negativeCondition.Value.Strength; j++)
				{
					list2.Add(negativeCondition.Key);
				}
			}
			else
			{
				list2.Add(negativeCondition.Key);
			}
		}
		EAbilityType abilityType = ability.AbilityType;
		int strength = ability.m_Strength;
		bool useSpecialBaseStat = ability.UseSpecialBaseStat;
		int range = ability.m_Range;
		int numberTargets = ability.m_NumberTargets;
		CAbilityFilterContainer abilityFilter = ability.AbilityFilter.Copy();
		string animOverload = ability.AnimOverload;
		List<CAbility> subAbilities = CopySubAbilities(ability.SubAbilities);
		CAreaEffect areaEffect = ability.AreaEffect?.Copy();
		bool attackSourcesOnly = ability is CAbilityPreventDamage && (ability as CAbilityPreventDamage).AttackSourcesOnly;
		bool jump = ability is CAbilityMove && (ability as CAbilityMove).Jump;
		bool fly = ability is CAbilityMove && (ability as CAbilityMove).Fly;
		bool ignoreDifficultTerrain = ability is CAbilityMove && (ability as CAbilityMove).IgnoreDifficultTerrain;
		bool ignoreHazardousTerrain = ability is CAbilityMove cAbilityMove && cAbilityMove.IgnoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost = ability is CAbilityMove cAbilityMove2 && cAbilityMove2.IgnoreBlockedTileMoveCost;
		bool carryOtherActorsOnHex = ability is CAbilityMove cAbilityMove3 && cAbilityMove3.CarryOtherActorsOnHex;
		CAIFocusOverrideDetails aiFocusOverride = ((ability is CAbilityMove cAbilityMove4) ? cAbilityMove4.AIFocusOverride : null);
		CAbilityMove.EMoveRestrictionType moveRestrictionType = ((ability is CAbilityMove cAbilityMove5) ? cAbilityMove5.MoveRestrictionType : CAbilityMove.EMoveRestrictionType.None);
		AbilityData.ActiveBonusData activeBonusData = ability.ActiveBonusData?.Copy();
		ActiveBonusLayout activeBonusYML = ability.ActiveBonusYML?.Copy();
		string areaEffectYMLString = ability.AreaEffectYMLString;
		string areaEffectLayoutOverrideYMLString = ability.AreaEffectLayoutOverrideYMLString;
		string name = ability.Name;
		List<CEnhancement> abilityEnhancements = ability.m_AbilityEnhancements.Select((CEnhancement s) => s.Copy()).ToList();
		bool multiPassAttack = ability is CAbilityAttack && (ability as CAbilityAttack).MultiPassAttack;
		bool chainAttack = ability is CAbilityAttack && (ability as CAbilityAttack).ChainAttack;
		int chainAttackRange = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).ChainAttackRange : 0);
		int chainAttackDamageReduction = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).ChainAttackDamageReduction : 0);
		int damageSelfBeforeAttack = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).DamageSelfBeforeAttack : 0);
		bool addAttackBaseStat = ability.AddAttackBaseStat;
		bool strengthIsBase = ability.StrengthIsBase;
		bool rangeIsBase = ability.RangeIsBase;
		bool targetIsBase = ability.TargetIsBase;
		string abilityText = ability.AbilityText;
		bool abilityTextOnly = ability.AbilityTextOnly;
		bool showRange = ability.ShowRange;
		bool showTarget = ability.ShowTarget;
		bool showArea = ability.ShowArea;
		bool onDeath = ability.OnDeath;
		bool isConsumeAbility = ability.IsConsumeAbility;
		bool allTargetsOnMovePath = ability.AllTargetsOnMovePath;
		bool allTargetsOnMovePathSameStartAndEnd = ability.AllTargetsOnMovePathSameStartAndEnd;
		bool allTargetsOnAttackPath = ability.AllTargetsOnAttackPath;
		List<string> summons = ((ability is CAbilitySummon cAbilitySummon) ? cAbilitySummon.SummonIDs : ((ability is CAbilityRevive cAbilityRevive) ? cAbilityRevive.SummonIDs : null));
		AbilityData.XpPerTargetData xpPerTargetData = ability.m_XpPerTargetData?.Copy();
		EAbilityTargeting targeting = ability.Targeting;
		List<ElementInfusionBoardManager.EElement> elementsToInfuse = ((ability is CAbilityInfuse) ? (ability as CAbilityInfuse).ElementsToInfuse.ToList() : ((ability is CAbilityConsumeElement) ? (ability as CAbilityConsumeElement).ElementsToConsume.ToList() : null));
		bool isMonsterAbility = ability.IsMonsterAbility;
		string propName = ((ability is CAbilityCreate) ? (ability as CAbilityCreate).PropName : string.Empty);
		AbilityData.TrapData trapData = ((ability is CAbilityTrap cAbilityTrap) ? cAbilityTrap.TrapData : null);
		bool isSubAbility = ability.IsSubAbility;
		bool isInlineSubAbility = ability.IsInlineSubAbility;
		bool isModifierAbility = ability.IsModifierAbility;
		int pierce = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).Pierce : 0);
		int retaliateRange = ((!(ability is CAbilityRetaliate)) ? 1 : (ability as CAbilityRetaliate).RetaliateRange);
		bool isTargetedAbility = ability.IsTargetedAbility;
		float spawnDelay = ability.SpawnDelay;
		CAbilityPull.EPullType pullType = ((ability is CAbilityPull cAbilityPull) ? cAbilityPull.PullType : CAbilityPull.EPullType.None);
		CAbilityPush.EAdditionalPushEffect additionalPushEffect = ((ability is CAbilityPush) ? (ability as CAbilityPush).AdditionalPushEffect : CAbilityPush.EAdditionalPushEffect.None);
		int additionalPushEffectDamage = ((ability is CAbilityPush) ? (ability as CAbilityPush).AdditionalPushEffectDamage : 0);
		int additionalPushEffectXP = ((ability is CAbilityPush) ? (ability as CAbilityPush).AdditionalPushEffectXP : 0);
		CAbilityLoot.LootData lootData = ((ability is CAbilityLoot) ? (ability as CAbilityLoot).AbilityLootData : null);
		int abilityXP = ability.AbilityXP;
		List<CConditionalOverride> conditionalOverrides = ability.ConditionalOverrides.Select((CConditionalOverride s) => s.Copy()).ToList();
		CAbilityRequirements startAbilityConditions = ability.StartAbilityRequirements.Copy();
		string parentName = ability.ParentName;
		AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData?.Copy();
		bool skipAnim = ability.SkipAnim;
		int conditionDuration = ((ability is CAbilityCondition) ? (ability as CAbilityCondition).Duration : 0);
		EConditionDecTrigger conditionDecTrigger = ((ability is CAbilityCondition) ? (ability as CAbilityCondition).DecrementTrigger : EConditionDecTrigger.None);
		bool isDefault = ((ability is CAbilityAttack) ? (ability as CAbilityAttack).IsDefaultAttack : (ability is CAbilityMove && (ability as CAbilityMove).IsDefaultMove));
		bool isItemAbility = ability.IsItemAbility;
		CAugment augment = ability.Augment;
		CSong song = ability.Song;
		CDoom doom = ((ability is CAbilityAddDoom cAbilityAddDoom) ? cAbilityAddDoom.Doom : null);
		List<CAttackEffect> attackEffects = ((!(ability is CAbilityAttack)) ? null : (ability as CAbilityAttack).AttackEffects?.ToList());
		CAbilityControlActor.ControlActorAbilityData controlActorData = ((ability is CAbilityControlActor cAbilityControlActor) ? cAbilityControlActor.ControlActorData : null);
		CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = ((ability is CAbilityChangeAllegiance cAbilityChangeAllegiance) ? cAbilityChangeAllegiance.ChangeAllegianceData.Copy() : null);
		bool isMergedAbility = ability.IsMergedAbility;
		List<CAbility> list3 = ((!(ability is CAbilityMerged)) ? null : (ability as CAbilityMerged).MergedAbilities?.ToList());
		CAbility cAbility = CreateAbility(targetActorWithTrapEffects: ability is CAbilityDisarmTrap && (ability as CAbilityDisarmTrap).TargetActorWithTrapEffects, targetActorWithTrapEffectRange: (ability is CAbilityDisarmTrap) ? (ability as CAbilityDisarmTrap).TargetActorWithTrapEffectRange : 0, isMergedAbility: isMergedAbility, mergedAbilities: list3, statIsBasedOnXEntries: ability.StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData s) => s.Copy()).ToList(), moveObstacleRange: (ability is CAbilityMoveObstacle cAbilityMoveObstacle) ? cAbilityMoveObstacle.MoveObstacleRange : ((ability is CAbilityMoveTrap cAbilityMoveTrap) ? cAbilityMoveTrap.MoveTrapRange : 0), moveObstacleAnimOverload: (ability is CAbilityMoveObstacle cAbilityMoveObstacle2) ? cAbilityMoveObstacle2.MoveObstacleAnimOverload : ((ability is CAbilityMoveTrap cAbilityMoveTrap2) ? cAbilityMoveTrap2.MoveTrapAnimOverload : null), slotsToRefresh: (ability is CAbilityRefreshItemCards cAbilityRefreshItemCards) ? cAbilityRefreshItemCards.SlotsToRefresh : ((ability is CAbilityConsumeItemCards cAbilityConsumeItemCards) ? cAbilityConsumeItemCards.SlotsToConsume : null), slotStatesToRefresh: (ability is CAbilityRefreshItemCards cAbilityRefreshItemCards2) ? cAbilityRefreshItemCards2.SlotStatesToRefresh : ((ability is CAbilityConsumeItemCards cAbilityConsumeItemCards2) ? cAbilityConsumeItemCards2.SlotStatesToConsume : null), addActiveBonusAbility: (ability is CAbilityAddActiveBonus cAbilityAddActiveBonus) ? cAbilityAddActiveBonus.AddAbility : null, overrideAugmentAttackType: (ability is CAbilityOverrideAugmentAttackType cAbilityOverrideAugmentAttackType) ? cAbilityOverrideAugmentAttackType.OverrideAttackType : EAttackType.None, showElementPicker: (ability is CAbilityInfuse cAbilityInfuse) ? cAbilityInfuse.ShowPicker : (ability is CAbilityConsumeElement cAbilityConsumeElement && cAbilityConsumeElement.ShowPicker), abilityType: abilityType, strength: strength, useSpecialBaseStat: useSpecialBaseStat, range: range, numberTargets: numberTargets, abilityFilter: abilityFilter, animationOverload: animOverload, subAbilities: subAbilities, areaEffect: areaEffect, attackSourcesOnly: attackSourcesOnly, jump: jump, fly: fly, ignoreDifficultTerrain: ignoreDifficultTerrain, ignoreHazardousTerrain: ignoreHazardousTerrain, ignoreBlockedTileMoveCost: ignoreBlockedTileMoveCost, carryOtherActorsOnHex: carryOtherActorsOnHex, aiFocusOverride: aiFocusOverride, moveRestrictionType: moveRestrictionType, activeBonusData: activeBonusData, activeBonusYML: activeBonusYML, areaEffectYMLString: areaEffectYMLString, areaEffectLayoutOverrideYMLString: areaEffectLayoutOverrideYMLString, name: name, abilityEnhancements: abilityEnhancements, positiveConditions: list, negativeConditions: list2, multiPassAttack: multiPassAttack, chainAttack: chainAttack, chainAttackRange: chainAttackRange, chainAttackDamageReduction: chainAttackDamageReduction, damageSelfBeforeAttack: damageSelfBeforeAttack, addAttackBaseStat: addAttackBaseStat, strengthIsBase: strengthIsBase, rangeIsBase: rangeIsBase, targetIsBase: targetIsBase, text: abilityText, textOnly: abilityTextOnly, showRange: showRange, showTarget: showTarget, showArea: showArea, onDeath: onDeath, isConsumeAbility: isConsumeAbility, allTargetsOnMovePath: allTargetsOnMovePath, allTargetsOnMovePathSameStartAndEnd: allTargetsOnMovePathSameStartAndEnd, allTargetsOnAttackPath: allTargetsOnAttackPath, summons: summons, xpPerTargetData: xpPerTargetData, targeting: targeting, elementsToInfuse: elementsToInfuse, isMonster: isMonsterAbility, propName: propName, trapData: trapData, isSubAbility: isSubAbility, isInlineSubAbility: isInlineSubAbility, pierce: pierce, retaliateRange: retaliateRange, isTargetedAbility: isTargetedAbility, spawnDelay: spawnDelay, pullType: pullType, additionalPushEffect: additionalPushEffect, additionalPushEffectDamage: additionalPushEffectDamage, additionalPushEffectXP: additionalPushEffectXP, lootData: lootData, conditionalOverrides: conditionalOverrides, startAbilityConditions: startAbilityConditions, abilityXP: abilityXP, parentName: parentName, miscAbilityData: miscAbilityData, skipAnim: skipAnim, conditionDuration: conditionDuration, conditionDecTrigger: conditionDecTrigger, augment: augment, song: song, doom: doom, controlActorData: controlActorData, changeAllegianceData: changeAllegianceData, attackEffects: attackEffects, chooseAbilities: (ability is CAbilityChoose cAbilityChoose) ? CopySubAbilities(cAbilityChoose.ChooseAbilities) : ((ability is CAbilityChooseAbility cAbilityChooseAbility) ? CopySubAbilities(cAbilityChooseAbility.ChooseAbilities) : null), recoverCardsAbilityTypeFilter: (ability is CAbilityRecoverDiscardedCards cAbilityRecoverDiscardedCards) ? cAbilityRecoverDiscardedCards.RecoverCardsWithAbilityOfTypeFilter : ((ability is CAbilityRecoverLostCards cAbilityRecoverLostCards) ? cAbilityRecoverLostCards.RecoverCardsWithAbilityOfTypeFilter : new List<EAbilityType>()), forgoTopActionAbility: (ability is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion) ? cAbilityForgoActionsForCompanion.ForgoTopActionAbility : null, forgoBottomActionAbility: (ability is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion2) ? cAbilityForgoActionsForCompanion2.ForgoBottomActionAbility : null, extraTurnType: (ability is CAbilityExtraTurn cAbilityExtraTurn) ? cAbilityExtraTurn.ExtraTurnType : CAbilityExtraTurn.EExtraTurnType.None, swapAbilityFirstTargetFilter: (ability is CAbilitySwap cAbilitySwap) ? cAbilitySwap.FirstTargetFilter : null, swapAbilitySecondTargetFilter: (ability is CAbilitySwap cAbilitySwap2) ? cAbilitySwap2.SecondTargetFilter : null, supplyCardsToGive: (ability is CAbilityGiveSupplyCard cAbilityGiveSupplyCard) ? cAbilityGiveSupplyCard.SupplyCardIDs : null, teleportData: (ability is CAbilityTeleport cAbilityTeleport) ? cAbilityTeleport.AbilityTeleportData.Copy() : null, immunityToAbilityTypes: (!(ability is CAbilityImmunityTo cAbilityImmunityTo)) ? null : cAbilityImmunityTo.ImmuneToAbilityTypes?.ToList(), immuneToAttackTypes: (!(ability is CAbilityImmunityTo cAbilityImmunityTo2)) ? null : cAbilityImmunityTo2.ImmuneToAttackTypes?.ToList(), addModifiers: (ability is CAbilityAddModifierToMonsterDeck cAbilityAddModifierToMonsterDeck) ? cAbilityAddModifierToMonsterDeck.ModifierCardNamesToAdd.ToList() : null, healData: (ability is CAbilityHeal cAbilityHeal) ? cAbilityHeal.HealData.Copy() : null, resourcesToAddOnAbilityEnd: ability.ResourcesToAddOnAbilityEnd, resourcesToTakeFromTargets: ability.ResourcesToTakeFromTargets, resourcesToGiveToTargets: ability.ResourcesToGiveToTargets, destroyObstacleData: (ability is CAbilityDestroyObstacle cAbilityDestroyObstacle) ? cAbilityDestroyObstacle.AbilityDestroyObstacleData.Copy() : null, changeCharacterModel: (ability is CAbilityChangeCharacterModel cAbilityChangeCharacterModel) ? cAbilityChangeCharacterModel.CharacterModel : ECharacter.None, tileFilter: ability.TileFilter, disableCardActionData: (ability is CAbilityDisableCardAction cAbilityDisableCardAction) ? cAbilityDisableCardAction.DisableCardActionAbilityData.Copy() : null, isDefault: isDefault, isItemAbility: isItemAbility, isModifierAbility: isModifierAbility, parentBaseCard: (ability is CAbilityInfuse cAbilityInfuse2) ? cAbilityInfuse2.ParentBaseCard : null, previewEffectId: ability.PreviewEffectId, previewEffectText: ability.PreviewEffectText, canUndoOverride: ability.CanUndoOverride, helpBoxTooltipLocKey: ability.HelpBoxTooltipLocKey);
		if (!generateNewID)
		{
			cAbility.ID = ability.ID;
		}
		cAbility.IsControlAbility = ability.IsControlAbility;
		if (fullCopy)
		{
			cAbility.m_AbilityStartComplete = ability.m_AbilityStartComplete;
			cAbility.ValidActorsInRange = ability.ValidActorsInRange?.ToList();
			cAbility.TargetingActor = ability.TargetingActor;
			cAbility.FilterActor = ability.FilterActor;
			cAbility.m_TilesSelected = ability.TilesSelected?.ToList();
			cAbility.m_ActorsToTarget = ability.ActorsToTarget?.ToList();
			cAbility.m_NumberTargetsRemaining = ability.NumberTargetsRemaining;
			cAbility.m_UndoNumberTargetsRemaining = ability.m_UndoNumberTargetsRemaining;
			cAbility.TilesInRange = ability.TilesInRange?.ToList();
			cAbility.m_CanUndo = ability.CanUndo;
			cAbility.m_CanSkip = ability.CanSkip;
			cAbility.AllPossibleTilesInAreaEffect = ability.AllPossibleTilesInAreaEffect?.ToList();
			cAbility.ActorsToIgnore = ability.ActorsToIgnore?.ToList();
			cAbility.m_ModifiedStrength = ability.ModifiedStrength();
			cAbility.m_AllTargets = ability.m_AllTargets;
			cAbility.m_CancelAbility = ability.m_CancelAbility;
			cAbility.ParentAbility = ability.ParentAbility;
			cAbility.InlineSubAbilityTiles = ability.InlineSubAbilityTiles.ToList();
			cAbility.TargetThisActorAutomatically = ability.TargetThisActorAutomatically;
			cAbility.ProcessIfDead = ability.ProcessIfDead;
			cAbility.StackedAttackEffectAbility = ability.StackedAttackEffectAbility;
			cAbility.AppliedEnhancements = ability.AppliedEnhancements;
			cAbility.AbilityStartListenersInvoked = ability.AbilityStartListenersInvoked;
			cAbility.ParentAbilityBaseCard = ability.ParentAbilityBaseCard;
			cAbility.m_AugmentAbilitiesProcessed = ability.m_AugmentAbilitiesProcessed;
			cAbility.m_AugmentOverridesProcessed = ability.m_AugmentOverridesProcessed;
			cAbility.m_SongAbilitiesProcessed = ability.m_SongAbilitiesProcessed;
			cAbility.m_SongOverridesProcessed = ability.m_SongOverridesProcessed;
		}
		if (copyCurrentOverrides)
		{
			cAbility.m_CurrentOverrides = ability.CurrentOverrides;
		}
		return cAbility;
	}

	public virtual void Undo()
	{
		if (Augment != null)
		{
			TargetingActor.UndoAugment(Augment);
		}
		if (Song != null)
		{
			TargetingActor.UndoSong(Song);
		}
		GameState.WaitingForMercenarySpecialMechanicSlotChoice = false;
	}

	public virtual void ClearTargets()
	{
		if (m_ActorsToTarget != null)
		{
			m_ActorsToTarget.Clear();
		}
		if (m_TilesSelected != null)
		{
			m_TilesSelected.Clear();
		}
		m_NumberTargetsRemaining = m_UndoNumberTargetsRemaining;
		m_AreaEffectLocked = false;
		if (AbilityType != EAbilityType.Attack)
		{
			TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
		}
		CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(TargetingActor);
		cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = false;
		cShowSingleTargetActiveBonus_MessageData.m_Ability = this;
		ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
	}

	public virtual bool CanClearTargets()
	{
		return false;
	}

	public virtual bool CanReceiveTileSelection()
	{
		if (m_AbilityStartComplete)
		{
			return !m_CancelAbility;
		}
		return false;
	}

	public virtual bool RequiresWaypointSelection()
	{
		return false;
	}

	public virtual bool CanApplyActiveBonusTogglesTo()
	{
		return true;
	}

	public virtual bool EnoughTargetsSelected()
	{
		return true;
	}

	public virtual bool IsCurrentlyTargetingActors()
	{
		return true;
	}

	public virtual bool MaxTargetsSelected()
	{
		if (AreaEffect != null)
		{
			return m_AreaEffectLocked;
		}
		return m_NumberTargetsRemaining <= 0;
	}

	public virtual bool ShouldRestartAbilityWhenApplyingOverride(CAbilityOverride abilityOverride)
	{
		if (!abilityOverride.Range.HasValue && !abilityOverride.NumberOfTargets.HasValue && abilityOverride.AbilityFilter == null && abilityOverride.AreaEffect == null && !abilityOverride.MultiPassAttack.HasValue && !abilityOverride.RangeIsBase.HasValue && !abilityOverride.TargetIsBase.HasValue && !abilityOverride.AllTargetsOnMovePath.HasValue && !abilityOverride.AllTargetsOnMovePathSameStartAndEnd.HasValue)
		{
			return abilityOverride.AllTargetsOnAttackPath.HasValue;
		}
		return true;
	}

	public static CAbility CreateAbility(EAbilityType abilityType, int strength, bool useSpecialBaseStat, int range, int numberTargets, CAbilityFilterContainer abilityFilter, string animationOverload, List<CAbility> subAbilities, CAreaEffect areaEffect, bool attackSourcesOnly, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, bool ignoreBlockedTileMoveCost, bool carryOtherActorsOnHex, CAIFocusOverrideDetails aiFocusOverride, CAbilityMove.EMoveRestrictionType moveRestrictionType, AbilityData.ActiveBonusData activeBonusData, ActiveBonusLayout activeBonusYML, string areaEffectYMLString, string areaEffectLayoutOverrideYMLString, string name, List<CEnhancement> abilityEnhancements, List<CCondition.EPositiveCondition> positiveConditions, List<CCondition.ENegativeCondition> negativeConditions, bool multiPassAttack, bool chainAttack, int chainAttackRange, int chainAttackDamageReduction, int damageSelfBeforeAttack, bool addAttackBaseStat, bool strengthIsBase, bool rangeIsBase, bool targetIsBase, string text, bool textOnly, bool showRange, bool showTarget, bool showArea, bool onDeath, bool isConsumeAbility, bool allTargetsOnMovePath, bool allTargetsOnMovePathSameStartAndEnd, bool allTargetsOnAttackPath, List<string> summons, AbilityData.XpPerTargetData xpPerTargetData, EAbilityTargeting targeting, List<ElementInfusionBoardManager.EElement> elementsToInfuse, bool isMonster, string propName, AbilityData.TrapData trapData, bool isSubAbility, bool isInlineSubAbility, int pierce, int retaliateRange, bool isTargetedAbility, float spawnDelay, CAbilityPull.EPullType pullType, CAbilityPush.EAdditionalPushEffect additionalPushEffect, int additionalPushEffectDamage, int additionalPushEffectXP, CAbilityLoot.LootData lootData, List<CConditionalOverride> conditionalOverrides, CAbilityRequirements startAbilityConditions, int abilityXP, string parentName, AbilityData.MiscAbilityData miscAbilityData, bool skipAnim, int conditionDuration, EConditionDecTrigger conditionDecTrigger, CAugment augment, CSong song, CDoom doom, CAbilityControlActor.ControlActorAbilityData controlActorData, CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData, List<CAttackEffect> attackEffects, bool targetActorWithTrapEffects, int targetActorWithTrapEffectRange, bool isMergedAbility, List<CAbility> mergedAbilities, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries, int moveObstacleRange, string moveObstacleAnimOverload, List<CItem.EItemSlot> slotsToRefresh, List<CItem.EItemSlotState> slotStatesToRefresh, CAbility addActiveBonusAbility, EAttackType overrideAugmentAttackType, List<CAbility> chooseAbilities, List<EAbilityType> recoverCardsAbilityTypeFilter, CAbility forgoTopActionAbility, CAbility forgoBottomActionAbility, CAbilityExtraTurn.EExtraTurnType extraTurnType, CAbilityFilterContainer swapAbilityFirstTargetFilter, CAbilityFilterContainer swapAbilitySecondTargetFilter, List<int> supplyCardsToGive, CAbilityTeleport.TeleportData teleportData, List<EAbilityType> immunityToAbilityTypes, List<EAttackType> immuneToAttackTypes, List<string> addModifiers, CAbilityHeal.HealAbilityData healData, Dictionary<string, int> resourcesToAddOnAbilityEnd, Dictionary<string, int> resourcesToTakeFromTargets, Dictionary<string, int> resourcesToGiveToTargets, CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData, ECharacter changeCharacterModel, CAbilityFilter.EFilterTile tileFilter, CAbilityDisableCardAction.DisableCardActionData disableCardActionData, bool isDefault = false, bool isItemAbility = false, bool showElementPicker = false, bool isModifierAbility = false, CBaseCard parentBaseCard = null, string previewEffectId = null, string previewEffectText = null, bool? canUndoOverride = null, string helpBoxTooltipLocKey = null)
	{
		CAbility cAbility = null;
		switch (abilityType)
		{
		case EAbilityType.Summon:
			cAbility = new CAbilitySummon(summons);
			break;
		case EAbilityType.Move:
			cAbility = new CAbilityMove(jump, fly, isDefault, moveRestrictionType, ignoreDifficultTerrain, ignoreHazardousTerrain, ignoreBlockedTileMoveCost, carryOtherActorsOnHex: false, aiFocusOverride);
			break;
		case EAbilityType.Attack:
			cAbility = new CAbilityAttack(multiPassAttack || (miscAbilityData != null && miscAbilityData.TargetOneEnemyWithAllAttacks.HasValue && (miscAbilityData?.TargetOneEnemyWithAllAttacks.Value ?? false)), chainAttack, pierce, isDefault, attackEffects, chainAttackRange, chainAttackDamageReduction, damageSelfBeforeAttack);
			break;
		case EAbilityType.Heal:
			cAbility = new CAbilityHeal(healData);
			break;
		case EAbilityType.Damage:
			cAbility = new CAbilityDamage();
			break;
		case EAbilityType.GainXP:
			cAbility = new CAbilityXP();
			break;
		case EAbilityType.Loot:
			cAbility = new CAbilityLoot(lootData);
			break;
		case EAbilityType.PreventDamage:
			cAbility = new CAbilityPreventDamage(attackSourcesOnly);
			break;
		case EAbilityType.Shield:
			cAbility = new CAbilityShield();
			break;
		case EAbilityType.AddTarget:
			cAbility = new CAbilityAddTarget();
			break;
		case EAbilityType.AddHeal:
			cAbility = new CAbilityAddHeal();
			break;
		case EAbilityType.AddRange:
			cAbility = new CAbilityAddRange();
			break;
		case EAbilityType.AddCondition:
			cAbility = new CAbilityAddCondition();
			break;
		case EAbilityType.AttackersGainDisadvantage:
			cAbility = new CAbilityAttackersGainDisadvantage();
			break;
		case EAbilityType.Redirect:
			cAbility = new CAbilityRedirect();
			break;
		case EAbilityType.Retaliate:
			cAbility = new CAbilityRetaliate(retaliateRange);
			break;
		case EAbilityType.Trap:
			cAbility = new CAbilityTrap(trapData);
			break;
		case EAbilityType.RecoverLostCards:
			cAbility = new CAbilityRecoverLostCards(recoverCardsAbilityTypeFilter);
			break;
		case EAbilityType.Create:
			cAbility = new CAbilityCreate(propName);
			break;
		case EAbilityType.Invisible:
			cAbility = new CAbilityInvisible(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Poison:
			cAbility = new CAbilityPoison();
			break;
		case EAbilityType.Bless:
			cAbility = new CAbilityBless();
			break;
		case EAbilityType.Curse:
			cAbility = new CAbilityCurse();
			break;
		case EAbilityType.Disarm:
			cAbility = new CAbilityDisarm(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Immobilize:
			cAbility = new CAbilityImmobilize(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Muddle:
			cAbility = new CAbilityMuddle(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Strengthen:
			cAbility = new CAbilityStrengthen(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Advantage:
			cAbility = new CAbilityAdvantage(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Stun:
			cAbility = new CAbilityStun(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Overheal:
			cAbility = new CAbilityOverheal(strength, conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.Wound:
			cAbility = new CAbilityWound();
			break;
		case EAbilityType.Infuse:
			cAbility = new CAbilityInfuse(elementsToInfuse, showElementPicker, parentBaseCard);
			break;
		case EAbilityType.ConsumeElement:
			cAbility = new CAbilityConsumeElement(elementsToInfuse, showElementPicker, parentBaseCard);
			break;
		case EAbilityType.Consume:
			cAbility = new CAbilityConsume();
			break;
		case EAbilityType.RefreshItemCards:
			cAbility = new CAbilityRefreshItemCards(slotsToRefresh, slotStatesToRefresh);
			break;
		case EAbilityType.Teleport:
			cAbility = new CAbilityTeleport(teleportData);
			break;
		case EAbilityType.Kill:
			cAbility = new CAbilityKill();
			break;
		case EAbilityType.DisarmTrap:
			cAbility = new CAbilityDisarmTrap(targetActorWithTrapEffects, targetActorWithTrapEffectRange);
			break;
		case EAbilityType.DestroyObstacle:
			cAbility = new CAbilityDestroyObstacle(destroyObstacleData);
			break;
		case EAbilityType.RecoverDiscardedCards:
			cAbility = new CAbilityRecoverDiscardedCards(recoverCardsAbilityTypeFilter);
			break;
		case EAbilityType.Pull:
			cAbility = new CAbilityPull(pullType);
			break;
		case EAbilityType.Push:
			cAbility = new CAbilityPush(additionalPushEffect, additionalPushEffectDamage, additionalPushEffectXP);
			break;
		case EAbilityType.ControlActor:
			cAbility = new CAbilityControlActor(controlActorData);
			break;
		case EAbilityType.AddAugment:
			cAbility = new CAbilityAddAugment();
			break;
		case EAbilityType.AddSong:
			cAbility = new CAbilityAddSong();
			break;
		case EAbilityType.PlaySong:
			cAbility = new CAbilityPlaySong();
			break;
		case EAbilityType.OverrideAugmentAttackType:
			cAbility = new CAbilityOverrideAugmentAttackType(overrideAugmentAttackType);
			break;
		case EAbilityType.MergedCreateAttack:
			cAbility = new CAbilityMergedCreateAttack((CAbilityCreate)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MergedDestroyObstacleAttack:
			cAbility = new CAbilityMergedDestroyAttack((CAbilityDestroyObstacle)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MergedMoveAttack:
			cAbility = new CAbilityMergedMoveAttack((CAbilityMove)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MoveObstacle:
			cAbility = new CAbilityMoveObstacle(moveObstacleRange, moveObstacleAnimOverload);
			break;
		case EAbilityType.MergedMoveObstacleAttack:
			cAbility = new CAbilityMergedMoveObstacleAttack((CAbilityMoveObstacle)mergedAbilities[0], (CAbilityAttack)mergedAbilities[1]);
			break;
		case EAbilityType.MergedDisarmTrapDestroyObstacles:
			cAbility = new CAbilityMergedDisarmTrapDestroyObstacle((CAbilityDisarmTrap)mergedAbilities[0], (CAbilityDestroyObstacle)mergedAbilities[1]);
			break;
		case EAbilityType.MergedKillCreate:
			cAbility = new CAbilityMergedKillCreate((CAbilityKill)mergedAbilities[0], (CAbilityCreate)mergedAbilities[1]);
			break;
		case EAbilityType.AddActiveBonus:
			cAbility = new CAbilityAddActiveBonus(addActiveBonusAbility);
			break;
		case EAbilityType.Null:
			cAbility = new CAbilityNull();
			break;
		case EAbilityType.ImmunityTo:
			cAbility = new CAbilityImmunityTo(immunityToAbilityTypes, immuneToAttackTypes);
			break;
		case EAbilityType.ChooseAbility:
			cAbility = new CAbilityChoose(chooseAbilities);
			break;
		case EAbilityType.Choose:
			cAbility = new CAbilityChooseAbility(chooseAbilities);
			break;
		case EAbilityType.LoseCards:
			cAbility = new CAbilityLoseCards();
			break;
		case EAbilityType.IncreaseCardLimit:
			cAbility = new CAbilityIncreaseCardLimit();
			break;
		case EAbilityType.StopFlying:
			cAbility = new CAbilityStopFlying();
			break;
		case EAbilityType.Immovable:
			cAbility = new CAbilityImmovable();
			break;
		case EAbilityType.ExtraTurn:
			cAbility = new CAbilityExtraTurn(extraTurnType);
			break;
		case EAbilityType.ImprovedShortRest:
			cAbility = new CAbilityImprovedShortRest();
			break;
		case EAbilityType.AdjustInitiative:
			cAbility = new CAbilityAdjustInitiative();
			break;
		case EAbilityType.Swap:
			cAbility = new CAbilitySwap(swapAbilityFirstTargetFilter, swapAbilitySecondTargetFilter);
			break;
		case EAbilityType.RedistributeDamage:
			cAbility = new CAbilityRedistributeDamage();
			break;
		case EAbilityType.ForgoActionsForCompanion:
			cAbility = new CAbilityForgoActionsForCompanion(forgoTopActionAbility, forgoBottomActionAbility);
			break;
		case EAbilityType.ChangeModifier:
			cAbility = new CAbilityChangeModifier();
			break;
		case EAbilityType.ChangeCondition:
			cAbility = new CAbilityChangeModifier();
			break;
		case EAbilityType.RemoveConditions:
			cAbility = new CAbilityRemoveConditions();
			break;
		case EAbilityType.ShuffleModifierDeck:
			cAbility = new CAbilityShuffleModifierDeck();
			break;
		case EAbilityType.DiscardCards:
			cAbility = new CAbilityDiscardCards();
			break;
		case EAbilityType.NullTargeting:
			cAbility = new CAbilityNullTargeting();
			break;
		case EAbilityType.AddDoom:
			cAbility = new CAbilityAddDoom(doom);
			break;
		case EAbilityType.AddDoomSlots:
			cAbility = new CAbilityAddDoomSlots();
			break;
		case EAbilityType.TransferDooms:
			cAbility = new CAbilityTransferDooms();
			break;
		case EAbilityType.GiveSupplyCard:
			cAbility = new CAbilityGiveSupplyCard(supplyCardsToGive);
			break;
		case EAbilityType.Fear:
			cAbility = new CAbilityFear(jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain);
			break;
		case EAbilityType.DeactivateSpawner:
			cAbility = new CAbilityDeactivateSpawner();
			break;
		case EAbilityType.RemoveActorFromMap:
			cAbility = new CAbilityRemoveActorFromMap();
			break;
		case EAbilityType.ConsumeItemCards:
			cAbility = new CAbilityConsumeItemCards(slotsToRefresh, slotStatesToRefresh);
			break;
		case EAbilityType.Invulnerability:
			cAbility = new CAbilityInvulnerability();
			break;
		case EAbilityType.PierceInvulnerability:
			cAbility = new CAbilityPierceInvulnerability();
			break;
		case EAbilityType.Sleep:
			cAbility = new CAbilitySleep();
			break;
		case EAbilityType.ActivateSpawner:
			cAbility = new CAbilityActivateSpawner();
			break;
		case EAbilityType.ItemLock:
			cAbility = new CAbilityItemLock();
			break;
		case EAbilityType.Untargetable:
			cAbility = new CAbilityUntargetable();
			break;
		case EAbilityType.LoseGoalChestReward:
			cAbility = new CAbilityLoseGoalChestReward();
			break;
		case EAbilityType.AddModifierToMonsterDeck:
			cAbility = new CAbilityAddModifierToMonsterDeck(addModifiers);
			break;
		case EAbilityType.RecoverResources:
			cAbility = new CAbilityRecoverResources();
			break;
		case EAbilityType.MoveTrap:
			cAbility = new CAbilityMoveTrap(moveObstacleRange, moveObstacleAnimOverload);
			break;
		case EAbilityType.BlockHealing:
			cAbility = new CAbilityBlockHealing(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.NeutralizeShield:
			cAbility = new CAbilityNeutralizeShield(conditionDuration, conditionDecTrigger);
			break;
		case EAbilityType.ChangeCharacterModel:
			cAbility = new CAbilityChangeCharacterModel(changeCharacterModel);
			break;
		case EAbilityType.NullHex:
			cAbility = new CAbilityNullHex();
			break;
		case EAbilityType.HealthReduction:
			cAbility = new CAbilityHealthReduction();
			break;
		case EAbilityType.ChangeAllegiance:
			cAbility = new CAbilityChangeAllegiance(changeAllegianceData);
			break;
		case EAbilityType.Revive:
			cAbility = new CAbilityRevive(summons);
			break;
		case EAbilityType.DisableCardAction:
			cAbility = new CAbilityDisableCardAction(disableCardActionData);
			break;
		default:
			DLLDebug.LogError("Error creating ability " + name + ".  Invalid Ability Type " + abilityType.ToString() + ".");
			return null;
		}
		cAbility.ID = Guid.NewGuid();
		cAbility.Name = name;
		cAbility.AbilityType = abilityType;
		cAbility.m_Strength = strength;
		cAbility.UseSpecialBaseStat = useSpecialBaseStat;
		cAbility.m_Range = range;
		cAbility.m_NumberTargets = numberTargets;
		cAbility.AbilityFilter = abilityFilter;
		cAbility.TileFilter = tileFilter;
		cAbility.AnimOverload = animationOverload;
		cAbility.IsMonsterAbility = isMonster;
		cAbility.ParentName = parentName;
		cAbility.m_IsSubAbility = isSubAbility;
		cAbility.IsInlineSubAbility = isInlineSubAbility;
		cAbility.IsTargetedAbility = isTargetedAbility;
		cAbility.SpawnDelay = spawnDelay;
		cAbility.AllTargetsOnMovePath = allTargetsOnMovePath;
		cAbility.AllTargetsOnMovePathSameStartAndEnd = allTargetsOnMovePathSameStartAndEnd;
		cAbility.AllTargetsOnAttackPath = allTargetsOnAttackPath;
		cAbility.ConditionalOverrides = conditionalOverrides;
		cAbility.StartAbilityRequirements = startAbilityConditions;
		cAbility.AbilityXP = abilityXP;
		cAbility.IsItemAbility = isItemAbility;
		cAbility.Augment = augment;
		cAbility.Song = song;
		cAbility.m_IsMergedAbility = isMergedAbility;
		cAbility.m_IsModifierAbility = isModifierAbility;
		cAbility.CanUndoOverride = canUndoOverride;
		cAbility.m_XpPerTargetData = xpPerTargetData;
		cAbility.MiscAbilityData = miscAbilityData;
		cAbility.SkipAnim = skipAnim;
		cAbility.m_AbilityEnhancements = abilityEnhancements;
		cAbility.ResourcesToAddOnAbilityEnd = resourcesToAddOnAbilityEnd;
		cAbility.ResourcesToGiveToTargets = resourcesToGiveToTargets;
		cAbility.ResourcesToTakeFromTargets = resourcesToTakeFromTargets;
		cAbility.SubAbilities = ((subAbilities != null) ? subAbilities : new List<CAbility>());
		cAbility.AreaEffect = areaEffect;
		cAbility.AreaEffectYMLString = areaEffectYMLString;
		cAbility.AreaEffectLayoutOverrideYMLString = areaEffectLayoutOverrideYMLString;
		cAbility.ActiveBonusData = ((activeBonusData != null) ? activeBonusData : new AbilityData.ActiveBonusData());
		cAbility.ActiveBonusYML = activeBonusYML;
		cAbility.StatIsBasedOnXEntries = statIsBasedOnXEntries ?? new List<AbilityData.StatIsBasedOnXData>();
		cAbility.AddAttackBaseStat = addAttackBaseStat;
		cAbility.StrengthIsBase = strengthIsBase;
		cAbility.RangeIsBase = rangeIsBase;
		cAbility.TargetIsBase = targetIsBase;
		cAbility.TargetsSet = numberTargets != 0;
		cAbility.AbilityText = text;
		cAbility.AbilityTextOnly = textOnly;
		cAbility.ShowRange = showRange;
		cAbility.ShowTarget = showTarget;
		cAbility.ShowArea = showArea;
		cAbility.OnDeath = onDeath;
		cAbility.IsConsumeAbility = isConsumeAbility;
		cAbility.Targeting = targeting;
		cAbility.PreviewEffectId = previewEffectId;
		cAbility.PreviewEffectText = previewEffectText;
		cAbility.HelpBoxTooltipLocKey = helpBoxTooltipLocKey;
		if (positiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition condition in positiveConditions)
			{
				if (condition == CCondition.EPositiveCondition.NA)
				{
					continue;
				}
				if (condition == CCondition.EPositiveCondition.Bless && cAbility.PositiveConditions.ContainsKey(condition))
				{
					if (cAbility.PositiveConditions[condition].Strength == 0)
					{
						cAbility.PositiveConditions[condition].Strength++;
					}
					cAbility.PositiveConditions[condition].Strength++;
					continue;
				}
				try
				{
					EAbilityType abilityType2 = AbilityTypes.Single((EAbilityType x) => x.ToString() == condition.ToString());
					cAbility.m_PositiveConditions.Add(condition, CreateAbility(abilityType2, abilityFilter, cAbility.IsMonsterAbility, cAbility.IsTargetedAbility));
				}
				catch
				{
					DLLDebug.LogError("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
				}
			}
		}
		if (negativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition condition2 in negativeConditions)
			{
				if (condition2 == CCondition.ENegativeCondition.NA)
				{
					continue;
				}
				if (condition2 == CCondition.ENegativeCondition.Curse && cAbility.NegativeConditions.ContainsKey(condition2))
				{
					if (cAbility.NegativeConditions[condition2].Strength == 0)
					{
						cAbility.NegativeConditions[condition2].Strength++;
					}
					cAbility.NegativeConditions[condition2].Strength++;
					continue;
				}
				try
				{
					EAbilityType abilityType3 = AbilityTypes.Single((EAbilityType x) => x.ToString() == condition2.ToString());
					cAbility.m_NegativeConditions.Add(condition2, CreateAbility(abilityType3, abilityFilter, cAbility.IsMonsterAbility, cAbility.IsTargetedAbility));
				}
				catch
				{
					DLLDebug.LogError("Condition " + condition2.ToString() + " could not be found in EAbilityType enum.");
				}
			}
		}
		return cAbility;
	}

	public void EnhanceAbility(CEnhancement enhancement)
	{
		CEnhancement cEnhancement = AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == enhancement.EnhancementLine && s.EnhancementSlot == enhancement.EnhancementSlot);
		if (cEnhancement != null)
		{
			cEnhancement.Enhancement = enhancement.Enhancement;
			cEnhancement.PaidPrice = enhancement.PaidPrice;
		}
	}

	public string GetTargetAbilityDescription()
	{
		string text = "   ";
		foreach (CAbility subAbility in SubAbilities)
		{
			if (text.Length > 0)
			{
				text += " Then ";
			}
			text = text + "For Each Target " + subAbility.GetDescription();
		}
		return text;
	}

	public string GetAreaEffectDescription()
	{
		if (AreaEffect == null)
		{
			return "";
		}
		return "   Area Effect : " + AreaEffect.Name;
	}

	public void PreProcessTargetAbility(CActor targetActor)
	{
		if (SubAbilities == null)
		{
			return;
		}
		foreach (CAbility subAbility in SubAbilities)
		{
			if (subAbility.AbilityType == EAbilityType.Damage && subAbility.IsInlineSubAbility)
			{
				CTargetAbilityRange_MessageData cTargetAbilityRange_MessageData = new CTargetAbilityRange_MessageData(targetActor);
				cTargetAbilityRange_MessageData.TargetingActor = targetActor;
				cTargetAbilityRange_MessageData.m_TargetAbilityRange = GameState.GetTilesInRange(targetActor, subAbility.Range, Targeting, emptyTilesOnly: false);
				ScenarioRuleClient.MessageHandler(cTargetAbilityRange_MessageData);
			}
		}
	}

	public void ProcessPositiveStatusEffects(CActor target)
	{
		if (m_PositiveConditions == null)
		{
			return;
		}
		foreach (CAbility value in m_PositiveConditions.Values)
		{
			if (value.AbilityType != AbilityType)
			{
				value.Start(TargetingActor, FilterActor);
				((CAbilityTargeting)value).ApplyToActor(target);
			}
		}
	}

	public void ProcessNegativeStatusEffects(CActor target)
	{
		if (m_NegativeConditions == null)
		{
			return;
		}
		foreach (CAbility value in m_NegativeConditions.Values)
		{
			if (value.AbilityType != AbilityType)
			{
				value.Start(TargetingActor, FilterActor);
				((CAbilityTargeting)value).ApplyToActor(target);
			}
		}
	}

	public void UpdateAreaEffect(CTile positionTile, float rotation)
	{
		if (m_AreaEffectLocked)
		{
			return;
		}
		bool num = rotation != AreaEffectAngle;
		AreaEffectAngle = rotation;
		m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, positionTile, AreaEffect, rotation, TilesInRange, getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
		if (!num)
		{
			return;
		}
		string text = "AoE Rotation changed, tiles in AoE are now: ";
		foreach (CTile item in m_ValidTilesInAreaEffectIncludingBlocked)
		{
			text = text + item.m_ArrayIndex.ToString() + ", ";
		}
		SimpleLog.AddToSimpleLog(text);
	}

	public int ValidTargetsInArea()
	{
		int num = 0;
		foreach (CTile item in m_ValidTilesInAreaEffectIncludingBlocked)
		{
			if (GameState.GetActorOnTile(item, FilterActor, AbilityFilter, null, IsTargetedAbility, MiscAbilityData?.CanTargetInvisible) != null)
			{
				num++;
			}
		}
		return num;
	}

	public List<CActor> GetValidActorsInArea(List<CTile> tiles)
	{
		List<CActor> list = new List<CActor>();
		foreach (CTile tile in tiles)
		{
			CActor actorOnTile = GameState.GetActorOnTile(tile, FilterActor, AbilityFilter, null, IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
			if (actorOnTile != null && !ImmuneToAbility(actorOnTile, this))
			{
				list.Add(actorOnTile);
			}
		}
		return list;
	}

	public virtual void RemoveImmuneActorsFromList(ref List<CActor> actorList)
	{
		for (int num = actorList.Count - 1; num >= 0; num--)
		{
			CActor cActor = actorList[num];
			if (cActor != null)
			{
				bool flag = false;
				if (ImmuneToAbility(cActor, this))
				{
					flag = true;
				}
				if (flag)
				{
					actorList.Remove(cActor);
				}
			}
		}
	}

	public void SetID(Guid id)
	{
		ID = id;
	}

	public virtual bool ApplyToActor(CActor actor)
	{
		bool flag = ImmuneToAbility(actor, this);
		if (!flag)
		{
			if (ActorsTargeted == null)
			{
				ActorsTargeted = new List<CActor>();
			}
			ActorsTargeted.Add(actor);
			if (ResourcesToTakeFromTargets != null && ResourcesToTakeFromTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in ResourcesToTakeFromTargets)
				{
					if (actor.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
					{
						actor.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
						TargetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
					}
				}
			}
			if (ResourcesToGiveToTargets != null && ResourcesToGiveToTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToGiveToTarget in ResourcesToGiveToTargets)
				{
					if (TargetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
					{
						TargetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
						actor.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					}
				}
			}
			if (!IsScenarioModifierAbility)
			{
				LogEvent(ESESubTypeAbility.ApplyToActor);
			}
			if (XpPerTargetData != null)
			{
				TargetingActor.GainXP(XpPerTargetData.AttackTarget(TargetingActor, actor));
			}
		}
		return !flag;
	}

	public static bool ImmuneToAbility(CActor actor, CAbility ability)
	{
		if (actor.Immunities.Contains(ability.AbilityType))
		{
			AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
			if (miscAbilityData == null || miscAbilityData.BypassImmunity != true)
			{
				return true;
			}
		}
		if (ability is CAbilityAttack cAbilityAttack)
		{
			foreach (EAttackType item in cAbilityAttack.AttackIsAttackTypes())
			{
				if (actor.AttackTypeImmunities.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CheckForAdjacentSleepingActorsToAwaken()
	{
		foreach (CActor item in GameState.GetActorsInRange(TargetingActor, TargetingActor, 1, new List<CActor> { TargetingActor }, CAbilityFilterContainer.CreateDefaultFilter(), null, null, isTargetedAbility: false, null, true))
		{
			if (item.Tokens.HasKey(CCondition.ENegativeCondition.Sleep) && !CActor.AreActorsAllied(TargetingActor.Type, item.Type))
			{
				item.RemoveNegativeConditionToken(CCondition.ENegativeCondition.Sleep);
				CActorAwakened_MessageData message = new CActorAwakened_MessageData(item);
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	private static Dictionary<EAbilityType, EAbilityStatType> GetAbilityDisplayValues()
	{
		Dictionary<EAbilityType, EAbilityStatType> dictionary = new Dictionary<EAbilityType, EAbilityStatType>();
		EAbilityType[] abilityTypes = AbilityTypes;
		foreach (EAbilityType eAbilityType in abilityTypes)
		{
			switch (eAbilityType)
			{
			case EAbilityType.Move:
			case EAbilityType.Attack:
			case EAbilityType.Heal:
			case EAbilityType.Damage:
			case EAbilityType.PreventDamage:
			case EAbilityType.Shield:
			case EAbilityType.Retaliate:
			case EAbilityType.Trap:
			case EAbilityType.RecoverLostCards:
			case EAbilityType.Push:
			case EAbilityType.Pull:
				dictionary.Add(eAbilityType, EAbilityStatType.Strength);
				break;
			case EAbilityType.Loot:
				dictionary.Add(eAbilityType, EAbilityStatType.Range);
				break;
			default:
				dictionary.Add(eAbilityType, EAbilityStatType.None);
				break;
			case EAbilityType.None:
				break;
			}
		}
		return dictionary;
	}

	public virtual void ToggleSingleTargetItem(CItem item)
	{
		if (m_ActiveSingleTargetItems.Contains(item))
		{
			item.SingleTarget = null;
			m_ActiveSingleTargetItems.Remove(item);
			return;
		}
		item.SingleTarget = null;
		m_ActiveSingleTargetItems.Add(item);
		if (ActorsToTarget.Count == 1)
		{
			ApplySingleTargetItem(ActorsToTarget[0]);
			Perform();
		}
	}

	public virtual bool IsSingleTargetItemActive(CItem item)
	{
		return m_ActiveSingleTargetItems.Contains(item);
	}

	public virtual void ToggleSingleTargetActiveBonus(CActiveBonus activeBonus)
	{
		if (m_ActiveSingleTargetActiveBonuses.Contains(activeBonus))
		{
			activeBonus.SingleTarget = null;
			m_ActiveSingleTargetActiveBonuses.Remove(activeBonus);
		}
		else if (ActorsToTarget != null)
		{
			activeBonus.SingleTarget = null;
			m_ActiveSingleTargetActiveBonuses.Add(activeBonus);
			if (ActorsToTarget.Count == 1)
			{
				ApplySingleTargetActiveBonus(ActorsToTarget[0]);
				Perform();
			}
			else
			{
				CWaitForSingleTargetActiveBonus_MessageData cWaitForSingleTargetActiveBonus_MessageData = new CWaitForSingleTargetActiveBonus_MessageData(TargetingActor);
				cWaitForSingleTargetActiveBonus_MessageData.m_ActiveBonus = activeBonus;
				ScenarioRuleClient.MessageHandler(cWaitForSingleTargetActiveBonus_MessageData);
			}
		}
	}

	public virtual bool IsSingleTargetActiveBonusActive(CActiveBonus activeBonus)
	{
		return m_ActiveSingleTargetActiveBonuses.Contains(activeBonus);
	}

	public bool HasID(Guid id)
	{
		if (id == ID)
		{
			return true;
		}
		foreach (CAbility subAbility in SubAbilities)
		{
			if (subAbility.HasID(id))
			{
				return true;
			}
		}
		if (Augment != null && Augment.Abilities != null)
		{
			foreach (CAbility ability in Augment.Abilities)
			{
				if (ability.HasID(id))
				{
					return true;
				}
			}
		}
		if (Song != null && Song.SongEffects != null)
		{
			foreach (CSong.SongEffect songEffect in Song.SongEffects)
			{
				if (songEffect.Abilities == null)
				{
					continue;
				}
				foreach (CAbility ability2 in songEffect.Abilities)
				{
					if (ability2.HasID(id))
					{
						return true;
					}
				}
			}
		}
		if (this is CAbilityControlActor cAbilityControlActor && cAbilityControlActor.ControlActorData.ControlAbilities != null)
		{
			foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
			{
				if (controlAbility.HasID(id))
				{
					return true;
				}
			}
		}
		if (this is CAbilityAddActiveBonus cAbilityAddActiveBonus && cAbilityAddActiveBonus.AddAbility.HasID(id))
		{
			return true;
		}
		return false;
	}

	public bool IsAnOffensiveAbility()
	{
		if (AbilityType != EAbilityType.Attack && AbilityType != EAbilityType.Damage && AbilityType != EAbilityType.Poison && AbilityType != EAbilityType.Wound && AbilityType != EAbilityType.Muddle && AbilityType != EAbilityType.Immobilize && AbilityType != EAbilityType.Disarm && AbilityType != EAbilityType.Curse && AbilityType != EAbilityType.Stun && AbilityType != EAbilityType.StopFlying && AbilityType != EAbilityType.AddCondition)
		{
			return AbilityType == EAbilityType.Sleep;
		}
		return true;
	}

	public CAbility FindAbility(string abilityName)
	{
		if (Name == abilityName)
		{
			return this;
		}
		CAbility cAbility = SubAbilities.SingleOrDefault((CAbility x) => x.Name == abilityName);
		if (cAbility != null)
		{
			return cAbility;
		}
		if (Augment != null && Augment.Abilities != null && Augment.Abilities.Count > 0)
		{
			CAbility cAbility2 = Augment.Abilities.SingleOrDefault((CAbility s) => s.Name == abilityName);
			if (cAbility2 != null)
			{
				return cAbility2;
			}
		}
		if (Song != null && Song.SongEffects != null && Song.SongEffects.Count > 0)
		{
			foreach (CSong.SongEffect songEffect in Song.SongEffects)
			{
				CAbility cAbility3 = songEffect.Abilities.SingleOrDefault((CAbility s) => s.Name == abilityName);
				if (cAbility3 != null)
				{
					return cAbility3;
				}
			}
		}
		if (this is CAbilityMerged cAbilityMerged)
		{
			CAbility cAbility4 = cAbilityMerged.MergedAbilities.SingleOrDefault((CAbility s) => s.Name == abilityName);
			if (cAbility4 != null)
			{
				return cAbility4;
			}
		}
		if (this is CAbilityControlActor cAbilityControlActor)
		{
			CAbility cAbility5 = cAbilityControlActor.ControlActorData.ControlAbilities.SingleOrDefault((CAbility s) => s.Name == abilityName);
			if (cAbility5 != null)
			{
				return cAbility5;
			}
		}
		return null;
	}

	public bool IsWaitingForSingleTargetItemOrActiveBonus()
	{
		if (!IsWaitingForSingleTargetItem())
		{
			return IsWaitingForSingleTargetActiveBonus();
		}
		return true;
	}

	public bool IsWaitingForSingleTargetItem()
	{
		return m_ActiveSingleTargetItems.Where((CItem w) => w.SingleTarget == null).Count() > 0;
	}

	public bool IsWaitingForSingleTargetActiveBonus()
	{
		return m_ActiveSingleTargetActiveBonuses.Where((CActiveBonus w) => w.SingleTarget == null).Count() > 0;
	}

	public void SetCanSkip(bool canSkip)
	{
		m_CanSkip = canSkip;
	}

	public void SetCanUndo(bool canUndo)
	{
		m_CanUndo = canUndo;
	}

	public void SetStatBasedOnXFromTarget(CActor targetedActor, CActor targetingActor, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXDataEntries, CAbilityFilterContainer abilityFilter = null, CAbility sourceAbility = null)
	{
		if (statIsBasedOnXDataEntries == null)
		{
			return;
		}
		CAbility cAbility = sourceAbility ?? this;
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXDataEntry in statIsBasedOnXDataEntries)
		{
			int num = 0;
			float num2 = 0f;
			bool flag = false;
			_ = statIsBasedOnXDataEntry.Filter;
			switch (statIsBasedOnXDataEntry.BasedOn)
			{
			case EStatIsBasedOnXType.TargetHPDifference:
				num2 = (float)Math.Abs(targetedActor.OriginalMaxHealth - targetedActor.Health) * statIsBasedOnXDataEntry.Multiplier;
				flag = true;
				break;
			case EStatIsBasedOnXType.TargetDamageTaken:
				num2 = (float)Math.Abs(targetedActor.MaxHealth - targetedActor.Health) * statIsBasedOnXDataEntry.Multiplier;
				flag = true;
				break;
			case EStatIsBasedOnXType.TargetDistanceRangeDiff:
				num2 = (float)Math.Abs(cAbility.Range - SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(targetedActor, targetingActor)) * statIsBasedOnXDataEntry.Multiplier;
				flag = true;
				break;
			}
			switch (statIsBasedOnXDataEntry?.RoundingType ?? EStatIsBasedOnXRoundingType.RoundOff)
			{
			case EStatIsBasedOnXRoundingType.RoundOff:
				num = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
				break;
			case EStatIsBasedOnXRoundingType.ToFloor:
				num = (int)Math.Floor(num2);
				break;
			case EStatIsBasedOnXRoundingType.ToCeil:
				num = (int)Math.Ceiling(num2);
				break;
			}
			if (flag)
			{
				switch (statIsBasedOnXDataEntry.AbilityStatType)
				{
				case EAbilityStatType.Strength:
					m_Strength = (statIsBasedOnXDataEntry.AddTo ? m_Strength : 0) + num;
					break;
				case EAbilityStatType.Range:
					m_Range = (statIsBasedOnXDataEntry.AddTo ? m_Range : 0) + num;
					break;
				case EAbilityStatType.NumberOfTargets:
					m_NumberTargets = (statIsBasedOnXDataEntry.AddTo ? m_NumberTargets : 0) + num;
					break;
				}
				if (statIsBasedOnXDataEntry.AddTo)
				{
					statIsBasedOnXDataEntry.AddedStat = num;
				}
			}
		}
	}

	public void SetStatBasedOnX(CActor targetingActor, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXDataEntries, CAbilityFilterContainer abilityFilter = null)
	{
		if (statIsBasedOnXDataEntries == null)
		{
			return;
		}
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXDataEntry in statIsBasedOnXDataEntries)
		{
			int statIsBasedOnXValue = GetStatIsBasedOnXValue(targetingActor, statIsBasedOnXDataEntry, abilityFilter);
			switch (statIsBasedOnXDataEntry.AbilityStatType)
			{
			case EAbilityStatType.Strength:
				m_Strength = (statIsBasedOnXDataEntry.AddTo ? m_Strength : 0) + statIsBasedOnXValue;
				break;
			case EAbilityStatType.Range:
				m_Range = (statIsBasedOnXDataEntry.AddTo ? m_Range : 0) + statIsBasedOnXValue;
				break;
			case EAbilityStatType.NumberOfTargets:
				m_NumberTargets = (statIsBasedOnXDataEntry.AddTo ? m_NumberTargets : 0) + statIsBasedOnXValue;
				break;
			}
			if (statIsBasedOnXDataEntry.AddTo)
			{
				statIsBasedOnXDataEntry.AddedStat = statIsBasedOnXValue;
			}
		}
	}

	public int GetStatIsBasedOnXValue(CActor targetingActor, AbilityData.StatIsBasedOnXData statIsBasedOnXData, CAbilityFilterContainer abilityFilter, EStatIsBasedOnXType basedOnOverride = EStatIsBasedOnXType.None, float xVariableOverride = 1f, float yVariableOverride = 1f)
	{
		int num = 0;
		float num2 = 0f;
		CAbilityFilterContainer filter = statIsBasedOnXData?.Filter ?? abilityFilter;
		EStatIsBasedOnXType eStatIsBasedOnXType = ((basedOnOverride == EStatIsBasedOnXType.None && statIsBasedOnXData != null) ? statIsBasedOnXData.BasedOn : basedOnOverride);
		float num3 = ((xVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.Multiplier : xVariableOverride);
		float num4 = ((yVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.SecondVariable : yVariableOverride);
		switch (eStatIsBasedOnXType)
		{
		case EStatIsBasedOnXType.HexesMovedThisTurn:
			num2 = (float)GameState.HexesMovedThisTurn.Count * num3;
			break;
		case EStatIsBasedOnXType.HexesMovedThisAction:
			num2 = (float)CPhaseAction.HexesMovedThisAction * num3;
			break;
		case EStatIsBasedOnXType.HexesMovedByParentAbility:
			if (ParentAbility != null && ParentAbility is CAbilityMove cAbilityMove)
			{
				num2 = (float)cAbilityMove.MoveCount * num3;
			}
			break;
		case EStatIsBasedOnXType.DamageInflictedThisTurn:
			num2 = (float)GameState.DamageInflictedThisTurn * num3;
			break;
		case EStatIsBasedOnXType.DamageInflictedThisAction:
			num2 = (float)CPhaseAction.DamageInflictedThisAction * num3;
			break;
		case EStatIsBasedOnXType.DamageInflictedByPreviousAbility:
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.PreviousPhaseAbilities.Count > 0)
			{
				CPhaseAction.CPhaseAbility cPhaseAbility = cPhaseAction.PreviousPhaseAbilities.Last();
				if (cPhaseAbility?.m_Ability != null)
				{
					num2 = (float)cPhaseAbility.m_Ability.DamageInflictedByAbility * num3;
				}
			}
			break;
		case EStatIsBasedOnXType.DamageInflictedByParent:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.DamageInflictedByAbility * num3;
			}
			break;
		case EStatIsBasedOnXType.DamageActuallyTakenByParent:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.DamageActuallyTakenByAbility * num3;
			}
			break;
		case EStatIsBasedOnXType.ActorsKilledThisAction:
			num2 = (float)CPhaseAction.ActorsKilledThisAction.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ActorsKilledThisTurn:
			num2 = (float)GameState.ActorsKilledThisTurn.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ActorsKilledThisRound:
			num2 = (float)GameState.ActorsKilledThisRound.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ActorsKilledThisScenario:
			num2 = (float)ScenarioManager.Scenario.AllDeadActors.Count((CActor x) => x.KilledByActor == targetingActor) * num3;
			break;
		case EStatIsBasedOnXType.ObstaclesDestroyedThisTurn:
			num2 = (float)GameState.ObstaclesDestroyedThisTurn * num3;
			break;
		case EStatIsBasedOnXType.ObstaclesDestroyedThisAction:
			num2 = (float)CPhaseAction.ObstaclesDestroyedThisAction * num3;
			break;
		case EStatIsBasedOnXType.ObstaclesDestroyedByParent:
			DLLDebug.LogError("Attempted to apply non implemented StrengthCondition, ObstaclesDestroyedByParent");
			break;
		case EStatIsBasedOnXType.ObstaclesToBeDestroyedByMergedAbility:
		{
			if (ParentAbility == null || !(ParentAbility is CAbilityMerged cAbilityMerged))
			{
				break;
			}
			CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
			if (mergedWithAbility == null || !(mergedWithAbility is CAbilityDestroyObstacle cAbilityDestroyObstacle))
			{
				break;
			}
			int num9 = 0;
			foreach (CTile item in cAbilityDestroyObstacle.TilesSelected)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle cObjectObstacle)
				{
					num9 += cObjectObstacle.PathingBlockers.Count;
				}
			}
			num2 = (float)num9 * num3;
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentActors:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentActors.Count * num3;
			string text3 = "Setting Strength based on Target Adjacent Actors:";
			if (filter.LastCheckedTargetAdjacentActors.Count > 0)
			{
				foreach (CActor lastCheckedTargetAdjacentActor in filter.LastCheckedTargetAdjacentActors)
				{
					text3 = text3 + "\n" + lastCheckedTargetAdjacentActor.ActorLocKey() + ((lastCheckedTargetAdjacentActor is CEnemyActor { ID: var iD3 }) ? iD3.ToString() : "") + "at array index: " + lastCheckedTargetAdjacentActor.ArrayIndex.ToString();
				}
			}
			else
			{
				text3 += "\nNo Target Adjacent Actors";
			}
			SimpleLog.AddToSimpleLog(text3);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentEnemies:
		case EStatIsBasedOnXType.TargetAdjacentEnemiesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentEnemies.Count * num3;
			string text6 = "Setting Strength based on Target Adjacent Enemies:";
			if (filter.LastCheckedTargetAdjacentEnemies.Count > 0)
			{
				foreach (CActor lastCheckedTargetAdjacentEnemy in filter.LastCheckedTargetAdjacentEnemies)
				{
					text6 = text6 + "\n" + lastCheckedTargetAdjacentEnemy.ActorLocKey() + ((lastCheckedTargetAdjacentEnemy is CEnemyActor { ID: var iD6 }) ? iD6.ToString() : "") + "at array index: " + lastCheckedTargetAdjacentEnemy.ArrayIndex.ToString();
				}
			}
			else
			{
				text6 += "\nNo Target Adjacent Enemies of Caster";
			}
			SimpleLog.AddToSimpleLog(text6);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentAllies:
		case EStatIsBasedOnXType.TargetAdjacentAlliesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentAllies.Count * num3;
			string text5 = "Setting Strength based on Target Adjacent Allies:";
			if (filter.LastCheckedTargetAdjacentAllies.Count > 0)
			{
				foreach (CActor lastCheckedTargetAdjacentAlly in filter.LastCheckedTargetAdjacentAllies)
				{
					text5 = text5 + "\n" + lastCheckedTargetAdjacentAlly.ActorLocKey() + ((lastCheckedTargetAdjacentAlly is CEnemyActor { ID: var iD5 }) ? iD5.ToString() : "") + "at array index: " + lastCheckedTargetAdjacentAlly.ArrayIndex.ToString();
				}
			}
			else
			{
				text5 += "\nNo Target Adjacent Allies of Caster";
			}
			SimpleLog.AddToSimpleLog(text5);
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentAlliesOfTarget:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedTargetAdjacentAlliesOfTarget.Count * num3;
			string text4 = "Setting Strength based on Target Adjacent Allies:";
			if (filter.LastCheckedTargetAdjacentAlliesOfTarget.Count > 0)
			{
				foreach (CActor item2 in filter.LastCheckedTargetAdjacentAlliesOfTarget)
				{
					text4 = text4 + "\n" + item2.ActorLocKey() + ((item2 is CEnemyActor { ID: var iD4 }) ? iD4.ToString() : "") + "at array index: " + item2.ArrayIndex.ToString();
				}
			}
			else
			{
				text4 += "\nNo Target Adjacent Allies of Target";
			}
			SimpleLog.AddToSimpleLog(text4);
			break;
		}
		case EStatIsBasedOnXType.CasterAdjacentEnemies:
		case EStatIsBasedOnXType.CasterAdjacentEnemiesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedCasterAdjacentEnemies.Count * num3;
			string text2 = "Setting Strength based on Caster Adjacent Enemies:";
			if (filter.LastCheckedCasterAdjacentEnemies.Count > 0)
			{
				foreach (CActor lastCheckedCasterAdjacentEnemy in filter.LastCheckedCasterAdjacentEnemies)
				{
					text2 = text2 + "\n" + lastCheckedCasterAdjacentEnemy.ActorLocKey() + ((lastCheckedCasterAdjacentEnemy is CEnemyActor { ID: var iD2 }) ? iD2.ToString() : "") + "at array index: " + lastCheckedCasterAdjacentEnemy.ArrayIndex.ToString();
				}
			}
			else
			{
				text2 += "\nNo Caster Adjacent Enemies of Caster";
			}
			SimpleLog.AddToSimpleLog(text2);
			break;
		}
		case EStatIsBasedOnXType.CasterAdjacentAllies:
		case EStatIsBasedOnXType.CasterAdjacentAlliesOfCaster:
		{
			if (filter == null)
			{
				break;
			}
			num2 = (float)filter.LastCheckedCasterAdjacentAllies.Count * num3;
			string text = "Setting Strength based on Caster Adjacent Allies:";
			if (filter.LastCheckedCasterAdjacentAllies.Count > 0)
			{
				foreach (CActor lastCheckedCasterAdjacentAlly in filter.LastCheckedCasterAdjacentAllies)
				{
					text = text + "\n" + lastCheckedCasterAdjacentAlly.ActorLocKey() + ((lastCheckedCasterAdjacentAlly is CEnemyActor { ID: var iD }) ? iD.ToString() : "") + "at array index: " + lastCheckedCasterAdjacentAlly.ArrayIndex.ToString();
				}
			}
			else
			{
				text += "\nNo Caster Adjacent Allies of Caster";
			}
			break;
		}
		case EStatIsBasedOnXType.TargetAdjacentWalls:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedTargetAdjacentWalls * num3;
				SimpleLog.AddToSimpleLog("Setting Strength based on Target Adjacent Walls:");
			}
			break;
		case EStatIsBasedOnXType.CasterAdjacentWalls:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedCasterAdjacentWalls * num3;
				SimpleLog.AddToSimpleLog("Setting Strength based on Caster Adjacent Walls:");
			}
			break;
		case EStatIsBasedOnXType.TargetAdjacentValidTiles:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedTargetAdjacentTiles * num3;
				SimpleLog.AddToSimpleLog("Setting Strength based on Target Adjacent Valid Tiles:");
			}
			break;
		case EStatIsBasedOnXType.CasterAdjacentValidTiles:
			if (filter != null)
			{
				num2 = (float)filter.LastCheckedCasterAdjacentValidTiles * num3;
				SimpleLog.AddToSimpleLog("Setting Strength based on Caster Adjacent Valid Tiles:");
			}
			break;
		case EStatIsBasedOnXType.DamageInflictedByParentOnLastTarget:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.DamageInflictedByAbilityOnLastTarget * num3;
			}
			break;
		case EStatIsBasedOnXType.ExcessDamageInflictedByParentOnLastTargetKilled:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.ExcessDamageInflictedOnLastTargetKilled * num3;
			}
			break;
		case EStatIsBasedOnXType.ActorsTargetedByParentAbility:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.ActorsTargeted.Count((CActor x) => filter == null || filter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false)) * num3;
			}
			break;
		case EStatIsBasedOnXType.ActorsKilledByParentAbility:
			if (ParentAbility != null)
			{
				num2 = (float)ParentAbility.ActorsTargeted.Count((CActor x) => x.IsDead && (filter == null || filter.IsValidTarget(x, TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false))) * num3;
			}
			break;
		case EStatIsBasedOnXType.LostCardCount:
			if (targetingActor is CPlayerActor cPlayerActor)
			{
				num2 = (float)(cPlayerActor.CharacterClass.LostAbilityCards.Count + cPlayerActor.CharacterClass.PermanentlyLostAbilityCards.Count + cPlayerActor.CharacterClass.ActivatedAbilityCards.Count((CAbilityCard x) => x != null && x.SelectedAction?.CardPile == CBaseCard.ECardPile.Lost)) * num3;
			}
			break;
		case EStatIsBasedOnXType.PercentageOfCurrentHP:
			num2 = (float)targetingActor.Health * num3;
			break;
		case EStatIsBasedOnXType.HPDifference:
			num2 = (float)Math.Abs(targetingActor.OriginalMaxHealth - targetingActor.Health) * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousAttackThisTurn:
			num2 = (float)GameState.TargetsDamagedInPrevAttackThisTurn * num3;
			break;
		case EStatIsBasedOnXType.TargetsActuallyDamagedByPreviousAttackThisTurn:
			num2 = (float)GameState.TargetsActuallyDamagedInPrevAttackThisTurn * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousAttackThisAction:
			num2 = (float)CPhaseAction.TargetsDamagedInPrevAttackThisAction * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousDamageAbilityThisTurn:
			num2 = (float)GameState.TargetsDamagedInPrevDamageAbilityThisTurn * num3;
			break;
		case EStatIsBasedOnXType.TargetsDamagedByPreviousDamageAbilityThisAction:
			num2 = (float)CPhaseAction.TargetsDamagedInPrevDamageAbilityThisAction * num3;
			break;
		case EStatIsBasedOnXType.HalfTargetsDamagedByPreviousDamageAbilityThisAction:
			num2 = (float)(CPhaseAction.TargetsDamagedInPrevDamageAbilityThisAction / 2) * num3;
			break;
		case EStatIsBasedOnXType.ActorCount:
		{
			int num8 = 0;
			foreach (ActorState actorState4 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState4.IsRevealed)
				{
					CActor cActor3 = null;
					cActor3 = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState4.ActorGuid);
					if (cActor3 == null)
					{
						cActor3 = ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState4.ActorGuid);
					}
					if (cActor3 == null)
					{
						cActor3 = ScenarioManager.Scenario.AllAliveMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState4.ActorGuid);
					}
					if (cActor3 == null)
					{
						cActor3 = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState4.ActorGuid);
					}
					if (cActor3 != null && filter != null && filter.IsValidTarget(cActor3, targetingActor, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num8++;
					}
				}
				else if (filter != null && statIsBasedOnXData.IncludeUnrevealed && (!(actorState4 is EnemyState enemyState3) || enemyState3.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden))
				{
					ActorState actorState5 = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == targetingActor.ActorGuid);
					if (actorState5 != null && filter.IsValidTarget_ActorState(actorState4, actorState5, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num8++;
					}
				}
			}
			if (statIsBasedOnXData.IncludeUnrevealed)
			{
				foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && !prop.PropActorHasBeenAssigned && prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						CTile propTile2 = ScenarioManager.Tiles[prop.ArrayIndex.X, prop.ArrayIndex.Y];
						int propStartingHealth2 = prop.PropHealthDetails.GetPropStartingHealth();
						ObjectState objectState2 = prop.PropHealthDetails.CreateStateForPropWithHealth(propTile2, propStartingHealth2, prop.PropHealthDetails.ActorType);
						if (filter.IsValidTarget_ActorState(objectState2, objectState2, isTargetedAbility: false, useTargetOriginalType: false, true))
						{
							num8++;
						}
						objectState2 = null;
					}
				}
			}
			num2 = (float)num8 * num3;
			break;
		}
		case EStatIsBasedOnXType.DeadActorCount:
		{
			int num7 = 0;
			foreach (ActorState actorState3 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState3 is EnemyState enemyState2 && enemyState2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				CActor cActor2 = null;
				cActor2 = ScenarioManager.Scenario.ExhaustedPlayers.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState3.ActorGuid);
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadHeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadEnemies.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadEnemy2Monsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadAllyMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.DeadNeutralMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 == null)
				{
					cActor2 = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState3.ActorGuid);
				}
				if (cActor2 != null && filter != null && filter.IsValidTarget(cActor2, cActor2, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					num7++;
				}
			}
			num2 = (float)num7 * num3;
			break;
		}
		case EStatIsBasedOnXType.ScenarioLevel:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Level, 1) * num3;
			break;
		case EStatIsBasedOnXType.InitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) * num3;
			break;
		case EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) + num3;
			break;
		case EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
			num2 = num3 + (float)(ScenarioManager.CurrentScenarioState.Players.Count * ScenarioManager.CurrentScenarioState.Level);
			break;
		case EStatIsBasedOnXType.CharactersTimesLevelPlusX:
			num2 = (num3 + (float)ScenarioManager.CurrentScenarioState.Level) * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.XAddedToYTimesLevel:
			num2 = num3 + num4 * (float)ScenarioManager.CurrentScenarioState.Level;
			break;
		case EStatIsBasedOnXType.XPlusCharactersPlusLevel:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level;
			break;
		case EStatIsBasedOnXType.XPlusLevel:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Level;
			break;
		case EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
			num2 = (float)ScenarioManager.CurrentScenarioState.Level + num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
			num2 = num3 + (float)(int)Math.Ceiling((float)ScenarioManager.CurrentScenarioState.Level * (float)ScenarioManager.CurrentScenarioState.Players.Count / num4);
			break;
		case EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level - num4;
			break;
		case EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
			num2 = (num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level - num4) * (float)ScenarioManager.CurrentScenarioState.RoundNumber;
			break;
		case EStatIsBasedOnXType.CharactersPlusLevelOverX:
			num2 = (float)ScenarioManager.CurrentScenarioState.Level / num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level * num4;
			break;
		case EStatIsBasedOnXType.CasterShieldValue:
			num2 = targetingActor.CalculateShield(null, ignoreNeutralizeShield: false, includeItemShieldValues: true);
			break;
		case EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count - num4;
			break;
		case EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
			num2 = (num3 + (float)ScenarioManager.CurrentScenarioState.Level) * ((float)ScenarioManager.CurrentScenarioState.Players.Count / num4);
			break;
		case EStatIsBasedOnXType.LevelTimesXPlusY:
			num2 = (float)ScenarioManager.CurrentScenarioState.Level * num3 + num4;
			break;
		case EStatIsBasedOnXType.LevelTimesXTimesHexesMovedThisTurnInDefinedRoom:
		{
			int num6 = 0;
			if (statIsBasedOnXData.CheckGUIDs != null && statIsBasedOnXData.CheckGUIDs.Count > 0)
			{
				foreach (TileIndex item3 in GameState.HexesMovedThisTurn)
				{
					CTile cTile = ScenarioManager.Tiles[item3.X, item3.Y];
					if (statIsBasedOnXData.CheckGUIDs.Contains(cTile.m_HexMap.MapGuid) || (cTile.m_Hex2Map != null && statIsBasedOnXData.CheckGUIDs.Contains(cTile.m_Hex2Map.MapGuid)))
					{
						num6++;
					}
				}
			}
			else
			{
				num6 = GameState.HexesMovedThisTurn.Count;
			}
			num2 = (float)Math.Floor((float)ScenarioManager.CurrentScenarioState.Level * num3) * (float)num6;
			break;
		}
		case EStatIsBasedOnXType.XTimesActorCountMinusY:
		{
			int num5 = 0;
			foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState.IsRevealed)
				{
					CActor cActor = null;
					cActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState.ActorGuid);
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState.ActorGuid);
					}
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.AllAliveMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
					}
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState.ActorGuid);
					}
					if (cActor != null && filter != null && filter.IsValidTarget(cActor, targetingActor, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num5++;
					}
				}
				else if (filter != null && statIsBasedOnXData.IncludeUnrevealed && (!(actorState is EnemyState enemyState) || enemyState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden))
				{
					ActorState actorState2 = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == targetingActor.ActorGuid);
					if (actorState2 != null && filter.IsValidTarget_ActorState(actorState, actorState2, isTargetedAbility: false, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
					{
						num5++;
					}
				}
			}
			if (statIsBasedOnXData.IncludeUnrevealed)
			{
				foreach (CObjectProp prop2 in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop2.PropHealthDetails != null && prop2.PropHealthDetails.HasHealth && !prop2.PropActorHasBeenAssigned && prop2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden)
					{
						CTile propTile = ScenarioManager.Tiles[prop2.ArrayIndex.X, prop2.ArrayIndex.Y];
						int propStartingHealth = prop2.PropHealthDetails.GetPropStartingHealth();
						ObjectState objectState = prop2.PropHealthDetails.CreateStateForPropWithHealth(propTile, propStartingHealth, prop2.PropHealthDetails.ActorType);
						if (filter.IsValidTarget_ActorState(objectState, objectState, isTargetedAbility: false, useTargetOriginalType: false, true))
						{
							num5++;
						}
						objectState = null;
					}
				}
			}
			num2 = (float)num5 * num3 - num4;
			break;
		}
		case EStatIsBasedOnXType.InitialPlayerCharacterCountMinusYTimesX:
			num2 = num3 * ((float)ScenarioManager.CurrentScenarioState.Players.Count - num4);
			break;
		}
		switch (statIsBasedOnXData?.RoundingType ?? EStatIsBasedOnXRoundingType.RoundOff)
		{
		case EStatIsBasedOnXRoundingType.RoundOff:
			num = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
			break;
		case EStatIsBasedOnXRoundingType.ToFloor:
			num = (int)Math.Floor(num2);
			break;
		case EStatIsBasedOnXRoundingType.ToCeil:
			num = (int)Math.Ceiling(num2);
			break;
		}
		if (statIsBasedOnXData != null)
		{
			num = Math.Max(statIsBasedOnXData.MinValue, num);
			num = Math.Min(statIsBasedOnXData.MaxValue, num);
		}
		return num;
	}

	public void ResetStatBasedOnXAddedStats(List<AbilityData.StatIsBasedOnXData> statIsBasedOnXDataEntries)
	{
		if (statIsBasedOnXDataEntries == null)
		{
			return;
		}
		foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXDataEntry in statIsBasedOnXDataEntries)
		{
			if (statIsBasedOnXDataEntry.AddTo)
			{
				switch (statIsBasedOnXDataEntry.AbilityStatType)
				{
				case EAbilityStatType.Strength:
					m_Strength -= statIsBasedOnXDataEntry.AddedStat;
					break;
				case EAbilityStatType.Range:
					m_Range -= statIsBasedOnXDataEntry.AddedStat;
					break;
				case EAbilityStatType.NumberOfTargets:
					m_NumberTargets -= statIsBasedOnXDataEntry.AddedStat;
					break;
				}
			}
			statIsBasedOnXDataEntry.AddedStat = 0;
		}
	}

	public virtual void Reset()
	{
		foreach (CEnhancement abilityEnhancement in AbilityEnhancements)
		{
			abilityEnhancement.Enhancement = EEnhancement.NoEnhancement;
		}
		if (SubAbilities != null)
		{
			foreach (CAbility subAbility in SubAbilities)
			{
				subAbility.Reset();
			}
		}
		if (this is CAbilityChoose { ChooseAbilities: not null } cAbilityChoose)
		{
			foreach (CAbility chooseAbility in cAbilityChoose.ChooseAbilities)
			{
				chooseAbility.Reset();
			}
		}
		if (this is CAbilityMerged { MergedAbilities: not null } cAbilityMerged)
		{
			foreach (CAbility mergedAbility in cAbilityMerged.MergedAbilities)
			{
				mergedAbility.Reset();
			}
		}
		if (!(this is CAbilityControlActor cAbilityControlActor) || cAbilityControlActor.ControlActorData.ControlAbilities == null)
		{
			return;
		}
		foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
		{
			controlAbility.Reset();
		}
	}

	public virtual void Restart()
	{
		m_CancelAbility = false;
		if (StartAbilityRequirements != null)
		{
			if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.ThisAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, this))
				{
					m_CancelAbility = true;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
			{
				if (!StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, ParentAbility))
				{
					m_CancelAbility = true;
				}
			}
			else if (StartAbilityRequirements.StartAbilityRequirementType.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
			{
				CPhaseAction.CPhaseAbility cPhaseAbility = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last();
				if ((!cPhaseAbility.m_Ability.AbilityHasHappened && StartAbilityRequirements.AbilityHasHappened != false) || !StartAbilityRequirements.MeetsAbilityRequirements(TargetingActor, cPhaseAbility.m_Ability))
				{
					m_CancelAbility = true;
				}
			}
		}
		CAbilityRestarted_MessageData message = new CAbilityRestarted_MessageData(TargetingActor);
		ScenarioRuleClient.MessageHandler(message);
		AbilityFilter.IsValidTarget(TargetingActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible);
		SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
	}

	public virtual void InterruptAbility()
	{
	}

	public virtual void ApplySingleTargetItem(CActor target)
	{
	}

	public virtual void ActiveBonusToggled(CActor actor, CActiveBonus activeBonus)
	{
	}

	public virtual void ApplySingleTargetActiveBonus(CActor target)
	{
	}

	public virtual void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		try
		{
			bool flag = false;
			if (!m_CurrentOverrides.Contains(abilityOverride))
			{
				return;
			}
			m_CurrentOverrides.Remove(abilityOverride);
			if (item != null && m_CurrentItemOverrides.ContainsKey(abilityOverride))
			{
				m_CurrentItemOverrides.Remove(abilityOverride);
			}
			if (abilityOverride.Strength.HasValue)
			{
				m_Strength -= abilityOverride.Strength.Value;
				if (TargetingActor != null)
				{
					m_ModifiedStrength -= abilityOverride.Strength.Value;
				}
				if (this is CAbilityPush cAbilityPush)
				{
					cAbilityPush.RemainingPushes -= abilityOverride.Strength.Value;
				}
				if (this is CAbilityPull cAbilityPull)
				{
					cAbilityPull.RemainingPulls -= abilityOverride.Strength.Value;
				}
			}
			if (abilityOverride.Range.HasValue)
			{
				if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
				{
					m_Range = abilityOverride.OriginalAbility.Range;
					m_OriginalRange = abilityOverride.OriginalAbility.Range;
				}
				else
				{
					m_Range -= abilityOverride.Range.Value;
					m_OriginalRange -= abilityOverride.Range.Value;
				}
			}
			if (abilityOverride.RangeAtLeastOne.HasValue && abilityOverride.OriginalAbility.Range <= 0)
			{
				m_Range = abilityOverride.OriginalAbility.Range;
				m_OriginalRange = abilityOverride.OriginalAbility.Range;
			}
			if (abilityOverride.NumberOfTargets.HasValue)
			{
				if (abilityOverride.TargetIsBase.HasValue && abilityOverride.TargetIsBase.Value)
				{
					m_NumberTargets = abilityOverride.OriginalAbility.NumberTargets;
				}
				else
				{
					m_NumberTargets -= abilityOverride.NumberOfTargets.Value;
				}
			}
			if (abilityOverride.TargetAtLeastOne.HasValue && abilityOverride.OriginalAbility.NumberTargets <= 0)
			{
				m_NumberTargets = abilityOverride.OriginalAbility.NumberTargets;
			}
			if (abilityOverride.RemoveConditionsOverride.HasValue)
			{
				m_PositiveConditions = abilityOverride.OriginalAbility.PositiveConditions;
				m_NegativeConditions = abilityOverride.OriginalAbility.NegativeConditions;
			}
			if (abilityOverride.Pierce.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).Pierce -= abilityOverride.Pierce.Value;
			}
			if (abilityOverride.SubAbilities != null)
			{
				foreach (CAbility subAbility in abilityOverride.SubAbilities)
				{
					SubAbilities.RemoveAll((CAbility x) => x.ID == subAbility.ID);
				}
			}
			if (abilityOverride.ChooseAbilities != null && this is CAbilityChoose cAbilityChoose)
			{
				foreach (CAbility chooseAbility in abilityOverride.ChooseAbilities)
				{
					if (cAbilityChoose.ChooseAbilities.Contains(chooseAbility))
					{
						cAbilityChoose.ChooseAbilities.Remove(chooseAbility);
					}
				}
			}
			if (abilityOverride.ElementsToInfuse != null && this is CAbilityInfuse)
			{
				CAbilityInfuse cAbilityInfuse = this as CAbilityInfuse;
				foreach (ElementInfusionBoardManager.EElement item2 in abilityOverride.ElementsToInfuse)
				{
					if (cAbilityInfuse.ElementsToInfuse.Contains(item2))
					{
						cAbilityInfuse.ElementsToInfuse.Remove(item2);
					}
				}
			}
			if (abilityOverride.AbilityType.HasValue)
			{
				flag = AbilityType != abilityOverride.OriginalAbility.AbilityType;
				AbilityType = abilityOverride.OriginalAbility.AbilityType;
			}
			if (abilityOverride.AbilityFilter != null)
			{
				AbilityFilter = abilityOverride.OriginalAbility.AbilityFilter;
			}
			if (abilityOverride.AnimationOverload != null)
			{
				AnimOverload = abilityOverride.OriginalAbility.AnimOverload;
			}
			if (abilityOverride.AreaEffect != null)
			{
				AreaEffect = abilityOverride.OriginalAbility.AreaEffect;
			}
			if (abilityOverride.AttackSourcesOnly.HasValue && this is CAbilityPreventDamage)
			{
				(this as CAbilityPreventDamage).AttackSourcesOnly = (abilityOverride.OriginalAbility as CAbilityPreventDamage).AttackSourcesOnly;
			}
			if (abilityOverride.Jump.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Jump = (abilityOverride.OriginalAbility as CAbilityMove).Jump;
			}
			if (abilityOverride.Fly.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Fly = (abilityOverride.OriginalAbility as CAbilityMove).Fly;
			}
			if (abilityOverride.IgnoreDifficultTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreDifficultTerrain = (abilityOverride.OriginalAbility as CAbilityMove).IgnoreDifficultTerrain;
			}
			if (abilityOverride.IgnoreHazardousTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreHazardousTerrain = (abilityOverride.OriginalAbility as CAbilityMove).IgnoreHazardousTerrain;
			}
			if (abilityOverride.CarryOtherActorsOnHex.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).CarryOtherActorsOnHex = (abilityOverride.OriginalAbility as CAbilityMove).CarryOtherActorsOnHex;
				(this as CAbilityMove).CarryOtherActorsOnHexUpdated();
			}
			if (abilityOverride.MoveRestrictionType.HasValue && this is CAbilityMove cAbilityMove)
			{
				cAbilityMove.MoveRestrictionType = (abilityOverride.OriginalAbility as CAbilityMove).MoveRestrictionType;
			}
			if (abilityOverride.ActiveBonusData != null)
			{
				ActiveBonusData = abilityOverride.OriginalAbility.ActiveBonusData;
			}
			if (abilityOverride.ActiveBonusYML != null)
			{
				ActiveBonusYML = abilityOverride.OriginalAbility.ActiveBonusYML;
			}
			if (abilityOverride.AreaEffectYMLString != null)
			{
				AreaEffectYMLString = abilityOverride.OriginalAbility.AreaEffectYMLString;
			}
			if (abilityOverride.AreaEffectLayoutOverrideYMLString != null)
			{
				AreaEffectLayoutOverrideYMLString = abilityOverride.OriginalAbility.AreaEffectLayoutOverrideYMLString;
			}
			if (abilityOverride.MultiPassAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).MultiPassAttack = (abilityOverride.OriginalAbility as CAbilityAttack).MultiPassAttack;
			}
			if (abilityOverride.DamageSelfBeforeAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).DamageSelfBeforeAttack = (abilityOverride.OriginalAbility as CAbilityAttack).DamageSelfBeforeAttack;
			}
			if (abilityOverride.AddAttackBaseStat.HasValue)
			{
				AddAttackBaseStat = abilityOverride.OriginalAbility.AddAttackBaseStat;
			}
			if (abilityOverride.StrengthIsBase.HasValue)
			{
				StrengthIsBase = abilityOverride.OriginalAbility.StrengthIsBase;
			}
			if (abilityOverride.RangeIsBase.HasValue)
			{
				RangeIsBase = abilityOverride.OriginalAbility.RangeIsBase;
			}
			if (abilityOverride.TargetIsBase.HasValue)
			{
				TargetIsBase = abilityOverride.OriginalAbility.TargetIsBase;
			}
			if (abilityOverride.AbilityText != null)
			{
				AbilityText = abilityOverride.OriginalAbility.AbilityText;
			}
			if (abilityOverride.AbilityTextOnly.HasValue)
			{
				AbilityTextOnly = abilityOverride.OriginalAbility.AbilityTextOnly;
			}
			if (abilityOverride.ShowRange.HasValue)
			{
				ShowRange = abilityOverride.OriginalAbility.ShowRange;
			}
			if (abilityOverride.ShowTarget.HasValue)
			{
				ShowTarget = abilityOverride.OriginalAbility.ShowTarget;
			}
			if (abilityOverride.ShowArea.HasValue)
			{
				ShowArea = abilityOverride.OriginalAbility.ShowArea;
			}
			if (abilityOverride.AllTargetsOnMovePath.HasValue)
			{
				AllTargetsOnMovePath = abilityOverride.OriginalAbility.AllTargetsOnMovePath;
			}
			if (abilityOverride.AllTargetsOnMovePathSameStartAndEnd.HasValue)
			{
				AllTargetsOnMovePathSameStartAndEnd = abilityOverride.OriginalAbility.AllTargetsOnMovePathSameStartAndEnd;
			}
			if (abilityOverride.AllTargetsOnAttackPath.HasValue)
			{
				AllTargetsOnAttackPath = abilityOverride.OriginalAbility.AllTargetsOnAttackPath;
			}
			if (abilityOverride.Summons != null && this is CAbilitySummon)
			{
				(this as CAbilitySummon).SummonIDs = (abilityOverride.OriginalAbility as CAbilitySummon).SummonIDs;
			}
			if (abilityOverride.XpPerTargetData != null)
			{
				m_XpPerTargetData = abilityOverride.OriginalAbility.XpPerTargetData;
			}
			if (abilityOverride.Targeting.HasValue)
			{
				Targeting = abilityOverride.OriginalAbility.Targeting;
			}
			if (abilityOverride.PropName != null && this is CAbilityCreate)
			{
				(this as CAbilityCreate).PropName = (abilityOverride.OriginalAbility as CAbilityCreate).PropName;
			}
			if (abilityOverride.TrapData != null && this is CAbilityTrap)
			{
				(this as CAbilityTrap).TrapData = (abilityOverride.OriginalAbility as CAbilityTrap).TrapData;
			}
			if (abilityOverride.IsSubAbility.HasValue)
			{
				m_IsSubAbility = abilityOverride.OriginalAbility.IsSubAbility;
			}
			if (abilityOverride.IsTargetedAbility.HasValue)
			{
				IsTargetedAbility = abilityOverride.OriginalAbility.IsTargetedAbility;
			}
			if (abilityOverride.SpawnDelay.HasValue)
			{
				SpawnDelay = abilityOverride.OriginalAbility.SpawnDelay;
			}
			if (abilityOverride.PullType.HasValue && this is CAbilityPull)
			{
				(this as CAbilityPull).PullType = (abilityOverride.OriginalAbility as CAbilityPull).PullType;
			}
			if (abilityOverride.AdditionalPushEffect.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffect = (abilityOverride.OriginalAbility as CAbilityPush).AdditionalPushEffect;
			}
			if (abilityOverride.AdditionalPushEffectDamage.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectDamage = (abilityOverride.OriginalAbility as CAbilityPush).AdditionalPushEffectDamage;
			}
			if (abilityOverride.AdditionalPushEffectXP.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectXP = (abilityOverride.OriginalAbility as CAbilityPush).AdditionalPushEffectXP;
			}
			if (abilityOverride.AbilityXP.HasValue)
			{
				AbilityXP = abilityOverride.OriginalAbility.AbilityXP;
			}
			if (abilityOverride.IsInlineSubAbility.HasValue)
			{
				IsInlineSubAbility = abilityOverride.OriginalAbility.IsInlineSubAbility;
			}
			if (abilityOverride.SharePreviousAnim.HasValue)
			{
				SkipAnim = abilityOverride.OriginalAbility.SkipAnim;
			}
			if (abilityOverride.IsConsumeAbility.HasValue)
			{
				IsConsumeAbility = abilityOverride.OriginalAbility.IsConsumeAbility;
			}
			if (abilityOverride.Augment != null)
			{
				Augment = abilityOverride.OriginalAbility.Augment;
			}
			if (abilityOverride.Song != null)
			{
				Song = abilityOverride.OriginalAbility.Song;
			}
			if (abilityOverride.Doom != null && this is CAbilityAddDoom cAbilityAddDoom)
			{
				cAbilityAddDoom.Doom = (abilityOverride.OriginalAbility as CAbilityAddDoom).Doom;
			}
			if (abilityOverride.AttackEffects != null && abilityOverride.AttackEffects.Count > 0 && this is CAbilityAttack cAbilityAttack)
			{
				foreach (CAttackEffect attackEffect in abilityOverride.AttackEffects)
				{
					cAbilityAttack.AttackEffects.Remove(attackEffect);
				}
			}
			if (abilityOverride.AddActiveBonusAbility != null && this is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				cAbilityAddActiveBonus.AddAbility = (abilityOverride.OriginalAbility as CAbilityAddActiveBonus).AddAbility;
			}
			if (abilityOverride.ShowElementPicker.HasValue && this is CAbilityInfuse cAbilityInfuse2)
			{
				cAbilityInfuse2.ShowPicker = (abilityOverride.OriginalAbility as CAbilityInfuse).ShowPicker;
			}
			if (abilityOverride.StartAbilityRequirements != null)
			{
				StartAbilityRequirements = abilityOverride.OriginalAbility.StartAbilityRequirements;
			}
			if (abilityOverride.RecoverCardsWithAbilityOfTypeFilter != null)
			{
				if (this is CAbilityRecoverDiscardedCards cAbilityRecoverDiscardedCards)
				{
					cAbilityRecoverDiscardedCards.RecoverCardsWithAbilityOfTypeFilter = (abilityOverride.OriginalAbility as CAbilityRecoverDiscardedCards).RecoverCardsWithAbilityOfTypeFilter;
				}
				else if (this is CAbilityRecoverLostCards cAbilityRecoverLostCards)
				{
					cAbilityRecoverLostCards.RecoverCardsWithAbilityOfTypeFilter = (abilityOverride.OriginalAbility as CAbilityRecoverLostCards).RecoverCardsWithAbilityOfTypeFilter;
				}
			}
			if (abilityOverride.ForgoTopActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion)
			{
				cAbilityForgoActionsForCompanion.ForgoTopActionAbility = (abilityOverride.OriginalAbility as CAbilityForgoActionsForCompanion).ForgoTopActionAbility;
			}
			if (abilityOverride.ForgoBottomActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion2)
			{
				cAbilityForgoActionsForCompanion2.ForgoBottomActionAbility = (abilityOverride.OriginalAbility as CAbilityForgoActionsForCompanion).ForgoBottomActionAbility;
			}
			if (abilityOverride.ExtraTurnType.HasValue && this is CAbilityExtraTurn cAbilityExtraTurn)
			{
				cAbilityExtraTurn.ExtraTurnType = (abilityOverride.OriginalAbility as CAbilityExtraTurn).ExtraTurnType;
			}
			if (abilityOverride.SwapFirstTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap)
			{
				cAbilitySwap.FirstTargetFilter = (abilityOverride.OriginalAbility as CAbilitySwap).FirstTargetFilter;
			}
			if (abilityOverride.SwapSecondTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap2)
			{
				cAbilitySwap2.SecondTargetFilter = (abilityOverride.OriginalAbility as CAbilitySwap).SecondTargetFilter;
			}
			if (abilityOverride.TeleportData != null && this is CAbilityTeleport cAbilityTeleport && abilityOverride.OriginalAbility is CAbilityTeleport cAbilityTeleport2)
			{
				cAbilityTeleport.AbilityTeleportData = cAbilityTeleport2.AbilityTeleportData;
			}
			if (abilityOverride.LootData != null && this is CAbilityLoot cAbilityLoot)
			{
				cAbilityLoot.AbilityLootData = (abilityOverride.OriginalAbility as CAbilityLoot).AbilityLootData;
			}
			if (abilityOverride.ControlActorData != null && this is CAbilityControlActor cAbilityControlActor && abilityOverride.OriginalAbility is CAbilityControlActor cAbilityControlActor2)
			{
				cAbilityControlActor.ControlActorData = cAbilityControlActor2.ControlActorData;
			}
			if (abilityOverride.ChangeAllegianceData != null && this is CAbilityChangeAllegiance cAbilityChangeAllegiance && abilityOverride.OriginalAbility is CAbilityChangeAllegiance cAbilityChangeAllegiance2)
			{
				cAbilityChangeAllegiance.ChangeAllegianceData = cAbilityChangeAllegiance2.ChangeAllegianceData.Copy();
			}
			if (abilityOverride.HealData != null && this is CAbilityHeal cAbilityHeal && abilityOverride.OriginalAbility is CAbilityHeal cAbilityHeal2)
			{
				cAbilityHeal.HealData = cAbilityHeal2.HealData;
			}
			if (abilityOverride.ImmunityToAbilityTypes != null && this is CAbilityImmunityTo cAbilityImmunityTo)
			{
				cAbilityImmunityTo.ImmuneToAbilityTypes = (abilityOverride.OriginalAbility as CAbilityImmunityTo).ImmuneToAbilityTypes.ToList();
			}
			if (abilityOverride.ModifierCardNamesToAdd != null && this is CAbilityAddModifierToMonsterDeck cAbilityAddModifierToMonsterDeck)
			{
				cAbilityAddModifierToMonsterDeck.ModifierCardNamesToAdd = (abilityOverride.OriginalAbility as CAbilityAddModifierToMonsterDeck).ModifierCardNamesToAdd.ToList();
			}
			if (abilityOverride.ResourcesToAddOnAbilityEnd != null)
			{
				ResourcesToAddOnAbilityEnd = abilityOverride.OriginalAbility.ResourcesToAddOnAbilityEnd.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToGiveToTargets != null)
			{
				ResourcesToGiveToTargets = abilityOverride.OriginalAbility.ResourcesToGiveToTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToTakeFromTargets != null)
			{
				ResourcesToTakeFromTargets = abilityOverride.OriginalAbility.ResourcesToTakeFromTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.DestroyObstacleData != null && this is CAbilityDestroyObstacle cAbilityDestroyObstacle && abilityOverride.OriginalAbility is CAbilityDestroyObstacle cAbilityDestroyObstacle2)
			{
				cAbilityDestroyObstacle.AbilityDestroyObstacleData = cAbilityDestroyObstacle2.AbilityDestroyObstacleData;
			}
			if (abilityOverride.HelpBoxLocalizationKey != null && HelpBoxTooltipLocKey == abilityOverride.HelpBoxLocalizationKey)
			{
				HelpBoxTooltipLocKey = abilityOverride.OriginalAbility.HelpBoxTooltipLocKey;
			}
			if (abilityOverride.StatIsBasedOnXEntries != null)
			{
				ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
				StatIsBasedOnXEntries.RemoveAll((AbilityData.StatIsBasedOnXData x) => abilityOverride.StatIsBasedOnXEntries.Contains(x));
				SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, AbilityFilter);
			}
			if (abilityOverride.MiscAbilityData != null && MiscAbilityData != null && abilityOverride.OriginalAbility != null && abilityOverride.OriginalAbility.MiscAbilityData != null)
			{
				MiscAbilityData.TargetOneEnemyWithAllAttacks = abilityOverride.OriginalAbility.MiscAbilityData.TargetOneEnemyWithAllAttacks;
				MiscAbilityData.TreatAsMelee = abilityOverride.OriginalAbility.MiscAbilityData.TreatAsMelee;
				MiscAbilityData.UseParentRangeType = abilityOverride.OriginalAbility.MiscAbilityData.UseParentRangeType;
				MiscAbilityData.AllTargetsAdjacentToParentMovePath = abilityOverride.OriginalAbility.MiscAbilityData.AllTargetsAdjacentToParentMovePath;
				MiscAbilityData.AllTargetsAdjacentToParentTargets = abilityOverride.OriginalAbility.MiscAbilityData.AllTargetsAdjacentToParentTargets;
				MiscAbilityData.UseParentTiles = abilityOverride.OriginalAbility.MiscAbilityData.UseParentTiles;
				MiscAbilityData.IgnoreParentAreaOfEffect = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreParentAreaOfEffect;
				MiscAbilityData.IgnorePreviousAbilityTargets = abilityOverride.OriginalAbility.MiscAbilityData.IgnorePreviousAbilityTargets?.ToList();
				MiscAbilityData.IgnoreMergedAbilityTargetSelection = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreMergedAbilityTargetSelection;
				MiscAbilityData.UseMergedWithAbilityTiles = abilityOverride.OriginalAbility.MiscAbilityData.UseMergedWithAbilityTiles;
				MiscAbilityData.AddTargetPropertyToStrength = abilityOverride.OriginalAbility.MiscAbilityData.AddTargetPropertyToStrength;
				MiscAbilityData.AddTargetPropertyToStrengthMultiplier = abilityOverride.OriginalAbility.MiscAbilityData.AddTargetPropertyToStrengthMultiplier;
				MiscAbilityData.RestrictMoveRange = abilityOverride.OriginalAbility.MiscAbilityData.RestrictMoveRange;
				MiscAbilityData.AutotriggerAbility = abilityOverride.OriginalAbility.MiscAbilityData.AutotriggerAbility;
				MiscAbilityData.HealPercentageOfHealth = abilityOverride.OriginalAbility.MiscAbilityData.HealPercentageOfHealth;
				MiscAbilityData.ExactRange = abilityOverride.OriginalAbility.MiscAbilityData.ExactRange;
				MiscAbilityData.FilterSpecified = abilityOverride.OriginalAbility.MiscAbilityData.FilterSpecified;
				MiscAbilityData.AttackHasAdvantage = abilityOverride.OriginalAbility.MiscAbilityData.AttackHasAdvantage;
				MiscAbilityData.AttackHasDisadvantage = abilityOverride.OriginalAbility.MiscAbilityData.AttackHasDisadvantage;
				MiscAbilityData.ExhaustSelf = abilityOverride.OriginalAbility.MiscAbilityData.ExhaustSelf;
				MiscAbilityData.GainXPPerXDamageDealt = abilityOverride.OriginalAbility.MiscAbilityData.GainXPPerXDamageDealt;
				MiscAbilityData.SetHPTo = abilityOverride.OriginalAbility.MiscAbilityData.SetHPTo;
				MiscAbilityData.PreventOnlyIfLethal = abilityOverride.OriginalAbility.MiscAbilityData.PreventOnlyIfLethal;
				MiscAbilityData.InfuseIfNotStrong = abilityOverride.OriginalAbility.MiscAbilityData.InfuseIfNotStrong;
				MiscAbilityData.CanTargetInvisible = abilityOverride.OriginalAbility.MiscAbilityData.CanTargetInvisible;
				MiscAbilityData.NotApplyEnemy = abilityOverride.OriginalAbility.MiscAbilityData.NotApplyEnemy;
				MiscAbilityData.CanUndo = abilityOverride.OriginalAbility.MiscAbilityData.CanUndo;
				MiscAbilityData.CanSkip = abilityOverride.OriginalAbility.MiscAbilityData.CanSkip;
				MiscAbilityData.ReplaceModifiers = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceModifiers;
				MiscAbilityData.ReplaceWithModifier = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithModifier;
				MiscAbilityData.ReplaceNegativeConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceNegativeConditions;
				MiscAbilityData.ReplacePositiveConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplacePositiveConditions;
				MiscAbilityData.ReplaceWithNegativeConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithNegativeConditions;
				MiscAbilityData.ReplaceWithPositiveConditions = abilityOverride.OriginalAbility.MiscAbilityData.ReplaceWithPositiveConditions;
				MiscAbilityData.GapAbove = abilityOverride.OriginalAbility.MiscAbilityData.GapAbove;
				MiscAbilityData.GapBelow = abilityOverride.OriginalAbility.MiscAbilityData.GapBelow;
				MiscAbilityData.IgnoreStun = abilityOverride.OriginalAbility.MiscAbilityData.IgnoreStun;
				MiscAbilityData.HasCondition = abilityOverride.OriginalAbility.MiscAbilityData.HasCondition;
				MiscAbilityData.RemoveCount = abilityOverride.OriginalAbility.MiscAbilityData.RemoveCount;
				MiscAbilityData.PerformXTimesBasedOn = abilityOverride.OriginalAbility.MiscAbilityData.PerformXTimesBasedOn;
				MiscAbilityData.OnAttackInfuse = abilityOverride.OriginalAbility.MiscAbilityData.OnAttackInfuse;
				MiscAbilityData.AlreadyHasConditionDamageInstead = abilityOverride.OriginalAbility.MiscAbilityData.AlreadyHasConditionDamageInstead;
				MiscAbilityData.UseParentTargets = abilityOverride.OriginalAbility.MiscAbilityData.UseParentTargets;
				MiscAbilityData.AlsoTargetSelf = abilityOverride.OriginalAbility.MiscAbilityData.AlsoTargetSelf;
				MiscAbilityData.AlsoTargetAdjacent = abilityOverride.OriginalAbility.MiscAbilityData.AlsoTargetAdjacent;
				MiscAbilityData.ConditionsToRemoveFirst = abilityOverride.OriginalAbility.MiscAbilityData.ConditionsToRemoveFirst;
				MiscAbilityData.ShowPlusX = abilityOverride.OriginalAbility.MiscAbilityData.ShowPlusX;
				MiscAbilityData.TriggeredScenarioModifiers = abilityOverride.OriginalAbility.MiscAbilityData.TriggeredScenarioModifiers;
				MiscAbilityData.TriggerScenarioModifierOnSelfOnly = abilityOverride.OriginalAbility.MiscAbilityData.TriggerScenarioModifierOnSelfOnly;
				MiscAbilityData.BypassImmunity = abilityOverride.OriginalAbility.MiscAbilityData.BypassImmunity;
				MiscAbilityData.AllowContinueForNullAbility = abilityOverride.OriginalAbility.MiscAbilityData.AllowContinueForNullAbility;
				MiscAbilityData.NoGoldDrop = abilityOverride.OriginalAbility.MiscAbilityData.NoGoldDrop;
				MiscAbilityData.UseOriginalActor = abilityOverride.OriginalAbility.MiscAbilityData.UseOriginalActor;
				MiscAbilityData.ObstacleTypes = abilityOverride.OriginalAbility.MiscAbilityData.ObstacleTypes;
			}
			if (abilityOverride.ConditionDuration.HasValue && this is CAbilityCondition)
			{
				(this as CAbilityCondition).Duration = (abilityOverride.OriginalAbility as CAbilityCondition).Duration;
			}
			if (abilityOverride.ConditionDecTrigger.HasValue)
			{
				(this as CAbilityCondition).DecrementTrigger = (abilityOverride.OriginalAbility as CAbilityCondition).DecrementTrigger;
			}
			if (abilityOverride.ConditionalOverrides != null)
			{
				foreach (CConditionalOverride conditionalOverride in abilityOverride.ConditionalOverrides)
				{
					if (ConditionalOverrides.Contains(conditionalOverride))
					{
						ConditionalOverrides.Remove(conditionalOverride);
					}
				}
			}
			if (abilityOverride.PositiveConditions != null)
			{
				foreach (CCondition.EPositiveCondition positiveCondition in abilityOverride.PositiveConditions)
				{
					if (m_PositiveConditions.Keys.Contains(positiveCondition))
					{
						m_PositiveConditions.Remove(positiveCondition);
					}
				}
			}
			if (abilityOverride.NegativeConditions != null)
			{
				foreach (CCondition.ENegativeCondition negativeCondition in abilityOverride.NegativeConditions)
				{
					if (m_NegativeConditions.Keys.Contains(negativeCondition))
					{
						m_NegativeConditions.Remove(negativeCondition);
					}
				}
			}
			if (TargetingActor == null || !m_AbilityStartComplete || flag)
			{
				return;
			}
			CPhaseAction cPhaseAction = null;
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			}
			if (ShouldRestartAbilityWhenApplyingOverride(abilityOverride) && (cPhaseAction == null || cPhaseAction.CurrentPhaseAbility.m_Ability == this || (cPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility == this)))
			{
				if (CanClearTargets())
				{
					ClearTargets();
				}
				Restart();
			}
			if (perform)
			{
				Perform();
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("An error occurred attempting to undo the ability override.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public virtual void OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null)
	{
		try
		{
			bool flag = false;
			if (m_CurrentOverrides.Contains(abilityOverride))
			{
				return;
			}
			m_CurrentOverrides.Add(abilityOverride);
			if (item != null)
			{
				m_CurrentItemOverrides.Add(abilityOverride, item);
			}
			abilityOverride.OriginalAbility = CopyAbility(this, generateNewID: false, fullCopy: true);
			bool flag2 = false;
			if (abilityOverride.Strength.HasValue)
			{
				m_Strength += abilityOverride.Strength.Value;
				if (TargetingActor != null)
				{
					m_ModifiedStrength += abilityOverride.Strength.Value;
				}
				if (this is CAbilityPush cAbilityPush)
				{
					cAbilityPush.RemainingPushes += abilityOverride.Strength.Value;
				}
				if (this is CAbilityPull cAbilityPull)
				{
					cAbilityPull.RemainingPulls += abilityOverride.Strength.Value;
				}
			}
			if (abilityOverride.Range.HasValue)
			{
				if (abilityOverride.RangeIsBase.HasValue && abilityOverride.RangeIsBase.Value)
				{
					m_Range = abilityOverride.Range.Value;
					m_OriginalRange = abilityOverride.Range.Value;
				}
				else
				{
					m_Range += abilityOverride.Range.Value;
					m_OriginalRange += abilityOverride.Range.Value;
				}
			}
			if (abilityOverride.RangeAtLeastOne.HasValue && m_Range <= 0)
			{
				m_Range = 1;
				m_OriginalRange = 1;
			}
			if (abilityOverride.NumberOfTargets.HasValue)
			{
				if (abilityOverride.TargetIsBase.HasValue && abilityOverride.TargetIsBase.Value)
				{
					m_NumberTargets = abilityOverride.NumberOfTargets.Value;
				}
				else
				{
					m_NumberTargets += abilityOverride.NumberOfTargets.Value;
				}
			}
			if (abilityOverride.TargetAtLeastOne.HasValue && m_NumberTargets <= 0)
			{
				m_NumberTargets = 1;
			}
			if (abilityOverride.RemoveConditionsOverride.HasValue)
			{
				m_PositiveConditions.Clear();
				m_NegativeConditions.Clear();
			}
			if (abilityOverride.Pierce.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).Pierce += abilityOverride.Pierce.Value;
			}
			if (abilityOverride.SubAbilities != null)
			{
				if (SubAbilities != null)
				{
					SubAbilities.AddRange(CopySubAbilities(abilityOverride.SubAbilities));
				}
				else
				{
					SubAbilities = CopySubAbilities(abilityOverride.SubAbilities);
				}
			}
			if (abilityOverride.ChooseAbilities != null && this is CAbilityChoose { ChooseAbilities: not null } cAbilityChoose)
			{
				cAbilityChoose.ChooseAbilities.AddRange(abilityOverride.ChooseAbilities);
			}
			if (abilityOverride.ElementsToInfuse != null && this is CAbilityInfuse)
			{
				CAbilityInfuse cAbilityInfuse = this as CAbilityInfuse;
				if (cAbilityInfuse.ElementsToInfuse == null)
				{
					cAbilityInfuse.ElementsToInfuse = abilityOverride.ElementsToInfuse;
				}
				else
				{
					cAbilityInfuse.ElementsToInfuse.AddRange(abilityOverride.ElementsToInfuse);
				}
			}
			if (abilityOverride.DamageSelfBeforeAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).DamageSelfBeforeAttack += abilityOverride.DamageSelfBeforeAttack.Value;
			}
			if (abilityOverride.AbilityType.HasValue)
			{
				flag = AbilityType != abilityOverride.AbilityType.Value;
				AbilityType = abilityOverride.AbilityType.Value;
			}
			if (abilityOverride.AbilityFilter != null)
			{
				AbilityFilter = abilityOverride.AbilityFilter;
				flag2 = true;
			}
			if (abilityOverride.AnimationOverload != null)
			{
				AnimOverload = abilityOverride.AnimationOverload;
			}
			if (abilityOverride.AreaEffect != null)
			{
				AreaEffect = abilityOverride.AreaEffect;
			}
			if (abilityOverride.AttackSourcesOnly.HasValue && this is CAbilityPreventDamage)
			{
				(this as CAbilityPreventDamage).AttackSourcesOnly = abilityOverride.AttackSourcesOnly.Value;
			}
			if (abilityOverride.Jump.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Jump = abilityOverride.Jump.Value;
			}
			if (abilityOverride.Fly.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).Fly = abilityOverride.Fly.Value;
			}
			if (abilityOverride.IgnoreDifficultTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreDifficultTerrain = abilityOverride.IgnoreDifficultTerrain.Value;
			}
			if (abilityOverride.IgnoreHazardousTerrain.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).IgnoreHazardousTerrain = abilityOverride.IgnoreHazardousTerrain.Value;
			}
			if (abilityOverride.CarryOtherActorsOnHex.HasValue && this is CAbilityMove)
			{
				(this as CAbilityMove).CarryOtherActorsOnHex = abilityOverride.CarryOtherActorsOnHex.Value;
				(this as CAbilityMove).CarryOtherActorsOnHexUpdated();
			}
			if (abilityOverride.MoveRestrictionType.HasValue && this is CAbilityMove cAbilityMove)
			{
				cAbilityMove.MoveRestrictionType = abilityOverride.MoveRestrictionType.Value;
			}
			if (abilityOverride.ActiveBonusData != null)
			{
				ActiveBonusData = abilityOverride.ActiveBonusData;
			}
			if (abilityOverride.ActiveBonusYML != null)
			{
				ActiveBonusYML = abilityOverride.ActiveBonusYML;
			}
			if (abilityOverride.AreaEffectYMLString != null)
			{
				AreaEffectYMLString = abilityOverride.AreaEffectYMLString;
			}
			if (abilityOverride.AreaEffectLayoutOverrideYMLString != null)
			{
				AreaEffectLayoutOverrideYMLString = abilityOverride.AreaEffectLayoutOverrideYMLString;
			}
			if (abilityOverride.MultiPassAttack.HasValue && this is CAbilityAttack)
			{
				(this as CAbilityAttack).MultiPassAttack = abilityOverride.MultiPassAttack.Value;
			}
			if (abilityOverride.AddAttackBaseStat.HasValue)
			{
				AddAttackBaseStat = abilityOverride.AddAttackBaseStat.Value;
			}
			if (abilityOverride.StrengthIsBase.HasValue)
			{
				StrengthIsBase = abilityOverride.StrengthIsBase.Value;
			}
			if (abilityOverride.RangeIsBase.HasValue)
			{
				RangeIsBase = abilityOverride.RangeIsBase.Value;
			}
			if (abilityOverride.TargetIsBase.HasValue)
			{
				TargetIsBase = abilityOverride.TargetIsBase.Value;
			}
			if (abilityOverride.AbilityText != null)
			{
				AbilityText = abilityOverride.AbilityText;
			}
			if (abilityOverride.AbilityTextOnly.HasValue)
			{
				AbilityTextOnly = abilityOverride.AbilityTextOnly.Value;
			}
			if (abilityOverride.ShowRange.HasValue)
			{
				ShowRange = abilityOverride.ShowRange.Value;
			}
			if (abilityOverride.ShowTarget.HasValue)
			{
				ShowTarget = abilityOverride.ShowTarget.Value;
			}
			if (abilityOverride.ShowArea.HasValue)
			{
				ShowArea = abilityOverride.ShowArea.Value;
			}
			if (abilityOverride.AllTargetsOnMovePath.HasValue)
			{
				AllTargetsOnMovePath = abilityOverride.AllTargetsOnMovePath.Value;
			}
			if (abilityOverride.AllTargetsOnMovePathSameStartAndEnd.HasValue)
			{
				AllTargetsOnMovePathSameStartAndEnd = abilityOverride.AllTargetsOnMovePathSameStartAndEnd.Value;
			}
			if (abilityOverride.AllTargetsOnAttackPath.HasValue)
			{
				AllTargetsOnAttackPath = abilityOverride.AllTargetsOnAttackPath.Value;
			}
			if (abilityOverride.Summons != null && this is CAbilitySummon)
			{
				(this as CAbilitySummon).SummonIDs = abilityOverride.Summons;
			}
			if (abilityOverride.XpPerTargetData != null)
			{
				m_XpPerTargetData = abilityOverride.XpPerTargetData;
			}
			if (abilityOverride.Targeting.HasValue)
			{
				Targeting = abilityOverride.Targeting.Value;
			}
			if (abilityOverride.PropName != null && this is CAbilityCreate)
			{
				(this as CAbilityCreate).PropName = abilityOverride.PropName;
			}
			if (abilityOverride.TrapData != null && this is CAbilityTrap)
			{
				(this as CAbilityTrap).TrapData = abilityOverride.TrapData;
			}
			if (abilityOverride.IsSubAbility.HasValue)
			{
				m_IsSubAbility = abilityOverride.IsSubAbility.Value;
			}
			if (abilityOverride.IsTargetedAbility.HasValue)
			{
				IsTargetedAbility = abilityOverride.IsTargetedAbility.Value;
				flag2 = true;
			}
			if (abilityOverride.SpawnDelay.HasValue)
			{
				SpawnDelay = abilityOverride.SpawnDelay.Value;
			}
			if (abilityOverride.AdditionalPushEffect.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffect = abilityOverride.AdditionalPushEffect.Value;
			}
			if (abilityOverride.AdditionalPushEffectDamage.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectDamage = abilityOverride.AdditionalPushEffectDamage.Value;
			}
			if (abilityOverride.AdditionalPushEffectXP.HasValue && this is CAbilityPush)
			{
				(this as CAbilityPush).AdditionalPushEffectXP = abilityOverride.AdditionalPushEffectXP.Value;
			}
			if (abilityOverride.PullType.HasValue && this is CAbilityPull cAbilityPull2)
			{
				cAbilityPull2.PullType = abilityOverride.PullType.Value;
			}
			if (abilityOverride.AbilityXP.HasValue)
			{
				AbilityXP = abilityOverride.AbilityXP.Value;
			}
			if (abilityOverride.IsInlineSubAbility.HasValue)
			{
				IsInlineSubAbility = abilityOverride.IsInlineSubAbility.Value;
			}
			if (abilityOverride.SharePreviousAnim.HasValue)
			{
				SkipAnim = abilityOverride.SharePreviousAnim.Value;
			}
			if (abilityOverride.ConditionDuration.HasValue && this is CAbilityCondition)
			{
				(this as CAbilityCondition).Duration = abilityOverride.ConditionDuration.Value;
			}
			if (abilityOverride.ConditionDecTrigger.HasValue && this is CAbilityCondition)
			{
				(this as CAbilityCondition).DecrementTrigger = abilityOverride.ConditionDecTrigger.Value;
			}
			if (abilityOverride.ConditionalOverrides != null && abilityOverride.ConditionalOverrides.Count > 0)
			{
				ConditionalOverrides.AddRange(abilityOverride.ConditionalOverrides);
			}
			if (abilityOverride.StartAbilityRequirements != null)
			{
				StartAbilityRequirements = abilityOverride.StartAbilityRequirements;
			}
			if (abilityOverride.IsConsumeAbility.HasValue)
			{
				IsConsumeAbility = abilityOverride.IsConsumeAbility.Value;
			}
			if (abilityOverride.Augment != null)
			{
				Augment = abilityOverride.Augment;
			}
			if (abilityOverride.Song != null)
			{
				Song = abilityOverride.Song;
			}
			if (abilityOverride.Doom != null && this is CAbilityAddDoom cAbilityAddDoom)
			{
				cAbilityAddDoom.Doom = abilityOverride.Doom;
			}
			if (abilityOverride.AttackEffects != null && abilityOverride.AttackEffects.Count > 0 && this is CAbilityAttack cAbilityAttack)
			{
				cAbilityAttack.AttackEffects.AddRange(abilityOverride.AttackEffects);
			}
			if (abilityOverride.ControlActorData != null && this is CAbilityControlActor cAbilityControlActor)
			{
				cAbilityControlActor.ControlActorData = cAbilityControlActor.ControlActorData.Merge(abilityOverride.ControlActorData);
			}
			if (abilityOverride.ChangeAllegianceData != null && this is CAbilityChangeAllegiance cAbilityChangeAllegiance)
			{
				cAbilityChangeAllegiance.ChangeAllegianceData = abilityOverride.ChangeAllegianceData.Copy();
			}
			if (abilityOverride.AddActiveBonusAbility != null && this is CAbilityAddActiveBonus cAbilityAddActiveBonus)
			{
				cAbilityAddActiveBonus.AddAbility = abilityOverride.AddActiveBonusAbility;
			}
			if (abilityOverride.ShowElementPicker.HasValue && this is CAbilityInfuse cAbilityInfuse2)
			{
				cAbilityInfuse2.ShowPicker = abilityOverride.ShowElementPicker.Value;
			}
			if (abilityOverride.RecoverCardsWithAbilityOfTypeFilter != null)
			{
				if (this is CAbilityRecoverDiscardedCards cAbilityRecoverDiscardedCards)
				{
					cAbilityRecoverDiscardedCards.RecoverCardsWithAbilityOfTypeFilter = abilityOverride.RecoverCardsWithAbilityOfTypeFilter;
				}
				else if (this is CAbilityRecoverLostCards cAbilityRecoverLostCards)
				{
					cAbilityRecoverLostCards.RecoverCardsWithAbilityOfTypeFilter = abilityOverride.RecoverCardsWithAbilityOfTypeFilter;
				}
			}
			if (abilityOverride.ForgoTopActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion)
			{
				cAbilityForgoActionsForCompanion.ForgoTopActionAbility = abilityOverride.ForgoTopActionAbility;
			}
			if (abilityOverride.ForgoBottomActionAbility != null && this is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion2)
			{
				cAbilityForgoActionsForCompanion2.ForgoBottomActionAbility = abilityOverride.ForgoBottomActionAbility;
			}
			if (abilityOverride.ExtraTurnType.HasValue && this is CAbilityExtraTurn cAbilityExtraTurn)
			{
				cAbilityExtraTurn.ExtraTurnType = abilityOverride.ExtraTurnType.Value;
			}
			if (abilityOverride.SwapFirstTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap)
			{
				cAbilitySwap.FirstTargetFilter = abilityOverride.SwapFirstTargetAbilityFilter;
			}
			if (abilityOverride.SwapSecondTargetAbilityFilter != null && this is CAbilitySwap cAbilitySwap2)
			{
				cAbilitySwap2.SecondTargetFilter = abilityOverride.SwapSecondTargetAbilityFilter;
			}
			if (abilityOverride.TeleportData != null && this is CAbilityTeleport cAbilityTeleport)
			{
				cAbilityTeleport.AbilityTeleportData = abilityOverride.TeleportData;
			}
			if (abilityOverride.LootData != null && this is CAbilityLoot cAbilityLoot)
			{
				cAbilityLoot.AbilityLootData = abilityOverride.LootData;
			}
			if (abilityOverride.HealData != null && this is CAbilityHeal cAbilityHeal)
			{
				cAbilityHeal.HealData = abilityOverride.HealData;
			}
			if (abilityOverride.ImmunityToAbilityTypes != null && this is CAbilityImmunityTo cAbilityImmunityTo)
			{
				cAbilityImmunityTo.ImmuneToAbilityTypes = abilityOverride.ImmunityToAbilityTypes.ToList();
			}
			if (abilityOverride.ModifierCardNamesToAdd != null && this is CAbilityAddModifierToMonsterDeck cAbilityAddModifierToMonsterDeck)
			{
				cAbilityAddModifierToMonsterDeck.ModifierCardNamesToAdd = abilityOverride.ModifierCardNamesToAdd.ToList();
			}
			if (abilityOverride.ResourcesToAddOnAbilityEnd != null)
			{
				ResourcesToAddOnAbilityEnd = abilityOverride.ResourcesToAddOnAbilityEnd.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToGiveToTargets != null)
			{
				ResourcesToGiveToTargets = abilityOverride.ResourcesToGiveToTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.ResourcesToTakeFromTargets != null)
			{
				ResourcesToTakeFromTargets = abilityOverride.ResourcesToTakeFromTargets.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			if (abilityOverride.DestroyObstacleData != null && this is CAbilityDestroyObstacle cAbilityDestroyObstacle)
			{
				cAbilityDestroyObstacle.AbilityDestroyObstacleData = abilityOverride.DestroyObstacleData;
			}
			if (abilityOverride.HelpBoxLocalizationKey != null)
			{
				HelpBoxTooltipLocKey = abilityOverride.HelpBoxLocalizationKey;
			}
			if (abilityOverride.StatIsBasedOnXEntries != null)
			{
				ResetStatBasedOnXAddedStats(StatIsBasedOnXEntries);
				foreach (AbilityData.StatIsBasedOnXData statIsBasedOnXEntry in abilityOverride.StatIsBasedOnXEntries)
				{
					StatIsBasedOnXEntries.Add(statIsBasedOnXEntry.Copy());
				}
				SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries, (filter != null) ? filter : AbilityFilter);
			}
			if (abilityOverride.MiscAbilityData != null)
			{
				if (MiscAbilityData == null)
				{
					MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				MiscAbilityData.TargetOneEnemyWithAllAttacks = abilityOverride.MiscAbilityData.TargetOneEnemyWithAllAttacks ?? MiscAbilityData.TargetOneEnemyWithAllAttacks;
				MiscAbilityData.TreatAsMelee = abilityOverride.MiscAbilityData.TreatAsMelee ?? MiscAbilityData.TreatAsMelee;
				MiscAbilityData.UseParentRangeType = abilityOverride.MiscAbilityData.UseParentRangeType ?? MiscAbilityData.UseParentRangeType;
				MiscAbilityData.AllTargetsAdjacentToParentMovePath = abilityOverride.MiscAbilityData.AllTargetsAdjacentToParentMovePath ?? MiscAbilityData.AllTargetsAdjacentToParentMovePath;
				MiscAbilityData.AllTargetsAdjacentToParentTargets = abilityOverride.MiscAbilityData.AllTargetsAdjacentToParentTargets ?? MiscAbilityData.AllTargetsAdjacentToParentTargets;
				MiscAbilityData.UseParentTiles = abilityOverride.MiscAbilityData.UseParentTiles ?? MiscAbilityData.UseParentTiles;
				MiscAbilityData.IgnoreParentAreaOfEffect = abilityOverride.MiscAbilityData.IgnoreParentAreaOfEffect ?? MiscAbilityData.IgnoreParentAreaOfEffect;
				MiscAbilityData.IgnorePreviousAbilityTargets = abilityOverride.MiscAbilityData.IgnorePreviousAbilityTargets ?? MiscAbilityData.IgnorePreviousAbilityTargets?.ToList();
				MiscAbilityData.IgnoreMergedAbilityTargetSelection = abilityOverride.MiscAbilityData.IgnoreMergedAbilityTargetSelection ?? MiscAbilityData.IgnoreMergedAbilityTargetSelection;
				MiscAbilityData.UseMergedWithAbilityTiles = abilityOverride.MiscAbilityData.UseMergedWithAbilityTiles ?? MiscAbilityData.UseMergedWithAbilityTiles;
				MiscAbilityData.AddTargetPropertyToStrength = abilityOverride.MiscAbilityData.AddTargetPropertyToStrength ?? MiscAbilityData.AddTargetPropertyToStrength;
				MiscAbilityData.AddTargetPropertyToStrengthMultiplier = abilityOverride.MiscAbilityData.AddTargetPropertyToStrengthMultiplier ?? MiscAbilityData.AddTargetPropertyToStrengthMultiplier;
				MiscAbilityData.RestrictMoveRange = abilityOverride.MiscAbilityData.RestrictMoveRange ?? MiscAbilityData.RestrictMoveRange;
				MiscAbilityData.AutotriggerAbility = abilityOverride.MiscAbilityData.AutotriggerAbility ?? MiscAbilityData.AutotriggerAbility;
				MiscAbilityData.HealPercentageOfHealth = abilityOverride.MiscAbilityData.HealPercentageOfHealth ?? MiscAbilityData.HealPercentageOfHealth;
				MiscAbilityData.ExactRange = abilityOverride.MiscAbilityData.ExactRange ?? MiscAbilityData.ExactRange;
				MiscAbilityData.FilterSpecified = abilityOverride.MiscAbilityData.FilterSpecified ?? MiscAbilityData.FilterSpecified;
				MiscAbilityData.AttackHasAdvantage = abilityOverride.MiscAbilityData.AttackHasAdvantage ?? MiscAbilityData.AttackHasAdvantage;
				MiscAbilityData.AttackHasDisadvantage = abilityOverride.MiscAbilityData.AttackHasDisadvantage ?? MiscAbilityData.AttackHasDisadvantage;
				MiscAbilityData.ExhaustSelf = abilityOverride.MiscAbilityData.ExhaustSelf ?? MiscAbilityData.ExhaustSelf;
				MiscAbilityData.GainXPPerXDamageDealt = abilityOverride.MiscAbilityData.GainXPPerXDamageDealt ?? MiscAbilityData.GainXPPerXDamageDealt;
				MiscAbilityData.SetHPTo = abilityOverride.MiscAbilityData.SetHPTo ?? MiscAbilityData.SetHPTo;
				MiscAbilityData.PreventOnlyIfLethal = abilityOverride.MiscAbilityData.PreventOnlyIfLethal ?? MiscAbilityData.PreventOnlyIfLethal;
				MiscAbilityData.InfuseIfNotStrong = abilityOverride.MiscAbilityData.InfuseIfNotStrong ?? MiscAbilityData.InfuseIfNotStrong;
				MiscAbilityData.CanTargetInvisible = abilityOverride.MiscAbilityData.CanTargetInvisible ?? MiscAbilityData.CanTargetInvisible;
				MiscAbilityData.NotApplyEnemy = abilityOverride.MiscAbilityData.NotApplyEnemy ?? MiscAbilityData.NotApplyEnemy;
				MiscAbilityData.CanUndo = abilityOverride.MiscAbilityData.CanUndo ?? MiscAbilityData.CanUndo;
				MiscAbilityData.CanSkip = abilityOverride.MiscAbilityData.CanSkip ?? MiscAbilityData.CanSkip;
				MiscAbilityData.ReplaceModifiers = abilityOverride.MiscAbilityData.ReplaceModifiers ?? MiscAbilityData.ReplaceModifiers;
				MiscAbilityData.ReplaceWithModifier = abilityOverride.MiscAbilityData.ReplaceWithModifier ?? MiscAbilityData.ReplaceWithModifier;
				MiscAbilityData.ReplaceNegativeConditions = abilityOverride.MiscAbilityData.ReplaceNegativeConditions ?? MiscAbilityData.ReplaceNegativeConditions;
				MiscAbilityData.ReplacePositiveConditions = abilityOverride.MiscAbilityData.ReplacePositiveConditions ?? MiscAbilityData.ReplacePositiveConditions;
				MiscAbilityData.ReplaceWithNegativeConditions = abilityOverride.MiscAbilityData.ReplaceWithNegativeConditions ?? MiscAbilityData.ReplaceWithNegativeConditions;
				MiscAbilityData.ReplaceWithPositiveConditions = abilityOverride.MiscAbilityData.ReplaceWithPositiveConditions ?? MiscAbilityData.ReplaceWithPositiveConditions;
				MiscAbilityData.GapAbove = abilityOverride.MiscAbilityData.GapAbove ?? MiscAbilityData.GapAbove;
				MiscAbilityData.GapBelow = abilityOverride.MiscAbilityData.GapBelow ?? MiscAbilityData.GapBelow;
				MiscAbilityData.IgnoreStun = abilityOverride.MiscAbilityData.IgnoreStun ?? MiscAbilityData.IgnoreStun;
				MiscAbilityData.HasCondition = abilityOverride.MiscAbilityData.HasCondition ?? MiscAbilityData.HasCondition;
				MiscAbilityData.RemoveCount = abilityOverride.MiscAbilityData.RemoveCount ?? MiscAbilityData.RemoveCount;
				MiscAbilityData.PerformXTimesBasedOn = abilityOverride.MiscAbilityData.PerformXTimesBasedOn ?? MiscAbilityData.PerformXTimesBasedOn;
				MiscAbilityData.OnAttackInfuse = abilityOverride.MiscAbilityData.OnAttackInfuse ?? MiscAbilityData.OnAttackInfuse;
				MiscAbilityData.AlreadyHasConditionDamageInstead = abilityOverride.MiscAbilityData.AlreadyHasConditionDamageInstead ?? MiscAbilityData.AlreadyHasConditionDamageInstead;
				MiscAbilityData.UseParentTargets = abilityOverride.MiscAbilityData.UseParentTargets ?? MiscAbilityData.UseParentTargets;
				MiscAbilityData.AlsoTargetSelf = abilityOverride.MiscAbilityData.AlsoTargetSelf ?? MiscAbilityData.AlsoTargetSelf;
				MiscAbilityData.AlsoTargetAdjacent = abilityOverride.MiscAbilityData.AlsoTargetAdjacent ?? MiscAbilityData.AlsoTargetAdjacent;
				MiscAbilityData.ConditionsToRemoveFirst = abilityOverride.MiscAbilityData.ConditionsToRemoveFirst ?? MiscAbilityData.ConditionsToRemoveFirst;
				MiscAbilityData.ShowPlusX = abilityOverride.MiscAbilityData.ShowPlusX ?? MiscAbilityData.ShowPlusX;
				MiscAbilityData.TriggeredScenarioModifiers = abilityOverride.MiscAbilityData.TriggeredScenarioModifiers ?? MiscAbilityData.TriggeredScenarioModifiers;
				MiscAbilityData.TriggerScenarioModifierOnSelfOnly = abilityOverride.MiscAbilityData.TriggerScenarioModifierOnSelfOnly ?? MiscAbilityData.TriggerScenarioModifierOnSelfOnly;
				MiscAbilityData.BypassImmunity = abilityOverride.MiscAbilityData.BypassImmunity ?? MiscAbilityData.BypassImmunity;
				MiscAbilityData.AllowContinueForNullAbility = abilityOverride.MiscAbilityData.AllowContinueForNullAbility ?? MiscAbilityData.AllowContinueForNullAbility;
				MiscAbilityData.NoGoldDrop = abilityOverride.MiscAbilityData.NoGoldDrop ?? MiscAbilityData.NoGoldDrop;
				MiscAbilityData.UseOriginalActor = abilityOverride.MiscAbilityData.UseOriginalActor ?? MiscAbilityData.UseOriginalActor;
				MiscAbilityData.ObstacleTypes = abilityOverride.MiscAbilityData.ObstacleTypes ?? MiscAbilityData.ObstacleTypes;
			}
			if (abilityOverride.PositiveConditions != null)
			{
				if (m_PositiveConditions == null)
				{
					m_PositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>();
				}
				else if (flag2)
				{
					Dictionary<CCondition.EPositiveCondition, CAbility> dictionary = new Dictionary<CCondition.EPositiveCondition, CAbility>();
					foreach (CCondition.EPositiveCondition posCondition in m_PositiveConditions.Keys)
					{
						dictionary.Add(posCondition, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == posCondition.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
					m_PositiveConditions = dictionary;
				}
				foreach (CCondition.EPositiveCondition posCondition2 in abilityOverride.PositiveConditions)
				{
					try
					{
						if (!m_PositiveConditions.ContainsKey(posCondition2))
						{
							m_PositiveConditions.Add(posCondition2, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == posCondition2.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility));
						}
					}
					catch
					{
						DLLDebug.LogError("Condition " + posCondition2.ToString() + " could not be found in EAbilityType enum.");
					}
				}
			}
			if (abilityOverride.NegativeConditions != null)
			{
				if (m_NegativeConditions == null)
				{
					m_NegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>();
				}
				else if (flag2)
				{
					Dictionary<CCondition.ENegativeCondition, CAbility> dictionary2 = new Dictionary<CCondition.ENegativeCondition, CAbility>();
					foreach (CCondition.ENegativeCondition negCondition in m_NegativeConditions.Keys)
					{
						dictionary2.Add(negCondition, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == negCondition.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility));
					}
					m_NegativeConditions = dictionary2;
				}
				foreach (CCondition.ENegativeCondition negCondition2 in abilityOverride.NegativeConditions)
				{
					try
					{
						if (!m_NegativeConditions.ContainsKey(negCondition2))
						{
							m_NegativeConditions.Add(negCondition2, CreateAbility(AbilityTypes.Single((EAbilityType x) => x.ToString() == negCondition2.ToString()), AbilityFilter, IsMonsterAbility, IsTargetedAbility));
						}
					}
					catch
					{
						DLLDebug.LogError("Error: Condition " + negCondition2.ToString() + " could not be found in EAbilityType enum.");
					}
				}
			}
			if (TargetingActor == null || !m_AbilityStartComplete || flag)
			{
				return;
			}
			CPhaseAction cPhaseAction = null;
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			}
			if (ShouldRestartAbilityWhenApplyingOverride(abilityOverride) && (cPhaseAction == null || cPhaseAction.CurrentPhaseAbility.m_Ability == this || (cPhaseAction.CurrentPhaseAbility.m_Ability is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility == this)))
			{
				if (CanClearTargets())
				{
					ClearTargets();
				}
				Restart();
			}
			if (perform)
			{
				Perform();
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("An error occurred attempting to perform the ability override.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static List<EEnhancement> GetValidEnhancements(EAbilityType abilityType, Dictionary<CCondition.EPositiveCondition, CAbility> positiveConditions, Dictionary<CCondition.ENegativeCondition, CAbility> negativeConditions, EEnhancementLine line)
	{
		List<EEnhancement> list = new List<EEnhancement>();
		switch (line)
		{
		case EEnhancementLine.Targets:
			list.Add(EEnhancement.PlusTarget);
			return list;
		case EEnhancementLine.Range:
			list.Add(EEnhancement.PlusRange);
			return list;
		case EEnhancementLine.Pierce:
			list.Add(EEnhancement.PlusPierce);
			return list;
		case EEnhancementLine.SummonAttack:
			list.Add(EEnhancement.SummonAttack);
			return list;
		case EEnhancementLine.SummonHealth:
			list.Add(EEnhancement.SummonHP);
			return list;
		case EEnhancementLine.SummonMove:
			list.Add(EEnhancement.SummonMove);
			return list;
		case EEnhancementLine.SummonRange:
			list.Add(EEnhancement.SummonRange);
			return list;
		case EEnhancementLine.RetaliateRange:
			list.Add(EEnhancement.PlusRetaliateRange);
			break;
		case EEnhancementLine.AreaHex:
			list.Add(EEnhancement.Area);
			return list;
		case EEnhancementLine.Push:
			list.Add(EEnhancement.PlusPush);
			return list;
		case EEnhancementLine.Pull:
			list.Add(EEnhancement.PlusPull);
			return list;
		case EEnhancementLine.Mainline:
			switch (abilityType)
			{
			case EAbilityType.Attack:
				list.Add(EEnhancement.PlusAttack);
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Move:
				break;
			case EAbilityType.Heal:
				list.Add(EEnhancement.PlusHeal);
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Retaliate:
				list.Add(EEnhancement.PlusRetaliate);
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Shield:
				list.Add(EEnhancement.PlusShield);
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Poison:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Wound:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Muddle:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Immobilize:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Disarm:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Stun:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Curse:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Strengthen:
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Invisible:
			case EAbilityType.Bless:
				if (!positiveConditions.ContainsKey(CCondition.EPositiveCondition.Strengthen))
				{
					list.Add(EEnhancement.Strengthen);
				}
				list.Add(EEnhancement.Bless);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Push:
				list.Add(EEnhancement.PlusPush);
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Pull:
				list.Add(EEnhancement.PlusPull);
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			case EAbilityType.Sleep:
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Poison))
				{
					list.Add(EEnhancement.Poison);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Wound))
				{
					list.Add(EEnhancement.Wound);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Muddle))
				{
					list.Add(EEnhancement.Muddle);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Immobilize))
				{
					list.Add(EEnhancement.Immobilize);
				}
				if (!negativeConditions.ContainsKey(CCondition.ENegativeCondition.Disarm))
				{
					list.Add(EEnhancement.Disarm);
				}
				list.Add(EEnhancement.Curse);
				list.Add(EEnhancement.AnyElement);
				list.Add(EEnhancement.Air);
				list.Add(EEnhancement.Dark);
				list.Add(EEnhancement.Earth);
				list.Add(EEnhancement.Fire);
				list.Add(EEnhancement.Ice);
				list.Add(EEnhancement.Light);
				return list;
			default:
				DLLDebug.LogError("Unsupported Ability Type for enhancemnets");
				return list;
			}
			list.Add(EEnhancement.PlusMove);
			list.Add(EEnhancement.Jump);
			list.Add(EEnhancement.AnyElement);
			list.Add(EEnhancement.Air);
			list.Add(EEnhancement.Dark);
			list.Add(EEnhancement.Earth);
			list.Add(EEnhancement.Fire);
			list.Add(EEnhancement.Ice);
			list.Add(EEnhancement.Light);
			break;
		default:
			DLLDebug.LogError("Unsupported Enhancement Line for enhancemnets");
			return list;
		}
		return list;
	}

	public virtual bool Validate()
	{
		return true;
	}

	public virtual void ShowToggleBonusesForAI()
	{
		CShowToggleBonusesForAI_MessageData cShowToggleBonusesForAI_MessageData = new CShowToggleBonusesForAI_MessageData(TargetingActor);
		cShowToggleBonusesForAI_MessageData.m_Ability = this;
		ScenarioRuleClient.MessageHandler(cShowToggleBonusesForAI_MessageData);
	}

	public virtual bool IsValidAbilityForActiveBonusOfType(EAbilityType abilityType)
	{
		if (AbilityType == abilityType)
		{
			if (ActiveBonusData != null)
			{
				return ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
			}
			return true;
		}
		return false;
	}

	public virtual bool IsPositive()
	{
		return false;
	}

	public CAbility()
	{
	}

	public CAbility(CAbility state, ReferenceDictionary references)
	{
		ID = state.ID;
		Name = state.Name;
		AbilityType = state.AbilityType;
		AnimOverload = state.AnimOverload;
		AbilityFilter = references.Get(state.AbilityFilter);
		if (AbilityFilter == null && state.AbilityFilter != null)
		{
			AbilityFilter = new CAbilityFilterContainer(state.AbilityFilter, references);
			references.Add(state.AbilityFilter, AbilityFilter);
		}
		TileFilter = state.TileFilter;
		Targeting = state.Targeting;
		AllTargetsOnMovePath = state.AllTargetsOnMovePath;
		AllTargetsOnMovePathSameStartAndEnd = state.AllTargetsOnMovePathSameStartAndEnd;
		AllTargetsOnAttackPath = state.AllTargetsOnAttackPath;
		IsTargetedAbility = state.IsTargetedAbility;
		SpawnDelay = state.SpawnDelay;
		ConditionalOverrides = references.Get(state.ConditionalOverrides);
		if (ConditionalOverrides == null && state.ConditionalOverrides != null)
		{
			ConditionalOverrides = new List<CConditionalOverride>();
			for (int i = 0; i < state.ConditionalOverrides.Count; i++)
			{
				CConditionalOverride cConditionalOverride = state.ConditionalOverrides[i];
				CConditionalOverride cConditionalOverride2 = references.Get(cConditionalOverride);
				if (cConditionalOverride2 == null && cConditionalOverride != null)
				{
					cConditionalOverride2 = new CConditionalOverride(cConditionalOverride, references);
					references.Add(cConditionalOverride, cConditionalOverride2);
				}
				ConditionalOverrides.Add(cConditionalOverride2);
			}
			references.Add(state.ConditionalOverrides, ConditionalOverrides);
		}
		ActiveConditionalOverrides = references.Get(state.ActiveConditionalOverrides);
		if (ActiveConditionalOverrides == null && state.ActiveConditionalOverrides != null)
		{
			ActiveConditionalOverrides = new List<CConditionalOverride>();
			for (int j = 0; j < state.ActiveConditionalOverrides.Count; j++)
			{
				CConditionalOverride cConditionalOverride3 = state.ActiveConditionalOverrides[j];
				CConditionalOverride cConditionalOverride4 = references.Get(cConditionalOverride3);
				if (cConditionalOverride4 == null && cConditionalOverride3 != null)
				{
					cConditionalOverride4 = new CConditionalOverride(cConditionalOverride3, references);
					references.Add(cConditionalOverride3, cConditionalOverride4);
				}
				ActiveConditionalOverrides.Add(cConditionalOverride4);
			}
			references.Add(state.ActiveConditionalOverrides, ActiveConditionalOverrides);
		}
		AbilityXP = state.AbilityXP;
		SkipAnim = state.SkipAnim;
		IsItemAbility = state.IsItemAbility;
		UseSpecialBaseStat = state.UseSpecialBaseStat;
		PreviewEffectId = state.PreviewEffectId;
		PreviewEffectText = state.PreviewEffectText;
		HelpBoxTooltipLocKey = state.HelpBoxTooltipLocKey;
		StatIsBasedOnXEntries = references.Get(state.StatIsBasedOnXEntries);
		if (StatIsBasedOnXEntries == null && state.StatIsBasedOnXEntries != null)
		{
			StatIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>();
			for (int k = 0; k < state.StatIsBasedOnXEntries.Count; k++)
			{
				AbilityData.StatIsBasedOnXData statIsBasedOnXData = state.StatIsBasedOnXEntries[k];
				AbilityData.StatIsBasedOnXData statIsBasedOnXData2 = references.Get(statIsBasedOnXData);
				if (statIsBasedOnXData2 == null && statIsBasedOnXData != null)
				{
					statIsBasedOnXData2 = new AbilityData.StatIsBasedOnXData(statIsBasedOnXData, references);
					references.Add(statIsBasedOnXData, statIsBasedOnXData2);
				}
				StatIsBasedOnXEntries.Add(statIsBasedOnXData2);
			}
			references.Add(state.StatIsBasedOnXEntries, StatIsBasedOnXEntries);
		}
		AreaEffectYMLString = state.AreaEffectYMLString;
		AreaEffectLayoutOverrideYMLString = state.AreaEffectLayoutOverrideYMLString;
		EnhancementsApplied = state.EnhancementsApplied;
		ResourcesToAddOnAbilityEnd = references.Get(state.ResourcesToAddOnAbilityEnd);
		if (ResourcesToAddOnAbilityEnd == null && state.ResourcesToAddOnAbilityEnd != null)
		{
			ResourcesToAddOnAbilityEnd = new Dictionary<string, int>(state.ResourcesToAddOnAbilityEnd.Comparer);
			foreach (KeyValuePair<string, int> item2 in state.ResourcesToAddOnAbilityEnd)
			{
				string key = item2.Key;
				int value = item2.Value;
				ResourcesToAddOnAbilityEnd.Add(key, value);
			}
			references.Add(state.ResourcesToAddOnAbilityEnd, ResourcesToAddOnAbilityEnd);
		}
		ResourcesToTakeFromTargets = references.Get(state.ResourcesToTakeFromTargets);
		if (ResourcesToTakeFromTargets == null && state.ResourcesToTakeFromTargets != null)
		{
			ResourcesToTakeFromTargets = new Dictionary<string, int>(state.ResourcesToTakeFromTargets.Comparer);
			foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in state.ResourcesToTakeFromTargets)
			{
				string key2 = resourcesToTakeFromTarget.Key;
				int value2 = resourcesToTakeFromTarget.Value;
				ResourcesToTakeFromTargets.Add(key2, value2);
			}
			references.Add(state.ResourcesToTakeFromTargets, ResourcesToTakeFromTargets);
		}
		ResourcesToGiveToTargets = references.Get(state.ResourcesToGiveToTargets);
		if (ResourcesToGiveToTargets == null && state.ResourcesToGiveToTargets != null)
		{
			ResourcesToGiveToTargets = new Dictionary<string, int>(state.ResourcesToGiveToTargets.Comparer);
			foreach (KeyValuePair<string, int> resourcesToGiveToTarget in state.ResourcesToGiveToTargets)
			{
				string key3 = resourcesToGiveToTarget.Key;
				int value3 = resourcesToGiveToTarget.Value;
				ResourcesToGiveToTargets.Add(key3, value3);
			}
			references.Add(state.ResourcesToGiveToTargets, ResourcesToGiveToTargets);
		}
		IsConsumeAbility = state.IsConsumeAbility;
		ParentName = state.ParentName;
		IsInlineSubAbility = state.IsInlineSubAbility;
		SubAbilities = references.Get(state.SubAbilities);
		if (SubAbilities == null && state.SubAbilities != null)
		{
			SubAbilities = new List<CAbility>();
			for (int l = 0; l < state.SubAbilities.Count; l++)
			{
				CAbility cAbility = state.SubAbilities[l];
				CAbility cAbility2 = references.Get(cAbility);
				if (cAbility2 == null && cAbility != null)
				{
					CAbility cAbility3 = ((cAbility is CAbilityBlockHealing state2) ? new CAbilityBlockHealing(state2, references) : ((cAbility is CAbilityNeutralizeShield state3) ? new CAbilityNeutralizeShield(state3, references) : ((cAbility is CAbilityAdvantage state4) ? new CAbilityAdvantage(state4, references) : ((cAbility is CAbilityBless state5) ? new CAbilityBless(state5, references) : ((cAbility is CAbilityCurse state6) ? new CAbilityCurse(state6, references) : ((cAbility is CAbilityDisarm state7) ? new CAbilityDisarm(state7, references) : ((cAbility is CAbilityImmobilize state8) ? new CAbilityImmobilize(state8, references) : ((cAbility is CAbilityImmovable state9) ? new CAbilityImmovable(state9, references) : ((cAbility is CAbilityInvisible state10) ? new CAbilityInvisible(state10, references) : ((cAbility is CAbilityMuddle state11) ? new CAbilityMuddle(state11, references) : ((cAbility is CAbilityOverheal state12) ? new CAbilityOverheal(state12, references) : ((cAbility is CAbilityPoison state13) ? new CAbilityPoison(state13, references) : ((cAbility is CAbilitySleep state14) ? new CAbilitySleep(state14, references) : ((cAbility is CAbilityStopFlying state15) ? new CAbilityStopFlying(state15, references) : ((cAbility is CAbilityStrengthen state16) ? new CAbilityStrengthen(state16, references) : ((cAbility is CAbilityStun state17) ? new CAbilityStun(state17, references) : ((cAbility is CAbilityWound state18) ? new CAbilityWound(state18, references) : ((cAbility is CAbilityChooseAbility state19) ? new CAbilityChooseAbility(state19, references) : ((cAbility is CAbilityAddActiveBonus state20) ? new CAbilityAddActiveBonus(state20, references) : ((cAbility is CAbilityAddAugment state21) ? new CAbilityAddAugment(state21, references) : ((cAbility is CAbilityAddCondition state22) ? new CAbilityAddCondition(state22, references) : ((cAbility is CAbilityAddDoom state23) ? new CAbilityAddDoom(state23, references) : ((cAbility is CAbilityAddDoomSlots state24) ? new CAbilityAddDoomSlots(state24, references) : ((cAbility is CAbilityAddHeal state25) ? new CAbilityAddHeal(state25, references) : ((cAbility is CAbilityAddRange state26) ? new CAbilityAddRange(state26, references) : ((cAbility is CAbilityAddSong state27) ? new CAbilityAddSong(state27, references) : ((cAbility is CAbilityAddTarget state28) ? new CAbilityAddTarget(state28, references) : ((cAbility is CAbilityAdjustInitiative state29) ? new CAbilityAdjustInitiative(state29, references) : ((cAbility is CAbilityAttackersGainDisadvantage state30) ? new CAbilityAttackersGainDisadvantage(state30, references) : ((cAbility is CAbilityChangeAllegiance state31) ? new CAbilityChangeAllegiance(state31, references) : ((cAbility is CAbilityChangeCharacterModel state32) ? new CAbilityChangeCharacterModel(state32, references) : ((cAbility is CAbilityChoose state33) ? new CAbilityChoose(state33, references) : ((cAbility is CAbilityConsume state34) ? new CAbilityConsume(state34, references) : ((cAbility is CAbilityConsumeItemCards state35) ? new CAbilityConsumeItemCards(state35, references) : ((cAbility is CAbilityControlActor state36) ? new CAbilityControlActor(state36, references) : ((cAbility is CAbilityDisableCardAction state37) ? new CAbilityDisableCardAction(state37, references) : ((cAbility is CAbilityDiscardCards state38) ? new CAbilityDiscardCards(state38, references) : ((cAbility is CAbilityExtraTurn state39) ? new CAbilityExtraTurn(state39, references) : ((cAbility is CAbilityForgoActionsForCompanion state40) ? new CAbilityForgoActionsForCompanion(state40, references) : ((cAbility is CAbilityGiveSupplyCard state41) ? new CAbilityGiveSupplyCard(state41, references) : ((cAbility is CAbilityHeal state42) ? new CAbilityHeal(state42, references) : ((cAbility is CAbilityHealthReduction state43) ? new CAbilityHealthReduction(state43, references) : ((cAbility is CAbilityImmunityTo state44) ? new CAbilityImmunityTo(state44, references) : ((cAbility is CAbilityImprovedShortRest state45) ? new CAbilityImprovedShortRest(state45, references) : ((cAbility is CAbilityIncreaseCardLimit state46) ? new CAbilityIncreaseCardLimit(state46, references) : ((cAbility is CAbilityInvulnerability state47) ? new CAbilityInvulnerability(state47, references) : ((cAbility is CAbilityItemLock state48) ? new CAbilityItemLock(state48, references) : ((cAbility is CAbilityKill state49) ? new CAbilityKill(state49, references) : ((cAbility is CAbilityLoseCards state50) ? new CAbilityLoseCards(state50, references) : ((cAbility is CAbilityLoseGoalChestReward state51) ? new CAbilityLoseGoalChestReward(state51, references) : ((cAbility is CAbilityNullTargeting state52) ? new CAbilityNullTargeting(state52, references) : ((cAbility is CAbilityOverrideAugmentAttackType state53) ? new CAbilityOverrideAugmentAttackType(state53, references) : ((cAbility is CAbilityPierceInvulnerability state54) ? new CAbilityPierceInvulnerability(state54, references) : ((cAbility is CAbilityPlaySong state55) ? new CAbilityPlaySong(state55, references) : ((cAbility is CAbilityPreventDamage state56) ? new CAbilityPreventDamage(state56, references) : ((cAbility is CAbilityRecoverDiscardedCards state57) ? new CAbilityRecoverDiscardedCards(state57, references) : ((cAbility is CAbilityRecoverLostCards state58) ? new CAbilityRecoverLostCards(state58, references) : ((cAbility is CAbilityRedirect state59) ? new CAbilityRedirect(state59, references) : ((cAbility is CAbilityRefreshItemCards state60) ? new CAbilityRefreshItemCards(state60, references) : ((cAbility is CAbilityRemoveActorFromMap state61) ? new CAbilityRemoveActorFromMap(state61, references) : ((cAbility is CAbilityRemoveConditions state62) ? new CAbilityRemoveConditions(state62, references) : ((cAbility is CAbilityRetaliate state63) ? new CAbilityRetaliate(state63, references) : ((cAbility is CAbilityShield state64) ? new CAbilityShield(state64, references) : ((cAbility is CAbilityShuffleModifierDeck state65) ? new CAbilityShuffleModifierDeck(state65, references) : ((cAbility is CAbilityTransferDooms state66) ? new CAbilityTransferDooms(state66, references) : ((cAbility is CAbilityUntargetable state67) ? new CAbilityUntargetable(state67, references) : ((cAbility is CAbilityCondition state68) ? new CAbilityCondition(state68, references) : ((cAbility is CAbilityMergedCreateAttack state69) ? new CAbilityMergedCreateAttack(state69, references) : ((cAbility is CAbilityMergedDestroyAttack state70) ? new CAbilityMergedDestroyAttack(state70, references) : ((cAbility is CAbilityMergedDisarmTrapDestroyObstacle state71) ? new CAbilityMergedDisarmTrapDestroyObstacle(state71, references) : ((cAbility is CAbilityMergedKillCreate state72) ? new CAbilityMergedKillCreate(state72, references) : ((cAbility is CAbilityMergedMoveAttack state73) ? new CAbilityMergedMoveAttack(state73, references) : ((cAbility is CAbilityMergedMoveObstacleAttack state74) ? new CAbilityMergedMoveObstacleAttack(state74, references) : ((cAbility is CAbilityActivateSpawner state75) ? new CAbilityActivateSpawner(state75, references) : ((cAbility is CAbilityAddModifierToMonsterDeck state76) ? new CAbilityAddModifierToMonsterDeck(state76, references) : ((cAbility is CAbilityAttack state77) ? new CAbilityAttack(state77, references) : ((cAbility is CAbilityChangeCondition state78) ? new CAbilityChangeCondition(state78, references) : ((cAbility is CAbilityChangeModifier state79) ? new CAbilityChangeModifier(state79, references) : ((cAbility is CAbilityConsumeElement state80) ? new CAbilityConsumeElement(state80, references) : ((cAbility is CAbilityCreate state81) ? new CAbilityCreate(state81, references) : ((cAbility is CAbilityDamage state82) ? new CAbilityDamage(state82, references) : ((cAbility is CAbilityDeactivateSpawner state83) ? new CAbilityDeactivateSpawner(state83, references) : ((cAbility is CAbilityDestroyObstacle state84) ? new CAbilityDestroyObstacle(state84, references) : ((cAbility is CAbilityDisarmTrap state85) ? new CAbilityDisarmTrap(state85, references) : ((cAbility is CAbilityFear state86) ? new CAbilityFear(state86, references) : ((cAbility is CAbilityInfuse state87) ? new CAbilityInfuse(state87, references) : ((cAbility is CAbilityLoot state88) ? new CAbilityLoot(state88, references) : ((cAbility is CAbilityMove state89) ? new CAbilityMove(state89, references) : ((cAbility is CAbilityMoveObstacle state90) ? new CAbilityMoveObstacle(state90, references) : ((cAbility is CAbilityMoveTrap state91) ? new CAbilityMoveTrap(state91, references) : ((cAbility is CAbilityNull state92) ? new CAbilityNull(state92, references) : ((cAbility is CAbilityNullHex state93) ? new CAbilityNullHex(state93, references) : ((cAbility is CAbilityPull state94) ? new CAbilityPull(state94, references) : ((cAbility is CAbilityPush state95) ? new CAbilityPush(state95, references) : ((cAbility is CAbilityRecoverResources state96) ? new CAbilityRecoverResources(state96, references) : ((cAbility is CAbilityRedistributeDamage state97) ? new CAbilityRedistributeDamage(state97, references) : ((cAbility is CAbilityRevive state98) ? new CAbilityRevive(state98, references) : ((cAbility is CAbilitySummon state99) ? new CAbilitySummon(state99, references) : ((cAbility is CAbilitySwap state100) ? new CAbilitySwap(state100, references) : ((cAbility is CAbilityTargeting state101) ? new CAbilityTargeting(state101, references) : ((cAbility is CAbilityTeleport state102) ? new CAbilityTeleport(state102, references) : ((cAbility is CAbilityTrap state103) ? new CAbilityTrap(state103, references) : ((cAbility is CAbilityXP state104) ? new CAbilityXP(state104, references) : ((!(cAbility is CAbilityMerged state105)) ? new CAbility(cAbility, references) : new CAbilityMerged(state105, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility2 = cAbility3;
					references.Add(cAbility, cAbility2);
				}
				SubAbilities.Add(cAbility2);
			}
			references.Add(state.SubAbilities, SubAbilities);
		}
		AddAttackBaseStat = state.AddAttackBaseStat;
		StrengthIsBase = state.StrengthIsBase;
		RangeIsBase = state.RangeIsBase;
		TargetIsBase = state.TargetIsBase;
		IsMonsterAbility = state.IsMonsterAbility;
		TargetsSet = state.TargetsSet;
		AbilityText = state.AbilityText;
		AbilityTextOnly = state.AbilityTextOnly;
		ShowRange = state.ShowRange;
		ShowTarget = state.ShowTarget;
		ShowArea = state.ShowArea;
		OnDeath = state.OnDeath;
		StackedAttackEffectAbility = state.StackedAttackEffectAbility;
		TilesInRange = references.Get(state.TilesInRange);
		if (TilesInRange == null && state.TilesInRange != null)
		{
			TilesInRange = new List<CTile>();
			for (int m = 0; m < state.TilesInRange.Count; m++)
			{
				CTile cTile = state.TilesInRange[m];
				CTile cTile2 = references.Get(cTile);
				if (cTile2 == null && cTile != null)
				{
					cTile2 = new CTile(cTile, references);
					references.Add(cTile, cTile2);
				}
				TilesInRange.Add(cTile2);
			}
			references.Add(state.TilesInRange, TilesInRange);
		}
		AllPossibleTilesInAreaEffect = references.Get(state.AllPossibleTilesInAreaEffect);
		if (AllPossibleTilesInAreaEffect == null && state.AllPossibleTilesInAreaEffect != null)
		{
			AllPossibleTilesInAreaEffect = new List<CTile>();
			for (int n = 0; n < state.AllPossibleTilesInAreaEffect.Count; n++)
			{
				CTile cTile3 = state.AllPossibleTilesInAreaEffect[n];
				CTile cTile4 = references.Get(cTile3);
				if (cTile4 == null && cTile3 != null)
				{
					cTile4 = new CTile(cTile3, references);
					references.Add(cTile3, cTile4);
				}
				AllPossibleTilesInAreaEffect.Add(cTile4);
			}
			references.Add(state.AllPossibleTilesInAreaEffect, AllPossibleTilesInAreaEffect);
		}
		ActorsTargeted = references.Get(state.ActorsTargeted);
		if (ActorsTargeted == null && state.ActorsTargeted != null)
		{
			ActorsTargeted = new List<CActor>();
			for (int num = 0; num < state.ActorsTargeted.Count; num++)
			{
				CActor cActor = state.ActorsTargeted[num];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state106) ? new CObjectActor(state106, references) : ((cActor is CEnemyActor state107) ? new CEnemyActor(state107, references) : ((cActor is CHeroSummonActor state108) ? new CHeroSummonActor(state108, references) : ((!(cActor is CPlayerActor state109)) ? new CActor(cActor, references) : new CPlayerActor(state109, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				ActorsTargeted.Add(cActor2);
			}
			references.Add(state.ActorsTargeted, ActorsTargeted);
		}
		DamageInflictedByAbility = state.DamageInflictedByAbility;
		DamageActuallyTakenByAbility = state.DamageActuallyTakenByAbility;
		CanUndoOverride = state.CanUndoOverride;
		DamageInflictedByAbilityOnLastTarget = state.DamageInflictedByAbilityOnLastTarget;
		ExcessDamageInflictedOnLastTargetKilled = state.ExcessDamageInflictedOnLastTargetKilled;
		AppliedEnhancements = state.AppliedEnhancements;
		AbilityStartListenersInvoked = state.AbilityStartListenersInvoked;
		m_ValidActorsInRange = references.Get(state.m_ValidActorsInRange);
		if (m_ValidActorsInRange == null && state.m_ValidActorsInRange != null)
		{
			m_ValidActorsInRange = new List<CActor>();
			for (int num2 = 0; num2 < state.m_ValidActorsInRange.Count; num2++)
			{
				CActor cActor4 = state.m_ValidActorsInRange[num2];
				CActor cActor5 = references.Get(cActor4);
				if (cActor5 == null && cActor4 != null)
				{
					CActor cActor3 = ((cActor4 is CObjectActor state110) ? new CObjectActor(state110, references) : ((cActor4 is CEnemyActor state111) ? new CEnemyActor(state111, references) : ((cActor4 is CHeroSummonActor state112) ? new CHeroSummonActor(state112, references) : ((!(cActor4 is CPlayerActor state113)) ? new CActor(cActor4, references) : new CPlayerActor(state113, references)))));
					cActor5 = cActor3;
					references.Add(cActor4, cActor5);
				}
				m_ValidActorsInRange.Add(cActor5);
			}
			references.Add(state.m_ValidActorsInRange, m_ValidActorsInRange);
		}
		m_IsSubAbility = state.m_IsSubAbility;
		m_IsMergedAbility = state.m_IsMergedAbility;
		m_IsModifierAbility = state.m_IsModifierAbility;
		m_Strength = state.m_Strength;
		m_ModifiedStrength = state.m_ModifiedStrength;
		m_Range = state.m_Range;
		m_OriginalRange = state.m_OriginalRange;
		m_NumberTargets = state.m_NumberTargets;
		m_OriginalTargetCount = state.m_OriginalTargetCount;
		m_ActorsToIgnore = references.Get(state.m_ActorsToIgnore);
		if (m_ActorsToIgnore == null && state.m_ActorsToIgnore != null)
		{
			m_ActorsToIgnore = new List<CActor>();
			for (int num3 = 0; num3 < state.m_ActorsToIgnore.Count; num3++)
			{
				CActor cActor6 = state.m_ActorsToIgnore[num3];
				CActor cActor7 = references.Get(cActor6);
				if (cActor7 == null && cActor6 != null)
				{
					CActor cActor3 = ((cActor6 is CObjectActor state114) ? new CObjectActor(state114, references) : ((cActor6 is CEnemyActor state115) ? new CEnemyActor(state115, references) : ((cActor6 is CHeroSummonActor state116) ? new CHeroSummonActor(state116, references) : ((!(cActor6 is CPlayerActor state117)) ? new CActor(cActor6, references) : new CPlayerActor(state117, references)))));
					cActor7 = cActor3;
					references.Add(cActor6, cActor7);
				}
				m_ActorsToIgnore.Add(cActor7);
			}
			references.Add(state.m_ActorsToIgnore, m_ActorsToIgnore);
		}
		m_AbilityEnhancements = references.Get(state.m_AbilityEnhancements);
		if (m_AbilityEnhancements == null && state.m_AbilityEnhancements != null)
		{
			m_AbilityEnhancements = new List<CEnhancement>();
			for (int num4 = 0; num4 < state.m_AbilityEnhancements.Count; num4++)
			{
				CEnhancement cEnhancement = state.m_AbilityEnhancements[num4];
				CEnhancement cEnhancement2 = references.Get(cEnhancement);
				if (cEnhancement2 == null && cEnhancement != null)
				{
					cEnhancement2 = new CEnhancement(cEnhancement, references);
					references.Add(cEnhancement, cEnhancement2);
				}
				m_AbilityEnhancements.Add(cEnhancement2);
			}
			references.Add(state.m_AbilityEnhancements, m_AbilityEnhancements);
		}
		m_PositiveConditions = references.Get(state.m_PositiveConditions);
		if (m_PositiveConditions == null && state.m_PositiveConditions != null)
		{
			m_PositiveConditions = new Dictionary<CCondition.EPositiveCondition, CAbility>(state.m_PositiveConditions.Comparer);
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in state.m_PositiveConditions)
			{
				CCondition.EPositiveCondition key4 = positiveCondition.Key;
				CAbility cAbility4 = references.Get(positiveCondition.Value);
				if (cAbility4 == null && positiveCondition.Value != null)
				{
					cAbility4 = new CAbility(positiveCondition.Value, references);
					references.Add(positiveCondition.Value, cAbility4);
				}
				m_PositiveConditions.Add(key4, cAbility4);
			}
			references.Add(state.m_PositiveConditions, m_PositiveConditions);
		}
		m_NegativeConditions = references.Get(state.m_NegativeConditions);
		if (m_NegativeConditions == null && state.m_NegativeConditions != null)
		{
			m_NegativeConditions = new Dictionary<CCondition.ENegativeCondition, CAbility>(state.m_NegativeConditions.Comparer);
			foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in state.m_NegativeConditions)
			{
				CCondition.ENegativeCondition key5 = negativeCondition.Key;
				CAbility cAbility5 = references.Get(negativeCondition.Value);
				if (cAbility5 == null && negativeCondition.Value != null)
				{
					cAbility5 = new CAbility(negativeCondition.Value, references);
					references.Add(negativeCondition.Value, cAbility5);
				}
				m_NegativeConditions.Add(key5, cAbility5);
			}
			references.Add(state.m_NegativeConditions, m_NegativeConditions);
		}
		m_TilesSelected = references.Get(state.m_TilesSelected);
		if (m_TilesSelected == null && state.m_TilesSelected != null)
		{
			m_TilesSelected = new List<CTile>();
			for (int num5 = 0; num5 < state.m_TilesSelected.Count; num5++)
			{
				CTile cTile5 = state.m_TilesSelected[num5];
				CTile cTile6 = references.Get(cTile5);
				if (cTile6 == null && cTile5 != null)
				{
					cTile6 = new CTile(cTile5, references);
					references.Add(cTile5, cTile6);
				}
				m_TilesSelected.Add(cTile6);
			}
			references.Add(state.m_TilesSelected, m_TilesSelected);
		}
		m_CanUndo = state.m_CanUndo;
		m_CanSkip = state.m_CanSkip;
		m_ActorsToTarget = references.Get(state.m_ActorsToTarget);
		if (m_ActorsToTarget == null && state.m_ActorsToTarget != null)
		{
			m_ActorsToTarget = new List<CActor>();
			for (int num6 = 0; num6 < state.m_ActorsToTarget.Count; num6++)
			{
				CActor cActor8 = state.m_ActorsToTarget[num6];
				CActor cActor9 = references.Get(cActor8);
				if (cActor9 == null && cActor8 != null)
				{
					CActor cActor3 = ((cActor8 is CObjectActor state118) ? new CObjectActor(state118, references) : ((cActor8 is CEnemyActor state119) ? new CEnemyActor(state119, references) : ((cActor8 is CHeroSummonActor state120) ? new CHeroSummonActor(state120, references) : ((!(cActor8 is CPlayerActor state121)) ? new CActor(cActor8, references) : new CPlayerActor(state121, references)))));
					cActor9 = cActor3;
					references.Add(cActor8, cActor9);
				}
				m_ActorsToTarget.Add(cActor9);
			}
			references.Add(state.m_ActorsToTarget, m_ActorsToTarget);
		}
		m_NumberTargetsRemaining = state.m_NumberTargetsRemaining;
		m_UndoNumberTargetsRemaining = state.m_UndoNumberTargetsRemaining;
		m_ProcessIfDead = state.m_ProcessIfDead;
		m_AllTargets = state.m_AllTargets;
		m_CancelAbility = state.m_CancelAbility;
		m_AbilityHasHappened = state.m_AbilityHasHappened;
		m_AbilityStartComplete = state.m_AbilityStartComplete;
		m_CurrentItemOverrides = references.Get(state.m_CurrentItemOverrides);
		if (m_CurrentItemOverrides == null && state.m_CurrentItemOverrides != null)
		{
			m_CurrentItemOverrides = new Dictionary<CAbilityOverride, CItem>(state.m_CurrentItemOverrides.Comparer);
			foreach (KeyValuePair<CAbilityOverride, CItem> currentItemOverride in state.m_CurrentItemOverrides)
			{
				CAbilityOverride cAbilityOverride = references.Get(currentItemOverride.Key);
				if (cAbilityOverride == null && currentItemOverride.Key != null)
				{
					cAbilityOverride = new CAbilityOverride(currentItemOverride.Key, references);
					references.Add(currentItemOverride.Key, cAbilityOverride);
				}
				CItem cItem = references.Get(currentItemOverride.Value);
				if (cItem == null && currentItemOverride.Value != null)
				{
					cItem = new CItem(currentItemOverride.Value, references);
					references.Add(currentItemOverride.Value, cItem);
				}
				m_CurrentItemOverrides.Add(cAbilityOverride, cItem);
			}
			references.Add(state.m_CurrentItemOverrides, m_CurrentItemOverrides);
		}
		m_CurrentOverrides = references.Get(state.m_CurrentOverrides);
		if (m_CurrentOverrides == null && state.m_CurrentOverrides != null)
		{
			m_CurrentOverrides = new List<CAbilityOverride>();
			for (int num7 = 0; num7 < state.m_CurrentOverrides.Count; num7++)
			{
				CAbilityOverride cAbilityOverride2 = state.m_CurrentOverrides[num7];
				CAbilityOverride cAbilityOverride3 = references.Get(cAbilityOverride2);
				if (cAbilityOverride3 == null && cAbilityOverride2 != null)
				{
					cAbilityOverride3 = new CAbilityOverride(cAbilityOverride2, references);
					references.Add(cAbilityOverride2, cAbilityOverride3);
				}
				m_CurrentOverrides.Add(cAbilityOverride3);
			}
			references.Add(state.m_CurrentOverrides, m_CurrentOverrides);
		}
		m_ActiveSingleTargetItems = references.Get(state.m_ActiveSingleTargetItems);
		if (m_ActiveSingleTargetItems == null && state.m_ActiveSingleTargetItems != null)
		{
			m_ActiveSingleTargetItems = new List<CItem>();
			for (int num8 = 0; num8 < state.m_ActiveSingleTargetItems.Count; num8++)
			{
				CItem cItem2 = state.m_ActiveSingleTargetItems[num8];
				CItem cItem3 = references.Get(cItem2);
				if (cItem3 == null && cItem2 != null)
				{
					cItem3 = new CItem(cItem2, references);
					references.Add(cItem2, cItem3);
				}
				m_ActiveSingleTargetItems.Add(cItem3);
			}
			references.Add(state.m_ActiveSingleTargetItems, m_ActiveSingleTargetItems);
		}
		m_ActiveSingleTargetActiveBonuses = references.Get(state.m_ActiveSingleTargetActiveBonuses);
		if (m_ActiveSingleTargetActiveBonuses == null && state.m_ActiveSingleTargetActiveBonuses != null)
		{
			m_ActiveSingleTargetActiveBonuses = new List<CActiveBonus>();
			for (int num9 = 0; num9 < state.m_ActiveSingleTargetActiveBonuses.Count; num9++)
			{
				CActiveBonus cActiveBonus = state.m_ActiveSingleTargetActiveBonuses[num9];
				CActiveBonus cActiveBonus2 = references.Get(cActiveBonus);
				if (cActiveBonus2 == null && cActiveBonus != null)
				{
					CActiveBonus cActiveBonus3 = ((cActiveBonus is CAddConditionActiveBonus state122) ? new CAddConditionActiveBonus(state122, references) : ((cActiveBonus is CAddHealActiveBonus state123) ? new CAddHealActiveBonus(state123, references) : ((cActiveBonus is CAddRangeActiveBonus state124) ? new CAddRangeActiveBonus(state124, references) : ((cActiveBonus is CAddTargetActiveBonus state125) ? new CAddTargetActiveBonus(state125, references) : ((cActiveBonus is CAdjustInitiativeActiveBonus state126) ? new CAdjustInitiativeActiveBonus(state126, references) : ((cActiveBonus is CAdvantageActiveBonus state127) ? new CAdvantageActiveBonus(state127, references) : ((cActiveBonus is CAttackActiveBonus state128) ? new CAttackActiveBonus(state128, references) : ((cActiveBonus is CAttackersGainDisadvantageActiveBonus state129) ? new CAttackersGainDisadvantageActiveBonus(state129, references) : ((cActiveBonus is CChangeCharacterModelActiveBonus state130) ? new CChangeCharacterModelActiveBonus(state130, references) : ((cActiveBonus is CChangeConditionActiveBonus state131) ? new CChangeConditionActiveBonus(state131, references) : ((cActiveBonus is CChangeModifierActiveBonus state132) ? new CChangeModifierActiveBonus(state132, references) : ((cActiveBonus is CChooseAbilityActiveBonus state133) ? new CChooseAbilityActiveBonus(state133, references) : ((cActiveBonus is CDamageActiveBonus state134) ? new CDamageActiveBonus(state134, references) : ((cActiveBonus is CDisableCardActionActiveBonus state135) ? new CDisableCardActionActiveBonus(state135, references) : ((cActiveBonus is CDuringActionAbilityActiveBonus state136) ? new CDuringActionAbilityActiveBonus(state136, references) : ((cActiveBonus is CDuringTurnAbilityActiveBonus state137) ? new CDuringTurnAbilityActiveBonus(state137, references) : ((cActiveBonus is CEndActionAbilityActiveBonus state138) ? new CEndActionAbilityActiveBonus(state138, references) : ((cActiveBonus is CEndRoundAbilityActiveBonus state139) ? new CEndRoundAbilityActiveBonus(state139, references) : ((cActiveBonus is CEndTurnAbilityActiveBonus state140) ? new CEndTurnAbilityActiveBonus(state140, references) : ((cActiveBonus is CForgoActionsForCompanionActiveBonus state141) ? new CForgoActionsForCompanionActiveBonus(state141, references) : ((cActiveBonus is CHealthReductionActiveBonus state142) ? new CHealthReductionActiveBonus(state142, references) : ((cActiveBonus is CImmunityActiveBonus state143) ? new CImmunityActiveBonus(state143, references) : ((cActiveBonus is CInfuseActiveBonus state144) ? new CInfuseActiveBonus(state144, references) : ((cActiveBonus is CInvulnerabilityActiveBonus state145) ? new CInvulnerabilityActiveBonus(state145, references) : ((cActiveBonus is CItemLockActiveBonus state146) ? new CItemLockActiveBonus(state146, references) : ((cActiveBonus is CLootActiveBonus state147) ? new CLootActiveBonus(state147, references) : ((cActiveBonus is CMoveActiveBonus state148) ? new CMoveActiveBonus(state148, references) : ((cActiveBonus is COverhealActiveBonus state149) ? new COverhealActiveBonus(state149, references) : ((cActiveBonus is COverrideAbilityTypeActiveBonus state150) ? new COverrideAbilityTypeActiveBonus(state150, references) : ((cActiveBonus is CPierceInvulnerabilityActiveBonus state151) ? new CPierceInvulnerabilityActiveBonus(state151, references) : ((cActiveBonus is CPreventDamageActiveBonus state152) ? new CPreventDamageActiveBonus(state152, references) : ((cActiveBonus is CRedirectActiveBonus state153) ? new CRedirectActiveBonus(state153, references) : ((cActiveBonus is CRetaliateActiveBonus state154) ? new CRetaliateActiveBonus(state154, references) : ((cActiveBonus is CShieldActiveBonus state155) ? new CShieldActiveBonus(state155, references) : ((cActiveBonus is CStartActionAbilityActiveBonus state156) ? new CStartActionAbilityActiveBonus(state156, references) : ((cActiveBonus is CStartRoundAbilityActiveBonus state157) ? new CStartRoundAbilityActiveBonus(state157, references) : ((cActiveBonus is CStartTurnAbilityActiveBonus state158) ? new CStartTurnAbilityActiveBonus(state158, references) : ((cActiveBonus is CSummonActiveBonus state159) ? new CSummonActiveBonus(state159, references) : ((!(cActiveBonus is CUntargetableActiveBonus state160)) ? new CActiveBonus(cActiveBonus, references) : new CUntargetableActiveBonus(state160, references))))))))))))))))))))))))))))))))))))))));
					cActiveBonus2 = cActiveBonus3;
					references.Add(cActiveBonus, cActiveBonus2);
				}
				m_ActiveSingleTargetActiveBonuses.Add(cActiveBonus2);
			}
			references.Add(state.m_ActiveSingleTargetActiveBonuses, m_ActiveSingleTargetActiveBonuses);
		}
		m_ValidTilesInAreaEffect = references.Get(state.m_ValidTilesInAreaEffect);
		if (m_ValidTilesInAreaEffect == null && state.m_ValidTilesInAreaEffect != null)
		{
			m_ValidTilesInAreaEffect = new List<CTile>();
			for (int num10 = 0; num10 < state.m_ValidTilesInAreaEffect.Count; num10++)
			{
				CTile cTile7 = state.m_ValidTilesInAreaEffect[num10];
				CTile cTile8 = references.Get(cTile7);
				if (cTile8 == null && cTile7 != null)
				{
					cTile8 = new CTile(cTile7, references);
					references.Add(cTile7, cTile8);
				}
				m_ValidTilesInAreaEffect.Add(cTile8);
			}
			references.Add(state.m_ValidTilesInAreaEffect, m_ValidTilesInAreaEffect);
		}
		m_ValidTilesInAreaEffectIncludingBlocked = references.Get(state.m_ValidTilesInAreaEffectIncludingBlocked);
		if (m_ValidTilesInAreaEffectIncludingBlocked == null && state.m_ValidTilesInAreaEffectIncludingBlocked != null)
		{
			m_ValidTilesInAreaEffectIncludingBlocked = new List<CTile>();
			for (int num11 = 0; num11 < state.m_ValidTilesInAreaEffectIncludingBlocked.Count; num11++)
			{
				CTile cTile9 = state.m_ValidTilesInAreaEffectIncludingBlocked[num11];
				CTile cTile10 = references.Get(cTile9);
				if (cTile10 == null && cTile9 != null)
				{
					cTile10 = new CTile(cTile9, references);
					references.Add(cTile9, cTile10);
				}
				m_ValidTilesInAreaEffectIncludingBlocked.Add(cTile10);
			}
			references.Add(state.m_ValidTilesInAreaEffectIncludingBlocked, m_ValidTilesInAreaEffectIncludingBlocked);
		}
		m_InlineSubAbilityTiles = references.Get(state.m_InlineSubAbilityTiles);
		if (m_InlineSubAbilityTiles == null && state.m_InlineSubAbilityTiles != null)
		{
			m_InlineSubAbilityTiles = new List<CTile>();
			for (int num12 = 0; num12 < state.m_InlineSubAbilityTiles.Count; num12++)
			{
				CTile cTile11 = state.m_InlineSubAbilityTiles[num12];
				CTile cTile12 = references.Get(cTile11);
				if (cTile12 == null && cTile11 != null)
				{
					cTile12 = new CTile(cTile11, references);
					references.Add(cTile11, cTile12);
				}
				m_InlineSubAbilityTiles.Add(cTile12);
			}
			references.Add(state.m_InlineSubAbilityTiles, m_InlineSubAbilityTiles);
		}
		m_AreaEffectLocked = state.m_AreaEffectLocked;
		m_AreaEffectAngle = state.m_AreaEffectAngle;
		m_IsScenarioModifierAbility = state.m_IsScenarioModifierAbility;
		m_AugmentsAdded = state.m_AugmentsAdded;
		m_AugmentAbilitiesProcessed = state.m_AugmentAbilitiesProcessed;
		m_AugmentAbilitiesTargetCount = state.m_AugmentAbilitiesTargetCount;
		m_AugmentAbilitiesNextTarget = state.m_AugmentAbilitiesNextTarget;
		m_AugmentOverridesProcessed = state.m_AugmentOverridesProcessed;
		m_AbilityAugments = references.Get(state.m_AbilityAugments);
		if (m_AbilityAugments == null && state.m_AbilityAugments != null)
		{
			m_AbilityAugments = new List<CAugment>();
			for (int num13 = 0; num13 < state.m_AbilityAugments.Count; num13++)
			{
				CAugment cAugment = state.m_AbilityAugments[num13];
				CAugment cAugment2 = references.Get(cAugment);
				if (cAugment2 == null && cAugment != null)
				{
					cAugment2 = new CAugment(cAugment, references);
					references.Add(cAugment, cAugment2);
				}
				m_AbilityAugments.Add(cAugment2);
			}
			references.Add(state.m_AbilityAugments, m_AbilityAugments);
		}
		m_OverrideAugments = references.Get(state.m_OverrideAugments);
		if (m_OverrideAugments == null && state.m_OverrideAugments != null)
		{
			m_OverrideAugments = new List<CAugment>();
			for (int num14 = 0; num14 < state.m_OverrideAugments.Count; num14++)
			{
				CAugment cAugment3 = state.m_OverrideAugments[num14];
				CAugment cAugment4 = references.Get(cAugment3);
				if (cAugment4 == null && cAugment3 != null)
				{
					cAugment4 = new CAugment(cAugment3, references);
					references.Add(cAugment3, cAugment4);
				}
				m_OverrideAugments.Add(cAugment4);
			}
			references.Add(state.m_OverrideAugments, m_OverrideAugments);
		}
		m_SongAbilitiesProcessed = state.m_SongAbilitiesProcessed;
		m_SongAbilitiesTargetCount = state.m_SongAbilitiesTargetCount;
		m_SongAbilitiesNextTarget = state.m_SongAbilitiesNextTarget;
		m_SongOverridesProcessed = state.m_SongOverridesProcessed;
		m_AbilitySongs = references.Get(state.m_AbilitySongs);
		if (m_AbilitySongs == null && state.m_AbilitySongs != null)
		{
			m_AbilitySongs = new List<CSong.SongEffect>();
			for (int num15 = 0; num15 < state.m_AbilitySongs.Count; num15++)
			{
				CSong.SongEffect songEffect = state.m_AbilitySongs[num15];
				CSong.SongEffect songEffect2 = references.Get(songEffect);
				if (songEffect2 == null && songEffect != null)
				{
					songEffect2 = new CSong.SongEffect(songEffect, references);
					references.Add(songEffect, songEffect2);
				}
				m_AbilitySongs.Add(songEffect2);
			}
			references.Add(state.m_AbilitySongs, m_AbilitySongs);
		}
		m_OverrideSongs = references.Get(state.m_OverrideSongs);
		if (m_OverrideSongs == null && state.m_OverrideSongs != null)
		{
			m_OverrideSongs = new List<CSong.SongEffect>();
			for (int num16 = 0; num16 < state.m_OverrideSongs.Count; num16++)
			{
				CSong.SongEffect songEffect3 = state.m_OverrideSongs[num16];
				CSong.SongEffect songEffect4 = references.Get(songEffect3);
				if (songEffect4 == null && songEffect3 != null)
				{
					songEffect4 = new CSong.SongEffect(songEffect3, references);
					references.Add(songEffect3, songEffect4);
				}
				m_OverrideSongs.Add(songEffect4);
			}
			references.Add(state.m_OverrideSongs, m_OverrideSongs);
		}
		m_IsControlAbility = state.m_IsControlAbility;
		m_InfuseElements = references.Get(state.m_InfuseElements);
		if (m_InfuseElements == null && state.m_InfuseElements != null)
		{
			m_InfuseElements = new List<ElementInfusionBoardManager.EElement>();
			for (int num17 = 0; num17 < state.m_InfuseElements.Count; num17++)
			{
				ElementInfusionBoardManager.EElement item = state.m_InfuseElements[num17];
				m_InfuseElements.Add(item);
			}
			references.Add(state.m_InfuseElements, m_InfuseElements);
		}
	}
}
