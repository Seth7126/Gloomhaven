#define DEBUG
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

[Documentation]
public class NetworkArray_Float : NetworkArray_Values<float>
{
	internal NetworkArray_Float(int length, int stride)
		: base(length, stride)
	{
		Assert.True(stride == 1 || stride == 2);
	}

	protected override float GetValue(int index)
	{
		return Storage.Values[index].Float0;
	}

	protected override bool SetValue(int index, float value)
	{
		if (Storage.Values[index].Float0 != value)
		{
			Storage.Values[index].Float0 = value;
			return true;
		}
		return false;
	}
}
