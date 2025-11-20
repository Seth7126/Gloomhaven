using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Shared;

public class CUnlockConditionTarget
{
	public static readonly string[] ValidMods = new string[12]
	{
		"*0", "*2", "x0", "x2", "-2", "-1", "+0", "0", "+1", "+2",
		"+3", "+4"
	};

	public static readonly EUnlockConditionTargetFilter[] UnlockConditionFilters = (EUnlockConditionTargetFilter[])Enum.GetValues(typeof(EUnlockConditionTargetFilter));

	public static readonly EUnlockConditionTargetSubFilter[] UnlockConditionSubFilters = (EUnlockConditionTargetSubFilter[])Enum.GetValues(typeof(EUnlockConditionTargetSubFilter));

	public EUnlockConditionTargetFilter Filter { get; private set; }

	public EUnlockConditionTargetSubFilter SubFilter { get; private set; }

	public List<string> CharacterIDs { get; private set; }

	public List<string> ExcludeCharacterIDs { get; set; }

	public int Value { get; set; }

	public List<EQuestType> QuestTypes { get; private set; }

	public List<EAdventureDifficulty> Difficulty { get; private set; }

	public List<string> TargetEnemyClassIDs { get; private set; }

	public List<string> TargetEnemyFamilies { get; private set; }

	public List<CAbility.EAbilityType> AbilityTypes { get; set; }

	public List<ElementInfusionBoardManager.EElement> Elements { get; private set; }

	public List<ElementInfusionBoardManager.EElement> Infused { get; private set; }

	public List<string> DamageSource { get; private set; }

	public List<string> Modifiers { get; private set; }

	public List<string> AttackTypes { get; private set; }

	public List<string> CauseOfDeath { get; private set; }

	public List<string> Survived { get; private set; }

	public List<string> EnemyType { get; private set; }

	public List<string> AllyType { get; private set; }

	public List<string> MonsterType { get; private set; }

	public List<string> SelfConditions { get; private set; }

	public List<string> EnemyConditions { get; private set; }

	public int EnemyMinConditions { get; private set; }

	public List<string> AllyConditions { get; private set; }

	public int AllyMinConditions { get; private set; }

	public bool DefaultAction { get; private set; }

	public bool SpecialAbility { get; private set; }

	public int Amount { get; private set; }

	public bool SingleActor { get; private set; }

	public bool SingleTarget { get; private set; }

	public bool SingleDamage { get; private set; }

	public bool SameTarget { get; private set; }

	public int? Overkill { get; private set; }

	public bool? AreaEffect { get; private set; }

	public int Targets { get; private set; }

	public CEqualityFilter TargetFilter { get; private set; }

	public EUnlockConditionalType ConditionalType { get; private set; }

	public List<string> TargetCharacterIDs { get; private set; }

	public List<string> Items { get; private set; }

	public List<string> LootTypes { get; private set; }

	public List<string> Slot { get; private set; }

	public List<string> Adjacency { get; private set; }

	public CEqualityFilter TargetPercentMaxHP { get; private set; }

	public string ScenarioResult { get; private set; }

	public bool HasFavorite { get; private set; }

	public List<string> Chests { get; private set; }

	public int Times { get; private set; }

