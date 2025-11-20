using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Parsing;

internal class StartParser : IJsonPathParser
{
	public bool Handles(string input, int index)
	{
		if (input[index] != '$')
		{
			return input[index] == '@';
		}
		return true;
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] ref JsonPath? path, [NotNullWhen(false)] out string? errorMessage)
	{
		if (path != null)
		{
			errorMessage = "Start token not valid in the middle of path.";
			return false;
		}
		path = new JsonPath();
		index++;
		errorMessage = null;
		return true;
	}
}
