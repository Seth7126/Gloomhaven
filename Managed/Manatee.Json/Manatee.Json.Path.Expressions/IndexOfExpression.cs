using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions;

internal class IndexOfExpression<T> : PathExpression<T>, IEquatable<IndexOfExpression<T>>
{
	public JsonValue Parameter { get; }

	public ExpressionTreeNode<JsonArray> ParameterExpression { get; }

	public IndexOfExpression(JsonPath path, bool isLocal, JsonValue parameter)
		: base(path, isLocal)
	{
		Parameter = parameter;
		ParameterExpression = null;
	}

	public IndexOfExpression(JsonPath path, bool isLocal, ExpressionTreeNode<JsonArray> parameterExpression)
		: base(path, isLocal)
	{
		Parameter = null;
		ParameterExpression = parameterExpression;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		JsonValue jsonValue = ((!base.IsLocal) ? root : ((json != null) ? json.AsJsonValue() : null));
		if (jsonValue == null)
		{
			throw new NotSupportedException("IndexOf requires a JsonValue to evaluate.");
		}
		JsonArray jsonArray = base.Path.Evaluate(jsonValue);
		if (jsonArray.Count > 1)
		{
			throw new InvalidOperationException($"Path '{base.Path}' returned more than one result on value '{jsonValue}'");
		}
		JsonValue jsonValue2 = jsonArray.FirstOrDefault();
		JsonValue jsonValue3 = _GetParameter();
		if (jsonValue2 == null || jsonValue2.Type != JsonValueType.Array || jsonValue3 == null)
		{
			return null;
		}
		return jsonValue2.Array.IndexOf(jsonValue3);
	}

	public override string? ToString()
	{
		string arg = ((base.Path == null) ? string.Empty : base.Path.GetRawString());
		string arg2 = ParameterExpression?.ToString() ?? Parameter.ToString();
		return string.Format(base.IsLocal ? "@{0}.indexOf({1})" : "${0}.indexOf({1})", arg, arg2);
	}

	public bool Equals(IndexOfExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Equals((PathExpression<T>?)other))
		{
			return object.Equals(ParameterExpression, other.ParameterExpression);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as IndexOfExpression<T>);
	}

	public override int GetHashCode()
	{
		return (((base.GetHashCode() * 397) ^ ((Parameter != null) ? Parameter.GetHashCode() : 0)) * 397) ^ (ParameterExpression?.GetHashCode() ?? 0);
	}

	private JsonValue? _GetParameter()
	{
		object obj = ParameterExpression?.Evaluate(null, null);
		if (!(obj is double n))
		{
			if (!(obj is bool b))
			{
				if (!(obj is string s))
				{
					if (!(obj is JsonArray a))
					{
						if (!(obj is JsonObject o))
						{
							if (obj is JsonValue result)
							{
								return result;
							}
							return Parameter;
						}
						return new JsonValue(o);
					}
					return new JsonValue(a);
				}
				return new JsonValue(s);
			}
			return new JsonValue(b);
		}
		return new JsonValue(n);
	}
}