	public CUnlockConditionTarget(EUnlockConditionTargetFilter filter, EUnlockConditionTargetSubFilter subFilter, List<string> characters, List<string> excludeCharacters, int value, List<EQuestType> questTypes = null, List<string> targetEnemyClassIDs = null, List<string> targetEnemyFamilies = null, List<CAbility.EAbilityType> abilityTypes = null, List<ElementInfusionBoardManager.EElement> elements = null, List<ElementInfusionBoardManager.EElement> infused = null, List<string> targetDamageSource = null, List<string> targetModifiers = null, List<string> targetAttackTypes = null, List<string> targetCauseOfDeath = null, List<string> targetSurvive = null, List<string> targetEnemyType = null, List<string> targetAllyType = null, List<string> targetMonsterType = null, List<string> targetSelfConditions = null, List<string> targetEnemyConditions = null, List<string> targetAllyConditions = null, bool targetSpecialAbility = false, int amount = 0, bool singleActor = false, bool singleTarget = false, bool? areaEffect = null, int targets = 0, CEqualityFilter targetFilter = null, List<string> targetCharacters = null, List<string> targetItems = null, List<string> targetLoot = null, List<string> targetSlot = null, bool singleDamage = false, CEqualityFilter percentMaxHP = null, EUnlockConditionalType conditionalType = EUnlockConditionalType.None, string scenarioResult = null, int? overkill = null, List<EAdventureDifficulty> difficulty = null, int enemyMinConditions = 0, int allyMinConditions = 0, bool defaultAction = false, List<string> adjacency = null, bool hasFavorite = false, List<string> chests = null, bool sameTarget = false, int times = 0)
	{
		Filter = filter;
		SubFilter = subFilter;
		CharacterIDs = characters;
		ExcludeCharacterIDs = excludeCharacters;
		Value = value;
		QuestTypes = questTypes;
		TargetEnemyClassIDs = targetEnemyClassIDs;
		TargetEnemyFamilies = targetEnemyFamilies;
		AbilityTypes = abilityTypes;
		Elements = elements;
		Infused = infused;
		DamageSource = targetDamageSource;
		Modifiers = targetModifiers;
		AttackTypes = targetAttackTypes;
		CauseOfDeath = targetCauseOfDeath;
		Survived = targetSurvive;
		EnemyType = targetEnemyType;
		AllyType = targetAllyType;
		MonsterType = targetMonsterType;
		SelfConditions = targetSelfConditions;
		EnemyConditions = targetEnemyConditions;
		AllyConditions = targetAllyConditions;
		SpecialAbility = targetSpecialAbility;
		Amount = amount;
		SingleActor = singleActor;
		SingleTarget = singleTarget;
		SameTarget = sameTarget;
		AreaEffect = areaEffect;
		Targets = targets;
		TargetFilter = targetFilter;
		TargetCharacterIDs = targetCharacters;
		Items = targetItems;
		LootTypes = targetLoot;
		Slot = targetSlot;
		SingleDamage = singleDamage;
		ConditionalType = conditionalType;
		TargetPercentMaxHP = percentMaxHP;
		ScenarioResult = scenarioResult;
		Overkill = overkill;
		Difficulty = difficulty;
		EnemyMinConditions = enemyMinConditions;
		AllyMinConditions = allyMinConditions;
		DefaultAction = defaultAction;
		Adjacency = adjacency;
		HasFavorite = hasFavorite;
		Chests = chests;
		Times = times;
	}

