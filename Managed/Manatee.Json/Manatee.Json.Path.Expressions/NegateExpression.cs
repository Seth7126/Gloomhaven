using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class NegateExpression<T> : ExpressionTreeNode<T>, IEquatable<NegateExpression<T>>
{
	public ExpressionTreeNode<T> Root { get; }

	public NegateExpression(ExpressionTreeNode<T> root)
	{
		Root = root;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		return 0.0 - Convert.ToDouble(Root.Evaluate(json, root));
	}

	public override string? ToString()
	{
		if (!(Root is ExpressionTreeBranch<T>))
		{
			return $"-{Root}";
		}
		return $"-({Root})";
	}

	public bool Equals(NegateExpression<T>? other)
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
		return Equals(obj as NegateExpression<T>);
	}

	public override int GetHashCode()
	{
		return Root?.GetHashCode() ?? 0;
	}
}
