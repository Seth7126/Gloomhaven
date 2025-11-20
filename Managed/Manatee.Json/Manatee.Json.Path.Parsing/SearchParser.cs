using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Parsing;

internal class SearchParser : IJsonPathParser
{
	private const string _allowedChars = "_'\"*";

	public bool Handles(string input, int index)
	{
		if (index + 2 >= input.Length)
		{
			return false;
		}
		int num = index + 2;
		if (input[index] != '.' || input[index + 1] != '.')
		{
			return false;
		}
		if (input[index + 2] == '[' && "_'\"*".IndexOf(input[index + 3]) >= 0)
		{
			num++;
		}
		if (!char.IsLetterOrDigit(input[num]))
		{
			return "_'\"*".IndexOf(input[num]) >= 0;
		}
		return true;
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] ref JsonPath? path, [NotNullWhen(false)] out string? errorMessage)
	{
		if (path == null)
		{
			errorMessage = "Start token not found.";
			return false;
		}
		index += 2;
		if (source[index] == '*')
		{
			path = path.Search();
			index++;
			errorMessage = null;
			return true;
		}
		if (!source.TryGetKey(ref index, out string key, out errorMessage))
		{
			return false;
		}
		path = path.Search(key);
		return true;
	}
}
