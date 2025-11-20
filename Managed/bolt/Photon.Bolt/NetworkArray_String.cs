#define DEBUG
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

[Documentation]
public class NetworkArray_String : NetworkArray_Values<string>
{
	internal NetworkArray_String(int length, int stride)
		: base(length, stride)
	{
		Assert.True(stride == 1);
	}

	protected override string GetValue(int index)
	{
		return Storage.Values[index].String;
	}

	protected override bool SetValue(int index, string value)
	{
		if (Storage.Values[index].String != value)
		{
			Storage.Values[index].String = value;
			return true;
		}
		return false;
	}
}
