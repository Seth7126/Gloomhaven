using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.Expressions;

internal class NameExpression<T> : PathExpression<T>, IEquatable<NameExpression<T>>
{
	public string Name { get; }

	public ExpressionTreeNode<T> NameExp { get; }

	public NameExpression(JsonPath path, bool isLocal, string name)
		: base(path, isLocal)
	{
		Name = name;
		NameExp = null;
	}

	public NameExpression(JsonPath path, bool isLocal, ExpressionTreeNode<T> nameExp)
		: base(path, isLocal)
	{
		Name = null;
		NameExp = nameExp;
	}

	public override object? Evaluate([MaybeNull] T json, JsonValue? root)
	{
		JsonValue jsonValue = ((!base.IsLocal) ? root : ((json != null) ? json.AsJsonValue() : null));
		if (jsonValue == null)
		{
			throw new NotSupportedException("Name requires a JsonValue to evaluate.");
		}
		JsonArray jsonArray = base.Path.Evaluate(jsonValue);
		if (jsonArray.Count > 1)
		{
			throw new InvalidOperationException($"Path '{base.Path}' returned more than one result on value '{jsonValue}'");
		}
		JsonValue jsonValue2 = jsonArray.FirstOrDefault();
		string key = _GetName();
		if (!(jsonValue2 != null) || jsonValue2.Type != JsonValueType.Object || !jsonValue2.Object.ContainsKey(key))
		{
			return null;
		}
		return jsonValue2.Object[key].GetValue();
	}

	public override string? ToString()
	{
		string arg = ((base.Path == null) ? string.Empty : base.Path.GetRawString());
		return string.Format(base.IsLocal ? "@{0}.{1}" : "${0}.{1}", arg, _GetName());
	}

	private string _GetName()
	{
		object obj = NameExp?.Evaluate(default(T), null);
		if (obj != null)
		{
			return (string)obj;
		}
		return Name;
	}

	public bool Equals(NameExpression<T>? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Equals((PathExpression<T>?)other) && string.Equals(Name, other.Name))
		{
			return object.Equals(NameExp, other.NameExp);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as NameExpression<T>);
	}

	public override int GetHashCode()
	{
		return (((base.GetHashCode() * 397) ^ (Name?.GetHashCode() ?? 0)) * 397) ^ (NameExp?.GetHashCode() ?? 0);
	}
}
