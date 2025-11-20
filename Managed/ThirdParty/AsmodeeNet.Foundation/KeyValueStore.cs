using AsmodeeNet.Utils;
using UnityEngine;

namespace AsmodeeNet.Foundation;

public abstract class KeyValueStore
{
	private const string _kModuleName = "KeyValueStore";

	public static KeyValueStore Instance { get; set; }

	public static void ResetInstance()
	{
		Instance = null;
	}

	public static void DeleteAll()
	{
		_CheckInstance();
		Instance?._DeleteAll();
	}

	public static void DeleteKey(string key)
	{
		_CheckInstance();
		Instance?._DeleteKey(NormalizeKey(key));
	}

	public static bool HasKey(string key)
	{
		_CheckInstance();
		return Instance?._HasKey(NormalizeKey(key)) ?? false;
	}

	public static void Save()
	{
		_CheckInstance();
		Instance?._Save();
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		_CheckInstance();
		return Instance?._GetInt(NormalizeKey(key), defaultValue) ?? defaultValue;
	}

	public static float GetFloat(string key, float defaultValue = 0f)
	{
		_CheckInstance();
		return Instance?._GetFloat(NormalizeKey(key), defaultValue) ?? defaultValue;
	}

	public static string GetString(string key, string defaultValue = "")
	{
		_CheckInstance();
		if (Instance == null)
		{
			return defaultValue;
		}
		return Instance._GetString(NormalizeKey(key), defaultValue);
	}

	public static void SetInt(string key, int value)
	{
		_CheckInstance();
		Instance?._SetInt(NormalizeKey(key), value);
	}

	public static void SetFloat(string key, float value)
	{
		_CheckInstance();
		Instance?._SetFloat(NormalizeKey(key), value);
	}

	public static void SetString(string key, string value)
	{
		_CheckInstance();
		Instance?._SetString(NormalizeKey(key), value);
	}

	private static void _CheckInstance()
	{
		if (Instance == null)
		{
			AsmoLogger.Trace("KeyValueStore", "KeyValueStore is not set");
		}
	}

	private static string NormalizeKey(string key)
	{
		return Application.platform.ToString() + key;
	}

	protected abstract void _DeleteAll();

	protected abstract void _DeleteKey(string key);

	protected abstract bool _HasKey(string key);

	protected abstract void _Save();

	protected abstract int _GetInt(string key, int defaultValue);

	protected abstract float _GetFloat(string key, float defaultValue);

	protected abstract string _GetString(string key, string defaultValue);

	protected abstract void _SetInt(string key, int value);

	protected abstract void _SetFloat(string key, float value);

	protected abstract void _SetString(string key, string value);
}
