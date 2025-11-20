using System;

namespace Manatee.Json.Path.Expressions;

internal class Expression<T, TIn> : IEquatable<Expression<T, TIn>>
{
	private readonly ExpressionTreeNode<TIn> _root;

	public Expression(ExpressionTreeNode<TIn> root)
	{
		_root = root;
	}

	public T Evaluate(TIn json, JsonValue root)
	{
		object obj = _root.Evaluate(json, root);
		if (typeof(T) == typeof(bool) && obj == null)
		{
			return (T)(object)false;
		}
		if (typeof(T) == typeof(bool) && obj != null && !(obj is bool))
		{
			return (T)(object)true;
		}
		if (typeof(T) == typeof(int) && obj == null)
		{
			return (T)(object)(-1);
		}
		JsonValue jsonValue = obj as JsonValue;
		if (typeof(T) == typeof(int) && (object)jsonValue != null && jsonValue.Type == JsonValueType.Number)
		{
			return (T)Convert.ChangeType(jsonValue.Number, typeof(T));
		}
		if (typeof(T) == typeof(JsonValue))
		{
			if (obj is JsonValue)
			{
				return (T)obj;
			}
			if (obj is double n)
			{
				return (T)(object)new JsonValue(n);
			}
			if (obj is bool b)
			{
				return (T)(object)new JsonValue(b);
			}
			if (obj is string s)
			{
				return (T)(object)new JsonValue(s);
			}
			if (obj is JsonArray a)
			{
				return (T)(object)new JsonValue(a);
			}
			if (obj is JsonObject o)
			{
				return (T)(object)new JsonValue(o);
			}
		}
		return (T)Convert.ChangeType(obj, typeof(T));
	}

	public override string? ToString()
	{
		return _root.ToString();
	}

	public bool Equals(Expression<T, TIn>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(_root, other._root);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as Expression<T, TIn>);
	}

	public override int GetHashCode()
	{
		return _root?.GetHashCode() ?? 0;
	}
}
