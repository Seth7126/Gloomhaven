using System;

namespace AsmodeeNet.Foundation;

[Serializable]
public class IntSetting : BaseSetting<int>
{
	public IntSetting(string name)
		: base(name)
	{
	}

	protected override int _ReadValue()
	{
		return KeyValueStore.GetInt(base._FullPath, base.DefaultValue);
	}

	protected override void _WriteValue(int value)
	{
		KeyValueStore.SetInt(base._FullPath, value);
	}
}
