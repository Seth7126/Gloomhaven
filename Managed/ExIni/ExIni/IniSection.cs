using System.Collections.Generic;
using System.Linq;

namespace ExIni;

public class IniSection
{
	private readonly IniComment _comments;

	private readonly List<IniKey> _keys;

	public IniKey this[string key] => CreateKey(key);

	public IniComment Comments => _comments;

	public List<IniKey> Keys => _keys;

	public string Section { get; set; }

	public IniSection(string section)
	{
		Section = section;
		_comments = new IniComment();
		_keys = new List<IniKey>();
	}

	public override string ToString()
	{
		return $"[{Section}]";
	}

	public IniKey CreateKey(string key)
	{
		IniKey key2 = GetKey(key);
		if (key2 != null)
		{
			return key2;
		}
		IniKey iniKey = new IniKey(key);
		_keys.Add(iniKey);
		return iniKey;
	}

	public IniKey GetKey(string key)
	{
		if (!HasKey(key))
		{
			return null;
		}
		return _keys.FirstOrDefault((IniKey iniKey) => iniKey.Key == key);
	}

	public bool HasKey(string key)
	{
		return _keys.Any((IniKey iniKey) => iniKey.Key == key);
	}

	public bool DeleteKey(string key)
	{
		if (!HasKey(key))
		{
			return false;
		}
		Keys.Remove(GetKey(key));
		return true;
	}
}
