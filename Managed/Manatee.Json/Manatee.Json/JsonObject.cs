using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manatee.Json.Internal;

namespace Manatee.Json;

public class JsonObject : Dictionary<string, JsonValue>
{
	public new JsonValue this[string key]
	{
		get
		{
			return base[key];
		}
		set
		{
			base[key] = value ?? JsonValue.Null;
		}
	}

	public JsonObject()
	{
	}

	public JsonObject(IDictionary<string, JsonValue?> collection)
		: base((IDictionary<string, JsonValue>)collection.ToDictionary<KeyValuePair<string, JsonValue>, string, JsonValue>((KeyValuePair<string, JsonValue> kvp) => kvp.Key, (KeyValuePair<string, JsonValue> kvp) => kvp.Value ?? JsonValue.Null))
	{
	}

	public string GetIndentedString(int indentLevel = 0)
	{
		if (base.Count == 0)
		{
			return "{}";
		}
		StringBuilder stringBuilder = new StringBuilder();
		AppendIndentedString(stringBuilder, indentLevel);
		return stringBuilder.ToString();
	}

	internal void AppendIndentedString(StringBuilder builder, int indentLevel = 0)
	{
		if (base.Count == 0)
		{
			builder.Append("{}");
			return;
		}
		string text = JsonOptions.PrettyPrintIndent.Repeat(indentLevel);
		string value = text + JsonOptions.PrettyPrintIndent;
		builder.Append("{\n");
		bool flag = false;
		using (Dictionary<string, JsonValue>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JsonValue> current = enumerator.Current;
				if (flag)
				{
					builder.Append(",\n");
				}
				builder.Append(value);
				builder.Append('"');
				builder.Append(current.Key.InsertEscapeSequences());
				builder.Append("\" : ");
				current.Value.AppendIndentedString(builder, indentLevel + 2);
				flag = true;
			}
		}
		builder.Append('\n');
		builder.Append(text);
		builder.Append('}');
	}

	public new void Add(string key, JsonValue? value)
	{
		switch (JsonOptions.DuplicateKeyBehavior)
		{
		case DuplicateKeyBehavior.Overwrite:
			this[key] = value ?? JsonValue.Null;
			break;
		case DuplicateKeyBehavior.Throw:
			base.Add(key, value ?? JsonValue.Null);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToString()
	{
		if (base.Count == 0)
		{
			return "{}";
		}
		StringBuilder stringBuilder = new StringBuilder();
		AppendString(stringBuilder);
		return stringBuilder.ToString();
	}

	internal void AppendString(StringBuilder builder)
	{
		if (base.Count == 0)
		{
			builder.Append("{}");
			return;
		}
		builder.Append('{');
		bool flag = false;
		using (Dictionary<string, JsonValue>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JsonValue> current = enumerator.Current;
				if (flag)
				{
					builder.Append(',');
				}
				builder.Append('"');
				builder.Append(current.Key.InsertEscapeSequences());
				builder.Append("\":");
				current.Value.AppendString(builder);
				flag = true;
			}
		}
		builder.Append('}');
	}

	public override bool Equals(object? obj)
	{
		JsonObject json = obj as JsonObject;
		if (json == null)
		{
			return false;
		}
		if (!base.Keys.ContentsEqual(json.Keys))
		{
			return false;
		}
		return this.All<KeyValuePair<string, JsonValue>>((KeyValuePair<string, JsonValue> pair) => json[pair.Key].Equals(pair.Value));
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
