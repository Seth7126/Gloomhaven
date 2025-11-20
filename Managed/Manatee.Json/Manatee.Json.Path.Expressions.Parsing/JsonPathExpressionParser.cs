using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions.Parsing;

internal static class JsonPathExpressionParser
{
	private static readonly List<IJsonPathExpressionParser> _parsers;

	static JsonPathExpressionParser()
	{
		_parsers = (from ti in typeof(JsonPathExpressionParser).GetTypeInfo().Assembly.DefinedTypes
			where typeof(IJsonPathExpressionParser).GetTypeInfo().IsAssignableFrom(ti) && ti.IsClass
			select Activator.CreateInstance(ti.AsType())).Cast<IJsonPathExpressionParser>().ToList();
	}

	public static bool TryParse<T, TIn>(string source, ref int index, [NotNullWhen(true)] out Expression<T, TIn>? expr, [NotNullWhen(false)] out string? errorMessage)
	{
		if (!_TryParse(source, ref index, out ExpressionTreeNode<TIn> root, out errorMessage))
		{
			expr = null;
			return false;
		}
		expr = new Expression<T, TIn>(root);
		return true;
	}

	private static bool _TryParse<TIn>(string source, ref int index, [NotNullWhen(true)] out ExpressionTreeNode<TIn>? root, [NotNullWhen(false)] out string? errorMessage)
	{
		JsonPathExpressionContext jsonPathExpressionContext = new JsonPathExpressionContext();
		while (index < source.Length)
		{
			errorMessage = source.SkipWhiteSpace(ref index, source.Length, out var _);
			if (errorMessage != null)
			{
				root = null;
				return false;
			}
			IJsonPathExpressionParser jsonPathExpressionParser = null;
			foreach (IJsonPathExpressionParser parser in _parsers)
			{
				if (parser.Handles(source, index))
				{
					jsonPathExpressionParser = parser;
					break;
				}
			}
			if (jsonPathExpressionParser == null)
			{
				root = null;
				errorMessage = "Unrecognized JSON Path Expression element.";
				return false;
			}
			if (!jsonPathExpressionParser.TryParse<TIn>(source, ref index, out JsonPathExpression expression, out errorMessage))
			{
				root = null;
				return false;
			}
			if (expression == null)
			{
				break;
			}
			if (expression is ValueExpression item)
			{
				jsonPathExpressionContext.Output.Push(item);
			}
			else if (expression is OperatorExpression operatorExpression)
			{
				if (operatorExpression.Operator == JsonPathOperator.GroupStart)
				{
					jsonPathExpressionContext.Operators.Push(new OperatorExpression
					{
						Operator = JsonPathOperator.GroupStart
					});
				}
				else if (operatorExpression.Operator == JsonPathOperator.GroupEnd)
				{
					while (jsonPathExpressionContext.Operators.Count > 0 && jsonPathExpressionContext.Operators.Peek().Operator != JsonPathOperator.GroupStart)
					{
						OperatorExpression operatorExpression2 = jsonPathExpressionContext.Operators.Pop();
						_GetRequiredChildrenFromOutput(jsonPathExpressionContext, operatorExpression2);
						jsonPathExpressionContext.Output.Push(operatorExpression2);
					}
					if (jsonPathExpressionContext.Operators.Count == 0 || jsonPathExpressionContext.Operators.Pop().Operator != JsonPathOperator.GroupStart)
					{
						errorMessage = "Unbalanced parentheses.";
						root = null;
						return false;
					}
				}
				else
				{
					if (operatorExpression.Operator == JsonPathOperator.Subtract && jsonPathExpressionContext.LastExpression is OperatorExpression { Operator: not JsonPathOperator.GroupEnd })
					{
						operatorExpression.Operator = JsonPathOperator.Negate;
					}
					while (jsonPathExpressionContext.Operators.Count > 0)
					{
						OperatorExpression operatorExpression4 = jsonPathExpressionContext.Operators.Peek();
						int num = _Compare(operatorExpression, operatorExpression4);
						if ((num >= 0 && (num != 0 || _IsRightAssociative(operatorExpression4.Operator))) || operatorExpression4.Operator == JsonPathOperator.GroupStart)
						{
							break;
						}
						OperatorExpression operatorExpression5 = jsonPathExpressionContext.Operators.Pop();
						_GetRequiredChildrenFromOutput(jsonPathExpressionContext, operatorExpression5);
						jsonPathExpressionContext.Output.Push(operatorExpression5);
					}
					jsonPathExpressionContext.Operators.Push(operatorExpression);
				}
			}
			jsonPathExpressionContext.LastExpression = expression;
		}
		return jsonPathExpressionContext.TryCreateExpressionTreeNode(out root, out errorMessage);
	}

	private static void _GetRequiredChildrenFromOutput(JsonPathExpressionContext context, OperatorExpression expr)
	{
		if (expr.IsBinary)
		{
			JsonPathExpression item = context.Output.Pop();
			JsonPathExpression item2 = context.Output.Pop();
			expr.Children.Add(item2);
			expr.Children.Add(item);
		}
		else
		{
			expr.Children.Add(context.Output.Pop());
		}
	}

	private static int _Compare(OperatorExpression a, OperatorExpression b)
	{
		return _Precedence(a.Operator).CompareTo(_Precedence(b.Operator));
	}

	private static int _Precedence(JsonPathOperator op)
	{
		switch (op)
		{
		case JsonPathOperator.GroupStart:
		case JsonPathOperator.GroupEnd:
			return 20;
		case JsonPathOperator.Negate:
		case JsonPathOperator.Not:
			return 16;
		case JsonPathOperator.Exponent:
			return 15;
		case JsonPathOperator.Divide:
		case JsonPathOperator.Modulo:
		case JsonPathOperator.Multiply:
			return 14;
		case JsonPathOperator.Add:
		case JsonPathOperator.Subtract:
			return 13;
		case JsonPathOperator.GreaterThan:
		case JsonPathOperator.GreaterThanOrEqual:
		case JsonPathOperator.LessThan:
		case JsonPathOperator.LessThanOrEqual:
			return 11;
		case JsonPathOperator.Equal:
		case JsonPathOperator.NotEqual:
			return 10;
		case JsonPathOperator.And:
			return 6;
		case JsonPathOperator.Or:
			return 5;
		default:
			return 0;
		}
	}

	private static bool _IsRightAssociative(JsonPathOperator op)
	{
		if (op == JsonPathOperator.Exponent || (uint)(op - 14) <= 1u)
		{
			return true;
		}
		return false;
	}
}
