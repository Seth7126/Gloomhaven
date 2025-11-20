#define DEBUG
using System;
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

[Documentation]
public class NetworkArray_Transform : NetworkArray_Values<NetworkTransform>
{
	public new NetworkTransform this[int index] => base[index];

	internal NetworkArray_Transform(int length, int stride)
		: base(length, stride)
	{
		Assert.True(stride == 3);
	}

	protected override NetworkTransform GetValue(int index)
	{
		return Storage.Values[index].Transform;
	}

	protected override bool SetValue(int index, NetworkTransform value)
	{
		throw new NotSupportedException();
	}
}
