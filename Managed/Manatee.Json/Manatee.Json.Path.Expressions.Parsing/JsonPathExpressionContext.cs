using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions.Parsing;

internal class JsonPathExpressionContext
{
	public Stack<JsonPathExpression> Output { get; } = new Stack<JsonPathExpression>();

	public Stack<OperatorExpression> Operators { get; } = new Stack<OperatorExpression>();

	public JsonPathExpression? LastExpression { get; set; }

	public bool TryCreateExpressionTreeNode<TIn>([NotNullWhen(true)] out ExpressionTreeNode<TIn> root, [NotNullWhen(false)] out string errorMessage)
	{
		if (Output.Count != 1)
		{
			root = null;
			errorMessage = "No expression to parse.";
			return false;
		}
		if (Operators.Count > 0)
		{
			root = null;
			errorMessage = "Unbalanced expression.";
			return false;
		}
		JsonPathExpression jsonPathExpression = Output.Pop();
		if (jsonPathExpression == null)
		{
			root = null;
			errorMessage = "No expression found.";
			return false;
		}
		ExpressionTreeNode<TIn> node = _Visit<TIn>(jsonPathExpression);
		root = _MakeHasPropertyIfNameExpression(node);
		errorMessage = null;
		return true;
	}

	private static ExpressionTreeNode<TIn> _Visit<TIn>(JsonPathExpression expr)
	{
		if (expr is PathValueExpression<TIn> pathValueExpression)
		{
			return pathValueExpression.Path;
		}
		if (expr is ValueExpression valueExpression)
		{
			return new ValueExpression<TIn>(valueExpression.Value);
		}
		if (!(expr is OperatorExpression operatorExpression))
		{
			throw new NotSupportedException($"Expressions of type {expr.GetType()} are not supported.");
		}
		ExpressionTreeNode<TIn> left = _Visit<TIn>(operatorExpression.Children[0]);
		if (operatorExpression.Operator == JsonPathOperator.Negate)
		{
			return _VisitNegate(left);
		}
		if (operatorExpression.Operator == JsonPathOperator.Not)
		{
			return new NotExpression<TIn>(_MakeHasPropertyIfNameExpression(left));
		}
		if (operatorExpression.Children.Count <= 1)
		{
			throw new InvalidOperationException($"Operator type {operatorExpression.Operator} requires two operands.");
		}
		ExpressionTreeNode<TIn> right = _Visit<TIn>(operatorExpression.Children[1]);
		_CheckAndReplaceIfHasPropertyNeeded(operatorExpression.Operator, ref left, ref right);
		return operatorExpression.Operator switch
		{
			JsonPathOperator.Add => new AddExpression<TIn>(left, right), 
			JsonPathOperator.And => new AndExpression<TIn>(left, right), 
			JsonPathOperator.Divide => new DivideExpression<TIn>(left, right), 
			JsonPathOperator.Exponent => new ExponentExpression<TIn>(left, right), 
			JsonPathOperator.Equal => new IsEqualExpression<TIn>(left, right), 
			JsonPathOperator.GreaterThan => new IsGreaterThanExpression<TIn>(left, right), 
			JsonPathOperator.GreaterThanOrEqual => new IsGreaterThanEqualExpression<TIn>(left, right), 
			JsonPathOperator.LessThan => new IsLessThanExpression<TIn>(left, right), 
			JsonPathOperator.LessThanOrEqual => new IsLessThanEqualExpression<TIn>(left, right), 
			JsonPathOperator.NotEqual => new IsNotEqualExpression<TIn>(left, right), 
			JsonPathOperator.Modulo => new ModuloExpression<TIn>(left, right), 
			JsonPathOperator.Multiply => new MultiplyExpression<TIn>(left, right), 
			JsonPathOperator.Subtract => new SubtractExpression<TIn>(left, right), 
			JsonPathOperator.Or => new OrExpression<TIn>(left, right), 
			_ => throw new NotSupportedException($"Expressions of type {expr.GetType()} are not supported"), 
		};
	}

