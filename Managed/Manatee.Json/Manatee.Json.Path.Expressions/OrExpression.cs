using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class OrExpression<T> : ExpressionTreeBranch<T>, IEquatable<OrExpression<T>>
{
	public OrExpression(ExpressionTreeNode<T> left, ExpressionTreeNode<T> right)
		: base(left, right)
	{
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		bool? flag = (bool?)base.Left.Evaluate(json, root);
		bool? flag2 = (bool?)base.Right.Evaluate(json, root);
		return flag.HasValue && flag2.HasValue && (flag.Value || flag2.Value);
	}

	public override string? ToString()
	{
		string obj = ((base.Left is ExpressionTreeBranch<T>) ? $"({base.Left})" : base.Left.ToString());
		string text = ((base.Right is ExpressionTreeBranch<T>) ? $"({base.Right})" : base.Right.ToString());
		return obj + " || " + text;
	}

	public bool Equals(OrExpression<T>? other)
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
		return Equals(obj as OrExpression<T>);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
	}
}
