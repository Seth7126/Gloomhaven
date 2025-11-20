using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal static class IdExtensions
{
	public static void WritePrefabId(this UdpPacket stream, PrefabId id)
	{
		stream.WriteIntVB(id.Value);
	}

	public static PrefabId ReadPrefabId(this UdpPacket stream)
	{
		return new PrefabId(stream.ReadIntVB());
	}

	public static void WriteTypeId(this UdpPacket stream, TypeId id)
	{
		stream.WriteIntVB(id.Value);
	}

	public static TypeId ReadTypeId(this UdpPacket stream)
	{
		return new TypeId(stream.ReadIntVB());
	}
}
