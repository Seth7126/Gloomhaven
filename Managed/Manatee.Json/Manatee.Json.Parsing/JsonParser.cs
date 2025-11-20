using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Json.Internal;

namespace Manatee.Json.Parsing;

internal static class JsonParser
{
	private static readonly List<IJsonParser> _parsers;

	static JsonParser()
	{
		_parsers = (from ti in typeof(JsonParser).GetTypeInfo().Assembly.DefinedTypes
			where typeof(IJsonParser).GetTypeInfo().IsAssignableFrom(ti) && ti.IsClass
			select Activator.CreateInstance(ti.AsType())).Cast<IJsonParser>().ToList();
	}

	public static JsonValue Parse(string source)
	{
		int index = 0;
		if (!TryParse(source, ref index, out JsonValue value, out string errorMessage))
		{
			throw new JsonSyntaxException(source, errorMessage, value);
		}
		return value;
	}

	public static JsonValue Parse(TextReader stream)
	{
		if (!TryParse(stream, out JsonValue value, out string errorMessage))
		{
			throw new JsonSyntaxException(errorMessage, value);
		}
		return value;
	}

	public static async Task<JsonValue> ParseAsync(TextReader stream, CancellationToken token = default(CancellationToken))
	{
		var (text, jsonValue) = await TryParseAsync(stream, token);
		if (text != null)
		{
			throw new JsonSyntaxException(text, jsonValue);
		}
		return jsonValue;
	}

	public static bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage, bool allowExtraChars = false)
	{
		int length = source.Length;
		errorMessage = source.SkipWhiteSpace(ref index, length, out var ch);
		if (errorMessage != null)
		{
			value = null;
			return false;
		}
		foreach (IJsonParser parser in _parsers)
		{
			if (parser.Handles(ch))
			{
				return parser.TryParse(source, ref index, out value, out errorMessage, allowExtraChars);
			}
		}
		value = null;
		errorMessage = "Cannot determine type.";
		return false;
	}

	public static bool TryParse(TextReader stream, [NotNullWhen(true)] out JsonValue? value, [NotNullWhen(false)] out string? errorMessage)
	{
		errorMessage = stream.SkipWhiteSpace(out var ch);
		if (errorMessage != null)
		{
			value = null;
			return false;
		}
		foreach (IJsonParser parser in _parsers)
		{
			if (parser.Handles(ch))
			{
				return parser.TryParse(stream, out value, out errorMessage);
			}
		}
		value = null;
		errorMessage = "Cannot determine type.";
		return false;
	}

	public static async Task<(string? errorMessage, JsonValue? value)> TryParseAsync(TextReader stream, CancellationToken token)
	{
		char ch;
		string text = stream.SkipWhiteSpace(out ch);
		if (text != null)
		{
			return (errorMessage: text, value: null);
		}
		foreach (IJsonParser parser in _parsers)
		{
			if (parser.Handles(ch))
			{
				return await parser.TryParseAsync(stream, token);
			}
		}
		return (errorMessage: "Cannot determine type.", value: null);
	}
}
