using System;
using System.Linq;
using System.Linq.Expressions;
using Manatee.Json.Path.ArrayParameters;
using Manatee.Json.Path.Operators;

namespace Manatee.Json.Path.Expressions.Translation;

internal abstract class PathExpressionTranslator : IExpressionTranslator
{
	public abstract ExpressionTreeNode<T> Translate<T>(Expression body);

	protected static JsonPath BuildPath(MethodCallExpression method, out bool isLocal)
	{
		JsonPath jsonPath = new JsonPath();
		MethodCallExpression methodCallExpression = method.Arguments.FirstOrDefault() as MethodCallExpression;
		isLocal = ((method.Method.Name == "Length") ? (method.Arguments.Count != 0) : (method.Arguments.Count != 1));
		while (methodCallExpression != null)
		{
			ConstantExpression constantExpression = methodCallExpression.Arguments.Last() as ConstantExpression;
			switch (methodCallExpression.Method.Name)
			{
			case "Name":
				if (constantExpression == null || constantExpression.Type != typeof(string))
				{
					throw new NotSupportedException("Only literal string arguments are supported within JsonPath expressions.");
				}
				jsonPath.Operators.Insert(0, new NameOperator((string)constantExpression.Value));
				break;
			case "ArrayIndex":
				if (constantExpression == null || constantExpression.Type != typeof(int))
				{
					throw new NotSupportedException("Only literal string arguments are supported within JsonPath expressions.");
				}
				jsonPath.Operators.Insert(0, new ArrayOperator(new SliceQuery(new Slice((int)constantExpression.Value))));
				break;
			}
			isLocal = methodCallExpression.Arguments.Count != 1;
			methodCallExpression = methodCallExpression.Arguments.FirstOrDefault() as MethodCallExpression;
		}
		return jsonPath;
	}
}
