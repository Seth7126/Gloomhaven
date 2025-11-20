using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary;

public class CardLayout
{
	private readonly string FileName;

	private readonly string AbilityCardName;

	private readonly DiscardType DiscardType;

	public CardLayoutGroup ParentGroup { get; private set; }

	public List<CardLayoutGroup> PersistentAbilityLayouts { get; private set; }

	public CardLayout(string text, List<CAbility> linkedAbilities, string abilityCardName, DiscardType discard, string filename)
	{
		FileName = filename;
		AbilityCardName = abilityCardName;
		DiscardType = discard;
		PersistentAbilityLayouts = new List<CardLayoutGroup>();
		CardLayoutGroup cardLayoutGroup = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
		CardLayoutGroup cardLayoutGroup2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
		cardLayoutGroup2.CreateRowData(new CardLayoutRow(text, AbilityCardName, linkedAbilities), new CardLayoutGroup.RowAttributes());
		cardLayoutGroup.CreateRowCollection(new List<CardLayoutGroup> { cardLayoutGroup2 });
		ParentGroup = cardLayoutGroup;
	}

	public CardLayout(Sequence data, List<CAbility> linkedAbilities, List<AbilityConsume> consumes, string abilityCardName, DiscardType discard, string filename, List<CAbilityOverride> overrides = null)
	{
		FileName = filename;
		AbilityCardName = abilityCardName;
		DiscardType = discard;
		PersistentAbilityLayouts = new List<CardLayoutGroup>();
		CardLayoutGroup parent = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
		CreateChildren(data, ref parent, linkedAbilities, consumes, new CardLayoutGroup.RowAttributes(), overrides);
		ParentGroup = parent;
	}

