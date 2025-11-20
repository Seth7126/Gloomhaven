#define DEBUG
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

[Documentation]
public class NetworkArray_Integer : NetworkArray_Values<int>
{
	internal NetworkArray_Integer(int length, int stride)
		: base(length, stride)
	{
		Assert.True(stride == 1);
	}

	protected override int GetValue(int index)
	{
		return Storage.Values[index].Int0;
	}

	protected override bool SetValue(int index, int value)
	{
		if (Storage.Values[index].Int0 != value)
		{
			Storage.Values[index].Int0 = value;
			return true;
		}
		return false;
	}
}
