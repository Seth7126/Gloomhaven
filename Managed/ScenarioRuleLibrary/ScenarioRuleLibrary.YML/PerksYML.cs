using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class PerksYML
{
	public const int MinimumFilesRequired = 1;

	public List<PerksYMLData> LoadedYML { get; private set; }

	public PerksYML()
	{
		LoadedYML = new List<PerksYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			PerksYMLData perkData = new PerksYMLData(fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value6))
						{
							perkData.ID = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value5))
						{
							perkData.Name = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Description", fileName, out var value3))
						{
							perkData.Description = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CharacterID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CharacterID", fileName, out var charID))
						{
							if (ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData c) => c.ID == charID) != null)
							{
								perkData.CharacterID = charID;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {charID} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Available":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Available", fileName, out var value2))
						{
							perkData.Available = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CardsToAdd":
					{
						if (YMLShared.GetStringList(entry.Value, "CardsToAdd", fileName, out var values))
						{
							foreach (string card in values)
							{
								if (ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData a) => a.Name == card) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {card} in {fileName}");
									flag = false;
								}
							}
							if (flag)
							{
								perkData.CardsToAddStrings = values;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CardsToRemove":
					{
						if (YMLShared.GetStringList(entry.Value, "CardsToRemove", fileName, out var values2))
						{
							foreach (string card2 in values2)
							{
								if (ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData a) => a.Name == card2) == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {card2} in {fileName}");
									flag = false;
								}
							}
							if (flag)
							{
								perkData.CardsToRemoveStrings = values2;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IgnoreNegativeItemEffects":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreNegativeItemEffects", fileName, out var value4))
						{
							perkData.IgnoreNegativeItemEffectsNullable = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IgnoreNegativeScenarioEffects":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreNegativeScenarioEffects", fileName, out var value))
						{
							perkData.IgnoreNegativeScenarioEffectsNullable = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (perkData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					PerksYMLData perksYMLData = LoadedYML.SingleOrDefault((PerksYMLData s) => s.ID == perkData.ID);
					if (perksYMLData == null)
					{
						LoadedYML.Add(perkData);
					}
					else
					{
						perksYMLData.UpdateData(perkData);
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
