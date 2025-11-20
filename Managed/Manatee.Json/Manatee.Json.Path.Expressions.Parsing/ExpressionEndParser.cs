namespace Manatee.Json.Path.Expressions.Parsing;

internal class ExpressionEndParser : IJsonPathExpressionParser
{
	public bool Handles(string input, int index)
	{
		if (input[index] != ')')
		{
			return input[index] == ']';
		}
		return true;
	}

	public bool TryParse<TIn>(string source, ref int index, out JsonPathExpression? expression, out string? errorMessage)
	{
		errorMessage = null;
		if (source[index] == ']')
		{
			expression = null;
			return true;
		}
		index++;
		expression = new OperatorExpression
		{
			Operator = JsonPathOperator.GroupEnd
		};
		return true;
	}
}
