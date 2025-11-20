using System.Diagnostics.CodeAnalysis;
using Manatee.Json.Path.ArrayParameters;
using Manatee.Json.Path.Expressions;
using Manatee.Json.Path.Expressions.Parsing;
using Manatee.Json.Path.Operators;

namespace Manatee.Json.Path.Parsing;

internal class ExpressionIndexParser : IJsonPathParser
{
	public bool Handles(string input, int index)
	{
		if (index + 1 >= input.Length)
		{
			return false;
		}
		if (input[index] == '[')
		{
			return input[index + 1] == '(';
		}
		return false;
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] ref JsonPath? path, [NotNullWhen(false)] out string? errorMessage)
	{
		if (path == null)
		{
			errorMessage = "Start token not found.";
			return false;
		}
		index++;
		if (!JsonPathExpressionParser.TryParse(source, ref index, out Expression<int, JsonArray> expr, out errorMessage))
		{
			return false;
		}
		if (index >= source.Length)
		{
			errorMessage = "Unexpected end of input.";
			return false;
		}
		if (source[index] != ']')
		{
			errorMessage = "Expected ']'";
			return false;
		}
		index++;
		path.Operators.Add(new ArrayOperator(new IndexExpressionQuery(expr)));
		return true;
	}
}
