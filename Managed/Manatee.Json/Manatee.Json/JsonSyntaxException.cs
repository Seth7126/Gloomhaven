using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Pointer;

namespace Manatee.Json;

public class JsonSyntaxException : Exception
{
	public string? SourceData { get; }

	public JsonPointer Location { get; }

	public override string Message => $"{base.Message} Path: '{Location}'";

	internal JsonSyntaxException(string message, JsonValue? value)
		: base(message)
	{
		Location = _BuildPointer(value);
	}

	internal JsonSyntaxException(string source, string message, JsonValue? value)
		: base(message)
	{
		SourceData = source;
		Location = _BuildPointer(value);
	}

	private static JsonPointer _BuildPointer(JsonValue? value)
	{
		JsonPointer jsonPointer = new JsonPointer();
		if (value == null)
		{
			return jsonPointer;
		}
		switch (value.Type)
		{
		case JsonValueType.Object:
		{
			if (!value.Object.Any())
			{
				return jsonPointer;
			}
			KeyValuePair<string, JsonValue> keyValuePair = value.Object.Last();
			string key = keyValuePair.Key;
			jsonPointer.Add(key);
			jsonPointer.AddRange(_BuildPointer(keyValuePair.Value));
			break;
		}
		case JsonValueType.Array:
		{
			if (!value.Array.Any())
			{
				return jsonPointer;
			}
			JsonValue value2 = value.Array.Last();
			jsonPointer.Add((value.Array.Count - 1).ToString());
			jsonPointer.AddRange(_BuildPointer(value2));
			break;
		}
		}
		return jsonPointer;
	}
}
