using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class IsNotEqualExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (!(body is BinaryExpression binaryExpression))
		{
			throw new InvalidOperationException();
		}
		return new IsNotEqualExpression<T>(ExpressionTranslator.TranslateNode<T>(binaryExpression.Left), ExpressionTranslator.TranslateNode<T>(binaryExpression.Right));
	}
}
