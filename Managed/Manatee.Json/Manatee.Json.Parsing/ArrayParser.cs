using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal class ArrayParser : IJsonParser
{
	public bool Handles(char c)
	{
		return c == '[';
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars)
	{
		bool flag = false;
		JsonArray jsonArray = new JsonArray();
		value = jsonArray;
		int length = source.Length;
		index++;
		while (index < length)
		{
			errorMessage = source.SkipWhiteSpace(ref index, length, out var ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch == ']')
			{
				if (jsonArray.Count == 0)
				{
					flag = true;
					index++;
					break;
				}
				errorMessage = "Expected value.";
				return false;
			}
			JsonValue value2;
			bool num = JsonParser.TryParse(source, ref index, out value2, out errorMessage);
			jsonArray.Add(value2);
			if (!num)
			{
				return false;
			}
			errorMessage = source.SkipWhiteSpace(ref index, length, out ch);
			if (errorMessage != null)
			{
				return false;
			}
			index++;
			if (ch == ']')
			{
				flag = true;
				break;
			}
			if (ch != ',')
			{
				errorMessage = "Expected ','.";
				return false;
			}
		}
		if (!flag)
		{
			errorMessage = "Unterminated array (missing ']')";
			return false;
		}
		errorMessage = null;
		return true;
	}

	public bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		bool flag = false;
		JsonArray jsonArray = new JsonArray();
		value = jsonArray;
		while (stream.Peek() != -1)
		{
			stream.Read();
			errorMessage = stream.SkipWhiteSpace(out var ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch == ']')
			{
				if (jsonArray.Count == 0)
				{
					flag = true;
					stream.Read();
					break;
				}
				errorMessage = "Expected value.";
				return false;
			}
			JsonValue value2;
			bool num = JsonParser.TryParse(stream, out value2, out errorMessage);
			jsonArray.Add(value2);
			if (!num)
			{
				return false;
			}
			errorMessage = stream.SkipWhiteSpace(out ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch == ']')
			{
				flag = true;
				stream.Read();
				break;
			}
			if (ch != ',')
			{
				errorMessage = "Expected ','.";
				return false;
			}
		}
		if (!flag)
		{
			errorMessage = "Unterminated array (missing ']')";
			return false;
		}
		errorMessage = null;
		return true;
	}

	public async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		char[] scratch = SmallBufferCache.Acquire(1);
		JsonArray array = new JsonArray();
		bool complete = false;
		string errorMessage = null;
		while (stream.Peek() != -1)
		{
			if (token.IsCancellationRequested)
			{
				errorMessage = "Parsing incomplete. The task was cancelled.";
				break;
			}
			await stream.TryRead(scratch, 0, 1, token);
			char c;
			(errorMessage, c) = await stream.SkipWhiteSpaceAsync(scratch);
			if (errorMessage != null)
			{
				break;
			}
			if (c == ']')
			{
				if (array.Count == 0)
				{
					complete = true;
					await stream.TryRead(scratch, 0, 1, token);
				}
				else
				{
					errorMessage = "Expected value.";
				}
				break;
			}
			JsonValue item;
			(errorMessage, item) = await JsonParser.TryParseAsync(stream, token);
			array.Add(item);
			if (errorMessage != null)
			{
				break;
			}
			(errorMessage, c) = await stream.SkipWhiteSpaceAsync(scratch);
			if (errorMessage != null)
			{
				break;
			}
			if (c == ']')
			{
				complete = true;
				await stream.TryRead(scratch, 0, 1, token);
				break;
			}
			if (c != ',')
			{
				errorMessage = "Expected ','.";
				break;
			}
		}
		if (!complete && errorMessage == null)
		{
			errorMessage = "Unterminated array (missing ']')";
		}
		SmallBufferCache.Release(scratch);
		return (errorMessage: errorMessage, value: array);
	}
}
