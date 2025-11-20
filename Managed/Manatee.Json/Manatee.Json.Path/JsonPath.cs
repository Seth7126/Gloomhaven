using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Path.Parsing;

namespace Manatee.Json.Path;

public class JsonPath : IEquatable<JsonPath>
{
	internal List<IJsonPathOperator> Operators { get; } = new List<IJsonPathOperator>();

	public static JsonPath Parse(string source)
	{
		return JsonPathParser.Parse(source);
	}

	public JsonArray Evaluate(JsonValue? json)
	{
		if (json == null)
		{
			throw new ArgumentNullException("json");
		}
		JsonArray jsonArray = new JsonArray { json };
		foreach (IJsonPathOperator @operator in Operators)
		{
			jsonArray = @operator.Evaluate(jsonArray, json);
		}
		return jsonArray;
	}

	public override string ToString()
	{
		return "$" + GetRawString();
	}

	public bool Equals(JsonPath? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Operators.SequenceEqual(other.Operators);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as JsonPath);
	}

	public override int GetHashCode()
	{
		return Operators?.GetCollectionHashCode() ?? 0;
	}

	internal string GetRawString()
	{
		return string.Join(string.Empty, Operators.Select((IJsonPathOperator o) => o.ToString()));
	}
}
