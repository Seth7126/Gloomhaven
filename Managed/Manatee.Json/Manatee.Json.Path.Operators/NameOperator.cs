using System;
using System.Linq;

namespace Manatee.Json.Path.Operators;

internal class NameOperator : IJsonPathOperator, IEquatable<NameOperator>
{
	public string Name { get; }

	public NameOperator(string name)
	{
		Name = name;
	}

	public JsonArray Evaluate(JsonArray json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		foreach (JsonValue item in json)
		{
			if (item.Type == JsonValueType.Object && item.Object.TryGetValue(Name, out JsonValue value))
			{
				jsonArray.Add(value);
			}
		}
		return jsonArray;
	}

	public override string? ToString()
	{
		if (!Name.Any((char c) => !char.IsLetterOrDigit(c)) && !string.IsNullOrWhiteSpace(Name))
		{
			return "." + Name;
		}
		return ".'" + Name + "'";
	}

	public bool Equals(NameOperator? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return string.Equals(Name, other.Name);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as NameOperator);
	}

	public override int GetHashCode()
	{
		return Name?.GetHashCode() ?? 0;
	}
}
