using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class StringValueExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		return new ValueExpression<T>(((body as ConstantExpression) ?? throw new InvalidOperationException()).Value);
	}
}
