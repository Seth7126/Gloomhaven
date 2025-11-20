using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Manatee.Json.Path.Expressions;

internal class LengthExpression<T> : PathExpression<T>, IEquatable<LengthExpression<T>>
{
	public LengthExpression()
		: base(new JsonPath(), isLocal: false)
	{
	}

	public LengthExpression(JsonPath path, bool isLocal)
		: base(path, isLocal)
	{
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		JsonArray jsonArray;
		if (base.Path == null || !base.Path.Operators.Any())
		{
			jsonArray = ((!base.IsLocal) ? (((object)root != null && root.Type == JsonValueType.Array) ? root.Array : null) : (json as JsonArray));
			if (jsonArray == null && json is JsonValue { Type: JsonValueType.Array } jsonValue)
			{
				jsonArray = jsonValue.Array;
			}
			if (jsonArray == null)
			{
				return null;
			}
		}
		else
		{
			JsonValue jsonValue2 = ((!base.IsLocal) ? root : ((json is JsonArray jsonArray2) ? ((JsonValue)jsonArray2) : (json as JsonValue)));
			JsonArray jsonArray3 = base.Path.Evaluate(jsonValue2);
			if (jsonArray3.Count > 1)
			{
				throw new InvalidOperationException($"Path '{base.Path}' returned more than one result on value '{jsonValue2}'");
			}
			JsonValue jsonValue3 = jsonArray3.FirstOrDefault();
			jsonArray = ((jsonValue3 != null && jsonValue3.Type == JsonValueType.Array) ? jsonValue3.Array : null);
		}
		if (jsonArray != null)
		{
			return (double)jsonArray.Count;
		}
		return null;
	}

	public override string? ToString()
	{
		string text = ((base.Path == null) ? string.Empty : base.Path.GetRawString());
		return (base.IsLocal ? "@" : "$") + text + ".length";
	}

	public bool Equals(LengthExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Equals((PathExpression<T>?)other);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as LengthExpression<T>);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
	}
}
