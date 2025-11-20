using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class AddExpression<T> : ExpressionTreeBranch<T>, IEquatable<AddExpression<T>>
{
	public AddExpression(ExpressionTreeNode<T> left, ExpressionTreeNode<T> right)
		: base(left, right)
	{
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		object obj = base.Left.Evaluate(json, root);
		object obj2 = base.Right.Evaluate(json, root);
		if (obj is double num && obj2 is double num2)
		{
			return num + num2;
		}
		if (obj is string text && obj2 is string text2)
		{
			return text + text2;
		}
		return null;
	}

	public override string? ToString()
	{
		string arg = ((base.Right is ExpressionTreeBranch<T>) ? $"({base.Right})" : base.Right.ToString());
		return $"{base.Left}+{arg}";
	}

	public bool Equals(AddExpression<T>? other)
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
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((AddExpression<T>)obj);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
	}
}
