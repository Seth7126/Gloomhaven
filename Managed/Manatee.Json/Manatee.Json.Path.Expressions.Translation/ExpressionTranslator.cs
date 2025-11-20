using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions.Translation;

internal static class ExpressionTranslator
{
	private static readonly Dictionary<Type, Func<Expression, IExpressionTranslator>> _translators = new Dictionary<Type, Func<Expression, IExpressionTranslator>>
	{
		{
			typeof(ConstantExpression),
			_GetValueTranslator
		},
		{
			typeof(BinaryExpression),
			(Expression e) => _GetNodeTypeBasedTranslator(e.NodeType)
		},
		{
			typeof(UnaryExpression),
			(Expression e) => _GetNodeTypeBasedTranslator(e.NodeType)
		},
		{
			typeof(MethodCallExpression),
			_GetMethodCallTranslator
		},
		{
			typeof(MemberExpression),
			_GetMemberTranslator
		}
	};

	private static IExpressionTranslator _GetValueTranslator(Expression e)
	{
		Type type = e.Type;
		if (type == typeof(bool))
		{
			return new BooleanValueExpressionTranslator();
		}
		if (type == typeof(string))
		{
			return new StringValueExpressionTranslator();
		}
		if (type.In(typeof(sbyte), typeof(byte), typeof(char), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)))
		{
			return new NumberValueExpressionTranslator();
		}
		if (((ConstantExpression)e).Value == null)
		{
			return new NullValueExpressionTranslator();
		}
		throw new NotSupportedException($"Values of type '{type}' are not supported.");
	}

	private static IExpressionTranslator _GetNodeTypeBasedTranslator(ExpressionType type)
	{
		switch (type)
		{
		case ExpressionType.Add:
		case ExpressionType.AddChecked:
			return new AddExpressionTranslator();
		case ExpressionType.Divide:
			return new DivideExpressionTranslator();
		case ExpressionType.Modulo:
			return new ModuloExpressionTranslator();
		case ExpressionType.Multiply:
		case ExpressionType.MultiplyChecked:
			return new MultiplyExpressionTranslator();
		case ExpressionType.Power:
			return new ExponentExpressionTranslator();
		case ExpressionType.Subtract:
		case ExpressionType.SubtractChecked:
			return new SubtractExpressionTranslator();
		case ExpressionType.Convert:
		case ExpressionType.ConvertChecked:
			return new ConversionExpressionTranslator();
		case ExpressionType.Negate:
		case ExpressionType.NegateChecked:
			return new NegateExpressionTranslator();
		case ExpressionType.Not:
			return new NotExpressionTranslator();
		case ExpressionType.And:
		case ExpressionType.AndAlso:
			return new AndExpressionTranslator();
		case ExpressionType.Or:
		case ExpressionType.OrElse:
			return new OrExpressionTranslator();
		case ExpressionType.Equal:
			return new IsEqualExpressionTranslator();
		case ExpressionType.LessThan:
			return new IsLessThanExpressionTranslator();
		case ExpressionType.LessThanOrEqual:
			return new IsLessThanEqualExpressionTranslator();
		case ExpressionType.GreaterThan:
			return new IsGreaterThanExpressionTranslator();
		case ExpressionType.GreaterThanOrEqual:
			return new IsGreaterThanEqualExpressionTranslator();
		case ExpressionType.NotEqual:
			return new IsNotEqualExpressionTranslator();
		default:
			throw new NotSupportedException($"Expression type '{type}' is not supported.");
		}
	}

	private static IExpressionTranslator _GetMethodCallTranslator(Expression exp)
	{
		MethodCallExpression methodCallExpression = (MethodCallExpression)exp;
		return methodCallExpression.Method.Name switch
		{
			"Length" => new LengthExpressionTranslator(), 
			"HasProperty" => new HasPropertyExpressionTranslator(), 
			"Name" => new NameExpressionTranslator(), 
			"ArrayIndex" => new ArrayIndexExpressionTranslator(), 
			"IndexOf" => new IndexOfExpressionTranslator(), 
			_ => throw new NotSupportedException("The method '" + methodCallExpression.Method.Name + "' is not supported."), 
		};
	}

	private static IExpressionTranslator _GetMemberTranslator(Expression exp)
	{
		MemberExpression memberExpression = (MemberExpression)exp;
		if (memberExpression.Member is FieldInfo && memberExpression.Expression is ConstantExpression)
		{
			return new FieldExpressionTranslator();
		}
		throw new NotSupportedException("Properties and static fields are not supported.");
	}

	public static ExpressionTreeNode<T> TranslateNode<T>(Expression source)
	{
		Type type = source.GetType();
		TypeInfo typeInfo = type.GetTypeInfo();
		Type type2 = _translators.Keys.FirstOrDefault((Type t) => t.GetTypeInfo().IsAssignableFrom(typeInfo));
		if (type2 != null)
		{
			IExpressionTranslator expressionTranslator = _translators[type2](source);
			if (expressionTranslator != null)
			{
				return expressionTranslator.Translate<T>(source);
			}
		}
		throw new NotSupportedException($"Expression type '{type}' is not supported.");
	}

	public static Expression<T, JsonValue> Translate<T>(Expression<Func<JsonPathValue, T>> source)
	{
		return new Expression<T, JsonValue>(TranslateNode<JsonValue>(source.Body));
	}

	public static Expression<T, JsonArray> Translate<T>(Expression<Func<JsonPathArray, T>> source)
	{
		return new Expression<T, JsonArray>(TranslateNode<JsonArray>(source.Body));
	}
}
