using System.Collections.Generic;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using StateCodeGenerator;
using YamlFormats;

namespace ScenarioRuleLibrary;

public class CAbilityOverride
{
	public string AbilityName { get; private set; }

	public string ParentName { get; private set; }

	public CAbility OriginalAbility { get; set; }

	public CAbility.EAbilityType? AbilityType { get; private set; }

	public int? Strength { get; private set; }

	public int? Range { get; private set; }

	public int? NumberOfTargets { get; private set; }

	public CAbilityFilterContainer AbilityFilter { get; private set; }

	public string AnimationOverload { get; private set; }

	public List<CAbility> SubAbilities { get; private set; }

	public CAreaEffect AreaEffect { get; private set; }

	public bool? AttackSourcesOnly { get; private set; }

	public bool? Jump { get; private set; }

	public bool? Fly { get; private set; }

	public bool? IgnoreDifficultTerrain { get; private set; }

	public bool? IgnoreHazardousTerrain { get; private set; }

	public bool? IgnoreBlockedTileMoveCost { get; private set; }

	public bool? CarryOtherActorsOnHex { get; private set; }

	public CAbilityMove.EMoveRestrictionType? MoveRestrictionType { get; private set; }

	public AbilityData.ActiveBonusData ActiveBonusData { get; private set; }

	public ActiveBonusLayout ActiveBonusYML { get; private set; }

	public string AreaEffectYMLString { get; private set; }

	public string AreaEffectLayoutOverrideYMLString { get; private set; }

	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	public bool? MultiPassAttack { get; private set; }

	public int? DamageSelfBeforeAttack { get; private set; }

	public bool? AddAttackBaseStat { get; private set; }

	public bool? StrengthIsBase { get; private set; }

	public bool? RangeIsBase { get; private set; }

	public bool? TargetIsBase { get; private set; }

	public bool? TargetAtLeastOne { get; private set; }

	public bool? RangeAtLeastOne { get; private set; }

	public bool? RemoveConditionsOverride { get; private set; }

	public string AbilityText { get; private set; }

	public bool? AbilityTextOnly { get; private set; }

	public bool? ShowRange { get; private set; }

	public bool? ShowTarget { get; private set; }

	public bool? ShowArea { get; private set; }

	public bool? OnDeath { get; private set; }

	public bool? AllTargetsOnMovePath { get; private set; }

	public bool? AllTargetsOnMovePathSameStartAndEnd { get; private set; }

	public bool? AllTargetsOnAttackPath { get; private set; }

	public List<string> Summons { get; private set; }

	public AbilityData.XpPerTargetData XpPerTargetData { get; private set; }

	public CAbility.EAbilityTargeting? Targeting { get; private set; }

	public List<ElementInfusionBoardManager.EElement> ElementsToInfuse { get; private set; }

	public bool? ShowElementPicker { get; private set; }

	public string PropName { get; private set; }

	public AbilityData.TrapData TrapData { get; private set; }

	public bool? IsSubAbility { get; private set; }

	public int? Pierce { get; private set; }

	public int? RetaliateRange { get; private set; }

	public bool? IsTargetedAbility { get; private set; }

	public float? SpawnDelay { get; private set; }

	public CAbilityPull.EPullType? PullType { get; private set; }

	public CAbilityPush.EAdditionalPushEffect? AdditionalPushEffect { get; private set; }

	public int? AdditionalPushEffectDamage { get; private set; }

	public int? AdditionalPushEffectXP { get; private set; }

	public int? AbilityXP { get; private set; }

	public List<CConditionalOverride> ConditionalOverrides { get; private set; }

	public CAbilityRequirements StartAbilityRequirements { get; private set; }

	public AbilityData.MiscAbilityData MiscAbilityData { get; private set; }

	public bool? SharePreviousAnim { get; private set; }

	public int? ConditionDuration { get; private set; }

	public EConditionDecTrigger? ConditionDecTrigger { get; private set; }

	public bool? IsInlineSubAbility { get; private set; }

	public bool? IsConsumeAbility { get; private set; }

	public CAugment Augment { get; private set; }

	public CSong Song { get; private set; }

	public CDoom Doom { get; private set; }

	public List<CAttackEffect> AttackEffects { get; private set; }

	public CAbilityControlActor.ControlActorAbilityData ControlActorData { get; private set; }

