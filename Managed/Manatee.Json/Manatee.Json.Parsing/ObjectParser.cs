using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal class ObjectParser : IJsonParser
{
	public bool Handles(char c)
	{
		return c == '{';
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars)
	{
		bool flag = false;
		JsonObject jsonObject = new JsonObject();
		value = jsonObject;
		int length = source.Length;
		index++;
		while (index < length)
		{
			errorMessage = source.SkipWhiteSpace(ref index, length, out var ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch == '}')
			{
				if (jsonObject.Count == 0)
				{
					flag = true;
					index++;
					break;
				}
				errorMessage = "Expected key.";
				return false;
			}
			errorMessage = source.SkipWhiteSpace(ref index, length, out ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch != '"')
			{
				errorMessage = "Expected key.";
				return false;
			}
			if (!JsonParser.TryParse(source, ref index, out JsonValue value2, out errorMessage))
			{
				return false;
			}
			string key = value2.String;
			errorMessage = source.SkipWhiteSpace(ref index, length, out ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch != ':')
			{
				jsonObject.Add(key, null);
				errorMessage = "Expected ':'.";
				return false;
			}
			index++;
			bool num = JsonParser.TryParse(source, ref index, out value2, out errorMessage);
			jsonObject.Add(key, value2);
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
			if (ch == '}')
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
			errorMessage = "Unterminated object (missing '}').";
			return false;
		}
		errorMessage = null;
		return true;
	}

	public bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		bool flag = false;
		JsonObject jsonObject = new JsonObject();
		value = jsonObject;
		while (stream.Peek() != -1)
		{
			stream.Read();
			errorMessage = stream.SkipWhiteSpace(out var ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch == '}')
			{
				if (jsonObject.Count == 0)
				{
					flag = true;
					stream.Read();
					break;
				}
				errorMessage = "Expected key.";
				return false;
			}
			errorMessage = stream.SkipWhiteSpace(out ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch != '"')
			{
				errorMessage = "Expected key.";
				return false;
			}
			if (!JsonParser.TryParse(stream, out JsonValue value2, out errorMessage))
			{
				return false;
			}
			string key = value2.String;
			errorMessage = stream.SkipWhiteSpace(out ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch != ':')
			{
				jsonObject.Add(key, null);
				errorMessage = "Expected ':'.";
				return false;
			}
			stream.Read();
			bool num = JsonParser.TryParse(stream, out value2, out errorMessage);
			jsonObject.Add(key, value2);
			if (!num)
			{
				return false;
			}
			errorMessage = stream.SkipWhiteSpace(out ch);
			if (errorMessage != null)
			{
				return false;
			}
			if (ch == '}')
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
			errorMessage = "Unterminated object (missing '}').";
			return false;
		}
		errorMessage = null;
		return true;
	}

	public async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		bool complete = false;
		JsonObject obj = new JsonObject();
		char[] scratch = SmallBufferCache.Acquire(1);
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
			if (c == '}')
			{
				if (obj.Count == 0)
				{
					complete = true;
					await stream.TryRead(scratch, 0, 1, token);
				}
				else
				{
					errorMessage = "Expected key.";
				}
				break;
			}
			(errorMessage, c) = await stream.SkipWhiteSpaceAsync(scratch);
			if (errorMessage != null)
			{
				break;
			}
			if (c != '"')
			{
				errorMessage = "Expected key.";
				break;
			}
			JsonValue jsonValue;
			(errorMessage, jsonValue) = await JsonParser.TryParseAsync(stream, token);
			if (errorMessage != null)
			{
				break;
			}
			string key = jsonValue.String;
			(errorMessage, c) = await stream.SkipWhiteSpaceAsync(scratch);
			if (errorMessage != null)
			{
				break;
			}
			if (c != ':')
			{
				obj.Add(key, null);
				errorMessage = "Expected ':'.";
				break;
			}
			await stream.TryRead(scratch, 0, 1, token);
			(errorMessage, jsonValue) = await JsonParser.TryParseAsync(stream, token);
			obj.Add(key, jsonValue);
			if (errorMessage != null)
			{
				break;
			}
			(errorMessage, c) = await stream.SkipWhiteSpaceAsync(scratch);
			if (errorMessage != null)
			{
				break;
			}
			if (c == '}')
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
			errorMessage = "Unterminated object (missing '}').";
		}
		SmallBufferCache.Release(scratch);
		return (errorMessage: errorMessage, value: obj);
	}
}
