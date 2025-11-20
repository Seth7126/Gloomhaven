using System.Collections.Generic;
using System.Linq;

namespace UdpKit;

public class UdpSessionFilter
{
	public UdpSessionFillMode FillMode { get; set; } = UdpSessionFillMode.Fill;

	private Dictionary<object, object> FilterProperties { get; }

	public object this[string key]
	{
		get
		{
			FilterProperties.TryGetValue(key, out var value);
			return value;
		}
		set
		{
			Add(key, value);
		}
	}

	public UdpSessionFilter()
	{
		FilterProperties = new Dictionary<object, object>();
	}

	public bool Add(string key, object value)
	{
		if (!IsValid(value))
		{
			UdpLog.Warn("Invalid Filter value type {0}", value.GetType());
			return false;
		}
		FilterProperties[key] = value;
		return true;
	}

	public bool Remove(string key)
	{
		return FilterProperties.Remove(key);
	}

	public Dictionary<object, object>.Enumerator GetEnumerator()
	{
		return FilterProperties.GetEnumerator();
	}

	public override string ToString()
	{
		return ToDebugString(FilterProperties);
	}

	public static bool IsValid(object value)
	{
		return value is byte || value is bool || value is short || value is int || value is long || value is float || value is double || value is string || value is byte[] || value is bool[] || value is short[] || value is int[] || value is long[] || value is float[] || value is double[] || value is string[];
	}

	private string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
	{
		return "{" + string.Join(",", dictionary.Select((KeyValuePair<TKey, TValue> kv) => kv.Key?.ToString() + "=" + kv.Value).ToArray()) + "}";
	}
}
