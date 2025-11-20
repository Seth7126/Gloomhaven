using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class NotExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		return new NotExpression<T>(ExpressionTranslator.TranslateNode<T>(((body as UnaryExpression) ?? throw new InvalidOperationException()).Operand));
	}
}
