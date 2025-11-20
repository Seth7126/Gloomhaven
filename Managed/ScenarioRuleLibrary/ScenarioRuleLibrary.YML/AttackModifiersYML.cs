using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class AttackModifiersYML
{
	public const int MinimumFilesRequired = 1;

	public List<AttackModifierYMLData> LoadedYML { get; private set; }

	public AttackModifiersYML()
	{
		LoadedYML = new List<AttackModifierYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			AttackModifierYMLData data = new AttackModifierYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			data.IsConditionalModifier = false;
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value8))
						{
							data.Name = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MathModifier":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MathModifier", fileName, out var value6))
						{
							if (value6.Contains(" "))
							{
								value6 = value6.Replace(" ", string.Empty);
							}
							if (int.TryParse(value6.Substring(1), out var result))
							{
								if (result < -2 || result > 2)
								{
									data.IsConditionalModifier = true;
								}
								else
								{
									data.IsConditionalModifier = false;
								}
								data.MathModifier = value6;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to Parse MathModifier " + value6 + " in file " + fileName);
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Shuffle":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Shuffle", fileName, out var value4))
						{
							data.Shuffle = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Rolling":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Rolling", fileName, out var value3))
						{
							data.Rolling = value3;
							data.IsConditionalModifier = true;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AddTarget":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "AddTarget", fileName, out var value2))
						{
							data.AddTarget = value2;
							data.IsConditionalModifier = true;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IsBless":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsBless", fileName, out var value5))
						{
							data.IsBless = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IsCurse":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsCurse", fileName, out var value7))
						{
							data.IsCurse = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Abilities":
					{
						List<CAbility> abilities;
						if (entry.Value is Scalar scalar && scalar.Text.ToLower() == "null")
						{
							data.NullAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, data.Name.GetHashCode(), isMonster: false, fileName, out abilities))
						{
							data.Abilities = abilities;
							data.IsConditionalModifier = true;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AbilityOverrides":
						if (entry.Value is Scalar scalar2 && scalar2.Text.ToLower() == "null")
						{
							data.NullOverrides = true;
						}
						else if (entry.Value is Mapping)
						{
							if (YMLShared.GetMapping(entry, fileName, out var mapping))
							{
								foreach (MappingEntry entry2 in mapping.Entries)
								{
									CAbilityOverride cAbilityOverride = CAbilityOverride.CreateAbilityOverride(entry2, data.Name.GetHashCode(), isMonster: false, fileName, string.Empty);
									if (cAbilityOverride != null)
									{
										if (data.Overrides == null)
										{
											data.Overrides = new List<CAbilityOverride>();
										}
										data.Overrides.Add(cAbilityOverride);
										data.IsConditionalModifier = true;
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
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry for AbilityOverrides.  Entry must be mapping.  File: " + fileName);
							flag = false;
						}
						break;
					case "UseGenericIcon":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IsBless", fileName, out var value))
						{
							data.UseGenericIcon = value;
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
					}
				}
				if (data.Name == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Name specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					AttackModifierYMLData attackModifierYMLData = LoadedYML.SingleOrDefault((AttackModifierYMLData s) => s.Name == data.Name);
					if (attackModifierYMLData == null)
					{
						LoadedYML.Add(data);
					}
					else
					{
						attackModifierYMLData.UpdateData(data);
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

	public static AttackModifierYMLData CreateCurse()
	{
		string curseName = (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness) ? "Reduced_Randomness_Curse" : "Curse");
		AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData f) => f.Name == curseName)?.Copy();
		if (attackModifierYMLData == null)
		{
			attackModifierYMLData = new AttackModifierYMLData(CCondition.ENegativeCondition.Curse.ToString(), "*0", isCurse: true, isBless: false);
			if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.ModdedRuleset)
			{
				ScenarioRuleClient.SRLYML.ModdedData.AttackModifiers.LoadedYML.Add(attackModifierYMLData);
			}
			else
			{
				ScenarioRuleClient.SRLYML.RulesetData.AttackModifiers.LoadedYML.Add(attackModifierYMLData);
			}
		}
		return attackModifierYMLData;
	}

	public static AttackModifierYMLData CreateBless()
	{
		string blessName = (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.ReducedRandomness) ? "Reduced_Randomness_Bless" : "Bless");
		AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData f) => f.Name == blessName)?.Copy();
		if (attackModifierYMLData == null)
		{
			attackModifierYMLData = new AttackModifierYMLData(CCondition.EPositiveCondition.Bless.ToString(), "*2", isCurse: false, isBless: true);
			if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.ModdedRuleset)
			{
				ScenarioRuleClient.SRLYML.ModdedData.AttackModifiers.LoadedYML.Add(attackModifierYMLData);
			}
			else
			{
				ScenarioRuleClient.SRLYML.RulesetData.AttackModifiers.LoadedYML.Add(attackModifierYMLData);
			}
		}
		return attackModifierYMLData;
	}
}
