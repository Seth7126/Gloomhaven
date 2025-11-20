using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Color : NetworkProperty
{
	public override int BitCount(NetworkObj obj)
	{
		return 32;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		Color color = (Color)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Color, color))
		{
			obj.Storage.Values[obj[this]].Color = color;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Color;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Color;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WriteColorRGBA(storage.Values[obj[this]].Color);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].Color = packet.ReadColorRGBA();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].Color = other.Values[obj[this]].Color;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Color == storage2.Values[obj[this]].Color;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
