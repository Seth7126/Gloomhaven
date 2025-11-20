using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class ConversionExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (!(body is UnaryExpression unaryExpression))
		{
			throw new InvalidOperationException();
		}
		return new ConversionExpression<T>(ExpressionTranslator.TranslateNode<T>(unaryExpression.Operand), unaryExpression.Type);
	}
}