	private void CreateChildren(DataItem dataItem, ref CardLayoutGroup parent, List<CAbility> linkedAbilities, List<AbilityConsume> consumes, CardLayoutGroup.RowAttributes rowAttributes, List<CAbilityOverride> overrides = null)
	{
		if (dataItem is Sequence)
		{
			Sequence sequence = dataItem as Sequence;
			List<MappingEntry> list = new List<MappingEntry>();
			List<MappingEntry> list2 = new List<MappingEntry>();
			List<MappingEntry> list3 = new List<MappingEntry>();
			List<MappingEntry> list4 = new List<MappingEntry>();
			int value = 0;
			List<int> list5 = new List<int>();
			List<CAbility> list6 = new List<CAbility>();
			if (linkedAbilities != null)
			{
				list6.AddRange(linkedAbilities);
			}
			if (consumes != null && consumes.Count > 0)
			{
				foreach (AbilityConsume consume in consumes)
				{
					foreach (CActionAugmentationOp augmentationOp in consume.ConsumeData.AugmentationOps)
					{
						if (augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.Ability)
						{
							list6.Add(augmentationOp.Ability);
						}
						else if (augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride && augmentationOp.AbilityOverride.SubAbilities != null && augmentationOp.AbilityOverride.SubAbilities.Count > 0)
						{
							list6.AddRange(augmentationOp.AbilityOverride.SubAbilities);
						}
					}
				}
			}
			foreach (DataItem entry in sequence.Entries)
			{
				MappingEntry mappingEntry = (entry as Mapping).Entries[0];
				switch (mappingEntry.Key.ToString())
				{
				case "ColumnSpacing":
					YMLShared.GetIntPropertyValue(mappingEntry.Value, mappingEntry.Key.ToString(), FileName, out value);
					break;
				case "ColumnWidths":
					list3.Add(mappingEntry);
					break;
				case "RowProperties":
					list4.Add(mappingEntry);
					break;
				case "Column":
					list.Add(mappingEntry);
					break;
				case "Row":
				case "Summon":
				case "Consume":
				case "Infuse":
				case "Augment":
				case "Doom":
				case "Song":
				case "Duration":
				case "XP":
				case "XPTracker":
					list2.Add(mappingEntry);
					break;
				case "Command":
					parent.IsCommand = true;
					break;
				case "Active":
					if (list6.Count <= 0)
					{
						break;
					}
					if (mappingEntry.Value is Mapping)
					{
						List<CAbility> list7 = new List<CAbility>();
						CardLayoutGroup parent2 = null;
						CardLayoutGroup parent3 = null;
						List<string> values = new List<string>();
						List<string> values2 = new List<string>();
						List<string> values3 = new List<string>();
						bool flag = false;
						foreach (MappingEntry entry2 in (mappingEntry.Value as Mapping).Entries)
						{
							switch (entry2.Key.ToString())
							{
							case "Ability":
							{
								if (!YMLShared.GetStringList(entry2.Value, "Ability", FileName, out var values4))
								{
									break;
								}
								foreach (string item in values4)
								{
									list7.Add(GetFromLinkedAbilities(item, list6));
								}
								break;
							}
							case "Layout":
								list2.Add(entry2);
								if (entry2.Value is Sequence)
								{
									parent2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
									CreateChildren(entry2.Value, ref parent2, linkedAbilities, new List<AbilityConsume>(), new CardLayoutGroup.RowAttributes(), overrides);
								}
								else if (entry2.Value is Scalar)
								{
									parent2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
									CardLayoutGroup cardLayoutGroup2 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
									cardLayoutGroup2.CreateRowData(new CardLayoutRow((entry2.Value as Scalar).Text, AbilityCardName, list6, overrides), new CardLayoutGroup.RowAttributes());
									parent2.CreateRowCollection(new List<CardLayoutGroup> { cardLayoutGroup2 });
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid layout entry. The entries under Active Bonus Layout must be a Scalar or a Sequence.\nFile: " + FileName);
								}
								break;
							case "LayoutAlt":
								if (entry2.Value is Sequence)
								{
									parent3 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
									CreateChildren(entry2.Value, ref parent3, linkedAbilities, new List<AbilityConsume>(), new CardLayoutGroup.RowAttributes(), overrides);
								}
								else if (entry2.Value is Scalar)
								{
									parent3 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
									CardLayoutGroup cardLayoutGroup = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
									cardLayoutGroup.CreateRowData(new CardLayoutRow((entry2.Value as Scalar).Text, AbilityCardName, list6, overrides), new CardLayoutGroup.RowAttributes());
									parent3.CreateRowCollection(new List<CardLayoutGroup> { cardLayoutGroup });
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid layout entry. The entries under Active Bonus Layout must be a Scalar or a Sequence.\nFile: " + FileName);
								}
								break;
							case "ListLayout":
								YMLShared.GetStringList(entry2.Value, "ListLayout", FileName, out values);
								values = values.Select((string x) => new CardLayoutRow(x, AbilityCardName, linkedAbilities, overrides).PropertyLookup(x, linkedAbilities)).ToList();
								break;
							case "ListLayoutAlt":
								YMLShared.GetStringList(entry2.Value, "ListLayoutAlt", FileName, out values2);
								values2 = values2.Select((string x) => new CardLayoutRow(x, AbilityCardName, linkedAbilities, overrides).PropertyLookup(x, linkedAbilities)).ToList();
								break;
							case "Icon":
							case "Icons":
								YMLShared.GetStringList(entry2.Value, "Icon", FileName, out values3);
								break;
							case "HideTracker":
							{
								if (YMLShared.GetBoolPropertyValue(entry2.Value, "HideTracker", FileName, out var value2))
								{
									flag = value2;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry " + entry2.Key.ToString() + ".\nFile: " + FileName);
								break;
							}
						}
						foreach (CAbility item2 in list7)
						{
							if (item2 != null && item2.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
							{
								item2.ActiveBonusYML = new ActiveBonusLayout(AbilityCardName, values3, parent2, parent3, values, values2, flag ? new List<int>() : item2.ActiveBonusData.Tracker, DiscardType);
							}
						}
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active entries in Layout section must be Mapping type.\nFile: " + FileName);
					}
					break;
				default:
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry '" + mappingEntry.Key.ToString() + "' within layout section.  File: " + FileName);
					break;
				}
			}
			if (list2.Count > 0 && list.Count > 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Rows and Columns are not able to exist together at the same level.  File: " + FileName);
				return;
			}
			if (value < 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Column spacing cannot be negative, setting to 0.  File: " + FileName);
				value = 0;
				return;
			}
			if (list3.Count > 0 && list3.Count == 0)
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Column Widths specified but no columns to apply them to.  File\n" + FileName);
				list3.Clear();
			}
			if (list3.Count > 0 && list3[0].Value is Scalar)
			{
				string text = (list3[0].Value as Scalar).Text;
				_ = text == "*";
				string[] array = text.Split(',');
				foreach (string text2 in array)
				{
					if (int.TryParse(text2, out var result))
					{
						list5.Add(result);
						continue;
					}
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "ColumnWidths definitions are invalid.  Could not parse value " + text2 + " to integers. File:\n" + FileName);
					list5.Clear();
				}
				if (list5.Count != list.Count)
				{
					SharedClient.ValidationRecord.RecordParseFailure(FileName, "ColumnWidths definitions are invalid.  Number of entries does not match the number of columns.  File:\n" + FileName);
				}
			}
			if (list4.Count > 0 && list4[0].Value is Sequence)
			{
				rowAttributes = GetRowAttributes(list4[0].Value as Sequence);
			}
			if (list.Count > 0)
			{
				List<CardLayoutGroup> list8 = new List<CardLayoutGroup>();
				foreach (MappingEntry item3 in list)
				{
					CardLayoutGroup parent4 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Column);
					if (item3.Value is Sequence)
					{
						CreateChildren(item3.Value, ref parent4, linkedAbilities, consumes, new CardLayoutGroup.RowAttributes(), overrides);
						list8.Add(parent4);
					}
					else if (item3.Value is Scalar)
					{
						List<CardLayoutGroup> list9 = new List<CardLayoutGroup>();
						CardLayoutGroup cardLayoutGroup3 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
						cardLayoutGroup3.CreateRowData(new CardLayoutRow((item3.Value as Scalar).Text, AbilityCardName, list6, overrides), rowAttributes);
						list9.Add(cardLayoutGroup3);
						parent4.CreateRowCollection(list9);
						list8.Add(parent4);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process column value " + item3.Value.ToString() + "  File\n" + FileName);
					}
				}
				parent.CreateColumnCollection(list8, new CardLayoutGroup.ColumnAttributes(list5.ToArray(), value));
			}
			else if (list2.Count > 0)
			{
				List<CardLayoutGroup> list10 = new List<CardLayoutGroup>();
				foreach (MappingEntry item4 in list2)
				{
					switch (item4.Key.ToString())
					{
					case "Summon":
					{
						CAbility cAbility2 = null;
						string value8 = string.Empty;
						CardLayout specialText = null;
						bool hideSummonBoxes = false;
						foreach (MappingEntry summonEntry in (item4.Value as Mapping).Entries)
						{
							switch (summonEntry.Key.ToString())
							{
							case "Ability":
								try
								{
									cAbility2 = list6.Single((CAbility x) => x.Name == summonEntry.Value.ToString() && x.AbilityType == CAbility.EAbilityType.Summon);
								}
								catch (Exception ex)
								{
									DLLDebug.LogError("Referenced Summon ability in Layout section '" + item4.Value?.ToString() + "' not found in Data section. File: " + FileName + ex.Message + "\n" + ex.StackTrace);
								}
								break;
							case "SummonName":
								YMLShared.GetStringPropertyValue(summonEntry.Value, "Summon/SummonName", FileName, out value8);
								break;
							case "SpecialText":
								if (summonEntry.Value is Sequence)
								{
									if (YMLShared.GetSequence(summonEntry.Value, "Summon/SpecialText", FileName, out var sequence4))
									{
										specialText = new CardLayout(sequence4, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (summonEntry.Value is Scalar)
								{
									specialText = new CardLayout((summonEntry.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "HideSummonBoxes":
							{
								if (YMLShared.GetBoolPropertyValue(summonEntry.Value, "HideSummonBoxes", FileName, out var value9))
								{
									hideSummonBoxes = value9;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Summon entry " + summonEntry.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						if (cAbility2 != null)
						{
							CardLayoutGroup cardLayoutGroup7 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Summon);
							cardLayoutGroup7.CreateSummonData(new CardLayoutGroup.SummonLayout(cAbility2, value8, specialText, hideSummonBoxes));
							list10.Add(cardLayoutGroup7);
						}
						continue;
					}
					case "Augment":
					{
						string text7 = null;
						string discardText3 = null;
						CAbility cAbility4 = null;
						string text8 = null;
						CardLayout cardLayout4 = null;
						CardLayout cardLayout5 = null;
						foreach (MappingEntry entry3 in (item4.Value as Mapping).Entries)
						{
							switch (entry3.Key.ToString())
							{
							case "ParentAbility":
							{
								if (!YMLShared.GetStringPropertyValue(entry3.Value, "ParentAbility", FileName, out var value17))
								{
									break;
								}
								foreach (CAbility item5 in list6)
								{
									if (item5.Name == value17)
									{
										cAbility4 = item5;
										continue;
									}
									CAugment augment = item5.Augment;
									if (augment == null || augment.Abilities.Count <= 0)
									{
										continue;
									}
									foreach (CAbility ability in item5.Augment.Abilities)
									{
										if (ability.Name == value17)
										{
											cAbility4 = ability;
										}
									}
								}
								if (cAbility4 == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + value17 + " not found in linked abilities.\nFile: " + FileName);
								}
								break;
							}
							case "ActiveBonusLayout":
							{
								if (YMLShared.GetStringPropertyValue(entry3.Value, "ActiveBonusLayout", FileName, out var value15))
								{
									text8 = new CardLayoutRow(value15, AbilityCardName, linkedAbilities, overrides).PropertyLookup(value15, linkedAbilities);
								}
								break;
							}
							case "AugmentAbilityLayout":
								if (entry3.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry3.Value, "Augment/AugmentAbilityLayout", FileName, out var sequence7))
									{
										cardLayout4 = new CardLayout(sequence7, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry3.Value is Scalar)
								{
									cardLayout4 = new CardLayout((entry3.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "NormalContentLayout":
								if (entry3.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry3.Value, "Augment/NormalContentLayout", FileName, out var sequence6))
									{
										cardLayout5 = new CardLayout(sequence6, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry3.Value is Scalar)
								{
									cardLayout5 = new CardLayout((entry3.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "AugmentIcon":
							{
								if (YMLShared.GetStringPropertyValue(entry3.Value, "AugmentIcon", FileName, out var value18))
								{
									text7 = value18;
								}
								break;
							}
							case "DiscardText":
							{
								if (YMLShared.GetStringPropertyValue(entry3.Value, "DiscardText", FileName, out var value16))
								{
									discardText3 = value16;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Augment entry " + entry3.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						bool flag4 = true;
						if (cAbility4?.Augment == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility name (ability with Augment within, not an augment's inner ability) required for Augment in layout section.\nFile: " + FileName);
							flag4 = false;
						}
						if (text8 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ListLayout entry required for active bonus to display in Augment in layout section.\nFile: " + FileName);
							flag4 = false;
						}
						if (cardLayout4 == null || cardLayout5 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "Both AugmentAbilityLayout and NormalContentLayout required for Augment in layout section.\nFile: " + FileName);
							flag4 = false;
						}
						if (text7 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "AugmentIcon required for Augemnt in layout section. \nFile: " + FileName);
						}
						if (flag4)
						{
							cAbility4.Augment.ActiveBonusLayout = text8;
							CardLayoutGroup cardLayoutGroup12 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Augment);
							cardLayoutGroup12.CreateAugmentData(new CardLayoutGroup.AugmentLayout(text7, discardText3, cardLayout4, cardLayout5));
							list10.Add(cardLayoutGroup12);
						}
						continue;
					}
					case "Doom":
					{
						CAbility cAbility = null;
						string text3 = null;
						CardLayout cardLayout = null;
						string reminderTextOverride = null;
						string text4 = null;
						string discardText = null;
						foreach (MappingEntry entry4 in (item4.Value as Mapping).Entries)
						{
							switch (entry4.Key.ToString())
							{
							case "ParentAbility":
							{
								if (!YMLShared.GetStringPropertyValue(entry4.Value, "ParentAbility", FileName, out var value5))
								{
									break;
								}
								foreach (CAbility item6 in list6)
								{
									if (item6.Name == value5)
									{
										cAbility = item6;
									}
								}
								if (cAbility == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + value5 + " not found in linked abilities.\nFile: " + FileName);
								}
								break;
							}
							case "ActiveBonusLayout":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "ActiveBonusLayout", FileName, out var value4))
								{
									text3 = new CardLayoutRow(value4, AbilityCardName, linkedAbilities, overrides).PropertyLookup(value4, linkedAbilities);
								}
								break;
							}
							case "DoomAbilityLayout":
								if (entry4.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry4.Value, "Doom/DoomAbilityLayout", FileName, out var sequence2))
									{
										cardLayout = new CardLayout(sequence2, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry4.Value is Scalar)
								{
									cardLayout = new CardLayout((entry4.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "ReminderTextOverride":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "ReminderTextOverride", FileName, out var value3))
								{
									reminderTextOverride = value3;
								}
								break;
							}
							case "DoomIcon":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "DoomIcon", FileName, out var value7))
								{
									text4 = value7;
								}
								break;
							}
							case "DiscardText":
							{
								if (YMLShared.GetStringPropertyValue(entry4.Value, "DoomIcon", FileName, out var value6))
								{
									discardText = value6;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Doom entry " + entry4.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						bool flag2 = true;
						if (!(cAbility is CAbilityAddDoom))
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility not AddDoom type as required for Doom in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if ((cAbility as CAbilityAddDoom)?.Doom == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility name with valid Doom required for Doom in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (text3 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ListLayout entry required for active bonus to display in Doom in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (cardLayout == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "DoomAbilityLayout required for Doom in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (text4 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "DoomIcon required for Doom in layout section.\nFile: " + FileName);
							flag2 = false;
						}
						if (flag2)
						{
							(cAbility as CAbilityAddDoom).Doom.ActiveBonusLayout = text3;
							CardLayoutGroup cardLayoutGroup5 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Doom);
							cardLayoutGroup5.CreateDoomData(new CardLayoutGroup.DoomLayout(text4, discardText, cardLayout, reminderTextOverride));
							list10.Add(cardLayoutGroup5);
						}
						continue;
					}
					case "Song":
					{
						CAbility cAbility3 = null;
						string text6 = null;
						CardLayout cardLayout3 = null;
						string songIcon = null;
						string discardText2 = null;
						foreach (MappingEntry entry5 in (item4.Value as Mapping).Entries)
						{
							switch (entry5.Key.ToString())
							{
							case "ParentAbility":
							{
								if (!YMLShared.GetStringPropertyValue(entry5.Value, "ParentAbility", FileName, out var value13))
								{
									break;
								}
								foreach (CAbility item7 in list6)
								{
									if (item7.Name == value13)
									{
										cAbility3 = item7;
									}
								}
								if (cAbility3 == null)
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + value13 + " not found in linked abilities.\nFile: " + FileName);
								}
								break;
							}
							case "ActiveBonusLayout":
							{
								if (YMLShared.GetStringPropertyValue(entry5.Value, "ActiveBonusLayout", FileName, out var value10))
								{
									text6 = new CardLayoutRow(value10, AbilityCardName, linkedAbilities, overrides).PropertyLookup(value10, linkedAbilities);
								}
								break;
							}
							case "SongAbilityLayout":
								if (entry5.Value is Sequence)
								{
									if (YMLShared.GetSequence(entry5.Value, "Song/SongAbilityLayout", FileName, out var sequence5))
									{
										cardLayout3 = new CardLayout(sequence5, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName);
									}
								}
								else if (entry5.Value is Scalar)
								{
									cardLayout3 = new CardLayout((entry5.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
								}
								break;
							case "SongIcon":
							{
								if (YMLShared.GetStringPropertyValue(entry5.Value, "SongIcon", FileName, out var value11))
								{
									songIcon = value11;
								}
								break;
							}
							case "DiscardText":
							{
								if (YMLShared.GetStringPropertyValue(entry5.Value, "DoomIcon", FileName, out var value12))
								{
									discardText2 = value12;
								}
								break;
							}
							default:
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Song entry " + entry5.Key?.ToString() + " in layout section.\nFile: " + FileName);
								break;
							}
						}
						bool flag3 = true;
						if (cAbility3?.Song == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ParentAbility name with valid Song required for Song in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (text6 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "ListLayout entry required for active bonus to display in Song in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (cardLayout3 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "SongAbilityLayout required for Song in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (cardLayout3 == null)
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "SongIcon required for Song in layout section.\nFile: " + FileName);
							flag3 = false;
						}
						if (flag3)
						{
							cAbility3.Song.ActiveBonusLayout = text6;
							CardLayoutGroup cardLayoutGroup9 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Song);
							cardLayoutGroup9.CreateSongData(new CardLayoutGroup.SongLayout(songIcon, discardText2, cardLayout3));
							list10.Add(cardLayoutGroup9);
						}
						continue;
					}
					case "Consume":
					{
						string name = string.Empty;
						CardLayout cardLayout2 = null;
						foreach (MappingEntry entry6 in (item4.Value as Mapping).Entries)
						{
							string text5 = entry6.Key.ToString();
							if (!(text5 == "Name"))
							{
								if (text5 == "Text")
								{
									if (entry6.Value is Sequence)
									{
										if (YMLShared.GetSequence(entry6.Value, "Consume/Text", FileName, out var sequence3))
										{
											cardLayout2 = new CardLayout(sequence3, linkedAbilities, consumes, AbilityCardName, DiscardType, FileName, overrides);
										}
									}
									else if (entry6.Value is Scalar)
									{
										cardLayout2 = new CardLayout((entry6.Value as Scalar).Text, linkedAbilities, AbilityCardName, DiscardType, FileName);
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid entry '" + entry6.Key.ToString() + "' under consumes in file " + FileName);
								}
							}
							else
							{
								YMLShared.GetStringPropertyValue(entry6.Value, "Consume/Name", FileName, out name);
							}
						}
						if (name != string.Empty && cardLayout2 != null && consumes != null)
						{
							AbilityConsume abilityConsume = consumes.SingleOrDefault((AbilityConsume x) => x.Name == name);
							if (abilityConsume != null)
							{
								CardLayoutGroup cardLayoutGroup6 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Consume);
								abilityConsume.Text = cardLayout2;
								cardLayoutGroup6.CreateConsumeData(abilityConsume);
								list10.Add(cardLayoutGroup6);
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to find Consume data for entry " + name + " in file " + FileName);
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Consume entry within Layout section of file " + FileName);
						}
						continue;
					}
					case "Infuse":
					{
						CardLayoutGroup cardLayoutGroup10 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Element);
						if (CardProcessingShared.GetElements(item4.Value, "Infuse", FileName, out var elements))
						{
							cardLayoutGroup10.CreateElementData(elements);
							list10.Add(cardLayoutGroup10);
						}
						continue;
					}
					case "Duration":
					{
						if (YMLShared.GetStringPropertyValue(item4.Value, "Duration", FileName, out var durationString))
						{
							CActiveBonus.EActiveBonusDurationType eActiveBonusDurationType = CActiveBonus.ActiveBonusDurationTypes.SingleOrDefault((CActiveBonus.EActiveBonusDurationType x) => x.ToString() == durationString);
							if (eActiveBonusDurationType != CActiveBonus.EActiveBonusDurationType.NA)
							{
								CardLayoutGroup cardLayoutGroup8 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Duration);
								cardLayoutGroup8.CreateDurationData(eActiveBonusDurationType);
								list10.Add(cardLayoutGroup8);
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid Active Bonus Duration '" + durationString + "' in file '" + FileName);
							}
						}
						continue;
					}
					case "XP":
					{
						CardLayoutGroup cardLayoutGroup4 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.XP);
						if (YMLShared.GetIntList(item4.Value, "XP", FileName, out var values5))
						{
							cardLayoutGroup4.CreateXPData(values5);
							list10.Add(cardLayoutGroup4);
						}
						continue;
					}
					case "XPTracker":
					{
						if (YMLShared.GetStringPropertyValue(item4.Value, "XPTracker", FileName, out var value14))
						{
							CAbility fromLinkedAbilities = GetFromLinkedAbilities(value14, list6);
							if (fromLinkedAbilities?.ActiveBonusData?.Tracker != null)
							{
								CardLayoutGroup cardLayoutGroup11 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.XP);
								cardLayoutGroup11.CreateXPData(fromLinkedAbilities.ActiveBonusData.Tracker);
								list10.Add(cardLayoutGroup11);
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to find linked ability and/or active bonus tracker for ability named " + value14 + "  File\n" + FileName);
							}
						}
						continue;
					}
					}
					CardLayoutGroup parent5 = new CardLayoutGroup(CardLayoutGroup.GroupTypes.Row);
					if (overrides == null && consumes != null)
					{
						IEnumerable<CActionAugmentationOp> source = consumes.SelectMany((AbilityConsume x) => x.ConsumeData.AugmentationOps.Where((CActionAugmentationOp y) => y.AbilityOverride != null));
						overrides = (from x in source
							where x.AbilityOverride != null
							select x.AbilityOverride).ToList();
					}
					if (item4.Value is Sequence)
					{
						if (IsRowDataSequence(item4.Value as Sequence))
						{
							CardLayoutGroup.RowAttributes rowAttributes2 = null;
							CardLayoutRow cardLayoutRow = null;
							foreach (DataItem entry7 in (item4.Value as Sequence).Entries)
							{
								MappingEntry mappingEntry2 = (entry7 as Mapping).Entries[0];
								if (mappingEntry2.Key.ToString() == "RowProperties")
								{
									rowAttributes2 = GetRowAttributes(mappingEntry2.Value as Sequence);
								}
								else if (mappingEntry2.Key.ToString() == "Data")
								{
									cardLayoutRow = new CardLayoutRow((mappingEntry2.Value as Scalar).Text, AbilityCardName, list6, overrides);
								}
							}
							if (cardLayoutRow != null)
							{
								parent5.CreateRowData(cardLayoutRow, rowAttributes2 ?? rowAttributes);
								list10.Add(parent5);
							}
						}
						else
						{
							CreateChildren(item4.Value, ref parent5, linkedAbilities, consumes, new CardLayoutGroup.RowAttributes(), overrides);
							list10.Add(parent5);
						}
					}
					else if (item4.Value is Scalar)
					{
						parent5.CreateRowData(new CardLayoutRow((item4.Value as Scalar).Text, AbilityCardName, list6, overrides), rowAttributes);
						list10.Add(parent5);
					}
					else
					{
						SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process row value " + item4.Value.ToString() + "  File\n" + FileName);
					}
				}
				parent.CreateRowCollection(list10);
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "No entries found to process.  File\n" + FileName);
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unexpected entry in Layout File:\n" + FileName + "\nData Item is not a Sequence or Scalar.\n" + dataItem.ToString());
		}
	}

	private CAbility GetFromLinkedAbilities(string abilityName, List<CAbility> allLinked)
	{
		CAbility result = null;
		if (allLinked.Any((CAbility x) => x.Name == abilityName))
		{
			result = allLinked.Single((CAbility x) => x.Name == abilityName);
		}
		else if (allLinked.SelectMany((CAbility sm) => sm.SubAbilities).Any((CAbility a) => a.Name == abilityName))
		{
			result = allLinked.SelectMany((CAbility sm) => sm.SubAbilities).Single((CAbility s) => s.Name == abilityName);
		}
		else if (allLinked.Where((CAbility x) => x.Augment != null && x.Augment.Abilities != null && x.Augment.Abilities.Count > 0).SelectMany((CAbility sm) => sm.Augment.Abilities).Any((CAbility a) => a.Name == abilityName))
		{
			result = allLinked.Where((CAbility x) => x.Augment != null && x.Augment.Abilities != null && x.Augment.Abilities.Count > 0).SelectMany((CAbility sm) => sm.Augment.Abilities).Single((CAbility s) => s.Name == abilityName);
		}
		else if (allLinked.Where((CAbility x) => x is CAbilityAddDoom { Doom: not null } cAbilityAddDoom && cAbilityAddDoom.Doom.DoomAbilities.Count > 0).SelectMany((CAbility sm) => (sm as CAbilityAddDoom).Doom.DoomAbilities).Any((CAbility a) => a.Name == abilityName))
		{
			result = allLinked.Where((CAbility x) => x is CAbilityAddDoom { Doom: not null } cAbilityAddDoom && cAbilityAddDoom.Doom.DoomAbilities.Count > 0).SelectMany((CAbility sm) => (sm as CAbilityAddDoom).Doom.DoomAbilities).Single((CAbility a) => a.Name == abilityName);
		}
		else if (allLinked.Where((CAbility x) => x is CAbilityControlActor).SelectMany((CAbility sm) => (sm as CAbilityControlActor).ControlActorData.ControlAbilities).Any((CAbility s) => s.Name == abilityName))
		{
			allLinked.Where((CAbility x) => x is CAbilityControlActor).SelectMany((CAbility sm) => (sm as CAbilityControlActor).ControlActorData.ControlAbilities).Single((CAbility s) => s.Name == abilityName);
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Active ability name " + abilityName + " not found in linked abilities.\nFile: " + FileName);
		}
		return result;
	}

	private CardLayoutGroup.RowAttributes GetRowAttributes(Sequence rowAttSequence)
	{
		CardLayoutGroup.RowAttributes rowAttributes = new CardLayoutGroup.RowAttributes();
		foreach (Mapping entry in rowAttSequence.Entries)
		{
			MappingEntry mappingEntry = entry.Entries[0];
			switch (mappingEntry.Key.ToString())
			{
			case "AutoSize":
			{
				if (YMLShared.GetBoolPropertyValue(mappingEntry.Value, "RowProperties/AutoSize", FileName, out var value4))
				{
					rowAttributes.AutoSize = value4;
				}
				break;
			}
			case "AutoSizeMin":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/AutoSizeMin", FileName, out var value8))
				{
					rowAttributes.AutoSizeMin = value8;
				}
				break;
			}
			case "AutoSizeMax":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/AutoSizeMax", FileName, out var value10))
				{
					rowAttributes.AutoSizeMax = value10;
				}
				break;
			}
			case "CharSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/CharSpacing", FileName, out var value6))
				{
					rowAttributes.CharSpacing = value6;
				}
				break;
			}
			case "WordSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/WordSpacing", FileName, out var value2))
				{
					rowAttributes.WordSpacing = value2;
				}
				break;
			}
			case "LineSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/LineSpacing", FileName, out var value9))
				{
					rowAttributes.LineSpacing = value9;
				}
				break;
			}
			case "ParSpacing":
			{
				if (YMLShared.GetIntPropertyValue(mappingEntry.Value, "RowProperties/ParSpacing", FileName, out var value7))
				{
					rowAttributes.ParSpacing = value7;
				}
				break;
			}
			case "Alignment":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "RowProperties/Alignment", FileName, out var value5))
				{
					rowAttributes.Alignment = value5;
				}
				break;
			}
			case "Wrapping":
			{
				if (YMLShared.GetBoolPropertyValue(mappingEntry.Value, "RowProperties/Wrapping", FileName, out var value3))
				{
					rowAttributes.Wrapping = value3;
				}
				break;
			}
			case "Overflow":
			{
				if (YMLShared.GetStringPropertyValue(mappingEntry.Value, "RowProperties/Overflow", FileName, out var value))
				{
					rowAttributes.Overflow = value;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(FileName, "Unable to process RowAttributes entry " + mappingEntry.Key.ToString() + "in file: " + FileName);
				break;
			}
		}
		return rowAttributes;
	}

	private bool IsRowDataSequence(Sequence sequence)
	{
		if (sequence.Entries.Count > 0 && sequence.Entries[0] is Mapping mapping && mapping.Entries.Count > 0 && mapping.Entries.Any((MappingEntry x) => x.Key.ToString() == "Data"))
		{
			return true;
		}
		return false;
	}
}
