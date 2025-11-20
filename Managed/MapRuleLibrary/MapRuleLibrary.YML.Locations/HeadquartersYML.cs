using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Locations;

public class HeadquartersYML
{
	public CHeadquarters Headquarters { get; private set; }

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		bool flag = true;
		try
		{
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				string gameMode = null;
				string text = string.Empty;
				string localisedName = null;
				string localisedDescription = null;
				string mesh = null;
				TileIndex mapLocation = null;
				List<TileIndex> list = new List<TileIndex>();
				List<TileIndex> list2 = new List<TileIndex>();
				List<Tuple<string, TileIndex>> list3 = new List<Tuple<string, TileIndex>>();
				List<Tuple<string, TileIndex>> list4 = new List<Tuple<string, TileIndex>>();
				List<Tuple<string, TileIndex>> list5 = new List<Tuple<string, TileIndex>>();
				CUnlockCondition unlockCondition = null;
				List<CMapScenario> list6 = new List<CMapScenario>();
				int startingPerksAmount = 0;
				List<string> startupTreasureTableNames = new List<string>();
				List<string> removeStartupTreasureTableNames = new List<string>();
				List<string> retirementTreasureTableNames = new List<string>();
				List<string> removeRetirementTreasureTableNames = new List<string>();
				List<string> characterUnlockAlternateTreasureTableNames = new List<string>();
				List<string> removeCharacterUnlockAlternateTreasureTableNames = new List<string>();
				int createCharacterGoldPerLevelAmount = 0;
				List<string> createCharacterTreasureTableNames = new List<string>();
				List<string> removeCreateCharacterTreasureTableNames = new List<string>();
				List<string> questNames = new List<string>();
				List<string> removeQuestNames = new List<string>();
				List<string> tutorialQuestNames = new List<string>();
				List<string> removeTutorialQuestNames = new List<string>();
				List<string> tutorialMessages = new List<string>();
				List<string> removeTutorialMessages = new List<string>();
				List<int> eventChance = null;
				List<Tuple<string, int>> list7 = new List<Tuple<string, int>>();
				List<string> removeEventPool = null;
				ScenarioLevelTable slt = null;
				List<Tuple<CItem.EItemRarity, int>> list8 = new List<Tuple<CItem.EItemRarity, int>>();
				List<CHeadquarters.ChapterDifficulty> list9 = new List<CHeadquarters.ChapterDifficulty>();
				List<int> values = null;
				List<int> values2 = null;
				List<int> values3 = null;
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "GameMode":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "GameMode", fileName, out var value2))
						{
							gameMode = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value16))
						{
							text = value16;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedName":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value12))
						{
							localisedName = value12;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedDescription":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value))
						{
							localisedDescription = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Mesh":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value13))
						{
							mesh = value13;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MapLocation":
					{
						if (YMLShared.GetIntList(entry.Value, "MapLocation", fileName, out var values12))
						{
							if (values12.Count == 2)
							{
								mapLocation = new TileIndex(values12[0], values12[1]);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid MapLocation entry " + entry.Key?.ToString() + ".  Must be two integers as coordinates in a list, e.g. [12, 34].  File:\n" + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "JobLocations":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence8))
						{
							foreach (DataItem entry2 in sequence8.Entries)
							{
								if (YMLShared.GetIntList(entry2, "JobLocations", fileName, out var values17, allowScalar: false) && values17.Count == 2)
								{
									list.Add(new TileIndex(values17[0], values17[1]));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid JobLocations entry " + entry.Key?.ToString() + ".  Must be list of integer pairs as coordinates in a list, e.g. [[12, 34], [45,67]].  File:\n" + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveJobLocations":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence4))
						{
							foreach (DataItem entry3 in sequence4.Entries)
							{
								if (YMLShared.GetIntList(entry3, "RemoveJobLocations", fileName, out var values5, allowScalar: false) && values5.Count == 2)
								{
									list2.Add(new TileIndex(values5[0], values5[1]));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RemoveJobLocations entry " + entry.Key?.ToString() + ".  Must be list of integer pairs as coordinates in a list, e.g. [[12, 34], [45,67]].  File:\n" + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StreetLocations":
						list3 = new List<Tuple<string, TileIndex>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence5 = entry.Value as Sequence;
							if (sequence5.Entries[0] is Sequence)
							{
								foreach (DataItem entry4 in sequence5.Entries)
								{
									if (YMLShared.GetTupleStringTileIndex(entry4, "StreetLocations", fileName, out var tuple4))
									{
										Tuple<string, TileIndex> item3 = new Tuple<string, TileIndex>(tuple4.Item1, new TileIndex(tuple4.Item2[0], tuple4.Item2[1]));
										list3.Add(item3);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence5.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringTileIndex(sequence5, "StreetLocations", fileName, out var tuple5))
								{
									Tuple<string, TileIndex> item4 = new Tuple<string, TileIndex>(tuple5.Item1, new TileIndex(tuple5.Item2[0], tuple5.Item2[1]));
									list3.Add(item4);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid StreetLocations entry, must be list of [StreetName, X, Y] tuples. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid StreetLocations entry, must be list of [StreetName, X, Y] tuples. File: " + fileName);
							flag = false;
						}
						break;
					case "RemoveStreetLocations":
						list4 = new List<Tuple<string, TileIndex>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence6 = entry.Value as Sequence;
							if (sequence6.Entries[0] is Sequence)
							{
								foreach (DataItem entry5 in sequence6.Entries)
								{
									if (YMLShared.GetTupleStringTileIndex(entry5, "RemoveStreetLocations", fileName, out var tuple6))
									{
										Tuple<string, TileIndex> item5 = new Tuple<string, TileIndex>(tuple6.Item1, new TileIndex(tuple6.Item2[0], tuple6.Item2[1]));
										list4.Add(item5);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence6.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringTileIndex(sequence6, "RemoveStreetLocations", fileName, out var tuple7))
								{
									Tuple<string, TileIndex> item6 = new Tuple<string, TileIndex>(tuple7.Item1, new TileIndex(tuple7.Item2[0], tuple7.Item2[1]));
									list4.Add(item6);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RemoveStreetLocations entry, must be list of [StreetName, X, Y] tuples. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RemoveStreetLocations entry, must be list of [StreetName, X, Y] tuples. File: " + fileName);
							flag = false;
						}
						break;
					case "UnlockCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition2))
						{
							unlockCondition = unlockCondition2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StartingScenario":
						if (list6.Count == 0)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping))
							{
								if (MapYMLShared.ProcessMapScenario(mapping.Entries, fileName, text, 0, out var mapScenario2))
								{
									if (ScenarioRuleClient.SRLYML.GetScenarioDefinition(text) != null)
									{
										list6.Add(mapScenario2);
										break;
									}
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {text} in {fileName}");
									flag = false;
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
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Single starting scenario already specified for Headquarters, use StartingScenarios instead to specify multiple in a list. File " + fileName);
							flag = false;
						}
						break;
					case "StartingScenarios":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence3))
						{
							int num = 0;
							foreach (DataItem entry6 in sequence3.Entries)
							{
								if (entry6 is Mapping)
								{
									num++;
									if (MapYMLShared.ProcessMapScenario((entry6 as Mapping).Entries, fileName, text, num, out var mapScenario))
									{
										if (ScenarioRuleClient.SRLYML.GetScenarioDefinition(text) != null || text == "Demonsgate" || text == "Gloomhaven")
										{
											list6.Add(mapScenario);
											continue;
										}
										SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {text} in {fileName}");
										flag = false;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected StartingScenarios entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected StartingScenarios entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "StartupTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "StartupTreasureTables", fileName, out var values20))
						{
							foreach (string name11 in values20)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name11 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name11} in {fileName}");
									flag = false;
								}
							}
							startupTreasureTableNames = values20;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RetirementTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "RetirementTreasureTables", fileName, out var values15))
						{
							foreach (string name7 in values15)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name7 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name7} in {fileName}");
									flag = false;
								}
							}
							retirementTreasureTableNames = values15;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterUnlockAlternateTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "CharacterUnlockAlternateTreasureTables", fileName, out var values13))
						{
							foreach (string name6 in values13)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name6 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name6} in {fileName}");
									flag = false;
								}
							}
							characterUnlockAlternateTreasureTableNames = values13;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StartingPerksAmount":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "StartingPerksAmount", fileName, out var value3))
						{
							startingPerksAmount = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveStartupTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveStartupTreasureTables", fileName, out var values6))
						{
							foreach (string name2 in values6)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name2 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name2} in {fileName}");
									flag = false;
								}
							}
							removeStartupTreasureTableNames = values6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveRetirementTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveRetirementTreasureTables", fileName, out var values21))
						{
							foreach (string name12 in values21)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name12 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name12} in {fileName}");
									flag = false;
								}
							}
							removeRetirementTreasureTableNames = values21;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveCharacterUnlockAlternateTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveCharacterUnlockAlternateTreasureTables", fileName, out var values18))
						{
							foreach (string name9 in values18)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name9 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name9} in {fileName}");
									flag = false;
								}
							}
							removeCharacterUnlockAlternateTreasureTableNames = values18;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CreateCharacterGoldPerLevelAmount":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "CreateCharacterGoldPerLevelAmount", fileName, out var value14))
						{
							createCharacterGoldPerLevelAmount = value14;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CreateCharacterTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "CreateCharacterTreasureTables", fileName, out var values9))
						{
							foreach (string name5 in values9)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name5 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name5} in {fileName}");
									flag = false;
								}
							}
							createCharacterTreasureTableNames = values9;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveCreateCharacterTreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveCreateCharacterTreasureTables", fileName, out var values7))
						{
							foreach (string name3 in values7)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name3 == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name3} in {fileName}");
									flag = false;
								}
							}
							removeCreateCharacterTreasureTableNames = values7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Quests":
					{
						if (YMLShared.GetStringList(entry.Value, "Quests", fileName, out var values8))
						{
							foreach (string name4 in values8)
							{
								if (MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest q) => name4 == q.ID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name4} in {fileName}");
									flag = false;
								}
							}
							questNames = values8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveQuests":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveQuests", fileName, out var values4))
						{
							foreach (string name in values4)
							{
								if (MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest q) => name == q.ID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name} in {fileName}");
									flag = false;
								}
							}
							removeQuestNames = values4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TutorialQuestNames":
					{
						if (YMLShared.GetStringList(entry.Value, "TutorialQuestNames", fileName, out var values19))
						{
							foreach (string name10 in values19)
							{
								if (MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest q) => name10 == q.ID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name10} in {fileName}");
									flag = false;
								}
							}
							tutorialQuestNames = values19;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveTutorialQuestNames":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveTutorialQuestNames", fileName, out var values16))
						{
							foreach (string name8 in values16)
							{
								if (MapRuleLibraryClient.MRLYML.Quests.SingleOrDefault((CQuest q) => name8 == q.ID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name8} in {fileName}");
									flag = false;
								}
							}
							removeTutorialQuestNames = values16;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TutorialMessages":
					{
						if (YMLShared.GetStringList(entry.Value, "TutorialMessages", fileName, out var values14))
						{
							foreach (string message2 in values14)
							{
								if (MapRuleLibraryClient.MRLYML.MapMessages.SingleOrDefault((CMapMessage m) => message2 == m.MessageID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {message2} in {fileName}");
									flag = false;
								}
							}
							tutorialMessages = values14;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveTutorialMessages":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveTutorialMessages", fileName, out var values11))
						{
							foreach (string message in values11)
							{
								if (MapRuleLibraryClient.MRLYML.MapMessages.SingleOrDefault((CMapMessage m) => message == m.MessageID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {message} in {fileName}");
									flag = false;
								}
							}
							removeTutorialMessages = values11;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RoadEventChance":
					{
						if (YMLShared.GetIntList(entry.Value, "RoadEventChance", fileName, out var values10))
						{
							eventChance = values10;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RoadEventPool":
						list7 = new List<Tuple<string, int>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence2 = entry.Value as Sequence;
							if (sequence2.Entries[0] is Sequence)
							{
								foreach (DataItem entry7 in sequence2.Entries)
								{
									if (YMLShared.GetTupleStringInt(entry7, "RoadEventPool", fileName, out var outEventPoolTuple))
									{
										if (MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent re) => outEventPoolTuple.Item1 == re.ID) != null)
										{
											list7.Add(outEventPoolTuple);
											continue;
										}
										SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outEventPoolTuple.Item1} in {fileName}");
										flag = false;
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence2.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringInt(sequence2, "RoadEventPool", fileName, out var tuple3))
								{
									list7.Add(tuple3);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RoadEventPool entry, must be list of [RoadEventPool, Chance] pairs. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RoadEventPool entry, must be list of [RoadEventPool, Chance] pairs. File: " + fileName);
							flag = false;
						}
						break;
					case "RemoveEventPool":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveEventPool", fileName, out var values22))
						{
							foreach (string rEvent in values22)
							{
								if (MapRuleLibraryClient.MRLYML.RoadEvents.SingleOrDefault((CRoadEvent re) => rEvent == re.ID) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {rEvent} in {fileName}");
									flag = false;
								}
							}
							removeEventPool = values22;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ScenarioLevelTable":
					{
						if (CardProcessingShared.GetScenarioLevelTable(entry, fileName, out var slt2))
						{
							slt = slt2;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ScenarioLevelTable Entry in file " + fileName);
						flag = false;
						break;
					}
					case "ItemMaxStock":
					{
						if (YMLShared.GetMapping(entry, fileName, out var mapping4))
						{
							foreach (MappingEntry entry8 in mapping4.Entries)
							{
								if (YMLShared.GetStringPropertyValue(entry8.Key, "ItemMaxStock", fileName, out var rarityString))
								{
									CItem.EItemRarity rarity = CItem.ItemRarities.SingleOrDefault((CItem.EItemRarity s) => s.ToString() == rarityString);
									if (rarity != CItem.EItemRarity.None && !list8.Any((Tuple<CItem.EItemRarity, int> a) => a.Item1 == rarity) && YMLShared.GetIntPropertyValue(entry8.Value, "ItemMaxStock", fileName, out var value15))
									{
										list8.Add(new Tuple<CItem.EItemRarity, int>(rarity, value15));
										continue;
									}
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ItemMaxStock Entry in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ChapterDifficulty":
					{
						if (YMLShared.GetMapping(entry, fileName, out var mapping2))
						{
							int num2 = int.MaxValue;
							List<CHeadquarters.SubChapterDifficulty> list10 = new List<CHeadquarters.SubChapterDifficulty>();
							foreach (MappingEntry entry9 in mapping2.Entries)
							{
								string text2 = entry9.Key.ToString();
								int value11;
								if (!(text2 == "Chapter"))
								{
									if (!(text2 == "SubChapterDifficulties"))
									{
										continue;
									}
									if (YMLShared.GetMapping(entry9, fileName, out var mapping3))
									{
										foreach (MappingEntry entry10 in mapping3.Entries)
										{
											if (YMLShared.ParseIntValue(entry10.Key.ToString(), "SubChapter", fileName, out var value4))
											{
												if (entry10.Value is Sequence sequence7 && sequence7.Entries.Count == 6)
												{
													if (YMLShared.GetFloatPropertyValue(sequence7.Entries[0], "Friendly", fileName, out var value5) && YMLShared.GetFloatPropertyValue(sequence7.Entries[1], "Easy", fileName, out var value6) && YMLShared.GetFloatPropertyValue(sequence7.Entries[2], "Normal", fileName, out var value7) && YMLShared.GetFloatPropertyValue(sequence7.Entries[3], "Hard", fileName, out var value8) && YMLShared.GetFloatPropertyValue(sequence7.Entries[4], "Insane", fileName, out var value9) && YMLShared.GetFloatPropertyValue(sequence7.Entries[5], "Deadly", fileName, out var value10))
													{
														list10.Add(new CHeadquarters.SubChapterDifficulty(value4, value5, value6, value7, value8, value9, value10));
													}
													else
													{
														flag = false;
													}
												}
												else
												{
													SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry value " + entry.Value.ToString() + " in SubChapterDifficulties in Headquarters file, must be a sequence/list of 6 floats as multipliers for difficulty of Friendly, Easy, Normal, Hard, Brutal and Deadly respectively: " + fileName);
													flag = false;
												}
											}
											else
											{
												SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry key " + entry.Key.ToString() + " in SubChapterDifficulties in Headquarters file, must be int: " + fileName);
												flag = false;
											}
										}
									}
									else
									{
										flag = false;
									}
								}
								else if (YMLShared.GetIntPropertyValue(entry9.Value, "Chapter", fileName, out value11))
								{
									num2 = value11;
								}
								else
								{
									flag = false;
								}
							}
							if (num2 == int.MaxValue)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Chapter property in ChapterDifficulty section in Headquarters file: " + fileName);
								flag = false;
							}
							if (list10.Count == 0)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "No valid SubChapterDifficulties found for Chapter " + num2 + " in Headquarters file: " + fileName);
								flag = false;
							}
							if (flag)
							{
								list9.Add(new CHeadquarters.ChapterDifficulty(num2, list10));
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterXPTable":
						if (YMLShared.GetIntList(entry.Value, "CharacterXPTable", fileName, out values))
						{
							if (values.Count() == 9)
							{
								values.Insert(0, 0);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterXP table. Only " + values.Count() + " entries, requires 9. File: " + fileName);
							flag = false;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterXP table. File: " + fileName);
							flag = false;
						}
						break;
					case "ProsperityTable":
						if (YMLShared.GetIntList(entry.Value, "ProsperityTable", fileName, out values2))
						{
							if (values2.Count() == 8)
							{
								values2.Insert(0, 0);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Prosperity table. Only " + values2.Count() + " entries, requires 8. File: " + fileName);
							flag = false;
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Prosperity table. File: " + fileName);
							flag = false;
						}
						break;
					case "ReputationTable":
						if (YMLShared.GetIntList(entry.Value, "ReputationTable", fileName, out values3))
						{
							if (values3.Count() != 11)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Reputation table. Only " + values2.Count() + " entries, requires 11. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Reputation table. File: " + fileName);
							flag = false;
						}
						break;
					case "WorldMapLabelLocations":
						list5 = new List<Tuple<string, TileIndex>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence = entry.Value as Sequence;
							if (sequence.Entries[0] is Sequence)
							{
								foreach (DataItem entry11 in sequence.Entries)
								{
									if (YMLShared.GetTupleStringTileIndex(entry11, "WorldMapLabelLocations", fileName, out var tuple))
									{
										Tuple<string, TileIndex> item = new Tuple<string, TileIndex>(tuple.Item1, new TileIndex(tuple.Item2[0], tuple.Item2[1]));
										list5.Add(item);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringTileIndex(sequence, "WorldMapLabelLocations", fileName, out var tuple2))
								{
									Tuple<string, TileIndex> item2 = new Tuple<string, TileIndex>(tuple2.Item1, new TileIndex(tuple2.Item2[0], tuple2.Item2[1]));
									list5.Add(item2);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid WorldMapLabelLocations entry, must be list of [Label, X, Y] tuples. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid StreetLocations entry, must be list of [StreetName, X, Y] tuples. File: " + fileName);
							flag = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in Headquarters file: " + fileName);
						flag = false;
						break;
					}
				}
				if (text == string.Empty)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified for Headquarters in file: " + fileName);
					flag = false;
				}
				if (flag)
				{
					if (Headquarters == null)
					{
						Headquarters = new CHeadquarters(text, localisedName, localisedDescription, mesh, mapLocation, list, list3, unlockCondition, fileName, gameMode, questNames, tutorialQuestNames, tutorialMessages, startingPerksAmount, retirementTreasureTableNames, characterUnlockAlternateTreasureTableNames, startupTreasureTableNames, createCharacterGoldPerLevelAmount, createCharacterTreasureTableNames, list6, eventChance, list7, slt, list8, list9, values, values2, values3, list5);
					}
					else
					{
						Headquarters.UpdateData(localisedName, localisedDescription, mesh, mapLocation, list, list2, list3, list4, unlockCondition, gameMode, questNames, removeQuestNames, tutorialQuestNames, removeTutorialQuestNames, tutorialMessages, removeTutorialMessages, startingPerksAmount, retirementTreasureTableNames, removeRetirementTreasureTableNames, characterUnlockAlternateTreasureTableNames, removeCharacterUnlockAlternateTreasureTableNames, startupTreasureTableNames, removeStartupTreasureTableNames, createCharacterGoldPerLevelAmount, createCharacterTreasureTableNames, removeCreateCharacterTreasureTableNames, list6, eventChance, list7, removeEventPool, slt, list8, list9, values, values2, values3, list5);
					}
				}
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
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
}
