using System;
using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_Guid : NetworkProperty
{
	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Guid;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		Guid guid = (Guid)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Guid, guid))
		{
			obj.Storage.Values[obj[this]].Guid = guid;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Guid;
	}

	public override int BitCount(NetworkObj obj)
	{
		return 128;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WriteGuid(storage.Values[obj[this]].Guid);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].Guid = packet.ReadGuid();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].Guid = other.Values[obj[this]].Guid;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Guid == storage2.Values[obj[this]].Guid;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
