using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary;

public class AbilityConsume
{
	public class ConsumeLayoutProperties
	{
		public int GapAbove;

		public int GapBelow;

		public string LayoutPriorTo = string.Empty;

		public ConsumeLayoutProperties Copy()
		{
			return new ConsumeLayoutProperties
			{
				GapAbove = GapAbove,
				GapBelow = GapBelow,
				LayoutPriorTo = LayoutPriorTo
			};
		}
	}

	private string m_FileName;

	public string Name { get; private set; }

	public CActionAugmentation ConsumeData { get; private set; }

	public CardLayout Text { get; set; }

	public AbilityConsume(CActionAugmentation consumeData, string fileName)
	{
		Name = consumeData.Name;
		ConsumeData = consumeData;
		m_FileName = fileName;
	}

	public static bool CreateAbilityConsume(Mapping mapping, string name, int cardID, string filename, bool isMonster, List<CAbility> actionAbilities, out AbilityConsume abilityConsume)
	{
		bool flag = true;
		abilityConsume = null;
		try
		{
			List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
			CAbility costAbility = null;
			List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>();
			List<CActionAugmentationOp> list3 = new List<CActionAugmentationOp>();
			int xp = 0;
			ConsumeLayoutProperties consumeLayoutProperties = new ConsumeLayoutProperties();
			int? num = null;
			int? num2 = null;
			string previewEffectId = null;
			string previewEffectText = null;
			string consumeGroup = null;
			foreach (MappingEntry entry in mapping.Entries)
			{
				switch (entry.Key.ToString())
				{
				case "Element":
				case "Elements":
				{
					if (CardProcessingShared.GetElements(entry.Value, name + "/Elements", filename, out var elements))
					{
						list = elements;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "CostAbility":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping5))
					{
						if (AbilityData.ProcessAbilityEntry(filename, mapping5, name + "CostAbility", cardID, isMonster, out var ability, isSubAbility: false, isConsumeAbility: true))
						{
							if (ability.MiscAbilityData == null)
							{
								ability.MiscAbilityData = new AbilityData.MiscAbilityData();
							}
							ability.MiscAbilityData.AlsoTargetSelf = true;
							costAbility = ability;
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
					break;
				}
				case "Abilities":
				{
					new List<CAbility>();
					if (YMLShared.GetMapping(entry, filename, out var mapping3))
					{
						foreach (MappingEntry entry2 in mapping3.Entries)
						{
							if (YMLShared.GetMapping(entry2, filename, out var mapping4))
							{
								if (AbilityData.ProcessAbilityEntry(filename, mapping4, entry2.Key.ToString(), cardID, isMonster, out var newAbility, isSubAbility: false, isConsumeAbility: true))
								{
									string parentAbilityName = string.Empty;
									CAbility.EAbilityType parentAbilityType = CAbility.EAbilityType.None;
									if (newAbility.ParentName != string.Empty)
									{
										CAbility cAbility2 = actionAbilities.SingleOrDefault((CAbility x) => x.Name == newAbility.ParentName);
										if (cAbility2 != null)
										{
											parentAbilityName = cAbility2.Name;
											parentAbilityType = cAbility2.AbilityType;
										}
									}
									list3.Add(new CActionAugmentationOp(CActionAugmentationOp.EActionAugmentationType.Ability, parentAbilityName, parentAbilityType, newAbility, null));
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
						num = 50;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "XP":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, name + "/XP", filename, out var value2))
					{
						xp = value2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "Infuse":
				{
					if (CardProcessingShared.GetElements(entry.Value, "Infuse", filename, out var elements2))
					{
						list2 = elements2;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "AbilityOverrides":
				{
					if (YMLShared.GetMapping(entry, filename, out var mapping2))
					{
						foreach (MappingEntry entry3 in mapping2.Entries)
						{
							CAbilityOverride newOverride = CAbilityOverride.CreateAbilityOverride(entry3, cardID, isMonster, filename, "", isConsume: true);
							if (newOverride != null)
							{
								List<CAbility> allActionAbilities = new List<CAbility>();
								allActionAbilities.AddRange(actionAbilities);
								GetAllActionAbilitiesRecursive(ref allActionAbilities, ref actionAbilities);
								CAbility cAbility = allActionAbilities.SingleOrDefault((CAbility x) => x.Name == newOverride.ParentName);
								if (cAbility != null)
								{
									list3.Add(new CActionAugmentationOp(CActionAugmentationOp.EActionAugmentationType.AbilityOverride, cAbility.Name, cAbility.AbilityType, null, newOverride));
									continue;
								}
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find parent ability " + newOverride.ParentName + " in action abilities for this override, needs to be specified.  File: " + filename);
								flag = false;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(filename, "Ability Override was null.  File: " + filename);
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
				case "GapAbove":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapAbove", filename, out var value6))
					{
						num = value6;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "GapBelow":
				{
					if (YMLShared.GetIntPropertyValue(entry.Value, "GapBelow", filename, out var value4))
					{
						num2 = value4;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "LayoutPriorTo":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "LayoutPriorTo", filename, out var layoutPriorToValue))
					{
						if (actionAbilities.SingleOrDefault((CAbility x) => x.Name == layoutPriorToValue) != null)
						{
							consumeLayoutProperties.LayoutPriorTo = layoutPriorToValue;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to find LayoutPriorTo ability " + layoutPriorToValue + ".  File: " + filename);
						flag = false;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PreviewEffectId":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, name + "/PreviewEffectId", filename, out var value5))
					{
						previewEffectId = value5;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "PreviewEffectText":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, name + "/PreviewEffectText", filename, out var value3))
					{
						previewEffectText = value3;
					}
					else
					{
						flag = false;
					}
					break;
				}
				case "ConsumeGroup":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Ability/Targeting", filename, out var value))
					{
						consumeGroup = value;
					}
					else
					{
						flag = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid consume entry " + entry.Key?.ToString() + ", File: " + filename);
					flag = false;
					break;
				}
			}
			if (num.HasValue)
			{
				consumeLayoutProperties.GapAbove = num.Value;
			}
			else if (!list3.Any((CActionAugmentationOp x) => x.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride) && consumeLayoutProperties.LayoutPriorTo != actionAbilities.First().Name)
			{
				consumeLayoutProperties.GapAbove = 50;
			}
			if (num2.HasValue)
			{
				consumeLayoutProperties.GapBelow = num2.Value;
			}
			else if (consumeLayoutProperties.LayoutPriorTo == actionAbilities.First().Name)
			{
				consumeLayoutProperties.GapBelow = 50;
			}
			if (flag && list != null && (list3.Count > 0 || list2.Count > 0))
			{
				CActionAugmentation consumeData = new CActionAugmentation(name, list, costAbility, list2, list3, xp, consumeLayoutProperties, previewEffectId, previewEffectText, consumeGroup);
				abilityConsume = new AbilityConsume(consumeData, filename);
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Consume Elements missing or invalid consume operation specified (did you forget to put parent ability?), File: " + filename);
				flag = false;
			}
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid Consume entry. File:\n" + filename + "\n" + ex.Message + "\n" + ex.StackTrace);
			flag = false;
		}
		return flag;
	}

	public static void GetAllActionAbilitiesRecursive(ref List<CAbility> allActionAbilities, ref List<CAbility> previouslyFoundAbilities)
	{
		List<CAbility> previouslyFoundAbilities2 = new List<CAbility>();
		foreach (CAbility previouslyFoundAbility in previouslyFoundAbilities)
		{
			previouslyFoundAbilities2.AddRange(previouslyFoundAbility.SubAbilities);
			if (previouslyFoundAbility is CAbilityMerged cAbilityMerged)
			{
				previouslyFoundAbilities2.AddRange(cAbilityMerged.MergedAbilities);
			}
			if (previouslyFoundAbility is CAbilityControlActor cAbilityControlActor)
			{
				previouslyFoundAbilities2.AddRange(cAbilityControlActor.ControlActorData.ControlAbilities);
			}
		}
		allActionAbilities.AddRange(previouslyFoundAbilities2);
		if (previouslyFoundAbilities2.Count > 0)
		{
			GetAllActionAbilitiesRecursive(ref allActionAbilities, ref previouslyFoundAbilities2);
		}
	}
}
