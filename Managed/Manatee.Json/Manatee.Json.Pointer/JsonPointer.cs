using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Manatee.Json.Internal;
using Manatee.Json.Serialization;

namespace Manatee.Json.Pointer;

[DebuggerDisplay("{ToString()}")]
public class JsonPointer : List<string>, IJsonSerializable, IEquatable<JsonPointer>
{
	private static readonly Regex _generalEscapePattern = new Regex("%(?<Value>[0-9A-F]{2})", RegexOptions.IgnoreCase);

	private bool _usesHash;

	public JsonPointer()
	{
	}

	public JsonPointer(params string[] source)
		: this((IEnumerable<string>)source)
	{
	}

	public JsonPointer(IEnumerable<string> source)
		: base(source.SkipWhile((string s) => s == "#"))
	{
		_usesHash = source.FirstOrDefault() == "#";
	}

	public static JsonPointer Parse(string source)
	{
		JsonPointer jsonPointer = new JsonPointer();
		string[] array = source.Split(new char[1] { '/' });
		if (array.Length == 0)
		{
			return jsonPointer;
		}
		if (!(array[0] == "#"))
		{
			array = ((!string.IsNullOrEmpty(array[0])) ? array.ToArray() : array.Skip(1).ToArray());
		}
		else
		{
			jsonPointer._usesHash = true;
		}
		jsonPointer.AddRange(array.SkipWhile((string s) => s == "#").Select(_Unescape));
		return jsonPointer;
	}

	public PointerEvaluationResults Evaluate(JsonValue root)
	{
		JsonPointer jsonPointer = new JsonPointer();
		JsonValue jsonValue = root;
		using (List<string>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				jsonPointer.Add(current);
				jsonValue = _EvaluateSegment(jsonValue, current);
				if (jsonValue == null)
				{
					return new PointerEvaluationResults($"No value found at '{jsonPointer}'");
				}
			}
		}
		return new PointerEvaluationResults(jsonValue);
	}

	public override string ToString()
	{
		string text = (_usesHash ? ("#/" + string.Join("/", this.Where((string s) => s != null).Select(_Escape))) : ("/" + string.Join("/", this.Where((string s) => s != null).Select(_Escape))));
		if (!(text == "/"))
		{
			return text.TrimEnd(new char[1] { '/' });
		}
		return text;
	}

	public JsonPointer Clone()
	{
		return new JsonPointer(this)
		{
			_usesHash = _usesHash
		};
	}

	public JsonPointer CloneAndAppend(params string[] append)
	{
		JsonPointer jsonPointer = new JsonPointer(this);
		jsonPointer._usesHash = _usesHash;
		jsonPointer.AddRange(append);
		return jsonPointer;
	}

	public bool IsChildOf(JsonPointer pointer)
	{
		return this.Take(pointer.Count).SequenceEqual(pointer);
	}

	internal JsonPointer CleanAndClone()
	{
		return new JsonPointer(this.WhereNotNull())
		{
			_usesHash = _usesHash
		};
	}

	internal JsonPointer WithHash()
	{
		return new JsonPointer(this)
		{
			_usesHash = true
		};
	}

	private static JsonValue? _EvaluateSegment(JsonValue current, string segment)
	{
		if (current.Type == JsonValueType.Array)
		{
			if (int.TryParse(segment, out var result))
			{
				if ((!(segment != "0") || !segment.StartsWith("0")) && 0 <= result && result < current.Array.Count)
				{
					return current.Array[result];
				}
				return null;
			}
			if (segment == "-")
			{
				return current.Array[current.Array.Count - 1];
			}
		}
		if (current.Type == JsonValueType.Object && current.Object.TryGetValue(segment, out JsonValue value))
		{
			return value;
		}
		return null;
	}

	private static string _Escape(string reference)
	{
		return reference.Replace("~", "~0").Replace("/", "~1");
	}

	private static string _Unescape(string reference)
	{
		string text = reference.Replace("~1", "/").Replace("~0", "~");
		foreach (Match item in _generalEscapePattern.Matches(text))
		{
			char c = (char)int.Parse(item.Groups["Value"].Value, NumberStyles.HexNumber);
			text = Regex.Replace(text, item.Value, new string(c, 1));
		}
		return text;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		AddRange(json.String.Split(new char[1] { '/' }).Skip(1).SkipWhile((string s) => s == "#")
			.Select(_Unescape));
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return ToString();
	}

	public bool Equals(JsonPointer? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (_usesHash == other._usesHash)
		{
			return this.SequenceEqual(other);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as JsonPointer);
	}

	public override int GetHashCode()
	{
		return (_usesHash.GetHashCode() * 397) ^ this.GetCollectionHashCode();
	}
}
