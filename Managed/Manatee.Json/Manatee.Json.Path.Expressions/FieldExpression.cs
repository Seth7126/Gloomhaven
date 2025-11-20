using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Manatee.Json.Path.Expressions;

internal class FieldExpression<T> : ExpressionTreeNode<T>, IEquatable<FieldExpression<T>>
{
	public FieldInfo Field { get; }

	public object Source { get; }

	public FieldExpression(FieldInfo field, object source)
	{
		Field = field;
		Source = source;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		if (Field.FieldType == typeof(string) || Field.FieldType == typeof(JsonArray) || Field.FieldType == typeof(JsonObject) || Field.FieldType == typeof(JsonValue))
		{
			return Field.GetValue(Source);
		}
		return Convert.ToDouble(Field.GetValue(Source));
	}

	public override string? ToString()
	{
		object obj = Evaluate(default(T), null);
		object obj2;
		if (!(obj is string))
		{
			obj2 = obj?.ToString();
			if (obj2 == null)
			{
				return "null";
			}
		}
		else
		{
			obj2 = $"\"{obj}\"";
		}
		return (string?)obj2;
	}

	public bool Equals(FieldExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Field, other.Field))
		{
			return object.Equals(Source, other.Source);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as FieldExpression<T>);
	}

	public override int GetHashCode()
	{
		return (((Field != null) ? Field.GetHashCode() : 0) * 397) ^ (Source?.GetHashCode() ?? 0);
	}
}
