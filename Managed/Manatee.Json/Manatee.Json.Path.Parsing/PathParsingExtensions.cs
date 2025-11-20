using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Parsing;

internal static class PathParsingExtensions
{
	private enum NumberPart
	{
		LeadingInteger,
		Fraction,
		ExponentDigitsOrSign,
		ExponentDigitsOnly
	}

	public static bool TryGetKey(this string source, ref int index, [NotNullWhen(true)] out string key, [NotNullWhen(false)] out string errorMessage)
	{
		if (source[index] == '"' || source[index] == '\'')
		{
			return _TryGetQuotedKey(source, ref index, out key, out errorMessage);
		}
		return _TryGetBasicKey(source, ref index, out key, out errorMessage);
	}

	private static bool _TryGetBasicKey(string source, ref int index, [NotNullWhen(true)] out string key, [NotNullWhen(false)] out string errorMessage)
	{
		int num = index;
		bool flag = false;
		while (index < source.Length)
		{
			if (char.IsLetterOrDigit(source[index]) || source[index] == '_')
			{
				index++;
				continue;
			}
			flag = true;
			break;
		}
		if (!flag && index + 1 < source.Length)
		{
			key = null;
			errorMessage = $"The character {source[index]} is not supported for unquoted names.";
			return false;
		}
		key = source.Substring(num, index - num);
		errorMessage = null;
		return true;
	}

	private static bool _TryGetQuotedKey(string source, ref int index, [NotNullWhen(true)] out string key, [NotNullWhen(false)] out string errorMessage)
	{
		char c = source[index];
		index++;
		int num = index;
		bool flag = false;
		bool flag2 = false;
		while (index < source.Length)
		{
			if (source[index] == '\\')
			{
				flag2 = true;
				break;
			}
			if (source[index] == c)
			{
				flag = true;
				break;
			}
			index++;
		}
		if (flag2)
		{
			return _TryGetQuotedKeyWithEscape(source, c, num, ref index, out key, out errorMessage);
		}
		if (!flag)
		{
			key = null;
			errorMessage = "Could not find end of string value.";
			return false;
		}
		key = source.Substring(num, index - num);
		errorMessage = null;
		index++;
		return true;
	}

	private static bool _TryGetQuotedKeyWithEscape(string source, char quoteChar, int originalIndex, ref int index, [NotNullWhen(true)] out string key, [NotNullWhen(false)] out string errorMessage)
	{
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(source.Substring(originalIndex, index - originalIndex));
		bool flag = false;
		errorMessage = null;
		while (index < source.Length)
		{
			char c = source[index++];
			if (c != '\\')
			{
				if (c == quoteChar)
				{
					flag = true;
					break;
				}
				stringBuilder.Append(c);
				continue;
			}
			if (index >= source.Length)
			{
				key = null;
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
				if (index + num + 2 < source.Length && source.IndexOf("\\u", index + num, 2) == index + num)
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
					result = StringExtensions.CalculateUtf32(result, result2);
				}
				if (result.IsValidUtf32CodePoint())
				{
					text = char.ConvertFromUtf32(result);
					index += num;
				}
				else
				{
					errorMessage = "Invalid UTF-32 code point.";
				}
				break;
			}
			case '\'':
				text = "'";
				break;
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
			if (text == null)
			{
				break;
			}
			stringBuilder.Append(text);
		}
		if (!flag || errorMessage != null)
		{
			StringBuilderCache.Release(stringBuilder);
			key = null;
			if (errorMessage == null)
			{
				errorMessage = "Could not find end of string value.";
			}
			return false;
		}
		key = StringBuilderCache.GetStringAndRelease(stringBuilder);
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

	public static bool TryGetSlices(this string source, ref int index, out IList<Slice> slices, [NotNullWhen(false)] out string errorMessage)
	{
		slices = new List<Slice>();
		Slice slice;
		do
		{
			index++;
			if (source._TryGetSlice(ref index, out slice, out errorMessage) && slice != null)
			{
				slices.Add(slice);
			}
		}
		while (errorMessage == null && slice != null);
		if (errorMessage != null)
		{
			return false;
		}
		if (slices.Any())
		{
			return true;
		}
		errorMessage = "Index required inside '[]'";
		return false;
	}

