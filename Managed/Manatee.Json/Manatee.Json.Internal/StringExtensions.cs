using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Json.Internal;

internal static class StringExtensions
{
	private static readonly int[] _availableChars = Enumerable.Range(0, 65535).Select(delegate(int n)
	{
		char c = (char)n;
		return (!char.IsControl(c) && c != '\\' && c != '"') ? n : 0;
	}).ToArray();

	private static readonly Regex _generalEscapePattern = new Regex("%(?<Value>[0-9A-F]{2})", RegexOptions.IgnoreCase);

	public static int CalculateUtf32(int hex, int hex2)
	{
		return (hex - 55296) * 1024 + (hex2 - 56320) % 1024 + 65536;
	}

	public static bool IsValidUtf32CodePoint(this int hex)
	{
		if (0 <= hex && hex <= 1114111)
		{
			if (55296 <= hex)
			{
				return hex > 57343;
			}
			return true;
		}
		return false;
	}

	public static string InsertEscapeSequences(this string source)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (_availableChars[(uint)source[i]] == 0)
			{
				StringBuilder stringBuilder = StringBuilderCache.Acquire();
				stringBuilder.Append(source, 0, i);
				_InsertEscapeSequencesSlow(source, stringBuilder, i);
				return StringBuilderCache.GetStringAndRelease(stringBuilder);
			}
		}
		return source;
	}

	public static void InsertEscapeSequences(this string source, StringBuilder builder)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (_availableChars[(uint)source[i]] == 0)
			{
				builder.Append(source, 0, i);
				_InsertEscapeSequencesSlow(source, builder, i);
				return;
			}
		}
		builder.Append(source);
	}

	private static void _InsertEscapeSequencesSlow(string source, StringBuilder builder, int index)
	{
		for (int i = index; i < source.Length; i++)
		{
			switch (source[i])
			{
			case '"':
				builder.Append("\\\"");
				continue;
			case '\\':
				builder.Append("\\\\");
				continue;
			case '\b':
				builder.Append("\\b");
				continue;
			case '\f':
				builder.Append("\\f");
				continue;
			case '\n':
				builder.Append("\\n");
				continue;
			case '\r':
				builder.Append("\\r");
				continue;
			case '\t':
				builder.Append("\\t");
				continue;
			}
			if (_availableChars[(uint)source[i]] != 0)
			{
				builder.Append(source[i]);
				continue;
			}
			builder.Append("\\u");
			builder.Append(((int)source[i]).ToString("X4"));
		}
	}

	public static string? SkipWhiteSpace(this string source, ref int index, int length, out char ch)
	{
		ch = '\0';
		while (index < length)
		{
			ch = source[index];
			if (!char.IsWhiteSpace(ch))
			{
				break;
			}
			index++;
		}
		if (index >= length)
		{
			ch = '\0';
			return "Unexpected end of input.";
		}
		return null;
	}

	public static string? SkipWhiteSpace(this TextReader stream, out char ch)
	{
		ch = '\0';
		int num;
		for (num = stream.Peek(); num != -1; num = stream.Peek())
		{
			ch = (char)num;
			if (!char.IsWhiteSpace(ch))
			{
				break;
			}
			stream.Read();
		}
		if (num == -1)
		{
			ch = '\0';
			return "Unexpected end of input.";
		}
		return null;
	}

	public static async Task<(string?, char)> SkipWhiteSpaceAsync(this TextReader stream, char[] scratch)
	{
		char ch = '\0';
		int num;
		for (num = stream.Peek(); num != -1; num = stream.Peek())
		{
			ch = (char)num;
			if (!char.IsWhiteSpace(ch))
			{
				break;
			}
			await stream.ReadAsync(scratch, 0, 1);
		}
		if (num == -1)
		{
			ch = '\0';
			return ("Unexpected end of input.", ch);
		}
		return (null, ch);
	}

	public static string UnescapePointer(this string reference)
	{
		string text = reference.Replace("~1", "/").Replace("~0", "~");
		foreach (Match item in _generalEscapePattern.Matches(text))
		{
			char c = (char)int.Parse(item.Groups["Value"].Value, NumberStyles.HexNumber);
			text = Regex.Replace(text, item.Value, new string(c, 1));
		}
		return text;
	}

	public static Task<bool> TryRead(this TextReader stream, char[] buffer, int offset, int count)
	{
		return stream.TryRead(buffer, offset, count, CancellationToken.None);
	}

	public static async Task<bool> TryRead(this TextReader stream, char[] buffer, int offset, int count, CancellationToken token)
	{
		if (token.IsCancellationRequested)
		{
			return false;
		}
		return count == await stream.ReadBlockAsync(buffer, offset, count);
	}

	public static string Repeat(this string s, int n)
	{
		return new StringBuilder(s.Length * n).Insert(0, s, n).ToString();
	}

	public static bool IsLocalSchemaId(this string s)
	{
		if (s != "#" && !s.Contains("#/"))
		{
			return s.Contains("#");
		}
		return false;
	}

	public static string ToStringList<T>(this IEnumerable<T> items, string separator = ", ")
	{
		return string.Join(separator, items);
	}
}
