using System;
using System.Collections.Generic;

namespace Manatee.Json.Path.SearchParameters;

internal class ArraySearchParameter : IJsonPathSearchParameter, IEquatable<ArraySearchParameter>
{
	private readonly IJsonPathArrayQuery _query;

	public ArraySearchParameter(IJsonPathArrayQuery query)
	{
		_query = query;
	}

	public IEnumerable<JsonValue> Find(IEnumerable<JsonValue> json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		foreach (JsonValue item in json)
		{
			_Find(item, root, jsonArray);
		}
		return jsonArray;
	}

	public override string? ToString()
	{
		return $"[{_query}]";
	}

	public bool Equals(ArraySearchParameter? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(_query, other._query);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ArraySearchParameter);
	}

	public override int GetHashCode()
	{
		return _query?.GetHashCode() ?? 0;
	}

	private void _Find(JsonValue value, JsonValue root, JsonArray results)
	{
		switch (value.Type)
		{
		case JsonValueType.Object:
		{
			foreach (JsonValue value2 in value.Object.Values)
			{
				_Find(value2, root, results);
			}
			break;
		}
		case JsonValueType.Array:
			results.AddRange(_query.Find(value.Array, root));
			{
				foreach (JsonValue item in value.Array)
				{
					_Find(item, root, results);
				}
				break;
			}
		}
	}
}
