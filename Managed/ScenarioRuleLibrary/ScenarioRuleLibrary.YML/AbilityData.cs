using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.YML;
using StateCodeGenerator;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AbilityData
{
	public class XpPerTargetData
	{
		public int XpPerTarget;

		public int PerEnemyCount = 1;

		public bool MustDie;

		private int targetsAttacked;

		public XpPerTargetData Copy()
		{
			return new XpPerTargetData
			{
				XpPerTarget = XpPerTarget,
				PerEnemyCount = PerEnemyCount,
				MustDie = MustDie,
				targetsAttacked = 0
			};
		}

		public void Init()
		{
			targetsAttacked = 0;
		}

		public int AttackTargets(CActor targetingActor, List<CActor> targets)
		{
			int num = 0;
			foreach (CActor target in targets)
			{
				num += AttackTarget(targetingActor, target);
			}
			return num;
		}

		public int AttackTarget(CActor targetingActor, CActor target)
		{
			int num = 0;
			if (targetingActor.Type == CActor.EType.Player && (!MustDie || (MustDie && target.Health <= 0)))
			{
				targetsAttacked++;
				if (targetsAttacked % PerEnemyCount == 0)
				{
					num += XpPerTarget;
				}
			}
			return num;
		}
	}

	public class MiscAbilityData
	{
		public bool? TargetOneEnemyWithAllAttacks;

		public bool? TreatAsMelee;

		public bool? UseParentRangeType;

		public bool? AllTargetsAdjacentToParentMovePath;

		public bool? AllTargetsAdjacentToParentTargets;

		public bool? UseParentTiles;

		public bool? IgnoreParentAreaOfEffect;

		public List<string> IgnorePreviousAbilityTargets;

		public bool? IgnoreMergedAbilityTargetSelection;

		public bool? UseMergedWithAbilityTiles;

		public CActor.EOverrideLookupProperty? AddTargetPropertyToStrength;

		public int? AddTargetPropertyToStrengthMultiplier;

		public int? RestrictMoveRange;

		public bool? AutotriggerAbility;

		public float? HealPercentageOfHealth;

		public bool? ExactRange;

		public bool? FilterSpecified;

		public bool? AttackHasAdvantage;

		public bool? AttackHasDisadvantage;

		public bool? ExhaustSelf;

		public int? GainXPPerXDamageDealt;

		public int? SetHPTo;

		public bool? PreventOnlyIfLethal;

		public bool? InfuseIfNotStrong;

		public bool? CanTargetInvisible;

		public bool? NotApplyEnemy;

		public bool? IgnoreMonsterColumnLayout;

		public int? IgnoreMCLAdjustY;

		public int? GapAbove;

		public int? GapBelow;

		public bool? CanUndo;

		public bool? CanSkip;

		public bool? TreatAsNonSubAbility;

		public CAbility.EStatIsBasedOnXType? PerformXTimesBasedOn;

		public bool? ConsiderMultiTargetForEnhancements;

		public int? AlreadyHasConditionDamageInstead;

		public bool? IgnoreStun;

		public bool? HasCondition;

		public bool? AreaEffectSymmetrical;

		public int? RemoveCount;

		public bool? UseParentTargets;

		public bool? AlsoTargetSelf;

		public CAbilityFilter AlsoTargetAdjacent;

		public List<CCondition.ENegativeCondition> ConditionsToRemoveFirst;

		public bool? ShowPlusX;

		public List<string> TriggeredScenarioModifiers;

		public bool? TriggerScenarioModifierOnSelfOnly;

		public bool? BypassImmunity;

		public bool? InlineSubAbilityOnKilledTargetsOnly;

		public CEqualityFilter MovePathIndexFilter;

		public bool? NegativeAbilityIsOptional;

		public bool? AllowContinueForNullAbility;

		public bool? NoGoldDrop;

		public bool? UseOriginalActor;

		public List<string> ReplaceModifiers;

		public string ReplaceWithModifier;

		public List<CCondition.ENegativeCondition> ReplaceNegativeConditions;

		public List<CCondition.EPositiveCondition> ReplacePositiveConditions;

		public List<CCondition.ENegativeCondition> ReplaceWithNegativeConditions;

		public List<CCondition.EPositiveCondition> ReplaceWithPositiveConditions;

		public List<ElementInfusionBoardManager.EElement> OnAttackInfuse;

		public List<EPropType> ObstacleTypes;

		public MiscAbilityData Copy()
		{
			return new MiscAbilityData
			{
				TargetOneEnemyWithAllAttacks = TargetOneEnemyWithAllAttacks,
				TreatAsMelee = TreatAsMelee,
				UseParentRangeType = UseParentRangeType,
				AllTargetsAdjacentToParentMovePath = AllTargetsAdjacentToParentMovePath,
				AllTargetsAdjacentToParentTargets = AllTargetsAdjacentToParentTargets,
				UseParentTiles = UseParentTiles,
				IgnoreParentAreaOfEffect = IgnoreParentAreaOfEffect,
				IgnorePreviousAbilityTargets = ((IgnorePreviousAbilityTargets != null) ? IgnorePreviousAbilityTargets.ToList() : null),
				IgnoreMergedAbilityTargetSelection = IgnoreMergedAbilityTargetSelection,
				UseMergedWithAbilityTiles = UseMergedWithAbilityTiles,
				AddTargetPropertyToStrength = AddTargetPropertyToStrength,
				AddTargetPropertyToStrengthMultiplier = AddTargetPropertyToStrengthMultiplier,
				RestrictMoveRange = RestrictMoveRange,
				AutotriggerAbility = AutotriggerAbility,
				HealPercentageOfHealth = HealPercentageOfHealth,
				ExactRange = ExactRange,
				FilterSpecified = FilterSpecified,
				AttackHasAdvantage = AttackHasAdvantage,
				AttackHasDisadvantage = AttackHasDisadvantage,
				ExhaustSelf = ExhaustSelf,
				GainXPPerXDamageDealt = GainXPPerXDamageDealt,
				SetHPTo = SetHPTo,
				PreventOnlyIfLethal = PreventOnlyIfLethal,
				InfuseIfNotStrong = InfuseIfNotStrong,
				CanTargetInvisible = CanTargetInvisible,
				NotApplyEnemy = NotApplyEnemy,
				CanUndo = CanUndo,
				CanSkip = CanSkip,
				TreatAsNonSubAbility = TreatAsNonSubAbility,
				ReplaceModifiers = ReplaceModifiers?.ToList(),
				ReplaceWithModifier = ReplaceWithModifier,
				ReplaceNegativeConditions = ReplaceNegativeConditions?.ToList(),
				ReplacePositiveConditions = ReplacePositiveConditions?.ToList(),
				ReplaceWithNegativeConditions = ReplaceWithNegativeConditions?.ToList(),
				ReplaceWithPositiveConditions = ReplaceWithPositiveConditions?.ToList(),
				IgnoreMonsterColumnLayout = IgnoreMonsterColumnLayout,
				IgnoreMCLAdjustY = IgnoreMCLAdjustY,
				GapAbove = GapAbove,
				GapBelow = GapBelow,
				IgnoreStun = IgnoreStun,
				HasCondition = HasCondition,
				AreaEffectSymmetrical = AreaEffectSymmetrical,
				RemoveCount = RemoveCount,
				PerformXTimesBasedOn = PerformXTimesBasedOn,
				ConsiderMultiTargetForEnhancements = ConsiderMultiTargetForEnhancements,
				OnAttackInfuse = OnAttackInfuse?.ToList(),
				AlreadyHasConditionDamageInstead = AlreadyHasConditionDamageInstead,
				UseParentTargets = UseParentTargets,
				AlsoTargetSelf = AlsoTargetSelf,
				AlsoTargetAdjacent = AlsoTargetAdjacent,
				ConditionsToRemoveFirst = ConditionsToRemoveFirst?.ToList(),
				ShowPlusX = ShowPlusX,
				TriggeredScenarioModifiers = TriggeredScenarioModifiers?.ToList(),
				TriggerScenarioModifierOnSelfOnly = TriggerScenarioModifierOnSelfOnly,
				BypassImmunity = BypassImmunity,
				InlineSubAbilityOnKilledTargetsOnly = InlineSubAbilityOnKilledTargetsOnly,
				MovePathIndexFilter = MovePathIndexFilter?.Copy(),
				NegativeAbilityIsOptional = NegativeAbilityIsOptional,
				AllowContinueForNullAbility = AllowContinueForNullAbility,
				NoGoldDrop = NoGoldDrop,
				UseOriginalActor = UseOriginalActor,
				ObstacleTypes = ObstacleTypes?.ToList()
			};
		}
	}

	public class ActiveBonusData
	{
		public CActiveBonus.EActiveBonusBehaviourType Behaviour;

		public CActiveBonus.EActiveBonusDurationType Duration;

		public CActiveBonus.EActiveBonusRestrictionType Restriction;

		public CAbilityRequirements Requirements;

		public List<int> Tracker = new List<int>();

		public CAbility.EAttackType AttackType;

		public List<CAbility.EAbilityType> ValidAbilityTypes = new List<CAbility.EAbilityType>();

		public List<CAbilityFilter.EFilterTargetType> ValidTargetTypes = new List<CAbilityFilter.EFilterTargetType>();

		public bool ValidTargetTypesExclusive;

		public bool InvertValidity;

		public List<CCondition.EPositiveCondition> ValidPositiveConditionTypes = new List<CCondition.EPositiveCondition>();

		public List<CCondition.ENegativeCondition> ValidNegativeConditionTypes = new List<CCondition.ENegativeCondition>();

		public int ProcXP;

		public bool StrengthIsScalar;

		public bool AbilityStrengthIsScalar;

		public CAbilityFilterContainer Filter = new CAbilityFilterContainer();

		public bool IsAura;

		public CAbilityFilterContainer AuraFilter = new CAbilityFilterContainer();

		public CAbility.EAbilityTargeting AuraTargeting = CAbility.EAbilityTargeting.Range;

		public CAbility AbilityData;

		public List<ElementInfusionBoardManager.EElement> Consuming = new List<ElementInfusionBoardManager.EElement>();

		public CAbility CostAbility;

		public bool IsToggleBonus;

		public bool ToggleIsOptional = true;

		public bool RestrictPerActor;

		public bool OverrideAsSong;

		public bool IsSingleTargetBonus;

		public int TargetCount;

		public List<CAbilityOverride> ActiveBonusAbilityOverrides = new List<CAbilityOverride>();

		public string ActiveBonusAnimOverload = string.Empty;

		public bool UseTriggerAbilityAsParent;

		public bool GiveAbilityCardToActor;

		public bool StartsRestricted;

		public bool EntireAction;

		public bool SetFilterToCaster;

		public bool OriginalTargetType;

		public Dictionary<string, int> RequiredResources = new Dictionary<string, int>();

		public bool ConsumeResources;

		public (string, string) ListLayoutOverride = (null, null);

		public CActiveBonus.EAuraTriggerAbilityAnimType AuraAnimTriggerAbilityType = CActiveBonus.EAuraTriggerAbilityAnimType.AnimateAuraReceiver;

		public bool TriggerOnCaster;

		public bool CannotCancel;

		public bool EndAllActiveBonusesOnBaseCardSimultaneously;

		public List<int> CancelActiveBonusesOnBaseCardIDs;

		public bool Hidden;

		public List<string> RemoveAllInstancesOfResourcesOnFinish;

		public string GroupID;

		public bool FullCopy;

		public bool NeedsDamage;

		public ActiveBonusData Copy()
		{
			return new ActiveBonusData
			{
				Behaviour = Behaviour,
				Duration = Duration,
				Restriction = Restriction,
				Requirements = Requirements?.Copy(),
				Tracker = Tracker.ToList(),
				AttackType = AttackType,
				ValidAbilityTypes = ValidAbilityTypes.ToList(),
				ValidTargetTypes = ValidTargetTypes.ToList(),
				ValidTargetTypesExclusive = ValidTargetTypesExclusive,
				InvertValidity = InvertValidity,
				ValidPositiveConditionTypes = ValidPositiveConditionTypes.ToList(),
				ValidNegativeConditionTypes = ValidNegativeConditionTypes.ToList(),
				ProcXP = ProcXP,
				StrengthIsScalar = StrengthIsScalar,
				AbilityStrengthIsScalar = AbilityStrengthIsScalar,
				Filter = Filter.Copy(),
				IsAura = IsAura,
				AuraFilter = AuraFilter.Copy(),
				AuraTargeting = AuraTargeting,
				AbilityData = ((AbilityData != null) ? CAbility.CopyAbility(AbilityData, generateNewID: false, fullCopy: true) : null),
				Consuming = Consuming.ToList(),
				CostAbility = ((CostAbility != null) ? CAbility.CopyAbility(CostAbility, generateNewID: false, fullCopy: true) : null),
				IsToggleBonus = IsToggleBonus,
				ToggleIsOptional = ToggleIsOptional,
				RestrictPerActor = RestrictPerActor,
				OverrideAsSong = OverrideAsSong,
				IsSingleTargetBonus = IsSingleTargetBonus,
				TargetCount = TargetCount,
				ActiveBonusAbilityOverrides = ActiveBonusAbilityOverrides.ToList(),
				ActiveBonusAnimOverload = ActiveBonusAnimOverload,
				UseTriggerAbilityAsParent = UseTriggerAbilityAsParent,
				GiveAbilityCardToActor = GiveAbilityCardToActor,
				StartsRestricted = StartsRestricted,
				EntireAction = EntireAction,
				OriginalTargetType = OriginalTargetType,
				SetFilterToCaster = SetFilterToCaster,
				RequiredResources = RequiredResources.ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value),
				ConsumeResources = ConsumeResources,
				ListLayoutOverride = ListLayoutOverride,
				AuraAnimTriggerAbilityType = AuraAnimTriggerAbilityType,
				TriggerOnCaster = TriggerOnCaster,
				CannotCancel = CannotCancel,
				EndAllActiveBonusesOnBaseCardSimultaneously = EndAllActiveBonusesOnBaseCardSimultaneously,
				CancelActiveBonusesOnBaseCardIDs = CancelActiveBonusesOnBaseCardIDs?.ToList(),
				Hidden = Hidden,
				RemoveAllInstancesOfResourcesOnFinish = RemoveAllInstancesOfResourcesOnFinish?.ToList(),
				GroupID = GroupID,
				FullCopy = FullCopy,
				NeedsDamage = NeedsDamage
			};
		}
	}

	public class TrapData
	{
		public string TrapName;

		public int Damage;

		public List<CCondition.ENegativeCondition> Conditions = new List<CCondition.ENegativeCondition>();

		public int AdjacentRange;

		public int AdjacentDamage;

		public List<CCondition.ENegativeCondition> AdjacentConditions = new List<CCondition.ENegativeCondition>();

		public CAbilityFilterContainer AdjacentFilter;

		public int TriggeredXP;

		public CAbilityFilter.EFilterTile PlacementTileFilter = CAbilityFilter.EFilterTile.EmptyHex;

		public TrapData Copy()
		{
			return new TrapData
			{
				TrapName = TrapName,
				Damage = Damage,
				Conditions = Conditions.ToList(),
				AdjacentRange = AdjacentRange,
				AdjacentDamage = AdjacentDamage,
				AdjacentConditions = AdjacentConditions.ToList(),
				AdjacentFilter = AdjacentFilter,
				TriggeredXP = TriggeredXP,
				PlacementTileFilter = PlacementTileFilter
			};
		}
	}

	[Serializable]
	public class StatIsBasedOnXData : ISerializable
	{
		public CAbility.EAbilityStatType AbilityStatType;

		public EMonsterBaseStats BaseStatType;

		public CAbility.EStatIsBasedOnXType BasedOn;

		public CAbility.EStatIsBasedOnXRoundingType RoundingType = CAbility.EStatIsBasedOnXRoundingType.RoundOff;

		public bool AddTo;

		public float Multiplier = 1f;

		public float SecondVariable = 1f;

		public CAbilityFilterContainer Filter;

		public int AddedStat;

		public bool IncludeUnrevealed;

		public int MinValue = int.MinValue;

		public int MaxValue = int.MaxValue;

		public List<string> CheckGUIDs;

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AbilityStatType", AbilityStatType);
			info.AddValue("BaseStatType", BaseStatType);
			info.AddValue("BasedOn", BasedOn);
			info.AddValue("RoundingType", RoundingType);
			info.AddValue("AddTo", AddTo);
			info.AddValue("Multiplier", Multiplier);
			info.AddValue("SecondVariable", SecondVariable);
			info.AddValue("Filter", Filter);
			info.AddValue("AddedStat", AddedStat);
			info.AddValue("IncludeUnrevealed", IncludeUnrevealed);
			info.AddValue("MinValue", MinValue);
			info.AddValue("MaxValue", MaxValue);
			info.AddValue("CheckGUIDs", CheckGUIDs);
		}

		public StatIsBasedOnXData(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					switch (current.Name)
					{
					case "AbilityStatType":
						AbilityStatType = (CAbility.EAbilityStatType)info.GetValue("AbilityStatType", typeof(CAbility.EAbilityStatType));
						break;
					case "BaseStatType":
						BaseStatType = (EMonsterBaseStats)info.GetValue("BaseStatType", typeof(EMonsterBaseStats));
						break;
					case "BasedOn":
						BasedOn = (CAbility.EStatIsBasedOnXType)info.GetValue("BasedOn", typeof(CAbility.EStatIsBasedOnXType));
						break;
					case "RoundingType":
						RoundingType = (CAbility.EStatIsBasedOnXRoundingType)info.GetValue("RoundingType", typeof(CAbility.EStatIsBasedOnXRoundingType));
						break;
					case "AddTo":
						AddTo = info.GetBoolean("AddTo");
						break;
					case "Multiplier":
						Multiplier = info.GetSingle("Multiplier");
						break;
					case "SecondVariable":
						SecondVariable = info.GetSingle("SecondVariable");
						break;
					case "Filter":
						Filter = (CAbilityFilterContainer)info.GetValue("Filter", typeof(CAbilityFilterContainer));
						break;
					case "AddedStat":
						AddedStat = info.GetInt32("AddedStat");
						break;
					case "IncludeUnrevealed":
						IncludeUnrevealed = info.GetBoolean("IncludeUnrevealed");
						break;
					case "MinValue":
						MinValue = info.GetInt32("MinValue");
						break;
					case "MaxValue":
						MaxValue = info.GetInt32("MaxValue");
						break;
					case "CheckGUIDs":
						CheckGUIDs = (List<string>)info.GetValue("CheckGUIDs", typeof(List<string>));
						break;
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize StatIsBasedOnXData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public StatIsBasedOnXData()
		{
		}

		public StatIsBasedOnXData Copy()
		{
			return new StatIsBasedOnXData
			{
				AbilityStatType = AbilityStatType,
				BaseStatType = BaseStatType,
				BasedOn = BasedOn,
				RoundingType = RoundingType,
				AddTo = AddTo,
				Multiplier = Multiplier,
				SecondVariable = SecondVariable,
				Filter = Filter?.Copy(),
				AddedStat = AddedStat,
				IncludeUnrevealed = IncludeUnrevealed,
				MinValue = MinValue,
				MaxValue = MaxValue,
				CheckGUIDs = CheckGUIDs?.ToList()
			};
		}

		public static string GetExpressionStringForType(CAbility.EStatIsBasedOnXType type)
		{
			return type switch
			{
				CAbility.EStatIsBasedOnXType.None => "None", 
				CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount => "X * C", 
				CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount => "X + C", 
				CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel => "X + (C * L)", 
				CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX => "(X + L) * C", 
				CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel => "X + (Y * L)", 
				CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel => "X + C + L", 
				CAbility.EStatIsBasedOnXType.XPlusLevel => "X + L", 
				CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters => "L + (X * C)", 
				CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY => "X + (L * C / Y)", 
				CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY => "(X * C) + L - Y", 
				CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound => "((X * C) + L - Y) * R", 
				CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX => "C + (L / X)", 
				CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY => "X + C + (L * Y)", 
				CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY => "(X * C) - Y", 
				CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY => "(X + L) * (C / Y)", 
				CAbility.EStatIsBasedOnXType.LevelTimesXPlusY => "(L * X) + Y", 
				_ => "UNSUPPORTED TYPE: " + type, 
			};
		}

		public StatIsBasedOnXData(StatIsBasedOnXData state, ReferenceDictionary references)
		{
			AbilityStatType = state.AbilityStatType;
			BaseStatType = state.BaseStatType;
			BasedOn = state.BasedOn;
			RoundingType = state.RoundingType;
			AddTo = state.AddTo;
			Multiplier = state.Multiplier;
			SecondVariable = state.SecondVariable;
			Filter = references.Get(state.Filter);
			if (Filter == null && state.Filter != null)
			{
				Filter = new CAbilityFilterContainer(state.Filter, references);
				references.Add(state.Filter, Filter);
			}
			AddedStat = state.AddedStat;
			IncludeUnrevealed = state.IncludeUnrevealed;
			MinValue = state.MinValue;
			MaxValue = state.MaxValue;
			CheckGUIDs = references.Get(state.CheckGUIDs);
			if (CheckGUIDs == null && state.CheckGUIDs != null)
			{
				CheckGUIDs = new List<string>();
				for (int i = 0; i < state.CheckGUIDs.Count; i++)
				{
					string item = state.CheckGUIDs[i];
					CheckGUIDs.Add(item);
				}
				references.Add(state.CheckGUIDs, CheckGUIDs);
			}
		}
	}

	private const string SpecialBaseStatString = "SpecialBaseStat";

	public static bool GetAction(Mapping actionMapping, int cardID, string filename, DiscardType discardType, out CAction action, out List<AbilityConsume> consumes, bool isMonster = false)
	{
		action = null;
		bool flag = true;
		List<CAbility> list = new List<CAbility>();
		List<CActionAugmentation> list2 = new List<CActionAugmentation>();
		consumes = new List<AbilityConsume>();
		List<ElementInfusionBoardManager.EElement> list3 = new List<ElementInfusionBoardManager.EElement>();
		int actionXP = 0;
		foreach (MappingEntry entry in actionMapping.Entries)
		{
			string text = entry.Key.ToString();
			switch (text)
			{
			case "Abilities":
			{
				if (CardProcessingShared.GetAbilities(entry, cardID, isMonster, filename, out var abilities))
				{
					list.AddRange(abilities);
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Consumes":
			{
				if (YMLShared.GetMapping(entry, filename, out var mapping))
				{
					foreach (MappingEntry entry2 in mapping.Entries)
					{
						if (YMLShared.GetMapping(entry2, filename, out var mapping2))
						{
							if (AbilityConsume.CreateAbilityConsume(mapping2, entry2.Key.ToString(), cardID, filename, isMonster, list, out var abilityConsume))
							{
								consumes.Add(abilityConsume);
								list2.Add(abilityConsume.ConsumeData);
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
					}
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Infuse":
			{
				if (CardProcessingShared.GetElements(entry.Value, "Infuse", filename, out var elements))
				{
					list3.AddRange(elements);
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "XP":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "XP", filename, out var value))
				{
					actionXP = value;
				}
				else
				{
					flag = false;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry under Data: " + text + ". File:\n" + filename);
				flag = false;
				break;
			}
		}
		if (list.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Action has no abilities.  Filename: " + filename);
			flag = false;
		}
		if (flag)
		{
			action = new CAction(list3, list2, list, actionXP, CardProcessingShared.GetCardPile(discardType));
		}
		return flag;
	}

	public static bool ProcessAbilityEntry(string filename, Mapping abilityMapping, string name, int cardID, bool isMonster, out CAbility ability, bool isSubAbility = false, bool isConsumeAbility = false, CAbilityFilterContainer parentFilter = null, int? parentStrength = null, int? parentRange = null, int? parentNumberOfTargets = null, string parentAreaEffectYMLString = null, string parentAreaEffectLayoutOverrideYMLString = null, CAbility.EAbilityTargeting? parentTargeting = null, CAreaEffect parentAreaEffect = null, bool? parentIsTargetedAbility = null, bool isMergedAbility = false)
	{
		ability = null;
		try
		{
			CAbility.EAbilityType? abilityType = null;
			string parentName = string.Empty;
			string animationOverload = string.Empty;
			List<CAbility> subAbilities = new List<CAbility>();
			bool? allTargetsOnMovePath = false;
			bool? allTargetsOnMovePathSameStartAndEnd = false;
			bool? allTargetsOnAttackPath = false;
			XpPerTargetData xpPerTargetData = new XpPerTargetData();
			ActiveBonusData activeBonusData = new ActiveBonusData();
			ActiveBonusLayout activeBonusYML = null;
			CAbility addActiveBonusAbility = null;
			float? spawnDelay = 0f;
			List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
			CAbilityRequirements startAbilityRequirements = new CAbilityRequirements();
			int? abilityXP = 0;
			MiscAbilityData miscAbilityData = new MiscAbilityData();
			bool? skipAnim = false;
			bool? isInlineSubAbility = false;
			bool? targetActorWithTrapEffects = false;
			int? targetActorWithTrapEffectRange = 0;
			CAugment augment = null;
			CSong song = null;
			CDoom doom = null;
			List<CAbility> mergedAbilities = null;
			List<StatIsBasedOnXData> statIsBasedOnXEntries = null;
			bool? useSpecialBaseStat = false;
			Dictionary<string, int> resourcesToAddOnAbilityEnd = null;
			Dictionary<string, int> resourcesToTakeFromTargets = null;
			Dictionary<string, int> resourcesToGiveToTargets = null;
			CAbilityFilterContainer filter = (filter = parentFilter?.Copy());
			int? strength = parentStrength ?? new int?(0);
			int? range = parentRange ?? new int?((!isMonster) ? 1 : 0);
			int? numberOfTargets = parentNumberOfTargets ?? new int?(0);
			bool numberOfTargetsSet = false;
			string areaEffectYMLString = ((parentAreaEffectYMLString != null) ? parentAreaEffectYMLString : string.Empty);
			string areaEffectLayoutOverrideYMLString = ((parentAreaEffectLayoutOverrideYMLString != null) ? parentAreaEffectLayoutOverrideYMLString : string.Empty);
			CAbility.EAbilityTargeting? targeting = parentTargeting ?? new CAbility.EAbilityTargeting?(CAbility.EAbilityTargeting.Range);
			CAreaEffect areaEffect = parentAreaEffect?.Copy();
			bool? isTargetedAbility = !parentIsTargetedAbility.HasValue || parentIsTargetedAbility.Value;
			bool? jump = false;
			bool? fly = false;
			bool? ignoreDifficultTerrain = false;
			bool? ignoreHazardousTerrain = false;
			bool? ignoreBlockedTileMoveCost = false;
			bool? carryOtherActorsOnHex = false;
			CAbilityMove.EMoveRestrictionType? moveRestrictionType = CAbilityMove.EMoveRestrictionType.None;
			CAbilityPush.EAdditionalPushEffect? additionalPushEffect = CAbilityPush.EAdditionalPushEffect.None;
			int? additionalPushEffectDamage = 0;
			int? additionalPushEffectXP = 0;
			CAIFocusOverrideDetails aiFocusOverride = null;
			CAbilityPull.EPullType? pullType = CAbilityPull.EPullType.PullTowardsActor;
			CAbilityLoot.LootData lootData = null;
			string propName = string.Empty;
			int? moveObstacleRange = 0;
			string moveObstacleAnimOverload = string.Empty;
			TrapData trapData = null;
			int? pierce = 0;
			bool? multiPassAttack = !isSubAbility && !isMergedAbility;
			bool? chainAttack = false;
			int? chainAttackRange = 0;
			int? chainAttackDamageReduction = 0;
			int? damageSelfBeforeAttack = 0;
			miscAbilityData.TargetOneEnemyWithAllAttacks = false;
			List<CAttackEffect> attackEffects = new List<CAttackEffect>();
			CAbilityHeal.HealAbilityData healAbilityData = new CAbilityHeal.HealAbilityData();
			int? retaliateRange = 1;
			List<string> summons = null;
			bool? attackSourcesOnly = false;
			List<ElementInfusionBoardManager.EElement> elementsToInfuse = new List<ElementInfusionBoardManager.EElement>();
			bool? showElementPicker = false;
			CAbilityControlActor.ControlActorAbilityData controlActorAbilityData = null;
			CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceAbilityData = null;
			List<CItem.EItemSlot> slotsToRefresh = new List<CItem.EItemSlot>();
			List<CItem.EItemSlotState> slotStatesToRefresh = new List<CItem.EItemSlotState>();
			CAbility.EAttackType? overrideAugmentAttackType = CAbility.EAttackType.None;
			List<CAbility> chooseAbilities = new List<CAbility>();
			List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter = new List<CAbility.EAbilityType>();
			CAbility forgoTopActionAbility = null;
			CAbility forgoBottomActionAbility = null;
			CAbilityExtraTurn.EExtraTurnType? extraTurnType = CAbilityExtraTurn.EExtraTurnType.None;
			CAbilityFilterContainer swapAbilityFirstTargetFilter = null;
			CAbilityFilterContainer swapAbilitySecondTargetFilter = null;
			List<int> supplyCardsToGive = new List<int>();
			CAbilityTeleport.TeleportData teleportData = null;
			List<CAbility.EAbilityType> immunityToAbilityTypes = new List<CAbility.EAbilityType>();
			List<CAbility.EAttackType> immuneToAttackTypes = new List<CAbility.EAttackType>();
			List<string> addModifiers = new List<string>();
			CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = new CAbilityDestroyObstacle.DestroyObstacleData();
			ECharacter changeCharacterModel = ECharacter.None;
			CAbilityFilter.EFilterTile tileFilter = CAbilityFilter.EFilterTile.None;
			CAbilityDisableCardAction.DisableCardActionData disableCardActionData = new CAbilityDisableCardAction.DisableCardActionData();
			List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
			bool? addAttackBaseStat = false;
			bool? strengthIsBase = false;
			bool? rangeIsBase = false;
			bool? targetIsBase = false;
			string abilityText = string.Empty;
			bool? abilityTextOnly = false;
			bool? showRange = true;
			bool? showTarget = true;
			bool? showArea = true;
			bool? onDeath = false;
			bool? rangeAtLeastOne = false;
			bool? targetAtLeastOne = false;
			bool? removeConditionsOverride = false;
			List<CCondition.EPositiveCondition> positiveConditions = new List<CCondition.EPositiveCondition>();
			List<CCondition.ENegativeCondition> negativeConditions = new List<CCondition.ENegativeCondition>();
			int? conditionDuration = 1;
			EConditionDecTrigger? conditionDecTrigger = EConditionDecTrigger.Turns;
			string previewEffectId = null;
			string previewEffectText = null;
			string helpBoxTooltipLocKey = null;
			if (!ParseAbilityProperties(abilityMapping, name, cardID, isMonster, isAbilityOverride: false, ref isSubAbility, ref parentName, ref abilityType, ref propName, ref trapData, ref abilityEnhancements, ref strength, ref useSpecialBaseStat, ref range, ref numberOfTargets, ref numberOfTargetsSet, ref filter, ref animationOverload, ref subAbilities, ref areaEffect, ref attackSourcesOnly, ref jump, ref fly, ref ignoreDifficultTerrain, ref ignoreHazardousTerrain, ref ignoreBlockedTileMoveCost, ref aiFocusOverride, ref carryOtherActorsOnHex, ref moveRestrictionType, ref activeBonusData, ref activeBonusYML, ref areaEffectYMLString, ref areaEffectLayoutOverrideYMLString, ref positiveConditions, ref negativeConditions, ref multiPassAttack, ref chainAttack, ref chainAttackRange, ref chainAttackDamageReduction, ref damageSelfBeforeAttack, ref addAttackBaseStat, ref strengthIsBase, ref rangeIsBase, ref targetIsBase, ref rangeAtLeastOne, ref targetAtLeastOne, ref removeConditionsOverride, ref abilityText, ref abilityTextOnly, ref showRange, ref showTarget, ref showArea, ref onDeath, ref allTargetsOnMovePath, ref allTargetsOnMovePathSameStartAndEnd, ref allTargetsOnAttackPath, ref summons, ref xpPerTargetData, ref targeting, ref elementsToInfuse, ref showElementPicker, ref pierce, ref retaliateRange, ref isTargetedAbility, ref spawnDelay, ref pullType, ref additionalPushEffect, ref additionalPushEffectDamage, ref additionalPushEffectXP, ref lootData, ref conditionalOverrides, ref startAbilityRequirements, ref abilityXP, ref miscAbilityData, ref skipAnim, ref conditionDuration, ref conditionDecTrigger, ref isInlineSubAbility, ref augment, ref song, ref doom, ref attackEffects, ref controlActorAbilityData, ref changeAllegianceAbilityData, ref mergedAbilities, ref targetActorWithTrapEffects, ref targetActorWithTrapEffectRange, ref statIsBasedOnXEntries, ref moveObstacleRange, ref moveObstacleAnimOverload, ref slotsToRefresh, ref slotStatesToRefresh, ref addActiveBonusAbility, ref overrideAugmentAttackType, ref chooseAbilities, ref recoverCardsAbilityTypeFilter, ref forgoTopActionAbility, ref forgoBottomActionAbility, ref extraTurnType, ref swapAbilityFirstTargetFilter, ref swapAbilitySecondTargetFilter, ref supplyCardsToGive, ref teleportData, ref immunityToAbilityTypes, ref immuneToAttackTypes, ref addModifiers, ref healAbilityData, ref resourcesToAddOnAbilityEnd, ref resourcesToTakeFromTargets, ref resourcesToGiveToTargets, ref destroyObstacleData, ref changeCharacterModel, ref tileFilter, ref disableCardActionData, ref previewEffectId, ref previewEffectText, ref helpBoxTooltipLocKey, filename))
			{
				return false;
			}
			if (filter == null)
			{
				filter = CAbilityFilterContainer.CreateDefaultFilter();
			}
			bool flag = true;
			if (name.Length == 0)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Ability Name is blank.  The Ability name must be specified in the YML.  File:\n" + filename);
			}
			else if (!abilityType.HasValue)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Ability Type has not been set.  All Abilities must have a type.  File:\n" + filename);
			}
			else if (abilityType == CAbility.EAbilityType.Trap)
			{
				if (trapData == null)
				{
					flag = false;
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid YML for Trap ability.  No TrapData was specified.  File: " + filename);
				}
			}
			else if (abilityType == CAbility.EAbilityType.Summon && activeBonusData.Duration != CActiveBonus.EActiveBonusDurationType.Summon)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid YML for Summon ability.  ActiveBonusDuration must be 'Summon'");
			}
			else if (abilityType == CAbility.EAbilityType.AddDoom && doom == null)
			{
				flag = false;
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid YML for Add Doom ability.  Doom cannot be null");
			}
			if (flag)
			{
				numberOfTargets = (isMonster ? numberOfTargets : (numberOfTargetsSet ? numberOfTargets : new int?(1)));
				CAbility.EAbilityType value = abilityType.Value;
				int value2 = strength.Value;
				bool value3 = useSpecialBaseStat.Value;
				int value4 = range.Value;
				int value5 = numberOfTargets.Value;
				CAbilityFilterContainer abilityFilter = filter;
				string animationOverload2 = animationOverload;
				List<CAbility> subAbilities2 = subAbilities;
				CAreaEffect areaEffect2 = areaEffect;
				bool value6 = attackSourcesOnly.Value;
				bool value7 = jump.Value;
				bool value8 = fly.Value;
				bool value9 = ignoreDifficultTerrain.Value;
				bool value10 = ignoreHazardousTerrain.Value;
				bool value11 = ignoreBlockedTileMoveCost.Value;
				bool value12 = carryOtherActorsOnHex.Value;
				CAIFocusOverrideDetails aiFocusOverride2 = aiFocusOverride;
				CAbilityMove.EMoveRestrictionType value13 = moveRestrictionType.Value;
				ActiveBonusData activeBonusData2 = activeBonusData;
				string areaEffectYMLString2 = areaEffectYMLString;
				string areaEffectLayoutOverrideYMLString2 = areaEffectLayoutOverrideYMLString;
				List<CEnhancement> abilityEnhancements2 = abilityEnhancements;
				List<CCondition.EPositiveCondition> positiveConditions2 = positiveConditions;
				List<CCondition.ENegativeCondition> negativeConditions2 = negativeConditions;
				bool value14 = multiPassAttack.Value;
				bool value15 = chainAttack.Value;
				int value16 = chainAttackRange.Value;
				int value17 = chainAttackDamageReduction.Value;
				int value18 = damageSelfBeforeAttack.Value;
				bool value19 = addAttackBaseStat.Value;
				bool value20 = strengthIsBase.Value;
				bool value21 = rangeIsBase.Value;
				bool value22 = targetIsBase.Value;
				string text = abilityText;
				bool value23 = abilityTextOnly.Value;
				bool value24 = showRange.Value;
				bool value25 = showTarget.Value;
				bool value26 = showArea.Value;
				bool value27 = onDeath.Value;
				bool value28 = allTargetsOnMovePath.Value;
				bool value29 = allTargetsOnMovePathSameStartAndEnd.Value;
				bool value30 = allTargetsOnAttackPath.Value;
				List<string> summons2 = summons;
				XpPerTargetData xpPerTargetData2 = xpPerTargetData;
				CAbility.EAbilityTargeting value31 = targeting.Value;
				List<ElementInfusionBoardManager.EElement> elementsToInfuse2 = elementsToInfuse;
				bool value32 = showElementPicker.Value;
				string propName2 = propName;
				TrapData trapData2 = trapData;
				bool isSubAbility2 = isSubAbility;
				bool value33 = isInlineSubAbility.Value;
				int value34 = pierce.Value;
				int value35 = retaliateRange.Value;
				bool value36 = isTargetedAbility.Value;
				float value37 = spawnDelay.Value;
				CAbilityPull.EPullType value38 = pullType.Value;
				CAbilityPush.EAdditionalPushEffect value39 = additionalPushEffect.Value;
				int value40 = additionalPushEffectDamage.Value;
				int value41 = additionalPushEffectXP.Value;
				CAbilityLoot.LootData lootData2 = lootData;
				List<CConditionalOverride> conditionalOverrides2 = conditionalOverrides;
				CAbilityRequirements startAbilityConditions = startAbilityRequirements;
				int value42 = abilityXP.Value;
				string parentName2 = parentName;
				MiscAbilityData miscAbilityData2 = miscAbilityData;
				bool value43 = skipAnim.Value;
				int value44 = conditionDuration.Value;
				EConditionDecTrigger value45 = conditionDecTrigger.Value;
				CAugment augment2 = augment;
				CSong song2 = song;
				CDoom doom2 = doom;
				List<CAttackEffect> attackEffects2 = attackEffects;
				CAbilityControlActor.ControlActorAbilityData controlActorData = controlActorAbilityData;
				CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = changeAllegianceAbilityData;
				bool isMergedAbility2 = isMergedAbility;
				List<CAbility> mergedAbilities2 = mergedAbilities;
				bool value46 = targetActorWithTrapEffects.Value;
				int value47 = targetActorWithTrapEffectRange.Value;
				List<StatIsBasedOnXData> statIsBasedOnXEntries2 = statIsBasedOnXEntries;
				int value48 = moveObstacleRange.Value;
				string moveObstacleAnimOverload2 = moveObstacleAnimOverload;
				List<CItem.EItemSlot> slotsToRefresh2 = slotsToRefresh;
				List<CItem.EItemSlotState> slotStatesToRefresh2 = slotStatesToRefresh;
				CAbility addActiveBonusAbility2 = addActiveBonusAbility;
				CAbility.EAttackType value49 = overrideAugmentAttackType.Value;
				List<CAbility> chooseAbilities2 = chooseAbilities;
				List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter2 = recoverCardsAbilityTypeFilter;
				CAbility forgoTopActionAbility2 = forgoTopActionAbility;
				CAbility forgoBottomActionAbility2 = forgoBottomActionAbility;
				CAbilityExtraTurn.EExtraTurnType value50 = extraTurnType.Value;
				CAbilityFilterContainer swapAbilityFirstTargetFilter2 = swapAbilityFirstTargetFilter;
				CAbilityFilterContainer swapAbilitySecondTargetFilter2 = swapAbilitySecondTargetFilter;
				List<int> supplyCardsToGive2 = supplyCardsToGive;
				CAbilityTeleport.TeleportData teleportData2 = teleportData;
				List<CAbility.EAbilityType> immunityToAbilityTypes2 = immunityToAbilityTypes;
				List<CAbility.EAttackType> immuneToAttackTypes2 = immuneToAttackTypes;
				List<string> addModifiers2 = addModifiers;
				CAbilityHeal.HealAbilityData healData = healAbilityData;
				Dictionary<string, int> resourcesToAddOnAbilityEnd2 = resourcesToAddOnAbilityEnd;
				Dictionary<string, int> resourcesToGiveToTargets2 = resourcesToGiveToTargets;
				Dictionary<string, int> resourcesToTakeFromTargets2 = resourcesToTakeFromTargets;
				CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData2 = destroyObstacleData;
				ECharacter changeCharacterModel2 = changeCharacterModel;
				CAbilityFilter.EFilterTile tileFilter2 = tileFilter;
				CAbilityDisableCardAction.DisableCardActionData disableCardActionData2 = disableCardActionData;
				string previewEffectId2 = previewEffectId;
				string previewEffectText2 = previewEffectText;
				string helpBoxTooltipLocKey2 = helpBoxTooltipLocKey;
				ability = CAbility.CreateAbility(value, value2, value3, value4, value5, abilityFilter, animationOverload2, subAbilities2, areaEffect2, value6, value7, value8, value9, value10, value11, value12, aiFocusOverride2, value13, activeBonusData2, null, areaEffectYMLString2, areaEffectLayoutOverrideYMLString2, name, abilityEnhancements2, positiveConditions2, negativeConditions2, value14, value15, value16, value17, value18, value19, value20, value21, value22, text, value23, value24, value25, value26, value27, isConsumeAbility, value28, value29, value30, summons2, xpPerTargetData2, value31, elementsToInfuse2, isMonster, propName2, trapData2, isSubAbility2, value33, value34, value35, value36, value37, value38, value39, value40, value41, lootData2, conditionalOverrides2, startAbilityConditions, value42, parentName2, miscAbilityData2, value43, value44, value45, augment2, song2, doom2, controlActorData, changeAllegianceData, attackEffects2, value46, value47, isMergedAbility2, mergedAbilities2, statIsBasedOnXEntries2, value48, moveObstacleAnimOverload2, slotsToRefresh2, slotStatesToRefresh2, addActiveBonusAbility2, value49, chooseAbilities2, recoverCardsAbilityTypeFilter2, forgoTopActionAbility2, forgoBottomActionAbility2, value50, swapAbilityFirstTargetFilter2, swapAbilitySecondTargetFilter2, supplyCardsToGive2, teleportData2, immunityToAbilityTypes2, immuneToAttackTypes2, addModifiers2, healData, resourcesToAddOnAbilityEnd2, resourcesToTakeFromTargets2, resourcesToGiveToTargets2, destroyObstacleData2, changeCharacterModel2, tileFilter2, disableCardActionData2, isDefault: false, isItemAbility: false, value32, isModifierAbility: false, null, previewEffectId2, previewEffectText2, null, helpBoxTooltipLocKey2);
				return true;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability entry " + name + ".  File:\n" + filename + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out int enhancementSlots, out int pierce, out int pierceEnhancements, bool ignoreAmount = false)
	{
		bool jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		ElementInfusionBoardManager.EElement? infuse;
		EConditionDecTrigger conditionDecTrigger;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out pierceEnhancements, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, out pierce, out conditionDecTrigger, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out int enhancementSlots, bool ignoreAmount = false)
	{
		bool jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		ElementInfusionBoardManager.EElement? infuse;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out EConditionDecTrigger conditonDecTrigger, out int enhancementSlots, bool ignoreAmount = false)
	{
		int pierceEnhancements;
		bool jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		ElementInfusionBoardManager.EElement? infuse;
		int pierce;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out pierceEnhancements, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, out pierce, out conditonDecTrigger, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out int enhancementSlots, out bool jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, out bool ignoreBlockedTileMoveCost, bool ignoreAmount = false)
	{
		ElementInfusionBoardManager.EElement? infuse;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out int enhancementSlots, out ElementInfusionBoardManager.EElement? infuse, bool ignoreAmount = false)
	{
		bool jump;
		bool fly;
		bool ignoreDifficultTerrain;
		bool ignoreHazardousTerrain;
		bool ignoreBlockedTileMoveCost;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out int enhancementSlots, out bool jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, out bool ignoreBlockedTileMoveCost, out ElementInfusionBoardManager.EElement? infuse, bool ignoreAmount = false)
	{
		int pierceEnhancements;
		int pierce;
		EConditionDecTrigger conditionDecTrigger;
		return GetEnhancedAbilityValues(di, entryName, filename, out amount, out useSpecialBaseState, out enhancementSlots, out pierceEnhancements, out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, out ignoreBlockedTileMoveCost, out infuse, out pierce, out conditionDecTrigger, ignoreAmount);
	}

	public static bool GetEnhancedAbilityValues(DataItem di, string entryName, string filename, out int amount, out bool useSpecialBaseState, out int enhancementSlots, out int pierceEnhancements, out bool jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, out bool ignoreBlockedTileMoveCost, out ElementInfusionBoardManager.EElement? infuse, out int pierce, out EConditionDecTrigger conditionDecTrigger, bool ignoreAmount = false)
	{
		amount = 0;
		useSpecialBaseState = false;
		enhancementSlots = 0;
		pierce = 0;
		pierceEnhancements = 0;
		infuse = null;
		jump = false;
		fly = false;
		ignoreDifficultTerrain = false;
		ignoreHazardousTerrain = false;
		ignoreBlockedTileMoveCost = false;
		conditionDecTrigger = EConditionDecTrigger.None;
		if (di is Mapping)
		{
			bool flag = false;
			foreach (MappingEntry abilityPropertyEntry in (di as Mapping).Entries)
			{
				switch (abilityPropertyEntry.Key.ToString())
				{
				case "Duration":
				case "Amount":
				{
					int value10;
					string value11;
					if (abilityPropertyEntry.Value.ToString() == "All")
					{
						amount = 1073741823;
						flag = true;
					}
					else if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Amount", filename, out value10, suppressErrors: true))
					{
						amount = value10;
						flag = true;
					}
					else if (YMLShared.GetStringPropertyValue(abilityPropertyEntry.Value, entryName + "/Amount", filename, out value11) && value11 == "SpecialBaseStat")
					{
						useSpecialBaseState = true;
					}
					break;
				}
				case "Enhancements":
				{
					if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Enhancements", filename, out var value3))
					{
						enhancementSlots = value3;
					}
					break;
				}
				case "PierceEnhancements":
				{
					if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Enhancements", filename, out var value8))
					{
						pierceEnhancements = value8;
					}
					break;
				}
				case "Jump":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/Jump", filename, out var value))
					{
						jump = value;
					}
					break;
				}
				case "Fly":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/Fly", filename, out var value9))
					{
						fly = value9;
					}
					break;
				}
				case "IgnoreDifficultTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/IgnoreDifficultTerrain", filename, out var value7))
					{
						ignoreDifficultTerrain = value7;
					}
					break;
				}
				case "IgnoreHazardousTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/IgnoreHazardousTerrain", filename, out var value2))
					{
						ignoreHazardousTerrain = value2;
					}
					break;
				}
				case "IgnoreBlockedTileMoveCost":
				{
					if (YMLShared.GetBoolPropertyValue(abilityPropertyEntry.Value, entryName + "/IgnoreBlockedTileMoveCost", filename, out var value12))
					{
						ignoreBlockedTileMoveCost = value12;
					}
					break;
				}
				case "Pierce":
				{
					string value6;
					if (YMLShared.GetIntPropertyValue(abilityPropertyEntry.Value, entryName + "/Pierce", filename, out var value5, suppressErrors: true))
					{
						pierce = value5;
					}
					else if (YMLShared.GetStringPropertyValue(abilityPropertyEntry.Value, entryName + "/Pierce", filename, out value6) && value6 == "All")
					{
						pierce = 99999;
					}
					break;
				}
				case "Infuse":
					try
					{
						ElementInfusionBoardManager.EElement value4 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == abilityPropertyEntry.Value.ToString());
						infuse = value4;
					}
					catch (Exception ex)
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Element " + abilityPropertyEntry.Value?.ToString() + " under entry " + entryName + ".  File:\n" + filename + "\n" + ex.Message + "\n" + ex.StackTrace);
						infuse = null;
					}
					break;
				case "DecrementTrigger":
				{
					if (YMLShared.GetStringPropertyValue(abilityPropertyEntry.Value, entryName + "/DecrementTrigger", filename, out var decValue))
					{
						EConditionDecTrigger eConditionDecTrigger = CAbilityCondition.ConditionDecTriggers.SingleOrDefault((EConditionDecTrigger s) => s.ToString() == decValue);
						if (eConditionDecTrigger != EConditionDecTrigger.None)
						{
							conditionDecTrigger = eConditionDecTrigger;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DecrementTrigger '" + decValue + "' in file " + filename);
						}
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + abilityPropertyEntry.Key.ToString() + " under " + entryName + ". File:\n" + filename);
					break;
				}
			}
			if (!flag && !ignoreAmount)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Missing entry 'Amount' under " + entryName + ". File:\n" + filename);
				return false;
			}
		}
		else
		{
			if (!(di is Scalar))
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Entry under ability " + entryName + " is invalid.  Entries under " + entryName + " must be a Mapping or Scalar. File:\n" + filename);
				return false;
			}
			if (!ignoreAmount)
			{
				if (YMLShared.GetIntPropertyValue(di, "Ability/" + entryName, filename, out var value13, suppressErrors: true))
				{
					amount = value13;
				}
				else
				{
					if (!YMLShared.GetStringPropertyValue(di, entryName + "/Amount", filename, out var value14))
					{
						return false;
					}
					if (value14 == "SpecialBaseStat")
					{
						useSpecialBaseState = true;
					}
				}
			}
		}
		return true;
	}

	public static bool ParseAbilityProperties(Mapping abilityMapping, string name, int cardID, bool isMonster, bool isAbilityOverride, ref bool isSubAbility, ref string parentName, ref CAbility.EAbilityType? abilityType, ref string propName, ref TrapData trapData, ref List<CEnhancement> abilityEnhancements, ref int? strength, ref bool? useSpecialBaseStat, ref int? range, ref int? numberOfTargets, ref bool numberOfTargetsSet, ref CAbilityFilterContainer filter, ref string animationOverload, ref List<CAbility> subAbilities, ref CAreaEffect areaEffect, ref bool? attackSourcesOnly, ref bool? jump, ref bool? fly, ref bool? ignoreDifficultTerrain, ref bool? ignoreHazardousTerrain, ref bool? ignoreBlockedTileMoveCost, ref CAIFocusOverrideDetails aiFocusOverride, ref bool? carryOtherActorsOnHex, ref CAbilityMove.EMoveRestrictionType? moveRestrictionType, ref ActiveBonusData activeBonusData, ref ActiveBonusLayout activeBonusYML, ref string areaEffectYMLString, ref string areaEffectLayoutOverrideYMLString, ref List<CCondition.EPositiveCondition> positiveConditions, ref List<CCondition.ENegativeCondition> negativeConditions, ref bool? multiPassAttack, ref bool? chainAttack, ref int? chainAttackRange, ref int? chainAttackDamageReduction, ref int? damageSelfBeforeAttack, ref bool? addAttackBaseStat, ref bool? strengthIsBase, ref bool? rangeIsBase, ref bool? targetIsBase, ref bool? rangeAtLeastOne, ref bool? targetAtLeastOne, ref bool? removeConditionsOverride, ref string abilityText, ref bool? abilityTextOnly, ref bool? showRange, ref bool? showTarget, ref bool? showArea, ref bool? onDeath, ref bool? allTargetsOnMovePath, ref bool? allTargetsOnMovePathSameStartAndEnd, ref bool? allTargetsOnAttackPath, ref List<string> summons, ref XpPerTargetData xpPerTargetData, ref CAbility.EAbilityTargeting? targeting, ref List<ElementInfusionBoardManager.EElement> elementsToInfuse, ref bool? showElementPicker, ref int? pierce, ref int? retaliateRange, ref bool? isTargetedAbility, ref float? spawnDelay, ref CAbilityPull.EPullType? pullType, ref CAbilityPush.EAdditionalPushEffect? additionalPushEffect, ref int? additionalPushEffectDamage, ref int? additionalPushEffectXP, ref CAbilityLoot.LootData lootData, ref List<CConditionalOverride> conditionalOverrides, ref CAbilityRequirements startAbilityRequirements, ref int? abilityXP, ref MiscAbilityData miscAbilityData, ref bool? skipAnim, ref int? conditionDuration, ref EConditionDecTrigger? conditionDecTrigger, ref bool? isInlineSubAbility, ref CAugment augment, ref CSong song, ref CDoom doom, ref List<CAttackEffect> attackEffects, ref CAbilityControlActor.ControlActorAbilityData controlActorAbilityData, ref CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceAbilityData, ref List<CAbility> mergedAbilities, ref bool? targetActorWithTrapEffects, ref int? targetActorWithTrapEffectRange, ref List<StatIsBasedOnXData> statIsBasedOnXEntries, ref int? moveObstacleRange, ref string moveObstacleAnimOverload, ref List<CItem.EItemSlot> slotsToRefresh, ref List<CItem.EItemSlotState> slotStatesToRefresh, ref CAbility addActiveBonusAbility, ref CAbility.EAttackType? overrideAugmentAttackType, ref List<CAbility> chooseAbilities, ref List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter, ref CAbility forgoTopActionAbility, ref CAbility forgoBottomActionAbility, ref CAbilityExtraTurn.EExtraTurnType? extraTurnType, ref CAbilityFilterContainer swapAbilityFirstTargetFilter, ref CAbilityFilterContainer swapAbilitySecondTargetFilter, ref List<int> supplyCardsToGive, ref CAbilityTeleport.TeleportData teleportData, ref List<CAbility.EAbilityType> immunityToAbilityTypes, ref List<CAbility.EAttackType> immuneToAttackTypes, ref List<string> addModifiers, ref CAbilityHeal.HealAbilityData healAbilityData, ref Dictionary<string, int> resourcesToAddOnAbilityEnd, ref Dictionary<string, int> resourcesToTakeFromTargets, ref Dictionary<string, int> resourcesToGiveToTargets, ref CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData, ref ECharacter changeCharacterModel, ref CAbilityFilter.EFilterTile tileFilter, ref CAbilityDisableCardAction.DisableCardActionData disableCardActionData, ref string previewEffectId, ref string previewEffectText, ref string helpBoxTooltipLocKey, string filename)
	{
		try
		{
			bool flag = true;
			Mapping mapping = null;
			CAbilityFilterContainer filter2 = CAbilityFilterContainer.CreateDefaultFilter();
			foreach (MappingEntry entry in abilityMapping.Entries)
			{
				int amount;
				bool useSpecialBaseState2;
				int enhancementSlots;
				bool value2;
				bool fly2;
				bool ignoreDifficultTerrain2;
				bool ignoreHazardousTerrain2;
				bool ignoreBlockedTileMoveCost2;
				string stringValue;
				bool useSpecialBaseState;
				EConditionDecTrigger conditonDecTrigger;
				List<string> values2;
				float value30;
				switch (entry.Key.ToString())
				{
				case "AddActiveBonus":
				{
					abilityType = CAbility.EAbilityType.AddActiveBonus;
					if (!isAbilityOverride && filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
					}
					if (YMLShared.GetMapping(entry, filename, out var mapping20))
					{
						if (mapping20 != null)
						{
							if (ProcessAbilityEntry(filename, mapping20, entry.Key.ToString(), cardID, isMonster, out var ability))
							{
								addActiveBonusAbility = ability;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability for AddActiveBonus.  Filename: " + filename);
							flag = false;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "No valid ability found for AddActiveBonus.  Filename: " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Summon":
				{
					string value29;
					if (entry.Value is Mapping)
					{
						foreach (MappingEntry entry2 in (entry.Value as Mapping).Entries)
						{
							string text = entry2.Key.ToString();
							string value28;
							if (!(text == "ID"))
							{
								if (text == "ActiveBonusData")
								{
									ParseActiveBonusData(entry2.Value, cardID, filename, isMonster: false, ref activeBonusData);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry2.Key.ToString() + " under Summon. File:\n" + filename);
								flag = false;
							}
							else if (YMLShared.GetStringPropertyValue(entry2.Value, "Summon", filename, out value28))
							{
								if (ParseSummon(value28, cardID, name, filename, ref abilityEnhancements, ref abilityType, ref activeBonusData, out var summons2))
								{
									summons = summons2;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Summon", filename, out value29))
					{
						if (ParseSummon(value29, cardID, name, filename, ref abilityEnhancements, ref abilityType, ref activeBonusData, out var summons3))
						{
							summons = summons3;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Summons":
				{
					if (YMLShared.GetStringList(entry.Value, "Summons", filename, out var summonNamesList))
					{
						if (summonNamesList.Count == 4)
						{
							bool[] array = new bool[4];
							int i;
							for (i = 0; i < 4; i++)
							{
								if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonNamesList[i]) == null)
								{
									array[i] = true;
								}
								else if (ScenarioRuleClient.SRLYML.GetMonsterData(summonNamesList[i]) != null)
								{
									array[i] = true;
								}
							}
							if (array[0] && array[1] && array[2] && array[3])
							{
								abilityType = CAbility.EAbilityType.Summon;
								activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Summon;
								summons = summonNamesList;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find all Summons " + string.Join(", ", summonNamesList) + ".  File: " + filename);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "More than 4 summon data defined for Summons " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Revive":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Revive", filename, out var value31))
					{
						if (ParseSummon(value31, cardID, name, filename, ref abilityEnhancements, ref abilityType, ref activeBonusData, out var summons4))
						{
							abilityType = CAbility.EAbilityType.Revive;
							summons = summons4;
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Revives":
				{
					if (YMLShared.GetStringList(entry.Value, "Revives", filename, out var values15))
					{
						if (values15.Count == 4)
						{
							bool[] array2 = new bool[4];
							for (int num16 = 0; num16 < 4; num16++)
							{
								if (ScenarioRuleClient.SRLYML.GetMonsterData(values15[num16]) != null)
								{
									array2[num16] = true;
								}
							}
							if (array2[0] && array2[1] && array2[2] && array2[3])
							{
								abilityType = CAbility.EAbilityType.Revive;
								summons = values15;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find all Revives " + string.Join(", ", values15) + ".  File: " + filename);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "More than 4 summon data defined for Revives " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Attack":
				{
					if (GetEnhancedAbilityValues(entry.Value, "Attack", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out var pierce3, out var pierceEnhancements))
					{
						abilityType = CAbility.EAbilityType.Attack;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						pierce = pierce3;
						if (enhancementSlots > 0)
						{
							for (int num32 = 0; num32 < enhancementSlots; num32++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num32));
							}
						}
						if (pierceEnhancements > 0)
						{
							for (int num33 = 0; num33 < enhancementSlots; num33++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Pierce, cardID, name, num33));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Push":
					if (GetEnhancedAbilityValues(entry.Value, "Push", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Push;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots <= 0)
						{
							break;
						}
						if (isSubAbility)
						{
							for (int num39 = 0; num39 < enhancementSlots; num39++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Push, cardID, name, num39));
							}
						}
						else
						{
							for (int num40 = 0; num40 < enhancementSlots; num40++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num40));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Pull":
					if (GetEnhancedAbilityValues(entry.Value, "Pull", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Pull;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots <= 0)
						{
							break;
						}
						if (isSubAbility)
						{
							for (int num28 = 0; num28 < enhancementSlots; num28++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Pull, cardID, name, num28));
							}
						}
						else
						{
							for (int num29 = 0; num29 < enhancementSlots; num29++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num29));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddAugment":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddAugment", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.AddAugment;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddSong":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddSong", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.AddSong;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddDoomSlot":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddDoomSlot", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.AddDoomSlots;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "TransferDooms":
					if (YMLShared.GetIntPropertyValue(entry.Value, "TransferAllDooms", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.TransferDooms;
						miscAbilityData.CanTargetInvisible = true;
						strength = ((amount == -1) ? int.MaxValue : amount);
					}
					else
					{
						flag = false;
					}
					break;
				case "PlaySong":
					if (YMLShared.GetIntPropertyValue(entry.Value, "PlaySong", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.PlaySong;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddDoom":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping11))
					{
						abilityType = CAbility.EAbilityType.AddDoom;
						miscAbilityData.CanTargetInvisible = true;
						doom = null;
						using (List<MappingEntry>.Enumerator enumerator2 = mapping11.Entries.GetEnumerator())
						{
							if (enumerator2.MoveNext())
							{
								MappingEntry current10 = enumerator2.Current;
								List<CAbility> doomAbilities = null;
								if (current10.Key.ToString() == "Doom")
								{
									if (YMLShared.GetMapping(current10, filename, out var mapping12))
									{
										foreach (MappingEntry entry3 in mapping12.Entries)
										{
											if (entry3.Key.ToString() == "DoomAbilities")
											{
												if (CardProcessingShared.GetAbilities(entry3, cardID, isMonster, filename, out var abilities5))
												{
													doomAbilities = abilities5;
												}
												else
												{
													flag = false;
												}
											}
											else
											{
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Doom.  Entry " + current10.Key.ToString() + " was not recognised.  File: " + filename);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Doom.  Entry " + current10.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
								doom = new CDoom(doomAbilities);
							}
						}
						if (doom == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Doom definition in file " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Move":
					if (GetEnhancedAbilityValues(entry.Value, "Move", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out value2, out fly2, out ignoreDifficultTerrain2, out ignoreHazardousTerrain2, out ignoreBlockedTileMoveCost2))
					{
						abilityType = CAbility.EAbilityType.Move;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						jump = value2;
						fly = fly2;
						ignoreDifficultTerrain = ignoreDifficultTerrain2;
						ignoreHazardousTerrain = ignoreHazardousTerrain2;
						ignoreBlockedTileMoveCost = ignoreBlockedTileMoveCost2;
						if (enhancementSlots > 0)
						{
							for (int num22 = 0; num22 < enhancementSlots; num22++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num22));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Fear":
					if (GetEnhancedAbilityValues(entry.Value, "Fear", filename, out amount, out useSpecialBaseState2, out enhancementSlots, out value2, out fly2, out ignoreDifficultTerrain2, out ignoreHazardousTerrain2, out ignoreBlockedTileMoveCost2))
					{
						abilityType = CAbility.EAbilityType.Fear;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						jump = value2;
						fly = fly2;
						ignoreDifficultTerrain = ignoreDifficultTerrain2;
						ignoreHazardousTerrain = ignoreHazardousTerrain2;
						ignoreBlockedTileMoveCost = ignoreBlockedTileMoveCost2;
						if (enhancementSlots > 0)
						{
							for (int num12 = 0; num12 < enhancementSlots; num12++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num12));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Heal":
					if (GetEnhancedAbilityValues(entry.Value, "Heal", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Heal;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots > 0)
						{
							for (int num35 = 0; num35 < enhancementSlots; num35++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num35));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AdditionalHealData":
					if (!ParseHealData(entry.Value, "AdditionalHealData", ref healAbilityData))
					{
						flag = false;
					}
					break;
				case "DisableCardAction":
					if (ParseDisableCardActionData(entry.Value, "DisableCardAction", ref disableCardActionData))
					{
						abilityType = CAbility.EAbilityType.DisableCardAction;
					}
					else
					{
						flag = false;
					}
					break;
				case "Damage":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/Damage", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.Damage;
						strength = amount;
						isTargetedAbility = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "GainXP":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/GainXP", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.GainXP;
						strength = amount;
						isTargetedAbility = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "ResourcesToAddOnAbilityEnd":
					if (entry.Value is Sequence)
					{
						resourcesToAddOnAbilityEnd = new Dictionary<string, int>();
						Sequence sequence2 = entry.Value as Sequence;
						if (sequence2.Entries[0] is Sequence)
						{
							foreach (DataItem entry4 in sequence2.Entries)
							{
								if (YMLShared.GetTupleStringInt(entry4, "ResourcesToAddOnAbilityEnd", filename, out var tuple2))
								{
									if (resourcesToAddOnAbilityEnd.ContainsKey(tuple2.Item1))
									{
										resourcesToAddOnAbilityEnd[tuple2.Item1] += tuple2.Item2;
									}
									else
									{
										resourcesToAddOnAbilityEnd.Add(tuple2.Item1, tuple2.Item2);
									}
								}
								else
								{
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToAddOnAbilityEnd entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToAddOnAbilityEnd entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
						flag = false;
					}
					break;
				case "ResourcesToGiveToTargets":
					if (entry.Value is Sequence)
					{
						resourcesToGiveToTargets = new Dictionary<string, int>();
						Sequence sequence = entry.Value as Sequence;
						if (sequence.Entries[0] is Sequence)
						{
							foreach (DataItem entry5 in sequence.Entries)
							{
								if (YMLShared.GetTupleStringInt(entry5, "ResourcesToGiveToTargets", filename, out var tuple))
								{
									if (resourcesToGiveToTargets.ContainsKey(tuple.Item1))
									{
										resourcesToGiveToTargets[tuple.Item1] += tuple.Item2;
									}
									else
									{
										resourcesToGiveToTargets.Add(tuple.Item1, tuple.Item2);
									}
								}
								else
								{
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToGiveToTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToGiveToTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
						flag = false;
					}
					break;
				case "ResourcesToTakeFromTargets":
					if (entry.Value is Sequence)
					{
						resourcesToTakeFromTargets = new Dictionary<string, int>();
						Sequence sequence3 = entry.Value as Sequence;
						if (sequence3.Entries[0] is Sequence)
						{
							foreach (DataItem entry6 in sequence3.Entries)
							{
								if (YMLShared.GetTupleStringInt(entry6, "ResourcesToTakeFromTargets", filename, out var tuple3))
								{
									if (resourcesToTakeFromTargets.ContainsKey(tuple3.Item1))
									{
										resourcesToTakeFromTargets[tuple3.Item1] += tuple3.Item2;
									}
									else
									{
										resourcesToTakeFromTargets.Add(tuple3.Item1, tuple3.Item2);
									}
								}
								else
								{
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToTakeFromTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ResourcesToTakeFromTargets entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
						flag = false;
					}
					break;
				case "Loot":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/Loot", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.Loot;
						if (lootData == null)
						{
							lootData = CAbilityLoot.LootData.CreateDefaultLootData();
						}
						range = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreventDamage":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/PreventDamage", filename, out amount))
					{
						abilityType = CAbility.EAbilityType.PreventDamage;
						strength = ((amount == -1) ? int.MaxValue : amount);
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						activeBonusData.IsToggleBonus = true;
						activeBonusData.ToggleIsOptional = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "Shield":
					if (GetEnhancedAbilityValues(entry.Value, "Shield", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Shield;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
						activeBonusData.ToggleIsOptional = false;
						if (enhancementSlots > 0)
						{
							for (int num38 = 0; num38 < enhancementSlots; num38++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num38));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ControlActor":
					if (ParseControlActorData(entry.Value, filename, ref controlActorAbilityData, cardID, isMonster))
					{
						abilityType = CAbility.EAbilityType.ControlActor;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ChangeAllegiance":
					if (ParseChangeAllegianceData(entry.Value, filename, ref changeAllegianceAbilityData, cardID, isMonster))
					{
						abilityType = CAbility.EAbilityType.ChangeAllegiance;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ChooseAbility":
				{
					abilityType = CAbility.EAbilityType.ChooseAbility;
					if (YMLShared.GetMapping(entry, filename, out var mapping21))
					{
						foreach (MappingEntry entry7 in mapping21.Entries)
						{
							string text = entry7.Key.ToString();
							string value24;
							if (!(text == "Strength"))
							{
								if (text == "Abilities")
								{
									if (CardProcessingShared.GetAbilities(entry7, cardID, isMonster, filename, out var abilities9))
									{
										chooseAbilities = abilities9;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process ChooseAbility.  Entry " + entry7.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry7.Value, "Strength", filename, out amount, suppressErrors: true))
							{
								strength = amount;
							}
							else if (YMLShared.GetStringPropertyValue(entry7.Value, "Strength", filename, out value24))
							{
								if (value24 == "Max")
								{
									strength = int.MaxValue;
								}
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Choose":
				{
					abilityType = CAbility.EAbilityType.Choose;
					if (YMLShared.GetMapping(entry, filename, out var mapping18))
					{
						foreach (MappingEntry entry8 in mapping18.Entries)
						{
							string text = entry8.Key.ToString();
							string value23;
							if (!(text == "Strength"))
							{
								if (text == "Abilities")
								{
									if (CardProcessingShared.GetAbilities(entry8, cardID, isMonster, filename, out var abilities8))
									{
										chooseAbilities = abilities8;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Choose.  Entry " + entry8.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry8.Value, "Strength", filename, out amount, suppressErrors: true))
							{
								strength = amount;
							}
							else if (YMLShared.GetStringPropertyValue(entry8.Value, "Strength", filename, out value23))
							{
								if (value23 == "Max")
								{
									strength = int.MaxValue;
								}
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ForgoActionsForCompanion":
				{
					abilityType = CAbility.EAbilityType.ForgoActionsForCompanion;
					if (YMLShared.GetMapping(entry, filename, out var mapping22))
					{
						foreach (MappingEntry entry9 in mapping22.Entries)
						{
							string text = entry9.Key.ToString();
							List<CAbility> abilities11;
							if (!(text == "TopActionAbility"))
							{
								if (text == "BottomActionAbility")
								{
									if (CardProcessingShared.GetAbilities(entry9, cardID, isMonster, filename, out var abilities10))
									{
										forgoBottomActionAbility = abilities10[0];
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process ForgoActionsForCompanion ability.  Entry " + entry9.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (CardProcessingShared.GetAbilities(entry9, cardID, isMonster, filename, out abilities11))
							{
								forgoTopActionAbility = abilities11[0];
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedCreateAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping10))
					{
						foreach (MappingEntry entry10 in mapping10.Entries)
						{
							if (entry10.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry10, cardID, isMonster, filename, out var abilities4, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedCreateAttack;
									mergedAbilities = abilities4;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedCreateAttack.  Entry " + entry10.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedDestroyObstacleAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping13))
					{
						foreach (MappingEntry entry11 in mapping13.Entries)
						{
							if (entry11.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry11, cardID, isMonster, filename, out var abilities6, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedDestroyObstacleAttack;
									mergedAbilities = abilities6;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedDestroyObstacleAttack.  Entry " + entry11.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedMoveAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping4))
					{
						foreach (MappingEntry entry12 in mapping4.Entries)
						{
							if (entry12.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry12, cardID, isMonster, filename, out var abilities2, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedMoveAttack;
									mergedAbilities = abilities2;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveAttack.  Entry " + entry12.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedMoveObstacleAttack":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping3))
					{
						foreach (MappingEntry entry13 in mapping3.Entries)
						{
							if (entry13.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry13, cardID, isMonster, filename, out var abilities, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedMoveObstacleAttack;
									mergedAbilities = abilities;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveObstacleAttack.  Entry " + entry13.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedDisarmTrapDestroyObstacle":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping25))
					{
						foreach (MappingEntry entry14 in mapping25.Entries)
						{
							if (entry14.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry14, cardID, isMonster, filename, out var abilities13, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedDisarmTrapDestroyObstacles;
									mergedAbilities = abilities13;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveObstacleAttack.  Entry " + entry14.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MergedKillCreate":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping24))
					{
						foreach (MappingEntry entry15 in mapping24.Entries)
						{
							if (entry15.Key.ToString() == "Abilities")
							{
								if (CardProcessingShared.GetAbilities(entry15, cardID, isMonster, filename, out var abilities12, isMergedAbility: true))
								{
									abilityType = CAbility.EAbilityType.MergedKillCreate;
									mergedAbilities = abilities12;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process MergedMoveObstacleAttack.  Entry " + entry15.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Redirect":
					abilityType = CAbility.EAbilityType.Redirect;
					break;
				case "AddTarget":
					if (GetEnhancedAbilityValues(entry.Value, "AddTarget", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddTarget;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots > 0)
						{
							for (int num26 = 0; num26 < enhancementSlots; num26++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num26));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddHeal":
					if (GetEnhancedAbilityValues(entry.Value, "AddHeal", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddHeal;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots > 0)
						{
							for (int num36 = 0; num36 < enhancementSlots; num36++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num36));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddRange":
					if (GetEnhancedAbilityValues(entry.Value, "AddRange", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddRange;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots > 0)
						{
							for (int num23 = 0; num23 < enhancementSlots; num23++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num23));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddCondition":
					if (GetEnhancedAbilityValues(entry.Value, "AddCondition", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AddCondition;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots > 0)
						{
							for (int num17 = 0; num17 < enhancementSlots; num17++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num17));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackersGainDisadvantage":
					if (GetEnhancedAbilityValues(entry.Value, "AttackersGainDisadvantage", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.AttackersGainDisadvantage;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (enhancementSlots > 0)
						{
							for (int num14 = 0; num14 < enhancementSlots; num14++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num14));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Retaliate":
					if (GetEnhancedAbilityValues(entry.Value, "Retaliate", filename, out amount, out useSpecialBaseState2, out enhancementSlots))
					{
						abilityType = CAbility.EAbilityType.Retaliate;
						strength = amount;
						useSpecialBaseStat = useSpecialBaseState2;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Round;
						if (enhancementSlots > 0)
						{
							for (int num11 = 0; num11 < enhancementSlots; num11++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num11));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Trap":
					if (ParseTrapData(entry.Value, filename, ref trapData))
					{
						abilityType = CAbility.EAbilityType.Trap;
					}
					else
					{
						flag = false;
					}
					break;
				case "DisarmTrap":
					abilityType = CAbility.EAbilityType.DisarmTrap;
					break;
				case "DeactivateSpawner":
					abilityType = CAbility.EAbilityType.DeactivateSpawner;
					break;
				case "ActivateSpawner":
					abilityType = CAbility.EAbilityType.ActivateSpawner;
					break;
				case "DestroyObstacle":
					abilityType = CAbility.EAbilityType.DestroyObstacle;
					if (destroyObstacleData == null)
					{
						destroyObstacleData = CAbilityDestroyObstacle.DestroyObstacleData.DefaultDestroyObstacleData();
					}
					break;
				case "DestroyObstacleData":
					if (ParseDestroyObstacleData(entry.Value, filename, ref destroyObstacleData))
					{
						abilityType = CAbility.EAbilityType.DestroyObstacle;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveObstacle":
					abilityType = CAbility.EAbilityType.MoveObstacle;
					break;
				case "MoveTrap":
					abilityType = CAbility.EAbilityType.MoveTrap;
					break;
				case "Create":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/Create", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.Create;
						propName = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "Kill":
					abilityType = CAbility.EAbilityType.Kill;
					break;
				case "RemoveActorFromMap":
					abilityType = CAbility.EAbilityType.RemoveActorFromMap;
					break;
				case "ExtraTurn":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ExtraTurn", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.ExtraTurn;
						CAbilityExtraTurn.EExtraTurnType eExtraTurnType = CAbilityExtraTurn.ExtraTurnTypes.SingleOrDefault((CAbilityExtraTurn.EExtraTurnType x) => x.ToString() == stringValue);
						if (eExtraTurnType != CAbilityExtraTurn.EExtraTurnType.None)
						{
							extraTurnType = eExtraTurnType;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ImprovedShortRest":
					abilityType = CAbility.EAbilityType.ImprovedShortRest;
					break;
				case "Swap":
				{
					abilityType = CAbility.EAbilityType.Swap;
					if (!(entry.Value is Mapping) || !YMLShared.GetMapping(entry, filename, out var mapping23))
					{
						break;
					}
					foreach (MappingEntry entry16 in mapping23.Entries)
					{
						string text = entry16.Key.ToString();
						CAbilityFilterContainer filter10;
						if (!(text == "FirstTargetFilter"))
						{
							if (text == "SecondTargetFilter")
							{
								if (CardProcessingShared.GetSingleAbilityFilter(entry16, filename, out var filter9))
								{
									swapAbilitySecondTargetFilter = filter9;
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Swap Ability.  Entry " + entry16.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
						else if (CardProcessingShared.GetSingleAbilityFilter(entry16, filename, out filter10))
						{
							swapAbilityFirstTargetFilter = filter10;
						}
						else
						{
							flag = false;
						}
					}
					break;
				}
				case "RedistributeDamage":
					abilityType = CAbility.EAbilityType.RedistributeDamage;
					break;
				case "ChangeCharacterModel":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "ChangeCharacterModel", filename, out var modelString))
					{
						ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == modelString);
						if (eCharacter != ECharacter.None)
						{
							changeCharacterModel = eCharacter;
							abilityType = CAbility.EAbilityType.ChangeCharacterModel;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid model name " + modelString + " in file " + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ChangeAttackModifier":
					abilityType = CAbility.EAbilityType.ChangeModifier;
					break;
				case "ChangeCondition":
					abilityType = CAbility.EAbilityType.ChangeCondition;
					break;
				case "RemoveConditions":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RefreshItems", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RemoveConditions;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RefreshItems", filename, out stringValue) && stringValue == "All")
					{
						strength = -1;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RemoveConditions;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShuffleModifierDeck":
					abilityType = CAbility.EAbilityType.ShuffleModifierDeck;
					break;
				case "Muddle":
					if (GetEnhancedAbilityValues(entry.Value, "Muddle", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Muddle;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num13 = 0; num13 < enhancementSlots; num13++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num13));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Poison":
					if (GetEnhancedAbilityValues(entry.Value, "Poison", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Poison;
						if (enhancementSlots > 0)
						{
							for (int num18 = 0; num18 < enhancementSlots; num18++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num18));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "StopFlying":
					if (GetEnhancedAbilityValues(entry.Value, "StopFlying", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.StopFlying;
						if (enhancementSlots > 0)
						{
							for (int num10 = 0; num10 < enhancementSlots; num10++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num10));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "BlockHealing":
					if (GetEnhancedAbilityValues(entry.Value, "BlockHealing", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.BlockHealing;
						if (enhancementSlots > 0)
						{
							for (int num8 = 0; num8 < enhancementSlots; num8++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num8));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "NeutralizeShield":
					if (GetEnhancedAbilityValues(entry.Value, "NeutralizeShield", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.NeutralizeShield;
						if (enhancementSlots > 0)
						{
							for (int num7 = 0; num7 < enhancementSlots; num7++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num7));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Immovable":
					if (GetEnhancedAbilityValues(entry.Value, "Immovable", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Immovable;
						if (enhancementSlots > 0)
						{
							for (int num3 = 0; num3 < enhancementSlots; num3++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num3));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Wound":
					if (GetEnhancedAbilityValues(entry.Value, "Wound", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Wound;
						if (enhancementSlots > 0)
						{
							for (int num = 0; num < enhancementSlots; num++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Immobilize":
					if (GetEnhancedAbilityValues(entry.Value, "Immobilize", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Immobilize;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num37 = 0; num37 < enhancementSlots; num37++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num37));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Teleport":
					if (GetEnhancedAbilityValues(entry.Value, "Teleport", filename, out amount, out useSpecialBaseState, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Teleport;
						if (teleportData == null)
						{
							teleportData = CAbilityTeleport.TeleportData.DefaultTeleportData();
						}
						if (enhancementSlots > 0)
						{
							for (int num31 = 0; num31 < enhancementSlots; num31++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num31));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "TeleportData":
					if (ParseTeleportData(entry.Value, filename, ref teleportData))
					{
						abilityType = CAbility.EAbilityType.Teleport;
					}
					else
					{
						flag = false;
					}
					break;
				case "Disarm":
					if (GetEnhancedAbilityValues(entry.Value, "Disarm", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Disarm;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num24 = 0; num24 < enhancementSlots; num24++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num24));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Stun":
					if (GetEnhancedAbilityValues(entry.Value, "Stun", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Stun;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num19 = 0; num19 < enhancementSlots; num19++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num19));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Sleep":
					if (GetEnhancedAbilityValues(entry.Value, "Sleep", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Sleep;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num15 = 0; num15 < enhancementSlots; num15++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num15));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AllowOverheal":
					if (GetEnhancedAbilityValues(entry.Value, "AllowOverheal", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: false))
					{
						abilityType = CAbility.EAbilityType.Overheal;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						strength = amount;
						if (enhancementSlots > 0)
						{
							for (int num9 = 0; num9 < enhancementSlots; num9++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num9));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "HealthReduction":
					if (GetEnhancedAbilityValues(entry.Value, "HealthReduction", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: false))
					{
						abilityType = CAbility.EAbilityType.HealthReduction;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						strength = amount;
						if (enhancementSlots > 0)
						{
							for (int num6 = 0; num6 < enhancementSlots; num6++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num6));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Bless":
					if (GetEnhancedAbilityValues(entry.Value, "Bless", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Bless;
						if (enhancementSlots > 0)
						{
							for (int num4 = 0; num4 < enhancementSlots; num4++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num4));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Invisible":
					if (GetEnhancedAbilityValues(entry.Value, "Invisible", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Invisible;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num41 = 0; num41 < enhancementSlots; num41++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num41));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Strengthen":
					if (GetEnhancedAbilityValues(entry.Value, "Strengthen", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Strengthen;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num34 = 0; num34 < enhancementSlots; num34++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num34));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Advantage":
					if (GetEnhancedAbilityValues(entry.Value, "Advantage", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Advantage;
						conditionDuration = ((amount == 0) ? 1 : amount);
						conditionDecTrigger = ((conditonDecTrigger != EConditionDecTrigger.None) ? conditonDecTrigger : EConditionDecTrigger.Turns);
						if (enhancementSlots > 0)
						{
							for (int num27 = 0; num27 < enhancementSlots; num27++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num27));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Curse":
					if (GetEnhancedAbilityValues(entry.Value, "Curse", filename, out amount, out useSpecialBaseState, out conditonDecTrigger, out enhancementSlots, ignoreAmount: true))
					{
						abilityType = CAbility.EAbilityType.Curse;
						if (enhancementSlots > 0)
						{
							for (int num30 = 0; num30 < enhancementSlots; num30++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Mainline, cardID, name, num30));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Consume":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/Consume", filename, out value2))
					{
						abilityType = CAbility.EAbilityType.Consume;
					}
					else
					{
						flag = false;
					}
					break;
				case "RecoverLostCards":
					if (entry.Value is Mapping)
					{
						if (!YMLShared.GetMapping(entry, filename, out var mapping17))
						{
							break;
						}
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
						foreach (MappingEntry entry17 in mapping17.Entries)
						{
							string text = entry17.Key.ToString();
							int value22;
							if (!(text == "Strength"))
							{
								if (text == "RecoverCardsWithAbilityOfTypeFilter")
								{
									if (YMLShared.GetStringList(entry17.Value, "RecoverCardsWithAbilityOfTypeFilter", filename, out var values12))
									{
										foreach (string abilityTypeString2 in values12)
										{
											try
											{
												CAbility.EAbilityType item2 = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityTypeString2);
												recoverCardsAbilityTypeFilter.Add(item2);
											}
											catch
											{
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityTypeString2 + "' specified for RecoverCardsWithAbilityOfTypeFilter in file " + filename);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry17.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry17.Value, "Strength", filename, out value22))
							{
								strength = value22;
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RecoverLostCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RecoverLostCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "RecoverDiscardedCards":
					if (entry.Value is Mapping)
					{
						if (!YMLShared.GetMapping(entry, filename, out var mapping9))
						{
							break;
						}
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RecoverLostCards;
						foreach (MappingEntry entry18 in mapping9.Entries)
						{
							string text = entry18.Key.ToString();
							int value17;
							if (!(text == "Strength"))
							{
								if (text == "RecoverCardsWithAbilityOfTypeFilter")
								{
									if (YMLShared.GetStringList(entry18.Value, "RecoverCardsWithAbilityOfTypeFilter", filename, out var values11))
									{
										foreach (string abilityTypeString in values11)
										{
											try
											{
												CAbility.EAbilityType item = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityTypeString);
												recoverCardsAbilityTypeFilter.Add(item);
											}
											catch
											{
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityTypeString + "' specified for RecoverCardsWithAbilityOfTypeFilter in file " + filename);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry18.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (YMLShared.GetIntPropertyValue(entry18.Value, "Strength", filename, out value17))
							{
								strength = value17;
							}
							else
							{
								flag = false;
							}
						}
					}
					else if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RecoverDiscardedCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.RecoverDiscardedCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RecoverDiscardedCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.RecoverDiscardedCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "LoseCards":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/LoseCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.LoseCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/LoseCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.LoseCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "DiscardCards":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/DiscardCards", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.DiscardCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/DiscardCards", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.DiscardCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "IncreaseCardLimit":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/IncreaseCardLimit", filename, out amount, suppressErrors: true))
					{
						strength = amount;
						abilityType = CAbility.EAbilityType.IncreaseCardLimit;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/IncreaseCardLimit", filename, out stringValue))
					{
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.IncreaseCardLimit;
					}
					else
					{
						flag = false;
					}
					break;
				case "GiveSupplyCard":
				{
					if (YMLShared.GetIntList(entry.Value, "Ability/GiveSupplyCard", filename, out var values4))
					{
						supplyCardsToGive = values4;
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.GiveSupplyCard;
					}
					else if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/GiveSupplyCard", filename, out amount))
					{
						supplyCardsToGive = new List<int> { amount };
						strength = int.MaxValue;
						abilityType = CAbility.EAbilityType.GiveSupplyCard;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdjustInitiative":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/AdjustInitiative", filename, out amount, suppressErrors: true))
					{
						abilityType = CAbility.EAbilityType.AdjustInitiative;
						strength = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "Null":
					abilityType = CAbility.EAbilityType.Null;
					break;
				case "NullTargeting":
					abilityType = CAbility.EAbilityType.NullTargeting;
					break;
				case "NullHex":
					if (YMLShared.GetStringList(entry.Value, "Ability/NullHex", filename, out values2))
					{
						foreach (string filterString2 in values2)
						{
							abilityType = CAbility.EAbilityType.NullHex;
							CAbilityFilter.EFilterTile eFilterTile3 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == filterString2);
							tileFilter = eFilterTile3;
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/NullTargeting", filename, out stringValue))
					{
						abilityType = CAbility.EAbilityType.NullHex;
						CAbilityFilter.EFilterTile eFilterTile4 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
						tileFilter |= eFilterTile4;
					}
					else
					{
						flag = false;
					}
					break;
				case "TileFilter":
					if (YMLShared.GetStringList(entry.Value, "Ability/TileFilter", filename, out values2))
					{
						foreach (string filterString in values2)
						{
							CAbilityFilter.EFilterTile eFilterTile = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == filterString);
							tileFilter |= eFilterTile;
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/TileFilter", filename, out stringValue))
					{
						CAbilityFilter.EFilterTile eFilterTile2 = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
						tileFilter = eFilterTile2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ImmunityTo":
					if (YMLShared.GetStringList(entry.Value, "Ability/ImmunityTo", filename, out values2))
					{
						abilityType = CAbility.EAbilityType.ImmunityTo;
						immunityToAbilityTypes = new List<CAbility.EAbilityType>();
						foreach (string validTypeString in values2)
						{
							CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == validTypeString);
							if (eAbilityType != CAbility.EAbilityType.None)
							{
								if (!immunityToAbilityTypes.Contains(eAbilityType))
								{
									immunityToAbilityTypes.Add(eAbilityType);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Immunity To Ability Type '" + validTypeString + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ImmunityToAttackTypes":
					if (YMLShared.GetStringList(entry.Value, "Ability/ImmunityToAttackTypes", filename, out values2))
					{
						abilityType = CAbility.EAbilityType.ImmunityTo;
						immuneToAttackTypes = new List<CAbility.EAttackType>();
						foreach (string validTypeString3 in values2)
						{
							CAbility.EAttackType eAttackType4 = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType x) => x.ToString() == validTypeString3);
							if (eAttackType4 != CAbility.EAttackType.None)
							{
								if (!immuneToAttackTypes.Contains(eAttackType4))
								{
									immuneToAttackTypes.Add(eAttackType4);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Immunity To Attack Type '" + validTypeString3 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Invulnerability":
					abilityType = CAbility.EAbilityType.Invulnerability;
					break;
				case "PierceInvulnerability":
					abilityType = CAbility.EAbilityType.PierceInvulnerability;
					break;
				case "Untargetable":
					abilityType = CAbility.EAbilityType.Untargetable;
					break;
				case "ItemLock":
					abilityType = CAbility.EAbilityType.ItemLock;
					break;
				case "RefreshItems":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/RefreshItems", filename, out amount, suppressErrors: true))
					{
						strength |= amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RefreshItemCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/RefreshItems", filename, out stringValue) && stringValue == "All")
					{
						strength = CAbilityRefreshItemCards.STRENGTH_ALL;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.RefreshItemCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "ConsumeItems":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ConsumeItems", filename, out amount, suppressErrors: true))
					{
						strength |= amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.ConsumeItemCards;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ConsumeItems", filename, out stringValue) && stringValue == "All")
					{
						strength = CAbilityConsumeItemCards.STRENGTH_ALL;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.ConsumeItemCards;
					}
					else
					{
						flag = false;
					}
					break;
				case "LoseGoalChestRewards":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/LoseGoalChestRewards", filename, out amount, suppressErrors: true))
					{
						strength |= amount;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.LoseGoalChestReward;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/LoseGoalChestRewards", filename, out stringValue) && stringValue == "All")
					{
						strength = CAbilityLoseGoalChestReward.STRENGTH_ALL;
						if (!isAbilityOverride && filter == null)
						{
							filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self);
						}
						abilityType = CAbility.EAbilityType.LoseGoalChestReward;
					}
					else
					{
						flag = false;
					}
					break;
				case "Infuse":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/Infuse", filename, out var values14))
					{
						foreach (string elementString2 in values14)
						{
							try
							{
								ElementInfusionBoardManager.EElement item4 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString2);
								elementsToInfuse.Add(item4);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString2 + "' specified for infuse ability in file " + filename);
								flag = false;
							}
						}
						if (elementsToInfuse.Count > 0)
						{
							abilityType = CAbility.EAbilityType.Infuse;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConsumeElements":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ConsumeElements", filename, out var values13))
					{
						foreach (string elementString in values13)
						{
							try
							{
								ElementInfusionBoardManager.EElement item3 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString);
								elementsToInfuse.Add(item3);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString + "' specified for infuse ability in file " + filename);
								flag = false;
							}
						}
						if (elementsToInfuse.Count > 0)
						{
							abilityType = CAbility.EAbilityType.ConsumeElement;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ShowElementPicker":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ShowElementPicker", filename, out value2))
					{
						showElementPicker = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackEffect":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping14))
					{
						CAttackEffect.EAttackEffect eAttackEffect = CAttackEffect.EAttackEffect.None;
						int strength2 = 0;
						int xp = 0;
						int pierce2 = 0;
						CAbilityFilter filter4 = new CAbilityFilter();
						List<CCondition.ENegativeCondition> negCons = null;
						List<CCondition.EPositiveCondition> posCons = null;
						CAbility.EAttackType attackType = CAbility.EAttackType.Attack;
						foreach (MappingEntry entry19 in mapping14.Entries)
						{
							switch (entry19.Key.ToString())
							{
							case "Strength":
							case "Amount":
							{
								if (YMLShared.GetIntPropertyValue(entry19.Value, "Strength", filename, out var value18))
								{
									strength2 = value18;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Type":
							{
								if (YMLShared.GetStringPropertyValue(entry19.Value, "Type", filename, out var effectTypeString))
								{
									CAttackEffect.EAttackEffect eAttackEffect2 = CAttackEffect.AttackEffects.SingleOrDefault((CAttackEffect.EAttackEffect s) => s.ToString() == effectTypeString);
									if (eAttackEffect2 != CAttackEffect.EAttackEffect.None)
									{
										eAttackEffect = eAttackEffect2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid property " + effectTypeString + " set for AttackEffect Type in file " + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "XP":
							{
								if (YMLShared.GetIntPropertyValue(entry19.Value, "XP", filename, out var value19))
								{
									xp = value19;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Pierce":
							{
								string value21;
								if (YMLShared.GetIntPropertyValue(entry19.Value, "Pierce", filename, out var value20, suppressErrors: true))
								{
									pierce2 = value20;
								}
								else if (YMLShared.GetStringPropertyValue(entry19.Value, "Pierce", filename, out value21))
								{
									if (value21 == "All")
									{
										pierce2 = 99999;
									}
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Filter":
							{
								if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter5))
								{
									filter4 = filter5;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Condition":
							case "Conditions":
							{
								if (GetConditions(entry19, filename, out var negativeConditions4, out var positiveConditions4))
								{
									negCons = negativeConditions4;
									posCons = positiveConditions4;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "AttackType":
							{
								if (YMLShared.GetStringPropertyValue(entry19.Value, "Type", filename, out var attackTypeString))
								{
									CAbility.EAttackType eAttackType2 = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType s) => s.ToString() == attackTypeString);
									if (eAttackType2 != CAbility.EAttackType.None)
									{
										attackType = eAttackType2;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid property " + attackTypeString + " set for AttackType Type in file " + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AttackEffect.  Entry " + entry19.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
						if (eAttackEffect == CAttackEffect.EAttackEffect.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid or Missing Attack Effect Type in file " + filename);
							flag = false;
						}
						else
						{
							attackEffects.Add(new CAttackEffect(eAttackEffect, strength2, xp, pierce2, filter4, negCons, posCons, attackType));
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConditionalOverrides":
				{
					Mapping mapping5;
					if (!(entry.Value is Mapping))
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in Conditional Override.  Value must be Mapping.  File: " + filename);
						flag = false;
					}
					else if (YMLShared.GetMapping(entry, filename, out mapping5))
					{
						foreach (MappingEntry entry20 in mapping5.Entries)
						{
							if (GetConditionalOverride(entry20, cardID, name, isMonster, filename, out var conditionalOverride))
							{
								conditionalOverrides.Add(conditionalOverride);
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "StartAbilityRequirements":
				{
					if (GetAbilityRequirements(entry, filename, out var abilityRequirements))
					{
						startAbilityRequirements = abilityRequirements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PullType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "PullType", filename, out var pullTypeValue))
					{
						CAbilityPull.EPullType ePullType = CAbilityPull.PullTypes.SingleOrDefault((CAbilityPull.EPullType x) => x.ToString() == pullTypeValue);
						if (ePullType != CAbilityPull.EPullType.None)
						{
							pullType = ePullType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for PullType in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdditionalPushEffect":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "AdditionalPushEffect", filename, out var additionalPushEffectValue))
					{
						CAbilityPush.EAdditionalPushEffect eAdditionalPushEffect = CAbilityPush.AdditionalPushEffects.SingleOrDefault((CAbilityPush.EAdditionalPushEffect x) => x.ToString() == additionalPushEffectValue);
						if (eAdditionalPushEffect != CAbilityPush.EAdditionalPushEffect.None)
						{
							additionalPushEffect = eAdditionalPushEffect;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for AdditionalPushEffect in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdditionalPushEffectDamage":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "AdditionalPushEffectDamage", filename, out var value14))
					{
						additionalPushEffectDamage = value14;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdditionalPushEffectXP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "AdditionalPushEffectXP", filename, out var value12))
					{
						additionalPushEffectXP = value12;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "LootData":
					if (ParseLootData(entry.Value, filename, ref lootData))
					{
						abilityType = CAbility.EAbilityType.Loot;
					}
					else
					{
						flag = false;
					}
					break;
				case "Range":
					if (GetEnhancedAbilityValues(entry.Value, "Range", filename, out amount, out useSpecialBaseState, out enhancementSlots))
					{
						range = amount;
						if (enhancementSlots > 0)
						{
							for (int num5 = 0; num5 < enhancementSlots; num5++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Range, cardID, name, num5));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "RangeIsBase":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "RangeIsBase", filename, out value2))
					{
						rangeIsBase = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "RangeAtLeastOne":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "RangeAtLeastOne", filename, out value2))
					{
						rangeAtLeastOne = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "RetaliateRange":
					if (entry.Value.ToString() == "All")
					{
						retaliateRange = 99999;
					}
					else if (GetEnhancedAbilityValues(entry.Value, "Ability/RetaliateRange", filename, out amount, out useSpecialBaseState, out enhancementSlots))
					{
						retaliateRange = amount;
						if (flag && enhancementSlots > 0)
						{
							for (int num2 = 0; num2 < enhancementSlots; num2++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.RetaliateRange, cardID, name, num2));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "MultiPassAttack":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/MultiPassAttack", filename, out value2))
					{
						multiPassAttack = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ChainAttack":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ChainAttack", filename, out value2))
					{
						chainAttack = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ChainAttackRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ChainAttackRange", filename, out amount))
					{
						chainAttackRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "ChainAttackDamageReduction":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/ChainAttackDamageReduction", filename, out amount))
					{
						chainAttackDamageReduction = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "DamageSelfBeforeAttack":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/DamageSelfBeforeAttack", filename, out amount))
					{
						damageSelfBeforeAttack = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "SpawnDelay":
					if (YMLShared.GetFloatPropertyValue(entry.Value, "Ability/SpawnDelay", filename, out value30))
					{
						spawnDelay = value30;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsOnMovePath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsOnMovePath", filename, out value2))
					{
						allTargetsOnMovePath = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsOnMovePathSameStartAndEnd":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsOnMovePathSameStartAndEnd", filename, out value2))
					{
						allTargetsOnMovePathSameStartAndEnd = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsOnAttackPath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsOnAttackPath", filename, out value2))
					{
						allTargetsOnAttackPath = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "XPPerTarget":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/XPPerTarget", filename, out amount))
					{
						xpPerTargetData.XpPerTarget = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "XPPerTargetPerEnemyCount":
					if (YMLShared.GetIntPropertyValue(entry.Value, "Ability/XPPerTargetPerEnemyCount", filename, out amount))
					{
						xpPerTargetData.PerEnemyCount = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "XPPerTargetMustDie":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/XPPerTargetMustDie", filename, out value2))
					{
						xpPerTargetData.MustDie = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "Targeting":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/Targeting", filename, out stringValue))
					{
						CAbility.EAbilityTargeting eAbilityTargeting = CAbility.AbilityTargetingTypes.SingleOrDefault((CAbility.EAbilityTargeting x) => x.ToString() == stringValue);
						if (eAbilityTargeting != CAbility.EAbilityTargeting.None)
						{
							targeting = eAbilityTargeting;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Targeting Type '" + stringValue + "'.  File: " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "Condition":
				case "Conditions":
				{
					if (GetConditions(entry, filename, out var negativeConditions6, out var positiveConditions6))
					{
						if (negativeConditions6.Count > 0)
						{
							negativeConditions = negativeConditions6;
						}
						if (positiveConditions6.Count > 0)
						{
							positiveConditions = positiveConditions6;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Filter":
				{
					if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter8))
					{
						filter = filter8;
						miscAbilityData.FilterSpecified = true;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "FilterContainer":
				{
					if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter7))
					{
						filter = filter7;
						miscAbilityData.FilterSpecified = true;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Target":
					if (GetEnhancedAbilityValues(entry.Value, "Target", filename, out amount, out useSpecialBaseState, out enhancementSlots))
					{
						numberOfTargets = amount;
						numberOfTargetsSet = true;
						if (enhancementSlots > 0)
						{
							for (int num25 = 0; num25 < enhancementSlots; num25++)
							{
								abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.Targets, cardID, name, num25));
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetIsBase":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TargetIsBase", filename, out var value27))
					{
						targetIsBase = value27;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TargetAtLeastOne":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TargetAtLeastOne", filename, out var value26))
					{
						targetAtLeastOne = value26;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RemoveConditionsOverride":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "RemoveConditionsOverride", filename, out var value25))
					{
						removeConditionsOverride = value25;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AreaEffect":
					if (YMLShared.GetStringPropertyValue(entry.Value, "AreaEffect", filename, out stringValue))
					{
						areaEffectYMLString = stringValue;
						try
						{
							if (stringValue == null || stringValue.Length <= 0)
							{
								break;
							}
							int num20 = 0;
							string[] array3 = stringValue.Split('|');
							for (int num21 = 0; num21 < array3.Length; num21++)
							{
								if (array3[num21].Split(',')[2].ToUpper() == "E")
								{
									abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.AreaHex, cardID, name, num20));
									num20++;
								}
							}
						}
						catch (Exception ex)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Area Effect string in file " + filename + "\n" + ex.Message + "\n" + ex.StackTrace);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AreaEffectLayoutOverride":
					if (YMLShared.GetStringPropertyValue(entry.Value, "AreaEffectLayoutOverride", filename, out stringValue))
					{
						areaEffectLayoutOverrideYMLString = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "AnimOverload":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AnimOverload", filename, out stringValue))
					{
						animationOverload = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "SkipAnim":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/SkipAnim", filename, out value2))
					{
						skipAnim = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "IsInlineSubAbility":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IsInlineSubAbility", filename, out value2))
					{
						isInlineSubAbility = value2;
						isSubAbility = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "SubAbilities":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping19))
					{
						mapping = mapping19;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Augment":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping15))
					{
						bool flag2 = false;
						CAbility.EAttackType eAttackType3 = CAbility.EAttackType.None;
						List<CAbility> list4 = null;
						List<CAbilityOverride> list5 = new List<CAbilityOverride>();
						foreach (MappingEntry entry21 in mapping15.Entries)
						{
							switch (entry21.Key.ToString())
							{
							case "AugmentIsBonus":
								flag2 = true;
								if (YMLShared.GetBoolPropertyValue(entry21.Value, "Augment/AugmentIsBonus", filename, out value2))
								{
									flag2 = value2;
								}
								else
								{
									flag = false;
								}
								break;
							case "Type":
							{
								if (YMLShared.GetStringPropertyValue(entry21.Value, "Augment/Type", filename, out var augTypeString))
								{
									eAttackType3 = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType s) => s.ToString() == augTypeString);
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Abilities":
							{
								if (CardProcessingShared.GetAbilities(entry21, cardID, isMonster, filename, out var abilities7))
								{
									list4 = abilities7;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "AbilityOverrides":
							{
								if (YMLShared.GetMapping(entry21, filename, out var mapping16))
								{
									foreach (MappingEntry entry22 in mapping16.Entries)
									{
										CAbilityOverride cAbilityOverride2 = CAbilityOverride.CreateAbilityOverride(entry22, cardID, isMonster, filename, parentName);
										if (cAbilityOverride2 != null)
										{
											list5.Add(cAbilityOverride2);
										}
										else
										{
											flag = false;
										}
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides in Augment.  Entry must be mapping.  File: " + filename);
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Augment.  Entry " + entry21.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
								break;
							}
						}
						if (eAttackType3 == CAbility.EAttackType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "No valid Type set for Augment in file " + filename);
							flag = false;
						}
						else if (list4 != null && list4.Count > 0)
						{
							foreach (CAbility item5 in list4)
							{
								if (flag2)
								{
									item5.ActiveBonusData.Duration = ((item5.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA) ? CActiveBonus.EActiveBonusDurationType.Persistent : item5.ActiveBonusData.Duration);
									item5.ActiveBonusData.ValidAbilityTypes = ((item5.ActiveBonusData.ValidAbilityTypes.Count <= 0) ? new List<CAbility.EAbilityType> { CAbility.EAbilityType.Attack } : item5.ActiveBonusData.ValidAbilityTypes.ToList());
									item5.ActiveBonusData.AttackType = ((item5.ActiveBonusData.AttackType == CAbility.EAttackType.None) ? eAttackType3 : item5.ActiveBonusData.AttackType);
								}
							}
							augment = new CAugment(list4, eAttackType3, flag2);
						}
						else if (list5 != null && list5.Count > 0)
						{
							augment = new CAugment(list5, eAttackType3, flag2);
						}
						else
						{
							augment = new CAugment(eAttackType3);
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Song":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping6))
					{
						List<CSong.SongEffect> list = new List<CSong.SongEffect>();
						foreach (MappingEntry entry23 in mapping6.Entries)
						{
							CSong.ESongActivationType activationType = CSong.ESongActivationType.AbilityStart;
							CAbility.EAttackType type = CAbility.EAttackType.None;
							CAbility.EAbilityType abilityType2 = CAbility.EAbilityType.None;
							List<CAbility> list2 = null;
							List<CAbilityOverride> list3 = new List<CAbilityOverride>();
							string text = entry23.Key.ToString();
							CAbilityFilterContainer filter3;
							if (!(text == "Filter"))
							{
								if (text == "SongEffect")
								{
									if (YMLShared.GetMapping(entry23, filename, out var mapping7))
									{
										foreach (MappingEntry entry24 in mapping7.Entries)
										{
											switch (entry24.Key.ToString())
											{
											case "ActivationType":
											{
												if (YMLShared.GetStringPropertyValue(entry24.Value, "Song/ActivationType", filename, out var songActivationTypeString))
												{
													activationType = CSong.SongActivationTypes.SingleOrDefault((CSong.ESongActivationType s) => s.ToString() == songActivationTypeString);
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "AttackType":
											{
												if (YMLShared.GetStringPropertyValue(entry24.Value, "Song/AttackType", filename, out var songAttackTypeString))
												{
													type = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType s) => s.ToString() == songAttackTypeString);
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "AbilityType":
											{
												if (YMLShared.GetStringPropertyValue(entry24.Value, "Song/AbilityType", filename, out var songAbilityTypeString))
												{
													abilityType2 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == songAbilityTypeString);
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "Abilities":
											{
												if (CardProcessingShared.GetAbilities(entry24, cardID, isMonster, filename, out var abilities3))
												{
													list2 = abilities3;
												}
												else
												{
													flag = false;
												}
												break;
											}
											case "AbilityOverrides":
											{
												if (YMLShared.GetMapping(entry24, filename, out var mapping8))
												{
													foreach (MappingEntry entry25 in mapping8.Entries)
													{
														CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry25, cardID, isMonster, filename, parentName);
														if (cAbilityOverride != null)
														{
															list3.Add(cAbilityOverride);
														}
													}
												}
												else
												{
													SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides in Song.  Entry must be mapping.  File: " + filename);
													flag = false;
												}
												break;
											}
											default:
												SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process SongEffect.  Entry " + entry23.Key.ToString() + " was not recognised.  File: " + filename);
												flag = false;
												break;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process Song.  Entry " + entry23.Key.ToString() + " was not recognised.  File: " + filename);
									flag = false;
								}
							}
							else if (CardProcessingShared.GetSingleAbilityFilter(entry23, filename, out filter3))
							{
								filter2 = filter3;
							}
							else
							{
								flag = false;
							}
							if (entry23.Key.ToString() != "Filter")
							{
								if (list2 != null && list2.Count > 0)
								{
									list.Add(new CSong.SongEffect(list2, activationType, type, abilityType2));
									continue;
								}
								if (list3 != null && list3.Count > 0)
								{
									list.Add(new CSong.SongEffect(list3, activationType, type, abilityType2));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Song definition in file " + filename);
								flag = false;
							}
						}
						if (list.Count <= 0)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Song definition in file " + filename);
							flag = false;
						}
						else
						{
							song = new CSong(list, filter2);
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AttackSourcesOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AttackSourcesOnly", filename, out value2))
					{
						attackSourcesOnly = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ActiveBonusDuration":
					if (!ParseActiveBonusDuration(entry.Value, filename, isMonster, ref activeBonusData))
					{
						flag = false;
					}
					break;
				case "ActiveBonus":
					if (ParseActiveBonusData(entry.Value, cardID, filename, isMonster, ref activeBonusData))
					{
						if (activeBonusData.OverrideAsSong)
						{
							List<CSong.SongEffect> songEffects = new List<CSong.SongEffect>();
							song = new CSong(songEffects, activeBonusData.Filter);
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "Text":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Text", filename, out stringValue))
					{
						abilityText = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "TextOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TextOnly", filename, out value2))
					{
						abilityTextOnly = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShowRange":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowRange", filename, out value2))
					{
						showRange = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShowTarget":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowTarget", filename, out value2))
					{
						showTarget = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ShowArea":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowArea", filename, out value2))
					{
						showArea = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "OnDeath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "OnDeath", filename, out value2))
					{
						onDeath = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "Strength":
				case "Amount":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Amount", filename, out var value13))
					{
						strength = value13;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AddAttackBaseStat":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AddAttackBaseStat", filename, out value2))
					{
						addAttackBaseStat = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "StrengthIsBase":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "StrengthIsBase", filename, out value2))
					{
						strengthIsBase = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ParentName":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "ParentName", filename, out var value11))
					{
						parentName = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IsTargetedAbility":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IsTargetedAbility", filename, out var value10))
					{
						isTargetedAbility = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityXP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Amount", filename, out var value9))
					{
						abilityXP = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Jump":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Jump", filename, out var value8))
					{
						jump = value8;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Fly":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Fly", filename, out var value7))
					{
						fly = value7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AIFocusOverride":
					if (!ParseAIFocusOverride(entry.Value, filename, ref aiFocusOverride))
					{
						flag = false;
					}
					break;
				case "IgnoreDifficultTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreDifficultTerrain", filename, out var value6))
					{
						ignoreDifficultTerrain = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreHazardousTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreHazardousTerrain", filename, out var value5))
					{
						ignoreHazardousTerrain = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreBlockedTileMoveCost":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreHazardousTerrain", filename, out var value3))
					{
						ignoreBlockedTileMoveCost = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CarryOtherActorsOnHex":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreHazardousTerrain", filename, out var value4))
					{
						carryOtherActorsOnHex = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MoveRestrictionType":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "MoveRestrictionType", filename, out var moveRestrictionTypeValue))
					{
						CAbilityMove.EMoveRestrictionType eMoveRestrictionType = CAbilityMove.MoveRestrictionTypes.SingleOrDefault((CAbilityMove.EMoveRestrictionType x) => x.ToString() == moveRestrictionTypeValue);
						if (eMoveRestrictionType != CAbilityMove.EMoveRestrictionType.None)
						{
							moveRestrictionType = eMoveRestrictionType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for MoveRestrictionType in file " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Pierce":
				{
					string value34;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", filename, out var value33, suppressErrors: true))
					{
						pierce = value33;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Pierce", filename, out value34))
					{
						if (value34 == "All")
						{
							pierce = 99999;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AutoTrigger":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AutoTrigger", filename, out var value32))
					{
						miscAbilityData.AutotriggerAbility = value32;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RestrictRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "RestrictRange", filename, out amount))
					{
						miscAbilityData.RestrictMoveRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetOneEnemyWithAllAttacks":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TargetOneEnemyWithAllAttacks", filename, out value2))
					{
						miscAbilityData.TargetOneEnemyWithAllAttacks = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "TreatAsMelee":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TreatAsMelee", filename, out value2))
					{
						miscAbilityData.TreatAsMelee = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseParentRangeType":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseParentRangeType", filename, out value2))
					{
						miscAbilityData.UseParentRangeType = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AddTargetPropertyToStrength":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AddTargetPropertyToStrength", filename, out stringValue))
					{
						CActor.EOverrideLookupProperty eOverrideLookupProperty = CActor.OverrideLookupProperties.SingleOrDefault((CActor.EOverrideLookupProperty x) => x.ToString() == stringValue);
						if (eOverrideLookupProperty == CActor.EOverrideLookupProperty.NA)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AddTargetPropertyToStrength property '" + stringValue + "' in file '" + filename);
							flag = false;
						}
						else
						{
							miscAbilityData.AddTargetPropertyToStrength = eOverrideLookupProperty;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AddTargetPropertyToStrengthMultiplier":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AddTargetPropertyToStrengthMultiplier", filename, out amount))
					{
						miscAbilityData.AddTargetPropertyToStrengthMultiplier = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsAdjacentToParentMovePath":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsAdjacentToParentMovePath", filename, out value2))
					{
						miscAbilityData.AllTargetsAdjacentToParentMovePath = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllTargetsAdjacentToParentTargets":
				case "AllTargetsAdjacentToParentTarget":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllTargetsAdjacentToParentTarget", filename, out value2))
					{
						miscAbilityData.AllTargetsAdjacentToParentTargets = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseParentTiles":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseParentTiles", filename, out value2))
					{
						miscAbilityData.UseParentTiles = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreParentAreaOfEffect":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreParentAreaOfEffect", filename, out value2))
					{
						miscAbilityData.IgnoreParentAreaOfEffect = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnorePreviousAbilityTargets":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/IgnorePreviousAbilityTargets", filename, out var values16))
					{
						miscAbilityData.IgnorePreviousAbilityTargets = values16.ToList();
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "IgnoreMergedAbilityTargetSelection":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreMergedAbilityTargetSelection", filename, out value2))
					{
						miscAbilityData.IgnoreMergedAbilityTargetSelection = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseMergedWithAbilityTiles":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseMergedWithAbilityTiles", filename, out value2))
					{
						miscAbilityData.UseMergedWithAbilityTiles = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "HealPercentageOfHealth":
					if (YMLShared.GetFloatPropertyValue(entry.Value, "HealPercentageOfHealth", filename, out value30))
					{
						miscAbilityData.HealPercentageOfHealth = value30;
					}
					else
					{
						flag = false;
					}
					break;
				case "ExactRange":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ExactRange", filename, out value2))
					{
						miscAbilityData.ExactRange = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackHasAdvantage":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AttackHasAdvantage", filename, out value2))
					{
						miscAbilityData.AttackHasAdvantage = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AttackHasDisadvantage":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AttackHasDisadvantage", filename, out value2))
					{
						miscAbilityData.AttackHasDisadvantage = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ExhaustSelf":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ExhaustSelf", filename, out value2))
					{
						miscAbilityData.ExhaustSelf = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "GainXPPerXDamageDealt":
					if (YMLShared.GetIntPropertyValue(entry.Value, "GainXPPerXDamageDealt", filename, out amount))
					{
						miscAbilityData.GainXPPerXDamageDealt = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "SetHPTo":
					if (YMLShared.GetIntPropertyValue(entry.Value, "SetHPTo", filename, out amount))
					{
						miscAbilityData.SetHPTo = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreventOnlyIfLethal":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "PreventOnlyIfLethal", filename, out value2))
					{
						miscAbilityData.PreventOnlyIfLethal = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "InfuseIfNotStrong":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "InfuseIfNotStrong", filename, out value2))
					{
						miscAbilityData.InfuseIfNotStrong = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "CanTargetInvisible":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanTargetInvisible", filename, out value2))
					{
						miscAbilityData.CanTargetInvisible = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "NotApplyEnemy":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "NotApplyEnemy", filename, out value2))
					{
						miscAbilityData.NotApplyEnemy = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreMonsterColumnLayout":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreMonsterColumnLayout", filename, out value2))
					{
						miscAbilityData.IgnoreMonsterColumnLayout = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreMCLAdjustY":
					if (YMLShared.GetIntPropertyValue(entry.Value, "IgnoreMCLAdjustY", filename, out amount))
					{
						miscAbilityData.IgnoreMCLAdjustY = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "GapAbove":
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapAbove", filename, out amount))
					{
						miscAbilityData.GapAbove = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "GapBelow":
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapBelow", filename, out amount))
					{
						miscAbilityData.GapBelow = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "IgnoreStun":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/IgnoreStun", filename, out value2))
					{
						miscAbilityData.IgnoreStun = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "HasCondition":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/HasCondition", filename, out value2))
					{
						miscAbilityData.HasCondition = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AreaEffectSymmetrical":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AreaEffectSymmetrical", filename, out value2))
					{
						miscAbilityData.AreaEffectSymmetrical = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "TreatAsNonSubAbility":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TreatAsNonSubAbility", filename, out value2))
					{
						miscAbilityData.TreatAsNonSubAbility = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "PerformXTimesBasedOn":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/PerformXTimesBasedOn", filename, out stringValue))
					{
						CAbility.EStatIsBasedOnXType eStatIsBasedOnXType = CAbility.StatIsBasedOnXTypes.SingleOrDefault((CAbility.EStatIsBasedOnXType x) => x.ToString() == stringValue);
						if (eStatIsBasedOnXType == CAbility.EStatIsBasedOnXType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid PerformXTimesBasedOn property '" + stringValue + "' in file '" + filename);
							flag = false;
						}
						else
						{
							miscAbilityData.PerformXTimesBasedOn = eStatIsBasedOnXType;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "ConsiderMultiTargetForEnhancements":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ConsiderMultiTargetForEnhancements", filename, out value2))
					{
						miscAbilityData.ConsiderMultiTargetForEnhancements = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "OnAttackInfuse":
				{
					if (CardProcessingShared.GetElements(entry.Value, "OnAttackInfuse", filename, out var elements))
					{
						miscAbilityData.OnAttackInfuse = elements.ToList();
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AlreadyHasConditionDamageInstead":
					if (YMLShared.GetIntPropertyValue(entry.Value, "AlreadyHasConditionDamageInstead", filename, out amount))
					{
						miscAbilityData.AlreadyHasConditionDamageInstead = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "RemoveCount":
					if (YMLShared.GetIntPropertyValue(entry.Value, "RemoveCount", filename, out amount))
					{
						miscAbilityData.RemoveCount = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseParentTargets":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseParentTargets", filename, out value2))
					{
						miscAbilityData.UseParentTargets = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AlsoTargetSelf":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AlsoTargetSelf", filename, out value2))
					{
						miscAbilityData.AlsoTargetSelf = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AlsoTargetAdjacent":
				{
					if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter6))
					{
						miscAbilityData.AlsoTargetAdjacent = filter6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConditionsToRemoveFirst":
				{
					if (GetConditions(entry, filename, out var negativeConditions5, out var _))
					{
						if (negativeConditions5.Count > 0)
						{
							miscAbilityData.ConditionsToRemoveFirst = negativeConditions5;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ShowPlusX":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/ShowPlusX", filename, out value2))
					{
						miscAbilityData.ShowPlusX = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "BypassImmunity":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/BypassImmunity", filename, out value2))
					{
						miscAbilityData.BypassImmunity = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "InlineSubAbilityOnKilledTargetsOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/InlineSubAbilityOnKilledTargetsOnly", filename, out value2))
					{
						miscAbilityData.InlineSubAbilityOnKilledTargetsOnly = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "NegativeAbilityIsOptional":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/NegativeAbilityIsOptional", filename, out value2))
					{
						miscAbilityData.NegativeAbilityIsOptional = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "AllowContinueForNullAbility":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/AllowContinueForNullAbility", filename, out value2))
					{
						miscAbilityData.AllowContinueForNullAbility = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "NoGoldDrop":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/NoGoldDrop", filename, out value2))
					{
						miscAbilityData.NoGoldDrop = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "UseOriginalActor":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/UseOriginalActor", filename, out value2))
					{
						miscAbilityData.UseOriginalActor = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "MovePathIndexFilter":
				{
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "MovePathIndexFilter", filename, out var equalityFilter))
					{
						miscAbilityData.MovePathIndexFilter = equalityFilter;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TriggeredScenarioModifiers":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/TriggeredScenarioModifiers", filename, out var values10))
					{
						miscAbilityData.TriggeredScenarioModifiers = values10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TriggerScenarioModifierOnSelfOnly":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Ability/TriggerScenarioModifierOnSelfOnly", filename, out value2))
					{
						miscAbilityData.TriggerScenarioModifierOnSelfOnly = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "ObstacleTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ObstacleTypes", filename, out var values9))
					{
						foreach (string validTypeString2 in values9)
						{
							EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == validTypeString2);
							if (ePropType != EPropType.None)
							{
								if (miscAbilityData.ObstacleTypes == null)
								{
									miscAbilityData.ObstacleTypes = new List<EPropType>();
								}
								if (!miscAbilityData.ObstacleTypes.Contains(ePropType))
								{
									miscAbilityData.ObstacleTypes.Add(ePropType);
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Obstacle Type '" + validTypeString2 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TargetActorWithTrapEffects":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "TargetActorWithTrapEffects", filename, out value2))
					{
						targetActorWithTrapEffects = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "TargetActorWithTrapEffectRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "TargetActorWithTrapEffectRange", filename, out amount))
					{
						targetActorWithTrapEffectRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "StatIsBasedOnX":
				{
					if (ParseStatIsBasedOnX(entry, filename, out var statIsBasedOnXEntries2, isBaseStats: false))
					{
						statIsBasedOnXEntries = statIsBasedOnXEntries2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MoveObstacleRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "MoveObstacleRange", filename, out amount))
					{
						moveObstacleRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveObstacleAnimOverload":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "MoveObstacleAnimOverload", filename, out var value16))
					{
						moveObstacleAnimOverload = value16;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MoveTrapRange":
					if (YMLShared.GetIntPropertyValue(entry.Value, "MoveTrapRange", filename, out amount))
					{
						moveObstacleRange = amount;
					}
					else
					{
						flag = false;
					}
					break;
				case "MoveTrapAnimOverload":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "MoveTrapAnimOverload", filename, out var value15))
					{
						moveObstacleAnimOverload = value15;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotToRefresh":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotToRefresh", filename, out var values8))
					{
						foreach (string slotTypeString4 in values8)
						{
							try
							{
								CItem.EItemSlot eItemSlot2 = CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotTypeString4);
								if (eItemSlot2 != CItem.EItemSlot.None)
								{
									slotsToRefresh.Add(eItemSlot2);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotStateToRefresh property '" + slotTypeString4 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotStateToRefresh":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotStateToRefresh", filename, out var values7))
					{
						foreach (string slotTypeString3 in values7)
						{
							try
							{
								CItem.EItemSlotState eItemSlotState2 = CItem.ItemSlotStates.SingleOrDefault((CItem.EItemSlotState x) => x.ToString() == slotTypeString3);
								if (eItemSlotState2 != CItem.EItemSlotState.None)
								{
									slotStatesToRefresh.Add(eItemSlotState2);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotStateToRefresh property '" + slotTypeString3 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotToConsume":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotToConsume", filename, out var values6))
					{
						foreach (string slotTypeString2 in values6)
						{
							try
							{
								CItem.EItemSlot eItemSlot = CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotTypeString2);
								if (eItemSlot != CItem.EItemSlot.None)
								{
									slotsToRefresh.Add(eItemSlot);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotToConsume property '" + slotTypeString2 + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SlotStateToConsume":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/SlotStateToConsume", filename, out var values5))
					{
						foreach (string slotTypeString in values5)
						{
							try
							{
								CItem.EItemSlotState eItemSlotState = CItem.ItemSlotStates.SingleOrDefault((CItem.EItemSlotState x) => x.ToString() == slotTypeString);
								if (eItemSlotState != CItem.EItemSlotState.None)
								{
									slotStatesToRefresh.Add(eItemSlotState);
								}
								else
								{
									flag = false;
								}
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid SlotStateToConsume property '" + slotTypeString + "' in file '" + filename);
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "OverrideAugmentAttackType":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/OverrideAugmentAttackType", filename, out stringValue))
					{
						CAbility.EAttackType eAttackType = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType x) => x.ToString() == stringValue);
						if (eAttackType != CAbility.EAttackType.None)
						{
							overrideAugmentAttackType = eAttackType;
							abilityType = CAbility.EAbilityType.OverrideAugmentAttackType;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid OverrideAugmentAttackType property '" + stringValue + "' in file '" + filename);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				case "AbilityType":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AbilityType", filename, out stringValue))
					{
						CAbility.EAbilityType eAbilityType2 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == stringValue);
						if (eAbilityType2 != CAbility.EAbilityType.None)
						{
							abilityType = eAbilityType2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AbilityType property '" + stringValue + "' in file '" + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreviewEffectId":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/PreviewEffectId", filename, out stringValue))
					{
						previewEffectId = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "PreviewEffectText":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/PreviewEffectText", filename, out stringValue))
					{
						previewEffectText = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "HelpBoxTooltipLocKey":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/HelpBoxTooltipLocKey", filename, out stringValue))
					{
						helpBoxTooltipLocKey = stringValue;
					}
					else
					{
						flag = false;
					}
					break;
				case "ReplaceModifier":
				case "ReplaceModifiers":
				{
					if (YMLShared.GetStringList(entry.Value, "Ability/ReplaceModifiers", filename, out var values3))
					{
						foreach (string item6 in values3)
						{
							if (int.TryParse(item6.Substring(1), out var result))
							{
								if (result < -2 || result > 2)
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
							if (!flag)
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid modifer '" + item6 + "' specified for ReplaceModifiers in file " + filename);
								flag = false;
							}
						}
						if (flag)
						{
							miscAbilityData.ReplaceModifiers = values3;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ReplaceWithModifier":
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/ReplaceWithModifier", filename, out stringValue))
					{
						miscAbilityData.ReplaceWithModifier = stringValue;
						break;
					}
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ReplaceWithModifier property '" + stringValue + "' in file '" + filename);
					flag = false;
					break;
				case "ReplaceCondition":
				case "ReplaceConditions":
				{
					if (GetConditions(entry, filename, out var negativeConditions3, out var positiveConditions3))
					{
						if (negativeConditions3.Count > 0)
						{
							miscAbilityData.ReplaceNegativeConditions = negativeConditions3;
						}
						if (positiveConditions3.Count > 0)
						{
							miscAbilityData.ReplacePositiveConditions = positiveConditions3;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ReplaceWithCondition":
				{
					if (GetConditions(entry, filename, out var negativeConditions2, out var positiveConditions2))
					{
						if (negativeConditions2.Count > 0)
						{
							miscAbilityData.ReplaceWithNegativeConditions = negativeConditions2;
						}
						if (positiveConditions2.Count > 0)
						{
							miscAbilityData.ReplaceWithPositiveConditions = positiveConditions2;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CanUndo":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanUndo", filename, out value2))
					{
						miscAbilityData.CanUndo = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "CanSkip":
					if (YMLShared.GetBoolPropertyValue(entry.Value, "CanSkip", filename, out value2))
					{
						miscAbilityData.CanSkip = value2;
					}
					else
					{
						flag = false;
					}
					break;
				case "RecoverResources":
					abilityType = CAbility.EAbilityType.RecoverResources;
					break;
				case "AddModifiersToMonsterDeck":
				{
					if (!(entry.Value is Mapping) || !YMLShared.GetMapping(entry, filename, out var mapping2))
					{
						break;
					}
					if (!isAbilityOverride && filter == null)
					{
						filter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy);
					}
					abilityType = CAbility.EAbilityType.AddModifierToMonsterDeck;
					foreach (MappingEntry entry26 in mapping2.Entries)
					{
						string text = entry26.Key.ToString();
						int value;
						if (!(text == "Strength"))
						{
							if (text == "ModifiersToAdd")
							{
								if (YMLShared.GetStringList(entry26.Value, "Ability/ModifiersToAdd", filename, out var values))
								{
									if (values.Count > 0)
									{
										addModifiers = values.ToList();
									}
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process AddModifiersToMonsterDeck.  Entry " + entry26.Key.ToString() + " was not recognised.  File: " + filename);
								flag = false;
							}
						}
						else if (YMLShared.GetIntPropertyValue(entry26.Value, "Strength", filename, out value))
						{
							strength = value;
						}
						else
						{
							flag = false;
						}
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid abilities entry " + entry.Key.ToString() + ". File:\n" + filename);
					flag = false;
					break;
				}
			}
			if (areaEffectYMLString != null && areaEffectYMLString.Length > 0)
			{
				areaEffect = CardProcessingShared.CreateArea(areaEffectYMLString, name + "Area", filename);
			}
			if (areaEffect == null && range.HasValue && range.Value > 0 && miscAbilityData.ExactRange.HasValue && miscAbilityData.ExactRange.Value)
			{
				areaEffect = CardProcessingShared.CreateExactRangeAsAreaEffect(range.Value, name + "Area", filename);
				miscAbilityData.TreatAsMelee = false;
			}
			if (mapping != null)
			{
				foreach (MappingEntry entry27 in mapping.Entries)
				{
					if (YMLShared.GetMapping(entry27, filename, out var mapping26))
					{
						if (ProcessAbilityEntry(filename, mapping26, entry27.Key.ToString(), cardID, isMonster, out var ability2, isSubAbility: true, isConsumeAbility: false, filter, strength, range, numberOfTargets, areaEffectYMLString, areaEffectLayoutOverrideYMLString, targeting, areaEffect, isTargetedAbility))
						{
							if (subAbilities == null)
							{
								subAbilities = new List<CAbility>();
							}
							subAbilities.Add(ability2);
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
			}
			return flag;
		}
		catch (Exception ex2)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing Ability Data.\n" + ex2.Message + "\n" + ex2.StackTrace + "\nFile:" + filename);
		}
		return false;
	}

	private static bool ParseActiveBonusDuration(DataItem di, string filename, bool isMonster, ref ActiveBonusData activeBonusData)
	{
		string stringValue = string.Empty;
		if (YMLShared.GetStringPropertyValue(di, "ActiveBonus/Duration", filename, out stringValue))
		{
			CActiveBonus.EActiveBonusDurationType eActiveBonusDurationType = CActiveBonus.ActiveBonusDurationTypes.SingleOrDefault((CActiveBonus.EActiveBonusDurationType x) => x.ToString() == stringValue);
			if (eActiveBonusDurationType == CActiveBonus.EActiveBonusDurationType.NA)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Duration '" + stringValue + "' in file '" + filename);
				return false;
			}
			if (eActiveBonusDurationType == CActiveBonus.EActiveBonusDurationType.Persistent && isMonster)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Cannot add persistent bonus effects to monster cards.  File:" + filename);
				return false;
			}
			activeBonusData.Duration = eActiveBonusDurationType;
			return true;
		}
		return false;
	}

	private static bool ParseActiveBonusData(DataItem di, int cardID, string filename, bool isMonster, ref ActiveBonusData activeBonusData)
	{
		if (di is Scalar)
		{
			return ParseActiveBonusDuration(di, filename, isMonster, ref activeBonusData);
		}
		if (di is Mapping)
		{
			bool result = true;
			string stringValue = string.Empty;
			bool value = false;
			int value2 = 0;
			List<CAbilityOverride> list = new List<CAbilityOverride>();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					List<string> values;
					switch (entry.Key.ToString())
					{
					case "Behaviour":
						if (YMLShared.GetStringPropertyValue(entry.Value, "ActiveBonus/Behaviour", filename, out stringValue))
						{
							CActiveBonus.EActiveBonusBehaviourType eActiveBonusBehaviourType = CActiveBonus.ActiveBonusTypes.SingleOrDefault((CActiveBonus.EActiveBonusBehaviourType s) => s.ToString() == stringValue);
							if (eActiveBonusBehaviourType != CActiveBonus.EActiveBonusBehaviourType.None)
							{
								activeBonusData.Behaviour = eActiveBonusBehaviourType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Behaviour '" + stringValue + "' in file '" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "Duration":
						if (!ParseActiveBonusDuration(entry.Value, filename, isMonster, ref activeBonusData))
						{
							result = false;
						}
						break;
					case "Restriction":
						if (YMLShared.GetStringPropertyValue(entry.Value, "ActiveBonus/Restriction", filename, out stringValue))
						{
							CActiveBonus.EActiveBonusRestrictionType eActiveBonusRestrictionType = CActiveBonus.ActiveBonusRestrictionTypes.SingleOrDefault((CActiveBonus.EActiveBonusRestrictionType s) => s.ToString() == stringValue);
							if (eActiveBonusRestrictionType != CActiveBonus.EActiveBonusRestrictionType.None)
							{
								activeBonusData.Restriction = eActiveBonusRestrictionType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Restriction '" + stringValue + "' in file '" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "Requirement":
					case "Requirements":
					{
						if (GetAbilityRequirements(entry, filename, out var abilityRequirements))
						{
							activeBonusData.Requirements = abilityRequirements;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "MoveType":
					case "AttackType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "ActiveBonus/AttackType", filename, out stringValue))
						{
							CAbility.EAttackType eAttackType = CAbility.AttackTypes.SingleOrDefault((CAbility.EAttackType x) => x.ToString() == stringValue);
							if (eAttackType != CAbility.EAttackType.None)
							{
								activeBonusData.AttackType = eAttackType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Attack Type '" + stringValue + "' in file '" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "ValidAbilityTypes":
					case "ValidAbilityType":
						if (YMLShared.GetStringList(entry.Value, "ActiveBonus/ValidAbilityType", filename, out values))
						{
							activeBonusData.ValidAbilityTypes = new List<CAbility.EAbilityType>();
							foreach (string validTypeString in values)
							{
								CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == validTypeString);
								if (eAbilityType != CAbility.EAbilityType.None)
								{
									if (!activeBonusData.ValidAbilityTypes.Contains(eAbilityType))
									{
										activeBonusData.ValidAbilityTypes.Add(eAbilityType);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Valid Ability Type '" + validTypeString + "' in file '" + filename);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ValidTargetTypes":
					case "ValidTargetType":
					{
						if (YMLShared.GetStringList(entry.Value, "ActiveBonus/ValidTargetType", filename, out var values4))
						{
							activeBonusData.ValidTargetTypes = new List<CAbilityFilter.EFilterTargetType>();
							foreach (string validTypeString2 in values4)
							{
								CAbilityFilter.EFilterTargetType eFilterTargetType = CAbilityFilter.FilterTargetTypes.SingleOrDefault((CAbilityFilter.EFilterTargetType x) => x.ToString() == validTypeString2);
								if (eFilterTargetType != CAbilityFilter.EFilterTargetType.None)
								{
									if (!activeBonusData.ValidTargetTypes.Contains(eFilterTargetType))
									{
										activeBonusData.ValidTargetTypes.Add(eFilterTargetType);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Active Bonus Valid Target Type '" + validTypeString2 + "' in file '" + filename);
									result = false;
								}
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ValidTargetTypesExclusive":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ValidTargetTypesExclusive", filename, out value))
						{
							activeBonusData.ValidTargetTypesExclusive = value;
						}
						else
						{
							result = false;
						}
						break;
					case "InvertValidity":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "InvertValidity", filename, out value))
						{
							activeBonusData.InvertValidity = value;
						}
						else
						{
							result = false;
						}
						break;
					case "ValidConditions":
					{
						if (GetConditions(entry, filename, out var negativeConditions, out var positiveConditions))
						{
							if (positiveConditions.Count > 0)
							{
								activeBonusData.ValidPositiveConditionTypes = positiveConditions;
							}
							if (negativeConditions.Count > 0)
							{
								activeBonusData.ValidNegativeConditionTypes = negativeConditions;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ProcXP":
						if (YMLShared.GetIntPropertyValue(entry.Value, "ActiveBonus/ProcXP", filename, out value2))
						{
							activeBonusData.ProcXP = value2;
						}
						else
						{
							result = false;
						}
						break;
					case "StrengthIsScalar":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ActiveBonusIsScalar", filename, out value))
						{
							activeBonusData.StrengthIsScalar = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AbilityStrengthIsScalar":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ActiveBonusIsScalar", filename, out value))
						{
							activeBonusData.AbilityStrengthIsScalar = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Filter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter4))
						{
							activeBonusData.Filter = filter4;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "FilterContainer":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter3))
						{
							activeBonusData.Filter = filter3;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "IsAura":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ActiveBonusIsAura", filename, out value))
						{
							activeBonusData.IsAura = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AuraFilter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter2))
						{
							activeBonusData.AuraFilter = filter2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AuraFilterContainer":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter))
						{
							activeBonusData.AuraFilter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AuraTargeting":
						if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/AuraTargeting", filename, out stringValue))
						{
							CAbility.EAbilityTargeting eAbilityTargeting = CAbility.AbilityTargetingTypes.SingleOrDefault((CAbility.EAbilityTargeting x) => x.ToString() == stringValue);
							if (eAbilityTargeting != CAbility.EAbilityTargeting.None)
							{
								activeBonusData.AuraTargeting = eAbilityTargeting;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Aura Targeting Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "AbilityData":
					{
						if (ProcessAbilityEntry(filename, entry.Value as Mapping, entry.Key.ToString(), cardID, isMonster, out var ability2))
						{
							activeBonusData.AbilityData = ability2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "Consuming":
					{
						if (CardProcessingShared.GetElements(entry.Value, "Consuming", filename, out var elements))
						{
							activeBonusData.Consuming.AddRange(elements);
						}
						else
						{
							result = false;
						}
						break;
					}
					case "CostAbility":
					{
						if (ProcessAbilityEntry(filename, entry.Value as Mapping, entry.Key.ToString(), cardID, isMonster, out var ability))
						{
							activeBonusData.CostAbility = ability;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "XPTracker":
					{
						if (YMLShared.GetIntList(entry.Value, "XPTracker", filename, out var values2))
						{
							activeBonusData.Tracker = values2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "IsToggleBonus":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsToggleBonus", filename, out value))
						{
							activeBonusData.IsToggleBonus = value;
							if (activeBonusData.Behaviour == CActiveBonus.EActiveBonusBehaviourType.None)
							{
								activeBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.DefaultToggleBehaviour;
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ToggleIsOptional":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ToggleIsOptional", filename, out value))
						{
							activeBonusData.ToggleIsOptional = value;
						}
						else
						{
							result = false;
						}
						break;
					case "RestrictPerActor":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "RestrictPerActor", filename, out value))
						{
							activeBonusData.RestrictPerActor = value;
						}
						else
						{
							result = false;
						}
						break;
					case "OverrideAsSong":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "OverrideAsSong", filename, out value))
						{
							activeBonusData.OverrideAsSong = value;
						}
						else
						{
							result = false;
						}
						break;
					case "IsSingleTargetBonus":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsSingleTargetBonus", filename, out value))
						{
							activeBonusData.IsSingleTargetBonus = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Target":
						if (YMLShared.GetIntPropertyValue(entry.Value, "Target", filename, out value2))
						{
							activeBonusData.TargetCount = value2;
						}
						else
						{
							result = false;
						}
						break;
					case "ActiveBonusAbilityOverrides":
					{
						if (YMLShared.GetMapping(entry, filename, out var mapping))
						{
							foreach (MappingEntry entry2 in mapping.Entries)
							{
								CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry2, cardID, isMonster, filename);
								if (cAbilityOverride != null)
								{
									list.Add(cAbilityOverride);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Failed to create active bonus ability override.  File: " + filename);
								result = false;
							}
							activeBonusData.ActiveBonusAbilityOverrides = list;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides in Augment.  Entry must be mapping.  File: " + filename);
							result = false;
						}
						break;
					}
					case "ActiveBonusAnimOverload":
						if (YMLShared.GetStringPropertyValue(entry.Value, "ActiveBonusAnimOverload", filename, out stringValue))
						{
							activeBonusData.ActiveBonusAnimOverload = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "UseTriggerAbilityAsParent":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseTriggerAbilityAsParent", filename, out value))
						{
							activeBonusData.UseTriggerAbilityAsParent = value;
						}
						else
						{
							result = false;
						}
						break;
					case "GiveAbilityCardToActor":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "GiveAbilityCardToActor", filename, out value))
						{
							activeBonusData.GiveAbilityCardToActor = value;
						}
						else
						{
							result = false;
						}
						break;
					case "StartsRestricted":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "StartsRestricted", filename, out value))
						{
							activeBonusData.StartsRestricted = value;
						}
						else
						{
							result = false;
						}
						break;
					case "EntireAction":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "EntireAction", filename, out value))
						{
							activeBonusData.EntireAction = value;
						}
						else
						{
							result = false;
						}
						break;
					case "OriginalTargetType":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "OriginalTargetType", filename, out value))
						{
							activeBonusData.OriginalTargetType = value;
						}
						else
						{
							result = false;
						}
						break;
					case "SetFilterToCaster":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "SetFilterToCaster", filename, out value))
						{
							activeBonusData.SetFilterToCaster = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TriggerOnCaster":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TriggerOnCaster", filename, out value))
						{
							activeBonusData.TriggerOnCaster = value;
						}
						else
						{
							result = false;
						}
						break;
					case "CannotCancel":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "CannotCancel", filename, out value))
						{
							activeBonusData.CannotCancel = value;
						}
						else
						{
							result = false;
						}
						break;
					case "EndAllActiveBonusesOnBaseCardSimultaneously":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "EndAllActiveBonusesOnBaseCardSimultaneously", filename, out value))
						{
							activeBonusData.EndAllActiveBonusesOnBaseCardSimultaneously = value;
						}
						else
						{
							result = false;
						}
						break;
					case "RequiredResources":
						if (entry.Value is Sequence)
						{
							activeBonusData.RequiredResources = new Dictionary<string, int>();
							Sequence sequence = entry.Value as Sequence;
							if (sequence.Entries[0] is Sequence)
							{
								foreach (DataItem entry3 in sequence.Entries)
								{
									if (YMLShared.GetTupleStringInt(entry3, "RequiredResources", filename, out var tuple))
									{
										if (activeBonusData.RequiredResources.ContainsKey(tuple.Item1))
										{
											activeBonusData.RequiredResources[tuple.Item1] += tuple.Item2;
										}
										else
										{
											activeBonusData.RequiredResources.Add(tuple.Item1, tuple.Item2);
										}
									}
									else
									{
										result = false;
									}
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid RequiredResources entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid RequiredResources entry, must be list of [ResourceID, Amount] pairs. File: " + filename);
							result = false;
						}
						break;
					case "ConsumeResources":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ConsumeResources", filename, out value))
						{
							activeBonusData.ConsumeResources = value;
						}
						else
						{
							result = false;
						}
						break;
					case "ListLayoutOverride":
						if (YMLShared.GetStringList(entry.Value, "ListLayoutOverride", filename, out values))
						{
							if (values.Count == 2)
							{
								activeBonusData.ListLayoutOverride = (values[0], values[1]);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ListLayoutOverride entry '" + entry.Value?.ToString() + ", expected format [$LocEntry$, Icon]'. File:\n" + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "AuraTriggerAbilityAnimType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "AuraTriggerAbilityAnimType", filename, out stringValue))
						{
							CActiveBonus.EAuraTriggerAbilityAnimType auraAnimTriggerAbilityType = CActiveBonus.AuraTriggerAbilityAnimTypes.SingleOrDefault((CActiveBonus.EAuraTriggerAbilityAnimType x) => x.ToString() == stringValue);
							activeBonusData.AuraAnimTriggerAbilityType = auraAnimTriggerAbilityType;
						}
						else
						{
							result = false;
						}
						break;
					case "CancelActiveBonusesOnBaseCardIDs":
					{
						if (YMLShared.GetIntList(entry.Value, "CancelActiveBonusesOnBaseCardIDs", filename, out var values3))
						{
							activeBonusData.CancelActiveBonusesOnBaseCardIDs = values3;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for CancelActiveBonusesOnBaseCardIDs in ActiveBonusData.  File: " + filename);
						result = false;
						break;
					}
					case "RemoveAllInstancesOfResourcesOnFinish":
						if (YMLShared.GetStringList(entry.Value, "RemoveAllInstancesOfResourcesOnFinish", filename, out values))
						{
							activeBonusData.RemoveAllInstancesOfResourcesOnFinish = values.ToList();
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for CancelActiveBonusesOnBaseCardIDs in ActiveBonusData.  File: " + filename);
						result = false;
						break;
					case "Hidden":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Hidden", filename, out value))
						{
							activeBonusData.Hidden = value;
						}
						else
						{
							result = false;
						}
						break;
					case "GroupID":
						if (YMLShared.GetStringPropertyValue(entry.Value, "GroupID", filename, out stringValue))
						{
							activeBonusData.GroupID = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "FullCopy":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "FullCopy", filename, out value))
						{
							activeBonusData.FullCopy = value;
						}
						else
						{
							result = false;
						}
						break;
					case "NeedsDamage":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "NeedsDamage", filename, out value))
						{
							activeBonusData.NeedsDamage = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ActiveBonus entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ActiveBonus value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseTrapData(DataItem di, string filename, ref TrapData trapData)
	{
		if (di is Mapping)
		{
			bool result = true;
			string stringValue = string.Empty;
			int value = 0;
			trapData = new TrapData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "TrapName":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TrapData/TrapName", filename, out stringValue))
						{
							trapData.TrapName = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "Damage":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/Damage", filename, out value))
						{
							trapData.Damage = value;
						}
						else
						{
							result = false;
						}
						break;
					case "Conditions":
					{
						if (GetConditions(entry, filename, out var negativeConditions, out var _))
						{
							if (negativeConditions.Count > 0)
							{
								trapData.Conditions = negativeConditions;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AdjacentRange":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/AdjacentRange", filename, out value))
						{
							trapData.AdjacentRange = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AdjacentDamage":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/AdjacentDamage", filename, out value))
						{
							trapData.AdjacentDamage = value;
						}
						else
						{
							result = false;
						}
						break;
					case "AdjacentConditions":
					{
						if (GetConditions(entry, filename, out var negativeConditions2, out var _))
						{
							if (negativeConditions2.Count > 0)
							{
								trapData.AdjacentConditions = negativeConditions2;
							}
						}
						else
						{
							result = false;
						}
						break;
					}
					case "AdjacentFilter":
					{
						if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter))
						{
							trapData.AdjacentFilter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TriggeredXP":
						if (YMLShared.GetIntPropertyValue(entry.Value, "TrapData/TriggeredXP", filename, out value))
						{
							trapData.TriggeredXP = value;
						}
						else
						{
							result = false;
						}
						break;
					case "PlacementTileFilter":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TrapData/PlacementTileFilter", filename, out stringValue))
						{
							CAbilityFilter.EFilterTile placementTileFilter = CAbilityFilter.FilterTiles.SingleOrDefault((CAbilityFilter.EFilterTile x) => x.ToString() == stringValue);
							trapData.PlacementTileFilter = placementTileFilter;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TrapData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TrapData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseDestroyObstacleData(DataItem di, string filename, ref CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			_ = string.Empty;
			destroyObstacleData = new CAbilityDestroyObstacle.DestroyObstacleData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					if (!(text == "CanTargetEmptyHexes"))
					{
						if (text == "CanDestroyObstaclesWithHP")
						{
							if (YMLShared.GetBoolPropertyValue(entry.Value, "CanDestroyObstaclesWithHP", filename, out value))
							{
								destroyObstacleData.CanDestroyObstaclesWithHP = value;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DestroyObstacleData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
							result = false;
						}
					}
					else if (YMLShared.GetBoolPropertyValue(entry.Value, "CanTargetEmptyHexes", filename, out value))
					{
						destroyObstacleData.CanTargetEmptyHexes = value;
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DestroyObstacleData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseTeleportData(DataItem di, string filename, ref CAbilityTeleport.TeleportData teleportData)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			string stringValue = string.Empty;
			teleportData = new CAbilityTeleport.TeleportData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "TeleportType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TeleportType", filename, out stringValue))
						{
							CAbilityTeleport.ETeleportType eTeleportType = CAbilityTeleport.TeleportTypes.SingleOrDefault((CAbilityTeleport.ETeleportType x) => x.ToString() == stringValue);
							if (eTeleportType != CAbilityTeleport.ETeleportType.None)
							{
								teleportData.TeleportType = eTeleportType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Teleport Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "PropType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "PropType", filename, out stringValue))
						{
							EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == stringValue);
							if (ePropType != EPropType.None)
							{
								teleportData.PropType = ePropType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Prop Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "MoveOtherThingsOffTiles":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "MoveOtherThingsOffTiles", filename, out value))
						{
							teleportData.MoveOtherThingsOffTiles = value;
						}
						else
						{
							result = false;
						}
						break;
					case "ShouldOpenDoorsToTeleportedLocation":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "ShouldOpenDoorsToTeleportedLocation", filename, out value))
						{
							teleportData.ShouldOpenDoorsToTeleportedLocation = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TeleportData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid TeleportData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseControlActorData(DataItem di, string filename, ref CAbilityControlActor.ControlActorAbilityData controlActorData, int cardID, bool isMonster)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			_ = string.Empty;
			controlActorData = new CAbilityControlActor.ControlActorAbilityData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Abilities":
					{
						if (CardProcessingShared.GetAbilities(entry, cardID, isMonster, filename, out var abilities))
						{
							controlActorData.ControlAbilities = abilities;
							controlActorData.ControlAbilities.ForEach(delegate(CAbility x)
							{
								x.IsControlAbility = true;
							});
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ControlDurationType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ControlDurationType", filename, out var controlDurationTypeValue))
						{
							CAbilityControlActor.EControlDurationType eControlDurationType = CAbilityControlActor.ControlDurationTypes.SingleOrDefault((CAbilityControlActor.EControlDurationType x) => x.ToString() == controlDurationTypeValue);
							if (eControlDurationType != CAbilityControlActor.EControlDurationType.None)
							{
								controlActorData.ControlDurationType = eControlDurationType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid value for ControlDurationType in file " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ControlActorType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ControlActorType", filename, out var controlActorTypeValue))
						{
							CActor.EType value2 = CActor.Types.SingleOrDefault((CActor.EType x) => x.ToString() == controlActorTypeValue);
							controlActorData.ControlType = value2;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "TransferAugments":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TransferAugments", filename, out value))
						{
							controlActorData.TransferAugments = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TreatControlAbilityParentAsParent":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatControlAbilityParentAsParent", filename, out value))
						{
							controlActorData.TreatControlAbilityParentAsParent = value;
						}
						else
						{
							result = false;
						}
						break;
					case "TreatPreviousAbilityToControlAbilityAsParent":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatPreviousAbilityToControlAbilityAsParent", filename, out value))
						{
							controlActorData.TreatPreviousAbilityToControlAbilityAsParent = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseControllingActorModifierDeckFilter":
					{
						if (CardProcessingShared.GetAbilityFilter(entry, filename, out var filter))
						{
							controlActorData.UseControllingActorModifierDeckFilter = filter;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "UseControllingActorModifierDeck":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseControllingActorModifierDeck", filename, out value))
						{
							controlActorData.UseControllingActorModifierDeck = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseControllingActorPlayerControl":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseControllingActorPlayerControl", filename, out value))
						{
							controlActorData.UseControllingActorPlayerControl = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ControlActorAbilityData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ControlActorAbilityData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseChangeAllegianceData(DataItem di, string filename, ref CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData, int cardID, bool isMonster)
	{
		if (di is Mapping)
		{
			bool result = true;
			_ = string.Empty;
			CActor.EType eType = CActor.EType.Player;
			foreach (MappingEntry entry in (di as Mapping).Entries)
			{
				if (entry.Key.ToString() == "ChangeToType")
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "ChangeToType", filename, out var controlActorTypeValue))
					{
						eType = CActor.Types.SingleOrDefault((CActor.EType x) => x.ToString() == controlActorTypeValue);
					}
					else
					{
						result = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ChangeAllegianceAbilityData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
					result = false;
				}
			}
			if (eType != CActor.EType.Enemy && eType != CActor.EType.Ally && eType != CActor.EType.Enemy2 && eType != CActor.EType.Neutral)
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ChangeToType ActorType in ChangeAllegianceAbilityData entry. File:\n" + filename);
				result = false;
			}
			changeAllegianceData = new CAbilityChangeAllegiance.ChangeAllegianceAbilityData(eType);
			return result;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ChangeAllegianceAbilityData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseLootData(DataItem di, string filename, ref CAbilityLoot.LootData lootData)
	{
		if (di is Mapping)
		{
			bool result = true;
			string stringValue = string.Empty;
			List<string> values = new List<string>();
			lootData = CAbilityLoot.LootData.CreateDefaultLootData();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					if (!(text == "LootableObjectImportTypes"))
					{
						if (text == "AdditionalLootEffect")
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "AdditionalLootEffect", filename, out stringValue))
							{
								CAbilityLoot.EAdditionalLootEffect eAdditionalLootEffect = CAbilityLoot.AdditionalLootEffects.SingleOrDefault((CAbilityLoot.EAdditionalLootEffect x) => x.ToString() == stringValue);
								if (eAdditionalLootEffect != CAbilityLoot.EAdditionalLootEffect.None)
								{
									lootData.AdditionalLootEffect = eAdditionalLootEffect;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AdditionalLootEffect Type '" + stringValue + "'.  File: " + filename);
								result = false;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid LootData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
							result = false;
						}
					}
					else if (YMLShared.GetStringList(entry.Value, "LootableObjectImportTypes", filename, out values))
					{
						lootData.LootableObjectImportTypes = new List<ScenarioManager.ObjectImportType>();
						foreach (string lootableTypeString in values)
						{
							ScenarioManager.ObjectImportType objectImportType = ScenarioManager.ObjectImportTypes.SingleOrDefault((ScenarioManager.ObjectImportType x) => x.ToString() == lootableTypeString);
							if (objectImportType != ScenarioManager.ObjectImportType.None)
							{
								lootData.LootableObjectImportTypes.Add(objectImportType);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Object Import Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid LootData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseHealData(DataItem di, string filename, ref CAbilityHeal.HealAbilityData healData)
	{
		if (di is Mapping)
		{
			bool result = true;
			healData = new CAbilityHeal.HealAbilityData();
			List<CCondition.ENegativeCondition> negativeConditions = null;
			List<CCondition.EPositiveCondition> positiveConditions = null;
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "IgnoreTokens":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreTokens", filename, out var value))
						{
							healData.IgnoreTokens = value;
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ConditionsToAddIfHealRemovesPoison":
						if (GetConditions(entry, filename, out negativeConditions, out positiveConditions))
						{
							if (negativeConditions.Count > 0)
							{
								healData.NegativeConditionsToAddIfHealRemovesPoison = negativeConditions.ToList();
							}
							if (positiveConditions.Count > 0)
							{
								healData.PositiveConditionsToAddIfHealRemovesPoison = positiveConditions.ToList();
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ApplyConditionsOnRemovingPoisonFilter":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter))
						{
							healData.ApplyConditionsOnRemovingPoisonFilter = filter.Copy();
						}
						else
						{
							result = false;
						}
						break;
					}
					case "ConditionsToAddIfHealRemovesWound":
						if (GetConditions(entry, filename, out negativeConditions, out positiveConditions))
						{
							if (negativeConditions.Count > 0)
							{
								healData.NegativeConditionsToAddIfHealRemovesWound = negativeConditions.ToList();
							}
							if (positiveConditions.Count > 0)
							{
								healData.PositiveConditionsToAddIfHealRemovesWound = positiveConditions.ToList();
							}
						}
						else
						{
							result = false;
						}
						break;
					case "ApplyConditionsOnRemovingWoundFilter":
					{
						if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter2))
						{
							healData.ApplyConditionsOnRemovingWoundFilter = filter2.Copy();
						}
						else
						{
							result = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid HealData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid HealData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseDisableCardActionData(DataItem di, string filename, ref CAbilityDisableCardAction.DisableCardActionData disableData)
	{
		if (di is Mapping)
		{
			bool result = true;
			disableData = new CAbilityDisableCardAction.DisableCardActionData();
			CBaseCard.ActionType actionType = CBaseCard.ActionType.NA;
			{
				string stringValue;
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					string text = entry.Key.ToString();
					if (!(text == "CardName"))
					{
						if (text == "DisableActionType")
						{
							if (YMLShared.GetStringPropertyValue(entry.Value, "DisableActionType", filename, out stringValue))
							{
								actionType = CBaseCard.ActionTypes.SingleOrDefault((CBaseCard.ActionType x) => x.ToString() == stringValue);
								if (actionType != CBaseCard.ActionType.NA)
								{
									disableData.DisableActionType = actionType;
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DisableActionType '" + stringValue + "'.  File: " + filename);
								result = false;
							}
							else
							{
								result = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DisableCardActionData entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
							result = false;
						}
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "CardName", filename, out stringValue))
					{
						disableData.CardName = stringValue;
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid DisableCardActionData value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	private static bool ParseAIFocusOverride(DataItem di, string filename, ref CAIFocusOverrideDetails aiFocusOverride)
	{
		if (di is Mapping)
		{
			bool result = true;
			bool value = false;
			string stringValue = string.Empty;
			aiFocusOverride = new CAIFocusOverrideDetails();
			{
				foreach (MappingEntry entry in (di as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "OverrideType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "OverrideType", filename, out stringValue))
						{
							CAIFocusOverrideDetails.EOverrideType eOverrideType = CAIFocusOverrideDetails.OverrideTypes.SingleOrDefault((CAIFocusOverrideDetails.EOverrideType x) => x.ToString() == stringValue);
							if (eOverrideType != CAIFocusOverrideDetails.EOverrideType.None)
							{
								aiFocusOverride.OverrideType = eOverrideType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AI Override Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "OverrideTargetType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "OverrideTargetType", filename, out stringValue))
						{
							CAIFocusOverrideDetails.EOverrideTargetType eOverrideTargetType = CAIFocusOverrideDetails.OverrideTargetTypes.SingleOrDefault((CAIFocusOverrideDetails.EOverrideTargetType x) => x.ToString() == stringValue);
							if (eOverrideTargetType != CAIFocusOverrideDetails.EOverrideTargetType.None)
							{
								aiFocusOverride.OverrideTargetType = eOverrideTargetType;
								aiFocusOverride.IsTemporary = true;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AI Override Target Type '" + stringValue + "'.  File: " + filename);
								result = false;
							}
						}
						else
						{
							result = false;
						}
						break;
					case "TargetGUID":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetGUID", filename, out stringValue))
						{
							aiFocusOverride.TargetGUID = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "TargetClassID":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetClassID", filename, out stringValue))
						{
							aiFocusOverride.TargetClassID = stringValue;
						}
						else
						{
							result = false;
						}
						break;
					case "TargetPropType":
						if (YMLShared.GetStringPropertyValue(entry.Value, "TargetPropType", filename, out stringValue))
						{
							EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == stringValue);
							if (ePropType != EPropType.None)
							{
								aiFocusOverride.TargetPropType = ePropType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AI Override Target Prop Type '" + stringValue + "'.  File: " + filename);
							result = false;
						}
						else
						{
							result = false;
						}
						break;
					case "DisallowDoorways":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "DisallowDoorways", filename, out value))
						{
							aiFocusOverride.DisallowDoorways = value;
						}
						else
						{
							result = false;
						}
						break;
					case "FocusBenign":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "FocusBenign", filename, out value))
						{
							aiFocusOverride.FocusBenign = value;
						}
						else
						{
							result = false;
						}
						break;
					case "UseFurthestFocus":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseFurthestFocus", filename, out value))
						{
							aiFocusOverride.UseFurthestFocus = value;
						}
						else
						{
							result = false;
						}
						break;
					case "IgnoreActivatedProps":
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreActivatedProps", filename, out value))
						{
							aiFocusOverride.IgnoreActivatedProps = value;
						}
						else
						{
							result = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AIFocusOverrideDetails entry '" + entry.Key?.ToString() + "'. File:\n" + filename);
						result = false;
						break;
					}
				}
				return result;
			}
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid AIFocusOverrideDetails value, must be Scalar or Mapping type. File:\n" + filename);
		return false;
	}

	public static bool ParseStatIsBasedOnX(MappingEntry statIsBasedOnXEntry, string filename, out List<StatIsBasedOnXData> statIsBasedOnXEntries, bool isBaseStats)
	{
		bool flag = true;
		statIsBasedOnXEntries = new List<StatIsBasedOnXData>();
		if (statIsBasedOnXEntry.Value is Mapping mapping)
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				StatIsBasedOnXData statIsBasedOnXData = new StatIsBasedOnXData();
				string statString = entry.Key.ToString();
				if (isBaseStats)
				{
					EMonsterBaseStats eMonsterBaseStats = MonsterYMLData.MonsterBaseStatsEnums.SingleOrDefault((EMonsterBaseStats x) => x.ToString() == statString);
					if (eMonsterBaseStats != EMonsterBaseStats.None)
					{
						statIsBasedOnXData.BaseStatType = eMonsterBaseStats;
						switch (eMonsterBaseStats)
						{
						case EMonsterBaseStats.Attack:
						case EMonsterBaseStats.Move:
							statIsBasedOnXData.AbilityStatType = CAbility.EAbilityStatType.Strength;
							break;
						case EMonsterBaseStats.Range:
							statIsBasedOnXData.AbilityStatType = CAbility.EAbilityStatType.Range;
							break;
						case EMonsterBaseStats.Target:
							statIsBasedOnXData.AbilityStatType = CAbility.EAbilityStatType.NumberOfTargets;
							break;
						}
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					if (statString == "Target")
					{
						statString = CAbility.EAbilityStatType.NumberOfTargets.ToString();
					}
					CAbility.EAbilityStatType eAbilityStatType = CAbility.AbilityStatTypes.SingleOrDefault((CAbility.EAbilityStatType x) => x.ToString() == statString);
					if (eAbilityStatType != CAbility.EAbilityStatType.None)
					{
						statIsBasedOnXData.AbilityStatType = eAbilityStatType;
					}
					else
					{
						flag = false;
					}
				}
				if (flag)
				{
					if (entry.Value is Mapping mapping2)
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							switch (entry2.Key.ToString())
							{
							case "BasedOn":
							{
								if (YMLShared.GetStringPropertyValue(entry2.Value, "BasedOn", filename, out var stringValue))
								{
									CAbility.EStatIsBasedOnXType eStatIsBasedOnXType = CAbility.StatIsBasedOnXTypes.SingleOrDefault((CAbility.EStatIsBasedOnXType x) => x.ToString() == stringValue);
									if (eStatIsBasedOnXType != CAbility.EStatIsBasedOnXType.None)
									{
										statIsBasedOnXData.BasedOn = eStatIsBasedOnXType;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid BasedOn property '" + stringValue + "' in file '" + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "RoundingType":
							{
								if (YMLShared.GetStringPropertyValue(entry2.Value, "RoundingType", filename, out var stringValue2))
								{
									CAbility.EStatIsBasedOnXRoundingType eStatIsBasedOnXRoundingType = CAbility.RoundingTypes.SingleOrDefault((CAbility.EStatIsBasedOnXRoundingType x) => x.ToString() == stringValue2);
									if (eStatIsBasedOnXRoundingType != CAbility.EStatIsBasedOnXRoundingType.None)
									{
										statIsBasedOnXData.RoundingType = eStatIsBasedOnXRoundingType;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid RoundingType property '" + stringValue2 + "' in file '" + filename);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "AddTo":
							{
								if (YMLShared.GetBoolPropertyValue(entry2.Value, "AddTo", filename, out var value))
								{
									statIsBasedOnXData.AddTo = value;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Multiplier":
							{
								if (YMLShared.GetFloatPropertyValue(entry2.Value, "Multiplier", filename, out var value3))
								{
									statIsBasedOnXData.Multiplier = value3;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "SecondVariable":
							{
								if (YMLShared.GetFloatPropertyValue(entry2.Value, "SecondVariable", filename, out var value5))
								{
									statIsBasedOnXData.SecondVariable = value5;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Filter":
							{
								if (CardProcessingShared.GetSingleAbilityFilter(entry2, filename, out var filter))
								{
									statIsBasedOnXData.Filter = filter;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "IncludeUnrevealed":
							{
								if (YMLShared.GetBoolPropertyValue(entry2.Value, "IncludeUnrevealed", filename, out var value6))
								{
									statIsBasedOnXData.IncludeUnrevealed = value6;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MinValue":
							{
								if (YMLShared.GetIntPropertyValue(entry2.Value, "MinValue", filename, out var value4))
								{
									statIsBasedOnXData.MinValue = value4;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "MaxValue":
							{
								if (YMLShared.GetIntPropertyValue(entry2.Value, "MaxValue", filename, out var value2))
								{
									statIsBasedOnXData.MaxValue = value2;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "CheckGUIDs":
							{
								if (YMLShared.GetStringList(entry2.Value, "CheckGUID", filename, out var values))
								{
									statIsBasedOnXData.CheckGUIDs = values.ToList();
								}
								else
								{
									flag = false;
								}
								break;
							}
							}
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX Stat value for stat type " + statString + ", must be Mapping type. File:\n" + filename);
						flag = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX Stat type " + statString + ". File:\n" + filename);
					flag = false;
				}
				statIsBasedOnXEntries.Add(statIsBasedOnXData);
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid StatIsBasedOnX value, must be Mapping type. File:\n" + filename);
			flag = false;
		}
		return flag;
	}

	public static bool GetConditions(MappingEntry abilityEntry, string filename, out List<CCondition.ENegativeCondition> negativeConditions, out List<CCondition.EPositiveCondition> positiveConditions)
	{
		negativeConditions = new List<CCondition.ENegativeCondition>();
		positiveConditions = new List<CCondition.EPositiveCondition>();
		try
		{
			bool result = true;
			if (YMLShared.GetStringList(abilityEntry.Value, "Conditions", filename, out var values))
			{
				foreach (string condition in values)
				{
					CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == condition);
					if (eNegativeCondition != CCondition.ENegativeCondition.NA)
					{
						if (negativeConditions == null)
						{
							negativeConditions = new List<CCondition.ENegativeCondition>();
						}
						negativeConditions.Add(eNegativeCondition);
						continue;
					}
					CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == condition);
					if (ePositiveCondition != CCondition.EPositiveCondition.NA)
					{
						if (positiveConditions == null)
						{
							positiveConditions = new List<CCondition.EPositiveCondition>();
						}
						positiveConditions.Add(ePositiveCondition);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find condition '" + condition + "'. File: " + filename);
						result = false;
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "An exception occurred while parsing conditions.\nException: " + ex.Message + "\nStackTrace:" + ex.StackTrace + "\nFile: " + filename);
			return false;
		}
	}

	private static bool GetConditionalOverride(MappingEntry keyMappingEntry, int cardID, string parentName, bool isMonster, string filename, out CConditionalOverride conditionalOverride)
	{
		conditionalOverride = null;
		bool flag = true;
		string text = keyMappingEntry.Key.ToString();
		CAbilityFilterContainer filter = CAbilityFilterContainer.CreateDefaultFilter();
		CAbilityRequirements requirements = new CAbilityRequirements();
		int xp = 0;
		List<CAbilityOverride> list = new List<CAbilityOverride>();
		bool self = false;
		if (YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "FilterContainer":
				{
					if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter3))
					{
						filter = filter3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Filter":
				{
					if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter2))
					{
						filter = filter2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Requirement":
				case "Requirements":
				{
					if (GetAbilityRequirements(entry, filename, out var abilityRequirements))
					{
						requirements = abilityRequirements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "XP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ConditionalOverride/" + text + "/XP", filename, out var value))
					{
						xp = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Self":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ConditionalOverride/" + text + "/Self", filename, out var value2))
					{
						self = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityOverrides":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping2))
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry2, cardID, isMonster, filename, parentName);
							if (cAbilityOverride != null)
							{
								list.Add(cAbilityOverride);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides.  File: " + filename);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry for AbilityOverrides.  Entry must be mapping.  File: " + filename);
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry.Key.ToString() + " in ConditionOverrides in file " + filename);
					flag = false;
					break;
				}
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in Conditional Override.  Value must be Mapping.  File: " + filename);
			flag = false;
		}
		if (list.Count == 0)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "No overrides specified for Conditional Override.  Only CancelAbilityOnFail conditional overrides can omit ability overrides.  File: " + filename);
			flag = false;
		}
		if (flag)
		{
			conditionalOverride = new CConditionalOverride(text, filter, requirements, xp, list, self);
		}
		return flag;
	}

	public static bool GetAbilityRequirements(MappingEntry keyMappingEntry, string filename, out CAbilityRequirements abilityRequirements)
	{
		abilityRequirements = new CAbilityRequirements();
		CEqualityFilter equalityFilter = null;
		bool result = true;
		if (YMLShared.GetMapping(keyMappingEntry, filename, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "Type":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Type", filename, out var stringValue))
					{
						CAbilityRequirements.EStartAbilityRequirementType eStartAbilityRequirementType = CAbilityRequirements.StartAbilityRequirementTypes.SingleOrDefault((CAbilityRequirements.EStartAbilityRequirementType x) => x.ToString() == stringValue);
						if (eStartAbilityRequirementType != CAbilityRequirements.EStartAbilityRequirementType.None)
						{
							abilityRequirements.StartAbilityRequirementType = eStartAbilityRequirementType;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in StartAbilityConditions/Type " + stringValue + ".  File: " + filename);
						result = false;
					}
					break;
				}
				case "MoveWasStraightLine":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "MoveWasStraightLine", filename, out var value))
					{
						abilityRequirements.AbilityMovementWasStraightLine = value;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "OccupyingObstacleHex":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "OccupyingObstacleHex", filename, out var value10))
					{
						abilityRequirements.OccupyingObstacleHex = value10;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ElementsConsumedThisAction":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ElementsConsumedThisAction", filename, out var value13))
					{
						abilityRequirements.ElementsConsumedThisAction = value13;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "OnYourTurn":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "OnYourTurn", filename, out var value5))
					{
						abilityRequirements.OnYourTurn = value5;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "AbilityHasHappened":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AbilityHasHappened", filename, out var value12))
					{
						abilityRequirements.AbilityHasHappened = value12;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ForgoTopAction":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ForgoTopAction", filename, out var value7))
					{
						abilityRequirements.ForgoTopAction = value7;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ForgoBottomAction":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ForgoBottomAction", filename, out var value14))
					{
						abilityRequirements.ForgoBottomAction = value14;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "EnterHazardousTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "EnterHazardousTerrain", filename, out var value11))
					{
						abilityRequirements.EnterHazardousTerrain = value11;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "EnterDifficultTerrain":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "EnterDifficultTerrain", filename, out var value9))
					{
						abilityRequirements.EnterDifficultTerrain = value9;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActiveBonusAtTrackerIndex":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ActiveBonusAtTrackerIndex", filename, out var value2))
					{
						abilityRequirements.ActiveBonusAtTrackerIndex = value2;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "HP":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "HP", filename, out equalityFilter))
					{
						abilityRequirements.HP = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "HPPercentOfMax":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "HPPercentOfMax", filename, out equalityFilter))
					{
						abilityRequirements.HPPercentOfMax = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "HexesMovedThisTurn":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "HexesMovedThisTurn", filename, out equalityFilter))
					{
						abilityRequirements.HexesMovedThisTurn = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsTargetedByAbility":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsTargetedByAbility", filename, out equalityFilter))
					{
						abilityRequirements.ActorsTargetedByAbility = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledByAbility":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledByAbility", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledByAbility = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledThisTurn":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledThisTurn", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledThisTurn = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorsKilledThisRound":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorsKilledThisRound", filename, out equalityFilter))
					{
						abilityRequirements.ActorsKilledThisRound = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "DoorsOpenedByAbility":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "DoorsOpenedByAbility", filename, out equalityFilter))
					{
						abilityRequirements.DoorsOpenedByAbility = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "WallsAdjacent":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "WallsAdjacent", filename, out equalityFilter))
					{
						abilityRequirements.WallsAdjacent = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorHasPerformedValidAbilityTypesThisTurn":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorHasPerformedValidAbilityTypesThisTurn", filename, out equalityFilter))
					{
						abilityRequirements.ActorHasPerformedValidAbilityTypesThisTurn = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActorHasPerformedValidAbilityTypesThisAction":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActorHasPerformedValidAbilityTypesThisAction", filename, out equalityFilter))
					{
						abilityRequirements.ActorHasPerformedValidAbilityTypesThisAction = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ValidActorsInRangeOfAddedAbilityRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ValidActorsInRangeOfAddedAbilityRange", filename, out equalityFilter))
					{
						abilityRequirements.ValidActorsInRangeOfAddedAbilityRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ValidActorsInRangeOfRequirementRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ValidActorsInRangeOfRequirementRange", filename, out equalityFilter))
					{
						abilityRequirements.ValidActorsInRangeOfRequirementRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "ActiveSpawnersInRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "ActiveSpawnersInRange", filename, out equalityFilter))
					{
						abilityRequirements.ActiveSpawnersInRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "InactiveSpawnersInRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "InactiveSpawnersInRange", filename, out equalityFilter))
					{
						abilityRequirements.InactiveSpawnersInRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "LootInRange":
					if (CardProcessingShared.GetEqualityFilter(entry.Value.ToString(), "LootInRange", filename, out equalityFilter))
					{
						abilityRequirements.LootInRange = equalityFilter;
					}
					else
					{
						result = false;
					}
					break;
				case "InertElements":
				{
					if (YMLShared.GetStringList(entry.Value, "InertElements", filename, out var values5))
					{
						abilityRequirements.InertElements = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString3 in values5)
						{
							try
							{
								ElementInfusionBoardManager.EElement item4 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString3);
								abilityRequirements.InertElements.Add(item4);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString3 + "' specified for InertElements requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "WaningElements":
				{
					if (YMLShared.GetStringList(entry.Value, "WaningElements", filename, out var values4))
					{
						abilityRequirements.WaningElements = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString2 in values4)
						{
							try
							{
								ElementInfusionBoardManager.EElement item3 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString2);
								abilityRequirements.WaningElements.Add(item3);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString2 + "' specified for WaningElements requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "StrongElements":
				{
					if (YMLShared.GetStringList(entry.Value, "StrongElements", filename, out var values3))
					{
						abilityRequirements.StrongElements = new List<ElementInfusionBoardManager.EElement>();
						foreach (string elementString in values3)
						{
							try
							{
								ElementInfusionBoardManager.EElement item2 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString);
								abilityRequirements.StrongElements.Add(item2);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid element '" + elementString + "' specified for StrongElements requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "AbilityTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "AbilityTypes", filename, out var values2))
					{
						abilityRequirements.AbilityTypes = new List<CAbility.EAbilityType>();
						foreach (string abilityString in values2)
						{
							try
							{
								CAbility.EAbilityType item = CAbility.AbilityTypes.Single((CAbility.EAbilityType x) => x.ToString() == abilityString);
								abilityRequirements.AbilityTypes.Add(item);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid ability type '" + abilityString + "' specified for AbilityTypes requirement in file " + filename);
								result = false;
							}
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ObjectImportTypes":
				{
					if (YMLShared.GetStringList(entry.Value, "ObjectImportTypes", filename, out var values))
					{
						abilityRequirements.ObjectImportTypes = new List<ScenarioManager.ObjectImportType>();
						foreach (string lootableTypeString in values)
						{
							ScenarioManager.ObjectImportType objectImportType = ScenarioManager.ObjectImportTypes.SingleOrDefault((ScenarioManager.ObjectImportType x) => x.ToString() == lootableTypeString);
							if (objectImportType != ScenarioManager.ObjectImportType.None)
							{
								abilityRequirements.ObjectImportTypes.Add(objectImportType);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Object Import Type '" + lootableTypeString + "'.  File: " + filename);
							result = false;
						}
					}
					else
					{
						result = false;
					}
					break;
				}
				case "XP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "XP", filename, out var value8))
					{
						abilityRequirements.XP = value8;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequirementRange":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "RequirementRange", filename, out var value6))
					{
						abilityRequirements.RequirementRange = value6;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RoundIsInterval":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "RoundIsInterval", filename, out var value4))
					{
						abilityRequirements.RoundIsInterval = value4;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ActiveBonusInternalRoundInterval":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "ActiveBonusInternalRoundInterval", filename, out var value3))
					{
						abilityRequirements.ActiveBonusInternalRoundInterval = value3;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequirementActorFilterContainer":
				{
					if (CardProcessingShared.GetAbilityFilterContainer(entry, filename, out var filter2))
					{
						abilityRequirements.RequirementActorFilter = filter2;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "RequirementActorFilter":
				{
					if (CardProcessingShared.GetSingleAbilityFilter(entry, filename, out var filter))
					{
						abilityRequirements.RequirementActorFilter = filter;
					}
					else
					{
						result = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to process StartAbilityConditions.  Entry " + entry.Key.ToString() + " was not recognised.  File: " + filename);
					result = false;
					break;
				}
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry in StartAbilityConditions.  Value must be Mapping.  File: " + filename);
			result = false;
		}
		return result;
	}

	private static bool ParseSummon(string summonString, int cardID, string abilityName, string filename, ref List<CEnhancement> abilityEnhancements, ref CAbility.EAbilityType? abilityType, ref ActiveBonusData activeBonusData, out List<string> summons)
	{
		bool result = true;
		summons = null;
		HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonString);
		if (heroSummonYMLData != null)
		{
			if (heroSummonYMLData.AttackEnhancements > 0)
			{
				for (int num = 0; num < heroSummonYMLData.AttackEnhancements; num++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonAttack, cardID, abilityName, num));
				}
			}
			if (heroSummonYMLData.MoveEnhancements > 0)
			{
				for (int num2 = 0; num2 < heroSummonYMLData.MoveEnhancements; num2++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonMove, cardID, abilityName, num2));
				}
			}
			if (heroSummonYMLData.RangeEnhancements > 0)
			{
				for (int num3 = 0; num3 < heroSummonYMLData.RangeEnhancements; num3++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonRange, cardID, abilityName, num3));
				}
			}
			if (heroSummonYMLData.HealthEnhancements > 0)
			{
				for (int num4 = 0; num4 < heroSummonYMLData.HealthEnhancements; num4++)
				{
					abilityEnhancements.Add(new CEnhancement(EEnhancement.NoEnhancement, EEnhancementLine.SummonHealth, cardID, abilityName, num4));
				}
			}
			abilityType = CAbility.EAbilityType.Summon;
			activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Summon;
			summons = new List<string> { summonString, summonString, summonString, summonString };
		}
		else
		{
			abilityType = CAbility.EAbilityType.Summon;
			activeBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Summon;
			summons = new List<string> { summonString, summonString, summonString, summonString };
		}
		return result;
	}
}
