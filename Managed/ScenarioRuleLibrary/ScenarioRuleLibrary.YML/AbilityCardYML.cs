using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AbilityCardYML
{
	public const int LongRestID = -1;

	public const int MinimumFilesRequired = 1;

	public List<AbilityCardYMLData> LoadedYML { get; private set; }

	public AbilityCardYML()
	{
		LoadedYML = new List<AbilityCardYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AbilityCardYMLData cardData = new AbilityCardYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value5))
						{
							cardData.Name = CardProcessingShared.GetLookupValue(value5);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Character":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Character", fileName, out var characterID))
						{
							if (characterID == "All")
							{
								cardData.CharacterID = characterID;
								break;
							}
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == characterID) != null)
							{
								cardData.CharacterID = characterID;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {characterID} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ID":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ID", fileName, out var value))
						{
							cardData.ID = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Level":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Level", fileName, out var value2))
						{
							cardData.Level = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "SupplyCard":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "SupplyCard", fileName, out var value4))
						{
							cardData.SupplyCard = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Initiative":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "Initiative", fileName, out var value3))
						{
							cardData.Initiative = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Discard":
					{
						if (YMLShared.GetSequence(entry.Value, "Discard", fileName, out var sequence))
						{
							string text = (sequence.Entries[0] as Scalar).Text;
							string text2 = (sequence.Entries[1] as Scalar).Text;
							switch (text)
							{
							case "Discard":
								cardData.TopDiscardType = DiscardType.Discard;
								break;
							case "Lost":
								cardData.TopDiscardType = DiscardType.Lost;
								break;
							case "Permanently Lost":
								cardData.TopDiscardType = DiscardType.PermanentlyLost;
								break;
							default:
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse Top Discard property in file " + fileName);
								break;
							}
							switch (text2)
							{
							case "Discard":
								cardData.BottomDiscardType = DiscardType.Discard;
								break;
							case "Lost":
								cardData.BottomDiscardType = DiscardType.Lost;
								break;
							case "Permanently Lost":
								cardData.BottomDiscardType = DiscardType.PermanentlyLost;
								break;
							default:
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse Bottom Discard property in file " + fileName);
								break;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Data":
					{
						if (YMLShared.GetMapping(entry, fileName, out var mapping2))
						{
							MappingEntry mappingEntry = mapping2.Entries.SingleOrDefault((MappingEntry x) => x.Key.ToString() == "Top");
							if (mappingEntry != null)
							{
								if (ProcessActionData(mappingEntry, cardData.ID, cardData.TopDiscardType, fileName, out var action, out var consumes))
								{
									cardData.TopActionCardData = action;
									cardData.TopConsumes.AddRange(consumes);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Top Action Data in file " + fileName);
								flag = false;
							}
							MappingEntry mappingEntry2 = mapping2.Entries.SingleOrDefault((MappingEntry x) => x.Key.ToString() == "Bottom");
							if (mappingEntry2 != null)
							{
								if (ProcessActionData(mappingEntry2, cardData.ID, cardData.BottomDiscardType, fileName, out var action2, out var consumes2))
								{
									cardData.BottomActionCardData = action2;
									cardData.BottomConsumes.AddRange(consumes2);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Top Bottom Data in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Layout":
					{
						Mapping mapping;
						if (cardData.ID == -1 && entry.Value is Sequence)
						{
							cardData.TopActionFullLayout = new CardLayout(entry.Value as Sequence, null, null, cardData.Name, DiscardType.None, fileName);
						}
						else if (YMLShared.GetMapping(entry, fileName, out mapping))
						{
							cardData.TopActionFullLayout = CardProcessingShared.ProcessAbilityLayout(entry, "Top", cardData.TopActionCardData?.Abilities, cardData.Name, cardData.TopDiscardType, fileName, cardData.TopConsumes);
							cardData.BottomActionFullLayout = CardProcessingShared.ProcessAbilityLayout(entry, "Bottom", cardData.BottomActionCardData?.Abilities, cardData.Name, cardData.BottomDiscardType, fileName, cardData.BottomConsumes);
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (cardData.ID == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					AbilityCardYMLData abilityCardYMLData = LoadedYML.SingleOrDefault((AbilityCardYMLData s) => s.ID == cardData.ID);
					if (abilityCardYMLData == null)
					{
						LoadedYML.Add(cardData);
					}
					else
					{
						abilityCardYMLData.UpdateData(cardData);
					}
				}
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	private bool ProcessActionData(MappingEntry mappingEntry, int cardID, DiscardType discardType, string fileName, out CAction action, out List<AbilityConsume> consumes)
	{
		consumes = null;
		action = null;
		if (mappingEntry.Value is Mapping)
		{
			if (AbilityData.GetAction(mappingEntry.Value as Mapping, cardID, fileName, discardType, out var action2, out consumes))
			{
				action = action2;
				return true;
			}
			return false;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Child of Data entry is not a mapping. File:\n" + fileName + "\nDataItem:\n" + mappingEntry.Value.ToString());
		return false;
	}
}
