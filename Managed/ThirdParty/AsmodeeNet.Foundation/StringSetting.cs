using System;

namespace AsmodeeNet.Foundation;

[Serializable]
public class StringSetting : BaseSetting<string>
{
	public StringSetting(string name)
		: base(name)
	{
	}

	protected override string _ReadValue()
	{
		return KeyValueStore.GetString(base._FullPath, base.DefaultValue);
	}

	protected override void _WriteValue(string value)
	{
		KeyValueStore.SetString(base._FullPath, value);
	}
}
