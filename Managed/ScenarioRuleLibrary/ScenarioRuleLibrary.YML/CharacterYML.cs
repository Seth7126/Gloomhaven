using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class CharacterYML
{
	public const int MinimumFilesRequired = 1;

	public List<CharacterYMLData> LoadedYML { get; private set; }

	public CharacterYML()
	{
		LoadedYML = new List<CharacterYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			CharacterYMLData characterData = new CharacterYMLData(fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value2))
						{
							if (value2.Length <= 64)
							{
								characterData.ID = value2;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Character ID exceeds max allowed length (64) in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocKey", fileName, out var value11))
						{
							characterData.LocKey = CardProcessingShared.GetLookupValue(value11);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Model":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Model", fileName, out var modelString))
						{
							ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == modelString);
							if (eCharacter != ECharacter.None)
							{
								characterData.Model = eCharacter;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid model name " + modelString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DefaultSkin":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "DefaultSkin", fileName, out var value8))
						{
							characterData.DefaultSkin = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "HealthTable":
					{
						if (YMLShared.GetIntArray(entry.Value, "Health", fileName, 10, out var outArray, ignorePositionZero: true))
						{
							characterData.HealthTable = outArray;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "NumberAbilityCardsInBattle":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "NumberAbilityCardsInBattle", fileName, out var value7))
						{
							characterData.NumberAbilityCardsInBattle = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AttackModifierDeck":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AttackModifierDeck", fileName, out var amdValue))
						{
							if (ScenarioRuleClient.SRLYML.AttackModifierDecks.SingleOrDefault((AttackModifierDeckYMLData a) => a.Name == amdValue) != null)
							{
								characterData.AttackModifierDeckString = amdValue;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {amdValue} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CompanionSummonData":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CompanionSummonData", fileName, out var summonID))
						{
							if (summonID.ToLower() == "null")
							{
								characterData.NullCompanionSummonID = true;
								break;
							}
							if (ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData h) => h.ID == summonID) != null)
							{
								characterData.CompanionSummonID = summonID;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, $"Unrecognized reference for {summonID} in {fileName}");
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Description", fileName, out var value9))
						{
							characterData.Description = value9;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Role":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Role", fileName, out var value5))
						{
							characterData.Role = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Difficulty":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Difficulty", fileName, out var value4))
						{
							characterData.Difficulty = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Strengths":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Strengths", fileName, out var value14))
						{
							characterData.Strengths = value14;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Weaknesses":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Weaknesses", fileName, out var value13))
						{
							characterData.Weaknesses = value13;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Adventure_Description":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Adventure_Description", fileName, out var value10))
						{
							characterData.Adventure_Description = value10;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Colour":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Colour", fileName, out var value12))
						{
							if (value12.Length == 3 || value12.Length == 4 || value12.Length == 6 || value12.Length == 8)
							{
								bool flag2 = true;
								string text = value12;
								foreach (char c in text)
								{
									if ((c < 'A' || c > 'F') && (c < '0' || c > '9'))
									{
										flag2 = false;
										break;
									}
								}
								if (flag2)
								{
									value12 = "#" + value12;
									characterData.ColourHTML = value12;
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Incorrect Colour format. Use only hexadecimal characters in file " + fileName);
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Colour property has to be exactly 3, 4, 6 or 8 characters long in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ChunkModifier":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "ChunkModifier", fileName, out var value6))
						{
							if (value6 >= 0f && value6 <= 1f)
							{
								characterData.Fatness = value6;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "ChunkModifier is out of range. It has to be a value between 0 and 1 in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Vertex":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "Vertex", fileName, out var value3))
						{
							if (value3 >= -0.5f && value3 <= 0.5f)
							{
								characterData.VertexAnimIntensity = value3;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Vertex is out of range. It has to be a value between -0.5 and 0.5 in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "CustomCharacterConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomCharacterConfig", fileName, out var value))
						{
							characterData.CustomCharacterConfig = value;
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
				if (characterData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					CharacterYMLData characterYMLData = LoadedYML.SingleOrDefault((CharacterYMLData s) => s.ID == characterData.ID);
					if (characterYMLData == null)
					{
						LoadedYML.Add(characterData);
					}
					else
					{
						characterYMLData.UpdateData(characterData);
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
