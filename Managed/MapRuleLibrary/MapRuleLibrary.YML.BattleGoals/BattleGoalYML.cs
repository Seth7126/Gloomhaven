using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.BattleGoals;

public class BattleGoalYML
{
	public const int MinimumFilesRequired = 1;

	public List<BattleGoalYMLData> LoadedYML { get; private set; }

	public BattleGoalYML()
	{
		LoadedYML = new List<BattleGoalYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			BattleGoalYMLData battleGoalData = new BattleGoalYMLData(fileName);
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
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value5))
						{
							battleGoalData.ID = value5;
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
							battleGoalData.LocalisedName = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedDescription":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocalisedDescription", fileName, out var value6))
						{
							battleGoalData.LocalisedDescription = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PerkPoints":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "PerkPoints", fileName, out var value2))
						{
							battleGoalData.PerkPoints = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BattleGoalCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition2))
						{
							battleGoalData.BattleGoalCondition = unlockCondition2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "BattleGoalFailCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition))
						{
							battleGoalData.BattleGoalFailCondition = unlockCondition;
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
							battleGoalData.TreasureTables = values;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CheckAtEndOfScenario":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "CheckAtEndOfScenario", fileName, out var value3))
						{
							battleGoalData.CheckAtEndOfScenario = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CampaignOnly":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "CampaignOnly", fileName, out var value))
						{
							battleGoalData.CampaignOnly = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of BattleGoal file: " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (battleGoalData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					BattleGoalYMLData battleGoalYMLData = LoadedYML.SingleOrDefault((BattleGoalYMLData s) => s.ID == battleGoalData.ID);
					if (battleGoalYMLData == null)
					{
						LoadedYML.Add(battleGoalData);
					}
					else
					{
						battleGoalYMLData.UpdateData(battleGoalData);
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
