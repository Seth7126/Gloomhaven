using System;
using System.Collections.Generic;

namespace Manatee.Json.Path.SearchParameters;

internal class LengthSearchParameter : IJsonPathSearchParameter, IEquatable<LengthSearchParameter>
{
	public static LengthSearchParameter Instance { get; } = new LengthSearchParameter();

	public IEnumerable<JsonValue> Find(IEnumerable<JsonValue> json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		foreach (JsonValue item in json)
		{
			_Find(item, jsonArray);
		}
		return jsonArray;
	}

	public override string? ToString()
	{
		return "length";
	}

	public bool Equals(LengthSearchParameter? other)
	{
		return other != null;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as LengthSearchParameter);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private static void _Find(JsonValue value, JsonArray results)
	{
		results.Add((value.Type != JsonValueType.Array) ? 1 : value.Array.Count);
		switch (value.Type)
		{
		case JsonValueType.Object:
		{
			foreach (JsonValue value2 in value.Object.Values)
			{
				_Find(value2, results);
			}
			break;
		}
		case JsonValueType.Array:
		{
			foreach (JsonValue item in value.Array)
			{
				_Find(item, results);
			}
			break;
		}
		}
	}
}
