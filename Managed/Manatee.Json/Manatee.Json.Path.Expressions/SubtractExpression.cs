using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class SubtractExpression<T> : ExpressionTreeBranch<T>, IEquatable<SubtractExpression<T>>
{
	public SubtractExpression(ExpressionTreeNode<T> left, ExpressionTreeNode<T> right)
		: base(left, right)
	{
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		object obj = base.Left.Evaluate(json, root);
		object obj2 = base.Right.Evaluate(json, root);
		if (!(obj is double) || !(obj2 is double))
		{
			return null;
		}
		return (double)obj - (double)obj2;
	}

	public override string? ToString()
	{
		string arg = ((base.Right is ExpressionTreeBranch<T>) ? $"({base.Right})" : base.Right.ToString());
		return $"{base.Left}-{arg}";
	}

	public bool Equals(SubtractExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Equals((ExpressionTreeBranch<T>?)other);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SubtractExpression<T>);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
	}
}