	public static bool Parse(MappingEntry ucPropertiesEntry, string fileName, out CUnlockConditionTarget unlockConditionTarget)
	{
		unlockConditionTarget = null;
		bool flag = true;
		if (YMLShared.GetMapping(ucPropertiesEntry, fileName, out var mapping))
		{
			EUnlockConditionTargetFilter eUnlockConditionTargetFilter = EUnlockConditionTargetFilter.None;
			EUnlockConditionTargetSubFilter eUnlockConditionTargetSubFilter = EUnlockConditionTargetSubFilter.Total;
			List<string> characters = null;
			List<string> excludeCharacters = null;
			List<string> targetCharacters = null;
			int value = 0;
			List<EQuestType> list = null;
			List<EAdventureDifficulty> list2 = null;
			List<string> list3 = null;
			List<string> targetEnemyClassIDs = null;
			List<string> targetEnemyFamilies = null;
			List<CAbility.EAbilityType> list4 = null;
			List<ElementInfusionBoardManager.EElement> list5 = null;
			List<ElementInfusionBoardManager.EElement> list6 = null;
			List<string> targetDamageSource = null;
			List<string> targetModifiers = null;
			List<string> targetAttackTypes = null;
			List<string> targetCauseOfDeath = null;
			List<string> targetSurvive = null;
			List<string> targetEnemyType = null;
			List<string> targetAllyType = null;
			List<string> targetMonsterType = null;
			List<string> targetSelfConditions = null;
			List<string> targetEnemyConditions = null;
			List<string> targetAllyConditions = null;
			List<string> targetItems = null;
			List<string> targetLoot = null;
			List<string> targetSlot = null;
			List<string> chests = null;
			CEqualityFilter targetFilter = null;
			CEqualityFilter percentMaxHP = null;
			EUnlockConditionalType conditionalType = EUnlockConditionalType.None;
			string scenarioResult = null;
			bool targetSpecialAbility = false;
			int amount = 0;
			bool singleActor = false;
			bool singleTarget = false;
			bool singleDamage = false;
			bool? areaEffect = null;
			int? overkill = null;
			int targets = 0;
			int enemyMinConditions = 0;
			int allyMinConditions = 0;
			bool defaultAction = false;
			bool hasFavorite = false;
			bool sameTarget = false;
			List<string> list7 = null;
			int times = 0;
			foreach (MappingEntry uctNameEntry in mapping.Entries)
			{
				if (uctNameEntry == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null UnlockConditionTarget in file: " + fileName);
					flag = false;
					continue;
				}
				switch (uctNameEntry.Key.ToString())
				{
				case "Filter":
					eUnlockConditionTargetFilter = UnlockConditionFilters.SingleOrDefault((EUnlockConditionTargetFilter x) => x.ToString() == (uctNameEntry.Value as Scalar).ToString());
					if (eUnlockConditionTargetFilter == EUnlockConditionTargetFilter.None)
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Filter entry " + uctNameEntry.Value?.ToString() + " in file: " + fileName);
						flag = false;
					}
					break;
				case "SubFilter":
					eUnlockConditionTargetSubFilter = UnlockConditionSubFilters.SingleOrDefault((EUnlockConditionTargetSubFilter x) => x.ToString() == (uctNameEntry.Value as Scalar).ToString());
					if (eUnlockConditionTargetSubFilter == EUnlockConditionTargetSubFilter.None)
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/SubFilter entry " + uctNameEntry.Value?.ToString() + " in file: " + fileName);
						flag = false;
					}
					break;
				case "CharacterIDs":
				case "CharacterID":
				case "SummonIDs":
				case "SummonID":
				{
					List<string> values26;
					if (uctNameEntry.Value is Mapping)
					{
						foreach (MappingEntry entry in (uctNameEntry.Value as Mapping).Entries)
						{
							switch (entry.Key.ToString())
							{
							case "ActorIDs":
							case "ActorID":
							{
								if (YMLShared.GetStringList(entry.Value, "UnlockCondition/Targets/Character/ActorIDs", fileName, out var values25))
								{
									characters = values25;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "TargetIDs":
							case "TargetID":
							{
								if (YMLShared.GetStringList(entry.Value, "UnlockCondition/Targets/Character/TargetIDs", fileName, out var values24))
								{
									targetCharacters = values24;
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
					else if (YMLShared.GetStringList(uctNameEntry.Value, "UnlockCondition/Targets/Character/ActorIDs", fileName, out values26))
					{
						characters = values26;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Chests":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "UnlockCondition/Targets/Chests", fileName, out var values17))
					{
						chests = values17;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ExcludeCharacterIDs":
				case "ExcludeCharacterID":
				case "ExcludeSummonIDs":
				case "ExcludeSummonID":
				{
					List<string> values10;
					if (uctNameEntry.Value is Mapping)
					{
						foreach (MappingEntry entry2 in (uctNameEntry.Value as Mapping).Entries)
						{
							string text = entry2.Key.ToString();
							if (text == "ActorIDs" || text == "ActorID")
							{
								if (YMLShared.GetStringList(entry2.Value, "UnlockCondition/Targets/Character/ActorIDs", fileName, out var values9))
								{
									excludeCharacters = values9;
								}
								else
								{
									flag = false;
								}
							}
						}
					}
					else if (YMLShared.GetStringList(uctNameEntry.Value, "UnlockCondition/Targets/Character/ActorIDs", fileName, out values10))
					{
						excludeCharacters = values10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SingleActor":
					singleActor = true;
					break;
				case "SingleTarget":
					singleTarget = true;
					break;
				case "KillInOneHit":
					singleDamage = true;
					break;
				case "Overkill":
				{
					if (YMLShared.GetIntPropertyValue(uctNameEntry.Value, "Overkill", fileName, out var value13))
					{
						overkill = value13;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Value":
				{
					if (YMLShared.GetIntPropertyValue(uctNameEntry.Value, "Value", fileName, out var value10))
					{
						value = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Count":
				case "Amount":
				{
					if (YMLShared.GetIntPropertyValue(uctNameEntry.Value, "Amount", fileName, out var value9))
					{
						amount = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Targets":
				{
					if (YMLShared.GetIntPropertyValue(uctNameEntry.Value, "Targets", fileName, out var value5))
					{
						targets = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Times":
				{
					if (YMLShared.GetIntPropertyValue(uctNameEntry.Value, "Times", fileName, out var value11))
					{
						times = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AreaEffect":
				{
					if (YMLShared.GetBoolPropertyValue(uctNameEntry.Value, "AreaEffect", fileName, out var value12))
					{
						areaEffect = value12;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "QuestTypes":
				{
					if (list == null)
					{
						list = new List<EQuestType>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "QuestTypes", fileName, out var values5))
					{
						foreach (string questTypeString in values5)
						{
							EQuestType eQuestType = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == questTypeString);
							if (eQuestType == EQuestType.None)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/QuestTypes entry " + questTypeString + " in file: " + fileName);
								flag = false;
							}
							else
							{
								list.Add(eQuestType);
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Difficulty":
				{
					if (list2 == null)
					{
						list2 = new List<EAdventureDifficulty>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "Difficulty", fileName, out var values28))
					{
						foreach (string difficultyString in values28)
						{
							EAdventureDifficulty eAdventureDifficulty = CAdventureDifficulty.AdventureDifficulties.SingleOrDefault((EAdventureDifficulty x) => x.ToString() == difficultyString);
							if (eAdventureDifficulty == EAdventureDifficulty.None)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Difficulty entry " + difficultyString + " in file: " + fileName);
								flag = false;
							}
							else
							{
								list2.Add(eAdventureDifficulty);
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Equipment":
				{
					if (list3 == null)
					{
						list3 = new List<string>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "Equipment", fileName, out var values22))
					{
						list3 = values22;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AdjacentTo":
				{
					if (list7 == null)
					{
						list7 = new List<string>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "AdjacentTo", fileName, out var values16))
					{
						list7 = values16;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "SpecialAbility":
				case "Jump":
					targetSpecialAbility = true;
					break;
				case "DefaultAction":
					defaultAction = true;
					break;
				case "HasFavorite":
					hasFavorite = true;
					break;
				case "SameTarget":
					sameTarget = true;
					break;
				case "Condition":
				case "Conditions":
				{
					if (YMLShared.GetMapping(uctNameEntry, fileName, out var mapping3))
					{
						foreach (MappingEntry entry3 in mapping3.Entries)
						{
							switch (entry3.Key.ToString())
							{
							case "Actor":
							{
								if (YMLShared.GetStringList(entry3.Value, "Actor", fileName, out var values6))
								{
									bool flag3 = true;
									foreach (string item3 in values6)
									{
										if (!Enum.TryParse<CCondition.EPositiveCondition>(item3, ignoreCase: false, out var _) && !Enum.TryParse<CCondition.ENegativeCondition>(item3, ignoreCase: false, out var _) && !item3.Contains("Doom"))
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Conditions/Actor entry " + item3 + " in file: " + fileName);
											flag3 = false;
										}
									}
									if (flag3)
									{
										targetAllyConditions = values6;
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
							case "ActorMinimum":
							{
								if (YMLShared.GetIntPropertyValue(entry3.Value, "ActorMinimum", fileName, out var value8))
								{
									allyMinConditions = value8;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "Target":
							{
								if (YMLShared.GetStringList(entry3.Value, "Target", fileName, out var values7))
								{
									bool flag4 = true;
									foreach (string item4 in values7)
									{
										if (!Enum.TryParse<CCondition.EPositiveCondition>(item4, ignoreCase: false, out var _) && !Enum.TryParse<CCondition.ENegativeCondition>(item4, ignoreCase: false, out var _) && !item4.Contains("Doom"))
										{
											SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Conditions/Target entry " + item4 + " in file: " + fileName);
											flag4 = false;
										}
									}
									if (flag4)
									{
										targetEnemyConditions = values7;
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
							case "TargetMinimum":
							{
								if (YMLShared.GetIntPropertyValue(entry3.Value, "TargetMinimum", fileName, out var value7))
								{
									enemyMinConditions = value7;
								}
								else
								{
									flag = false;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Conditions entry " + entry3.Key.ToString() + " in file: " + fileName);
								flag = false;
								break;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ScenarioResult":
				{
					if (YMLShared.GetStringPropertyValue(uctNameEntry.Value, "ScenarioResult", fileName, out var value6))
					{
						scenarioResult = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Conditional":
				{
					if (YMLShared.GetMapping(uctNameEntry, fileName, out var mapping2))
					{
						foreach (MappingEntry entry4 in mapping2.Entries)
						{
							switch (entry4.Key.ToString())
							{
							case "Type":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "Conditional/Type", fileName, out var value3))
								{
									if (Enum.TryParse<EUnlockConditionalType>(value3, ignoreCase: false, out var result))
									{
										conditionalType = result;
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Conditional/Type entry " + value3 + " in file: " + fileName);
									flag = false;
								}
								else
								{
									flag = false;
								}
								break;
							}
							case "HPPercentOfMax":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "Conditional/HPPercentofMax", fileName, out var value4))
								{
									if (CardProcessingShared.GetEqualityFilter(value4, "HPPercentofMax", fileName, out var equalityFilter2))
									{
										percentMaxHP = equalityFilter2;
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
							case "Amount":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "Conditional/Amount", fileName, out var value2))
								{
									if (CardProcessingShared.GetEqualityFilter(value2, "Amount", fileName, out var equalityFilter))
									{
										targetFilter = equalityFilter;
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
							default:
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Conditional entry " + entry4.Key.ToString() + " in file: " + fileName);
								flag = false;
								break;
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Who":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "Who", fileName, out var values21))
					{
						bool flag11 = true;
						foreach (string item5 in values21)
						{
							if (item5 != "NotSelf" && ((!Enum.TryParse<CActor.EType>(item5, ignoreCase: false, out var _) && !item5.Contains("EnemySummon")) || item5.Contains("Enemy")))
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/CharacterTypes/Actor entry " + item5 + " in file: " + fileName);
								flag11 = false;
							}
						}
						if (flag11)
						{
							targetAllyType = values21;
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
				case "MonsterType":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "MonsterType", fileName, out var values19))
					{
						bool flag9 = true;
						foreach (string item6 in values19)
						{
							switch (item6)
							{
							case "Elite":
							case "Normal":
							case "Boss":
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/MonsterType " + item6 + " in file: " + fileName);
							flag9 = false;
						}
						if (flag9)
						{
							targetMonsterType = values19;
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
				case "CharacterTypes":
				{
					if (YMLShared.GetMapping(uctNameEntry, fileName, out var mapping4))
					{
						foreach (MappingEntry entry5 in mapping4.Entries)
						{
							string text = entry5.Key.ToString();
							List<string> values14;
							if (!(text == "Actor"))
							{
								if (text == "Target")
								{
									if (YMLShared.GetStringList(entry5.Value, "Target", fileName, out var values13))
									{
										bool flag7 = true;
										for (int num = 0; num < values13.Count; num++)
										{
											if (values13[num] == "Self")
											{
												values13[num] = "OnlySelf";
											}
											if (values13[num] == "Ally")
											{
												values13.Add("Player");
												values13.Add("HeroSummon");
												values13.Add("NotSelf");
											}
											if (!Enum.TryParse<CActor.EType>(values13[num], ignoreCase: false, out var _) && !values13[num].Contains("EnemySummon") && !(values13[num] == "NotSelf") && !(values13[num] == "OnlySelf"))
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/CharacterTypes/Target entry " + values13[num] + " in file: " + fileName);
												flag7 = false;
											}
										}
										if (flag7)
										{
											targetEnemyType = values13;
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
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/CharacterTypes entry " + entry5.Key.ToString() + " in file: " + fileName);
									flag = false;
								}
							}
							else if (YMLShared.GetStringList(entry5.Value, "Actor", fileName, out values14))
							{
								bool flag8 = true;
								foreach (string item7 in values14)
								{
									if ((!Enum.TryParse<CActor.EType>(item7, ignoreCase: false, out var _) && !item7.Contains("EnemySummon")) || item7.Contains("Ally"))
									{
										SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/CharacterTypes/Actor entry " + item7 + " in file: " + fileName);
										flag8 = false;
									}
								}
								if (flag8)
								{
									targetAllyType = values14;
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
				case "Survive":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "Survive", fileName, out var values11))
					{
						bool flag5 = true;
						foreach (string item8 in values11)
						{
							if (!item8.Contains("All") && !item8.Contains("True"))
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Survive entry " + item8 + " in file: " + fileName);
								flag5 = false;
							}
						}
						if (flag5)
						{
							targetSurvive = values11;
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
				case "CauseOfDeath":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "CauseOfDeath", fileName, out var values4))
					{
						bool flag2 = true;
						foreach (string item9 in values4)
						{
							if (!Enum.TryParse<CActor.ECauseOfDeath>(item9, out var _))
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/CauseOfDeath entry " + item9 + " in file: " + fileName);
								flag2 = false;
							}
						}
						if (flag2)
						{
							targetCauseOfDeath = values4;
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
				case "AttackTypes":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "AttackTypes", fileName, out var values2))
					{
						targetAttackTypes = values2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Items":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "Items", fileName, out var values29))
					{
						targetItems = values29;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "LootType":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "LootType", fileName, out var values27))
					{
						targetLoot = values27;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Slot":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "Slot", fileName, out var values23))
					{
						targetSlot = values23;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Modifiers":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "Modifiers", fileName, out var values20))
					{
						bool flag10 = true;
						foreach (string item10 in values20)
						{
							if (!ValidMods.Contains(item10))
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Modifiers entry " + item10 + " in file: " + fileName);
								flag10 = false;
							}
						}
						if (flag10)
						{
							targetModifiers = values20;
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
				case "EnemyClasses":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "EnemyClasses", fileName, out var values18))
					{
						targetEnemyClassIDs = values18;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "EnemyFamilies":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "EnemyFamilies", fileName, out var values15))
					{
						targetEnemyFamilies = values15;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "DamageSource":
				{
					if (YMLShared.GetStringList(uctNameEntry.Value, "DamageSource", fileName, out var values12))
					{
						bool flag6 = true;
						foreach (string item11 in values12)
						{
							if (!Enum.TryParse<CActor.ECauseOfDamage>(item11, ignoreCase: false, out var _) && !item11.Contains("Poison"))
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/DamageSource entry " + item11 + " in file: " + fileName);
								flag6 = false;
							}
						}
						if (flag6)
						{
							targetDamageSource = values12;
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
				case "AbilityTypes":
				{
					if (list4 == null)
					{
						list4 = new List<CAbility.EAbilityType>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "AbilityTypes", fileName, out var values8))
					{
						foreach (string abilityTypeString in values8)
						{
							CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == abilityTypeString);
							if (eAbilityType == CAbility.EAbilityType.None)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/AbilityType entry " + abilityTypeString + " in file: " + fileName);
								flag = false;
							}
							else
							{
								list4.Add(eAbilityType);
							}
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Infused":
				{
					if (list6 == null)
					{
						list6 = new List<ElementInfusionBoardManager.EElement>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "Infused", fileName, out var values3))
					{
						foreach (string elementString2 in values3)
						{
							try
							{
								ElementInfusionBoardManager.EElement item2 = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString2);
								list6.Add(item2);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Elements entry " + elementString2 + " in file: " + fileName);
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
				case "Elements":
				{
					if (list5 == null)
					{
						list5 = new List<ElementInfusionBoardManager.EElement>();
					}
					if (YMLShared.GetStringList(uctNameEntry.Value, "Elements", fileName, out var values))
					{
						foreach (string elementString in values)
						{
							try
							{
								ElementInfusionBoardManager.EElement item = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == elementString);
								list5.Add(item);
							}
							catch
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid UnlockCondition/Targets/Elements entry " + elementString + " in file: " + fileName);
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
				default:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected UnlockCondition/Targets entry " + uctNameEntry.Key.ToString() + " in file: " + fileName);
					flag = false;
					break;
				}
			}
			if (flag)
			{
				unlockConditionTarget = new CUnlockConditionTarget(eUnlockConditionTargetFilter, eUnlockConditionTargetSubFilter, characters, excludeCharacters, value, list, targetEnemyClassIDs, targetEnemyFamilies, list4, list5, list6, targetDamageSource, targetModifiers, targetAttackTypes, targetCauseOfDeath, targetSurvive, targetEnemyType, targetAllyType, targetMonsterType, targetSelfConditions, targetEnemyConditions, targetAllyConditions, targetSpecialAbility, amount, singleActor, singleTarget, areaEffect, targets, targetFilter, targetCharacters, targetItems, targetLoot, targetSlot, singleDamage, percentMaxHP, conditionalType, scenarioResult, overkill, list2, enemyMinConditions, allyMinConditions, defaultAction, list7, hasFavorite, chests, sameTarget, times);
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}
}
