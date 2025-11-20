using System;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class NegateExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		return new NegateExpression<T>(ExpressionTranslator.TranslateNode<T>(((body as UnaryExpression) ?? throw new InvalidOperationException()).Operand));
	}
}
