using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Locations;

public class VillageYML
{
	public const int MinimumFilesRequired = 1;

	public List<CVillage> LoadedYML { get; private set; }

	public VillageYML()
	{
		LoadedYML = new List<CVillage>();
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
			string localisedName = null;
			string localisedDescription = null;
			string mesh = null;
			TileIndex mapLocation = null;
			List<TileIndex> list = new List<TileIndex>();
			List<TileIndex> list2 = new List<TileIndex>();
			CUnlockCondition unlockCondition = null;
			List<string> questNames = new List<string>();
			List<string> removeQuestNames = new List<string>();
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value4))
						{
							id = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedName":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value2))
						{
							localisedName = value2;
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
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value3))
						{
							mesh = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MapLocation":
					{
						if (YMLShared.GetIntList(entry.Value, entry.Key.ToString(), fileName, out var values4))
						{
							if (values4.Count == 2)
							{
								mapLocation = new TileIndex(values4[0], values4[1]);
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
						if (YMLShared.GetSequence(entry, fileName, out var sequence))
						{
							foreach (DataItem entry2 in sequence.Entries)
							{
								if (YMLShared.GetIntList(entry2, "JobLocations", fileName, out var values3, allowScalar: false) && values3.Count == 2)
								{
									list.Add(new TileIndex(values3[0], values3[1]));
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
						if (YMLShared.GetSequence(entry, fileName, out var sequence2))
						{
							foreach (DataItem entry3 in sequence2.Entries)
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
					case "Quests":
					{
						if (YMLShared.GetStringList(entry.Value, entry.Key.ToString(), fileName, out var values2))
						{
							questNames = values2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RemoveQuests":
					{
						if (YMLShared.GetStringList(entry.Value, entry.Key.ToString(), fileName, out var values))
						{
							removeQuestNames = values;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of Village file " + fileName);
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
					CVillage cVillage = LoadedYML.SingleOrDefault((CVillage s) => s.ID == id);
					if (cVillage == null)
					{
						LoadedYML.Add(new CVillage(id, localisedName, localisedDescription, mesh, mapLocation, list, unlockCondition, questNames, fileName));
					}
					else
					{
						cVillage.UpdateData(localisedName, localisedDescription, mesh, mapLocation, list, list2, unlockCondition, questNames, removeQuestNames);
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
