using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Events;

public class CRoadEventOption
{
	public enum ERoadEventUnavailableOptionVisibility
	{
		NA,
		Grey,
		Hide
	}

	public enum ERoadEventOptionCondition
	{
		NA,
		InParty,
		NotInParty,
		MinGold,
		MaxGold,
		HaveItem,
		HaveFlagSet,
		HaveItemSlotEquipped
	}

	public static readonly ERoadEventUnavailableOptionVisibility[] UnavailableOptionVisibilities = (ERoadEventUnavailableOptionVisibility[])Enum.GetValues(typeof(ERoadEventUnavailableOptionVisibility));

	public static readonly ERoadEventOptionCondition[] OptionConditions = (ERoadEventOptionCondition[])Enum.GetValues(typeof(ERoadEventOptionCondition));

	public string Name { get; private set; }

	public string Text { get; private set; }

	public string Subtext { get; private set; }

	public string Next { get; private set; }

	public List<TreasureTable> TreasureTables { get; private set; }

	public Dictionary<ERoadEventOptionCondition, List<string>> Conditions { get; private set; }

	public ERoadEventUnavailableOptionVisibility IfUnavailable { get; private set; }

	public string UnavailableText { get; private set; }

	public int RollForEach { get; private set; }

	public CRoadEventOption(string name, string text, string subtext, string next, List<TreasureTable> treasureTables, Dictionary<ERoadEventOptionCondition, List<string>> conditions, ERoadEventUnavailableOptionVisibility ifUnavailable, string unavailableText, int rollForEach)
	{
		Name = name;
		Text = text;
		Subtext = subtext;
		Next = next;
		TreasureTables = treasureTables;
		Conditions = conditions;
		IfUnavailable = ifUnavailable;
		UnavailableText = unavailableText;
		RollForEach = rollForEach;
	}

	public List<CItem> GetRoadEventOptionItems()
	{
		List<CItem> list = new List<CItem>();
		foreach (TreasureTable treasureTable in TreasureTables)
		{
			list.AddRange(treasureTable.GetTreasureTableItems());
		}
		return list;
	}

