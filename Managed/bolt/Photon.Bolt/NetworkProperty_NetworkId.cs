using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_NetworkId : NetworkProperty
{
	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].NetworkId;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		NetworkId networkId = (NetworkId)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].NetworkId, networkId))
		{
			obj.Storage.Values[obj[this]].NetworkId = networkId;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].NetworkId;
	}

	public override int BitCount(NetworkObj obj)
	{
		return 64;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WriteNetworkId(storage.Values[obj[this]].NetworkId);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].NetworkId = packet.ReadNetworkId();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].NetworkId = other.Values[obj[this]].NetworkId;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].NetworkId == storage2.Values[obj[this]].NetworkId;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
