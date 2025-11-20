namespace Manatee.Json.Path.Expressions;

internal abstract class ExpressionTreeBranch<T> : ExpressionTreeNode<T>
{
	public ExpressionTreeNode<T> Left { get; }

	public ExpressionTreeNode<T> Right { get; }

	public ExpressionTreeBranch(ExpressionTreeNode<T> left, ExpressionTreeNode<T> right)
	{
		Left = left;
		Right = right;
	}

	protected bool Equals(ExpressionTreeBranch<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Left, other.Left))
		{
			return object.Equals(Right, other.Right);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ExpressionTreeBranch<T>);
	}

	public override int GetHashCode()
	{
		return (Left.GetHashCode() * 397) ^ Right.GetHashCode();
	}
}
