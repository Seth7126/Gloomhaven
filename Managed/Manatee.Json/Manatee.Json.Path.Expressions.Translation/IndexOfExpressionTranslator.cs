using System;
using System.Linq;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class IndexOfExpressionTranslator : PathExpressionTranslator
{
	public override ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (!(body is MethodCallExpression methodCallExpression))
		{
			throw new InvalidOperationException();
		}
		bool isLocal;
		if (!(methodCallExpression.Arguments.Last() is ConstantExpression constantExpression) || constantExpression.Type != typeof(JsonValue))
		{
			return new IndexOfExpression<T>(PathExpressionTranslator.BuildPath(methodCallExpression, out isLocal), isLocal, ExpressionTranslator.TranslateNode<JsonArray>(methodCallExpression.Arguments.Last()));
		}
		return new IndexOfExpression<T>(PathExpressionTranslator.BuildPath(methodCallExpression, out isLocal), isLocal, (JsonValue)constantExpression.Value);
	}
}
