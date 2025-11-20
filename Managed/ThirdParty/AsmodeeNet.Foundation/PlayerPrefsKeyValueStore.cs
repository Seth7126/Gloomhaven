using UnityEngine;

namespace AsmodeeNet.Foundation;

public class PlayerPrefsKeyValueStore : KeyValueStore
{
	protected override void _DeleteAll()
	{
		PlayerPrefs.DeleteAll();
	}

	protected override void _DeleteKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	protected override bool _HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	protected override void _Save()
	{
		PlayerPrefs.Save();
	}

	protected override int _GetInt(string key, int defaultValue)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	protected override float _GetFloat(string key, float defaultValue)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	protected override string _GetString(string key, string defaultValue)
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	protected override void _SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	protected override void _SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	protected override void _SetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}
}
