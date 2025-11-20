using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal class StringParser : IJsonParser
{
	public bool Handles(char c)
	{
		return c == '"';
	}

	public bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars)
	{
		value = null;
		bool flag = false;
		bool flag2 = false;
		int num = ++index;
		while (index < source.Length)
		{
			if (source[index] == '\\')
			{
				flag2 = true;
				break;
			}
			if (source[index] == '"')
			{
				flag = true;
				break;
			}
			index++;
		}
		if (!flag2)
		{
			if (!flag)
			{
				errorMessage = "Could not find end of string value.";
				return false;
			}
			value = source.Substring(num, index - num);
			index++;
			errorMessage = null;
			return true;
		}
		index = num;
		return _TryParseInterpretedString(source, ref index, out value, out errorMessage);
	}

	public bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		value = null;
		stream.Read();
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		bool flag = false;
		bool flag2 = false;
		while (stream.Peek() != -1)
		{
			char c = (char)stream.Peek();
			if (c == '\\')
			{
				flag2 = true;
				break;
			}
			stream.Read();
			if (c == '"')
			{
				flag = true;
				break;
			}
			stringBuilder.Append(c);
		}
		if (!flag2)
		{
			if (!flag)
			{
				errorMessage = "Could not find end of string value.";
				return false;
			}
			value = StringBuilderCache.GetStringAndRelease(stringBuilder);
			errorMessage = null;
			return true;
		}
		return _TryParseInterpretedString(stringBuilder, stream, out value, out errorMessage);
	}

	public async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		char[] scratch = SmallBufferCache.Acquire(4);
		await stream.TryRead(scratch, 0, 1, token);
		StringBuilder builder = StringBuilderCache.Acquire();
		bool complete = false;
		bool mustInterpret = false;
		while (stream.Peek() != -1)
		{
			char c = (char)stream.Peek();
			if (c == '\\')
			{
				mustInterpret = true;
				break;
			}
			await stream.TryRead(scratch, 0, 1, token);
			if (c == '"')
			{
				complete = true;
				break;
			}
			builder.Append(c);
		}
		if (!mustInterpret)
		{
			SmallBufferCache.Release(scratch);
			string item = null;
			JsonValue item2 = null;
			if (!complete)
			{
				item = "Could not find end of string value.";
			}
			else
			{
				item2 = StringBuilderCache.GetStringAndRelease(builder);
			}
			return (errorMessage: item, value: item2);
		}
		return await _TryParseInterpretedStringAsync(builder, stream, scratch);
	}

	private static bool _TryParseInterpretedString(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		value = null;
		errorMessage = null;
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		bool flag = false;
		while (index < source.Length)
		{
			char c = source[index++];
			switch (c)
			{
			case '"':
				flag = true;
				break;
			default:
				stringBuilder.Append(c);
				continue;
			case '\\':
			{
				if (index >= source.Length)
				{
					errorMessage = "Could not find end of string value.";
					return false;
				}
				string text = null;
				c = source[index++];
				switch (c)
				{
				case 'b':
					text = "\b";
					break;
				case 'f':
					text = "\f";
					break;
				case 'n':
					text = "\n";
					break;
				case 'r':
					text = "\r";
					break;
				case 't':
					text = "\t";
					break;
				case 'u':
				{
					int num = 4;
					if (index + num >= source.Length)
					{
						errorMessage = $"Invalid escape sequence: '\\{c}{source.Substring(index)}'.";
						break;
					}
					if (!_IsValidHex(source, index, 4) || !int.TryParse(source.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
					{
						errorMessage = $"Invalid escape sequence: '\\{c}{source.Substring(index, num)}'.";
						break;
					}
					if (index + num + 2 < source.Length && source.IndexOf("\\u", index + num, 2, StringComparison.InvariantCulture) == index + num)
					{
						num += 6;
						if (index + num >= source.Length)
						{
							errorMessage = $"Invalid escape sequence: '\\{c}{source.Substring(index)}'.";
							break;
						}
						if (!_IsValidHex(source, index + 6, 4) || !int.TryParse(source.Substring(index + 6, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result2))
						{
							errorMessage = $"Invalid escape sequence: '\\{c}{source.Substring(index, num)}'.";
							break;
						}
						int num2 = StringExtensions.CalculateUtf32(result, result2);
						if (num2.IsValidUtf32CodePoint())
						{
							result = num2;
						}
						else
						{
							num -= 6;
						}
					}
					text = char.ConvertFromUtf32(result);
					index += num;
					break;
				}
				case '"':
					text = "\"";
					break;
				case '\\':
					text = "\\";
					break;
				case '/':
					text = "/";
					break;
				default:
					flag = true;
					errorMessage = $"Invalid escape sequence: '\\{c}'.";
					break;
				}
				if (text != null)
				{
					stringBuilder.Append(text);
					continue;
				}
				break;
			}
			}
			break;
		}
		if (!flag || errorMessage != null)
		{
			value = null;
			StringBuilderCache.Release(stringBuilder);
			if (errorMessage == null)
			{
				errorMessage = "Could not find end of string value.";
			}
			return false;
		}
		value = StringBuilderCache.GetStringAndRelease(stringBuilder);
		return true;
	}

	private static bool _TryParseInterpretedString(StringBuilder builder, TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		value = null;
		bool flag = false;
		int? num = null;
		while (stream.Peek() != -1)
		{
			char c = (char)stream.Read();
			if (c == '\\')
			{
				if (stream.Peek() == -1)
				{
					StringBuilderCache.Release(builder);
					errorMessage = "Could not find end of string value.";
					return false;
				}
				char c2 = (char)stream.Peek();
				if (_MustInterpretComplex(c2))
				{
					if (c2 != 'u')
					{
						StringBuilderCache.Release(builder);
						errorMessage = $"Invalid escape sequence: '\\{c2}'.";
						return false;
					}
					char[] array = SmallBufferCache.Acquire(4);
					stream.Read();
					if (4 != stream.Read(array, 0, 4))
					{
						StringBuilderCache.Release(builder);
						errorMessage = "Could not find end of string value.";
						return false;
					}
					string text = new string(array, 0, 4);
					if (!_IsValidHex(text, 0, 4) || !int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
					{
						StringBuilderCache.Release(builder);
						errorMessage = $"Invalid escape sequence: '\\{c2}{text}'.";
						return false;
					}
					if (num.HasValue)
					{
						int num2 = StringExtensions.CalculateUtf32(num.Value, result);
						if (num2.IsValidUtf32CodePoint())
						{
							builder.Append(char.ConvertFromUtf32(num2));
						}
						else
						{
							builder.Append((char)num.Value);
							builder.Append((char)result);
						}
						num = null;
					}
					else
					{
						num = result;
					}
					SmallBufferCache.Release(array);
					continue;
				}
				stream.Read();
				c = _InterpretSimpleEscapeSequence(c2);
			}
			else if (c == '"')
			{
				flag = true;
				break;
			}
			if (num.HasValue)
			{
				builder.Append(char.ConvertFromUtf32(num.Value));
				num = null;
			}
			builder.Append(c);
		}
		if (num.HasValue)
		{
			builder.Append(char.ConvertFromUtf32(num.Value));
		}
		if (!flag)
		{
			value = null;
			StringBuilderCache.Release(builder);
			errorMessage = "Could not find end of string value.";
			return false;
		}
		value = StringBuilderCache.GetStringAndRelease(builder);
		errorMessage = null;
		return true;
	}

	private static bool _IsValidHex(string source, int offset, int count)
	{
		for (int i = offset; i < offset + count; i++)
		{
			if ((source[i] < '0' || source[i] > '9') && (source[i] < 'A' || source[i] > 'F') && (source[i] < 'a' || source[i] > 'f'))
			{
				return false;
			}
		}
		return true;
	}

	private static bool _MustInterpretComplex(char lookAhead)
	{
		switch (lookAhead)
		{
		case '"':
		case '/':
		case '\\':
			return false;
		case 'b':
		case 'f':
		case 'n':
		case 'r':
		case 't':
			return false;
		case 'u':
			return true;
		default:
			return true;
		}
	}

	private static char _InterpretSimpleEscapeSequence(char lookAhead)
	{
		return lookAhead switch
		{
			'b' => '\b', 
			'f' => '\f', 
			'n' => '\n', 
			'r' => '\r', 
			't' => '\t', 
			_ => lookAhead, 
		};
	}

	private static async Task<(string? errorMessage, JsonValue? value)> _TryParseInterpretedStringAsync(StringBuilder builder, TextReader stream, char[] scratch)
	{
		bool complete = false;
		int? previousHex = null;
		while (stream.Peek() != -1)
		{
			await stream.TryRead(scratch, 0, 1);
			char c = scratch[0];
			if (c == '\\')
			{
				if (stream.Peek() == -1)
				{
					StringBuilderCache.Release(builder);
					SmallBufferCache.Release(scratch);
					return (errorMessage: "Could not find end of string value.", value: null);
				}
				char lookAhead = (char)stream.Peek();
				if (_MustInterpretComplex(lookAhead))
				{
					if (lookAhead != 'u')
					{
						StringBuilderCache.Release(builder);
						SmallBufferCache.Release(scratch);
						return (errorMessage: $"Invalid escape sequence: '\\{lookAhead}'.", value: null);
					}
					await stream.TryRead(scratch, 0, 1);
					if (await stream.ReadAsync(scratch, 0, 4) < 4)
					{
						StringBuilderCache.Release(builder);
						SmallBufferCache.Release(scratch);
						return (errorMessage: "Could not find end of string value.", value: null);
					}
					int num = int.Parse(new string(scratch, 0, 4), NumberStyles.HexNumber);
					if (previousHex.HasValue)
					{
						int num2 = StringExtensions.CalculateUtf32(previousHex.Value, num);
						if (num2.IsValidUtf32CodePoint())
						{
							builder.Append(char.ConvertFromUtf32(num2));
						}
						else
						{
							builder.Append((char)previousHex.Value);
							builder.Append((char)num);
						}
						previousHex = null;
					}
					else
					{
						previousHex = num;
					}
					continue;
				}
				await stream.TryRead(scratch, 0, 1);
				c = _InterpretSimpleEscapeSequence(lookAhead);
			}
			else if (c == '"')
			{
				complete = true;
				break;
			}
			if (previousHex.HasValue)
			{
				builder.Append(char.ConvertFromUtf32(previousHex.Value));
				previousHex = null;
			}
			builder.Append(c);
		}
		SmallBufferCache.Release(scratch);
		if (previousHex.HasValue)
		{
			builder.Append(char.ConvertFromUtf32(previousHex.Value));
		}
		if (!complete)
		{
			StringBuilderCache.Release(builder);
			return (errorMessage: "Could not find end of string value.", value: null);
		}
		JsonValue item = StringBuilderCache.GetStringAndRelease(builder);
		return (errorMessage: null, value: item);
	}
}
