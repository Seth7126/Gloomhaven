using System.Collections.Generic;
using Photon.Bolt.Internal;

namespace Photon.Bolt.Channel;

internal class BinaryDataChannel : BoltChannel
{
	public Queue<byte[]> Incomming = new Queue<byte[]>();

	public Queue<byte[]> Outgoing = new Queue<byte[]>();

	public override void Pack(Packet packet)
	{
		if (packet.UdpPacket.WriteBool(Outgoing.Count > 0))
		{
			packet.UdpPacket.WriteByteArrayWithPrefix(Outgoing.Dequeue());
		}
	}

	public override void Read(Packet packet)
	{
		if (packet.UdpPacket.ReadBool())
		{
			Incomming.Enqueue(packet.UdpPacket.ReadByteArrayWithPrefix());
		}
	}

	public override void Disconnected()
	{
		Incomming.Clear();
		Outgoing.Clear();
	}
}