	public CAbilityChangeAllegiance.ChangeAllegianceAbilityData ChangeAllegianceData { get; private set; }

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; private set; }

	public CAbility AddActiveBonusAbility { get; private set; }

	public List<CAbility> ChooseAbilities { get; private set; }

	public List<CAbility.EAbilityType> RecoverCardsWithAbilityOfTypeFilter { get; private set; }

	public CAbility ForgoTopActionAbility { get; private set; }

	public CAbility ForgoBottomActionAbility { get; private set; }

	public CAbilityExtraTurn.EExtraTurnType? ExtraTurnType { get; private set; }

	public CAbilityFilterContainer SwapFirstTargetAbilityFilter { get; private set; }

	public CAbilityFilterContainer SwapSecondTargetAbilityFilter { get; private set; }

	public CAbilityTeleport.TeleportData TeleportData { get; private set; }

	public CAbilityLoot.LootData LootData { get; private set; }

	public List<CAbility.EAbilityType> ImmunityToAbilityTypes { get; private set; }

	public List<string> ModifierCardNamesToAdd { get; private set; }

	public CAbilityHeal.HealAbilityData HealData { get; private set; }

	public Dictionary<string, int> ResourcesToAddOnAbilityEnd { get; private set; }

	public Dictionary<string, int> ResourcesToTakeFromTargets { get; private set; }

	public Dictionary<string, int> ResourcesToGiveToTargets { get; private set; }

	public CAbilityDestroyObstacle.DestroyObstacleData DestroyObstacleData { get; private set; }

	public string HelpBoxLocalizationKey { get; private set; }

	public CAbilityOverride(string abilityName, string parentName, CAbility.EAbilityType? abilityType = null, int? strength = null, int? range = null, int? numberOfTargets = null, CAbilityFilterContainer abilityFilter = null, string animationOverload = null, List<CAbility> targetAbilities = null, CAreaEffect areaEffect = null, bool? attackSourcesOnly = null, bool? jump = null, bool? fly = null, bool? ignoreDifficultTerrain = null, bool? ignoreHazardousTerrain = null, bool? ignoreBlockedTileMoveCost = null, bool? carryOtherActorsOnHex = null, CAbilityMove.EMoveRestrictionType? moveRestrictionType = null, AbilityData.ActiveBonusData activeBonusData = null, ActiveBonusLayout activeBonusYML = null, string areaEffectYMLString = null, string areaEffectLayoutOverrideYMLString = null, List<CCondition.EPositiveCondition> positiveConditions = null, List<CCondition.ENegativeCondition> negativeConditions = null, bool? multiPassAttack = null, int? damageSelfBeforeAttack = null, bool? addAttackBaseStat = null, bool? strengthIsBase = null, bool? rangeIsBase = null, bool? targetIsBase = null, bool? rangeAtLeastOne = null, bool? targetAtLeastOne = null, bool? removeConditions = null, string text = null, bool? textOnly = null, bool? showRange = null, bool? showTarget = null, bool? showArea = null, bool? onDeath = null, bool? allTargetsOnMovePath = null, bool? allTargetsOnMovePathSameStartAndEnd = null, bool? allTargetsOnAttackPath = null, List<string> summons = null, AbilityData.XpPerTargetData xpPerTargetData = null, CAbility.EAbilityTargeting? targeting = null, List<ElementInfusionBoardManager.EElement> elementsToInfuse = null, bool? showElementPicker = null, string propName = null, AbilityData.TrapData trapData = null, bool? isSubAbility = null, int? pierce = null, int? retaliateRange = null, bool? isTargetedAbility = null, float? spawnDelay = null, CAbilityPull.EPullType? pullType = null, CAbilityPush.EAdditionalPushEffect? additionalPushEffect = null, int? additionalPushEffectDamage = null, int? additionalPushEffectXP = null, int? abilityXP = null, List<CConditionalOverride> conditionalOverrides = null, AbilityData.MiscAbilityData miscAbilityData = null, bool? sharePreviousAnim = null, int? conditionDuration = null, EConditionDecTrigger? conditionDecTrigger = null, bool? isInlineSubAbility = null, bool? isConsumeAbility = null, CAbilityControlActor.ControlActorAbilityData controlActorData = null, CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = null, CAugment augment = null, CSong song = null, CDoom doom = null, List<CAttackEffect> attackEffects = null, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null, CAbility addActiveBonusAbility = null, List<CAbility> chooseAbilities = null, List<CAbility.EAbilityType> recoverCardAbilityTypeFilter = null, CAbility forgoTopActionAbility = null, CAbility forgoBottomActionAbility = null, CAbilityExtraTurn.EExtraTurnType? extraTurnType = null, CAbilityFilterContainer swapFirstTargetAbilityFilter = null, CAbilityFilterContainer swapSecondTargetAbilityFilter = null, CAbilityTeleport.TeleportData teleportData = null, CAbilityLoot.LootData lootData = null, List<CAbility.EAbilityType> immunityToAbilityTypes = null, List<string> addModifiers = null, CAbilityHeal.HealAbilityData healData = null, Dictionary<string, int> resourcesToAddOnAbilityEnd = null, Dictionary<string, int> resourcesToTakeFromTargets = null, Dictionary<string, int> resourcesToGiveToTargets = null, CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = null, string helpBoxLocalizationKey = null)
	{
		AbilityName = abilityName;
		ParentName = parentName;
		AbilityType = abilityType;
		Strength = strength;
		Range = range;
		NumberOfTargets = numberOfTargets;
		AbilityFilter = abilityFilter;
		AnimationOverload = animationOverload;
		SubAbilities = targetAbilities;
		AreaEffect = areaEffect;
		AttackSourcesOnly = attackSourcesOnly;
		Jump = jump;
		Fly = fly;
		IgnoreDifficultTerrain = ignoreDifficultTerrain;
		IgnoreHazardousTerrain = ignoreHazardousTerrain;
		IgnoreBlockedTileMoveCost = ignoreBlockedTileMoveCost;
		CarryOtherActorsOnHex = carryOtherActorsOnHex;
		MoveRestrictionType = moveRestrictionType;
		ActiveBonusData = activeBonusData;
		ActiveBonusYML = activeBonusYML;
		AreaEffectYMLString = areaEffectYMLString;
		AreaEffectLayoutOverrideYMLString = areaEffectLayoutOverrideYMLString;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
		MultiPassAttack = multiPassAttack;
		DamageSelfBeforeAttack = damageSelfBeforeAttack;
		AddAttackBaseStat = addAttackBaseStat;
		StrengthIsBase = strengthIsBase;
		RangeIsBase = rangeIsBase;
		TargetIsBase = targetIsBase;
		RangeAtLeastOne = rangeAtLeastOne;
		TargetAtLeastOne = targetAtLeastOne;
		RemoveConditionsOverride = removeConditions;
		AbilityText = text;
		AbilityTextOnly = textOnly;
		ShowRange = showRange;
		ShowTarget = showTarget;
		ShowArea = showArea;
		OnDeath = onDeath;
		AllTargetsOnMovePath = allTargetsOnMovePath;
		AllTargetsOnMovePathSameStartAndEnd = allTargetsOnMovePathSameStartAndEnd;
		AllTargetsOnAttackPath = allTargetsOnAttackPath;
		Summons = summons;
		XpPerTargetData = xpPerTargetData;
		Targeting = targeting;
		ElementsToInfuse = elementsToInfuse;
		ShowElementPicker = showElementPicker;
		PropName = propName;
		TrapData = trapData;
		IsSubAbility = isSubAbility;
		Pierce = pierce;
		RetaliateRange = retaliateRange;
		IsTargetedAbility = isTargetedAbility;
		SpawnDelay = spawnDelay;
		PullType = pullType;
		AdditionalPushEffect = additionalPushEffect;
		AdditionalPushEffectDamage = additionalPushEffectDamage;
		AdditionalPushEffectXP = additionalPushEffectXP;
		AbilityXP = abilityXP;
		ConditionalOverrides = conditionalOverrides;
		MiscAbilityData = miscAbilityData ?? new AbilityData.MiscAbilityData();
		SharePreviousAnim = sharePreviousAnim;
		ConditionDuration = conditionDuration;
		ConditionDecTrigger = conditionDecTrigger;
		IsInlineSubAbility = isInlineSubAbility;
		IsConsumeAbility = isConsumeAbility;
		ControlActorData = controlActorData;
		ChangeAllegianceData = changeAllegianceData;
		Augment = augment;
		Song = song;
		Doom = doom;
		AttackEffects = attackEffects;
		StatIsBasedOnXEntries = statIsBasedOnXEntries;
		AddActiveBonusAbility = addActiveBonusAbility;
		ChooseAbilities = chooseAbilities;
		RecoverCardsWithAbilityOfTypeFilter = recoverCardAbilityTypeFilter;
		ForgoTopActionAbility = forgoTopActionAbility;
		ForgoBottomActionAbility = forgoBottomActionAbility;
		ExtraTurnType = extraTurnType;
		SwapFirstTargetAbilityFilter = swapFirstTargetAbilityFilter;
		SwapSecondTargetAbilityFilter = swapSecondTargetAbilityFilter;
		TeleportData = teleportData;
		LootData = lootData;
		ImmunityToAbilityTypes = immunityToAbilityTypes;
		ModifierCardNamesToAdd = addModifiers;
		HealData = healData;
		ResourcesToAddOnAbilityEnd = resourcesToAddOnAbilityEnd;
		ResourcesToTakeFromTargets = resourcesToTakeFromTargets;
		ResourcesToGiveToTargets = resourcesToGiveToTargets;
		DestroyObstacleData = destroyObstacleData;
		HelpBoxLocalizationKey = helpBoxLocalizationKey;
	}

	public static CAbilityOverride CreateAbilityOverride(CAbility abilityToOverride, int? overrideStrength, int? overrideRange, int? overrideNumberOfTargets)
	{
		string name = abilityToOverride.Name;
		CAbility.EAbilityType? abilityType = abilityToOverride.AbilityType;
		return new CAbilityOverride("", name, abilityType, overrideStrength, overrideRange, overrideNumberOfTargets);
	}

	public static CAbilityOverride CreateAbilityOverride(MappingEntry overrideEntry, int cardID, bool isMonster, string filename, string parentNameParam = "", bool isConsume = false, string abilityNameOverride = null)
	{
		string text = abilityNameOverride ?? overrideEntry.Key.ToString();
		string parentName = parentNameParam;
		CAbility.EAbilityType? abilityType = null;
		string propName = null;
		AbilityData.TrapData trapData = null;
		bool isSubAbility = false;
		int? strength = null;
		int? range = null;
		int? numberOfTargets = null;
		bool numberOfTargetsSet = false;
		CAbilityFilterContainer filter = null;
		string animationOverload = null;
		List<CAbility> subAbilities = null;
		CAreaEffect areaEffect = null;
		bool? attackSourcesOnly = null;
		bool? jump = null;
		bool? fly = null;
		bool? ignoreDifficultTerrain = null;
		bool? ignoreHazardousTerrain = null;
		bool? ignoreBlockedTileMoveCost = null;
		bool? carryOtherActorsOnHex = null;
		CAIFocusOverrideDetails aiFocusOverride = null;
		CAbilityMove.EMoveRestrictionType? moveRestrictionType = null;
		AbilityData.ActiveBonusData activeBonusData = null;
		ActiveBonusLayout activeBonusYML = null;
		string areaEffectYMLString = null;
		string areaEffectLayoutOverrideYMLString = null;
		List<CCondition.EPositiveCondition> positiveConditions = null;
		List<CCondition.ENegativeCondition> negativeConditions = null;
		bool? multiPassAttack = null;
		bool? chainAttack = null;
		int? chainAttackRange = null;
		int? chainAttackDamageReduction = null;
		int? damageSelfBeforeAttack = null;
		bool? addAttackBaseStat = null;
		bool? strengthIsBase = null;
		bool? rangeIsBase = null;
		bool? targetIsBase = null;
		bool? rangeAtLeastOne = null;
		bool? targetAtLeastOne = null;
		bool? removeConditionsOverride = null;
		string abilityText = null;
		bool? abilityTextOnly = null;
		bool? showRange = null;
		bool? showTarget = null;
		bool? showArea = null;
		bool? onDeath = null;
		bool? allTargetsOnMovePath = null;
		bool? allTargetsOnMovePathSameStartAndEnd = null;
		bool? allTargetsOnAttackPath = null;
		List<string> summons = null;
		AbilityData.XpPerTargetData xpPerTargetData = null;
		CAbility.EAbilityTargeting? targeting = null;
		List<ElementInfusionBoardManager.EElement> elementsToInfuse = null;
		bool? showElementPicker = null;
		int? pierce = null;
		int? retaliateRange = null;
		bool? isTargetedAbility = null;
		float? spawnDelay = null;
		CAbilityPull.EPullType? pullType = null;
		CAbilityPush.EAdditionalPushEffect? additionalPushEffect = null;
		int? additionalPushEffectDamage = null;
		int? additionalPushEffectXP = null;
		CAbilityLoot.LootData lootData = null;
		List<CConditionalOverride> conditionalOverrides = new List<CConditionalOverride>();
		CAbilityRequirements startAbilityRequirements = new CAbilityRequirements();
		int? abilityXP = null;
		AbilityData.MiscAbilityData miscAbilityData = new AbilityData.MiscAbilityData();
		bool? skipAnim = null;
		int? conditionDuration = null;
		EConditionDecTrigger? conditionDecTrigger = null;
		bool? isInlineSubAbility = null;
		bool? flag = isConsume;
		CAugment augment = null;
		CSong song = null;
		CDoom doom = null;
		List<CAttackEffect> attackEffects = new List<CAttackEffect>();
		CAbilityControlActor.ControlActorAbilityData controlActorAbilityData = null;
		CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceAbilityData = null;
		List<CAbility> mergedAbilities = null;
		bool? targetActorWithTrapEffects = false;
		int? targetActorWithTrapEffectRange = 0;
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null;
		int? moveObstacleRange = null;
		string moveObstacleAnimOverload = null;
		List<CItem.EItemSlot> slotsToRefresh = null;
		List<CItem.EItemSlotState> slotStatesToRefresh = null;
		CAbility addActiveBonusAbility = null;
		CAbility.EAttackType? overrideAugmentAttackType = null;
		List<CAbility> chooseAbilities = null;
		List<CAbility.EAbilityType> recoverCardsAbilityTypeFilter = null;
		CAbility forgoTopActionAbility = null;
		CAbility forgoBottomActionAbility = null;
		CAbilityExtraTurn.EExtraTurnType? extraTurnType = null;
		CAbilityFilterContainer swapAbilityFirstTargetFilter = null;
		CAbilityFilterContainer swapAbilitySecondTargetFilter = null;
		List<int> supplyCardsToGive = null;
		CAbilityTeleport.TeleportData teleportData = null;
		List<CAbility.EAbilityType> immunityToAbilityTypes = null;
		List<CAbility.EAttackType> immuneToAttackTypes = null;
		List<string> addModifiers = null;
		CAbilityHeal.HealAbilityData healAbilityData = null;
		Dictionary<string, int> resourcesToAddOnAbilityEnd = null;
		Dictionary<string, int> resourcesToTakeFromTargets = null;
		Dictionary<string, int> resourcesToGiveToTargets = null;
		string previewEffectId = null;
		string previewEffectText = null;
		string helpBoxTooltipLocKey = null;
		CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData = null;
		ECharacter changeCharacterModel = ECharacter.None;
		CAbilityFilter.EFilterTile tileFilter = CAbilityFilter.EFilterTile.None;
		CAbilityDisableCardAction.DisableCardActionData disableCardActionData = null;
		List<CEnhancement> abilityEnhancements = new List<CEnhancement>();
		bool? useSpecialBaseStat = false;
		if (YMLShared.GetMapping(overrideEntry, filename, out var mapping))
		{
			if (AbilityData.ParseAbilityProperties(mapping, text, cardID, isMonster, isAbilityOverride: true, ref isSubAbility, ref parentName, ref abilityType, ref propName, ref trapData, ref abilityEnhancements, ref strength, ref useSpecialBaseStat, ref range, ref numberOfTargets, ref numberOfTargetsSet, ref filter, ref animationOverload, ref subAbilities, ref areaEffect, ref attackSourcesOnly, ref jump, ref fly, ref ignoreDifficultTerrain, ref ignoreHazardousTerrain, ref ignoreBlockedTileMoveCost, ref aiFocusOverride, ref carryOtherActorsOnHex, ref moveRestrictionType, ref activeBonusData, ref activeBonusYML, ref areaEffectYMLString, ref areaEffectLayoutOverrideYMLString, ref positiveConditions, ref negativeConditions, ref multiPassAttack, ref chainAttack, ref chainAttackRange, ref chainAttackDamageReduction, ref damageSelfBeforeAttack, ref addAttackBaseStat, ref strengthIsBase, ref rangeIsBase, ref targetIsBase, ref rangeAtLeastOne, ref targetAtLeastOne, ref removeConditionsOverride, ref abilityText, ref abilityTextOnly, ref showRange, ref showTarget, ref showArea, ref onDeath, ref allTargetsOnMovePath, ref allTargetsOnMovePathSameStartAndEnd, ref allTargetsOnAttackPath, ref summons, ref xpPerTargetData, ref targeting, ref elementsToInfuse, ref showElementPicker, ref pierce, ref retaliateRange, ref isTargetedAbility, ref spawnDelay, ref pullType, ref additionalPushEffect, ref additionalPushEffectDamage, ref additionalPushEffectXP, ref lootData, ref conditionalOverrides, ref startAbilityRequirements, ref abilityXP, ref miscAbilityData, ref skipAnim, ref conditionDuration, ref conditionDecTrigger, ref isInlineSubAbility, ref augment, ref song, ref doom, ref attackEffects, ref controlActorAbilityData, ref changeAllegianceAbilityData, ref mergedAbilities, ref targetActorWithTrapEffects, ref targetActorWithTrapEffectRange, ref statIsBasedOnXEntries, ref moveObstacleRange, ref moveObstacleAnimOverload, ref slotsToRefresh, ref slotStatesToRefresh, ref addActiveBonusAbility, ref overrideAugmentAttackType, ref chooseAbilities, ref recoverCardsAbilityTypeFilter, ref forgoTopActionAbility, ref forgoBottomActionAbility, ref extraTurnType, ref swapAbilityFirstTargetFilter, ref swapAbilitySecondTargetFilter, ref supplyCardsToGive, ref teleportData, ref immunityToAbilityTypes, ref immuneToAttackTypes, ref addModifiers, ref healAbilityData, ref resourcesToAddOnAbilityEnd, ref resourcesToTakeFromTargets, ref resourcesToGiveToTargets, ref destroyObstacleData, ref changeCharacterModel, ref tileFilter, ref disableCardActionData, ref previewEffectId, ref previewEffectText, ref helpBoxTooltipLocKey, filename))
			{
				string parentName2 = parentName;
				CAbility.EAbilityType? abilityType2 = abilityType;
				int? strength2 = strength;
				int? range2 = range;
				int? numberOfTargets2 = numberOfTargets;
				CAbilityFilterContainer abilityFilter = filter;
				string animationOverload2 = animationOverload;
				List<CAbility> targetAbilities = subAbilities;
				CAreaEffect areaEffect2 = areaEffect;
				bool? attackSourcesOnly2 = attackSourcesOnly;
				bool? jump2 = jump;
				bool? fly2 = fly;
				bool? ignoreDifficultTerrain2 = ignoreDifficultTerrain;
				bool? ignoreHazardousTerrain2 = ignoreHazardousTerrain;
				bool? carryOtherActorsOnHex2 = carryOtherActorsOnHex;
				CAbilityMove.EMoveRestrictionType? moveRestrictionType2 = moveRestrictionType;
				AbilityData.ActiveBonusData activeBonusData2 = activeBonusData;
				ActiveBonusLayout activeBonusYML2 = activeBonusYML;
				string areaEffectYMLString2 = areaEffectYMLString;
				string areaEffectLayoutOverrideYMLString2 = areaEffectLayoutOverrideYMLString;
				List<CCondition.EPositiveCondition> positiveConditions2 = positiveConditions;
				List<CCondition.ENegativeCondition> negativeConditions2 = negativeConditions;
				bool? multiPassAttack2 = multiPassAttack;
				int? damageSelfBeforeAttack2 = damageSelfBeforeAttack;
				bool? addAttackBaseStat2 = addAttackBaseStat;
				bool? strengthIsBase2 = strengthIsBase;
				bool? rangeIsBase2 = rangeIsBase;
				bool? targetIsBase2 = targetIsBase;
				bool? rangeAtLeastOne2 = rangeAtLeastOne;
				bool? targetAtLeastOne2 = targetAtLeastOne;
				bool? removeConditions = removeConditionsOverride;
				string text2 = abilityText;
				bool? textOnly = abilityTextOnly;
				bool? showRange2 = showRange;
				bool? showTarget2 = showTarget;
				bool? showArea2 = showArea;
				bool? onDeath2 = onDeath;
				bool? allTargetsOnMovePath2 = allTargetsOnMovePath;
				bool? allTargetsOnMovePathSameStartAndEnd2 = allTargetsOnMovePathSameStartAndEnd;
				bool? allTargetsOnAttackPath2 = allTargetsOnAttackPath;
				List<string> summons2 = summons;
				AbilityData.XpPerTargetData xpPerTargetData2 = xpPerTargetData;
				CAbility.EAbilityTargeting? targeting2 = targeting;
				List<ElementInfusionBoardManager.EElement> elementsToInfuse2 = elementsToInfuse;
				bool? showElementPicker2 = showElementPicker;
				string propName2 = propName;
				AbilityData.TrapData trapData2 = trapData;
				bool? isSubAbility2 = null;
				int? pierce2 = pierce;
				int? retaliateRange2 = retaliateRange;
				bool? isTargetedAbility2 = isTargetedAbility;
				float? spawnDelay2 = spawnDelay;
				CAbilityPull.EPullType? pullType2 = pullType;
				CAbilityPush.EAdditionalPushEffect? additionalPushEffect2 = additionalPushEffect;
				int? additionalPushEffectDamage2 = additionalPushEffectDamage;
				int? additionalPushEffectXP2 = additionalPushEffectXP;
				int? abilityXP2 = abilityXP;
				List<CConditionalOverride> conditionalOverrides2 = conditionalOverrides;
				AbilityData.MiscAbilityData miscAbilityData2 = miscAbilityData;
				bool? sharePreviousAnim = skipAnim;
				int? conditionDuration2 = conditionDuration;
				EConditionDecTrigger? conditionDecTrigger2 = conditionDecTrigger;
				bool? isInlineSubAbility2 = isInlineSubAbility;
				bool? isConsumeAbility = flag;
				CAbilityControlActor.ControlActorAbilityData controlActorData = controlActorAbilityData;
				CAbilityChangeAllegiance.ChangeAllegianceAbilityData changeAllegianceData = changeAllegianceAbilityData;
				CAugment augment2 = augment;
				CSong song2 = song;
				CDoom doom2 = doom;
				List<CAttackEffect> attackEffects2 = attackEffects;
				List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries2 = statIsBasedOnXEntries;
				CAbility addActiveBonusAbility2 = addActiveBonusAbility;
				List<CAbility> chooseAbilities2 = chooseAbilities;
				List<CAbility.EAbilityType> recoverCardAbilityTypeFilter = recoverCardsAbilityTypeFilter;
				CAbility forgoTopActionAbility2 = forgoTopActionAbility;
				CAbility forgoBottomActionAbility2 = forgoBottomActionAbility;
				CAbilityExtraTurn.EExtraTurnType? extraTurnType2 = extraTurnType;
				CAbilityFilterContainer swapFirstTargetAbilityFilter = swapAbilityFirstTargetFilter;
				CAbilityFilterContainer swapSecondTargetAbilityFilter = swapAbilitySecondTargetFilter;
				CAbilityTeleport.TeleportData teleportData2 = teleportData;
				List<CAbility.EAbilityType> immunityToAbilityTypes2 = immunityToAbilityTypes;
				List<string> addModifiers2 = addModifiers;
				Dictionary<string, int> resourcesToAddOnAbilityEnd2 = resourcesToAddOnAbilityEnd;
				Dictionary<string, int> resourcesToGiveToTargets2 = resourcesToGiveToTargets;
				Dictionary<string, int> resourcesToTakeFromTargets2 = resourcesToTakeFromTargets;
				CAbilityDestroyObstacle.DestroyObstacleData destroyObstacleData2 = destroyObstacleData;
				string helpBoxLocalizationKey = helpBoxTooltipLocKey;
				return new CAbilityOverride(text, parentName2, abilityType2, strength2, range2, numberOfTargets2, abilityFilter, animationOverload2, targetAbilities, areaEffect2, attackSourcesOnly2, jump2, fly2, ignoreDifficultTerrain2, ignoreHazardousTerrain2, null, carryOtherActorsOnHex2, moveRestrictionType2, activeBonusData2, activeBonusYML2, areaEffectYMLString2, areaEffectLayoutOverrideYMLString2, positiveConditions2, negativeConditions2, multiPassAttack2, damageSelfBeforeAttack2, addAttackBaseStat2, strengthIsBase2, rangeIsBase2, targetIsBase2, rangeAtLeastOne2, targetAtLeastOne2, removeConditions, text2, textOnly, showRange2, showTarget2, showArea2, onDeath2, allTargetsOnMovePath2, allTargetsOnMovePathSameStartAndEnd2, allTargetsOnAttackPath2, summons2, xpPerTargetData2, targeting2, elementsToInfuse2, showElementPicker2, propName2, trapData2, isSubAbility2, pierce2, retaliateRange2, isTargetedAbility2, spawnDelay2, pullType2, additionalPushEffect2, additionalPushEffectDamage2, additionalPushEffectXP2, abilityXP2, conditionalOverrides2, miscAbilityData2, sharePreviousAnim, conditionDuration2, conditionDecTrigger2, isInlineSubAbility2, isConsumeAbility, controlActorData, changeAllegianceData, augment2, song2, doom2, attackEffects2, statIsBasedOnXEntries2, addActiveBonusAbility2, chooseAbilities2, recoverCardAbilityTypeFilter, forgoTopActionAbility2, forgoBottomActionAbility2, extraTurnType2, swapFirstTargetAbilityFilter, swapSecondTargetAbilityFilter, teleportData2, null, immunityToAbilityTypes2, addModifiers2, null, resourcesToAddOnAbilityEnd2, resourcesToTakeFromTargets2, resourcesToGiveToTargets2, destroyObstacleData2, helpBoxLocalizationKey);
			}
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Failed to parse ability properties for Ability Override. File: " + filename);
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid property in AbilityOverride.  Property must be Mapping. File: " + filename);
		}
		return null;
	}

	public CAbilityOverride()
	{
	}

	public CAbilityOverride(CAbilityOverride state, ReferenceDictionary references)
	{
		AbilityName = state.AbilityName;
		ParentName = state.ParentName;
		AbilityType = state.AbilityType;
		Strength = state.Strength;
		Range = state.Range;
		NumberOfTargets = state.NumberOfTargets;
		AbilityFilter = references.Get(state.AbilityFilter);
		if (AbilityFilter == null && state.AbilityFilter != null)
		{
			AbilityFilter = new CAbilityFilterContainer(state.AbilityFilter, references);
			references.Add(state.AbilityFilter, AbilityFilter);
		}
		AnimationOverload = state.AnimationOverload;
		SubAbilities = references.Get(state.SubAbilities);
		if (SubAbilities == null && state.SubAbilities != null)
		{
			SubAbilities = new List<CAbility>();
			for (int i = 0; i < state.SubAbilities.Count; i++)
			{
				CAbility cAbility = state.SubAbilities[i];
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
		AttackSourcesOnly = state.AttackSourcesOnly;
		Jump = state.Jump;
		Fly = state.Fly;
		IgnoreDifficultTerrain = state.IgnoreDifficultTerrain;
		IgnoreHazardousTerrain = state.IgnoreHazardousTerrain;
		IgnoreBlockedTileMoveCost = state.IgnoreBlockedTileMoveCost;
		CarryOtherActorsOnHex = state.CarryOtherActorsOnHex;
		MoveRestrictionType = state.MoveRestrictionType;
		AreaEffectYMLString = state.AreaEffectYMLString;
		AreaEffectLayoutOverrideYMLString = state.AreaEffectLayoutOverrideYMLString;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<CCondition.EPositiveCondition>();
			for (int j = 0; j < state.PositiveConditions.Count; j++)
			{
				CCondition.EPositiveCondition item = state.PositiveConditions[j];
				PositiveConditions.Add(item);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions == null && state.NegativeConditions != null)
		{
			NegativeConditions = new List<CCondition.ENegativeCondition>();
			for (int k = 0; k < state.NegativeConditions.Count; k++)
			{
				CCondition.ENegativeCondition item2 = state.NegativeConditions[k];
				NegativeConditions.Add(item2);
			}
			references.Add(state.NegativeConditions, NegativeConditions);
		}
		MultiPassAttack = state.MultiPassAttack;
		DamageSelfBeforeAttack = state.DamageSelfBeforeAttack;
		AddAttackBaseStat = state.AddAttackBaseStat;
		StrengthIsBase = state.StrengthIsBase;
		RangeIsBase = state.RangeIsBase;
		TargetIsBase = state.TargetIsBase;
		TargetAtLeastOne = state.TargetAtLeastOne;
		RangeAtLeastOne = state.RangeAtLeastOne;
		RemoveConditionsOverride = state.RemoveConditionsOverride;
		AbilityText = state.AbilityText;
		AbilityTextOnly = state.AbilityTextOnly;
		ShowRange = state.ShowRange;
		ShowTarget = state.ShowTarget;
		ShowArea = state.ShowArea;
		OnDeath = state.OnDeath;
		AllTargetsOnMovePath = state.AllTargetsOnMovePath;
		AllTargetsOnMovePathSameStartAndEnd = state.AllTargetsOnMovePathSameStartAndEnd;
		AllTargetsOnAttackPath = state.AllTargetsOnAttackPath;
		Summons = references.Get(state.Summons);
		if (Summons == null && state.Summons != null)
		{
			Summons = new List<string>();
			for (int l = 0; l < state.Summons.Count; l++)
			{
				string item3 = state.Summons[l];
				Summons.Add(item3);
			}
			references.Add(state.Summons, Summons);
		}
		Targeting = state.Targeting;
		ElementsToInfuse = references.Get(state.ElementsToInfuse);
		if (ElementsToInfuse == null && state.ElementsToInfuse != null)
		{
			ElementsToInfuse = new List<ElementInfusionBoardManager.EElement>();
			for (int m = 0; m < state.ElementsToInfuse.Count; m++)
			{
				ElementInfusionBoardManager.EElement item4 = state.ElementsToInfuse[m];
				ElementsToInfuse.Add(item4);
			}
			references.Add(state.ElementsToInfuse, ElementsToInfuse);
		}
		ShowElementPicker = state.ShowElementPicker;
		PropName = state.PropName;
		IsSubAbility = state.IsSubAbility;
		Pierce = state.Pierce;
		RetaliateRange = state.RetaliateRange;
		IsTargetedAbility = state.IsTargetedAbility;
		SpawnDelay = state.SpawnDelay;
		PullType = state.PullType;
		AdditionalPushEffect = state.AdditionalPushEffect;
		AdditionalPushEffectDamage = state.AdditionalPushEffectDamage;
		AdditionalPushEffectXP = state.AdditionalPushEffectXP;
		AbilityXP = state.AbilityXP;
		ConditionalOverrides = references.Get(state.ConditionalOverrides);
		if (ConditionalOverrides == null && state.ConditionalOverrides != null)
		{
			ConditionalOverrides = new List<CConditionalOverride>();
			for (int n = 0; n < state.ConditionalOverrides.Count; n++)
			{
				CConditionalOverride cConditionalOverride = state.ConditionalOverrides[n];
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
		SharePreviousAnim = state.SharePreviousAnim;
		ConditionDuration = state.ConditionDuration;
		ConditionDecTrigger = state.ConditionDecTrigger;
		IsInlineSubAbility = state.IsInlineSubAbility;
		IsConsumeAbility = state.IsConsumeAbility;
		AttackEffects = references.Get(state.AttackEffects);
		if (AttackEffects == null && state.AttackEffects != null)
		{
			AttackEffects = new List<CAttackEffect>();
			for (int num = 0; num < state.AttackEffects.Count; num++)
			{
				CAttackEffect cAttackEffect = state.AttackEffects[num];
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
		StatIsBasedOnXEntries = references.Get(state.StatIsBasedOnXEntries);
		if (StatIsBasedOnXEntries == null && state.StatIsBasedOnXEntries != null)
		{
			StatIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>();
			for (int num2 = 0; num2 < state.StatIsBasedOnXEntries.Count; num2++)
			{
				AbilityData.StatIsBasedOnXData statIsBasedOnXData = state.StatIsBasedOnXEntries[num2];
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
		ChooseAbilities = references.Get(state.ChooseAbilities);
		if (ChooseAbilities == null && state.ChooseAbilities != null)
		{
			ChooseAbilities = new List<CAbility>();
			for (int num3 = 0; num3 < state.ChooseAbilities.Count; num3++)
			{
				CAbility cAbility4 = state.ChooseAbilities[num3];
				CAbility cAbility5 = references.Get(cAbility4);
				if (cAbility5 == null && cAbility4 != null)
				{
					CAbility cAbility3 = ((cAbility4 is CAbilityBlockHealing state106) ? new CAbilityBlockHealing(state106, references) : ((cAbility4 is CAbilityNeutralizeShield state107) ? new CAbilityNeutralizeShield(state107, references) : ((cAbility4 is CAbilityAdvantage state108) ? new CAbilityAdvantage(state108, references) : ((cAbility4 is CAbilityBless state109) ? new CAbilityBless(state109, references) : ((cAbility4 is CAbilityCurse state110) ? new CAbilityCurse(state110, references) : ((cAbility4 is CAbilityDisarm state111) ? new CAbilityDisarm(state111, references) : ((cAbility4 is CAbilityImmobilize state112) ? new CAbilityImmobilize(state112, references) : ((cAbility4 is CAbilityImmovable state113) ? new CAbilityImmovable(state113, references) : ((cAbility4 is CAbilityInvisible state114) ? new CAbilityInvisible(state114, references) : ((cAbility4 is CAbilityMuddle state115) ? new CAbilityMuddle(state115, references) : ((cAbility4 is CAbilityOverheal state116) ? new CAbilityOverheal(state116, references) : ((cAbility4 is CAbilityPoison state117) ? new CAbilityPoison(state117, references) : ((cAbility4 is CAbilitySleep state118) ? new CAbilitySleep(state118, references) : ((cAbility4 is CAbilityStopFlying state119) ? new CAbilityStopFlying(state119, references) : ((cAbility4 is CAbilityStrengthen state120) ? new CAbilityStrengthen(state120, references) : ((cAbility4 is CAbilityStun state121) ? new CAbilityStun(state121, references) : ((cAbility4 is CAbilityWound state122) ? new CAbilityWound(state122, references) : ((cAbility4 is CAbilityChooseAbility state123) ? new CAbilityChooseAbility(state123, references) : ((cAbility4 is CAbilityAddActiveBonus state124) ? new CAbilityAddActiveBonus(state124, references) : ((cAbility4 is CAbilityAddAugment state125) ? new CAbilityAddAugment(state125, references) : ((cAbility4 is CAbilityAddCondition state126) ? new CAbilityAddCondition(state126, references) : ((cAbility4 is CAbilityAddDoom state127) ? new CAbilityAddDoom(state127, references) : ((cAbility4 is CAbilityAddDoomSlots state128) ? new CAbilityAddDoomSlots(state128, references) : ((cAbility4 is CAbilityAddHeal state129) ? new CAbilityAddHeal(state129, references) : ((cAbility4 is CAbilityAddRange state130) ? new CAbilityAddRange(state130, references) : ((cAbility4 is CAbilityAddSong state131) ? new CAbilityAddSong(state131, references) : ((cAbility4 is CAbilityAddTarget state132) ? new CAbilityAddTarget(state132, references) : ((cAbility4 is CAbilityAdjustInitiative state133) ? new CAbilityAdjustInitiative(state133, references) : ((cAbility4 is CAbilityAttackersGainDisadvantage state134) ? new CAbilityAttackersGainDisadvantage(state134, references) : ((cAbility4 is CAbilityChangeAllegiance state135) ? new CAbilityChangeAllegiance(state135, references) : ((cAbility4 is CAbilityChangeCharacterModel state136) ? new CAbilityChangeCharacterModel(state136, references) : ((cAbility4 is CAbilityChoose state137) ? new CAbilityChoose(state137, references) : ((cAbility4 is CAbilityConsume state138) ? new CAbilityConsume(state138, references) : ((cAbility4 is CAbilityConsumeItemCards state139) ? new CAbilityConsumeItemCards(state139, references) : ((cAbility4 is CAbilityControlActor state140) ? new CAbilityControlActor(state140, references) : ((cAbility4 is CAbilityDisableCardAction state141) ? new CAbilityDisableCardAction(state141, references) : ((cAbility4 is CAbilityDiscardCards state142) ? new CAbilityDiscardCards(state142, references) : ((cAbility4 is CAbilityExtraTurn state143) ? new CAbilityExtraTurn(state143, references) : ((cAbility4 is CAbilityForgoActionsForCompanion state144) ? new CAbilityForgoActionsForCompanion(state144, references) : ((cAbility4 is CAbilityGiveSupplyCard state145) ? new CAbilityGiveSupplyCard(state145, references) : ((cAbility4 is CAbilityHeal state146) ? new CAbilityHeal(state146, references) : ((cAbility4 is CAbilityHealthReduction state147) ? new CAbilityHealthReduction(state147, references) : ((cAbility4 is CAbilityImmunityTo state148) ? new CAbilityImmunityTo(state148, references) : ((cAbility4 is CAbilityImprovedShortRest state149) ? new CAbilityImprovedShortRest(state149, references) : ((cAbility4 is CAbilityIncreaseCardLimit state150) ? new CAbilityIncreaseCardLimit(state150, references) : ((cAbility4 is CAbilityInvulnerability state151) ? new CAbilityInvulnerability(state151, references) : ((cAbility4 is CAbilityItemLock state152) ? new CAbilityItemLock(state152, references) : ((cAbility4 is CAbilityKill state153) ? new CAbilityKill(state153, references) : ((cAbility4 is CAbilityLoseCards state154) ? new CAbilityLoseCards(state154, references) : ((cAbility4 is CAbilityLoseGoalChestReward state155) ? new CAbilityLoseGoalChestReward(state155, references) : ((cAbility4 is CAbilityNullTargeting state156) ? new CAbilityNullTargeting(state156, references) : ((cAbility4 is CAbilityOverrideAugmentAttackType state157) ? new CAbilityOverrideAugmentAttackType(state157, references) : ((cAbility4 is CAbilityPierceInvulnerability state158) ? new CAbilityPierceInvulnerability(state158, references) : ((cAbility4 is CAbilityPlaySong state159) ? new CAbilityPlaySong(state159, references) : ((cAbility4 is CAbilityPreventDamage state160) ? new CAbilityPreventDamage(state160, references) : ((cAbility4 is CAbilityRecoverDiscardedCards state161) ? new CAbilityRecoverDiscardedCards(state161, references) : ((cAbility4 is CAbilityRecoverLostCards state162) ? new CAbilityRecoverLostCards(state162, references) : ((cAbility4 is CAbilityRedirect state163) ? new CAbilityRedirect(state163, references) : ((cAbility4 is CAbilityRefreshItemCards state164) ? new CAbilityRefreshItemCards(state164, references) : ((cAbility4 is CAbilityRemoveActorFromMap state165) ? new CAbilityRemoveActorFromMap(state165, references) : ((cAbility4 is CAbilityRemoveConditions state166) ? new CAbilityRemoveConditions(state166, references) : ((cAbility4 is CAbilityRetaliate state167) ? new CAbilityRetaliate(state167, references) : ((cAbility4 is CAbilityShield state168) ? new CAbilityShield(state168, references) : ((cAbility4 is CAbilityShuffleModifierDeck state169) ? new CAbilityShuffleModifierDeck(state169, references) : ((cAbility4 is CAbilityTransferDooms state170) ? new CAbilityTransferDooms(state170, references) : ((cAbility4 is CAbilityUntargetable state171) ? new CAbilityUntargetable(state171, references) : ((cAbility4 is CAbilityCondition state172) ? new CAbilityCondition(state172, references) : ((cAbility4 is CAbilityMergedCreateAttack state173) ? new CAbilityMergedCreateAttack(state173, references) : ((cAbility4 is CAbilityMergedDestroyAttack state174) ? new CAbilityMergedDestroyAttack(state174, references) : ((cAbility4 is CAbilityMergedDisarmTrapDestroyObstacle state175) ? new CAbilityMergedDisarmTrapDestroyObstacle(state175, references) : ((cAbility4 is CAbilityMergedKillCreate state176) ? new CAbilityMergedKillCreate(state176, references) : ((cAbility4 is CAbilityMergedMoveAttack state177) ? new CAbilityMergedMoveAttack(state177, references) : ((cAbility4 is CAbilityMergedMoveObstacleAttack state178) ? new CAbilityMergedMoveObstacleAttack(state178, references) : ((cAbility4 is CAbilityActivateSpawner state179) ? new CAbilityActivateSpawner(state179, references) : ((cAbility4 is CAbilityAddModifierToMonsterDeck state180) ? new CAbilityAddModifierToMonsterDeck(state180, references) : ((cAbility4 is CAbilityAttack state181) ? new CAbilityAttack(state181, references) : ((cAbility4 is CAbilityChangeCondition state182) ? new CAbilityChangeCondition(state182, references) : ((cAbility4 is CAbilityChangeModifier state183) ? new CAbilityChangeModifier(state183, references) : ((cAbility4 is CAbilityConsumeElement state184) ? new CAbilityConsumeElement(state184, references) : ((cAbility4 is CAbilityCreate state185) ? new CAbilityCreate(state185, references) : ((cAbility4 is CAbilityDamage state186) ? new CAbilityDamage(state186, references) : ((cAbility4 is CAbilityDeactivateSpawner state187) ? new CAbilityDeactivateSpawner(state187, references) : ((cAbility4 is CAbilityDestroyObstacle state188) ? new CAbilityDestroyObstacle(state188, references) : ((cAbility4 is CAbilityDisarmTrap state189) ? new CAbilityDisarmTrap(state189, references) : ((cAbility4 is CAbilityFear state190) ? new CAbilityFear(state190, references) : ((cAbility4 is CAbilityInfuse state191) ? new CAbilityInfuse(state191, references) : ((cAbility4 is CAbilityLoot state192) ? new CAbilityLoot(state192, references) : ((cAbility4 is CAbilityMove state193) ? new CAbilityMove(state193, references) : ((cAbility4 is CAbilityMoveObstacle state194) ? new CAbilityMoveObstacle(state194, references) : ((cAbility4 is CAbilityMoveTrap state195) ? new CAbilityMoveTrap(state195, references) : ((cAbility4 is CAbilityNull state196) ? new CAbilityNull(state196, references) : ((cAbility4 is CAbilityNullHex state197) ? new CAbilityNullHex(state197, references) : ((cAbility4 is CAbilityPull state198) ? new CAbilityPull(state198, references) : ((cAbility4 is CAbilityPush state199) ? new CAbilityPush(state199, references) : ((cAbility4 is CAbilityRecoverResources state200) ? new CAbilityRecoverResources(state200, references) : ((cAbility4 is CAbilityRedistributeDamage state201) ? new CAbilityRedistributeDamage(state201, references) : ((cAbility4 is CAbilityRevive state202) ? new CAbilityRevive(state202, references) : ((cAbility4 is CAbilitySummon state203) ? new CAbilitySummon(state203, references) : ((cAbility4 is CAbilitySwap state204) ? new CAbilitySwap(state204, references) : ((cAbility4 is CAbilityTargeting state205) ? new CAbilityTargeting(state205, references) : ((cAbility4 is CAbilityTeleport state206) ? new CAbilityTeleport(state206, references) : ((cAbility4 is CAbilityTrap state207) ? new CAbilityTrap(state207, references) : ((cAbility4 is CAbilityXP state208) ? new CAbilityXP(state208, references) : ((!(cAbility4 is CAbilityMerged state209)) ? new CAbility(cAbility4, references) : new CAbilityMerged(state209, references)))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
					cAbility5 = cAbility3;
					references.Add(cAbility4, cAbility5);
				}
				ChooseAbilities.Add(cAbility5);
			}
			references.Add(state.ChooseAbilities, ChooseAbilities);
		}
		RecoverCardsWithAbilityOfTypeFilter = references.Get(state.RecoverCardsWithAbilityOfTypeFilter);
		if (RecoverCardsWithAbilityOfTypeFilter == null && state.RecoverCardsWithAbilityOfTypeFilter != null)
		{
			RecoverCardsWithAbilityOfTypeFilter = new List<CAbility.EAbilityType>();
			for (int num4 = 0; num4 < state.RecoverCardsWithAbilityOfTypeFilter.Count; num4++)
			{
				CAbility.EAbilityType item5 = state.RecoverCardsWithAbilityOfTypeFilter[num4];
				RecoverCardsWithAbilityOfTypeFilter.Add(item5);
			}
			references.Add(state.RecoverCardsWithAbilityOfTypeFilter, RecoverCardsWithAbilityOfTypeFilter);
		}
		ExtraTurnType = state.ExtraTurnType;
		SwapFirstTargetAbilityFilter = references.Get(state.SwapFirstTargetAbilityFilter);
		if (SwapFirstTargetAbilityFilter == null && state.SwapFirstTargetAbilityFilter != null)
		{
			SwapFirstTargetAbilityFilter = new CAbilityFilterContainer(state.SwapFirstTargetAbilityFilter, references);
			references.Add(state.SwapFirstTargetAbilityFilter, SwapFirstTargetAbilityFilter);
		}
		SwapSecondTargetAbilityFilter = references.Get(state.SwapSecondTargetAbilityFilter);
		if (SwapSecondTargetAbilityFilter == null && state.SwapSecondTargetAbilityFilter != null)
		{
			SwapSecondTargetAbilityFilter = new CAbilityFilterContainer(state.SwapSecondTargetAbilityFilter, references);
			references.Add(state.SwapSecondTargetAbilityFilter, SwapSecondTargetAbilityFilter);
		}
		ImmunityToAbilityTypes = references.Get(state.ImmunityToAbilityTypes);
		if (ImmunityToAbilityTypes == null && state.ImmunityToAbilityTypes != null)
		{
			ImmunityToAbilityTypes = new List<CAbility.EAbilityType>();
			for (int num5 = 0; num5 < state.ImmunityToAbilityTypes.Count; num5++)
			{
				CAbility.EAbilityType item6 = state.ImmunityToAbilityTypes[num5];
				ImmunityToAbilityTypes.Add(item6);
			}
			references.Add(state.ImmunityToAbilityTypes, ImmunityToAbilityTypes);
		}
		ModifierCardNamesToAdd = references.Get(state.ModifierCardNamesToAdd);
		if (ModifierCardNamesToAdd == null && state.ModifierCardNamesToAdd != null)
		{
			ModifierCardNamesToAdd = new List<string>();
			for (int num6 = 0; num6 < state.ModifierCardNamesToAdd.Count; num6++)
			{
				string item7 = state.ModifierCardNamesToAdd[num6];
				ModifierCardNamesToAdd.Add(item7);
			}
			references.Add(state.ModifierCardNamesToAdd, ModifierCardNamesToAdd);
		}
		ResourcesToAddOnAbilityEnd = references.Get(state.ResourcesToAddOnAbilityEnd);
		if (ResourcesToAddOnAbilityEnd == null && state.ResourcesToAddOnAbilityEnd != null)
		{
			ResourcesToAddOnAbilityEnd = new Dictionary<string, int>(state.ResourcesToAddOnAbilityEnd.Comparer);
			foreach (KeyValuePair<string, int> item8 in state.ResourcesToAddOnAbilityEnd)
			{
				string key = item8.Key;
				int value = item8.Value;
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
		HelpBoxLocalizationKey = state.HelpBoxLocalizationKey;
	}
}
