using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Achievements;

public class AchievementYML
{
	public const int MinimumFilesRequired = 1;

	public List<AchievementYMLData> LoadedYML { get; private set; }

	public AchievementYML()
	{
		LoadedYML = new List<AchievementYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AchievementYMLData achievementData = new AchievementYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			achievementData.UnlockCondition = new CUnlockCondition();
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value2))
						{
							achievementData.ID = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedName":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocalisedName", fileName, out var value4))
						{
							achievementData.LocalisedName = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedDescription":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocalisedDescription", fileName, out var value3))
						{
							achievementData.LocalisedDescription = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UnlockCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition))
						{
							achievementData.UnlockCondition = unlockCondition;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AchievementCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition2))
						{
							achievementData.AchievementCondition = unlockCondition2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TreasureTables":
					{
						if (YMLShared.GetStringList(entry.Value, "TreasureTables", fileName, out var values))
						{
							foreach (string table in values)
							{
								if (ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable t) => t.Name == table) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {table} in {fileName}");
									flag = false;
								}
							}
							achievementData.TreasureTables = values;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AchievementType":
						achievementData.AchievementType = AchievementYMLData.AchievementTypes.SingleOrDefault((EAchievementType it) => it.ToString() == entry.Value.ToString());
						if (achievementData.AchievementType == EAchievementType.None)
						{
							flag = false;
						}
						break;
					case "AchievementLevel":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "AchievementLevel", fileName, out var value5))
						{
							achievementData.AchievementLevel = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CompleteDialogueLines":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence))
						{
							foreach (DataItem entry2 in sequence.Entries)
							{
								if (entry2 is Mapping)
								{
									if (MapMessagesYML.ParseDialogueLine((entry2 as Mapping).Entries, fileName, out var mapDialogueLine))
									{
										if (achievementData.CompleteDialogueLines == null)
										{
											achievementData.CompleteDialogueLines = new List<MapDialogueLine>();
										}
										achievementData.CompleteDialogueLines.Add(mapDialogueLine);
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
					case "AchievementOrderId":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AchievementOrderId", fileName, out var value))
						{
							achievementData.AchievementOrderId = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of Achievement file: " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (achievementData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					AchievementYMLData achievementYMLData = LoadedYML.SingleOrDefault((AchievementYMLData s) => s.ID == achievementData.ID);
					if (achievementYMLData == null)
					{
						LoadedYML.Add(achievementData);
					}
					else
					{
						achievementYMLData.UpdateData(achievementData);
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
