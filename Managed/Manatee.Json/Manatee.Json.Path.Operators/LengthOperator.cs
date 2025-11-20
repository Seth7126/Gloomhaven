using System;

namespace Manatee.Json.Path.Operators;

internal class LengthOperator : IJsonPathOperator, IEquatable<LengthOperator>
{
	public static LengthOperator Instance { get; } = new LengthOperator();

	private LengthOperator()
	{
	}

	public JsonArray Evaluate(JsonArray json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		foreach (JsonValue item in json)
		{
			if (item.Type == JsonValueType.Array)
			{
				jsonArray.Add(item.Array.Count);
			}
		}
		return jsonArray;
	}

	public override string? ToString()
	{
		return ".length";
	}

	public bool Equals(LengthOperator? other)
	{
		return other != null;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as LengthOperator);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
