using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class IsGreaterThanExpression<T> : ExpressionTreeBranch<T>, IEquatable<IsGreaterThanExpression<T>>
{
	public IsGreaterThanExpression(ExpressionTreeNode<T> left, ExpressionTreeNode<T> right)
		: base(left, right)
	{
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		object? a = base.Left.Evaluate(json, root);
		object b = base.Right.Evaluate(json, root);
		return ValueComparer.GreaterThan(a, b);
	}

	public override string? ToString()
	{
		string obj = ((base.Left is ExpressionTreeBranch<T>) ? $"({base.Left})" : base.Left.ToString());
		string text = ((base.Right is ExpressionTreeBranch<T>) ? $"({base.Right})" : base.Right.ToString());
		return obj + " > " + text;
	}

	public bool Equals(IsGreaterThanExpression<T>? other)
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
		return Equals(obj as IsGreaterThanExpression<T>);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
	}
}
