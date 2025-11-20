using System;

namespace AsmodeeNet.Foundation;

[Serializable]
public class FloatSetting : BaseSetting<float>
{
	public FloatSetting(string name)
		: base(name)
	{
	}

	protected override float _ReadValue()
	{
		return KeyValueStore.GetFloat(base._FullPath, base.DefaultValue);
	}

	protected override void _WriteValue(float value)
	{
		KeyValueStore.SetFloat(base._FullPath, value);
	}
}
