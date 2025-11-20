using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;

namespace Manatee.Json.Serialization.Internal;

internal class SerializationReferenceCache
{
	private readonly IDictionary<object, SerializationReference> _objMap = new Dictionary<object, SerializationReference>();

	private readonly IDictionary<string, SerializationReference> _refMap = new Dictionary<string, SerializationReference>();

	public int Count => _objMap.Count;

	public void Add(SerializationReference value)
	{
		if (value.Object != null)
		{
			_objMap.Add(value.Object, value);
		}
		if (value.Source != null)
		{
			_refMap.Add(value.Source.ToString(), value);
		}
	}

	public void AddReference(JsonPointer source, JsonPointer target)
	{
		string key = source.ToString();
		if (!_refMap.TryGetValue(key, out SerializationReference value))
		{
			value = new SerializationReference(source);
			_refMap[key] = value;
		}
		value.Targets.Add(target);
	}

	public void Clear()
	{
		_objMap.Clear();
		_refMap.Clear();
	}

	public void Complete(object root)
	{
		foreach (SerializationReference item in _refMap.Values.Where((SerializationReference r) => r.Targets.Count > 0))
		{
			if (!item.DeserializationIsComplete)
			{
				continue;
			}
			foreach (JsonPointer target in item.Targets)
			{
				root.SetMember(target, item.Object);
			}
		}
		Clear();
	}

	public bool TryGetPair(object obj, [NotNullWhen(true)] out SerializationReference? pair)
	{
		return _objMap.TryGetValue(obj, out pair);
	}
}
