using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SM;

public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	private TKey[] _keys;

	[SerializeField]
	private TValueStorage[] _values;

	protected SerializableDictionaryBase()
	{
	}

	protected SerializableDictionaryBase(IDictionary<TKey, TValue> dict)
		: base(dict.Count)
	{
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			base[item.Key] = item.Value;
		}
	}

	protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);

	protected abstract TValue GetValue(TValueStorage[] storage, int i);

	public void CopyFrom(IDictionary<TKey, TValue> dict)
	{
		Clear();
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			base[item.Key] = item.Value;
		}
	}

	public void OnAfterDeserialize()
	{
		if (_keys != null && _values != null && _keys.Length == _values.Length)
		{
			Clear();
			int num = _keys.Length;
			for (int i = 0; i < num; i++)
			{
				base[_keys[i]] = GetValue(_values, i);
			}
			_keys = null;
			_values = null;
		}
	}

	public void OnBeforeSerialize()
	{
		int count = base.Count;
		_keys = new TKey[count];
		_values = new TValueStorage[count];
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			_keys[num] = current.Key;
			SetValue(_values, num, current.Value);
			num++;
		}
	}
}
