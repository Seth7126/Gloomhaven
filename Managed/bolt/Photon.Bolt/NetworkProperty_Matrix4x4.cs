using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Matrix4x4 : NetworkProperty
{
	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Matrix4x4;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		Matrix4x4 matrix4x = (Matrix4x4)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Matrix4x4, matrix4x))
		{
			obj.Storage.Values[obj[this]].Matrix4x4 = matrix4x;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Matrix4x4;
	}

	public override int BitCount(NetworkObj obj)
	{
		return 512;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WriteMatrix4x4(storage.Values[obj[this]].Matrix4x4);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].Matrix4x4 = packet.ReadMatrix4x4();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].Matrix4x4 = other.Values[obj[this]].Matrix4x4;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Matrix4x4 == storage2.Values[obj[this]].Matrix4x4;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
