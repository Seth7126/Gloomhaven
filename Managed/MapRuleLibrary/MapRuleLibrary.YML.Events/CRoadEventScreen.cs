using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Events;

public class CRoadEventScreen
{
	public enum ActionAtEventEnd
	{
		None,
		Reuse,
		Discard,
		True
	}

	public string Name { get; private set; }

	public string Text { get; private set; }

	public string Header { get; private set; }

	public string Image { get; private set; }

	public List<CRoadEventOption> Options { get; private set; }

	public List<CRoadEventTreasureCondition> TreasureConditions { get; private set; }

	public ActionAtEventEnd End { get; private set; }

	public string Next { get; private set; }

	public List<TreasureTable> TreasureTables { get; private set; }

	public int Player { get; private set; }

	public CRoadEventScreen(string name, string text, string image, string header, List<CRoadEventOption> options, List<CRoadEventTreasureCondition> treasureConditions, ActionAtEventEnd end, string next, List<TreasureTable> treasureTables, int player)
	{
		Name = name;
		Text = text;
		Image = image;
		Options = options;
		TreasureConditions = treasureConditions;
		End = end;
		Next = next;
		TreasureTables = treasureTables;
		Header = header;
		Player = player;
	}

	public static bool ProcessScreensEntry(string screenName, MappingEntry screensEntry, string fileName, out CRoadEventScreen roadEventScreen)
	{
		roadEventScreen = null;
		bool flag = true;
		if (YMLShared.GetMapping(screensEntry, fileName, out var mapping))
		{
			string text = null;
			string image = null;
			string header = null;
			List<CRoadEventOption> list = new List<CRoadEventOption>();
			List<CRoadEventTreasureCondition> list2 = new List<CRoadEventTreasureCondition>();
			ActionAtEventEnd result = ActionAtEventEnd.None;
			string text2 = null;
			List<TreasureTable> list3 = new List<TreasureTable>();
			int player = 0;
			foreach (MappingEntry entry in mapping.Entries)
			{
				if (entry == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null entry in Road Event Screen. File:" + fileName);
					flag = false;
					continue;
				}
				switch (entry.Key.ToString())
				{
				case "Gained":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition5))
					{
						list2.Add(roadEventTreasureCondition5);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Lost":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition3))
					{
						list2.Add(roadEventTreasureCondition3);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "InParty":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition6))
					{
						list2.Add(roadEventTreasureCondition6);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Reputation":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition))
					{
						list2.Add(roadEventTreasureCondition);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RollResult":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition2))
					{
						list2.Add(roadEventTreasureCondition2);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "MinGold":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition7))
					{
						list2.Add(roadEventTreasureCondition7);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "HaveItem":
				{
					if (CRoadEventTreasureCondition.ProcessEventTreasureCondition(entry, fileName, out var roadEventTreasureCondition4))
					{
						list2.Add(roadEventTreasureCondition4);
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Text":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, screensEntry.Key.ToString() + "/" + entry.Key.ToString(), fileName, out var value))
					{
						text = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Header":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, screensEntry.Key.ToString() + "/" + entry.Key.ToString(), fileName, out var value5))
					{
						header = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Player":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, screensEntry.Key.ToString() + "/" + entry.Key.ToString(), fileName, out var value3))
					{
						player = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Image":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, screensEntry.Key.ToString() + "/" + entry.Key.ToString(), fileName, out var value2))
					{
						image = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Options":
				{
					if (YMLShared.GetMapping(entry, fileName, out var mapping2))
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							if (entry2 != null)
							{
								if (CRoadEventOption.ProcessOptionEntry(entry2, screensEntry.Key.ToString(), fileName, out var roadEventOption))
								{
									list.Add(roadEventOption);
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null entry in Options. File:" + fileName);
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
				case "Next":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, screensEntry.Key.ToString() + "/" + entry.Key.ToString(), fileName, out var value6))
					{
						text2 = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "End":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, screensEntry.Key.ToString() + "/" + entry.Key.ToString(), fileName, out var value4))
					{
						if (!Enum.TryParse<ActionAtEventEnd>(value4, out result))
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + value4 + " in End. File: " + fileName);
							flag = false;
						}
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
					if (YMLShared.GetStringList(entry.Value, "TreasureTables", fileName, out var values))
					{
						foreach (string tableName in values)
						{
							TreasureTable treasureTable = ScenarioRuleClient.SRLYML.TreasureTables.SingleOrDefault((TreasureTable x) => x.Name == tableName);
							if (treasureTable != null)
							{
								list3.Add(treasureTable);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find treasure table with name " + tableName + ". File: " + fileName);
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
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry.Key?.ToString() + " in screen " + screensEntry.Key?.ToString() + ". File:" + fileName);
					flag = false;
					break;
				}
			}
			if (text == null && list2.Count == 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Text entry in screen " + screensEntry.Key?.ToString() + ". File:\n" + fileName);
				flag = false;
			}
			if (list.Count == 0 && result == ActionAtEventEnd.None && list2.Count == 0 && text2 == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Missing Options entry for non-terminating screen " + screensEntry.Key?.ToString() + " (i.e. End not set). File:\n" + fileName);
				flag = false;
			}
			if (flag)
			{
				roadEventScreen = new CRoadEventScreen(screenName, text, image, header, list, list2, result, text2, list3, player);
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}

	public List<CItem> GetScreenItems()
	{
		List<CItem> list = new List<CItem>();
		foreach (CRoadEventOption option in Options)
		{
			list.AddRange(option.GetRoadEventOptionItems());
		}
		foreach (TreasureTable treasureTable in TreasureTables)
		{
			list.AddRange(treasureTable.GetTreasureTableItems());
		}
		foreach (CRoadEventTreasureCondition treasureCondition in TreasureConditions)
		{
			list.AddRange(treasureCondition.Screen.GetScreenItems());
		}
		return list;
	}
}
