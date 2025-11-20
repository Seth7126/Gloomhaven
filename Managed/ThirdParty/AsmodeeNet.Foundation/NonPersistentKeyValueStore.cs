using System.Collections.Generic;

namespace AsmodeeNet.Foundation;

public class NonPersistentKeyValueStore : KeyValueStore
{
	private Dictionary<string, int> _ints = new Dictionary<string, int>();

	private Dictionary<string, float> _floats = new Dictionary<string, float>();

	private Dictionary<string, string> _strings = new Dictionary<string, string>();

	protected override void _DeleteAll()
	{
		_ints.Clear();
		_floats.Clear();
		_strings.Clear();
	}

	protected override void _DeleteKey(string key)
	{
		_ints.Remove(key);
		_floats.Remove(key);
		_strings.Remove(key);
	}

	protected override bool _HasKey(string key)
	{
		if (!_ints.ContainsKey(key) && !_floats.ContainsKey(key))
		{
			return _strings.ContainsKey(key);
		}
		return true;
	}

	protected override void _Save()
	{
	}

	protected override int _GetInt(string key, int defaultValue)
	{
		if (_ints.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	protected override float _GetFloat(string key, float defaultValue)
	{
		if (_floats.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	protected override string _GetString(string key, string defaultValue)
	{
		if (_strings.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	protected override void _SetInt(string key, int value)
	{
		_DeleteKey(key);
		_ints.Add(key, value);
	}

	protected override void _SetFloat(string key, float value)
	{
		_DeleteKey(key);
		_floats.Add(key, value);
	}

	protected override void _SetString(string key, string value)
	{
		_DeleteKey(key);
		_strings.Add(key, value);
	}
}
