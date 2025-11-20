using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Path.Expressions;

namespace Manatee.Json.Path.ArrayParameters;

internal class IndexExpressionQuery : IJsonPathArrayQuery, IEquatable<IndexExpressionQuery>
{
	private readonly Expression<int, JsonArray> _expression;

	public IndexExpressionQuery(Expression<int, JsonArray> expression)
	{
		_expression = expression;
	}

	public IEnumerable<JsonValue> Find(JsonArray json, JsonValue root)
	{
		int num = _expression.Evaluate(json, root);
		if (num < 0 || num >= json.Count)
		{
			return Enumerable.Empty<JsonValue>();
		}
		return new JsonValue[1] { json[num] };
	}

	public override string ToString()
	{
		return $"({_expression})";
	}

	public bool Equals(IndexExpressionQuery? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(_expression, other._expression);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as IndexExpressionQuery);
	}

	public override int GetHashCode()
	{
		return _expression?.GetHashCode() ?? 0;
	}
}
