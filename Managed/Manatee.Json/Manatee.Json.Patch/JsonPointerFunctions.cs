using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Internal;

namespace Manatee.Json.Patch;

internal static class JsonPointerFunctions
{
	private static readonly (JsonValue?, string?, int, bool) _empty = (null, null, -1, false);

	public static (JsonValue? parent, string? key, int index, bool success) ResolvePointer(JsonValue json, string path)
	{
		if (path == string.Empty)
		{
			return _empty;
		}
		IEnumerable<string> enumerable = path.Split(new char[1] { '/' }).Skip(1);
		JsonValue item = null;
		JsonValue jsonValue = json;
		string text = null;
		int result = -1;
		foreach (string item2 in enumerable)
		{
			text = item2.UnescapePointer();
			if (text.StartsWith("0") && text != "0")
			{
				result = -1;
			}
			else if (!int.TryParse(item2, out result))
			{
				result = ((text == "-") ? int.MaxValue : (-1));
			}
			if (jsonValue.Type == JsonValueType.Object)
			{
				if (!jsonValue.Object.TryGetValue(text, out JsonValue value))
				{
					return _empty;
				}
				item = jsonValue;
				jsonValue = value;
				continue;
			}
			if (jsonValue.Type == JsonValueType.Array)
			{
				switch (result)
				{
				case -1:
					return _empty;
				case int.MaxValue:
					result = jsonValue.Array.Count - 1;
					break;
				default:
					if (jsonValue.Array.Count <= result)
					{
						return _empty;
					}
					break;
				}
				item = jsonValue;
				jsonValue = jsonValue.Array[result];
				continue;
			}
			return _empty;
		}
		return (parent: item, key: text, index: result, success: true);
	}

	public static (JsonValue? result, bool success) InsertValue(JsonValue json, string path, JsonValue value, bool insertAfter)
	{
		if (path == string.Empty)
		{
			return (result: value, success: true);
		}
		IEnumerable<string> enumerable = path.Split(new char[1] { '/' }).Skip(1);
		JsonValue value2 = json;
		Func<JsonValue, bool> func = null;
		foreach (string item in enumerable)
		{
			string key = item.UnescapePointer();
			int result;
			if (key.StartsWith("0") && key != "0")
			{
				result = -1;
			}
			else if (!int.TryParse(item, out result))
			{
				result = ((key == "-") ? int.MaxValue : (-1));
			}
			if ((object)value2 == null)
			{
				return (result: null, success: false);
			}
			if (value2.Type == JsonValueType.Object)
			{
				JsonObject obj = value2.Object;
				func = delegate(JsonValue v)
				{
					obj[key] = v;
					return true;
				};
				value2.Object.TryGetValue(key, out value2);
				continue;
			}
			if (value2.Type == JsonValueType.Array)
			{
				switch (result)
				{
				case -1:
					return (result: null, success: false);
				case int.MaxValue:
					result = Math.Max(0, value2.Array.Count - 1);
					break;
				}
				JsonArray array = value2.Array;
				if (key == "-" || result == value2.Array.Count)
				{
					func = delegate(JsonValue v)
					{
						array.Add(v);
						return true;
					};
				}
				else
				{
					int tempIndex = result;
					func = delegate(JsonValue v)
					{
						if (tempIndex > array.Count - 1)
						{
							return false;
						}
						array.Insert(tempIndex, v);
						if (insertAfter)
						{
							JsonValue value3 = array[tempIndex];
							array[tempIndex] = array[tempIndex + 1];
							array[tempIndex + 1] = value3;
						}
						return true;
					};
				}
				value2 = ((result < value2.Array.Count) ? value2.Array[result] : null);
				continue;
			}
			return (result: null, success: false);
		}
		if (func == null)
		{
			return (result: null, success: false);
		}
		bool flag = func(value);
		return (result: flag ? json : null, success: flag);
	}
}
