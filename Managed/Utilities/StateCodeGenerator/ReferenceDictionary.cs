using System;
using System.Collections.Generic;

namespace StateCodeGenerator;

public class ReferenceDictionary
{
	private readonly Dictionary<object, object> _references = new Dictionary<object, object>();

	public T Get<T>(T original, Func<T> createObject)
	{
		if (original == null)
		{
			return default(T);
		}
		if (_references.TryGetValue(original, out var value))
		{
			return (T)value;
		}
		T val = createObject();
		_references.Add(original, val);
		return val;
	}

	public T Get<T>(T original)
	{
		if (original == null)
		{
			return default(T);
		}
		_references.TryGetValue(original, out var value);
		return (T)value;
	}

	public void Add(object original, object copy)
	{
		_references.TryAdd(original, copy);
	}
}
