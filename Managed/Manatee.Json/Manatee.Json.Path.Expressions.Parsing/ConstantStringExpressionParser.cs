using Manatee.Json.Path.Parsing;

namespace Manatee.Json.Path.Expressions.Parsing;

internal class ConstantStringExpressionParser : IJsonPathExpressionParser
{
	public bool Handles(string input, int index)
	{
		if (input[index] != '"')
		{
			return input[index] == '\'';
		}
		return true;
	}

	public bool TryParse<TIn>(string source, ref int index, out JsonPathExpression? expression, out string? errorMessage)
	{
		if (!source.TryGetKey(ref index, out string key, out errorMessage))
		{
			expression = null;
			return false;
		}
		expression = new ValueExpression(key);
		return true;
	}
}
