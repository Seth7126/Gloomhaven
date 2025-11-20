using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicLinq;

internal static class DynamicExpression
{
	public static Expression Parse(Type resultType, string expression, params object[] values)
	{
		return new ExpressionParser(null, expression, values).Parse(resultType);
	}

	public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values)
	{
		return ParseLambda(new ParameterExpression[1] { Expression.Parameter(itType, "") }, resultType, expression, values);
	}

	public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
	{
		return Expression.Lambda(new ExpressionParser(parameters, expression, values).Parse(resultType), parameters);
	}

	public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values)
	{
		return (Expression<Func<T, S>>)ParseLambda(typeof(T), typeof(S), expression, values);
	}

	public static Type CreateClass(params DynamicProperty[] properties)
	{
		throw new NotImplementedException();
	}

	public static Type CreateClass(IEnumerable<DynamicProperty> properties)
	{
		throw new NotImplementedException();
	}
}
