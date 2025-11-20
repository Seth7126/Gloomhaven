using System;
using System.Diagnostics.CodeAnalysis;

namespace Manatee.Json.Path.Expressions;

internal class HasPropertyExpression<T> : PathExpression<T>, IEquatable<HasPropertyExpression<T>>
{
	public string Name { get; }

	public HasPropertyExpression(JsonPath path, bool isLocal, string name)
		: base(path, isLocal)
	{
		Name = name;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		JsonValue jsonValue = json as JsonValue;
		if (jsonValue == null)
		{
			throw new NotSupportedException("HasProperty requires an array to evaluate.");
		}
		bool flag = jsonValue.Type == JsonValueType.Object && jsonValue.Object.ContainsKey(Name);
		if (flag && jsonValue.Object[Name].Type == JsonValueType.Boolean)
		{
			flag = jsonValue.Object[Name].Boolean;
		}
		return flag;
	}

	public override string? ToString()
	{
		return "@." + Name;
	}

	public bool Equals(HasPropertyExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Equals((PathExpression<T>?)other))
		{
			return string.Equals(Name, other.Name);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as HasPropertyExpression<T>);
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ (Name?.GetHashCode() ?? 0);
	}
}
