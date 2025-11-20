using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Events;

public class CInitialEventsYML
{
	public List<string> RoadEvents { get; private set; }

	public List<string> CityEvents { get; private set; }

	public List<string> JOTLEvents { get; private set; }

	public bool IsLoaded { get; private set; }

	public string FileName { get; private set; }

	public CInitialEventsYML()
	{
		RoadEvents = null;
		CityEvents = null;
		JOTLEvents = null;
		IsLoaded = false;
	}

	public bool Validate()
	{
		bool result = true;
		if (RoadEvents.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Road Events were loaded " + FileName);
			result = false;
		}
		if (CityEvents.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No City Events were loaded " + FileName);
			result = false;
		}
		return result;
	}

	public void UpdateData(List<string> roadEvents, List<string> cityEvents, List<string> jotlEvents)
	{
		if (roadEvents != null)
		{
			RoadEvents.AddRange(roadEvents);
		}
		if (cityEvents != null)
		{
			CityEvents.AddRange(cityEvents);
		}
		if (JOTLEvents != null)
		{
			JOTLEvents.AddRange(jotlEvents);
		}
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			FileName = fileName;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				List<string> list3 = new List<string>();
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "RoadEvents":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence3))
						{
							foreach (DataItem entry2 in sequence3.Entries)
							{
								if (YMLShared.GetStringPropertyValue(entry2, "RoadEvents", fileName, out var value3))
								{
									list.Add(value3);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid RoadEvents entry " + entry.Key?.ToString() + ".  Must be Road Event Name.  File:\n" + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CityEvents":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence2))
						{
							foreach (DataItem entry3 in sequence2.Entries)
							{
								if (YMLShared.GetStringPropertyValue(entry3, "CityEvents", fileName, out var value2))
								{
									list2.Add(value2);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CityEvents entry " + entry.Key?.ToString() + ".  Must be City Event Name.  File:\n" + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "JOTLEvents":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence))
						{
							foreach (DataItem entry4 in sequence.Entries)
							{
								if (YMLShared.GetStringPropertyValue(entry4, "JOTLEvents", fileName, out var value))
								{
									list3.Add(value);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid JOTLEvents entry " + entry.Key?.ToString() + ".  Must be Road Event Name.  File:\n" + fileName);
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
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + FileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (flag)
				{
					if (!IsLoaded)
					{
						RoadEvents = list;
						CityEvents = list2;
						JOTLEvents = list3;
						IsLoaded = true;
					}
					else
					{
						UpdateData(list, list2, list3);
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
}
