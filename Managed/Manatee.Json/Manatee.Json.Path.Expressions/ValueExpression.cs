using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class ValueExpression<T> : ExpressionTreeNode<T>, IEquatable<ValueExpression<T>>
{
	public object Value { get; }

	public ValueExpression(object value)
	{
		Value = value;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		return Value;
	}

	public override string? ToString()
	{
		object value = Value;
		if (!(value is string text))
		{
			if (value is bool flag)
			{
				return flag.ToString().ToLower();
			}
			return Value.ToString();
		}
		return "\"" + text + "\"";
	}

	public bool Equals(ValueExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(Value, other.Value);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ValueExpression<T>);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}
}
