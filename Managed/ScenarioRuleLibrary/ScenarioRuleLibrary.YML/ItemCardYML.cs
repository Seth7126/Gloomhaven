using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class ItemCardYML
{
	public const int MinimumFilesRequired = 1;

	public List<ItemCardYMLData> LoadedYML { get; private set; }

	public ItemCardYML()
	{
		LoadedYML = new List<ItemCardYMLData>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			ItemCardYMLData itemData = new ItemCardYMLData(fileName);
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
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "ID", fileName, out var value5))
						{
							itemData.ID = value5;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StringID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "StringID", fileName, out var value4))
						{
							itemData.StringID = CardProcessingShared.GetLookupValue(value4);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value8))
						{
							itemData.Name = CardProcessingShared.GetLookupValue(value8);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Art":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Art", fileName, out var value2))
						{
							itemData.Art = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "TotalInGame":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "TotalInGame", fileName, out var value10))
						{
							itemData.TotalInGame = value10;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Cost":
					{
						if (YMLShared.ParseIntValue(entry.Value.ToString(), "Cost", fileName, out var value9))
						{
							itemData.Cost = value9;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Slot":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Slot", fileName, out var slotString))
						{
							CItem.EItemSlot eItemSlot = CItem.ItemSlots.SingleOrDefault((CItem.EItemSlot x) => x.ToString() == slotString);
							if (eItemSlot != CItem.EItemSlot.None)
							{
								itemData.Slot = eItemSlot;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Slot " + slotString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Usage":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Usage", fileName, out var usageString))
						{
							CItem.EUsageType eUsageType = CItem.UsageTypes.SingleOrDefault((CItem.EUsageType x) => x.ToString() == usageString);
							if (eUsageType != CItem.EUsageType.None)
							{
								itemData.Usage = eUsageType;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Usage " + usageString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UsedWhenEquipped":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "UsedWhenEquipped", fileName, out var value6))
						{
							itemData.UsedWhenEquipped = value6;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Rarity":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Rarity", fileName, out var rarityString))
						{
							CItem.EItemRarity eItemRarity = CItem.ItemRarities.SingleOrDefault((CItem.EItemRarity x) => x.ToString() == rarityString);
							if (eItemRarity != CItem.EItemRarity.None)
							{
								itemData.Rarity = eItemRarity;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Rarity " + rarityString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ProsperityRequirement":
					{
						if (YMLShared.GetIntPropertyValue(entry.Value, "ProsperityRequirement", fileName, out var value7))
						{
							itemData.ProsperityRequirement = value7;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Trigger":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Trigger", fileName, out var triggerString))
						{
							CItem.EItemTrigger eItemTrigger = CItem.ItemTriggers.SingleOrDefault((CItem.EItemTrigger x) => x.ToString() == triggerString);
							if (eItemTrigger != CItem.EItemTrigger.None)
							{
								itemData.Trigger = eItemTrigger;
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Trigger " + triggerString + " in file " + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Consumes":
					{
						if (YMLShared.GetStringList(entry.Value, "Consume", fileName, out var values2))
						{
							foreach (string consumeValue in values2)
							{
								try
								{
									ElementInfusionBoardManager.EElement item = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == consumeValue);
									itemData.Consumes.Add(item);
								}
								catch
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid consume element " + consumeValue + " in file " + fileName);
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
					case "Data":
					{
						ItemData itemData2 = new ItemData();
						if (itemData2.ProcessEntry(entry, itemData.ID, fileName))
						{
							itemData.Data = itemData2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Layout":
					{
						if (YMLShared.GetSequence(entry.Value, "Layout", fileName, out var sequence))
						{
							itemData.CardLayout = new CardLayout(sequence, itemData.Data.Abilities, null, itemData.Name, DiscardType.None, fileName, itemData.Data.Overrides);
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "ValidEquipCharacterClassIDs":
					{
						if (YMLShared.GetStringList(entry.Value, "ValidEquipCharacterClassIDs", fileName, out var values))
						{
							itemData.ValidEquipCharacterClassIDs = values.Distinct().ToList();
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Tradeable":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "Tradeable", fileName, out var value3))
						{
							itemData.Tradeable = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "PermanentlyConsumed":
					{
						if (YMLShared.GetBoolPropertyValue(entry.Value, "PermanentlyConsumed", fileName, out var value))
						{
							itemData.PermanentlyConsumed = value;
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
				if (itemData.ID == int.MaxValue)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					if (itemData.Data.Abilities != null && itemData.Data.Abilities.Count > 0)
					{
						itemData.ItemType = CItem.EItemType.Ability;
					}
					if (itemData.Data.Overrides != null && itemData.Data.Overrides.Count > 0)
					{
						if (itemData.ItemType == CItem.EItemType.Ability)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Both Abilities and Overrides can't be set on the same item card.  File: " + fileName);
							return false;
						}
						itemData.ItemType = CItem.EItemType.Override;
					}
					if (itemData.Data.Abilities != null)
					{
						foreach (CAbility ability in itemData.Data.Abilities)
						{
							if (!(ability is CAbilityConsumeElement))
							{
								continue;
							}
							foreach (ElementInfusionBoardManager.EElement item2 in (ability as CAbilityConsumeElement).ElementsToConsume)
							{
								itemData.Consumes.Add(item2);
							}
						}
					}
					ItemCardYMLData itemCardYMLData = LoadedYML.SingleOrDefault((ItemCardYMLData s) => s.ID == itemData.ID);
					if (itemCardYMLData == null)
					{
						LoadedYML.Add(itemData);
					}
					else
					{
						itemCardYMLData.UpdateData(itemData);
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
