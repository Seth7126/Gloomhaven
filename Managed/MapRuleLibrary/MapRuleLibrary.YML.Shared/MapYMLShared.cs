using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Shared;

public static class MapYMLShared
{
	public const float GMScreenTop = 91.2f;

	public const float GMScreenLeft = 118f;

	public const float GMScreenRight = -118f;

	public const float GMScreenBottom = -91.2f;

	public const float GMMapImageTop = 60f;

	public const float GMMapImageLeft = 148f;

	public const float GMMapImageRight = 1950f;

	public const float GMMapImageBottom = 1409f;

	public const float CampaignScreenTop = 87.16f;

	public const float CampaignScreenLeft = 111.51f;

	public const float CampaignScreenRight = -111.1f;

	public const float CampaignScreenBottom = -88.8f;

	public const float CampaignMapImageTop = 93f;

	public const float CampaignMapImageLeft = 124f;

	public const float CampaignMapImageRight = 2445f;

	public const float CampaignMapImageBottom = 1918f;

	public static CVector3 GetScreenPointFromMap(int mapX, int mapY)
	{
		if (MapRuleLibraryClient.MRLYML.MapMode == ScenarioManager.EDLLMode.Guildmaster)
		{
			if ((float)mapY < 60f || (float)mapY > 1409f)
			{
				DLLDebug.LogError("Invalid Y coordinate for map location, using default map location");
				return new CVector3(0f, 0f, 0f);
			}
			if ((float)mapX < 148f || (float)mapX > 1950f)
			{
				DLLDebug.LogError("Invalid X coordinate for map location, using default map location");
				return new CVector3(0f, 0f, 0f);
			}
			float z = ((float)mapX - 148f) / 1802f * -236f + 118f;
			return new CVector3(((float)mapY - 60f) / 1349f * -182.4f + 91.2f, 0f, z);
		}
		if (MapRuleLibraryClient.MRLYML.MapMode == ScenarioManager.EDLLMode.Campaign)
		{
			if ((float)mapY < 93f || (float)mapY > 1918f)
			{
				DLLDebug.LogError("Invalid Y coordinate for map location");
				return new CVector3(0f, 0f, 0f);
			}
			if ((float)mapX < 124f || (float)mapX > 2445f)
			{
				DLLDebug.LogError("Invalid X coordinate for map location");
				return new CVector3(0f, 0f, 0f);
			}
			float z2 = ((float)mapX - 124f) / 2321f * -222.61f + 111.51f;
			return new CVector3(((float)mapY - 93f) / 1825f * -175.96f + 87.16f, 0f, z2);
		}
		DLLDebug.LogError("Invalid Map Mode set for screen positions in GetScreenPointFromMap");
		return new CVector3(0f, 0f, 0f);
	}

