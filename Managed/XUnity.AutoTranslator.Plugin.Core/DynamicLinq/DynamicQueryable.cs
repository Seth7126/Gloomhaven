using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicLinq;

internal static class DynamicQueryable
{
	public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values)
	{
		return (IQueryable<T>)((IQueryable)source).Where(predicate, values);
	}

	public static IQueryable Where(this IQueryable source, string predicate, params object[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		LambdaExpression expression = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, values);
		return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Where", new Type[1] { source.ElementType }, source.Expression, Expression.Quote(expression)));
	}

	public static IQueryable Select(this IQueryable source, string selector, params object[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		LambdaExpression lambdaExpression = DynamicExpression.ParseLambda(source.ElementType, null, selector, values);
		return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Select", new Type[2]
		{
			source.ElementType,
			lambdaExpression.Body.Type
		}, source.Expression, Expression.Quote(lambdaExpression)));
	}

	public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values)
	{
		return (IQueryable<T>)((IQueryable)source).OrderBy(ordering, values);
	}

	public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (ordering == null)
		{
			throw new ArgumentNullException("ordering");
		}
		ParameterExpression[] parameters = new ParameterExpression[1] { Expression.Parameter(source.ElementType, "") };
		IEnumerable<DynamicOrdering> enumerable = new ExpressionParser(parameters, ordering, values).ParseOrdering();
		Expression expression = source.Expression;
		string text = "OrderBy";
		string text2 = "OrderByDescending";
		foreach (DynamicOrdering item in enumerable)
		{
			expression = Expression.Call(typeof(Queryable), item.Ascending ? text : text2, new Type[2]
			{
				source.ElementType,
				item.Selector.Type
			}, expression, Expression.Quote(Expression.Lambda(item.Selector, parameters)));
			text = "ThenBy";
			text2 = "ThenByDescending";
		}
		return source.Provider.CreateQuery(expression);
	}

	public static IQueryable Take(this IQueryable source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Take", new Type[1] { source.ElementType }, source.Expression, Expression.Constant(count)));
	}

	public static IQueryable Skip(this IQueryable source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Skip", new Type[1] { source.ElementType }, source.Expression, Expression.Constant(count)));
	}

	public static IQueryable GroupBy(this IQueryable source, string keySelector, string elementSelector, params object[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		LambdaExpression lambdaExpression = DynamicExpression.ParseLambda(source.ElementType, null, keySelector, values);
		LambdaExpression lambdaExpression2 = DynamicExpression.ParseLambda(source.ElementType, null, elementSelector, values);
		return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "GroupBy", new Type[3]
		{
			source.ElementType,
			lambdaExpression.Body.Type,
			lambdaExpression2.Body.Type
		}, source.Expression, Expression.Quote(lambdaExpression), Expression.Quote(lambdaExpression2)));
	}

	public static bool Any(this IQueryable source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return (bool)source.Provider.Execute(Expression.Call(typeof(Queryable), "Any", new Type[1] { source.ElementType }, source.Expression));
	}

	public static int Count(this IQueryable source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return (int)source.Provider.Execute(Expression.Call(typeof(Queryable), "Count", new Type[1] { source.ElementType }, source.Expression));
	}
}
