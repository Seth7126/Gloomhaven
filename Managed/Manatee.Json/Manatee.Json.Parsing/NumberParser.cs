using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal class NumberParser : IJsonParser
{
	public bool Handles(char c)
	{
		if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' && c != '8' && c != '9')
		{
			return c == '-';
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
		int num = index;
		while (index < source.Length)
		{
			char c = source[index];
			if (char.IsWhiteSpace(c) || c == ',' || c == ']' || c == '}')
			{
				break;
			}
			bool flag = _IsNumberChar(c);
			if (!flag && allowExtraChars)
			{
				break;
			}
			if (!flag)
			{
				errorMessage = "Expected ',', ']', or '}'.";
				return false;
			}
			index++;
		}
		string text = source.Substring(num, index - num);
		if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			errorMessage = "Value not recognized: '" + text + "'";
			return false;
		}
		value = result;
		errorMessage = null;
		return true;
	}

	public bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		value = null;
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		while (stream.Peek() != -1)
		{
			char c = (char)stream.Peek();
			if (char.IsWhiteSpace(c) || c == ',' || c == ']' || c == '}')
			{
				break;
			}
			stream.Read();
			if (!_IsNumberChar(c))
			{
				StringBuilderCache.Release(stringBuilder);
				errorMessage = "Expected ',', ']', or '}'.";
				return false;
			}
			stringBuilder.Append(c);
		}
		string stringAndRelease = StringBuilderCache.GetStringAndRelease(stringBuilder);
		if (!double.TryParse(stringAndRelease, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			errorMessage = "Value not recognized: '" + stringAndRelease + "'";
			return false;
		}
		value = result;
		errorMessage = null;
		return true;
	}

	public async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		StringBuilder buffer = StringBuilderCache.Acquire();
		char[] scratch = SmallBufferCache.Acquire(1);
		string errorMessage = null;
		while (stream.Peek() != -1)
		{
			if (token.IsCancellationRequested)
			{
				errorMessage = "Parsing incomplete. The task was cancelled.";
				break;
			}
			char c = (char)stream.Peek();
			if (char.IsWhiteSpace(c) || c == ',' || c == ']' || c == '}')
			{
				break;
			}
			await stream.TryRead(scratch, 0, 1, token);
			if (!_IsNumberChar(c))
			{
				errorMessage = "Expected ',', ']', or '}'.";
				break;
			}
			buffer.Append(c);
		}
		SmallBufferCache.Release(scratch);
		if (errorMessage != null)
		{
			StringBuilderCache.Release(buffer);
			return (errorMessage: errorMessage, value: null);
		}
		string stringAndRelease = StringBuilderCache.GetStringAndRelease(buffer);
		if (!double.TryParse(stringAndRelease, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return (errorMessage: "Value not recognized: '" + stringAndRelease + "'", value: null);
		}
		return (errorMessage: null, value: result);
	}

	private static bool _IsNumberChar(char c)
	{
		if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' && c != '8' && c != '9' && c != '-' && c != '+' && c != '.' && c != 'e')
		{
			return c == 'E';
		}
		return true;
	}
}
