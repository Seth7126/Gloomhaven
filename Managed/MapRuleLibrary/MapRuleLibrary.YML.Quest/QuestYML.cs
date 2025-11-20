using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Shared;
using MapRuleLibrary.YML.VisibilitySpheres;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Quest;

public class QuestYML
{
	public class CQuestCharacterRequirement
	{
		public string RequiredCharacterID;

		public int? RequiredLevel;

		public string RequiredItemID;

		public string RequiredPersonalQuestID;

		public int? RequiredCharacterCount;

		public CQuestCharacterRequirement(string requiredCharacterID, int? requiredLevel, string requiredItemID, string requiredPersonalQuestID, int? requiredCharacterCount)
		{
			RequiredCharacterID = requiredCharacterID;
			RequiredLevel = requiredLevel;
			RequiredItemID = requiredItemID;
			RequiredPersonalQuestID = requiredPersonalQuestID;
			RequiredCharacterCount = requiredCharacterCount;
		}

		public static bool Parse(List<MappingEntry> entries, string fileName, out CQuestCharacterRequirement questCharacterRequirement)
		{
			questCharacterRequirement = null;
			bool flag = true;
			if (entries != null)
			{
				string requiredCharacterID = null;
				int? requiredLevel = 0;
				string requiredItemID = null;
				string requiredPersonalQuestID = null;
				int? requiredCharacterCount = null;
				foreach (MappingEntry entry in entries)
				{
					switch (entry.Key.ToString())
					{
					case "Character":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Character", fileName, out var characterValue))
						{
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData ch) => ch.ID == characterValue) != null)
							{
								requiredCharacterID = characterValue;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {characterValue} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Level":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Level", fileName, out var value2))
						{
							requiredLevel = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Item":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Item", fileName, out var itemValue))
						{
							if (ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData it) => it.StringID == itemValue) != null)
							{
								requiredItemID = itemValue;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {itemValue} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PersonalQuest":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "PersonalQuest", fileName, out var value3))
						{
							requiredPersonalQuestID = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterCount":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "CharacterCount", fileName, out var value))
						{
							requiredCharacterCount = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected QuestCharacterRequirement entry " + entry.Key.ToString() + " in file: " + fileName);
						flag = false;
						break;
					}
				}
				if (flag)
				{
					questCharacterRequirement = new CQuestCharacterRequirement(requiredCharacterID, requiredLevel, requiredItemID, requiredPersonalQuestID, requiredCharacterCount);
				}
			}
			else
			{
				flag = false;
			}
			return flag;
		}
	}

	public const int MinimumFilesRequired = 1;

	public List<CQuest> LoadedYML { get; private set; }

	public QuestYML()
	{
		LoadedYML = new List<CQuest>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			string id = null;
			int chapter = int.MaxValue;
			EQuestType eQuestType = EQuestType.None;
			EQuestIconType eQuestIconType = EQuestIconType.None;
			ECharacter characterIcon = ECharacter.None;
			List<string> completionRewards = new List<string>();
			CUnlockCondition unlockCondition = null;
			CUnlockCondition blockedCondition = null;
			List<CQuestCharacterRequirement> list = new List<CQuestCharacterRequirement>();
			bool nullQuestCharacterRequirement = false;
			string startingVillage = string.Empty;
			string endingVillage = string.Empty;
			string linkedQuestID = string.Empty;
			List<int> eventChance = null;
			List<Tuple<string, int>> list2 = new List<Tuple<string, int>>();
			List<string> removeEventPool = null;
			CMapScenario mapScenario = null;
			List<MapDialogueLine> list3 = new List<MapDialogueLine>();
			List<MapDialogueLine> list4 = new List<MapDialogueLine>();
			List<VisibilitySphereYML.VisibilitySphereDefinition> list5 = new List<VisibilitySphereYML.VisibilitySphereDefinition>();
			List<VisibilitySphereYML.VisibilitySphereDefinition> list6 = new List<VisibilitySphereYML.VisibilitySphereDefinition>();
			string loadoutImageId = null;
			string loadoutAudioId = null;
			List<Tuple<string, string>> list7 = null;
			List<Tuple<string, string>> list8 = null;
			EQuestAreaType eQuestAreaType = EQuestAreaType.None;
			string localisedCustomTreasureRewardKey = null;
			bool hideTreasureWhenCompleted = false;
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value7))
						{
							id = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Chapter":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Chapter", fileName, out var value6))
						{
							chapter = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "QuestType":
						eQuestType = CQuest.QuestTypes.SingleOrDefault((EQuestType x) => x.ToString() == entry.Value.ToString());
						if (eQuestType == EQuestType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid QuestType: " + entry.Value?.ToString() + ". File:\n" + fileName);
							flag = false;
						}
						break;
					case "IconType":
						eQuestIconType = CQuest.QuestIconTypes.SingleOrDefault((EQuestIconType x) => x.ToString() == entry.Value.ToString());
						if (eQuestIconType == EQuestIconType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid QuestIconType: " + entry.Value?.ToString() + ". File:\n" + fileName);
							flag = false;
						}
						break;
					case "CharacterIcon":
					{
						CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == entry.Value.ToString());
						if (cCharacterClass != null)
						{
							characterIcon = cCharacterClass.CharacterModel;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CharacterIcon: " + entry.Value?.ToString() + ". File:\n" + fileName);
						flag = false;
						break;
					}
					case "UnlockDialogueLines":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence3))
						{
							foreach (DataItem entry2 in sequence3.Entries)
							{
								if (entry2 is Mapping)
								{
									if (MapMessagesYML.ParseDialogueLine((entry2 as Mapping).Entries, fileName, out var mapDialogueLine))
									{
										list3.Add(mapDialogueLine);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected UnlockDialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected UnlockDialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "CompleteDialogueLines":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence4))
						{
							foreach (DataItem entry3 in sequence4.Entries)
							{
								if (entry3 is Mapping)
								{
									if (MapMessagesYML.ParseDialogueLine((entry3 as Mapping).Entries, fileName, out var mapDialogueLine2))
									{
										list4.Add(mapDialogueLine2);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected CompleteDialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected CompleteDialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "UnlockRevealFoWSphere":
					case "CompleteRevealFoWSphere":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence))
						{
							foreach (DataItem entry4 in sequence.Entries)
							{
								if (entry4 is Mapping)
								{
									VisibilitySphereYML.VisibilitySphereDefinition visibilitySphereDefinition2;
									if (entry.Key.ToString() == "UnlockRevealFoWSphere")
									{
										if (VisibilitySphereYML.ParseSphereDefinition((entry4 as Mapping).Entries, fileName, out var visibilitySphereDefinition))
										{
											list5.Add(visibilitySphereDefinition);
										}
										else
										{
											flag = false;
										}
									}
									else if (VisibilitySphereYML.ParseSphereDefinition((entry4 as Mapping).Entries, fileName, out visibilitySphereDefinition2))
									{
										list6.Add(visibilitySphereDefinition2);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected Unlock/CompleteRevealFoWSphere entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected Unlock/CompleteRevealFoWSphere entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "CompletionRewards":
					{
						if (YMLShared.GetStringList(entry.Value, entry.Key.ToString(), fileName, out var values3))
						{
							foreach (string name in values3)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => name == t.Name) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {name} in {fileName}");
									flag = false;
								}
							}
							completionRewards = values3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UnlockCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition3))
						{
							unlockCondition = unlockCondition3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BlockCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition2))
						{
							blockedCondition = unlockCondition2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RequiredCharacters":
					{
						Sequence sequence6;
						if (entry.Value is Scalar scalar && scalar.Text.ToLower() == "null")
						{
							nullQuestCharacterRequirement = true;
						}
						else if (YMLShared.GetSequence(entry, fileName, out sequence6))
						{
							foreach (DataItem entry5 in sequence6.Entries)
							{
								if (entry5 is Mapping mapping2)
								{
									if (CQuestCharacterRequirement.Parse(mapping2.Entries, fileName, out var questCharacterRequirement))
									{
										list.Add(questCharacterRequirement);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected RequiredCharacters entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected RequiredCharacters entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "StartingVillage":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "StartingVillage", fileName, out var outStartingVillage))
						{
							if (MapRuleLibraryClient.MRLYML.Villages.SingleOrDefault((CVillage v) => v.ID == outStartingVillage) != null || outStartingVillage == "Demonsgate" || outStartingVillage == "Gloomhaven")
							{
								startingVillage = outStartingVillage;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outStartingVillage} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "EndingVillage":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "EndingVillage", fileName, out var outEndingVillage))
						{
							if (MapRuleLibraryClient.MRLYML.Villages.SingleOrDefault((CVillage v) => v.ID == outEndingVillage) != null || outEndingVillage == "Demonsgate" || outEndingVillage == "Gloomhaven")
							{
								endingVillage = outEndingVillage;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {outEndingVillage} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LinkedQuestID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LinkedQuestID", fileName, out var value5))
						{
							linkedQuestID = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "EventChance":
					{
						if (YMLShared.GetIntList(entry.Value, entry.Key.ToString(), fileName, out var values))
						{
							eventChance = values;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "EventPool":
						if (entry.Value is Sequence)
						{
							list2 = new List<Tuple<string, int>>();
							Sequence sequence7 = entry.Value as Sequence;
							if (sequence7.Entries[0] is Sequence)
							{
								foreach (DataItem entry6 in sequence7.Entries)
								{
									if (YMLShared.GetTupleStringInt(entry6, "EventPool", fileName, out var tuple5))
									{
										list2.Add(tuple5);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence7.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringInt(sequence7, "EventPool", fileName, out var tuple6))
								{
									list2.Add(tuple6);
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
					case "RemoveEventPool":
					{
						if (YMLShared.GetStringList(entry.Value, "RemoveEventPool", fileName, out var values2))
						{
							removeEventPool = values2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Scenarios":
					case "Scenario":
						if (id != null)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping))
							{
								if (MapYMLShared.ProcessMapScenario(mapping.Entries, fileName, id, 0, out var mapScenario2))
								{
									mapScenario = mapScenario2;
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
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "ID must be specified before Scenario. File " + fileName);
							flag = false;
						}
						break;
					case "LoadoutImageId":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LoadoutImageId", fileName, out var value4))
						{
							loadoutImageId = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LoadoutAudioId":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LoadoutAudioId", fileName, out var value3))
						{
							loadoutAudioId = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "NarrativeTextAudioOverride":
						if (entry.Value is Sequence)
						{
							list8 = new List<Tuple<string, string>>();
							Sequence sequence5 = entry.Value as Sequence;
							if (sequence5.Entries[0] is Sequence)
							{
								foreach (DataItem entry7 in sequence5.Entries)
								{
									if (YMLShared.GetTupleStringString(entry7, "NarrativeTextAudioOverride", fileName, out var tuple3))
									{
										list8.Add(tuple3);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence5.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringString(sequence5, "NarrativeTextAudioOverride", fileName, out var tuple4))
								{
									list8.Add(tuple4);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid NarrativeTextAudioOverride entry, must be list of [LocalisationKey, ImageName] pairs. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid NarrativeTextAudioOverride entry, must be list of [LocalisationKey, ImageName] pairs. File: " + fileName);
							flag = false;
						}
						break;
					case "NarrativeTextImageOverride":
						if (entry.Value is Sequence)
						{
							list7 = new List<Tuple<string, string>>();
							Sequence sequence2 = entry.Value as Sequence;
							if (sequence2.Entries[0] is Sequence)
							{
								foreach (DataItem entry8 in sequence2.Entries)
								{
									if (YMLShared.GetTupleStringString(entry8, "NarrativeTextImageOverride", fileName, out var tuple))
									{
										list7.Add(tuple);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence2.Entries.Count == 2)
							{
								if (YMLShared.GetTupleStringString(sequence2, "NarrativeTextImageOverride", fileName, out var tuple2))
								{
									list7.Add(tuple2);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid NarrativeTextImageOverride entry, must be list of [LocalisationKey, ImageName] pairs. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid NarrativeTextImageOverride entry, must be list of [LocalisationKey, ImageName] pairs. File: " + fileName);
							flag = false;
						}
						break;
					case "LocationArea":
						eQuestAreaType = CQuest.QuestAreaTypes.SingleOrDefault((EQuestAreaType x) => x.ToString() == entry.Value.ToString());
						if (eQuestAreaType == EQuestAreaType.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid LocationArea: " + entry.Value?.ToString() + ". File:\n" + fileName);
							flag = false;
						}
						break;
					case "LocalizedCustomTreasureRewardKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocalizedCustomTreasureRewardKey", fileName, out var value2))
						{
							localisedCustomTreasureRewardKey = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "HideTreasureWhenCompleted":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "HideTreasureWhenCompleted", fileName, out var value))
						{
							hideTreasureWhenCompleted = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of Quest file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (id == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					CQuest cQuest = LoadedYML.SingleOrDefault((CQuest s) => s.ID == id);
					if (cQuest == null)
					{
						LoadedYML.Add(new CQuest(id, chapter, eQuestType, eQuestIconType, characterIcon, completionRewards, unlockCondition, blockedCondition, list, mapScenario, startingVillage, endingVillage, linkedQuestID, eventChance, list2, list3, list4, list5, list6, loadoutImageId, loadoutAudioId, list7, list8, fileName, eQuestAreaType, localisedCustomTreasureRewardKey, hideTreasureWhenCompleted));
					}
					else
					{
						cQuest.UpdateData(chapter, eQuestType, eQuestIconType, characterIcon, completionRewards, unlockCondition, blockedCondition, list, nullQuestCharacterRequirement, mapScenario, startingVillage, endingVillage, linkedQuestID, eventChance, list2, removeEventPool, list3, list4, list5, list6, loadoutImageId, loadoutAudioId, list7, list8, eQuestAreaType, localisedCustomTreasureRewardKey, hideTreasureWhenCompleted);
					}
				}
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
			return false;
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}
}
