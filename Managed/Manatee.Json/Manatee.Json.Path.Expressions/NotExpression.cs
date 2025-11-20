using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class NotExpression<T> : ExpressionTreeNode<T>, IEquatable<NotExpression<T>>
{
	public ExpressionTreeNode<T> Root { get; }

	public NotExpression(ExpressionTreeNode<T> root)
	{
		Root = root;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		return Root.Evaluate(json, root)?.Equals(true) ?? false;
	}

	public override string? ToString()
	{
		if (!(Root is ExpressionTreeBranch<T>))
		{
			return $"!{Root}";
		}
		return $"!({Root})";
	}

	public bool Equals(NotExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(Root, other.Root);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as NotExpression<T>);
	}

	public override int GetHashCode()
	{
		return Root.GetHashCode();
	}
}
