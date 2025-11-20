using System;

namespace AsmodeeNet.Foundation;

[Serializable]
public class BoolSetting : BaseSetting<bool>
{
	public BoolSetting(string name)
		: base(name)
	{
	}

	protected override bool _ReadValue()
	{
		return _IntToBool(KeyValueStore.GetInt(base._FullPath, _BoolToInt(base.DefaultValue)));
	}

	protected override void _WriteValue(bool value)
	{
		KeyValueStore.SetInt(base._FullPath, _BoolToInt(value));
	}

	private bool _IntToBool(int i)
	{
		return i > 0;
	}

	private int _BoolToInt(bool b)
	{
		if (!b)
		{
			return 0;
		}
		return 1;
	}
}
