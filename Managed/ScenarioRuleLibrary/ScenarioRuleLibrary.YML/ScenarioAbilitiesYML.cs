using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ScenarioAbilitiesYML
{
	public List<ScenarioAbilitiesYMLData> LoadedYML { get; private set; }

	public ScenarioAbilitiesYML()
	{
		LoadedYML = new List<ScenarioAbilitiesYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			ScenarioAbilitiesYMLData scenarioAbilityData = new ScenarioAbilitiesYMLData(fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value))
						{
							scenarioAbilityData.ScenarioAbilityID = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ScenarioAbility":
					{
						if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out var abilities))
						{
							scenarioAbilityData.ScenarioAbilities = abilities;
						}
						else
						{
							flag = false;
						}
						break;
					}
					}
				}
				if (flag)
				{
					ScenarioAbilitiesYMLData scenarioAbilitiesYMLData = LoadedYML.SingleOrDefault((ScenarioAbilitiesYMLData s) => s.ScenarioAbilityID == scenarioAbilityData.ScenarioAbilityID);
					if (scenarioAbilitiesYMLData == null)
					{
						LoadedYML.Add(scenarioAbilityData);
					}
					else
					{
						scenarioAbilitiesYMLData.UpdateData(scenarioAbilityData);
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
