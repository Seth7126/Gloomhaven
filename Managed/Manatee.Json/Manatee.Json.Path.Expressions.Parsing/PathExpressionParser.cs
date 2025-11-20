using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Manatee.Json.Parsing;
using Manatee.Json.Path.ArrayParameters;
using Manatee.Json.Path.Operators;
using Manatee.Json.Path.Parsing;

namespace Manatee.Json.Path.Expressions.Parsing;

internal class PathExpressionParser : IJsonPathExpressionParser
{
	public bool Handles(string input, int index)
	{
		if (input[index] != '@')
		{
			return input[index] == '$';
		}
		return true;
	}

	public bool TryParse<T>(string source, ref int index, [NotNullWhen(true)] out JsonPathExpression? expression, [NotNullWhen(false)] out string? errorMessage)
	{
		bool isLocal = source[index] == '@';
		if (!JsonPathParser.TryParse(source, ref index, out JsonPath path, out errorMessage) && errorMessage != "Unrecognized JSON Path element.")
		{
			expression = null;
			return false;
		}
		IJsonPathOperator jsonPathOperator = path.Operators.Last();
		PathExpression<T> value2;
		if (jsonPathOperator is NameOperator nameOperator)
		{
			path.Operators.Remove(nameOperator);
			if (nameOperator.Name == "indexOf")
			{
				if (source[index] != '(')
				{
					errorMessage = "Expected '('.  'indexOf' operator requires a parameter.";
					expression = null;
					return false;
				}
				index++;
				if (!JsonParser.TryParse(source, ref index, out JsonValue value, out errorMessage, allowExtraChars: true) && errorMessage != "Expected ',', ']', or '}'.")
				{
					errorMessage = "Error parsing parameter for 'indexOf' expression: " + errorMessage + ".";
					expression = null;
					return false;
				}
				if (source[index] != ')')
				{
					errorMessage = "Expected ')'.";
					expression = null;
					return false;
				}
				index++;
				value2 = new IndexOfExpression<T>(path, isLocal, value);
			}
			else
			{
				value2 = new NameExpression<T>(path, isLocal, nameOperator.Name);
			}
		}
		else if (jsonPathOperator is LengthOperator item)
		{
			path.Operators.Remove(item);
			value2 = new LengthExpression<T>(path, isLocal);
		}
		else
		{
			if (!(jsonPathOperator is ArrayOperator arrayOperator))
			{
				throw new NotImplementedException();
			}
			path.Operators.Remove(arrayOperator);
			SliceQuery sliceQuery = arrayOperator.Query as SliceQuery;
			int? num = sliceQuery?.Slices.FirstOrDefault()?.Index;
			if (sliceQuery == null || sliceQuery.Slices.Count() != 1 || !num.HasValue)
			{
				errorMessage = "JSON Path expression indexers only support single constant values.";
				expression = null;
				return false;
			}
			value2 = new ArrayIndexExpression<T>(path, isLocal, num.Value);
		}
		expression = new PathValueExpression<T>(value2);
		return true;
	}
}
