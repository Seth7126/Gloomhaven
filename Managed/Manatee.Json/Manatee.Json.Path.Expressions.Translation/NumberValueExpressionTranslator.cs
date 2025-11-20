using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class NumberValueExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		return new ValueExpression<T>(Convert.ToDouble(((body as ConstantExpression) ?? throw new InvalidOperationException()).Value));
	}
}
