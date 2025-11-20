using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Locations;

public class TempleYML
{
	public class TempleBlessingDefinition
	{
		public RewardCondition TempleBlessingCondition { get; private set; }

		public int Quantity { get; private set; }

		public int GoldCost { get; private set; }

		public TempleBlessingDefinition(RewardCondition blessingCondition, int quantity, int goldCost)
		{
			TempleBlessingCondition = blessingCondition;
			Quantity = quantity;
			GoldCost = goldCost;
		}
	}

	public const int MinimumFilesRequired = 1;

	public List<CTemple> LoadedYML { get; private set; }

	public TempleYML()
	{
		LoadedYML = new List<CTemple>();
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
			List<TempleBlessingDefinition> list = new List<TempleBlessingDefinition>();
			List<Tuple<int, string>> list2 = new List<Tuple<int, string>>();
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value))
						{
							id = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TempleBlessings":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence2))
						{
							foreach (DataItem entry2 in sequence2.Entries)
							{
								if (entry2 is Mapping)
								{
									TempleBlessingDefinition templeBlessingDefinition = ParseTempleBlessingDefinition((entry2 as Mapping).Entries, fileName);
									if (templeBlessingDefinition != null)
									{
										list.Add(templeBlessingDefinition);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected TempleBlessings entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected TempleBlessings entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "DonationTable":
						list2 = new List<Tuple<int, string>>();
						if (entry.Value is Sequence)
						{
							Sequence sequence = entry.Value as Sequence;
							if (sequence.Entries[0] is Sequence)
							{
								foreach (DataItem entry3 in sequence.Entries)
								{
									if (YMLShared.GetTupleIntString(entry3, "DonationTable", fileName, out var tuple))
									{
										list2.Add(tuple);
									}
									else
									{
										flag = false;
									}
								}
							}
							else if (sequence.Entries.Count == 2)
							{
								if (YMLShared.GetTupleIntString(sequence, "DonationTable", fileName, out var tuple2))
								{
									list2.Add(tuple2);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid DonationTable entry, must be list of [Donation Amount, Treasure Table] pairs. File: " + fileName);
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid DonationTable entry, must be list of [Donation Amount, Treasure Table] pairs. File: " + fileName);
							flag = false;
						}
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
					CTemple cTemple = LoadedYML.SingleOrDefault((CTemple s) => s.ID == id);
					if (cTemple == null)
					{
						LoadedYML.Add(new CTemple(id, list, list2, fileName));
					}
					else
					{
						cTemple.UpdateData(list, list2);
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

	public static TempleBlessingDefinition ParseTempleBlessingDefinition(List<MappingEntry> entries, string fileName)
	{
		bool flag = true;
		string conditionName = string.Empty;
		CCondition.EPositiveCondition ePositiveCondition = CCondition.EPositiveCondition.NA;
		RewardCondition.EConditionMapDuration mapDuration = RewardCondition.EConditionMapDuration.NextScenario;
		int roundDuration = 1;
		int quantity = 1;
		int goldCost = 0;
		foreach (MappingEntry entry in entries)
		{
			switch (entry.Key.ToString())
			{
			case "Condition":
				if (YMLShared.GetStringPropertyValue(entry.Value, "Condition", fileName, out conditionName))
				{
					ePositiveCondition = CCondition.PositiveConditions.SingleOrDefault((CCondition.EPositiveCondition x) => x.ToString() == conditionName);
				}
				break;
			case "Duration":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "Duration", fileName, out var conditionDurationValue))
				{
					RewardCondition.EConditionMapDuration eConditionMapDuration = RewardCondition.ConditionDurations.SingleOrDefault((RewardCondition.EConditionMapDuration x) => x.ToString() == conditionDurationValue);
					if (eConditionMapDuration == RewardCondition.EConditionMapDuration.None)
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find condition duration " + conditionDurationValue + ". File: " + fileName);
						return null;
					}
					mapDuration = eConditionMapDuration;
				}
				break;
			}
			case "RoundDuration":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "RoundDuration", fileName, out var value3))
				{
					roundDuration = value3;
				}
				break;
			}
			case "Quantity":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "Quantity", fileName, out var value2))
				{
					quantity = value2;
				}
				break;
			}
			case "GoldCost":
			{
				if (YMLShared.GetIntPropertyValue(entry.Value, "GoldCost", fileName, out var value))
				{
					goldCost = value;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of MapMessage file " + fileName);
				flag = false;
				break;
			}
		}
		if (ePositiveCondition == CCondition.EPositiveCondition.NA)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "TempleBlessingDefinition condition is still NA, set to a valid positive condition value. File: " + fileName);
			flag = false;
		}
		if (flag)
		{
			return new TempleBlessingDefinition(new RewardCondition(mapDuration, ePositiveCondition, roundDuration), quantity, goldCost);
		}
		return null;
	}
}
