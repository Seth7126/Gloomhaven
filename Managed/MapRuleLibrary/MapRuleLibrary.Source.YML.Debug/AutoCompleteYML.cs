using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.Source.YML.Debug;

public class AutoCompleteYML
{
	public List<string> AutoCompleteQuestIDs { get; set; }

	public List<string> AutoCompleteAchievementIDs { get; set; }

	public bool IsLoaded { get; set; }

	public string FileName { get; private set; }

	public AutoCompleteYML()
	{
		AutoCompleteQuestIDs = new List<string>();
		AutoCompleteAchievementIDs = new List<string>();
		IsLoaded = false;
	}

	public bool Validate()
	{
		return true;
	}

	public bool ProcessFile(string fileName)
	{
		try
		{
			bool flag = true;
			YamlParser yamlParser = new YamlParser();
			if (!File.Exists(fileName))
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "AutoComplete.yml file does not exist. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
				return true;
			}
			string text = File.ReadAllText(fileName);
			if (text.Trim().Length == 0)
			{
				return true;
			}
			TextInput input = new TextInput(text);
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "AutoCompleteQuestIDs":
					{
						if (!(entry.Value is Sequence) || !YMLShared.GetStringList(entry.Value, "AutoCompleteQuestIDs", fileName, out var values2))
						{
							break;
						}
						foreach (string item in values2)
						{
							list.Add(item);
						}
						break;
					}
					case "AutoCompleteAchievementIDs":
					{
						if (!(entry.Value is Sequence) || !YMLShared.GetStringList(entry.Value, "AutoCompleteAchievementIDs", fileName, out var values))
						{
							break;
						}
						foreach (string item2 in values)
						{
							list2.Add(item2);
						}
						break;
					}
					}
				}
				AutoCompleteQuestIDs.Clear();
				AutoCompleteAchievementIDs.Clear();
				foreach (string item3 in list)
				{
					if (!AutoCompleteQuestIDs.Contains(item3))
					{
						AutoCompleteQuestIDs.Add(item3);
					}
				}
				foreach (string item4 in list2)
				{
					if (!AutoCompleteAchievementIDs.Contains(item4))
					{
						AutoCompleteAchievementIDs.Add(item4);
					}
				}
				if (flag)
				{
					IsLoaded = true;
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
