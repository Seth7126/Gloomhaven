using System;
using System.Linq;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class NameExpressionTranslator : PathExpressionTranslator
{
	public override ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (!(body is MethodCallExpression methodCallExpression))
		{
			throw new InvalidOperationException();
		}
		bool isLocal;
		if (!(methodCallExpression.Arguments.Last() is ConstantExpression constantExpression) || constantExpression.Type != typeof(string))
		{
			return new NameExpression<T>(PathExpressionTranslator.BuildPath(methodCallExpression, out isLocal), isLocal, ExpressionTranslator.TranslateNode<T>(methodCallExpression.Arguments.Last()));
		}
		return new NameExpression<T>(PathExpressionTranslator.BuildPath(methodCallExpression, out isLocal), isLocal, (string)constantExpression.Value);
	}
}
