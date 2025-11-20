using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Parsing;

internal static class JsonPathParser
{
	private static readonly List<IJsonPathParser> _parsers;

	static JsonPathParser()
	{
		_parsers = (from ti in typeof(JsonPathParser).GetTypeInfo().Assembly.DefinedTypes
			where typeof(IJsonPathParser).GetTypeInfo().IsAssignableFrom(ti) && ti.IsClass
			select Activator.CreateInstance(ti.AsType())).Cast<IJsonPathParser>().ToList();
	}

	public static JsonPath Parse(string source)
	{
		int index = 0;
		if (!TryParse(source.Trim(), ref index, out JsonPath path, out string errorMessage))
		{
			throw new JsonPathSyntaxException(path, errorMessage);
		}
		return path;
	}

	public static bool TryParse(string source, ref int index, [NotNullWhen(true)] out JsonPath? path, [NotNullWhen(false)] out string? errorMessage)
	{
		int length = source.Length;
		path = null;
		while (index < length)
		{
			errorMessage = source.SkipWhiteSpace(ref index, length, out var _);
			if (errorMessage != null)
			{
				return false;
			}
			int i = index;
			IJsonPathParser jsonPathParser = _parsers.FirstOrDefault((IJsonPathParser p) => p.Handles(source, i));
			if (jsonPathParser == null)
			{
				errorMessage = "Unrecognized JSON Path element.";
				return false;
			}
			if (!jsonPathParser.TryParse(source, ref index, ref path, out errorMessage))
			{
				return false;
			}
		}
		errorMessage = null;
		return true;
	}
}