	private static bool _TryGetSlice(this string source, ref int index, out Slice? slice, [NotNullWhen(false)] out string errorMessage)
	{
		slice = null;
		if (source[index - 1] == ']')
		{
			errorMessage = null;
			return true;
		}
		if (!_TryGetInt(source, ref index, out var number, out errorMessage))
		{
			return false;
		}
		if (index >= source.Length)
		{
			errorMessage = "Expected ':', ',', or ']'.";
			return false;
		}
		if (number.HasValue && (source[index] == ',' || source[index] == ']'))
		{
			slice = new Slice(number.Value);
			return true;
		}
		if (source[index] != ':')
		{
			errorMessage = "Expected ':', ',', or ']'.";
			return false;
		}
		index++;
		if (!_TryGetInt(source, ref index, out var number2, out errorMessage))
		{
			return false;
		}
		if (source[index] == ',' || source[index] == ']')
		{
			slice = new Slice(number, number2);
			return true;
		}
		if (source[index] != ':')
		{
			errorMessage = "Expected ':', ',', or ']'.";
			return false;
		}
		index++;
		if (!_TryGetInt(source, ref index, out var number3, out errorMessage))
		{
			return false;
		}
		if (source[index] == ',' || source[index] == ']')
		{
			slice = new Slice(number, number2, number3);
			return true;
		}
		errorMessage = "Expected ',' or ']'.";
		return false;
	}

	private static bool _TryGetInt(string source, ref int index, out int? number, [NotNullWhen(false)] out string errorMessage)
	{
		number = null;
		int num = index;
		if (source[index] == '-')
		{
			index++;
		}
		while (index < source.Length && char.IsDigit(source[index]))
		{
			index++;
		}
		if (index - num == 0 && (source[index] == ':' || source[index] == ',' || source[index] == ']'))
		{
			errorMessage = null;
			return true;
		}
		if (!int.TryParse(source.Substring(num, index - num), out var result))
		{
			errorMessage = "Expected number.";
			return false;
		}
		number = result;
		errorMessage = null;
		return true;
	}

	public static bool TryGetNumber(this string source, ref int index, out double number, [NotNullWhen(false)] out string errorMessage)
	{
		int num = index;
		if (source[index] == '-')
		{
			index++;
		}
		bool flag = false;
		NumberPart numberPart = NumberPart.LeadingInteger;
		while (index < source.Length && !flag)
		{
			switch (numberPart)
			{
			case NumberPart.LeadingInteger:
				if (char.IsDigit(source[index]))
				{
					index++;
				}
				else if (source[index] == '.')
				{
					numberPart = NumberPart.Fraction;
					index++;
				}
				else if (source[index] == 'e' || source[index] == 'E')
				{
					numberPart = NumberPart.ExponentDigitsOrSign;
					index++;
				}
				else
				{
					flag = true;
				}
				break;
			case NumberPart.Fraction:
				if (char.IsDigit(source[index]))
				{
					index++;
				}
				else if (source[index] == 'e' || source[index] == 'E')
				{
					numberPart = NumberPart.ExponentDigitsOrSign;
					index++;
				}
				else
				{
					flag = true;
				}
				break;
			case NumberPart.ExponentDigitsOrSign:
				if (source[index] == '-' || source[index] == '+')
				{
					numberPart = NumberPart.ExponentDigitsOnly;
					index++;
				}
				else if (char.IsDigit(source[index]))
				{
					numberPart = NumberPart.ExponentDigitsOnly;
					index++;
				}
				else
				{
					flag = true;
				}
				break;
			case NumberPart.ExponentDigitsOnly:
				if (char.IsDigit(source[index]))
				{
					numberPart = NumberPart.ExponentDigitsOnly;
					index++;
				}
				else
				{
					flag = true;
				}
				break;
			}
		}
		if (index - num == 0 && (source[index] == ':' || source[index] == ',' || source[index] == ']'))
		{
			number = 0.0;
			errorMessage = null;
			return true;
		}
		if (!double.TryParse(source.Substring(num, index - num), out var result))
		{
			number = 0.0;
			errorMessage = "Expected number.";
			return false;
		}
		number = result;
		errorMessage = null;
		return true;
	}
}
