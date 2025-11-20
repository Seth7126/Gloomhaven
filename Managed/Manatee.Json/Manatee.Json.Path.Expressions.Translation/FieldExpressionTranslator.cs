using System.Linq.Expressions;
using System.Reflection;

namespace Manatee.Json.Path.Expressions.Translation;

internal class FieldExpressionTranslator : IExpressionTranslator
{
	public ExpressionTreeNode<T> Translate<T>(Expression body)
	{
		MemberExpression memberExpression = (MemberExpression)body;
		return new FieldExpression<T>((FieldInfo)memberExpression.Member, ((ConstantExpression)memberExpression.Expression).Value);
	}
}
