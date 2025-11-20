using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manatee.Json.Internal;

namespace Manatee.Json;

public class JsonArray : List<JsonValue>
{
	public ArrayEquality EqualityStandard { get; set; } = JsonOptions.DefaultArrayEquality;

	public JsonArray()
	{
	}

	public JsonArray(IEnumerable<JsonValue> collection)
		: base(collection.Select((JsonValue jv) => jv ?? JsonValue.Null))
	{
	}

	public string GetIndentedString(int indentLevel = 0)
	{
		if (base.Count == 0)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder();
		AppendIndentedString(stringBuilder, indentLevel);
		return stringBuilder.ToString();
	}

	internal void AppendIndentedString(StringBuilder builder, int indentLevel)
	{
		if (base.Count == 0)
		{
			builder.Append("[]");
			return;
		}
		string text = JsonOptions.PrettyPrintIndent.Repeat(indentLevel);
		string value = text + JsonOptions.PrettyPrintIndent;
		builder.Append("[\n");
		bool flag = false;
		using (List<JsonValue>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				JsonValue current = enumerator.Current;
				if (flag)
				{
					builder.Append(",\n");
				}
				builder.Append(value);
				current.AppendIndentedString(builder, indentLevel + 1);
				flag = true;
			}
		}
		builder.Append('\n');
		builder.Append(text);
		builder.Append(']');
	}

	public new void Add(JsonValue item)
	{
		base.Add(item ?? JsonValue.Null);
	}

	public new void AddRange(IEnumerable<JsonValue> collection)
	{
		base.AddRange(collection.Select((JsonValue v) => v ?? JsonValue.Null));
	}

	public override string ToString()
	{
		if (base.Count == 0)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder();
		AppendString(stringBuilder);
		return stringBuilder.ToString();
	}

	internal void AppendString(StringBuilder builder)
	{
		if (base.Count == 0)
		{
			builder.Append("[]");
			return;
		}
		builder.Append('[');
		bool flag = false;
		using (List<JsonValue>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				JsonValue current = enumerator.Current;
				if (flag)
				{
					builder.Append(',');
				}
				current.AppendString(builder);
				flag = true;
			}
		}
		builder.Append(']');
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is JsonArray jsonArray))
		{
			return false;
		}
		if (EqualityStandard != ArrayEquality.SequenceEqual)
		{
			return this.ContentsEqual(jsonArray);
		}
		return this.SequenceEqual(jsonArray);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}
}
