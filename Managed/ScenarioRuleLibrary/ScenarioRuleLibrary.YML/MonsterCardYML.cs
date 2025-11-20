using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonsterCardYML
{
	public const int MinimumFilesRequired = 1;

	public List<MonsterCardYMLData> LoadedYML { get; private set; }

	public MonsterCardYML()
	{
		LoadedYML = new List<MonsterCardYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			MonsterCardYMLData data = new MonsterCardYMLData(fileName);
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
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ID", fileName, out var value3))
						{
							data.ID = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Initiative":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "Initiative", fileName, out var value2))
						{
							data.Initiative = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Shuffle":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Shuffle", fileName, out var value))
						{
							data.Shuffle = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Data":
						if (entry.Value is Mapping)
						{
							if (AbilityData.GetAction(entry.Value as Mapping, data.ID, fileName, DiscardType.Discard, out var action, out var _, isMonster: true))
							{
								data.CardAction = action;
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Child of Data entry is not a mapping. File:\n" + fileName + "\nDataItem:\n" + entry.Value.ToString());
							flag = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (data.ID == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					MonsterCardYMLData monsterCardYMLData = LoadedYML.SingleOrDefault((MonsterCardYMLData s) => s.ID == data.ID);
					if (monsterCardYMLData == null)
					{
						LoadedYML.Add(data);
					}
					else
					{
						monsterCardYMLData.UpdateData(data);
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
