using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class IsEqualExpression<T> : ExpressionTreeBranch<T>, IEquatable<IsEqualExpression<T>>
{
	public IsEqualExpression(ExpressionTreeNode<T> left, ExpressionTreeNode<T> right)
		: base(left, right)
	{
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		object obj = base.Left.Evaluate(json, root);
		object obj2 = base.Right.Evaluate(json, root);
		if (obj == null && obj2 == null)
		{
			return true;
		}
		if (obj == null || obj2 == null)
		{
			return false;
		}
		return ValueComparer.Equal(obj, obj2);
	}

	public override string? ToString()
	{
		string obj = ((base.Left is ExpressionTreeBranch<T>) ? $"({base.Left})" : base.Left.ToString());
		string text = ((base.Right is ExpressionTreeBranch<T>) ? $"({base.Right})" : base.Right.ToString());
		return obj + " == " + text;
	}

	public bool Equals(IsEqualExpression<T>? other)
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
		return Equals(obj as IsEqualExpression<T>);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
	}
}
