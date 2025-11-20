using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.PersonalQuests;

public class PersonalQuestYML
{
	public const int MinimumFilesRequired = 1;

	public List<PersonalQuestYMLData> LoadedYML { get; private set; }

	public PersonalQuestYML()
	{
		LoadedYML = new List<PersonalQuestYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			PersonalQuestYMLData personalQuestData = new PersonalQuestYMLData(fileName);
			personalQuestData.IsPersonalQuestStep = false;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				flag = Parse((yamlStream.Documents[0].Root as Mapping).Entries, fileName, ref personalQuestData);
				if (personalQuestData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					PersonalQuestYMLData personalQuestYMLData = LoadedYML.SingleOrDefault((PersonalQuestYMLData s) => s.ID == personalQuestData.ID);
					if (personalQuestYMLData == null)
					{
						LoadedYML.Add(personalQuestData);
					}
					else
					{
						personalQuestYMLData.UpdateData(personalQuestData);
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

	public bool Parse(List<MappingEntry> mappingEntries, string fileName, ref PersonalQuestYMLData personalQuestData)
	{
		bool flag = true;
		foreach (MappingEntry mappingEntry in mappingEntries)
		{
			switch (mappingEntry.Key.ToString())
			{
			case "ID":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "ID", fileName, out var value5))
				{
					personalQuestData.ID = value5;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "LocalisedName":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "LocalisedName", fileName, out var value2))
				{
					personalQuestData.LocalisedName = value2;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "LocalisedDescription":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "LocalisedDescription", fileName, out var value4))
				{
					personalQuestData.LocalisedDescription = value4;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "AudioIdCompletedStory":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "AudioIdCompletedStory", fileName, out var value7))
				{
					personalQuestData.AudioIdCompletedStory = value7;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "AudioIdProgressFirstStepStory":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "AudioIdProgressFirstStepStory", fileName, out var value3))
				{
					personalQuestData.AudioIdProgressFirstStepStory = value3;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "LocalisedObjectiveDescription":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "LocalisedObjectiveDescription", fileName, out var value6))
				{
					personalQuestData.LocalisedObjectiveDescription = value6;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "PersonalQuestCondition":
			{
				if (CUnlockCondition.Parse(mappingEntry, fileName, out var unlockCondition))
				{
					personalQuestData.PersonalQuestCondition = unlockCondition;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Discard":
			{
				if (YMLShared.GetBoolPropertyValue(mappingEntry.Value, "Discard", fileName, out var value8))
				{
					personalQuestData.Discard = value8;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "QuestSteps":
			{
				if (!YMLShared.GetSequence(mappingEntry, fileName, out var sequence))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected QuestSteps entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
					flag = false;
					break;
				}
				foreach (DataItem entry in sequence.Entries)
				{
					if (!(entry is Mapping))
					{
						continue;
					}
					Mapping mapping = entry as Mapping;
					if (!YMLShared.GetMapping(mapping.Entries[0], fileName, out mapping))
					{
						continue;
					}
					PersonalQuestYMLData personalQuestData2 = new PersonalQuestYMLData(fileName);
					flag = Parse(mapping.Entries, fileName, ref personalQuestData2);
					if (flag)
					{
						if (personalQuestData.PersonalQuestSteps == null)
						{
							personalQuestData.PersonalQuestSteps = new List<PersonalQuestYMLData>();
						}
						personalQuestData2.IsPersonalQuestStep = true;
						personalQuestData2.LocalisedDescription = personalQuestData.LocalisedDescription;
						personalQuestData2.ID = personalQuestData.ID;
						personalQuestData2.LocalisedName = personalQuestData.LocalisedName;
						personalQuestData.PersonalQuestSteps.Add(personalQuestData2);
						personalQuestData2.AudioIdCompletedStory = personalQuestData.AudioIdCompletedStory;
						personalQuestData2.AudioIdProgressFirstStepStory = personalQuestData.AudioIdProgressFirstStepStory;
					}
				}
				break;
			}
			case "TreasureTables":
			{
				if (YMLShared.GetStringList(mappingEntry.Value, "TreasureTables", fileName, out var values))
				{
					foreach (string table in values)
					{
						if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => table == t.Name) == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {table} in {fileName}");
							flag = false;
						}
					}
					personalQuestData.TreasureTables = values;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "LocalisedObjectiveTitle":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "LocalisedObjectiveTitle", fileName, out var value))
				{
					personalQuestData.LocalisedObjectiveTitle = value;
				}
				else
				{
					flag = false;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + mappingEntry.Key.ToString() + " in root of PersonalQuest file: " + fileName);
				flag = false;
				break;
			case "Parser":
				break;
			}
		}
		return flag;
	}
}
