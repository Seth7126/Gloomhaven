using System.Diagnostics.CodeAnalysis;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Parsing;

internal class PathObjectParser : IJsonPathParser
{
	private static readonly string allowedChars = "_'\"*";

	public bool Handles(string input, int index)
	{
		if (index + 1 >= input.Length)
		{
			return false;
		}
		int num = index + 1;
		if (input[index] == '.')
		{
			if (input[index + 1] == '[' && input[index + 2].In('\'', '"'))
			{
				num++;
			}
			if (!char.IsLetterOrDigit(input[num]))
			{
				return allowedChars.IndexOf(input[num]) >= 0;
			}
			return true;
		}
		if (input[index] == '[' && input[index + 1].In('\'', '"'))
		{
			if (!char.IsLetterOrDigit(input[num]))
			{
				return allowedChars.IndexOf(input[num]) >= 0;
			}
			return true;
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
		if (source[index] == '.')
		{
			index++;
		}
		bool flag = false;
		if (source[index] == '[')
		{
			flag = true;
			index++;
		}
		if (source[index] == '*')
		{
			path = path.Name();
			index++;
			errorMessage = null;
			return true;
		}
		if (!source.TryGetKey(ref index, out string key, out errorMessage))
		{
			return false;
		}
		if (flag)
		{
			if (source[index] != ']')
			{
				errorMessage = "Expected close bracket.";
				return false;
			}
			index++;
		}
		path = path.Name(key);
		return true;
	}
}
