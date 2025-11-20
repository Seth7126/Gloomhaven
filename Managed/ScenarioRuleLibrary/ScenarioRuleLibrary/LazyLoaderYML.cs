#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using SM.Utils;

namespace ScenarioRuleLibrary;

public class LazyLoaderYML<T>
{
	private Dictionary<string, string> _filesDict = new Dictionary<string, string>();

	private Dictionary<string, T> _loadedYMLs = new Dictionary<string, T>();

	private Func<StreamReader, string, Dictionary<string, T>, bool> _parseMethod;

	public LazyLoaderYML(string[] files, Func<StreamReader, string, Dictionary<string, T>, bool> parseMethod)
	{
		_parseMethod = parseMethod;
		LoadAllFilename(files);
	}

	private void LoadAllFilename(string[] files)
	{
		foreach (string text in files)
		{
			string key = Path.GetFileNameWithoutExtension(text).ToLower();
			if (!_filesDict.ContainsKey(key))
			{
				_filesDict.Add(key, text);
			}
		}
	}

	public T GetYML(string id, bool removeSuffix = false)
	{
		lock (string.Intern(id))
		{
			if (_loadedYMLs.ContainsKey(id))
			{
				return _loadedYMLs[id];
			}
			if (LoadYML(id, removeSuffix))
			{
				return _loadedYMLs[id];
			}
			return default(T);
		}
	}

	private string RemoveSuffix(string source, string suffix)
	{
		int num = source.IndexOf(suffix, StringComparison.Ordinal);
		if (num >= 0)
		{
			return source.Remove(num, suffix.Length);
		}
		return source;
	}

	private bool LoadYML(string id, bool removeSuffix)
	{
		string text = (removeSuffix ? RemoveSuffix(id, "ID") : id);
		string value = string.Empty;
		_filesDict.TryGetValue(text.ToLower(), out value);
		if (value == null)
		{
			LogUtils.Log("[LazyLoader] Some missing asset(id=" + id + "). Not Loaded by lazy loading.");
			return false;
		}
		using MemoryStream stream = new MemoryStream(File.ReadAllBytes(value));
		using StreamReader arg = new StreamReader(stream);
		return _parseMethod(arg, value, _loadedYMLs);
	}
}
