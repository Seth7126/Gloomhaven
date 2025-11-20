using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class HeroSummonYML
{
	public List<HeroSummonYMLData> LoadedYML { get; private set; }

	public HeroSummonYML()
	{
		LoadedYML = new List<HeroSummonYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			HeroSummonYMLData summonData = new HeroSummonYMLData(fileName);
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					bool useSpecialBaseState;
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value19))
						{
							if (value19.Length <= 64)
							{
								summonData.ID = value19;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Summon ID exceeds max allowed length (64) in file " + fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocKey", fileName, out var value5))
						{
							summonData.LocKey = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Model":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Model", fileName, out var value2))
						{
							CClass.ENPCModel nPCModelEnumFromSpaceName = GetNPCModelEnumFromSpaceName(value2);
							if (nPCModelEnumFromSpaceName != CClass.ENPCModel.None)
							{
								summonData.Model = value2;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Summon Model " + nPCModelEnumFromSpaceName.ToString() + " File:\n" + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Attack":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Attack", fileName, out int amount, out useSpecialBaseState, out int enhancementSlots, out ElementInfusionBoardManager.EElement? infuse, ignoreAmount: false))
						{
							summonData.Attack = amount;
							summonData.AttackEnhancements = enhancementSlots;
							summonData.AttackInfuse = infuse;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Range":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Range", fileName, out var amount4, out useSpecialBaseState, out var enhancementSlots4))
						{
							summonData.Range = amount4;
							summonData.RangeEnhancements = enhancementSlots4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Move":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Move", fileName, out var amount2, out useSpecialBaseState, out var enhancementSlots2))
						{
							summonData.Move = amount2;
							summonData.MoveEnhancements = enhancementSlots2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Health":
					{
						if (AbilityData.GetEnhancedAbilityValues(entry.Value, "Health", fileName, out var amount3, out useSpecialBaseState, out var enhancementSlots3))
						{
							summonData.HealthTable = new int[10] { amount3, amount3, amount3, amount3, amount3, amount3, amount3, amount3, amount3, amount3 };
							summonData.HealthEnhancements = enhancementSlots3;
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
							summonData.HealthTable = outArray;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Shield":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Shield", fileName, out var value12))
						{
							summonData.Shield = value12;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Retaliate":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Retaliate", fileName, out var value29))
						{
							summonData.Retaliate = value29;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "RetaliateRange":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "RetaliateRange", fileName, out var value28))
						{
							summonData.RetaliateRange = value28;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Pierce":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", fileName, out var value18))
						{
							summonData.Pierce = value18;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Flying":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value7))
						{
							summonData.Flying = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Invulnerable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Invulnerable", fileName, out var value9))
						{
							summonData.Invulnerable = value9;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PierceInvulnerability":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PierceInvulnerability", fileName, out var value26))
						{
							summonData.PierceInvulnerability = value26;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Untargetable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Untargetable", fileName, out var value22))
						{
							summonData.Untargetable = value22;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DoesNotBlock":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "DoesNotBlock", fileName, out var value16))
						{
							summonData.DoesNotBlock = value16;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PlayerControlled":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PlayerControlled", fileName, out var value17))
						{
							summonData.PlayerControlled = value17;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "IgnoreActorCollision":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "IgnoreActorCollision", fileName, out var value11))
						{
							summonData.IgnoreActorCollision = value11;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TreatAsTrap":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "TreatAsTrap", fileName, out var value8))
						{
							summonData.TreatAsTrap = value8;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AttackAnimOverload":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AttackAnimOverload", fileName, out var value3))
						{
							if (value3.ToLower() == "null")
							{
								summonData.NullAttackAnimOverload = true;
							}
							else
							{
								summonData.AttackAnimOverload = value3;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AttackOnDeathAnimOverload":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "AttackOnDeathAnimOverload", fileName, out var value21))
						{
							if (value21.ToLower() == "null")
							{
								summonData.NullAttackOnDeathAnimOverload = true;
							}
							else
							{
								summonData.AttackOnDeathAnimOverload = value21;
							}
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "AdjacentAttackOnDeath":
					{
						int value24;
						if (YMLShared.GetStringPropertyValue(entry.Value, "AdjacentAttackOnDeath", fileName, out var value23) && value23.ToLower() == "null")
						{
							summonData.NullAdjacentAttackOnDeathNullable = true;
						}
						else if (YMLShared.GetIntPropertyValue(entry.Value, "AdjacentAttackOnDeath", fileName, out value24))
						{
							summonData.AdjacentAttackOnDeath = value24;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StatIsBasedOnX":
					{
						List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "StatIsBasedOnX", fileName, out var value13) && value13.ToLower() == "null")
						{
							summonData.NullStatIsBasedOnXEntries = true;
						}
						else if (AbilityData.ParseStatIsBasedOnX(entry, fileName, out statIsBasedOnXEntries, isBaseStats: true))
						{
							summonData.StatIsBasedOnXEntries = statIsBasedOnXEntries;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "OnAttackCondition":
					case "OnAttackConditions":
					{
						List<CCondition.ENegativeCondition> negativeConditions;
						List<CCondition.EPositiveCondition> positiveConditions;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "OnAttackCondition", fileName, out var value27) && value27.ToLower() == "null")
						{
							summonData.NullOnAttackConditions = true;
						}
						else if (AbilityData.GetConditions(entry, fileName, out negativeConditions, out positiveConditions))
						{
							summonData.OnAttackConditions = negativeConditions;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "OnSummonAbilities":
					{
						List<CAbility> abilities3;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "OnSummonAbilities", fileName, out var value20) && value20.ToLower() == "null")
						{
							summonData.NullOnSummonAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out abilities3))
						{
							summonData.OnSummonAbilities = abilities3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "OnDeathAbilities":
					{
						List<CAbility> abilities2;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "OnDeathAbilities", fileName, out var value15) && value15.ToLower() == "null")
						{
							summonData.NullOnDeathAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out abilities2))
						{
							summonData.OnDeathAbilities = abilities2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "SpecialAbilities":
					{
						List<CAbility> abilities;
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "SpecialAbilities", fileName, out var value6) && value6.ToLower() == "null")
						{
							summonData.NullSpecialAbilities = true;
						}
						else if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out abilities))
						{
							summonData.SpecialAbilities = abilities;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MoveOverride":
					{
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "MoveOverride", fileName, out var value30) && value30.ToLower() == "null")
						{
							summonData.NullMoveOverride = true;
							break;
						}
						summonData.MoveOverride = CAbilityOverride.CreateAbilityOverride(entry, 0, isMonster: false, fileName, "", isConsume: false, "DefaultMove");
						if (summonData.MoveOverride == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "MoveOverride was null.  File: " + fileName);
							flag = false;
						}
						break;
					}
					case "AttackOverride":
					{
						if (entry.Value is Scalar && YMLShared.GetStringPropertyValue(entry.Value, "AttackOverride", fileName, out var value25) && value25.ToLower() == "null")
						{
							summonData.NullAttackOverride = true;
							break;
						}
						summonData.AttackOverride = CAbilityOverride.CreateAbilityOverride(entry, 0, isMonster: false, fileName, "", isConsume: false, "DefaultAttack");
						if (summonData.AttackOverride == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "AttackOverride was null.  File: " + fileName);
							flag = false;
						}
						break;
					}
					case "Colour":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Colour", fileName, out var value14))
						{
							if (value14.Length == 3 || value14.Length == 4 || value14.Length == 6 || value14.Length == 8)
							{
								bool flag2 = true;
								string text = value14;
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
									value14 = "#" + value14;
									summonData.ColourHTML = value14;
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
						if (YMLShared.GetFloatPropertyValue(entry.Value, "ChunkModifier", fileName, out var value10))
						{
							if (value10 >= 0f && value10 <= 1f)
							{
								summonData.Fatness = value10;
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
						if (YMLShared.GetFloatPropertyValue(entry.Value, "Vertex", fileName, out var value4))
						{
							if (value4 >= -0.5f && value4 <= 0.5f)
							{
								summonData.VertexAnimIntensity = value4;
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
					case "CustomConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomConfig", fileName, out var value))
						{
							summonData.CustomConfig = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entry.Key.ToString() + " File:\n" + fileName);
						flag = false;
						break;
					}
				}
				if (summonData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					HeroSummonYMLData heroSummonYMLData = LoadedYML.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonData.ID);
					if (heroSummonYMLData == null)
					{
						LoadedYML.Add(summonData);
					}
					else
					{
						heroSummonYMLData.UpdateData(summonData);
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

	private static CClass.ENPCModel GetNPCModelEnumFromSpaceName(string spacedName)
	{
		string noSpaces = spacedName.Replace(" ", string.Empty);
		return CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s.ToString() == noSpaces);
	}
}
