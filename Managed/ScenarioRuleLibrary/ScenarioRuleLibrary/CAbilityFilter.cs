using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CAbilityFilter : ISerializable
{
	[Serializable]
	[Flags]
	public enum EFilterTargetType
	{
		None = 0,
		Self = 1,
		Enemy = 2,
		Ally = 4,
		Companion = 8
	}

	[Serializable]
	[Flags]
	public enum EFilterEnemy
	{
		None = 0,
		Normal = 1,
		Elite = 2,
		Boss = 4,
		Object = 8
	}

	[Serializable]
	[Flags]
	public enum EFilterActorType
	{
		None = 0,
		Player = 1,
		Enemy = 2,
		Summon = 4,
		Ally = 8,
		Enemy2 = 0x10,
		Neutral = 0x20
	}

	[Serializable]
	[Flags]
	public enum EFilterRevealedType
	{
		None = 0,
		IsRevealed = 1,
		NotRevealed = 2
	}

	[Serializable]
	[Flags]
	public enum EFilterTile
	{
		None = 0,
		EmptyHex = 1,
		Trap = 2,
		Obstacle = 4,
		Loot = 8,
		SingleHexObstacle = 0x10,
		ActiveSpawner = 0x20,
		DeactiveSpawner = 0x40,
		AdjacentToWall = 0x80,
		Actor = 0x100,
		ObjectActor = 0x200,
		Wall = 0x400
	}

	[Serializable]
	[Flags]
	public enum ELootType
	{
		None = 0,
		Gold = 1,
		Chest = 2,
		GoalChest = 4,
		QuestItem = 8,
		Resource = 0x10
	}

	[Serializable]
	[Flags]
	public enum EFilterFlags
	{
		None = 0,
		IsDoomed = 1,
		CarryingQuestItem = 2,
		LootedGoalChest = 4,
		TargetAttackedByCasterThisRound = 8,
		TargetTargetedByCasterPreviousAbility = 0x10,
		TargetTargetedByAllPreviousAbilitiesInAction = 0x20,
		CasterPreviousMovementEachHexCloserToTarget = 0x40
	}

	public static EFilterTargetType[] FilterTargetTypes = (EFilterTargetType[])Enum.GetValues(typeof(EFilterTargetType));

	public static EFilterEnemy[] FilterEnemyTypes = (EFilterEnemy[])Enum.GetValues(typeof(EFilterEnemy));

	public static EFilterActorType[] FilterActorTypes = (EFilterActorType[])Enum.GetValues(typeof(EFilterActorType));

	public static EFilterRevealedType[] FilterRevealedTypes = (EFilterRevealedType[])Enum.GetValues(typeof(EFilterRevealedType));

	public static EFilterTile[] FilterTiles = (EFilterTile[])Enum.GetValues(typeof(EFilterTile));

	public static ELootType[] FilterLootTypes = (ELootType[])Enum.GetValues(typeof(ELootType));

	public static EFilterFlags[] FilterFlagTypes = (EFilterFlags[])Enum.GetValues(typeof(EFilterFlags));

	private List<CActor> m_LastCheckedTargetAdjacentActors = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentEnemies = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentAllies = new List<CActor>();

	private List<CActor> m_LastCheckedTargetAdjacentAlliesOfTarget = new List<CActor>();

	private List<CActor> m_LastCheckedCasterAdjacentEnemies = new List<CActor>();

	private List<CActor> m_LastCheckedCasterAdjacentAllies = new List<CActor>();

	public EFilterTargetType FilterTargetType { get; set; }

	public EFilterEnemy FilterEnemy { get; set; }

	public EFilterActorType FilterActorType { get; set; }

	public List<string> FilterPlayerClasses { get; set; }

	public List<string> FilterEnemyClasses { get; set; }

	public List<string> FilterHeroSummonClasses { get; set; }

	public List<string> FilterObjectClasses { get; set; }

	public List<string> FilterSummonerClasses { get; set; }

	public CEqualityFilter FilterHealth { get; set; }

	public CEqualityFilter FilterHealthSelf { get; set; }

	public CEqualityFilter FilterTargetAdjacentActors { get; set; }

	public CEqualityFilter FilterTargetAdjacentEnemies { get; set; }

	public CEqualityFilter FilterTargetAdjacentAllies { get; set; }

	public CEqualityFilter FilterTargetAdjacentAlliesOfTarget { get; set; }

	public CEqualityFilter FilterCasterAdjacentEnemies { get; set; }

	public CEqualityFilter FilterCasterAdjacentAllies { get; set; }

	public CEqualityFilter FilterTargetAdjacentToWalls { get; set; }

	public CEqualityFilter FilterCasterAdjacentToWalls { get; set; }

	public CEqualityFilter FilterTargetAdjacentValidTiles { get; set; }

	public List<EFilterTile> FilterTargetAdjacentValidTilesFilterList { get; set; }

	public CEqualityFilter FilterCasterAdjacentValidTiles { get; set; }

	public List<EFilterTile> FilterCasterAdjacentValidTilesFilterList { get; set; }

	public List<CCondition.ENegativeCondition> FilterTargetHasNegativeConditions { get; set; }

	public CEqualityFilter FilterTargetNegativeConditionCount { get; set; }

	public List<CCondition.EPositiveCondition> FilterTargetHasPositiveConditions { get; set; }

	public CEqualityFilter FilterTargetPositiveConditionCount { get; set; }

	public List<CCondition.ENegativeCondition> FilterCasterHasNegativeConditions { get; set; }

	public CEqualityFilter FilterCasterNegativeConditionCount { get; set; }

	public List<CCondition.EPositiveCondition> FilterCasterHasPositiveConditions { get; set; }

	public CEqualityFilter FilterCasterPositiveConditionCount { get; set; }

	public List<CAbility.EAbilityType> FilterTargetHasImmunities { get; set; }

	public CEqualityFilter FilterTargetImmunitiesCount { get; set; }

	public List<CAbility.EAbilityType> FilterCasterHasImmunities { get; set; }

	public CEqualityFilter FilterCasterImmunitiesCount { get; set; }

	public CEqualityFilter FilterCompareTargetHPToYourMissingHP { get; set; }

	public CEqualityFilter FilterTargetMissingHP { get; set; }

	public EFilterFlags FilterFlags { get; set; }

	public List<string> FilterTargetHasCharacterResource { get; set; }

	public bool Invert { get; set; }

	public int CheckAdjacentRange { get; set; }

	public bool UseTargetOriginalType { get; set; }

	public List<string> SpecificAbilityNames { get; set; }

	public List<CActor> LastCheckedTargetAdjacentActors => m_LastCheckedTargetAdjacentActors;

	public List<CActor> LastCheckedTargetAdjacentEnemies => m_LastCheckedTargetAdjacentEnemies;

	public List<CActor> LastCheckedTargetAdjacentAllies => m_LastCheckedTargetAdjacentAllies;

	public List<CActor> LastCheckedTargetAdjacentAlliesOfTarget => m_LastCheckedTargetAdjacentAlliesOfTarget;

	public List<CActor> LastCheckedCasterAdjacentEnemies => m_LastCheckedCasterAdjacentEnemies;

	public List<CActor> LastCheckedCasterAdjacentAllies => m_LastCheckedCasterAdjacentAllies;

	public int LastCheckedTargetAdjacentWalls { get; private set; }

	public int LastCheckedCasterAdjacentWalls { get; private set; }

	public int LastCheckedTargetAdjacentTiles { get; private set; }

	public int LastCheckedCasterAdjacentValidTiles { get; private set; }

	public static CAbilityFilter CreateDefaultFilter()
	{
		return new CAbilityFilter(EFilterTargetType.Self | EFilterTargetType.Enemy | EFilterTargetType.Ally | EFilterTargetType.Companion);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("FilterTargetType", FilterTargetType);
		info.AddValue("FilterEnemy", FilterEnemy);
		info.AddValue("FilterActor", FilterActorType);
		info.AddValue("FilterPlayerClasses", FilterPlayerClasses);
		info.AddValue("FilterEnemyClasses", FilterEnemyClasses);
		info.AddValue("FilterHeroSummonClasses", FilterHeroSummonClasses);
		info.AddValue("FilterObjectClasses", FilterObjectClasses);
		info.AddValue("FilterTargetAdjacentActors", FilterTargetAdjacentActors);
		info.AddValue("FilterSummonerClasses", FilterSummonerClasses);
		info.AddValue("FilterHealth", FilterHealth);
		info.AddValue("FilterHealthSelf", FilterHealthSelf);
		info.AddValue("FilterTargetAdjacentEnemies", FilterTargetAdjacentEnemies);
		info.AddValue("FilterTargetAdjacentAllies", FilterTargetAdjacentAllies);
		info.AddValue("FilterTargetAdjacentAlliesOfTarget", FilterTargetAdjacentAlliesOfTarget);
		info.AddValue("FilterCasterAdjacentEnemies", FilterCasterAdjacentEnemies);
		info.AddValue("FilterCasterAdjacentAllies", FilterCasterAdjacentAllies);
		info.AddValue("FilterTargetAdjacentToWalls", FilterTargetAdjacentToWalls);
		info.AddValue("FilterCasterAdjacentToWalls", FilterCasterAdjacentToWalls);
		info.AddValue("FilterTargetAdjacentValidTiles", FilterTargetAdjacentValidTiles);
		info.AddValue("FilterTargetAdjacentValidTilesFilterList", FilterTargetAdjacentValidTilesFilterList);
		info.AddValue("FilterCasterAdjacentValidTiles", FilterCasterAdjacentValidTiles);
		info.AddValue("FilterCasterAdjacentValidTilesFilterList", FilterCasterAdjacentValidTilesFilterList);
		info.AddValue("FilterTargetHasNegativeConditions", FilterTargetHasNegativeConditions);
		info.AddValue("FilterTargetNegativeConditionCount", FilterTargetNegativeConditionCount);
		info.AddValue("FilterTargetHasPositiveConditions", FilterTargetHasPositiveConditions);
		info.AddValue("FilterTargetPositiveConditionCount", FilterTargetPositiveConditionCount);
		info.AddValue("FilterCasterHasNegativeConditions", FilterCasterHasNegativeConditions);
		info.AddValue("FilterCasterNegativeConditionCount", FilterCasterNegativeConditionCount);
		info.AddValue("FilterCasterHasPositiveConditions", FilterCasterHasPositiveConditions);
		info.AddValue("FilterCasterPositiveConditionCount", FilterCasterPositiveConditionCount);
		info.AddValue("FilterTargetHasImmunities", FilterTargetHasImmunities);
		info.AddValue("FilterTargetImmunitiesCount", FilterTargetImmunitiesCount);
		info.AddValue("FilterCasterHasImmunities", FilterCasterHasImmunities);
		info.AddValue("FilterCasterImmunitiesCount", FilterCasterImmunitiesCount);
		info.AddValue("Invert", Invert);
		info.AddValue("CheckAdjacentRange", CheckAdjacentRange);
		info.AddValue("FilterCompareTargetHPToYourMissingHP", FilterCompareTargetHPToYourMissingHP);
		info.AddValue("FilterTargetMissingHP", FilterTargetMissingHP);
		info.AddValue("FilterFlags", FilterFlags);
		info.AddValue("FilterTargetHasCharacterResource", FilterTargetHasCharacterResource);
		info.AddValue("UseTargetOriginalType", UseTargetOriginalType);
		info.AddValue("SpecificAbilityNames", SpecificAbilityNames);
	}

	public CAbilityFilter(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "FilterTargetType":
					FilterTargetType = (EFilterTargetType)info.GetValue("FilterTargetType", typeof(EFilterTargetType));
					break;
				case "FilterEnemy":
					FilterEnemy = (EFilterEnemy)info.GetValue("FilterEnemy", typeof(EFilterEnemy));
					break;
				case "FilterActor":
					FilterActorType = (EFilterActorType)info.GetValue("FilterActor", typeof(EFilterActorType));
					break;
				case "FilterActorType":
				{
					int num = info.GetInt32("FilterActorType");
					if (num == 22)
					{
						num = 16;
					}
					FilterActorType = (EFilterActorType)num;
					break;
				}
				case "FilterPlayerClasses":
					FilterPlayerClasses = (List<string>)info.GetValue("FilterPlayerClasses", typeof(List<string>));
					break;
				case "FilterEnemyClasses":
					FilterEnemyClasses = (List<string>)info.GetValue("FilterEnemyClasses", typeof(List<string>));
					break;
				case "FilterHeroSummonClasses":
					FilterHeroSummonClasses = (List<string>)info.GetValue("FilterHeroSummonClasses", typeof(List<string>));
					break;
				case "FilterObjectClasses":
					FilterObjectClasses = (List<string>)info.GetValue("FilterObjectClasses", typeof(List<string>));
					break;
				case "FilterSummonerClasses":
					FilterSummonerClasses = (List<string>)info.GetValue("FilterSummonerClasses", typeof(List<string>));
					break;
				case "FilterHealth":
					FilterHealth = (CEqualityFilter)info.GetValue("FilterHealth", typeof(CEqualityFilter));
					break;
				case "FilterHealthSelf":
					FilterHealthSelf = (CEqualityFilter)info.GetValue("FilterHealthSelf", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentActors":
					FilterTargetAdjacentActors = (CEqualityFilter)info.GetValue("FilterTargetAdjacentActors", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentEnemies":
					FilterTargetAdjacentEnemies = (CEqualityFilter)info.GetValue("FilterTargetAdjacentEnemies", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentAllies":
					FilterTargetAdjacentAllies = (CEqualityFilter)info.GetValue("FilterTargetAdjacentAllies", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentAlliesOfTarget":
					FilterTargetAdjacentAlliesOfTarget = (CEqualityFilter)info.GetValue("FilterTargetAdjacentAlliesOfTarget", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentEnemies":
					FilterCasterAdjacentEnemies = (CEqualityFilter)info.GetValue("FilterCasterAdjacentEnemies", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentAllies":
					FilterCasterAdjacentAllies = (CEqualityFilter)info.GetValue("FilterCasterAdjacentAllies", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentToWalls":
					FilterTargetAdjacentToWalls = (CEqualityFilter)info.GetValue("FilterTargetAdjacentToWalls", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentToWalls":
					FilterCasterAdjacentToWalls = (CEqualityFilter)info.GetValue("FilterCasterAdjacentToWalls", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentValidTiles":
					FilterTargetAdjacentValidTiles = (CEqualityFilter)info.GetValue("FilterTargetAdjacentValidTiles", typeof(CEqualityFilter));
					break;
				case "FilterTargetAdjacentValidTilesFilterList":
					FilterTargetAdjacentValidTilesFilterList = (List<EFilterTile>)info.GetValue("FilterTargetAdjacentValidTilesFilterList", typeof(List<EFilterTile>));
					break;
				case "FilterCasterAdjacentValidTiles":
					FilterCasterAdjacentValidTiles = (CEqualityFilter)info.GetValue("FilterCasterAdjacentValidTiles", typeof(CEqualityFilter));
					break;
				case "FilterCasterAdjacentValidTilesFilterList":
					FilterCasterAdjacentValidTilesFilterList = (List<EFilterTile>)info.GetValue("FilterCasterAdjacentValidTilesFilterList", typeof(List<EFilterTile>));
					break;
				case "FilterTargetHasNegativeConditions":
					FilterTargetHasNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("FilterTargetHasNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "FilterTargetNegativeConditionCount":
					FilterTargetNegativeConditionCount = (CEqualityFilter)info.GetValue("FilterTargetNegativeConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterTargetHasPositiveConditions":
					FilterTargetHasPositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("FilterTargetHasPositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "FilterTargetPositiveConditionCount":
					FilterTargetPositiveConditionCount = (CEqualityFilter)info.GetValue("FilterTargetPositiveConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterCasterHasNegativeConditions":
					FilterCasterHasNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("FilterCasterHasNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "FilterCasterNegativeConditionCount":
					FilterCasterNegativeConditionCount = (CEqualityFilter)info.GetValue("FilterCasterNegativeConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterCasterHasPositiveConditions":
					FilterCasterHasPositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("FilterCasterHasPositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "FilterCasterPositiveConditionCount":
					FilterCasterPositiveConditionCount = (CEqualityFilter)info.GetValue("FilterCasterPositiveConditionCount", typeof(CEqualityFilter));
					break;
				case "FilterTargetHasImmunities":
					FilterTargetHasImmunities = (List<CAbility.EAbilityType>)info.GetValue("FilterTargetHasImmunities", typeof(List<CAbility.EAbilityType>));
					break;
				case "FilterTargetImmunitiesCount":
					FilterTargetImmunitiesCount = (CEqualityFilter)info.GetValue("FilterTargetImmunitiesCount", typeof(CEqualityFilter));
					break;
				case "FilterCasterHasImmunities":
					FilterCasterHasImmunities = (List<CAbility.EAbilityType>)info.GetValue("FilterCasterHasImmunities", typeof(List<CAbility.EAbilityType>));
					break;
				case "FilterCasterImmunitiesCount":
					FilterCasterImmunitiesCount = (CEqualityFilter)info.GetValue("FilterCasterImmunitiesCount", typeof(CEqualityFilter));
					break;
				case "Invert":
					Invert = info.GetBoolean("Invert");
					break;
				case "CheckAdjacentRange":
					CheckAdjacentRange = info.GetInt32("CheckAdjacentRange");
					break;
				case "FilterCompareTargetHPToYourMissingHP":
					FilterCompareTargetHPToYourMissingHP = (CEqualityFilter)info.GetValue("FilterCompareTargetHPToYourMissingHP", typeof(CEqualityFilter));
					break;
				case "FilterTargetMissingHP":
					FilterTargetMissingHP = (CEqualityFilter)info.GetValue("FilterTargetMissingHP", typeof(CEqualityFilter));
					break;
				case "FilterFlags":
					FilterFlags = (EFilterFlags)info.GetValue("FilterFlags", typeof(EFilterFlags));
					break;
				case "FilterTargetHasCharacterResource":
					FilterTargetHasCharacterResource = (List<string>)info.GetValue("FilterTargetHasCharacterResource", typeof(List<string>));
					break;
				case "UseTargetOriginalType":
					UseTargetOriginalType = info.GetBoolean("UseTargetOriginalType");
					break;
				case "SpecificAbilityNames":
					SpecificAbilityNames = (List<string>)info.GetValue("SpecificAbilityNames", typeof(List<string>));
					break;
				case "FilterTargetAdjacentValidTilesFilter":
				{
					EFilterTile item2 = (EFilterTile)info.GetValue("FilterTargetAdjacentValidTilesFilter", typeof(EFilterTile));
					FilterTargetAdjacentValidTilesFilterList = new List<EFilterTile> { item2 };
					break;
				}
				case "FilterCasterAdjacentValidTilesFilter":
				{
					EFilterTile item = (EFilterTile)info.GetValue("FilterCasterAdjacentValidTilesFilter", typeof(EFilterTile));
					FilterCasterAdjacentValidTilesFilterList = new List<EFilterTile> { item };
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CAbilityFilter entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAbilityFilter(EFilterTargetType filterTargetType = EFilterTargetType.None, EFilterEnemy filterEnemy = EFilterEnemy.None, EFilterActorType filterActorType = EFilterActorType.None, List<string> filterPlayerClasses = null, List<string> filterEnemyClasses = null, List<string> filterHeroSummonClasses = null, List<string> filterObjectClasses = null, List<string> filterSummonerClasses = null, CEqualityFilter filterHealth = null, CEqualityFilter filterHealthSelf = null, CEqualityFilter filterTargetAdjacentActors = null, CEqualityFilter filterTargetAdjacentEnemies = null, CEqualityFilter filterTargetAdjacentAllies = null, CEqualityFilter filterTargetAdjacentAlliesOfTarget = null, CEqualityFilter filterCasterAdjacentEnemies = null, CEqualityFilter filterCasterAdjacentAllies = null, CEqualityFilter filterTargetAdjacentToWalls = null, CEqualityFilter filterCasterAdjacentToWalls = null, CEqualityFilter filterTargetAdjacentTiles = null, List<EFilterTile> filterTargetAdjacentTilesFilter = null, CEqualityFilter filterCasterAdjacentTiles = null, List<EFilterTile> filterCasterAdjacentTilesFilter = null, List<CCondition.ENegativeCondition> filterTargetHasNegativeConditions = null, CEqualityFilter filterTargetNegativeConditionCount = null, List<CCondition.EPositiveCondition> filterTargetHasPositiveConditions = null, CEqualityFilter filterTargetPositiveConditionCount = null, List<CCondition.ENegativeCondition> filterCasterHasNegativeConditions = null, CEqualityFilter filterCasterNegativeConditionCount = null, List<CCondition.EPositiveCondition> filterCasterHasPositiveConditions = null, CEqualityFilter filterCasterPositiveConditionCount = null, List<CAbility.EAbilityType> filterTargetHasImmunities = null, CEqualityFilter filterTargetImmunitiesCount = null, List<CAbility.EAbilityType> filterCasterHasImmunities = null, CEqualityFilter filterCasterImmunitiesCount = null, CEqualityFilter filterCompareTargetHPToYourMissingHP = null, CEqualityFilter filterTargetMissingHP = null, EFilterFlags filterFlags = EFilterFlags.None, List<string> filterTargetHasCharacterResource = null, bool invert = false, bool useTargetOriginalType = false, int checkAdjacentRange = 1, List<string> specificAbilityNames = null)
	{
		FilterTargetType = filterTargetType;
		FilterEnemy = filterEnemy;
		FilterActorType = filterActorType;
		FilterPlayerClasses = filterPlayerClasses;
		FilterEnemyClasses = filterEnemyClasses;
		FilterHeroSummonClasses = filterHeroSummonClasses;
		FilterObjectClasses = filterObjectClasses;
		FilterSummonerClasses = filterSummonerClasses;
		FilterHealth = filterHealth;
		FilterHealthSelf = filterHealthSelf;
		FilterTargetAdjacentActors = filterTargetAdjacentActors;
		FilterTargetAdjacentEnemies = filterTargetAdjacentEnemies;
		FilterTargetAdjacentAllies = filterTargetAdjacentAllies;
		FilterTargetAdjacentAlliesOfTarget = filterTargetAdjacentAlliesOfTarget;
		FilterCasterAdjacentEnemies = filterCasterAdjacentEnemies;
		FilterCasterAdjacentAllies = filterCasterAdjacentAllies;
		FilterTargetAdjacentToWalls = filterTargetAdjacentToWalls;
		FilterCasterAdjacentToWalls = filterCasterAdjacentToWalls;
		FilterTargetAdjacentValidTiles = filterTargetAdjacentTiles;
		FilterTargetAdjacentValidTilesFilterList = filterTargetAdjacentTilesFilter;
		FilterCasterAdjacentValidTiles = filterCasterAdjacentTiles;
		FilterCasterAdjacentValidTilesFilterList = filterCasterAdjacentTilesFilter;
		FilterTargetHasNegativeConditions = filterTargetHasNegativeConditions;
		FilterTargetNegativeConditionCount = filterTargetNegativeConditionCount;
		FilterTargetHasPositiveConditions = filterTargetHasPositiveConditions;
		FilterTargetPositiveConditionCount = filterTargetPositiveConditionCount;
		FilterCasterHasNegativeConditions = filterCasterHasNegativeConditions;
		FilterCasterNegativeConditionCount = filterCasterNegativeConditionCount;
		FilterCasterHasPositiveConditions = filterCasterHasPositiveConditions;
		FilterCasterPositiveConditionCount = filterCasterPositiveConditionCount;
		FilterTargetHasImmunities = filterTargetHasImmunities;
		FilterTargetImmunitiesCount = filterTargetImmunitiesCount;
		FilterCasterHasImmunities = filterCasterHasImmunities;
		FilterCasterImmunitiesCount = filterCasterImmunitiesCount;
		FilterCompareTargetHPToYourMissingHP = filterCompareTargetHPToYourMissingHP;
		FilterTargetMissingHP = filterTargetMissingHP;
		FilterFlags = filterFlags;
		FilterTargetHasCharacterResource = filterTargetHasCharacterResource;
		Invert = invert;
		UseTargetOriginalType = useTargetOriginalType;
		CheckAdjacentRange = checkAdjacentRange;
		SpecificAbilityNames = specificAbilityNames;
	}

	public CAbilityFilter Copy()
	{
		EFilterTargetType filterTargetType = FilterTargetType;
		EFilterEnemy filterEnemy = FilterEnemy;
		EFilterActorType filterActorType = FilterActorType;
		List<string> filterPlayerClasses = FilterPlayerClasses?.ToList();
		List<string> filterEnemyClasses = FilterEnemyClasses?.ToList();
		List<string> filterHeroSummonClasses = FilterHeroSummonClasses?.ToList();
		List<string> filterObjectClasses = FilterObjectClasses?.ToList();
		List<string> filterSummonerClasses = FilterSummonerClasses?.ToList();
		CEqualityFilter filterHealth = FilterHealth?.Copy();
		CEqualityFilter filterHealthSelf = FilterHealthSelf?.Copy();
		CEqualityFilter filterTargetAdjacentActors = FilterTargetAdjacentActors?.Copy();
		CEqualityFilter filterTargetAdjacentEnemies = FilterTargetAdjacentEnemies?.Copy();
		CEqualityFilter filterTargetAdjacentAllies = FilterTargetAdjacentAllies?.Copy();
		CEqualityFilter filterTargetAdjacentAlliesOfTarget = FilterTargetAdjacentAlliesOfTarget?.Copy();
		CEqualityFilter filterCasterAdjacentEnemies = FilterCasterAdjacentEnemies?.Copy();
		CEqualityFilter filterCasterAdjacentAllies = FilterCasterAdjacentAllies?.Copy();
		CEqualityFilter filterTargetAdjacentToWalls = FilterTargetAdjacentToWalls?.Copy();
		CEqualityFilter filterCasterAdjacentToWalls = FilterCasterAdjacentToWalls?.Copy();
		CEqualityFilter filterTargetAdjacentTiles = FilterTargetAdjacentValidTiles?.Copy();
		List<EFilterTile> filterTargetAdjacentValidTilesFilterList = FilterTargetAdjacentValidTilesFilterList;
		CEqualityFilter filterCasterAdjacentTiles = FilterCasterAdjacentValidTiles?.Copy();
		List<EFilterTile> filterCasterAdjacentValidTilesFilterList = FilterCasterAdjacentValidTilesFilterList;
		List<CCondition.ENegativeCondition> filterTargetHasNegativeConditions = FilterTargetHasNegativeConditions?.ToList();
		CEqualityFilter filterTargetNegativeConditionCount = FilterTargetNegativeConditionCount?.Copy();
		List<CCondition.EPositiveCondition> filterTargetHasPositiveConditions = FilterTargetHasPositiveConditions?.ToList();
		CEqualityFilter filterTargetPositiveConditionCount = FilterTargetPositiveConditionCount?.Copy();
		List<CCondition.ENegativeCondition> filterCasterHasNegativeConditions = FilterCasterHasNegativeConditions?.ToList();
		CEqualityFilter filterCasterNegativeConditionCount = FilterCasterNegativeConditionCount?.Copy();
		List<CCondition.EPositiveCondition> filterCasterHasPositiveConditions = FilterCasterHasPositiveConditions?.ToList();
		CEqualityFilter filterCasterPositiveConditionCount = FilterCasterPositiveConditionCount?.Copy();
		List<CAbility.EAbilityType> filterTargetHasImmunities = FilterTargetHasImmunities?.ToList();
		CEqualityFilter filterTargetImmunitiesCount = FilterTargetImmunitiesCount?.Copy();
		List<CAbility.EAbilityType> filterCasterHasImmunities = FilterCasterHasImmunities?.ToList();
		CEqualityFilter filterCasterImmunitiesCount = FilterCasterImmunitiesCount?.Copy();
		CEqualityFilter filterCompareTargetHPToYourMissingHP = FilterCompareTargetHPToYourMissingHP?.Copy();
		CEqualityFilter filterTargetMissingHP = FilterTargetMissingHP?.Copy();
		EFilterFlags filterFlags = FilterFlags;
		List<string> filterTargetHasCharacterResource = (FilterTargetHasCharacterResource = FilterTargetHasCharacterResource?.ToList());
		return new CAbilityFilter(filterTargetType, filterEnemy, filterActorType, filterPlayerClasses, filterEnemyClasses, filterHeroSummonClasses, filterObjectClasses, filterSummonerClasses, filterHealth, filterHealthSelf, filterTargetAdjacentActors, filterTargetAdjacentEnemies, filterTargetAdjacentAllies, filterTargetAdjacentAlliesOfTarget, filterCasterAdjacentEnemies, filterCasterAdjacentAllies, filterTargetAdjacentToWalls, filterCasterAdjacentToWalls, filterTargetAdjacentTiles, filterTargetAdjacentValidTilesFilterList, filterCasterAdjacentTiles, filterCasterAdjacentValidTilesFilterList, filterTargetHasNegativeConditions, filterTargetNegativeConditionCount, filterTargetHasPositiveConditions, filterTargetPositiveConditionCount, filterCasterHasNegativeConditions, filterCasterNegativeConditionCount, filterCasterHasPositiveConditions, filterCasterPositiveConditionCount, filterTargetHasImmunities, filterTargetImmunitiesCount, filterCasterHasImmunities, filterCasterImmunitiesCount, filterCompareTargetHPToYourMissingHP, filterTargetMissingHP, filterFlags, filterTargetHasCharacterResource, Invert, UseTargetOriginalType, CheckAdjacentRange, SpecificAbilityNames);
	}

	public bool Equals(CAbilityFilter compare)
	{
		if (compare == null)
		{
			return false;
		}
		bool num = FilterTargetType == compare.FilterTargetType;
		bool flag = FilterEnemy == compare.FilterEnemy;
		bool flag2 = FilterActorType == compare.FilterActorType;
		bool flag3 = ((FilterPlayerClasses != null && compare.FilterPlayerClasses != null) ? FilterPlayerClasses.SequenceEqual(compare.FilterPlayerClasses) : StateShared.CheckNullsMatchBoolean(FilterPlayerClasses, compare.FilterPlayerClasses));
		bool flag4 = ((FilterEnemyClasses != null && compare.FilterEnemyClasses != null) ? FilterEnemyClasses.SequenceEqual(compare.FilterEnemyClasses) : StateShared.CheckNullsMatchBoolean(FilterEnemyClasses, compare.FilterEnemyClasses));
		bool flag5 = ((FilterHeroSummonClasses != null && compare.FilterHeroSummonClasses != null) ? FilterHeroSummonClasses.SequenceEqual(compare.FilterHeroSummonClasses) : StateShared.CheckNullsMatchBoolean(FilterHeroSummonClasses, compare.FilterHeroSummonClasses));
		bool flag6 = ((FilterObjectClasses != null && compare.FilterObjectClasses != null) ? FilterObjectClasses.SequenceEqual(compare.FilterObjectClasses) : StateShared.CheckNullsMatchBoolean(FilterObjectClasses, compare.FilterObjectClasses));
		bool flag7 = ((FilterSummonerClasses != null && compare.FilterSummonerClasses != null) ? FilterSummonerClasses.SequenceEqual(compare.FilterSummonerClasses) : StateShared.CheckNullsMatchBoolean(FilterSummonerClasses, compare.FilterSummonerClasses));
		bool flag8 = ((FilterHealth != null) ? FilterHealth.Equals(compare.FilterHealth) : (compare.FilterHealth == null));
		bool flag9 = ((FilterHealthSelf != null) ? FilterHealthSelf.Equals(compare.FilterHealthSelf) : (compare.FilterHealthSelf == null));
		bool flag10 = ((FilterTargetAdjacentActors != null) ? FilterTargetAdjacentActors.Equals(compare.FilterTargetAdjacentActors) : (compare.FilterTargetAdjacentActors == null));
		bool flag11 = ((FilterTargetAdjacentEnemies != null) ? FilterTargetAdjacentEnemies.Equals(compare.FilterTargetAdjacentEnemies) : (compare.FilterTargetAdjacentEnemies == null));
		bool flag12 = ((FilterTargetAdjacentAllies != null) ? FilterTargetAdjacentAllies.Equals(compare.FilterTargetAdjacentAllies) : (compare.FilterTargetAdjacentAllies == null));
		bool flag13 = ((FilterTargetAdjacentAlliesOfTarget != null) ? FilterTargetAdjacentAlliesOfTarget.Equals(compare.FilterTargetAdjacentAlliesOfTarget) : (compare.FilterTargetAdjacentAlliesOfTarget == null));
		bool flag14 = ((FilterCasterAdjacentEnemies != null) ? FilterCasterAdjacentEnemies.Equals(compare.FilterCasterAdjacentEnemies) : (compare.FilterCasterAdjacentEnemies == null));
		bool flag15 = ((FilterCasterAdjacentAllies != null) ? FilterCasterAdjacentAllies.Equals(compare.FilterCasterAdjacentAllies) : (compare.FilterCasterAdjacentAllies == null));
		bool flag16 = ((FilterTargetAdjacentToWalls != null) ? FilterTargetAdjacentToWalls.Equals(compare.FilterTargetAdjacentToWalls) : (compare.FilterTargetAdjacentToWalls == null));
		bool flag17 = ((FilterCasterAdjacentToWalls != null) ? FilterCasterAdjacentToWalls.Equals(compare.FilterCasterAdjacentToWalls) : (compare.FilterCasterAdjacentToWalls == null));
		bool flag18 = ((FilterTargetAdjacentValidTiles != null) ? FilterTargetAdjacentValidTiles.Equals(compare.FilterTargetAdjacentValidTiles) : (compare.FilterTargetAdjacentValidTiles == null));
		bool flag19 = FilterTargetAdjacentValidTilesFilterList == compare.FilterTargetAdjacentValidTilesFilterList;
		bool flag20 = ((FilterCasterAdjacentValidTiles != null) ? FilterCasterAdjacentValidTiles.Equals(compare.FilterCasterAdjacentValidTiles) : (compare.FilterCasterAdjacentValidTiles == null));
		bool flag21 = FilterCasterAdjacentValidTilesFilterList == compare.FilterCasterAdjacentValidTilesFilterList;
		bool flag22 = ((FilterTargetHasNegativeConditions != null && compare.FilterTargetHasNegativeConditions != null) ? FilterTargetHasNegativeConditions.SequenceEqual(compare.FilterTargetHasNegativeConditions) : StateShared.CheckNullsMatchBoolean(FilterTargetHasNegativeConditions, compare.FilterTargetHasNegativeConditions));
		bool flag23 = ((FilterTargetNegativeConditionCount != null) ? FilterTargetNegativeConditionCount.Equals(compare.FilterTargetNegativeConditionCount) : (compare.FilterTargetNegativeConditionCount == null));
		bool flag24 = ((FilterTargetHasPositiveConditions != null && compare.FilterTargetHasPositiveConditions != null) ? FilterTargetHasPositiveConditions.SequenceEqual(compare.FilterTargetHasPositiveConditions) : StateShared.CheckNullsMatchBoolean(FilterTargetHasPositiveConditions, compare.FilterTargetHasPositiveConditions));
		bool flag25 = ((FilterTargetPositiveConditionCount != null) ? FilterTargetPositiveConditionCount.Equals(compare.FilterTargetPositiveConditionCount) : (compare.FilterTargetPositiveConditionCount == null));
		bool flag26 = ((FilterCasterHasNegativeConditions != null && compare.FilterCasterHasNegativeConditions != null) ? FilterCasterHasNegativeConditions.SequenceEqual(compare.FilterCasterHasNegativeConditions) : StateShared.CheckNullsMatchBoolean(FilterCasterHasNegativeConditions, compare.FilterCasterHasNegativeConditions));
		bool flag27 = ((FilterCasterNegativeConditionCount != null) ? FilterCasterNegativeConditionCount.Equals(compare.FilterCasterNegativeConditionCount) : (compare.FilterCasterNegativeConditionCount == null));
		bool flag28 = ((FilterCasterHasPositiveConditions != null && compare.FilterCasterHasPositiveConditions != null) ? FilterCasterHasPositiveConditions.SequenceEqual(compare.FilterCasterHasPositiveConditions) : StateShared.CheckNullsMatchBoolean(FilterCasterHasPositiveConditions, compare.FilterCasterHasPositiveConditions));
		bool flag29 = ((FilterCasterPositiveConditionCount != null) ? FilterCasterPositiveConditionCount.Equals(compare.FilterCasterPositiveConditionCount) : (compare.FilterCasterPositiveConditionCount == null));
		bool flag30 = ((FilterTargetHasImmunities != null && compare.FilterTargetHasImmunities != null) ? FilterTargetHasImmunities.SequenceEqual(compare.FilterTargetHasImmunities) : StateShared.CheckNullsMatchBoolean(FilterTargetHasImmunities, compare.FilterTargetHasImmunities));
		bool flag31 = ((FilterTargetImmunitiesCount != null) ? FilterTargetImmunitiesCount.Equals(compare.FilterTargetImmunitiesCount) : (compare.FilterTargetImmunitiesCount == null));
		bool flag32 = ((FilterCasterHasImmunities != null && compare.FilterCasterHasImmunities != null) ? FilterCasterHasImmunities.SequenceEqual(compare.FilterCasterHasImmunities) : StateShared.CheckNullsMatchBoolean(FilterCasterHasImmunities, compare.FilterCasterHasImmunities));
		bool flag33 = ((FilterCasterImmunitiesCount != null) ? FilterCasterImmunitiesCount.Equals(compare.FilterCasterImmunitiesCount) : (compare.FilterCasterImmunitiesCount == null));
		bool flag34 = ((FilterCompareTargetHPToYourMissingHP != null) ? FilterCompareTargetHPToYourMissingHP.Equals(compare.FilterCompareTargetHPToYourMissingHP) : (compare.FilterCompareTargetHPToYourMissingHP == null));
		bool flag35 = ((FilterTargetMissingHP != null) ? FilterTargetMissingHP.Equals(compare.FilterTargetMissingHP) : (compare.FilterTargetMissingHP == null));
		bool flag36 = FilterFlags == compare.FilterFlags;
		bool flag37 = ((FilterTargetHasCharacterResource != null && compare.FilterTargetHasCharacterResource != null) ? FilterTargetHasCharacterResource.SequenceEqual(compare.FilterTargetHasCharacterResource) : StateShared.CheckNullsMatchBoolean(FilterTargetHasCharacterResource, compare.FilterTargetHasCharacterResource));
		bool flag38 = Invert == compare.Invert;
		bool flag39 = UseTargetOriginalType == compare.UseTargetOriginalType;
		bool flag40 = CheckAdjacentRange == compare.CheckAdjacentRange;
		bool flag41 = ((SpecificAbilityNames != null && compare.SpecificAbilityNames != null) ? SpecificAbilityNames.SequenceEqual(compare.SpecificAbilityNames) : StateShared.CheckNullsMatchBoolean(SpecificAbilityNames, compare.SpecificAbilityNames));
		return num && flag && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8 && flag9 && flag10 && flag11 && flag12 && flag13 && flag14 && flag15 && flag16 && flag17 && flag18 && flag19 && flag20 && flag21 && flag22 && flag23 && flag24 && flag25 && flag26 && flag27 && flag28 && flag29 && flag30 && flag31 && flag32 && flag33 && flag34 && flag35 && flag36 && flag37 && flag38 && flag39 && flag40 && flag41;
	}

	public bool IsValidTarget(CActor target, CActor self, bool isTargetedAbility, bool useTargetOriginalType = false, bool? canTargetInvisible = false, bool skipUntargetableCheck = false)
	{
		bool? flag = GameState.CanTargetInvisible(self, canTargetInvisible);
		if (target == null)
		{
			return false;
		}
		if (self == null)
		{
			DLLDebug.LogError(" Unable to check valid target as self is null!");
			return false;
		}
		if (target.Deactivated || target.PhasedOut)
		{
			return false;
		}
		if (target.Untargetable && target != self && !skipUntargetableCheck)
		{
			return false;
		}
		bool num = CheckTargetType(target, self, useTargetOriginalType);
		bool flag2 = CheckEnemy(target);
		bool flag3 = CheckActorType(target, useTargetOriginalType);
		bool flag4 = CheckHealthFilter(target);
		bool flag5 = CheckHealthSelfFilter(self);
		bool flag6 = CheckAdjacencyFilter(target, self, flag);
		bool flag7 = CheckWallAdjacencyFilter(target, self, flag);
		bool flag8 = CheckTileAdjacencyFilter(target, self, false);
		bool flag9 = CheckConditionsFilters(target, self);
		bool flag10 = CheckConditionImmunitiesFilters(target, self);
		bool flag11 = CheckTargetHPtoMissingHPFilters(target, self);
		bool flag12 = CheckTargetMissingHPFilters(target);
		bool flag13 = CheckClass(target);
		bool flag14 = CheckSummoner(target);
		bool flag15 = CheckFlags(target, self);
		bool flag16 = CheckCharacterResources(target);
		if (num && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8 && flag9 && flag10 && flag11 && flag12 && flag13 && flag14 && flag15 && flag16)
		{
			if (!CActor.AreActorsAllied(target.Type, self.Type) && isTargetedAbility && target.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) && flag != true)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsValidTarget_ActorState(ActorState targetState, ActorState selfState, bool isTargetedAbility, bool useTargetOriginalType = false, bool? canTargetInvisible = false)
	{
		CActor cActor = ScenarioManager.FindActor(targetState.ActorGuid);
		CActor cActor2 = ScenarioManager.FindActor(selfState.ActorGuid);
		if (cActor != null && cActor2 != null)
		{
			return IsValidTarget(cActor, cActor2, isTargetedAbility, useTargetOriginalType, canTargetInvisible);
		}
		bool? flag = ((cActor2 != null) ? GameState.CanTargetInvisible(cActor2, canTargetInvisible) : new bool?(false));
		if (targetState == null)
		{
			return false;
		}
		if (selfState == null)
		{
			DLLDebug.LogError(" Unable to check valid target state as self is null!");
			return false;
		}
		bool num = CheckTargetType_ActorState(targetState, selfState);
		bool flag2 = CheckEnemy_ActorState(targetState);
		bool flag3 = CheckActorType_ActorState(targetState);
		bool flag4 = CheckHealthFilter_ActorState(targetState);
		bool flag5 = CheckHealthSelfFilter_ActorState(selfState);
		bool flag6 = CheckAdjacencyFilter_ActorState(targetState, selfState, flag);
		bool flag7 = CheckConditionsFilters_ActorState(targetState, selfState);
		bool flag8 = CheckConditionsImmunitiesFilters_ActorState(targetState, selfState);
		bool flag9 = CheckTargetHPtoMissingHPFilters_ActorState(targetState, selfState);
		bool flag10 = CheckTargetMissingHPFilters_ActorState(targetState);
		bool flag11 = CheckClass_ActorState(targetState);
		bool flag12 = CheckSummoner_ActorState(targetState);
		bool flag13 = CheckFlags_ActorState(targetState);
		if (num && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8 && flag9 && flag10 && flag11 && flag12 && flag13)
		{
			CActor.EType actorType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
			CActor.EType actorType2 = ScenarioManager.CurrentScenarioState.GetActorType(selfState);
			if (!CActor.AreActorsAllied(actorType, actorType2) && isTargetedAbility && targetState.PositiveConditions.Any((PositiveConditionPair c) => c.PositiveCondition == CCondition.EPositiveCondition.Invisible) && flag != true)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CheckTargetType(CActor target, CActor self, bool useTargetOriginalType = false)
	{
		CActor.EType actor1Type = ((UseTargetOriginalType || useTargetOriginalType) ? target.OriginalType : target.Type);
		bool flag = true;
		if (FilterTargetType == EFilterTargetType.None)
		{
			return true;
		}
		if (target == self)
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Self);
		}
		else if ((((self.Type == CActor.EType.Player || self.Type == CActor.EType.HeroSummon || self.Type == CActor.EType.Ally) && target.Type == CActor.EType.Neutral) || ((target.Type == CActor.EType.Player || target.Type == CActor.EType.HeroSummon || target.Type == CActor.EType.Ally) && self.Type == CActor.EType.Neutral)) && !FilterActorType.HasFlag(EFilterActorType.Neutral))
		{
			flag = false;
		}
		else if (CActor.AreActorsAllied(actor1Type, self.Type))
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Ally);
			if (self is CPlayerActor { CompanionSummon: not null } cPlayerActor && cPlayerActor.CompanionSummon == target)
			{
				flag |= FilterTargetType.HasFlag(EFilterTargetType.Companion);
			}
		}
		else
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Enemy);
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetType_ActorState(ActorState targetState, ActorState selfState)
	{
		CActor.EType actorType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
		CActor.EType actorType2 = ScenarioManager.CurrentScenarioState.GetActorType(selfState);
		bool flag = true;
		if (FilterTargetType == EFilterTargetType.None)
		{
			return true;
		}
		if (targetState.ActorGuid == selfState.ActorGuid)
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Self);
		}
		else if (CActor.AreActorsAllied(actorType, actorType2))
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Ally);
			if (selfState is PlayerState playerState && !string.IsNullOrEmpty(playerState.CompanionSummonGuid) && playerState.CompanionSummonGuid == targetState.ActorGuid)
			{
				flag |= FilterTargetType.HasFlag(EFilterTargetType.Companion);
			}
		}
		else
		{
			flag = FilterTargetType.HasFlag(EFilterTargetType.Enemy);
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckEnemy(CActor target)
	{
		if (FilterEnemy == EFilterEnemy.None)
		{
			return true;
		}
		if (target.IsMonsterType)
		{
			CEnemyActor cEnemyActor = target as CEnemyActor;
			EFilterEnemy eFilterEnemy = EFilterEnemy.None;
			eFilterEnemy = ((target is CObjectActor) ? EFilterEnemy.Object : (cEnemyActor.MonsterClass.Boss ? EFilterEnemy.Boss : ((cEnemyActor.MonsterClass.NonEliteVariant == null) ? EFilterEnemy.Normal : EFilterEnemy.Elite)));
			bool flag = FilterEnemy.HasFlag(eFilterEnemy);
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	private bool CheckEnemy_ActorState(ActorState targetState)
	{
		if (FilterEnemy == EFilterEnemy.None)
		{
			return true;
		}
		if (targetState is EnemyState enemyState)
		{
			EFilterEnemy eFilterEnemy = EFilterEnemy.None;
			eFilterEnemy = ((enemyState is ObjectState) ? EFilterEnemy.Object : (enemyState.IsBoss ? EFilterEnemy.Boss : ((!enemyState.IsElite) ? EFilterEnemy.Normal : EFilterEnemy.Elite)));
			bool flag = FilterEnemy.HasFlag(eFilterEnemy);
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	private bool CheckActorType(CActor target, bool useTargetOriginalType = false)
	{
		if (FilterActorType == EFilterActorType.None)
		{
			return true;
		}
		CActor.EType eType = (useTargetOriginalType ? target.OriginalType : target.Type);
		EFilterActorType eFilterActorType = EFilterActorType.None;
		switch (eType)
		{
		case CActor.EType.Player:
			eFilterActorType = EFilterActorType.Player;
			break;
		case CActor.EType.Enemy:
			eFilterActorType = EFilterActorType.Enemy;
			break;
		case CActor.EType.HeroSummon:
			eFilterActorType = EFilterActorType.Summon;
			break;
		case CActor.EType.Ally:
			eFilterActorType = EFilterActorType.Ally;
			break;
		case CActor.EType.Enemy2:
			eFilterActorType = EFilterActorType.Enemy2;
			break;
		case CActor.EType.Neutral:
			eFilterActorType = EFilterActorType.Neutral;
			break;
		}
		bool flag = FilterActorType.HasFlag(eFilterActorType);
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckActorType_ActorState(ActorState targetState)
	{
		if (FilterActorType == EFilterActorType.None)
		{
			return true;
		}
		CActor.EType actorType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
		EFilterActorType eFilterActorType = EFilterActorType.None;
		switch (actorType)
		{
		case CActor.EType.Player:
			eFilterActorType = EFilterActorType.Player;
			break;
		case CActor.EType.Enemy:
			eFilterActorType = EFilterActorType.Enemy;
			break;
		case CActor.EType.HeroSummon:
			eFilterActorType = EFilterActorType.Summon;
			break;
		case CActor.EType.Ally:
			eFilterActorType = EFilterActorType.Ally;
			break;
		case CActor.EType.Enemy2:
			eFilterActorType = EFilterActorType.Enemy2;
			break;
		case CActor.EType.Neutral:
			eFilterActorType = EFilterActorType.Neutral;
			break;
		}
		bool flag = FilterActorType.HasFlag(eFilterActorType);
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckClass(CActor target)
	{
		if (FilterPlayerClasses == null && FilterObjectClasses == null && FilterEnemyClasses == null && FilterHeroSummonClasses == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((target is CPlayerActor && FilterPlayerClasses != null && FilterPlayerClasses.Count > 0) ? FilterPlayerClasses.Contains(target.Class.ID) : ((target is CObjectActor && FilterObjectClasses != null && FilterObjectClasses.Count > 0) ? FilterObjectClasses.Contains(target.Class.ID) : ((target is CEnemyActor && FilterEnemyClasses != null && FilterEnemyClasses.Count > 0) ? FilterEnemyClasses.Contains(target.Class.ID) : ((target is CHeroSummonActor && FilterHeroSummonClasses != null && FilterHeroSummonClasses.Count > 0) ? FilterHeroSummonClasses.Contains(target.Class.ID) : (((FilterPlayerClasses == null || FilterPlayerClasses.Count <= 0) && (FilterEnemyClasses == null || FilterEnemyClasses.Count <= 0) && (FilterHeroSummonClasses == null || FilterHeroSummonClasses.Count <= 0) && (FilterObjectClasses == null || FilterObjectClasses.Count <= 0)) ? true : false)))));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckClass_ActorState(ActorState targetState)
	{
		if (FilterPlayerClasses == null && FilterObjectClasses == null && FilterEnemyClasses == null && FilterHeroSummonClasses == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((targetState is PlayerState && FilterPlayerClasses != null && FilterPlayerClasses.Count > 0) ? FilterPlayerClasses.Contains(targetState.ClassID) : ((targetState is ObjectState && FilterObjectClasses != null && FilterObjectClasses.Count > 0) ? FilterObjectClasses.Contains(targetState.ClassID) : ((targetState is EnemyState && FilterEnemyClasses != null && FilterEnemyClasses.Count > 0) ? FilterEnemyClasses.Contains(targetState.ClassID) : ((targetState is HeroSummonState && FilterHeroSummonClasses != null && FilterHeroSummonClasses.Count > 0) ? FilterHeroSummonClasses.Contains(targetState.ClassID) : (((FilterPlayerClasses == null || FilterPlayerClasses.Count <= 0) && (FilterEnemyClasses == null || FilterEnemyClasses.Count <= 0) && (FilterHeroSummonClasses == null || FilterHeroSummonClasses.Count <= 0) && (FilterObjectClasses == null || FilterObjectClasses.Count <= 0)) ? true : false)))));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckSummoner(CActor target)
	{
		if (FilterSummonerClasses == null)
		{
			return true;
		}
		if (target is CHeroSummonActor cHeroSummonActor && FilterSummonerClasses.Contains(cHeroSummonActor.Summoner.Class.ID))
		{
			return true;
		}
		return false;
	}

	private bool CheckSummoner_ActorState(ActorState targetState)
	{
		if (FilterSummonerClasses == null)
		{
			return true;
		}
		HeroSummonState summonState = targetState as HeroSummonState;
		if (summonState != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == summonState.Summoner);
			if (actorState != null && FilterSummonerClasses.Contains(actorState.ClassID))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckCharacterResources(CActor target)
	{
		bool flag = true;
		if (FilterTargetHasCharacterResource != null && FilterTargetHasCharacterResource.Count > 0)
		{
			foreach (string item in FilterTargetHasCharacterResource)
			{
				if (!target.CharacterHasResource(item, 1))
				{
					flag = false;
					break;
				}
			}
			if (!Invert)
			{
				return flag;
			}
			return !flag;
		}
		return true;
	}

	private bool CheckFlags(CActor target, CActor self)
	{
		if (FilterFlags == EFilterFlags.None)
		{
			return true;
		}
		bool flag = true;
		if (FilterFlags.HasFlag(EFilterFlags.IsDoomed))
		{
			flag = target.IsDoomed;
		}
		bool flag2 = true;
		if (FilterFlags.HasFlag(EFilterFlags.CarryingQuestItem))
		{
			flag2 = target.CarriedQuestItems.Count > 0;
		}
		bool flag3 = true;
		if (FilterFlags.HasFlag(EFilterFlags.LootedGoalChest))
		{
			List<CObjectProp> list = ScenarioManager.CurrentScenarioState.ActivatedProps.Where((CObjectProp p) => p.IsLootable && p.ObjectType == ScenarioManager.ObjectImportType.GoalChest).ToList();
			if (list != null)
			{
				flag3 = list.Any((CObjectProp x) => x.ActorActivated == target.ActorGuid);
			}
		}
		bool flag4 = true;
		if (FilterFlags.HasFlag(EFilterFlags.TargetAttackedByCasterThisRound))
		{
			flag4 = self.ActorsAttackedThisRound.Contains(target);
		}
		bool flag5 = true;
		if (FilterFlags.HasFlag(EFilterFlags.TargetTargetedByCasterPreviousAbility))
		{
			flag5 = false;
			if (SpecificAbilityNames != null)
			{
				if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
				{
					foreach (CPhaseAction.CPhaseAbility previousPhaseAbility in ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities)
					{
						if (SpecificAbilityNames.Contains(previousPhaseAbility.m_Ability.Name))
						{
							flag5 = previousPhaseAbility.m_Ability.ActorsTargeted.Contains(target);
						}
					}
				}
			}
			else if (self.LastAbilityPerformed != null && self.LastAbilityPerformed.ActorsTargeted != null)
			{
				flag5 = self.LastAbilityPerformed.ActorsTargeted.Contains(target);
			}
		}
		bool flag6 = true;
		if (FilterFlags.HasFlag(EFilterFlags.TargetTargetedByAllPreviousAbilitiesInAction) && PhaseManager.PhaseType == CPhase.PhaseType.Action)
		{
			foreach (CPhaseAction.CPhaseAbility previousPhaseAbility2 in ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities)
			{
				if (!previousPhaseAbility2.m_Ability.IsModifierAbility && (previousPhaseAbility2.m_Ability.ActorsTargeted == null || !previousPhaseAbility2.m_Ability.ActorsTargeted.Contains(target)))
				{
					flag6 = false;
					break;
				}
			}
		}
		bool flag7 = true;
		if (FilterFlags.HasFlag(EFilterFlags.CasterPreviousMovementEachHexCloserToTarget))
		{
			int num = int.MaxValue;
			foreach (Point allArrayIndexOnPathIncludingRepeat in CAbilityMove.AllArrayIndexOnPathIncludingRepeats)
			{
				int tileDistance = ScenarioManager.GetTileDistance(allArrayIndexOnPathIncludingRepeat.X, allArrayIndexOnPathIncludingRepeat.Y, target.ArrayIndex.X, target.ArrayIndex.Y);
				if (tileDistance >= num)
				{
					flag7 = false;
					break;
				}
				num = tileDistance;
			}
		}
		bool flag8 = flag && flag2 && flag3 && flag4 && flag5 && flag6 && flag7;
		if (!Invert)
		{
			return flag8;
		}
		return !flag8;
	}

	private bool CheckFlags_ActorState(ActorState targetState)
	{
		if (FilterFlags == EFilterFlags.None)
		{
			return true;
		}
		bool flag = false;
		if (FilterFlags.HasFlag(EFilterFlags.IsDoomed))
		{
			flag = false;
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthFilter(CActor target)
	{
		if (FilterHealth == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealth.ValueIsPercentage) ? FilterHealth.Compare(target.Health) : FilterHealth.Compare(target.Health, target.OriginalMaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthFilter_ActorState(ActorState targetState)
	{
		if (FilterHealth == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealth.ValueIsPercentage) ? FilterHealth.Compare(targetState.Health) : FilterHealth.Compare(targetState.Health, targetState.MaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthSelfFilter(CActor self)
	{
		if (FilterHealthSelf == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealthSelf.ValueIsPercentage) ? FilterHealthSelf.Compare(self.Health) : FilterHealthSelf.Compare(self.Health, self.OriginalMaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckHealthSelfFilter_ActorState(ActorState selfState)
	{
		if (FilterHealthSelf == null)
		{
			return true;
		}
		bool flag = false;
		flag = ((!FilterHealthSelf.ValueIsPercentage) ? FilterHealthSelf.Compare(selfState.Health) : FilterHealthSelf.Compare(selfState.Health, selfState.MaxHealth));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckAdjacencyFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentActors == null && FilterTargetAdjacentAllies == null && FilterTargetAdjacentAlliesOfTarget == null && FilterTargetAdjacentEnemies == null && FilterCasterAdjacentAllies == null && FilterCasterAdjacentEnemies == null)
		{
			return true;
		}
		List<CActor> actorsInRange = GameState.GetActorsInRange(target, target, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible);
		List<CActor> actorsInRange2 = GameState.GetActorsInRange(self, self, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible);
		if (self is CEnemyActor)
		{
			actorsInRange.RemoveAll((CActor x) => x is CObjectActor);
			actorsInRange2.RemoveAll((CActor x) => x is CObjectActor);
		}
		List<CActor> list = actorsInRange.Where((CActor x) => !CActor.AreActorsAllied(x.Type, self.Type)).ToList();
		List<CActor> list2 = actorsInRange.Where((CActor x) => CActor.AreActorsAllied(x.Type, self.Type) && x.ActorGuid != self.ActorGuid).ToList();
		List<CActor> list3 = actorsInRange.Where((CActor x) => CActor.AreActorsAllied(x.Type, target.Type) && x.ActorGuid != self.ActorGuid).ToList();
		List<CActor> list4 = actorsInRange2.Where((CActor x) => !CActor.AreActorsAllied(x.Type, self.Type)).ToList();
		List<CActor> list5 = actorsInRange2.Where((CActor x) => CActor.AreActorsAllied(x.Type, self.Type) && x.ActorGuid != self.ActorGuid).ToList();
		bool flag = FilterTargetAdjacentActors == null;
		if (FilterTargetAdjacentActors != null)
		{
			flag = FilterTargetAdjacentActors.Compare(actorsInRange.Count);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterTargetAdjacentEnemies == null;
		if (FilterTargetAdjacentEnemies != null)
		{
			flag2 = FilterTargetAdjacentEnemies.Compare(list.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		bool flag3 = FilterTargetAdjacentAllies == null;
		if (FilterTargetAdjacentAllies != null)
		{
			flag3 = FilterTargetAdjacentAllies.Compare(list2.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		bool flag4 = FilterTargetAdjacentAlliesOfTarget == null;
		if (FilterTargetAdjacentAlliesOfTarget != null)
		{
			flag4 = FilterTargetAdjacentAlliesOfTarget.Compare(list3.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		bool flag5 = FilterCasterAdjacentEnemies == null;
		if (FilterCasterAdjacentEnemies != null)
		{
			flag5 = FilterCasterAdjacentEnemies.Compare(list4.Count);
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		bool flag6 = FilterCasterAdjacentAllies == null;
		if (FilterCasterAdjacentAllies != null)
		{
			flag6 = FilterCasterAdjacentAllies.Compare(list5.Count);
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		m_LastCheckedTargetAdjacentActors = actorsInRange;
		m_LastCheckedTargetAdjacentEnemies = list;
		m_LastCheckedTargetAdjacentAllies = list2;
		m_LastCheckedTargetAdjacentAlliesOfTarget = list3;
		m_LastCheckedCasterAdjacentEnemies = list4;
		m_LastCheckedCasterAdjacentAllies = list5;
		return flag && flag2 && flag3 && flag4 && flag5 && flag6;
	}

	private bool CheckAdjacencyFilter_ActorState(ActorState targetState, ActorState selfState, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentActors == null && FilterTargetAdjacentAllies == null && FilterTargetAdjacentAlliesOfTarget == null && FilterTargetAdjacentEnemies == null && FilterCasterAdjacentAllies == null && FilterCasterAdjacentEnemies == null)
		{
			return true;
		}
		CActor cActor = ScenarioManager.FindActor(targetState.ActorGuid);
		CActor cActor2 = ScenarioManager.FindActor(selfState.ActorGuid);
		List<CActor> list = ((cActor == null) ? new List<CActor>() : GameState.GetActorsInRange(cActor, cActor, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible));
		List<CActor> obj = ((cActor2 == null) ? new List<CActor>() : GameState.GetActorsInRange(cActor2, cActor2, CheckAdjacentRange, null, new CAbilityFilterContainer(EFilterTargetType.Enemy | EFilterTargetType.Ally), null, null, isTargetedAbility: false, null, canTargetInvisible));
		list.RemoveAll((CActor x) => x is CObjectActor);
		obj.RemoveAll((CActor x) => x is CObjectActor);
		CActor.EType targetType = ScenarioManager.CurrentScenarioState.GetActorType(targetState);
		CActor.EType selfType = ScenarioManager.CurrentScenarioState.GetActorType(selfState);
		List<CActor> list2 = list.Where((CActor x) => !CActor.AreActorsAllied(x.Type, selfType)).ToList();
		List<CActor> list3 = list.Where((CActor x) => CActor.AreActorsAllied(x.Type, selfType) && x.ActorGuid != selfState.ActorGuid).ToList();
		List<CActor> list4 = list.Where((CActor x) => CActor.AreActorsAllied(x.Type, targetType) && x.ActorGuid != selfState.ActorGuid).ToList();
		List<CActor> list5 = obj.Where((CActor x) => !CActor.AreActorsAllied(x.Type, selfType)).ToList();
		List<CActor> list6 = obj.Where((CActor x) => CActor.AreActorsAllied(x.Type, selfType) && x.ActorGuid != selfState.ActorGuid).ToList();
		bool flag = FilterTargetAdjacentActors == null;
		if (FilterTargetAdjacentActors != null)
		{
			flag = FilterTargetAdjacentActors.Compare(list.Count);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterTargetAdjacentEnemies == null;
		if (FilterTargetAdjacentEnemies != null)
		{
			flag2 = FilterTargetAdjacentEnemies.Compare(list2.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		bool flag3 = FilterTargetAdjacentAllies == null;
		if (FilterTargetAdjacentAllies != null)
		{
			flag3 = FilterTargetAdjacentAllies.Compare(list3.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		bool flag4 = FilterTargetAdjacentAlliesOfTarget == null;
		if (FilterTargetAdjacentAlliesOfTarget != null)
		{
			flag4 = FilterTargetAdjacentAlliesOfTarget.Compare(list4.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		bool flag5 = FilterCasterAdjacentEnemies == null;
		if (FilterCasterAdjacentEnemies != null)
		{
			flag5 = FilterCasterAdjacentEnemies.Compare(list5.Count);
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		bool flag6 = FilterCasterAdjacentAllies == null;
		if (FilterCasterAdjacentAllies != null)
		{
			flag6 = FilterCasterAdjacentAllies.Compare(list6.Count);
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		m_LastCheckedTargetAdjacentActors = list;
		m_LastCheckedTargetAdjacentEnemies = list2;
		m_LastCheckedTargetAdjacentAllies = list3;
		m_LastCheckedTargetAdjacentAlliesOfTarget = list4;
		m_LastCheckedCasterAdjacentEnemies = list5;
		m_LastCheckedCasterAdjacentAllies = list6;
		return flag && flag2 && flag3 && flag4 && flag5 && flag6;
	}

	private bool CheckWallAdjacencyFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentToWalls == null && FilterCasterAdjacentToWalls == null)
		{
			return true;
		}
		CTile tile = ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y];
		int num = 0;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(target.ArrayIndex.X, target.ArrayIndex.Y, eposition);
			if (adjacentTile != null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if ((!cNode.Walkable && !cNode.IsBridge) || !adjacentTile.IsMapShared(tile, hexMapOnly: true))
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		CTile tile2 = ScenarioManager.Tiles[self.ArrayIndex.X, self.ArrayIndex.Y];
		int num2 = 0;
		for (int j = 1; j < 7; j++)
		{
			ScenarioManager.EAdjacentPosition eposition2 = (ScenarioManager.EAdjacentPosition)j;
			CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(self.ArrayIndex.X, self.ArrayIndex.Y, eposition2);
			if (adjacentTile2 != null)
			{
				CNode cNode2 = ScenarioManager.PathFinder.Nodes[adjacentTile2.m_ArrayIndex.X, adjacentTile2.m_ArrayIndex.Y];
				if ((!cNode2.Walkable && !cNode2.IsBridge) || !adjacentTile2.IsMapShared(tile2, hexMapOnly: true))
				{
					num2++;
				}
			}
			else
			{
				num2++;
			}
		}
		bool flag = FilterTargetAdjacentToWalls == null;
		if (FilterTargetAdjacentToWalls != null)
		{
			flag = FilterTargetAdjacentToWalls.Compare(num);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterCasterAdjacentToWalls == null;
		if (FilterCasterAdjacentToWalls != null)
		{
			flag2 = FilterCasterAdjacentToWalls.Compare(num2);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		LastCheckedTargetAdjacentWalls = num;
		LastCheckedCasterAdjacentWalls = num2;
		return flag && flag2;
	}

	private bool CheckTileAdjacencyFilter(CActor target, CActor self, bool? canTargetInvisible = false)
	{
		if (FilterTargetAdjacentValidTiles == null && FilterCasterAdjacentValidTiles == null)
		{
			return true;
		}
		List<CTile> list = new List<CTile>();
		if (FilterTargetAdjacentValidTilesFilterList != null)
		{
			for (int i = 1; i < 7; i++)
			{
				ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(target.ArrayIndex.X, target.ArrayIndex.Y, eposition);
				foreach (EFilterTile filterTargetAdjacentValidTilesFilter in FilterTargetAdjacentValidTilesFilterList)
				{
					if (adjacentTile != null && IsValidTile(adjacentTile, filterTargetAdjacentValidTilesFilter))
					{
						list.Add(adjacentTile);
						break;
					}
				}
			}
		}
		List<CTile> list2 = new List<CTile>();
		if (FilterCasterAdjacentValidTilesFilterList != null)
		{
			for (int j = 1; j < 7; j++)
			{
				ScenarioManager.EAdjacentPosition eposition2 = (ScenarioManager.EAdjacentPosition)j;
				CTile adjacentTile2 = ScenarioManager.GetAdjacentTile(self.ArrayIndex.X, self.ArrayIndex.Y, eposition2);
				foreach (EFilterTile filterCasterAdjacentValidTilesFilter in FilterCasterAdjacentValidTilesFilterList)
				{
					if (adjacentTile2 != null && IsValidTile(adjacentTile2, filterCasterAdjacentValidTilesFilter))
					{
						list2.Add(adjacentTile2);
						break;
					}
				}
			}
		}
		bool flag = FilterTargetAdjacentValidTiles == null;
		if (FilterTargetAdjacentValidTiles != null)
		{
			flag = FilterTargetAdjacentValidTiles.Compare(list.Count);
			if (Invert)
			{
				flag = !flag;
			}
		}
		bool flag2 = FilterCasterAdjacentValidTiles == null;
		if (FilterCasterAdjacentValidTiles != null)
		{
			flag2 = FilterCasterAdjacentValidTiles.Compare(list2.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		LastCheckedTargetAdjacentTiles = list.Count;
		LastCheckedCasterAdjacentValidTiles = list2.Count;
		return flag && flag2;
	}

	private bool CheckConditionsFilters(CActor target, CActor caster)
	{
		bool flag = FilterTargetHasNegativeConditions == null;
		bool flag2 = FilterTargetHasPositiveConditions == null;
		bool flag3 = FilterTargetNegativeConditionCount == null;
		bool flag4 = FilterTargetPositiveConditionCount == null;
		bool flag5 = FilterCasterHasNegativeConditions == null;
		bool flag6 = FilterCasterHasPositiveConditions == null;
		bool flag7 = FilterCasterNegativeConditionCount == null;
		bool flag8 = FilterCasterPositiveConditionCount == null;
		if (FilterTargetHasNegativeConditions != null)
		{
			flag = FilterTargetHasNegativeConditions.Any((CCondition.ENegativeCondition a) => target.Tokens.HasKey(a));
			if (Invert)
			{
				flag = !flag;
			}
		}
		if (FilterTargetHasPositiveConditions != null)
		{
			flag2 = FilterTargetHasPositiveConditions.Any((CCondition.EPositiveCondition a) => target.Tokens.HasKey(a));
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		if (FilterTargetNegativeConditionCount != null)
		{
			flag3 = FilterTargetNegativeConditionCount.Compare(target.Tokens.CheckNegativeTokens.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		if (FilterTargetPositiveConditionCount != null)
		{
			flag4 = FilterTargetPositiveConditionCount.Compare(target.Tokens.CheckPositiveTokens.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		if (FilterCasterHasNegativeConditions != null)
		{
			flag5 = FilterCasterHasNegativeConditions.Any((CCondition.ENegativeCondition a) => caster.Tokens.HasKey(a));
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		if (FilterCasterHasPositiveConditions != null)
		{
			flag6 = FilterCasterHasPositiveConditions.Any((CCondition.EPositiveCondition a) => caster.Tokens.HasKey(a));
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		if (FilterCasterNegativeConditionCount != null)
		{
			flag7 = FilterCasterNegativeConditionCount.Compare(caster.Tokens.CheckNegativeTokens.Count);
			if (Invert)
			{
				flag7 = !flag7;
			}
		}
		if (FilterCasterPositiveConditionCount != null)
		{
			flag8 = FilterCasterPositiveConditionCount.Compare(caster.Tokens.CheckPositiveTokens.Count);
			if (Invert)
			{
				flag8 = !flag8;
			}
		}
		return flag && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8;
	}

	private bool CheckConditionsFilters_ActorState(ActorState targetState, ActorState casterState)
	{
		bool flag = FilterTargetHasNegativeConditions == null;
		bool flag2 = FilterTargetHasPositiveConditions == null;
		bool flag3 = FilterTargetNegativeConditionCount == null;
		bool flag4 = FilterTargetPositiveConditionCount == null;
		bool flag5 = FilterCasterHasNegativeConditions == null;
		bool flag6 = FilterCasterHasPositiveConditions == null;
		bool flag7 = FilterCasterNegativeConditionCount == null;
		bool flag8 = FilterCasterPositiveConditionCount == null;
		if (FilterTargetHasNegativeConditions != null)
		{
			flag = FilterTargetHasNegativeConditions.Any((CCondition.ENegativeCondition a) => targetState.NegativeConditions.Any((NegativeConditionPair n) => n.NegativeCondition == a));
			if (Invert)
			{
				flag = !flag;
			}
		}
		if (FilterTargetHasPositiveConditions != null)
		{
			flag2 = FilterTargetHasPositiveConditions.Any((CCondition.EPositiveCondition a) => targetState.PositiveConditions.Any((PositiveConditionPair n) => n.PositiveCondition == a));
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		if (FilterTargetNegativeConditionCount != null)
		{
			flag3 = FilterTargetNegativeConditionCount.Compare(targetState.NegativeConditions.Count);
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		if (FilterTargetPositiveConditionCount != null)
		{
			flag4 = FilterTargetPositiveConditionCount.Compare(targetState.PositiveConditions.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		if (FilterCasterHasNegativeConditions != null)
		{
			flag5 = FilterCasterHasNegativeConditions.Any((CCondition.ENegativeCondition a) => casterState.NegativeConditions.Any((NegativeConditionPair n) => n.NegativeCondition == a));
			if (Invert)
			{
				flag5 = !flag5;
			}
		}
		if (FilterCasterHasPositiveConditions != null)
		{
			flag6 = FilterCasterHasPositiveConditions.Any((CCondition.EPositiveCondition a) => casterState.PositiveConditions.Any((PositiveConditionPair n) => n.PositiveCondition == a));
			if (Invert)
			{
				flag6 = !flag6;
			}
		}
		if (FilterCasterNegativeConditionCount != null)
		{
			flag7 = FilterCasterNegativeConditionCount.Compare(casterState.NegativeConditions.Count);
			if (Invert)
			{
				flag7 = !flag7;
			}
		}
		if (FilterCasterPositiveConditionCount != null)
		{
			flag8 = FilterCasterPositiveConditionCount.Compare(casterState.PositiveConditions.Count);
			if (Invert)
			{
				flag8 = !flag8;
			}
		}
		return flag && flag2 && flag3 && flag4 && flag5 && flag6 && flag7 && flag8;
	}

	private bool CheckConditionImmunitiesFilters(CActor target, CActor caster)
	{
		bool flag = FilterTargetHasImmunities == null;
		bool flag2 = FilterTargetImmunitiesCount == null;
		bool flag3 = FilterCasterHasImmunities == null;
		bool flag4 = FilterCasterImmunitiesCount == null;
		if (FilterTargetHasImmunities != null)
		{
			flag = FilterTargetHasImmunities.Any((CAbility.EAbilityType a) => target.Immunities.Contains(a));
			if (Invert)
			{
				flag = !flag;
			}
		}
		if (FilterTargetImmunitiesCount != null)
		{
			flag2 = FilterTargetImmunitiesCount.Compare(target.Immunities.Count);
			if (Invert)
			{
				flag2 = !flag2;
			}
		}
		if (FilterCasterHasImmunities != null)
		{
			flag3 = FilterCasterHasImmunities.Any((CAbility.EAbilityType a) => caster.Immunities.Contains(a));
			if (Invert)
			{
				flag3 = !flag3;
			}
		}
		if (FilterCasterImmunitiesCount != null)
		{
			flag4 = FilterCasterImmunitiesCount.Compare(caster.Immunities.Count);
			if (Invert)
			{
				flag4 = !flag4;
			}
		}
		return flag && flag2 && flag3 && flag4;
	}

	private bool CheckConditionsImmunitiesFilters_ActorState(ActorState targetState, ActorState casterState)
	{
		bool num = FilterTargetHasImmunities == null;
		bool flag = FilterTargetImmunitiesCount == null;
		bool flag2 = FilterCasterHasImmunities == null;
		bool flag3 = FilterCasterImmunitiesCount == null;
		return num && flag && flag2 && flag3;
	}

	private bool CheckTargetHPtoMissingHPFilters(CActor target, CActor self)
	{
		if (FilterCompareTargetHPToYourMissingHP == null)
		{
			return true;
		}
		bool flag = FilterCompareTargetHPToYourMissingHP.Compare(target.Health, Math.Abs(self.OriginalMaxHealth - self.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetHPtoMissingHPFilters_ActorState(ActorState targetState, ActorState selfState)
	{
		if (FilterCompareTargetHPToYourMissingHP == null)
		{
			return true;
		}
		bool flag = FilterCompareTargetHPToYourMissingHP.Compare(targetState.Health, Math.Abs(selfState.MaxHealth - selfState.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetMissingHPFilters(CActor target)
	{
		if (FilterTargetMissingHP == null)
		{
			return true;
		}
		bool flag = FilterTargetMissingHP.Compare(Math.Abs(target.OriginalMaxHealth - target.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool CheckTargetMissingHPFilters_ActorState(ActorState targetState)
	{
		if (FilterTargetMissingHP == null)
		{
			return true;
		}
		bool flag = FilterTargetMissingHP.Compare(Math.Abs(targetState.MaxHealth - targetState.Health));
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	public static bool WallIsAdjacent(CTile tile)
	{
		bool result = false;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile != null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
				if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public static int ObstacleIsAdjacent(CTile tile)
	{
		int num = 0;
		List<CObjectObstacle> list;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
		}
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile == null)
			{
				continue;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
			{
				continue;
			}
			foreach (CObjectObstacle item in list)
			{
				foreach (TileIndex pathingBlocker in item.PathingBlockers)
				{
					if (adjacentTile == ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y])
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public static int ActorIsAdjacent(CTile tile, CActor actor, CActor.EType type)
	{
		int num = 0;
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile == null)
			{
				continue;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
			{
				continue;
			}
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(adjacentTile.m_ArrayIndex))
			{
				bool flag = CActor.AreActorsAllied(actor.Type, item.Type);
				if (type == CActor.EType.Ally && flag)
				{
					num++;
				}
				else if (type == CActor.EType.Enemy && !flag)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static int ActorIsAdjacent(CTile tile, CActor actor, CActor.EType type, out List<CActor> adjacent)
	{
		int num = 0;
		adjacent = new List<CActor>();
		for (int i = 1; i < 7; i++)
		{
			ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)i;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, eposition);
			if (adjacentTile == null)
			{
				continue;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if ((!cNode.Walkable && !cNode.IsBridge) || !tile.IsMapShared(adjacentTile))
			{
				continue;
			}
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(adjacentTile.m_ArrayIndex))
			{
				bool flag = CActor.AreActorsAllied(actor.Type, item.Type);
				if (type == CActor.EType.Ally && flag)
				{
					num++;
					adjacent.Add(item);
				}
				else if (type == CActor.EType.Enemy && !flag)
				{
					num++;
					adjacent.Add(item);
				}
			}
		}
		return num;
	}

	public static bool IsValidTile(CTile tile, EFilterTile tileFilter, bool includeInitial = false)
	{
		bool result = true;
		if (tileFilter.HasFlag(EFilterTile.EmptyHex))
		{
			if (ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial) != null)
			{
				result = false;
			}
			else if (tile.m_Props.Count > 0)
			{
				foreach (CObjectProp prop in tile.m_Props)
				{
					if (prop.ObjectType == ScenarioManager.ObjectImportType.Door)
					{
						if (prop is CObjectDoor { Activated: false })
						{
							result = false;
						}
					}
					else if ((prop.ObjectType != ScenarioManager.ObjectImportType.PressurePlate && prop.ObjectType != ScenarioManager.ObjectImportType.Portal && prop.ObjectType != ScenarioManager.ObjectImportType.TerrainVisualEffect && prop.ObjectType != ScenarioManager.ObjectImportType.MonsterGrave && prop.ObjectType != ScenarioManager.ObjectImportType.Trap) || (prop.ObjectType == ScenarioManager.ObjectImportType.Trap && !prop.Activated))
					{
						result = false;
					}
				}
			}
			if (CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile) != null)
			{
				result = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Obstacle))
		{
			CObjectObstacle cObjectObstacle = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile);
			}
			if (cObjectObstacle == null && !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Blocked)
			{
				result = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Trap) && tile.FindProp(ScenarioManager.ObjectImportType.Trap) == null)
		{
			result = false;
		}
		if (tileFilter.HasFlag(EFilterTile.Loot) && tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null && tile.FindProp(ScenarioManager.ObjectImportType.GoalChest) == null && tile.FindProp(ScenarioManager.ObjectImportType.MoneyToken) == null && tile.FindProp(ScenarioManager.ObjectImportType.CarryableQuestItem) == null && tile.FindProp(ScenarioManager.ObjectImportType.Resource) == null)
		{
			result = false;
		}
		if (tileFilter.HasFlag(EFilterTile.SingleHexObstacle))
		{
			CObjectProp cObjectProp = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle);
			if (cObjectProp == null || (cObjectProp is CObjectObstacle cObjectObstacle2 && cObjectObstacle2.PathingBlockers.Count > 1))
			{
				result = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.ActiveSpawner) && !tile.m_Spawners.Any((CSpawner x) => x is CInteractableSpawner && x.IsActive))
		{
			CTile spawnerTile = null;
			CInteractableSpawner cInteractableSpawner = CObjectProp.FindSpawnerWithPathingBlocker(tile.m_ArrayIndex, ref spawnerTile);
			if (cInteractableSpawner == null || !cInteractableSpawner.IsActive)
			{
				result = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.AdjacentToWall) && !WallIsAdjacent(tile))
		{
			result = false;
		}
		if (tileFilter.HasFlag(EFilterTile.DeactiveSpawner) && !tile.m_Spawners.Any((CSpawner x) => x is CInteractableSpawner && !x.IsActive))
		{
			CTile spawnerTile2 = null;
			CInteractableSpawner cInteractableSpawner2 = CObjectProp.FindSpawnerWithPathingBlocker(tile.m_ArrayIndex, ref spawnerTile2);
			if (cInteractableSpawner2 == null || cInteractableSpawner2.IsActive)
			{
				result = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Actor) && ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial) == null)
		{
			result = false;
		}
		if (tileFilter.HasFlag(EFilterTile.ObjectActor))
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_ArrayIndex, null, includeInitial);
			if (cActor == null || !(cActor is CObjectActor))
			{
				result = false;
			}
		}
		if (tileFilter.HasFlag(EFilterTile.Wall))
		{
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
			if (cNode.Walkable || cNode.IsBridge)
			{
				result = false;
			}
		}
		return result;
	}

	public CAbilityFilter(CAbilityFilter state, ReferenceDictionary references)
	{
		FilterTargetType = state.FilterTargetType;
		FilterEnemy = state.FilterEnemy;
		FilterActorType = state.FilterActorType;
		FilterPlayerClasses = references.Get(state.FilterPlayerClasses);
		if (FilterPlayerClasses == null && state.FilterPlayerClasses != null)
		{
			FilterPlayerClasses = new List<string>();
			for (int i = 0; i < state.FilterPlayerClasses.Count; i++)
			{
				string item = state.FilterPlayerClasses[i];
				FilterPlayerClasses.Add(item);
			}
			references.Add(state.FilterPlayerClasses, FilterPlayerClasses);
		}
		FilterEnemyClasses = references.Get(state.FilterEnemyClasses);
		if (FilterEnemyClasses == null && state.FilterEnemyClasses != null)
		{
			FilterEnemyClasses = new List<string>();
			for (int j = 0; j < state.FilterEnemyClasses.Count; j++)
			{
				string item2 = state.FilterEnemyClasses[j];
				FilterEnemyClasses.Add(item2);
			}
			references.Add(state.FilterEnemyClasses, FilterEnemyClasses);
		}
		FilterHeroSummonClasses = references.Get(state.FilterHeroSummonClasses);
		if (FilterHeroSummonClasses == null && state.FilterHeroSummonClasses != null)
		{
			FilterHeroSummonClasses = new List<string>();
			for (int k = 0; k < state.FilterHeroSummonClasses.Count; k++)
			{
				string item3 = state.FilterHeroSummonClasses[k];
				FilterHeroSummonClasses.Add(item3);
			}
			references.Add(state.FilterHeroSummonClasses, FilterHeroSummonClasses);
		}
		FilterObjectClasses = references.Get(state.FilterObjectClasses);
		if (FilterObjectClasses == null && state.FilterObjectClasses != null)
		{
			FilterObjectClasses = new List<string>();
			for (int l = 0; l < state.FilterObjectClasses.Count; l++)
			{
				string item4 = state.FilterObjectClasses[l];
				FilterObjectClasses.Add(item4);
			}
			references.Add(state.FilterObjectClasses, FilterObjectClasses);
		}
		FilterSummonerClasses = references.Get(state.FilterSummonerClasses);
		if (FilterSummonerClasses == null && state.FilterSummonerClasses != null)
		{
			FilterSummonerClasses = new List<string>();
			for (int m = 0; m < state.FilterSummonerClasses.Count; m++)
			{
				string item5 = state.FilterSummonerClasses[m];
				FilterSummonerClasses.Add(item5);
			}
			references.Add(state.FilterSummonerClasses, FilterSummonerClasses);
		}
		FilterTargetAdjacentValidTilesFilterList = references.Get(state.FilterTargetAdjacentValidTilesFilterList);
		if (FilterTargetAdjacentValidTilesFilterList == null && state.FilterTargetAdjacentValidTilesFilterList != null)
		{
			FilterTargetAdjacentValidTilesFilterList = new List<EFilterTile>();
			for (int n = 0; n < state.FilterTargetAdjacentValidTilesFilterList.Count; n++)
			{
				EFilterTile item6 = state.FilterTargetAdjacentValidTilesFilterList[n];
				FilterTargetAdjacentValidTilesFilterList.Add(item6);
			}
			references.Add(state.FilterTargetAdjacentValidTilesFilterList, FilterTargetAdjacentValidTilesFilterList);
		}
		FilterCasterAdjacentValidTilesFilterList = references.Get(state.FilterCasterAdjacentValidTilesFilterList);
		if (FilterCasterAdjacentValidTilesFilterList == null && state.FilterCasterAdjacentValidTilesFilterList != null)
		{
			FilterCasterAdjacentValidTilesFilterList = new List<EFilterTile>();
			for (int num = 0; num < state.FilterCasterAdjacentValidTilesFilterList.Count; num++)
			{
				EFilterTile item7 = state.FilterCasterAdjacentValidTilesFilterList[num];
				FilterCasterAdjacentValidTilesFilterList.Add(item7);
			}
			references.Add(state.FilterCasterAdjacentValidTilesFilterList, FilterCasterAdjacentValidTilesFilterList);
		}
		FilterTargetHasNegativeConditions = references.Get(state.FilterTargetHasNegativeConditions);
		if (FilterTargetHasNegativeConditions == null && state.FilterTargetHasNegativeConditions != null)
		{
			FilterTargetHasNegativeConditions = new List<CCondition.ENegativeCondition>();
			for (int num2 = 0; num2 < state.FilterTargetHasNegativeConditions.Count; num2++)
			{
				CCondition.ENegativeCondition item8 = state.FilterTargetHasNegativeConditions[num2];
				FilterTargetHasNegativeConditions.Add(item8);
			}
			references.Add(state.FilterTargetHasNegativeConditions, FilterTargetHasNegativeConditions);
		}
		FilterTargetHasPositiveConditions = references.Get(state.FilterTargetHasPositiveConditions);
		if (FilterTargetHasPositiveConditions == null && state.FilterTargetHasPositiveConditions != null)
		{
			FilterTargetHasPositiveConditions = new List<CCondition.EPositiveCondition>();
			for (int num3 = 0; num3 < state.FilterTargetHasPositiveConditions.Count; num3++)
			{
				CCondition.EPositiveCondition item9 = state.FilterTargetHasPositiveConditions[num3];
				FilterTargetHasPositiveConditions.Add(item9);
			}
			references.Add(state.FilterTargetHasPositiveConditions, FilterTargetHasPositiveConditions);
		}
		FilterCasterHasNegativeConditions = references.Get(state.FilterCasterHasNegativeConditions);
		if (FilterCasterHasNegativeConditions == null && state.FilterCasterHasNegativeConditions != null)
		{
			FilterCasterHasNegativeConditions = new List<CCondition.ENegativeCondition>();
			for (int num4 = 0; num4 < state.FilterCasterHasNegativeConditions.Count; num4++)
			{
				CCondition.ENegativeCondition item10 = state.FilterCasterHasNegativeConditions[num4];
				FilterCasterHasNegativeConditions.Add(item10);
			}
			references.Add(state.FilterCasterHasNegativeConditions, FilterCasterHasNegativeConditions);
		}
		FilterCasterHasPositiveConditions = references.Get(state.FilterCasterHasPositiveConditions);
		if (FilterCasterHasPositiveConditions == null && state.FilterCasterHasPositiveConditions != null)
		{
			FilterCasterHasPositiveConditions = new List<CCondition.EPositiveCondition>();
			for (int num5 = 0; num5 < state.FilterCasterHasPositiveConditions.Count; num5++)
			{
				CCondition.EPositiveCondition item11 = state.FilterCasterHasPositiveConditions[num5];
				FilterCasterHasPositiveConditions.Add(item11);
			}
			references.Add(state.FilterCasterHasPositiveConditions, FilterCasterHasPositiveConditions);
		}
		FilterTargetHasImmunities = references.Get(state.FilterTargetHasImmunities);
		if (FilterTargetHasImmunities == null && state.FilterTargetHasImmunities != null)
		{
			FilterTargetHasImmunities = new List<CAbility.EAbilityType>();
			for (int num6 = 0; num6 < state.FilterTargetHasImmunities.Count; num6++)
			{
				CAbility.EAbilityType item12 = state.FilterTargetHasImmunities[num6];
				FilterTargetHasImmunities.Add(item12);
			}
			references.Add(state.FilterTargetHasImmunities, FilterTargetHasImmunities);
		}
		FilterCasterHasImmunities = references.Get(state.FilterCasterHasImmunities);
		if (FilterCasterHasImmunities == null && state.FilterCasterHasImmunities != null)
		{
			FilterCasterHasImmunities = new List<CAbility.EAbilityType>();
			for (int num7 = 0; num7 < state.FilterCasterHasImmunities.Count; num7++)
			{
				CAbility.EAbilityType item13 = state.FilterCasterHasImmunities[num7];
				FilterCasterHasImmunities.Add(item13);
			}
			references.Add(state.FilterCasterHasImmunities, FilterCasterHasImmunities);
		}
		FilterFlags = state.FilterFlags;
		FilterTargetHasCharacterResource = references.Get(state.FilterTargetHasCharacterResource);
		if (FilterTargetHasCharacterResource == null && state.FilterTargetHasCharacterResource != null)
		{
			FilterTargetHasCharacterResource = new List<string>();
			for (int num8 = 0; num8 < state.FilterTargetHasCharacterResource.Count; num8++)
			{
				string item14 = state.FilterTargetHasCharacterResource[num8];
				FilterTargetHasCharacterResource.Add(item14);
			}
			references.Add(state.FilterTargetHasCharacterResource, FilterTargetHasCharacterResource);
		}
		Invert = state.Invert;
		CheckAdjacentRange = state.CheckAdjacentRange;
		UseTargetOriginalType = state.UseTargetOriginalType;
		SpecificAbilityNames = references.Get(state.SpecificAbilityNames);
		if (SpecificAbilityNames == null && state.SpecificAbilityNames != null)
		{
			SpecificAbilityNames = new List<string>();
			for (int num9 = 0; num9 < state.SpecificAbilityNames.Count; num9++)
			{
				string item15 = state.SpecificAbilityNames[num9];
				SpecificAbilityNames.Add(item15);
			}
			references.Add(state.SpecificAbilityNames, SpecificAbilityNames);
		}
		LastCheckedTargetAdjacentWalls = state.LastCheckedTargetAdjacentWalls;
		LastCheckedCasterAdjacentWalls = state.LastCheckedCasterAdjacentWalls;
		LastCheckedTargetAdjacentTiles = state.LastCheckedTargetAdjacentTiles;
		LastCheckedCasterAdjacentValidTiles = state.LastCheckedCasterAdjacentValidTiles;
		m_LastCheckedTargetAdjacentActors = references.Get(state.m_LastCheckedTargetAdjacentActors);
		if (m_LastCheckedTargetAdjacentActors == null && state.m_LastCheckedTargetAdjacentActors != null)
		{
			m_LastCheckedTargetAdjacentActors = new List<CActor>();
			for (int num10 = 0; num10 < state.m_LastCheckedTargetAdjacentActors.Count; num10++)
			{
				CActor cActor = state.m_LastCheckedTargetAdjacentActors[num10];
				CActor cActor2 = references.Get(cActor);
				if (cActor2 == null && cActor != null)
				{
					CActor cActor3 = ((cActor is CObjectActor state2) ? new CObjectActor(state2, references) : ((cActor is CEnemyActor state3) ? new CEnemyActor(state3, references) : ((cActor is CHeroSummonActor state4) ? new CHeroSummonActor(state4, references) : ((!(cActor is CPlayerActor state5)) ? new CActor(cActor, references) : new CPlayerActor(state5, references)))));
					cActor2 = cActor3;
					references.Add(cActor, cActor2);
				}
				m_LastCheckedTargetAdjacentActors.Add(cActor2);
			}
			references.Add(state.m_LastCheckedTargetAdjacentActors, m_LastCheckedTargetAdjacentActors);
		}
		m_LastCheckedTargetAdjacentEnemies = references.Get(state.m_LastCheckedTargetAdjacentEnemies);
		if (m_LastCheckedTargetAdjacentEnemies == null && state.m_LastCheckedTargetAdjacentEnemies != null)
		{
			m_LastCheckedTargetAdjacentEnemies = new List<CActor>();
			for (int num11 = 0; num11 < state.m_LastCheckedTargetAdjacentEnemies.Count; num11++)
			{
				CActor cActor4 = state.m_LastCheckedTargetAdjacentEnemies[num11];
				CActor cActor5 = references.Get(cActor4);
				if (cActor5 == null && cActor4 != null)
				{
					CActor cActor3 = ((cActor4 is CObjectActor state6) ? new CObjectActor(state6, references) : ((cActor4 is CEnemyActor state7) ? new CEnemyActor(state7, references) : ((cActor4 is CHeroSummonActor state8) ? new CHeroSummonActor(state8, references) : ((!(cActor4 is CPlayerActor state9)) ? new CActor(cActor4, references) : new CPlayerActor(state9, references)))));
					cActor5 = cActor3;
					references.Add(cActor4, cActor5);
				}
				m_LastCheckedTargetAdjacentEnemies.Add(cActor5);
			}
			references.Add(state.m_LastCheckedTargetAdjacentEnemies, m_LastCheckedTargetAdjacentEnemies);
		}
		m_LastCheckedTargetAdjacentAllies = references.Get(state.m_LastCheckedTargetAdjacentAllies);
		if (m_LastCheckedTargetAdjacentAllies == null && state.m_LastCheckedTargetAdjacentAllies != null)
		{
			m_LastCheckedTargetAdjacentAllies = new List<CActor>();
			for (int num12 = 0; num12 < state.m_LastCheckedTargetAdjacentAllies.Count; num12++)
			{
				CActor cActor6 = state.m_LastCheckedTargetAdjacentAllies[num12];
				CActor cActor7 = references.Get(cActor6);
				if (cActor7 == null && cActor6 != null)
				{
					CActor cActor3 = ((cActor6 is CObjectActor state10) ? new CObjectActor(state10, references) : ((cActor6 is CEnemyActor state11) ? new CEnemyActor(state11, references) : ((cActor6 is CHeroSummonActor state12) ? new CHeroSummonActor(state12, references) : ((!(cActor6 is CPlayerActor state13)) ? new CActor(cActor6, references) : new CPlayerActor(state13, references)))));
					cActor7 = cActor3;
					references.Add(cActor6, cActor7);
				}
				m_LastCheckedTargetAdjacentAllies.Add(cActor7);
			}
			references.Add(state.m_LastCheckedTargetAdjacentAllies, m_LastCheckedTargetAdjacentAllies);
		}
		m_LastCheckedTargetAdjacentAlliesOfTarget = references.Get(state.m_LastCheckedTargetAdjacentAlliesOfTarget);
		if (m_LastCheckedTargetAdjacentAlliesOfTarget == null && state.m_LastCheckedTargetAdjacentAlliesOfTarget != null)
		{
			m_LastCheckedTargetAdjacentAlliesOfTarget = new List<CActor>();
			for (int num13 = 0; num13 < state.m_LastCheckedTargetAdjacentAlliesOfTarget.Count; num13++)
			{
				CActor cActor8 = state.m_LastCheckedTargetAdjacentAlliesOfTarget[num13];
				CActor cActor9 = references.Get(cActor8);
				if (cActor9 == null && cActor8 != null)
				{
					CActor cActor3 = ((cActor8 is CObjectActor state14) ? new CObjectActor(state14, references) : ((cActor8 is CEnemyActor state15) ? new CEnemyActor(state15, references) : ((cActor8 is CHeroSummonActor state16) ? new CHeroSummonActor(state16, references) : ((!(cActor8 is CPlayerActor state17)) ? new CActor(cActor8, references) : new CPlayerActor(state17, references)))));
					cActor9 = cActor3;
					references.Add(cActor8, cActor9);
				}
				m_LastCheckedTargetAdjacentAlliesOfTarget.Add(cActor9);
			}
			references.Add(state.m_LastCheckedTargetAdjacentAlliesOfTarget, m_LastCheckedTargetAdjacentAlliesOfTarget);
		}
		m_LastCheckedCasterAdjacentEnemies = references.Get(state.m_LastCheckedCasterAdjacentEnemies);
		if (m_LastCheckedCasterAdjacentEnemies == null && state.m_LastCheckedCasterAdjacentEnemies != null)
		{
			m_LastCheckedCasterAdjacentEnemies = new List<CActor>();
			for (int num14 = 0; num14 < state.m_LastCheckedCasterAdjacentEnemies.Count; num14++)
			{
				CActor cActor10 = state.m_LastCheckedCasterAdjacentEnemies[num14];
				CActor cActor11 = references.Get(cActor10);
				if (cActor11 == null && cActor10 != null)
				{
					CActor cActor3 = ((cActor10 is CObjectActor state18) ? new CObjectActor(state18, references) : ((cActor10 is CEnemyActor state19) ? new CEnemyActor(state19, references) : ((cActor10 is CHeroSummonActor state20) ? new CHeroSummonActor(state20, references) : ((!(cActor10 is CPlayerActor state21)) ? new CActor(cActor10, references) : new CPlayerActor(state21, references)))));
					cActor11 = cActor3;
					references.Add(cActor10, cActor11);
				}
				m_LastCheckedCasterAdjacentEnemies.Add(cActor11);
			}
			references.Add(state.m_LastCheckedCasterAdjacentEnemies, m_LastCheckedCasterAdjacentEnemies);
		}
		m_LastCheckedCasterAdjacentAllies = references.Get(state.m_LastCheckedCasterAdjacentAllies);
		if (m_LastCheckedCasterAdjacentAllies != null || state.m_LastCheckedCasterAdjacentAllies == null)
		{
			return;
		}
		m_LastCheckedCasterAdjacentAllies = new List<CActor>();
		for (int num15 = 0; num15 < state.m_LastCheckedCasterAdjacentAllies.Count; num15++)
		{
			CActor cActor12 = state.m_LastCheckedCasterAdjacentAllies[num15];
			CActor cActor13 = references.Get(cActor12);
			if (cActor13 == null && cActor12 != null)
			{
				CActor cActor3 = ((cActor12 is CObjectActor state22) ? new CObjectActor(state22, references) : ((cActor12 is CEnemyActor state23) ? new CEnemyActor(state23, references) : ((cActor12 is CHeroSummonActor state24) ? new CHeroSummonActor(state24, references) : ((!(cActor12 is CPlayerActor state25)) ? new CActor(cActor12, references) : new CPlayerActor(state25, references)))));
				cActor13 = cActor3;
				references.Add(cActor12, cActor13);
			}
			m_LastCheckedCasterAdjacentAllies.Add(cActor13);
		}
		references.Add(state.m_LastCheckedCasterAdjacentAllies, m_LastCheckedCasterAdjacentAllies);
	}
}
