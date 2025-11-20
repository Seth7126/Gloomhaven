using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ExIni;

public class IniKey
{
	private readonly IniComment _comments;

	public IniComment Comments => _comments;

	public string Key { get; set; }

	public string RawValue { get; set; }

	public string Value
	{
		get
		{
			return Resolve(RawValue);
		}
		set
		{
			RawValue = value;
		}
	}

	public IniKey(string key, string value = null)
	{
		Key = key;
		Value = value;
		_comments = new IniComment();
	}

	public override string ToString()
	{
		return $"{Key}={RawValue}";
	}

	private static string GetEnvironment(string env)
	{
		return Environment.ExpandEnvironmentVariables(env);
	}

	private static string GetRegistry(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		string fileName = Path.GetFileName(path);
		if (string.IsNullOrEmpty(directoryName))
		{
			return null;
		}
		return Registry.GetValue(directoryName, fileName, string.Empty)?.ToString();
	}

	private static string Resolve(string value)
	{
		if (value == null)
		{
			return null;
		}
		Regex regex = new Regex("\\$\\((?<reg>.*)\\)");
		Regex regex2 = new Regex("%.*%");
		while (regex.IsMatch(value) || regex2.IsMatch(value))
		{
			value = regex.Replace(value, (Match match) => GetRegistry(match.Groups["reg"].Value));
			value = GetEnvironment(value);
		}
		return value;
	}
}
