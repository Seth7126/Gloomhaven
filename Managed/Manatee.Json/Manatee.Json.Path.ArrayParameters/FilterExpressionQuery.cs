using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Path.Expressions;

namespace Manatee.Json.Path.ArrayParameters;

internal class FilterExpressionQuery : IJsonPathArrayQuery, IEquatable<FilterExpressionQuery>
{
	private readonly Expression<bool, JsonValue> _expression;

	public FilterExpressionQuery(Expression<bool, JsonValue> expression)
	{
		_expression = expression;
	}

	public IEnumerable<JsonValue> Find(JsonArray json, JsonValue root)
	{
		return json.Where((JsonValue v) => _expression.Evaluate(v, root));
	}

	public override string ToString()
	{
		return $"?({_expression})";
	}

	public bool Equals(FilterExpressionQuery? other)
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
		return Equals(obj as FilterExpressionQuery);
	}

	public override int GetHashCode()
	{
		return _expression?.GetHashCode() ?? 0;
	}
}
