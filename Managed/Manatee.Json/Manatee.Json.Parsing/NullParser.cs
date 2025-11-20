using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal class NullParser : IJsonParser
{
	private const string _unexpectedEndOfInput = "Unexpected end of input.";

	public bool Handles(char c)
	{
		if (c != 'n')
		{
			return c == 'N';
		}
		return true;
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars)
	{
		value = null;
		if (index + 4 > source.Length)
		{
			errorMessage = "Unexpected end of input.";
			return false;
		}
		if (source.IndexOf("null", index, 4, StringComparison.OrdinalIgnoreCase) != index)
		{
			errorMessage = "Value not recognized: '" + source.Substring(index, 4) + "'.";
			return false;
		}
		index += 4;
		value = JsonValue.Null;
		errorMessage = null;
		return true;
	}

	public bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		value = null;
		char[] array = SmallBufferCache.Acquire(4);
		if (stream.ReadBlock(array, 0, 4) != 4)
		{
			SmallBufferCache.Release(array);
			errorMessage = "Unexpected end of input.";
			return false;
		}
		if ((array[0] == 'n' || array[0] == 'N') && (array[1] == 'u' || array[1] == 'U') && (array[2] == 'l' || array[2] == 'L') && (array[3] == 'l' || array[3] == 'L'))
		{
			value = JsonValue.Null;
			SmallBufferCache.Release(array);
			errorMessage = null;
			return true;
		}
		errorMessage = "Value not recognized: '" + new string(array).Trim(new char[1]) + "'.";
		return false;
	}

	public async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		char[] buffer = SmallBufferCache.Acquire(4);
		if (await stream.ReadBlockAsync(buffer, 0, 4) < 4)
		{
			SmallBufferCache.Release(buffer);
			return (errorMessage: "Unexpected end of input.", value: null);
		}
		JsonValue item = null;
		string item2 = null;
		if ((buffer[0] == 'n' || buffer[0] == 'N') && (buffer[1] == 'u' || buffer[1] == 'U') && (buffer[2] == 'l' || buffer[2] == 'L') && (buffer[3] == 'l' || buffer[3] == 'L'))
		{
			item = JsonValue.Null;
		}
		else
		{
			item2 = "Value not recognized: '" + new string(buffer).Trim(new char[1]) + "'.";
		}
		SmallBufferCache.Release(buffer);
		return (errorMessage: item2, value: item);
	}
}
