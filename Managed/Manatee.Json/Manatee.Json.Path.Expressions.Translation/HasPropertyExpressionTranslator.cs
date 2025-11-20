using System;
using System.Linq;
using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class HasPropertyExpressionTranslator : PathExpressionTranslator
{
	public override ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		MethodCallExpression obj = (body as MethodCallExpression) ?? throw new InvalidOperationException();
		if (!(obj.Arguments.Last() is ConstantExpression constantExpression) || constantExpression.Type != typeof(string))
		{
			throw new NotSupportedException("Only constant string arguments are supported in HasProperty()");
		}
		bool isLocal;
		return new HasPropertyExpression<T>(PathExpressionTranslator.BuildPath(obj, out isLocal), isLocal, constantExpression.Value.ToString());
	}
}
