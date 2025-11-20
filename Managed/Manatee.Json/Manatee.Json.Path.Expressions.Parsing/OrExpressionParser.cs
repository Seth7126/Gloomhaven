namespace Manatee.Json.Path.Expressions.Parsing;

internal class OrExpressionParser : IJsonPathExpressionParser
{
	public bool Handles(string input, int index)
	{
		if (index + 1 < input.Length && input[index] == '|')
		{
			return input[index + 1] == '|';
		}
		return false;
	}

	public bool TryParse<TIn>(string source, ref int index, out JsonPathExpression? expression, out string? errorMessage)
	{
		index += 2;
		expression = new OperatorExpression
		{
			Operator = JsonPathOperator.Or
		};
		errorMessage = null;
		return true;
	}
}
