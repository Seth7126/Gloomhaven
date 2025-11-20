using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions;

internal class ArrayIndexExpression<T> : PathExpression<T>, IEquatable<ArrayIndexExpression<T>>
{
	public int Index { get; }

	public ExpressionTreeNode<T> IndexExpression { get; }

	public ArrayIndexExpression(JsonPath path, bool isLocal, int index)
		: base(path, isLocal)
	{
		Index = index;
		IndexExpression = null;
	}

	public ArrayIndexExpression(JsonPath path, bool isLocal, ExpressionTreeNode<T> indexExpression)
		: base(path, isLocal)
	{
		IndexExpression = indexExpression;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		JsonValue jsonValue = ((!base.IsLocal) ? root : ((json != null) ? json.AsJsonValue() : null));
		if (jsonValue == null)
		{
			throw new NotSupportedException("ArrayIndex requires a JsonValue to evaluate.");
		}
		JsonArray jsonArray = base.Path.Evaluate(jsonValue);
		if (jsonArray.Count > 1)
		{
			throw new InvalidOperationException($"Path '{base.Path}' returned more than one result on value '{jsonValue}'");
		}
		JsonValue jsonValue2 = jsonArray.FirstOrDefault();
		int num = _GetIndex();
		if (jsonValue2 != null && num >= 0)
		{
			if (jsonValue2.Type == JsonValueType.Array && num < jsonValue2.Array.Count)
			{
				return jsonValue2.Array[num];
			}
			if (jsonValue2.Type == JsonValueType.Object && num < jsonValue2.Object.Count)
			{
				return jsonValue2.Object.ElementAt(num).Value;
			}
		}
		return null;
	}

	public override string? ToString()
	{
		string arg = ((base.Path == null) ? string.Empty : base.Path.GetRawString());
		return string.Format(base.IsLocal ? "@{0}[{1}]" : "${0}[{1}]", arg, _GetIndex());
	}

	public bool Equals(ArrayIndexExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Equals((PathExpression<T>?)other) && Index == other.Index)
		{
			return object.Equals(IndexExpression, other.IndexExpression);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ArrayIndexExpression<T>);
	}

	public override int GetHashCode()
	{
		return (((base.GetHashCode() * 397) ^ Index) * 397) ^ (IndexExpression?.GetHashCode() ?? 0);
	}

	private int _GetIndex()
	{
		object obj = IndexExpression?.Evaluate(default(T), null);
		if (obj != null)
		{
			return (int)obj;
		}
		return Index;
	}
}