	private static ExpressionTreeNode<TIn> _VisitNegate<TIn>(ExpressionTreeNode<TIn> left)
	{
		if (left is ValueExpression<TIn> { Value: not null } valueExpression)
		{
			object obj = _Negate(valueExpression.Value);
			if (obj != null)
			{
				return new ValueExpression<TIn>(obj);
			}
		}
		return new NegateExpression<TIn>(left);
	}

	private static object? _Negate(object value)
	{
		if (!(value is byte b))
		{
			if (!(value is sbyte b2))
			{
				if (!(value is short num))
				{
					if (!(value is ushort num2))
					{
						if (!(value is int num3))
						{
							if (!(value is uint num4))
							{
								if (!(value is long num5))
								{
									if (!(value is ulong))
									{
										if (!(value is float num6))
										{
											if (!(value is double num7))
											{
												if (value is decimal num8)
												{
													return -num8;
												}
												return null;
											}
											return 0.0 - num7;
										}
										return 0f - num6;
									}
									return null;
								}
								return -num5;
							}
							return 0L - (long)num4;
						}
						return -num3;
					}
					return -num2;
				}
				return -num;
			}
			return -b2;
		}
		return -b;
	}

	private static void _CheckAndReplaceIfHasPropertyNeeded<TIn>(JsonPathOperator op, ref ExpressionTreeNode<TIn> left, ref ExpressionTreeNode<TIn> right)
	{
		NameExpression<TIn> nameExpression = left as NameExpression<TIn>;
		NameExpression<TIn> nameExpression2 = right as NameExpression<TIn>;
		if (nameExpression == null && nameExpression2 == null)
		{
			return;
		}
		switch (op)
		{
		case JsonPathOperator.And:
		case JsonPathOperator.Not:
		case JsonPathOperator.Or:
			left = _MakeHasPropertyIfNameExpression(left);
			right = _MakeHasPropertyIfNameExpression(right);
			break;
		case JsonPathOperator.Equal:
		case JsonPathOperator.NotEqual:
			if (((nameExpression == null) ^ (nameExpression2 == null)) && ((nameExpression == null) ? _IsBooleanResult(left) : _IsBooleanResult(right)))
			{
				left = _MakeHasPropertyIfNameExpression(left);
				right = _MakeHasPropertyIfNameExpression(right);
			}
			break;
		}
	}

	private static ExpressionTreeNode<TIn> _MakeHasPropertyIfNameExpression<TIn>(ExpressionTreeNode<TIn> node)
	{
		if (node is NameExpression<TIn> nameExpression)
		{
			return new HasPropertyExpression<TIn>(nameExpression.Path, nameExpression.IsLocal, nameExpression.Name);
		}
		return node;
	}

	private static bool _IsBooleanResult<TIn>(ExpressionTreeNode<TIn> node)
	{
		if (node == null)
		{
			return false;
		}
		if (node is ValueExpression<TIn> valueExpression && valueExpression.Value is bool)
		{
			return true;
		}
		if (node is NotExpression<TIn>)
		{
			return true;
		}
		if (node is ExpressionTreeBranch<TIn> expressionTreeBranch)
		{
			if (!(expressionTreeBranch is AndExpression<TIn>) && !(expressionTreeBranch is OrExpression<TIn>) && !(expressionTreeBranch is IsEqualExpression<TIn>) && !(expressionTreeBranch is IsNotEqualExpression<TIn>) && !(expressionTreeBranch is IsGreaterThanEqualExpression<TIn>) && !(expressionTreeBranch is IsGreaterThanExpression<TIn>) && !(expressionTreeBranch is IsLessThanEqualExpression<TIn>))
			{
				return expressionTreeBranch is IsLessThanExpression<TIn>;
			}
			return true;
		}
		return false;
	}
}
