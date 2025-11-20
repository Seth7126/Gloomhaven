using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class ModuloExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (!(body is BinaryExpression binaryExpression))
		{
			throw new InvalidOperationException();
		}
		return new ModuloExpression<T>(ExpressionTranslator.TranslateNode<T>(binaryExpression.Left), ExpressionTranslator.TranslateNode<T>(binaryExpression.Right));
	}
}
