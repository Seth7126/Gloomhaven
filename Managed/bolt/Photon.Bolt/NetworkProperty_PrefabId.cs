using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_PrefabId : NetworkProperty
{
	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].PrefabId;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		PrefabId prefabId = (PrefabId)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].PrefabId, prefabId))
		{
			obj.Storage.Values[obj[this]].PrefabId = prefabId;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].PrefabId;
	}

	public override int BitCount(NetworkObj obj)
	{
		return 32;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WritePrefabId(storage.Values[obj[this]].PrefabId);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].PrefabId = packet.ReadPrefabId();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].PrefabId = other.Values[obj[this]].PrefabId;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].PrefabId == storage2.Values[obj[this]].PrefabId;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
