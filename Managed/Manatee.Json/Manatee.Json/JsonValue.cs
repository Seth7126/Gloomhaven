using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Manatee.Json.Internal;
using Manatee.Json.Parsing;

namespace Manatee.Json;

public class JsonValue : IEquatable<JsonValue>
{
	private readonly bool _boolValue;

	private readonly string _stringValue;

	private readonly double _numberValue;

	private readonly JsonObject _objectValue;

	private readonly JsonArray _arrayValue;

	public static readonly JsonValue Null = new JsonValue();

	public bool Boolean
	{
		get
		{
			if (Type != JsonValueType.Boolean && JsonOptions.ThrowOnIncorrectTypeAccess)
			{
				throw new JsonValueIncorrectTypeException(Type, JsonValueType.Boolean);
			}
			return _boolValue;
		}
	}

	public string String
	{
		get
		{
			if (Type != JsonValueType.String && JsonOptions.ThrowOnIncorrectTypeAccess)
			{
				throw new JsonValueIncorrectTypeException(Type, JsonValueType.String);
			}
			return _stringValue;
		}
	}

	public double Number
	{
		get
		{
			if (Type != JsonValueType.Number && JsonOptions.ThrowOnIncorrectTypeAccess)
			{
				throw new JsonValueIncorrectTypeException(Type, JsonValueType.Number);
			}
			return _numberValue;
		}
	}

	public JsonObject Object
	{
		get
		{
			if (Type != JsonValueType.Object && JsonOptions.ThrowOnIncorrectTypeAccess)
			{
				throw new JsonValueIncorrectTypeException(Type, JsonValueType.Object);
			}
			return _objectValue;
		}
	}

	public JsonArray Array
	{
		get
		{
			if (Type != JsonValueType.Array && JsonOptions.ThrowOnIncorrectTypeAccess)
			{
				throw new JsonValueIncorrectTypeException(Type, JsonValueType.Array);
			}
			return _arrayValue;
		}
	}

	public JsonValueType Type { get; }

	public JsonValue()
	{
		Type = JsonValueType.Null;
	}

	public JsonValue(bool b)
	{
		_boolValue = b;
		Type = JsonValueType.Boolean;
	}

	public JsonValue(string s)
	{
		_stringValue = s ?? throw new ArgumentNullException("s");
		Type = JsonValueType.String;
	}

	public JsonValue(double n)
	{
		_numberValue = n;
		Type = JsonValueType.Number;
	}

	public JsonValue(JsonObject o)
	{
		_objectValue = o ?? throw new ArgumentNullException("o");
		Type = JsonValueType.Object;
	}

	public JsonValue(JsonArray a)
	{
		_arrayValue = a ?? throw new ArgumentNullException("a");
		Type = JsonValueType.Array;
	}

	public JsonValue(JsonValue other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		_arrayValue = other._arrayValue;
		_objectValue = other._objectValue;
		_numberValue = other._numberValue;
		_stringValue = other._stringValue;
		_boolValue = other._boolValue;
		Type = other.Type;
	}

	public string GetIndentedString(int indentLevel = 0)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendIndentedString(stringBuilder, indentLevel);
		return stringBuilder.ToString();
	}

	internal void AppendIndentedString(StringBuilder builder, int indentLevel)
	{
		switch (Type)
		{
		case JsonValueType.Object:
			_objectValue.AppendIndentedString(builder, indentLevel);
			break;
		case JsonValueType.Array:
			_arrayValue.AppendIndentedString(builder, indentLevel);
			break;
		default:
			AppendString(builder);
			break;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendString(stringBuilder);
		return stringBuilder.ToString();
	}

	internal void AppendString(StringBuilder builder)
	{
		switch (Type)
		{
		case JsonValueType.Number:
			builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", _numberValue);
			break;
		case JsonValueType.String:
			builder.Append('"');
			_stringValue.InsertEscapeSequences(builder);
			builder.Append('"');
			break;
		case JsonValueType.Boolean:
			builder.Append(_boolValue ? "true" : "false");
			break;
		case JsonValueType.Object:
			_objectValue.AppendString(builder);
			break;
		case JsonValueType.Array:
			_arrayValue.AppendString(builder);
			break;
		default:
			builder.Append("null");
			break;
		}
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj?.AsJsonValue());
	}

	public bool Equals(JsonValue? other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (other.Type != Type)
		{
			return false;
		}
		switch (Type)
		{
		case JsonValueType.Number:
		{
			double numberValue = _numberValue;
			return numberValue.Equals(other.Number);
		}
		case JsonValueType.String:
			return _stringValue.Equals(other.String);
		case JsonValueType.Boolean:
		{
			bool boolValue = _boolValue;
			return boolValue.Equals(other.Boolean);
		}
		case JsonValueType.Object:
			return _objectValue.Equals(other.Object);
		case JsonValueType.Array:
			return _arrayValue.Equals(other.Array);
		case JsonValueType.Null:
			return true;
		default:
			return false;
		}
	}

	public override int GetHashCode()
	{
		switch (Type)
		{
		case JsonValueType.Number:
		{
			double numberValue = _numberValue;
			return numberValue.GetHashCode();
		}
		case JsonValueType.String:
			return _stringValue.GetHashCode();
		case JsonValueType.Boolean:
		{
			bool boolValue = _boolValue;
			return boolValue.GetHashCode();
		}
		case JsonValueType.Object:
			return _objectValue.GetHashCode();
		case JsonValueType.Array:
			return _arrayValue.GetHashCode();
		case JsonValueType.Null:
			return JsonValueType.Null.GetHashCode();
		default:
			return base.GetHashCode();
		}
	}

	public static JsonValue Parse(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (string.IsNullOrWhiteSpace(source))
		{
			throw new ArgumentException("Source string contains no data.");
		}
		return JsonParser.Parse(source);
	}

	public static JsonValue Parse(TextReader stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return JsonParser.Parse(stream);
	}

	public static Task<JsonValue> ParseAsync(TextReader stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return JsonParser.ParseAsync(stream);
	}

	public static implicit operator JsonValue(bool b)
	{
		return new JsonValue(b);
	}

	public static implicit operator JsonValue(string? s)
	{
		if (s != null)
		{
			return new JsonValue(s);
		}
		return null;
	}

	public static implicit operator JsonValue(double n)
	{
		return new JsonValue(n);
	}

	public static implicit operator JsonValue(JsonObject? o)
	{
		if (o != null)
		{
			return new JsonValue(o);
		}
		return null;
	}

	public static implicit operator JsonValue(JsonArray? a)
	{
		if (a != null)
		{
			return new JsonValue(a);
		}
		return null;
	}

	public static bool operator ==(JsonValue? a, JsonValue? b)
	{
		if ((object)a != b)
		{
			if (a != null)
			{
				return a.Equals(b);
			}
			return false;
		}
		return true;
	}

	public static bool operator !=(JsonValue? a, JsonValue? b)
	{
		return !object.Equals(a, b);
	}

	internal object GetValue()
	{
		return Type switch
		{
			JsonValueType.Number => _numberValue, 
			JsonValueType.String => _stringValue, 
			JsonValueType.Boolean => _boolValue, 
			JsonValueType.Object => _objectValue, 
			JsonValueType.Array => _arrayValue, 
			JsonValueType.Null => this, 
			_ => throw new ArgumentOutOfRangeException("Type"), 
		};
	}
}
