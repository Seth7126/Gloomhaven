using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal class BoolParser : IJsonParser
{
	private const string _unexpectedEndOfInput = "Unexpected end of input.";

	public bool Handles(char c)
	{
		if (c != 't' && c != 'T' && c != 'f')
		{
			return c == 'F';
		}
		return true;
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars)
	{
		if (index >= source.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		value = null;
		if (source[index] == 't' || source[index] == 'T')
		{
			if (index + 4 > source.Length)
			{
				errorMessage = "Unexpected end of input.";
				return false;
			}
			if (source.IndexOf("true", index, 4, StringComparison.OrdinalIgnoreCase) != index)
			{
				errorMessage = "Value not recognized: '" + source.Substring(index, 4) + "'.";
				return false;
			}
			index += 4;
			value = true;
		}
		else
		{
			if (index + 5 > source.Length)
			{
				errorMessage = "Unexpected end of input.";
				return false;
			}
			if (source.IndexOf("false", index, 5, StringComparison.OrdinalIgnoreCase) != index)
			{
				errorMessage = "Value not recognized: '" + source.Substring(index, 5) + "'.";
				return false;
			}
			index += 5;
			value = false;
		}
		errorMessage = null;
		return true;
	}

	public bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		value = null;
		char c = (char)stream.Peek();
		int num = ((c != 't' && c != 'T') ? 5 : 4);
		char[] array = SmallBufferCache.Acquire(num);
		if (stream.ReadBlock(array, 0, num) != num)
		{
			SmallBufferCache.Release(array);
			errorMessage = "Unexpected end of input.";
			return false;
		}
		if (num == 4)
		{
			if ((array[0] != 't' && array[0] != 'T') || (array[1] != 'r' && array[1] != 'R') || (array[2] != 'u' && array[2] != 'U') || (array[3] != 'e' && array[3] != 'E'))
			{
				errorMessage = "Value not recognized: '" + new string(array, 0, num) + "'.";
				return false;
			}
			value = true;
		}
		else
		{
			if ((array[0] != 'f' && array[0] != 'F') || (array[1] != 'a' && array[1] != 'A') || (array[2] != 'l' && array[2] != 'L') || (array[3] != 's' && array[3] != 'S') || (array[4] != 'e' && array[4] != 'E'))
			{
				errorMessage = "Value not recognized: '" + new string(array, 0, num) + "'.";
				return false;
			}
			value = false;
		}
		SmallBufferCache.Release(array);
		errorMessage = null;
		return true;
	}

	public async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		char[] buffer = SmallBufferCache.Acquire(5);
		int num = await stream.ReadAsync(buffer, 0, 4);
		if (num < 4)
		{
			SmallBufferCache.Release(buffer);
			return (errorMessage: "Unexpected end of input.", value: null);
		}
		if (token.IsCancellationRequested)
		{
			SmallBufferCache.Release(buffer);
			return (errorMessage: "Parsing incomplete. The task was cancelled.", value: null);
		}
		JsonValue value = null;
		string errorMessage = null;
		if ((buffer[0] == 't' || buffer[0] == 'T') && (buffer[1] == 'r' || buffer[1] == 'R') && (buffer[2] == 'u' || buffer[2] == 'U') && (buffer[3] == 'e' || buffer[3] == 'E'))
		{
			value = true;
		}
		else if ((buffer[0] == 'f' || buffer[0] == 'F') && (buffer[1] == 'a' || buffer[1] == 'A') && (buffer[2] == 'l' || buffer[2] == 'L') && (buffer[3] == 's' || buffer[3] == 'S'))
		{
			if (await stream.TryRead(buffer, 4, 1, token))
			{
				if (buffer[4] == 'e' || buffer[4] == 'E')
				{
					value = false;
				}
				else
				{
					errorMessage = $"Value not recognized: 'fals{buffer[4]}'.";
				}
			}
			else
			{
				errorMessage = "Unexpected end of input.";
			}
		}
		else
		{
			errorMessage = "Value not recognized: '" + new string(buffer, 0, num) + "'.";
		}
		SmallBufferCache.Release(buffer);
		return (errorMessage: errorMessage, value: value);
	}
}