	public static bool GetDifficulty(Mapping mapping, string entryName, string fileName, out CAdventureDifficulty difficulty)
	{
		difficulty = null;
		bool flag = true;
		if (mapping == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null difficulty mapping in file " + fileName);
			return false;
		}
		string text = null;
		List<EAdventureDifficulty> list = new List<EAdventureDifficulty>();
		float threat = 1f;
		float health = 1f;
		float xp = 1f;
		float gold = 1f;
		int bless = 0;
		int curse = 0;
		int enemyLevelModifier = 0;
		bool positiveEffect = true;
		bool flag2 = false;
		foreach (MappingEntry entry in mapping.Entries)
		{
			switch (entry.Key.ToString())
			{
			case "LoadAsNewDifficulty":
			{
				if (YMLShared.GetBoolPropertyValue(entry.Value, "LoadAsNewDifficulty", fileName, out var value5))
				{
					flag2 = value5;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Text":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "Text", fileName, out var value10))
				{
					text = value10;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "ActiveOn":
			{
				if (YMLShared.GetStringList(entry.Value, "ActiveOn", fileName, out var values))
				{
					foreach (string activeOnValue in values)
					{
						EAdventureDifficulty eAdventureDifficulty = CAdventureDifficulty.AdventureDifficulties.SingleOrDefault((EAdventureDifficulty s) => s.ToString() == activeOnValue);
						if (eAdventureDifficulty != EAdventureDifficulty.None)
						{
							list.Add(eAdventureDifficulty);
							continue;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ActiveOn value '" + activeOnValue + "' specified in " + entryName + " in file " + fileName);
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Threat":
			{
				if (YMLShared.GetFloatPropertyValue(entry.Value, "Threat", fileName, out var value8))
				{
					threat = value8;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "HeroHealth":
			{
				if (YMLShared.GetFloatPropertyValue(entry.Value, "HeroHealth", fileName, out var value2))
				{
					health = value2;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "XP":
			{
				if (YMLShared.GetFloatPropertyValue(entry.Value, "XP", fileName, out var value7))
				{
					xp = value7;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Gold":
			{
				if (YMLShared.GetFloatPropertyValue(entry.Value, "Gold", fileName, out var value3))
				{
					gold = value3;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "BlessCards":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "BlessCards", fileName, out var value9))
				{
					bless = value9;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "CurseCards":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "CurseCards", fileName, out var value6))
				{
					curse = value6;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "ScenarioLevelModifier":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "ScenarioLevelModifier", fileName, out var value4))
				{
					enemyLevelModifier = value4;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "PositiveEffect":
			{
				if (YMLShared.GetBoolPropertyValue(entry.Value, entryName + "/PositiveEffect", fileName, out var value))
				{
					positiveEffect = value;
				}
				else
				{
					flag = false;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry in Difficulty '" + entry.Key.ToString() + "' in file " + fileName);
				flag = false;
				break;
			}
		}
		if (text == null && !flag2)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "No text has been specified for the difficulty setting " + entryName + " in file " + fileName);
			flag = false;
		}
		else if (list.Count == 0 && !flag2)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ActiveOn value has been specified for the difficulty setting " + entryName + " in file " + fileName);
			flag = false;
		}
		if (flag)
		{
			difficulty = new CAdventureDifficulty(text, list, positiveEffect, threat, health, xp, gold, bless, curse, enemyLevelModifier, flag2);
		}
		return flag;
	}

	public static bool ProcessMapScenario(List<MappingEntry> entries, string fileName, string questID, int scenarioIndex, out CMapScenario mapScenario)
	{
		mapScenario = null;
		bool flag = true;
		try
		{
			if (entries != null)
			{
				string id = questID + "_Scenario" + scenarioIndex;
				TileIndex tileIndex = null;
				float? pathDistancePercentage = null;
				List<int> list = null;
				List<Tuple<string, int>> list2 = null;
				List<Tuple<string, int>> list3 = null;
				foreach (MappingEntry entry in entries)
				{
					switch (entry.Key.ToString())
					{
					case "EventChance":
					{
						if (YMLShared.GetIntList(entry.Value, "EventChance", fileName, out var values))
						{
							list = values;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "EventPool":
						list2 = new List<Tuple<string, int>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence2 = entry.Value as Sequence;
							if (sequence2.Entries[0] is Sequence)
							{
								foreach (DataItem entry2 in sequence2.Entries)
								{
									if (YMLShared.GetTupleStringInt(entry2, "EventPool", fileName, out var tuple3))
									{
										list2.Add(tuple3);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence2.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringInt(sequence2, "EventPool", fileName, out var tuple4))
								{
									list2.Add(tuple4);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid EventPool entry, must be list of [EventName, Chance] pairs. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid EventPool entry, must be list of [EventName, Chance] pairs. File: " + fileName);
							flag = false;
						}
						break;
					case "MapLocation":
					{
						if (YMLShared.GetIntList(entry.Value, "MapLocation", fileName, out var values2))
						{
							if (values2.Count == 2)
							{
								tileIndex = new TileIndex(values2[0], values2[1]);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry.Key?.ToString() + ".  Must be two integers as coordinates in a list, e.g. [12, 34].  File:\n" + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PathDistancePercentage":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "PathDistancePercentage", fileName, out var value))
						{
							pathDistancePercentage = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ScenarioPool":
						list3 = new List<Tuple<string, int>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence = entry.Value as Sequence;
							if (sequence.Entries[0] is Sequence)
							{
								foreach (DataItem entry3 in sequence.Entries)
								{
									if (YMLShared.GetTupleStringInt(entry3, "EventPool", fileName, out var tuple))
									{
										list3.Add(tuple);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringInt(sequence, "EventPool", fileName, out var tuple2))
								{
									list3.Add(tuple2);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ScenarioPool entry, must be list of [ScenarioName, Chance] pairs. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ScenarioPool entry, must be list of [ScenarioName, Chance] pairs. File: " + fileName);
							flag = false;
						}
						break;
					}
				}
				if (!pathDistancePercentage.HasValue && tileIndex == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No MapLocation and PathDistancePercentage specified in file: " + fileName + " specify one");
					flag = false;
				}
				if (pathDistancePercentage.HasValue && tileIndex != null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Both MapLocation and PathDistancePercentage specified in file: " + fileName + " specify only one");
					flag = false;
				}
				if (list2 != null || list != null)
				{
					if (list2 == null || list == null)
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Both Event Pool and Event Chance must be specified together.  File: " + fileName);
						flag = false;
					}
					else if (list.Count != 8)
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Event Chance array size.  Must be exactly " + 8 + " length.  File: " + fileName);
						flag = false;
					}
				}
				if (flag)
				{
					if (tileIndex != null)
					{
						mapScenario = new CMapScenario(id, null, null, null, tileIndex, new CUnlockCondition(), fileName, list, list2, list3);
					}
					else if (pathDistancePercentage.HasValue)
					{
						mapScenario = new CMapScenario(id, null, null, null, pathDistancePercentage, new CUnlockCondition(), fileName, list, list2, list3);
					}
					else
					{
						flag = false;
					}
				}
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n");
				flag = false;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, ex.Message + "\n" + ex.StackTrace);
			flag = false;
		}
		return flag;
	}

	public static string RollTupleList(List<Tuple<string, int>> tupleList)
	{
		if (tupleList != null && tupleList.Count > 0)
		{
			int num = AdventureState.MapState.MapRNG.Next(tupleList.Select((Tuple<string, int> s) => s.Item2).Sum());
			int num2 = 0;
			foreach (Tuple<string, int> tuple in tupleList)
			{
				num2 += tuple.Item2;
				if (num <= num2)
				{
					return tuple.Item1;
				}
			}
			SimpleLog.AddToSimpleLog("MapRNG (roll tuple): " + AdventureState.MapState.PeekMapRNG);
		}
		return null;
	}

	public static List<TreasureTable> ValidateTreasureTableRewards(List<TreasureTable> initialTables, int chapter, int subChapter)
	{
		List<TreasureTable> list = initialTables.Select((TreasureTable s) => s.Copy()).ToList();
		List<CItem> allUnlockedItems = AdventureState.MapState.MapParty.GetAllUnlockedItems();
		foreach (TreasureTable item in list)
		{
			if (item.Groups != null && item.Groups.Count > 0)
			{
				item.Groups = ValidateIsGroupEmpty(ValidateTreasureTableGroupRewards(item.Groups, chapter, subChapter, allUnlockedItems));
			}
			if (item.Entries != null && item.Entries.Count > 0)
			{
				item.Entries = ValidateTreasureTableEntryRewards(item.Entries, chapter, subChapter, allUnlockedItems);
			}
		}
		return list;
	}

	private static List<TreasureTableGroup> ValidateIsGroupEmpty(List<TreasureTableGroup> groups)
	{
		if (groups == null)
		{
			return null;
		}
		List<TreasureTableGroup> list = groups.ToList();
		foreach (TreasureTableGroup item in list)
		{
			item.Groups = ValidateIsGroupEmpty(item.Groups);
		}
		foreach (TreasureTableGroup item2 in list.ToList())
		{
			if ((item2.Entries == null || item2.Entries.Count == 0) && (item2.Groups == null || item2.Groups.Count == 0))
			{
				list.Remove(item2);
			}
		}
		return list;
	}

	private static List<TreasureTableGroup> ValidateTreasureTableGroupRewards(List<TreasureTableGroup> groups, int chapter, int subChapter, List<CItem> allUnlockedItems)
	{
		foreach (TreasureTableGroup group in groups)
		{
			if (group.Groups != null && group.Groups.Count > 0)
			{
				group.Groups = ValidateTreasureTableGroupRewards(group.Groups, chapter, subChapter, allUnlockedItems);
			}
			if (group.Entries != null && group.Entries.Count > 0)
			{
				group.Entries = ValidateTreasureTableEntryRewards(group.Entries, chapter, subChapter, allUnlockedItems);
			}
		}
		return groups;
	}

	private static List<TreasureTableEntry> ValidateTreasureTableEntryRewards(List<TreasureTableEntry> entries, int chapter, int subChapter, List<CItem> allUnlockedItems)
	{
		foreach (TreasureTableEntry entry in entries.ToList())
		{
			if (entry.ChapterFilter != int.MaxValue)
			{
				if (entry.ChapterFilter > chapter)
				{
					entries.Remove(entry);
					continue;
				}
				if (entry.ChapterFilter == chapter && entry.SubChapterFilter != int.MaxValue && entry.SubChapterFilter > subChapter)
				{
					entries.Remove(entry);
					continue;
				}
			}
			if (entry.Type == ETreasureType.Item)
			{
				ItemCardYMLData item = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == entry.ItemID);
				if (item.IsProsperityItem && !AdventureState.MapState.IsCampaign)
				{
					if (!allUnlockedItems.Any((CItem a) => a.ID == item.ID) || allUnlockedItems.Where((CItem w) => w.ID == item.ID).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item.Rarity))
					{
						entries.Remove(entry);
					}
				}
				else if (allUnlockedItems.Where((CItem w) => w.ID == item.ID).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item.Rarity) && !AdventureState.MapState.IsCampaign)
				{
					entries.Remove(entry);
				}
			}
			if (entry.Type == ETreasureType.ItemStock)
			{
				ItemCardYMLData item2 = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == entry.ItemID);
				if (item2.IsProsperityItem && !AdventureState.MapState.IsCampaign)
				{
					if (!allUnlockedItems.Any((CItem a) => a.ID == item2.ID) || allUnlockedItems.Where((CItem w) => w.ID == item2.ID).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item2.Rarity))
					{
						entries.Remove(entry);
					}
				}
				else if (allUnlockedItems.Where((CItem w) => w.ID == item2.ID).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item2.Rarity))
				{
					entries.Remove(entry);
				}
			}
			if (entry.Type == ETreasureType.UnlockProsperityItem)
			{
				ItemCardYMLData item3 = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == entry.ItemID);
				if (allUnlockedItems.Where((CItem w) => w.ID == item3.ID).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item3.Rarity) && !AdventureState.MapState.IsCampaign)
				{
					entries.Remove(entry);
				}
			}
			if (entry.Type == ETreasureType.UnlockProsperityItemStock)
			{
				ItemCardYMLData item4 = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == entry.ItemID);
				if (allUnlockedItems.Where((CItem w) => w.ID == item4.ID).Count() >= AdventureState.MapState.HeadquartersState.Headquarters.GetMaxItemStock(item4.Rarity))
				{
					entries.Remove(entry);
				}
			}
			if (entry.Type != ETreasureType.UnlockQuest)
			{
				continue;
			}
			CQuestState cQuestState = AdventureState.MapState.AllQuests.FirstOrDefault((CQuestState f) => f.ID == entry.UnlockQuest);
			if (cQuestState != null)
			{
				bool flag = cQuestState.Quest.Type == EQuestType.Relic && AdventureState.MapState.DLLMode == ScenarioManager.EDLLMode.Guildmaster;
				if (cQuestState.QuestState != CQuestState.EQuestState.Locked && !flag)
				{
					entries.Remove(entry);
				}
			}
		}
		return entries;
	}
}
