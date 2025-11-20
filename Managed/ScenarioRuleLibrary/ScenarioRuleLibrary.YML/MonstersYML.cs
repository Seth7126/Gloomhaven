using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class MonstersYML
{
	private static bool _enableLazy = true;

	private LazyLoaderContainer<MonsterYMLData> _lazyLoadersContainer;

	public const int MinimumFilesRequired = 1;

	public LazyLoaderContainer<MonsterYMLData> LoadersContainer => _lazyLoadersContainer;

	public List<MonsterYMLData> LoadedYML { get; private set; }

	public static bool EnableLazy
	{
		get
		{
			return _enableLazy;
		}
		set
		{
			_enableLazy = value;
		}
	}

	public MonstersYML()
	{
		_enableLazy = false;
		LoadedYML = new List<MonsterYMLData>();
		_lazyLoadersContainer = new LazyLoaderContainer<MonsterYMLData>("Enemy", ProcessFile);
	}

	public MonsterYMLData GetMonsterData(string id, ScenarioManager.EDLLMode mode)
	{
		if (!_enableLazy)
		{
			return LoadedYML.FirstOrDefault((MonsterYMLData x) => x.ID == id);
		}
		return _lazyLoadersContainer.GetData(id, mode, removeSuffix: true);
	}

	public bool ProcessFile(StreamReader fileStream, string fileName, Dictionary<string, MonsterYMLData> dictionary = null)
	{
		if (dictionary == null && _enableLazy)
		{
			return true;
		}
		try
		{
			bool flag = true;
			MonsterYMLData monsterYMLData = new MonsterYMLData(fileName);
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
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value11))
						{
							if (value11.Length <= 64)
							{
								monsterYMLData.ID = value11;
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Monster ID exceeds max allowed length (64) in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "LocKey":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "LocKey", fileName, out var value8))
						{
							monsterYMLData.LocKey = CardProcessingShared.GetLookupValue(value8);
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "MonsterType":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MonsterType", fileName, out var monsterTypeString))
						{
							EMonsterType eMonsterType = MonsterYMLData.MonsterTypes.Single((EMonsterType x) => x.ToString() == monsterTypeString);
							if (eMonsterType != EMonsterType.None)
							{
								monsterYMLData.MonsterType = eMonsterType;
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
						continue;
					}
					case "Model":
					{
						if (YMLShared.GetStringList(entry.Value, "Model", fileName, out var values2))
						{
							monsterYMLData.Models = new List<string>();
							foreach (string item in values2)
							{
								if (GetNPCModelEnumFromSpaceName(item) != CClass.ENPCModel.None)
								{
									if (!monsterYMLData.Models.Contains(item))
									{
										monsterYMLData.Models.Add(item);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Model '" + item + "' specified in file " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "NonEliteVariant":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "NonEliteVariant", fileName, out var value))
						{
							monsterYMLData.NonEliteVariant = value;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "PredominantlyMelee":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PredominantlyMelee", fileName, out var value10))
						{
							monsterYMLData.PredominantlyMelee = value10;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "AbilityCards":
					{
						if (YMLShared.GetIntList(entry.Value, "AbilityCards", fileName, out var values3))
						{
							foreach (int cardID in values3)
							{
								MonsterCardYMLData monsterCardYMLData = ScenarioRuleClient.SRLYML.MonsterCards.SingleOrDefault((MonsterCardYMLData s) => s.ID == cardID);
								if (monsterCardYMLData != null)
								{
									if (monsterYMLData.AbilityCards == null)
									{
										monsterYMLData.AbilityCards = new List<MonsterCardYMLData>();
									}
									monsterYMLData.AbilityCards.Add(monsterCardYMLData);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to find Monster Card with ID " + cardID + " in file " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "AddNumberOfPlayersTo":
					{
						if (YMLShared.GetStringList(entry.Value, "AddNumberOfPlayersTo", fileName, out var values))
						{
							foreach (string addProp in values)
							{
								EMonsterBaseStats eMonsterBaseStats = MonsterYMLData.MonsterBaseStatsEnums.SingleOrDefault((EMonsterBaseStats s) => s.ToString() == addProp);
								if (eMonsterBaseStats != EMonsterBaseStats.None)
								{
									if (monsterYMLData.AddNumberOfPlayersTo == null)
									{
										monsterYMLData.AddNumberOfPlayersTo = new List<EMonsterBaseStats>();
									}
									monsterYMLData.AddNumberOfPlayersTo.Add(eMonsterBaseStats);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid value specified for AddNumberOfPlayersTo in file " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "MonsterClassIDToActImmediatelyBefore":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "MonsterClassIDToActImmediatelyBefore", fileName, out var value7))
						{
							monsterYMLData.MonsterClassIDToActImmediatelyBefore = value7;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "StandeeLimit":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "StandeeLimit", fileName, out var value6))
						{
							monsterYMLData.StandeeLimit = value6;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "UseNormalSizeAvatarForBoss":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UseNormalSizeAvatarForBoss", fileName, out var value4))
						{
							monsterYMLData.UseNormalSizeAvatarForBoss = value4;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "Colour":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Colour", fileName, out var value3))
						{
							if (value3.Length == 3 || value3.Length == 4 || value3.Length == 6 || value3.Length == 8)
							{
								bool flag2 = true;
								string text = value3;
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
									value3 = (monsterYMLData.ColourHTML = "#" + value3);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Incorrect Colour format. Use only hexadecimal characters in file " + fileName);
								flag = false;
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
						continue;
					}
					case "ChunkModifier":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "ChunkModifier", fileName, out var value9))
						{
							if (value9 >= 0f && value9 <= 1f)
							{
								monsterYMLData.Fatness = value9;
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "ChunkModifier is out of range. It has to be a value between 0 and 1 in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "Vertex":
					{
						if (YMLShared.GetFloatPropertyValue(entry.Value, "Vertex", fileName, out var value5))
						{
							if (value5 >= -0.5f && value5 <= 0.5f)
							{
								monsterYMLData.VertexAnimIntensity = value5;
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Vertex is out of range. It has to be a value between -0.5 and 0.5 in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "CustomConfig":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "CustomConfig", fileName, out var value2))
						{
							monsterYMLData.CustomConfig = value2;
						}
						else
						{
							flag = false;
						}
						continue;
					}
					case "Parser":
						continue;
					}
					if (entry.Key.ToString().Length > 4 && entry.Key.ToString().Substring(0, 5) == "Level")
					{
						try
						{
							if (int.TryParse(entry.Key.ToString()[5].ToString(), out var result))
							{
								if (result >= 0 && result < 8 && entry.Value is Mapping)
								{
									if (!GetBaseStats(entry, result, out var baseStats, fileName, monsterYMLData.MonsterType == EMonsterType.Object))
									{
										return false;
									}
									if (monsterYMLData.MonsterBaseStats == null)
									{
										monsterYMLData.MonsterBaseStats = new List<BaseStats>();
									}
									monsterYMLData.MonsterBaseStats.Add(baseStats);
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Base Stats entry " + entry.Key.ToString() + " in file " + fileName);
									flag = false;
								}
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Base Stats entry " + entry.Key.ToString() + " in file " + fileName);
								flag = false;
							}
						}
						catch
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Base Stats entry " + entry.Key.ToString() + " in file " + fileName);
							flag = false;
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of layout file " + fileName);
						flag = false;
					}
				}
				if (monsterYMLData.ID == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					if (dictionary != null)
					{
						dictionary.Add(monsterYMLData.ID, monsterYMLData);
					}
					else
					{
						LoadedYML.Add(monsterYMLData);
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

	private bool GetBaseStats(MappingEntry parentMapEntry, int level, out BaseStats baseStats, string fileName, bool isObject = false)
	{
		bool flag = true;
		baseStats = null;
		int num = int.MaxValue;
		int num2 = ((!isObject) ? int.MaxValue : 0);
		int num3 = ((!isObject) ? int.MaxValue : 0);
		int num4 = ((!isObject) ? int.MaxValue : 0);
		int num5 = ((!isObject) ? int.MaxValue : 0);
		int shield = 0;
		int retaliate = 0;
		int retaliateRange = 0;
		int pierce = 0;
		bool flying = false;
		bool advantage = false;
		bool attackersGainDisadvantage = false;
		bool invulnerable = false;
		bool pierceInvulnerablility = false;
		bool untargetable = false;
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		List<CAbility.EAbilityType> list2 = new List<CAbility.EAbilityType>();
		Dictionary<CAbility.EAbilityType, int> dictionary = new Dictionary<CAbility.EAbilityType, int>();
		List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = new List<AbilityData.StatIsBasedOnXData>();
		List<CAbility> onDeathAbilities = new List<CAbility>();
		if (isObject)
		{
			list2.AddRange(new List<CAbility.EAbilityType>
			{
				CAbility.EAbilityType.Shield,
				CAbility.EAbilityType.Retaliate,
				CAbility.EAbilityType.Invisible,
				CAbility.EAbilityType.Poison,
				CAbility.EAbilityType.Wound,
				CAbility.EAbilityType.Muddle,
				CAbility.EAbilityType.Immobilize,
				CAbility.EAbilityType.Disarm,
				CAbility.EAbilityType.Curse,
				CAbility.EAbilityType.Stun,
				CAbility.EAbilityType.Strengthen,
				CAbility.EAbilityType.Bless,
				CAbility.EAbilityType.Push,
				CAbility.EAbilityType.Pull,
				CAbility.EAbilityType.ControlActor,
				CAbility.EAbilityType.StopFlying,
				CAbility.EAbilityType.Swap,
				CAbility.EAbilityType.Sleep
			});
		}
		if (YMLShared.GetMapping(parentMapEntry, fileName, out var mapping))
		{
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "Health":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Health", fileName, out var value8))
					{
						num = value8;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Move":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Move", fileName, out var value17))
					{
						num2 = value17;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Attack":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Attack", fileName, out var value12))
					{
						num3 = value12;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Range":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Range", fileName, out var value11))
					{
						num4 = value11;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Target":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Target", fileName, out var value4))
					{
						num5 = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Shield":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Shield", fileName, out var value9))
					{
						shield = value9;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Retaliate":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "Retaliate", fileName, out var value5))
					{
						retaliate = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "RetaliateRange":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "RetaliateRange", fileName, out var value15))
					{
						retaliateRange = value15;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Pierce":
				{
					string value2;
					if (YMLShared.GetIntPropertyValue(entry.Value, "Pierce", fileName, out var value, suppressErrors: true))
					{
						pierce = value;
					}
					else if (YMLShared.GetStringPropertyValue(entry.Value, "Pierce", fileName, out value2))
					{
						if (value2 == "All")
						{
							pierce = 99999;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Pierce value " + value2 + " in file " + fileName);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Flying":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value14))
					{
						flying = value14;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Advantage":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Advantage", fileName, out var value10))
					{
						advantage = value10;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AttackersGainDisadvantage":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "AttackersGainDisadvantage", fileName, out var value7))
					{
						attackersGainDisadvantage = value7;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Invulnerable":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value3))
					{
						invulnerable = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PierceInvulnerability":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value16))
					{
						pierceInvulnerablility = value16;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Untargetable":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "Flying", fileName, out var value13))
					{
						untargetable = value13;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Conditions":
				{
					if (YMLShared.GetStringList(entry.Value, "AttackersGainDisadvantage", fileName, out var values2))
					{
						foreach (string negString in values2)
						{
							CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition s) => s.ToString() == negString);
							if (eNegativeCondition != CCondition.ENegativeCondition.NA)
							{
								list.Add(eNegativeCondition);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid condition '" + negString + "' specified for Condtions in file " + fileName);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Immunities":
				{
					if (YMLShared.GetStringList(entry.Value, "Immunities", fileName, out var values))
					{
						foreach (string abilityTypeString in values)
						{
							CAbility.EAbilityType eAbilityType2 = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == abilityTypeString);
							if (eAbilityType2 != CAbility.EAbilityType.None)
							{
								list2.Add(eAbilityType2);
								continue;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ability type '" + abilityTypeString + "' specified for Immunities in file " + fileName);
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Specials":
				{
					if (YMLShared.GetMapping(entry, fileName, out var mapping2))
					{
						foreach (MappingEntry entry2 in mapping2.Entries)
						{
							if (YMLShared.GetStringPropertyValue(entry2.Key, "Specials/" + entry.Key, fileName, out var abilityString) && YMLShared.GetIntPropertyValue(entry2.Value, "Specials/" + entry.Key, fileName, out var value6))
							{
								CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == abilityString);
								if (eAbilityType != CAbility.EAbilityType.None)
								{
									dictionary.Add(eAbilityType, value6);
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry '" + abilityString + "' specified for Specials in file " + fileName);
								flag = false;
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
					break;
				}
				case "StatIsBasedOnX":
				{
					if (AbilityData.ParseStatIsBasedOnX(entry, fileName, out var statIsBasedOnXEntries2, isBaseStats: true))
					{
						statIsBasedOnXEntries = statIsBasedOnXEntries2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "OnDeathAbilities":
				{
					if (CardProcessingShared.GetAbilities(entry, 0, isMonster: false, fileName, out var abilities))
					{
						onDeathAbilities = abilities;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in file " + fileName);
					flag = false;
					break;
				}
			}
		}
		else
		{
			flag = false;
		}
		if (num == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Health specified for monster in file " + fileName);
			flag = false;
		}
		if (!isObject)
		{
			if (num2 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Move specified for monster in file " + fileName);
				flag = false;
			}
			if (num3 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Attack specified for monster in file " + fileName);
				flag = false;
			}
			if (num4 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Range specified for monster in file " + fileName);
				flag = false;
			}
			if (num5 == int.MaxValue)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "No Target specified for monster in file " + fileName);
				flag = false;
			}
		}
		if (flag)
		{
			baseStats = new BaseStats(level, num, num2, num3, num4, num5, shield, retaliate, retaliateRange, pierce, flying, advantage, attackersGainDisadvantage, invulnerable, pierceInvulnerablility, untargetable, list, list2, dictionary, statIsBasedOnXEntries, onDeathAbilities);
		}
		return flag;
	}

	public static CClass.ENPCModel GetNPCModelEnumFromSpaceName(string spacedName)
	{
		string noSpaces = spacedName.Replace(" ", string.Empty);
		return CClass.NPCModels.SingleOrDefault((CClass.ENPCModel s) => s.ToString() == noSpaces);
	}
}
