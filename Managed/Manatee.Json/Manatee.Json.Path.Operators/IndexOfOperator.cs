using System;
using Manatee.Json.Path.Expressions;

namespace Manatee.Json.Path.Operators;

internal class IndexOfOperator : IJsonPathOperator, IEquatable<IndexOfOperator>
{
	public Expression<JsonValue, JsonArray> Parameter { get; }

	public IndexOfOperator(Expression<JsonValue, JsonArray> parameter)
	{
		Parameter = parameter;
	}

	public JsonArray Evaluate(JsonArray json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		JsonValue item = Parameter.Evaluate(json, root);
		foreach (JsonValue item2 in json)
		{
			if (item2.Type == JsonValueType.Array)
			{
				jsonArray.Add(item2.Array.IndexOf(item));
			}
		}
		return jsonArray;
	}

	public override string? ToString()
	{
		return $".indexOf({Parameter})";
	}

	public bool Equals(IndexOfOperator? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(Parameter, other.Parameter);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as IndexOfOperator);
	}

	public override int GetHashCode()
	{
		return Parameter?.GetHashCode() ?? 0;
	}
}
