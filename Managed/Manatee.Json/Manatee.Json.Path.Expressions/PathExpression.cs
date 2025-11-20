using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Manatee.Json.Path.Expressions;

internal class PathExpression<T> : ExpressionTreeNode<T>, IEquatable<PathExpression<T>>
{
	public JsonPath Path { get; }

	public bool IsLocal { get; }

	public PathExpression(JsonPath path, bool isLocal)
	{
		Path = path;
		IsLocal = isLocal;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		JsonValue jsonValue = (IsLocal ? (json as JsonValue) : root);
		if (jsonValue == null)
		{
			throw new NotSupportedException("Path requires a JsonValue to evaluate.");
		}
		JsonArray jsonArray = Path.Evaluate(jsonValue);
		if (jsonArray.Count > 1)
		{
			throw new InvalidOperationException($"Path '{Path}' returned more than one result on value '{jsonValue}'");
		}
		return jsonArray.FirstOrDefault();
	}

	public override string? ToString()
	{
		return (IsLocal ? "@" : "$") + Path.GetRawString();
	}

	public bool Equals(PathExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Path, other.Path))
		{
			return IsLocal == other.IsLocal;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PathExpression<T>);
	}

	public override int GetHashCode()
	{
		return ((Path?.GetHashCode() ?? 0) * 397) ^ IsLocal.GetHashCode();
	}
}
