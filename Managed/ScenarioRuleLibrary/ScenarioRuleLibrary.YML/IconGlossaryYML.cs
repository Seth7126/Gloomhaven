using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace ScenarioRuleLibrary.YML;

public class IconGlossaryYML
{
	[Serializable]
	public enum EAttachTo
	{
		NA,
		Move,
		Attack
	}

	public class IconGlossaryEntry
	{
		public string Name { get; set; }

		public string Text { get; set; }

		public string Definition { get; set; }

		public string Sprite { get; set; }

		public EAttachTo AttachTo { get; set; }

		public bool? ShowSymbolNullable { get; set; }

		public bool? ShowSelfNullable { get; set; }

		public string FileName { get; private set; }

		public bool ShowSymbol
		{
			get
			{
				if (!ShowSymbolNullable.HasValue)
				{
					return false;
				}
				return ShowSymbolNullable.Value;
			}
		}

		public bool ShowSelf
		{
			get
			{
				if (!ShowSelfNullable.HasValue)
				{
					return false;
				}
				return ShowSelfNullable.Value;
			}
		}

		public IconGlossaryEntry()
		{
			Name = null;
			Text = null;
			Definition = null;
			Sprite = null;
			AttachTo = EAttachTo.NA;
			ShowSymbolNullable = null;
			ShowSelfNullable = null;
		}

		public IconGlossaryEntry(string name, string text, string definition, string sprite, EAttachTo attachTo, bool? showSymbol, bool? showSelf, string fileName)
		{
			Name = name;
			Text = text;
			Definition = definition;
			Sprite = sprite;
			AttachTo = attachTo;
			ShowSymbolNullable = showSymbol;
			ShowSelfNullable = showSelf;
			FileName = fileName;
		}

		public IconGlossaryEntry Copy()
		{
			return new IconGlossaryEntry(Name, Text, Definition, Sprite, AttachTo, ShowSymbolNullable, ShowSelfNullable, FileName);
		}

		public bool Validate()
		{
			if (Text == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryEntry", "Missing Text entry property in " + Name + ".");
				return false;
			}
			if (Definition == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryEntry", "Missing Definition entry property in " + Name + ".");
				return false;
			}
			if (Sprite == null)
			{
				SharedClient.ValidationRecord.RecordParseFailure("IconGlossaryEntry", "Missing Sprite entry property in " + Name + ".");
				return false;
			}
			return true;
		}

		public void UpdateEntry(IconGlossaryEntry entry)
		{
			if (entry.Text != null)
			{
				Text = entry.Text;
			}
			if (entry.Definition != null)
			{
				Definition = entry.Definition;
			}
			if (entry.Sprite != null)
			{
				Sprite = entry.Sprite;
			}
			if (entry.AttachTo != EAttachTo.NA)
			{
				AttachTo = entry.AttachTo;
			}
			if (entry.ShowSymbolNullable.HasValue)
			{
				ShowSymbolNullable = entry.ShowSymbolNullable;
			}
			if (entry.ShowSelfNullable.HasValue)
			{
				ShowSelfNullable = entry.ShowSelfNullable;
			}
		}
	}

	public static readonly EAttachTo[] AttachTos = (EAttachTo[])Enum.GetValues(typeof(EAttachTo));

	public const int MinimumFilesRequired = 1;

	public List<IconGlossaryEntry> Entries { get; private set; }

	public IconGlossaryYML()
	{
		Entries = new List<IconGlossaryEntry>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool result = true;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					if (entry.Key.ToString() == "Parser")
					{
						continue;
					}
					if (ProcessEntry(entry, fileName, out var outEntry))
					{
						IconGlossaryEntry iconGlossaryEntry = Entries.SingleOrDefault((IconGlossaryEntry s) => s.Name == outEntry.Name);
						if (iconGlossaryEntry == null)
						{
							Entries.Add(outEntry);
						}
						else
						{
							iconGlossaryEntry.UpdateEntry(outEntry);
						}
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	private bool ProcessEntry(MappingEntry glossaryEntry, string fileName, out IconGlossaryEntry outEntry)
	{
		bool result = true;
		outEntry = new IconGlossaryEntry();
		if (YMLShared.GetMapping(glossaryEntry, fileName, out var mapping))
		{
			outEntry.Name = glossaryEntry.Key.ToString();
			foreach (MappingEntry entry in mapping.Entries)
			{
				if (entry == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Icon Glossary Entry was null. File: " + fileName);
					result = false;
					continue;
				}
				switch (entry.Key.ToString())
				{
				case "Text":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Text", fileName, out var value3))
					{
						outEntry.Text = value3;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "Definition":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Definition", fileName, out var value4))
					{
						outEntry.Definition = value4;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "Sprite":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "Sprite", fileName, out var value))
					{
						outEntry.Sprite = value;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "AttachTo":
				{
					if (YMLShared.GetStringPropertyValue(entry.Value, "AttachTo", fileName, out var attachToString))
					{
						EAttachTo eAttachTo = AttachTos.SingleOrDefault((EAttachTo x) => x.ToString() == attachToString);
						if (eAttachTo != EAttachTo.NA)
						{
							outEntry.AttachTo = eAttachTo;
							break;
						}
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid AttachTo entry " + entry.Value?.ToString() + " in " + glossaryEntry.Key?.ToString() + ". File: " + fileName);
						result = false;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ShowSymbol":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowSymbol", fileName, out var value5))
					{
						outEntry.ShowSymbolNullable = value5;
					}
					else
					{
						result = false;
					}
					break;
				}
				case "ShowSelf":
				{
					if (YMLShared.GetBoolPropertyValue(entry.Value, "ShowSelf", fileName, out var value2))
					{
						outEntry.ShowSelfNullable = value2;
					}
					else
					{
						result = false;
					}
					break;
				}
				default:
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid glossary entry property " + entry.Key?.ToString() + " in " + glossaryEntry.Key?.ToString() + ". File: " + fileName);
					result = false;
					break;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}
}
