using System.Linq.Expressions;

namespace Manatee.Json.Path.Expressions.Translation;

internal class LengthExpressionTranslator : PathExpressionTranslator
{
	public override ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		if (body is MethodCallExpression method)
		{
			bool isLocal;
			return new LengthExpression<T>(PathExpressionTranslator.BuildPath(method, out isLocal), isLocal);
		}
		return new LengthExpression<T>();
	}
}
