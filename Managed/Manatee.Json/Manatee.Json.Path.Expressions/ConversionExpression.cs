using System;
using System.Diagnostics.CodeAnalysis;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions;

internal class ConversionExpression<T> : ExpressionTreeNode<T>, IEquatable<ConversionExpression<T>>
{
	public ExpressionTreeNode<T> Root { get; }

	public Type TargetType { get; }

	public ConversionExpression(ExpressionTreeNode<T> root, Type targetType)
	{
		Root = root;
		TargetType = targetType;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		object value = Root.Evaluate(json, root);
		return _CastValue(value);
	}

	public override string? ToString()
	{
		return Root.ToString();
	}

	public bool Equals(ConversionExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Root, other.Root))
		{
			return TargetType == other.TargetType;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ConversionExpression<T>);
	}

	public override int GetHashCode()
	{
		return ((Root?.GetHashCode() ?? 0) * 397) ^ (TargetType?.GetHashCode() ?? 0);
	}

	private object? _CastValue(object? value)
	{
		if (value == null)
		{
			return null;
		}
		if (TargetType != typeof(JsonValue))
		{
			return value;
		}
		if (value is bool b)
		{
			return new JsonValue(b);
		}
		if (value is string s)
		{
			return new JsonValue(s);
		}
		if (value.IsNumber())
		{
			return new JsonValue(Convert.ToDouble(value));
		}
		return value;
	}
}
