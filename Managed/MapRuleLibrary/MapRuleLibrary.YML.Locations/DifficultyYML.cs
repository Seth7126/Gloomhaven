using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Locations;

public class DifficultyYML
{
	public List<DifficultyYMLData> LoadedYML { get; private set; }

	public DifficultyYML()
	{
		LoadedYML = new List<DifficultyYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		bool flag = true;
		try
		{
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					DifficultyYMLData data = new DifficultyYMLData(fileName)
					{
						GameMode = entry.Key.ToString()
					};
					if (YMLShared.GetMapping(entry, fileName, out var mapping))
					{
						foreach (MappingEntry entry2 in mapping.Entries)
						{
							if (YMLShared.GetMapping(entry2, fileName, out var mapping2))
							{
								if (MapYMLShared.GetDifficulty(mapping2, "DifficultySettings", fileName, out var difficulty))
								{
									data.DifficultySettings.Add(difficulty);
								}
								else
								{
									flag = false;
								}
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
					if (!flag || data.GameMode == null || data.GameMode.Length <= 0)
					{
						continue;
					}
					DifficultyYMLData difficultyYMLData = LoadedYML.SingleOrDefault((DifficultyYMLData s) => s.GameMode == data.GameMode);
					if (difficultyYMLData == null)
					{
						data.DifficultySettings.RemoveAll((CAdventureDifficulty r) => r.LoadAsNewDifficulty);
						LoadedYML.Add(data);
					}
					else
					{
						difficultyYMLData.UpdateData(data);
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
