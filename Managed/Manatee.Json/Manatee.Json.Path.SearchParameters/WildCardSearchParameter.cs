using System;
using System.Collections.Generic;

namespace Manatee.Json.Path.SearchParameters;

internal class WildCardSearchParameter : IJsonPathSearchParameter, IEquatable<WildCardSearchParameter>
{
	public static WildCardSearchParameter Instance { get; } = new WildCardSearchParameter();

	private WildCardSearchParameter()
	{
	}

	public IEnumerable<JsonValue> Find(IEnumerable<JsonValue> json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		foreach (JsonValue item in json)
		{
			_Find(item, jsonArray);
		}
		jsonArray.RemoveAt(0);
		return jsonArray;
	}

	public override string? ToString()
	{
		return "*";
	}

	public bool Equals(WildCardSearchParameter? other)
	{
		return other != null;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as WildCardSearchParameter);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private static void _Find(JsonValue value, JsonArray results)
	{
		results.Add(value);
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
