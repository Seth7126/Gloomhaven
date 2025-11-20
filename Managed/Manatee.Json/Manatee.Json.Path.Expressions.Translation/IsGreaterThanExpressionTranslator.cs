using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class IsGreaterThanExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (!(body is BinaryExpression binaryExpression))
		{
			throw new InvalidOperationException();
		}
		return new IsGreaterThanExpression<T>(ExpressionTranslator.TranslateNode<T>(binaryExpression.Left), ExpressionTranslator.TranslateNode<T>(binaryExpression.Right));
	}
}