	public static bool ProcessOptionEntry(MappingEntry optionEntry, string screen, string fileName, out CRoadEventOption roadEventOption)
	{
		roadEventOption = null;
		bool flag = true;
		if (YMLShared.GetMapping(optionEntry, fileName, out var mapping))
		{
			string text = null;
			string subtext = null;
			string text2 = null;
			List<TreasureTable> list = new List<TreasureTable>();
			Dictionary<ERoadEventOptionCondition, List<string>> dictionary = null;
			ERoadEventUnavailableOptionVisibility ifUnavailable = ERoadEventUnavailableOptionVisibility.Grey;
			string unavailableText = string.Empty;
			int rollForEach = 0;
			foreach (MappingEntry optionPropertyEntry in mapping.Entries)
			{
				if (optionPropertyEntry == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null entry in Road Event Option. File:\n" + fileName);
					flag = false;
					continue;
				}
				switch (optionPropertyEntry.Key.ToString())
				{
				case "Text":
				{
					if (YMLShared.GetStringPropertyValue(optionPropertyEntry.Value, screen + "/Options/" + optionPropertyEntry.Key.ToString(), fileName, out var value3))
					{
						text = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Subtext":
				{
					if (YMLShared.GetStringPropertyValue(optionPropertyEntry.Value, screen + "/Options/" + optionPropertyEntry.Key.ToString(), fileName, out var value2))
					{
						subtext = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Next":
				{
					if (YMLShared.GetStringPropertyValue(optionPropertyEntry.Value, screen + "/Options/" + optionPropertyEntry.Key.ToString(), fileName, out var value5))
					{
						text2 = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "TreasureTable":
				case "TreasureTables":
				{
					Sequence sequence;
					if (optionPropertyEntry.Value is Scalar)
					{
						if (YMLShared.GetStringPropertyValue(optionPropertyEntry.Value, "TreasureTables", fileName, out var tableName))
						{
							TreasureTable treasureTable = ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable x) => x.Name == tableName);
							if (treasureTable != null)
							{
								list.Add(treasureTable);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find treasure table with name " + tableName + ". File: " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
					}
					else if (YMLShared.GetSequence(optionPropertyEntry.Value, "TreasureTables", fileName, out sequence))
					{
						foreach (DataItem entry in sequence.Entries)
						{
							if (YMLShared.GetStringPropertyValue(entry, "TreasureTables", fileName, out var tableName2))
							{
								TreasureTable treasureTable2 = ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable x) => x.Name == tableName2);
								if (treasureTable2 != null)
								{
									list.Add(treasureTable2);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find treasure table with name " + tableName2 + ". File: " + fileName);
								flag = false;
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
				case "OptionConditions":
				{
					if (YMLShared.GetMapping(optionPropertyEntry, fileName, out var mapping2))
					{
						foreach (MappingEntry conditionEntry in mapping2.Entries)
						{
							if (conditionEntry == null)
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null entry in OptionConditions. File: " + fileName);
								flag = false;
								continue;
							}
							ERoadEventOptionCondition eRoadEventOptionCondition = OptionConditions.SingleOrDefault((ERoadEventOptionCondition x) => x.ToString() == conditionEntry.Key.ToString());
							if (eRoadEventOptionCondition != ERoadEventOptionCondition.NA)
							{
								if (dictionary == null)
								{
									dictionary = new Dictionary<ERoadEventOptionCondition, List<string>>();
								}
								YMLShared.GetStringList(conditionEntry.Value, screen + "/Options/" + optionEntry.Key?.ToString() + "/" + optionPropertyEntry.Key?.ToString() + "/" + conditionEntry.Key.ToString(), fileName, out var values);
								dictionary.Add(eRoadEventOptionCondition, values);
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + conditionEntry.Key?.ToString() + " in " + screen + "/Options/" + optionEntry.Key?.ToString() + "/" + optionPropertyEntry.Key?.ToString() + ". File:\n" + fileName);
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
				case "IfUnavailable":
				{
					ERoadEventUnavailableOptionVisibility eRoadEventUnavailableOptionVisibility = UnavailableOptionVisibilities.SingleOrDefault((ERoadEventUnavailableOptionVisibility x) => x.ToString() == optionPropertyEntry.Value.ToString());
					if (eRoadEventUnavailableOptionVisibility != ERoadEventUnavailableOptionVisibility.NA)
					{
						ifUnavailable = eRoadEventUnavailableOptionVisibility;
						break;
					}
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + optionPropertyEntry.Value?.ToString() + " in " + screen + "/Options/" + optionEntry.Key?.ToString() + "/" + optionPropertyEntry.Key?.ToString() + ". File:\n" + fileName);
					flag = false;
					break;
				}
				case "UnavailableText":
				{
					if (YMLShared.GetStringPropertyValue(optionPropertyEntry.Value, screen + "/Options/" + optionPropertyEntry.Key.ToString(), fileName, out var value4))
					{
						unavailableText = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RollForEach":
				{
					if (YMLShared.GetIntPropertyValue(optionPropertyEntry.Value, screen + "/Options/" + optionPropertyEntry.Key.ToString(), fileName, out var value))
					{
						rollForEach = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + optionPropertyEntry.Key?.ToString() + " in " + screen + "/Options/" + optionEntry.Key?.ToString() + "/" + optionPropertyEntry.Key?.ToString() + ". File:\n" + fileName);
					flag = false;
					break;
				}
			}
			if (text == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Text entry in " + screen + "/Options/" + optionEntry.Key?.ToString() + ". File:\n" + fileName);
				flag = false;
			}
			if (text2 == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Next entry in " + screen + "/Options/" + optionEntry.Key?.ToString() + ". File:\n" + fileName);
				flag = false;
			}
			if (flag)
			{
				roadEventOption = new CRoadEventOption(optionEntry.Key.ToString(), text, subtext, text2, list, dictionary, ifUnavailable, unavailableText, rollForEach);
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}
}
