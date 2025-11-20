using System;
using System.Collections.Generic;
using System.Linq;

namespace XUnity.Common.Utilities;

public static class ExtensionDataHelper
{
	private static readonly object Sync;

	private static readonly WeakDictionary<object, object> WeakDynamicFields;

	public static int WeakReferenceCount
	{
		get
		{
			lock (Sync)
			{
				return WeakDynamicFields.Count;
			}
		}
	}

	static ExtensionDataHelper()
	{
		Sync = new object();
		WeakDynamicFields = new WeakDictionary<object, object>();
		MaintenanceHelper.AddMaintenanceFunction(Cull, 12);
	}

	public static void SetExtensionData<T>(this object obj, T t)
	{
		lock (Sync)
		{
			if (WeakDynamicFields.TryGetValue(obj, out var value))
			{
				if (value is Dictionary<Type, object> dictionary)
				{
					dictionary[typeof(T)] = t;
					return;
				}
				Dictionary<Type, object> dictionary2 = new Dictionary<Type, object>();
				dictionary2.Add(value.GetType(), value);
				dictionary2[typeof(T)] = t;
				WeakDynamicFields[obj] = dictionary2;
			}
			else
			{
				WeakDynamicFields[obj] = t;
			}
		}
	}

	public static T GetOrCreateExtensionData<T>(this object obj) where T : new()
	{
		if (obj == null)
		{
			return default(T);
		}
		lock (Sync)
		{
			if (WeakDynamicFields.TryGetValue(obj, out var value))
			{
				if (value is Dictionary<Type, object> dictionary)
				{
					if (dictionary.TryGetValue(typeof(T), out value))
					{
						return (T)value;
					}
					T val = new T();
					dictionary[typeof(T)] = val;
					return val;
				}
				if (!(value is T result))
				{
					Dictionary<Type, object> dictionary2 = new Dictionary<Type, object>();
					dictionary2.Add(value.GetType(), value);
					T val2 = new T();
					dictionary2[typeof(T)] = val2;
					WeakDynamicFields[obj] = dictionary2;
					return val2;
				}
				return result;
			}
			T val3 = new T();
			WeakDynamicFields[obj] = val3;
			return val3;
		}
	}

	public static T GetExtensionData<T>(this object obj)
	{
		if (obj == null)
		{
			return default(T);
		}
		lock (Sync)
		{
			if (WeakDynamicFields.TryGetValue(obj, out var value))
			{
				if (value is Dictionary<Type, object> dictionary && dictionary.TryGetValue(typeof(T), out value))
				{
					if (!(value is T result))
					{
						return default(T);
					}
					return result;
				}
				if (!(value is T result2))
				{
					return default(T);
				}
				return result2;
			}
		}
		return default(T);
	}

	public static void Cull()
	{
		lock (Sync)
		{
			WeakDynamicFields.RemoveCollectedEntries();
		}
	}

	public static List<KeyValuePair<object, object>> GetAllRegisteredObjects()
	{
		lock (Sync)
		{
			return IterateAllPairs().ToList();
		}
	}

	public static void Remove(object obj)
	{
		lock (Sync)
		{
			WeakDynamicFields.Remove(obj);
		}
	}

	private static IEnumerable<KeyValuePair<object, object>> IterateAllPairs()
	{
		foreach (KeyValuePair<object, object> kvp in WeakDynamicFields)
		{
			if (kvp.Value is Dictionary<Type, object> dictionary)
			{
				foreach (KeyValuePair<Type, object> item in dictionary)
				{
					yield return new KeyValuePair<object, object>(kvp.Key, item.Value);
				}
			}
			else
			{
				yield return kvp;
			}
		}
	}
}
