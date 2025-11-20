using System;
using System.Collections.Generic;

namespace Manatee.Json.Path.Operators;

internal class ArrayOperator : IJsonPathOperator, IEquatable<ArrayOperator>
{
	public IJsonPathArrayQuery Query { get; }

	public ArrayOperator(IJsonPathArrayQuery query)
	{
		Query = query;
	}

	public JsonArray Evaluate(JsonArray json, JsonValue root)
	{
		List<JsonValue> list = new List<JsonValue>();
		foreach (JsonValue item in json)
		{
			_Evaluate(item, root, list);
		}
		return new JsonArray(list);
	}

	public override string? ToString()
	{
		return $"[{Query}]";
	}

	public bool Equals(ArrayOperator? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(Query, other.Query);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ArrayOperator);
	}

	public override int GetHashCode()
	{
		return Query?.GetHashCode() ?? 0;
	}

	private void _Evaluate(JsonValue value, JsonValue root, List<JsonValue> results)
	{
		switch (value.Type)
		{
		case JsonValueType.Array:
			results.AddRange(Query.Find(value.Array, root));
			break;
		case JsonValueType.Object:
			results.AddRange(Query.Find(new JsonArray(value.Object.Values), root));
			break;
		}
	}
}
