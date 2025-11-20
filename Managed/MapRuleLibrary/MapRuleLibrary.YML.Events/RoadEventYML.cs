using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Events;

public class RoadEventYML
{
	public const char LookupSymbol = '¬';

	public const int MinimumFilesRequired = 1;

	public List<CRoadEvent> LoadedYML { get; set; }

	public RoadEventYML()
	{
		LoadedYML = new List<CRoadEvent>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName, string eType)
	{
		bool flag = true;
		try
		{
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			string id = null;
			string locKey = null;
			List<CRoadEventScreen> list = new List<CRoadEventScreen>();
			List<string> requiredClass = new List<string>();
			string eventType = eType;
			string expansion = "";
			string narrativeImageId = null;
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Parser":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Parser", fileName, out var value3))
						{
							eventType = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value2))
						{
							id = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value4))
						{
							locKey = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RequiredClass":
					{
						if (YMLShared.GetStringList(entry.Value, "RequiredClass", fileName, out var values))
						{
							foreach (string character in values)
							{
								if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == character) == null && !character.Contains("None"))
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Class:" + character + " specified for Event Required Class.  File: " + fileName);
									flag = false;
								}
							}
							if (flag)
							{
								requiredClass = values;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Expansion":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "EventType", fileName, out var value5))
						{
							expansion = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Screens":
					{
						if (YMLShared.GetMapping(entry, fileName, out var mapping))
						{
							foreach (MappingEntry entry2 in mapping.Entries)
							{
								if (entry2 != null)
								{
									if (CRoadEventScreen.ProcessScreensEntry(entry2.Key.ToString(), entry2, fileName, out var roadEventScreen))
									{
										list.Add(roadEventScreen);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Null entry in Road Event Screens in File:" + fileName);
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
					case "NarrativeImageId":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NarrativeImageId", fileName, out var value))
						{
							narrativeImageId = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid root level entry " + entry.Key.ToString() + " in File:" + fileName);
						flag = false;
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
					CRoadEvent cRoadEvent = LoadedYML.SingleOrDefault((CRoadEvent s) => s.ID == id);
					if (cRoadEvent == null)
					{
						LoadedYML.Add(new CRoadEvent(id, locKey, list, eventType, fileName, narrativeImageId, requiredClass, expansion));
					}
					else
					{
						cRoadEvent.UpdateData(locKey, list, eventType, narrativeImageId, requiredClass, expansion);
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
