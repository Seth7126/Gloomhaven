using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Color32 : NetworkProperty
{
	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Color32;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		Color32 color = (Color32)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Color32, color))
		{
			obj.Storage.Values[obj[this]].Color32 = color;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Color32;
	}

	public override int BitCount(NetworkObj obj)
	{
		return 128;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WriteColor32RGBA(storage.Values[obj[this]].Color32);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].Color32 = packet.ReadColor32RGBA();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].Color32 = other.Values[obj[this]].Color32;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		Color32 color = storage1.Values[obj[this]].Color32;
		Color32 color2 = storage2.Values[obj[this]].Color32;
		return color.r == color2.r && color.g == color2.g && color.b == color2.b && color.a == color2.a;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
