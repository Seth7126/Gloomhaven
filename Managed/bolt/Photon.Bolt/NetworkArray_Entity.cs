#define DEBUG
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

[Documentation]
public class NetworkArray_Entity : NetworkArray_Values<BoltEntity>
{
	internal NetworkArray_Entity(int length, int stride)
		: base(length, stride)
	{
		Assert.True(stride == 1);
	}

	protected override BoltEntity GetValue(int index)
	{
		return BoltNetwork.FindEntity(Storage.Values[index].NetworkId);
	}

	protected override bool SetValue(int index, BoltEntity value)
	{
		NetworkId networkId = ((value == null) ? default(NetworkId) : value.Entity.NetworkId);
		if (Storage.Values[index].NetworkId != networkId)
		{
			Storage.Values[index].NetworkId = networkId;
			return true;
		}
		return false;
	}
}
