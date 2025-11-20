using System;
using System.Collections.Generic;
using System.Globalization;
using SharedLibrary.Client;
using YamlFormats;

namespace SharedLibrary.YML;

public class YMLShared
{
	public static int GetDeterministicHashCode(string str)
	{
		int num = 352654597;
		int num2 = num;
		for (int i = 0; i < str.Length; i += 2)
		{
			num = ((num << 5) + num) ^ str[i];
			if (i == str.Length - 1)
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ str[i + 1];
		}
		return num + num2 * 1566083941;
	}

	public static bool ParseIntValue(string text, string key, string filename, out int value)
	{
		if (int.TryParse(text, out value))
		{
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse value of " + key + " as an integer in file " + filename);
		return false;
	}

	public static bool GetMapping(MappingEntry mappingEntry, string filename, out Mapping mapping, bool suppressErrors = false)
	{
		mapping = null;
		if (mappingEntry.Value is Mapping)
		{
			mapping = mappingEntry.Value as Mapping;
			return true;
		}
		if (!suppressErrors)
		{
			SharedClient.ValidationRecord.RecordParseFailure(filename, "Unable to parse file.  Mapping entry value for key " + mappingEntry.Key?.ToString() + " is not Mapping type.  File:" + filename);
		}
		return false;
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

	public static bool GetSequence(MappingEntry entry, string filename, out Sequence sequence)
	{
		return GetSequence(entry.Value, entry.Key.ToString(), filename, out sequence);
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

	public static bool GetIntArray(DataItem entry, string entryName, string filename, int arraySize, out int[] outArray, bool ignorePositionZero = false)
	{
		outArray = null;
		bool result = true;
		if (GetIntList(entry, entryName, filename, out var values, allowScalar: false))
		{
			if (!ignorePositionZero && values.Count == arraySize)
			{
				outArray = values.ToArray();
			}
			else if (ignorePositionZero && values.Count == arraySize - 1)
			{
				values.Insert(0, 1);
				outArray = values.ToArray();
			}
			else
			{
				SharedClient.ValidationRecord.RecordParseFailure(filename, "Entry " + entryName + " is does not have the required number of entries.  Unable to process. File: " + filename);
				result = false;
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	public static bool GetIntList(DataItem entry, string entryName, string filename, out List<int> values, bool allowScalar = true, int scalarDupeFactor = 1)
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

	public static bool GetStringList(DataItem entry, string entryName, string filename, out List<string> values, bool allowScalar = true)
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

	public static bool GetTupleStringInt(DataItem di, string entryName, string fileName, out Tuple<string, int> tuple)
	{
		tuple = null;
		if (di is Sequence sequence && sequence.Entries.Count == 2)
		{
			if (!int.TryParse((sequence.Entries[1] as Scalar).ToString(), out var result))
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ". Second value of pair, must be an integer. File: " + fileName);
				return false;
			}
			tuple = new Tuple<string, int>((sequence.Entries[0] as Scalar).ToString(), result);
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Must be list of [string, int] pairs. File: " + fileName);
		return false;
	}

	public static bool GetTupleIntString(DataItem di, string entryName, string fileName, out Tuple<int, string> tuple)
	{
		tuple = null;
		if (di is Sequence sequence && sequence.Entries.Count == 2)
		{
			if (!int.TryParse((sequence.Entries[0] as Scalar).ToString(), out var result))
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ". First value of pair, must be an integer. File: " + fileName);
				return false;
			}
			tuple = new Tuple<int, string>(result, (sequence.Entries[1] as Scalar).ToString());
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Must be list of [int, string] pairs. File: " + fileName);
		return false;
	}

	public static bool GetTupleStringString(DataItem di, string entryName, string fileName, out Tuple<string, string> tuple)
	{
		tuple = null;
		if (di is Sequence sequence && sequence.Entries.Count == 2)
		{
			tuple = new Tuple<string, string>((sequence.Entries[0] as Scalar).ToString(), (sequence.Entries[1] as Scalar).ToString());
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Must be list of [string, string] pairs. File: " + fileName);
		return false;
	}

	public static bool GetTupleStringListInt(DataItem di, string entryName, string fileName, out Tuple<string, List<int>> tuple)
	{
		tuple = null;
		if (di is Sequence sequence && sequence.Entries.Count == 5)
		{
			List<int> list = new List<int>();
			for (int i = 1; i < sequence.Entries.Count; i++)
			{
				if (!int.TryParse((sequence.Entries[i] as Scalar).ToString(), out var result))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Value for list int, must be an integer. File: " + fileName);
					return false;
				}
				list.Add(result);
			}
			tuple = new Tuple<string, List<int>>((sequence.Entries[0] as Scalar).ToString(), list);
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Must be list of [string, int, int, int, int] pairs. File: " + fileName);
		return false;
	}

	public static bool GetTupleStringTileIndex(DataItem di, string entryName, string fileName, out Tuple<string, List<int>> tuple)
	{
		tuple = null;
		if (di is Sequence sequence && sequence.Entries.Count == 3)
		{
			List<int> list = new List<int>();
			for (int i = 1; i < sequence.Entries.Count; i++)
			{
				if (!int.TryParse((sequence.Entries[i] as Scalar).ToString(), out var result))
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Value for list int, must be an integer. File: " + fileName);
					return false;
				}
				list.Add(result);
			}
			tuple = new Tuple<string, List<int>>((sequence.Entries[0] as Scalar).ToString(), list);
			return true;
		}
		SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid entry " + entryName + ".  Must be list of [string, int, int] pairs. File: " + fileName);
		return false;
	}
}
