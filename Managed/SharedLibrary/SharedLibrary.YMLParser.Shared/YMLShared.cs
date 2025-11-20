using System.Collections.Generic;
using System.Globalization;
using SharedLibrary.Client;
using YamlFormats;

namespace SharedLibrary.YMLParser.Shared;

public class YMLShared
{
	public static int ParseIntValue(string text, string key, string filename)
	{
		if (int.TryParse(text, out var result))
		{
			return result;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse value of " + key + " as an integer in file " + filename);
		return -1;
	}

	public static bool GetMapping(MappingEntry mappingEntry, string filename, out Mapping mapping)
	{
		if (mappingEntry.Value is Mapping)
		{
			mapping = mappingEntry.Value as Mapping;
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse file.  Mapping entry value for key " + mappingEntry.Key?.ToString() + " is not Mapping type.  File:" + filename);
		mapping = null;
		return false;
	}

	public static Mapping GetMapping(MappingEntry mappingEntry, string filename)
	{
		if (mappingEntry.Value is Mapping)
		{
			return mappingEntry.Value as Mapping;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse file.  Mapping entry value for key " + mappingEntry.Key?.ToString() + " is not Mapping type.  File:" + filename);
		return null;
	}

	public static bool GetIntPropertyValue(DataItem entry, string entryName, string filename, out int value, bool suppressErrors = false)
	{
		value = 0;
		if (entry is Scalar && int.TryParse((entry as Scalar).Text, out value))
		{
			return true;
		}
		if (!suppressErrors)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry.ToString() + " in " + entryName + " entry.  File:\n" + filename);
		}
		return false;
	}

	public static string GetStringPropertyValue(MappingEntry entry, string filename)
	{
		string value = null;
		GetStringPropertyValue(entry.Value, entry.Key.ToString(), filename, out value);
		return value;
	}

	public static bool GetStringPropertyValue(DataItem entry, string entryName, string filename, out string value)
	{
		if (entry is Scalar)
		{
			value = (entry as Scalar).Text;
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry.ToString() + " in " + entryName + " entry.  File:\n" + filename);
		value = string.Empty;
		return false;
	}

	public static bool GetBoolPropertyValue(MappingEntry entry, string filename, bool orig = false)
	{
		bool value = orig;
		GetBoolPropertyValue(entry.Value, entry.Key.ToString(), filename, out value);
		return value;
	}

	public static bool GetBoolPropertyValue(DataItem entry, string entryName, string filename, out bool value)
	{
		if (entry is Scalar)
		{
			string text = (entry as Scalar).Text.ToLower();
			if (text == "true")
			{
				value = true;
				return true;
			}
			if (text == "false")
			{
				value = false;
				return true;
			}
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid bool entry " + entryName + ".  Must be either True or False.  File:\n" + filename);
			value = false;
			return false;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry.ToString() + " in " + entryName + " entry.  File:\n" + filename);
		value = false;
		return false;
	}

	public static bool GetFloatPropertyValue(DataItem entry, string entryName, string filename, out float value)
	{
		value = 0f;
		if (entry is Scalar && float.TryParse((entry as Scalar).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
		{
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Invalid entry " + entry.ToString() + " in " + entryName + " entry.  File:\n" + filename);
		return false;
	}

	public static bool GetSequence(DataItem di, string entryName, string filename, out Sequence sequence)
	{
		if (di is Sequence)
		{
			sequence = di as Sequence;
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Entry under " + entryName + " is invalid, must be a Sequence.  File:\n" + filename);
		sequence = null;
		return false;
	}

	public static bool GetIntList(DataItem entry, string entryName, string filename, out List<int> values, bool allowScalar = false, int scalarDupeFactor = 1)
	{
		values = new List<int>();
		bool result = true;
		if (entry is Sequence)
		{
			foreach (DataItem entry2 in (entry as Sequence).Entries)
			{
				if (GetIntPropertyValue(entry2, entryName, filename, out var value))
				{
					values.Add(value);
				}
				else
				{
					result = false;
				}
			}
		}
		else if (allowScalar && entry is Scalar)
		{
			result = GetIntPropertyValue(entry, entryName, filename, out var value2);
			for (int i = 0; i < scalarDupeFactor; i++)
			{
				values.Add(value2);
			}
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Entry " + entryName + " is not a sequence.  Unable to process. File: " + filename);
			result = false;
		}
		return result;
	}

	public static bool GetStringList(DataItem entry, string entryName, string filename, out List<string> values, bool allowScalar = false)
	{
		values = new List<string>();
		bool result = true;
		if (entry is Sequence)
		{
			foreach (DataItem entry2 in (entry as Sequence).Entries)
			{
				if (GetStringPropertyValue(entry2, entryName, filename, out var value))
				{
					values.Add(value);
				}
				else
				{
					result = false;
				}
			}
		}
		else if (allowScalar && entry is Scalar)
		{
			values.Add((entry as Scalar).Text);
		}
		else
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Entry " + entryName + " is not a sequence/scalar.  Unable to process.  File: " + filename);
			result = false;
		}
		return result;
	}
}
