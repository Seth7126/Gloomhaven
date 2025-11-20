using System;
using System.Collections.Generic;

namespace XUnity.Common.Utilities;

public sealed class WeakDictionary<TKey, TValue> : BaseDictionary<TKey, TValue> where TKey : class
{
	private Dictionary<object, TValue> dictionary;

	private WeakKeyComparer<TKey> comparer;

	public override int Count => dictionary.Count;

	public WeakDictionary()
		: this(0, (IEqualityComparer<TKey>)null)
	{
	}

	public WeakDictionary(int capacity)
		: this(capacity, (IEqualityComparer<TKey>)null)
	{
	}

	public WeakDictionary(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	public WeakDictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		this.comparer = new WeakKeyComparer<TKey>(comparer);
		dictionary = new Dictionary<object, TValue>(capacity, this.comparer);
	}

	public override void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		WeakReference<TKey> key2 = new WeakKeyReference<TKey>(key, comparer);
		dictionary.Add(key2, value);
	}

	public override bool ContainsKey(TKey key)
	{
		return dictionary.ContainsKey(key);
	}

	public override bool Remove(TKey key)
	{
		return dictionary.Remove(key);
	}

	public override bool TryGetValue(TKey key, out TValue value)
	{
		if (dictionary.TryGetValue(key, out value))
		{
			return true;
		}
		value = default(TValue);
		return false;
	}

	protected override void SetValue(TKey key, TValue value)
	{
		WeakReference<TKey> key2 = new WeakKeyReference<TKey>(key, comparer);
		dictionary[key2] = value;
	}

	public override void Clear()
	{
		dictionary.Clear();
	}

	public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		foreach (KeyValuePair<object, TValue> item in dictionary)
		{
			WeakReference<TKey> obj = (WeakReference<TKey>)item.Key;
			TValue value = item.Value;
			TKey target = obj.Target;
			if (obj.IsAlive)
			{
				yield return new KeyValuePair<TKey, TValue>(target, value);
			}
		}
	}

	public void RemoveCollectedEntries()
	{
		List<object> list = null;
		foreach (KeyValuePair<object, TValue> item in dictionary)
		{
			WeakReference<TKey> weakReference = (WeakReference<TKey>)item.Key;
			if (!weakReference.IsAlive)
			{
				if (list == null)
				{
					list = new List<object>();
				}
				list.Add(weakReference);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (object item2 in list)
		{
			dictionary.Remove(item2);
		}
	}
}
