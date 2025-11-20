#define DEBUG
using System.Collections.Generic;

namespace UdpKit;

public class UdpPacketPool
{
	private readonly UdpSocket socket;

	private readonly Stack<UdpPacket> pool = new Stack<UdpPacket>();

	internal UdpPacketPool(UdpSocket s)
	{
		socket = s;
	}

	internal void Release(UdpPacket packet)
	{
		UdpAssert.Assert(!packet.IsPooled);
		lock (pool)
		{
			packet.Size = 0;
			packet.Position = 0;
			packet.IsPooled = true;
			pool.Push(packet);
		}
	}

	public UdpPacket Acquire()
	{
		UdpPacket udpPacket = null;
		lock (pool)
		{
			if (pool.Count > 0)
			{
				udpPacket = pool.Pop();
			}
		}
		if (udpPacket == null)
		{
			udpPacket = new UdpPacket(new byte[socket.Config.PacketDatagramSize * 2], this);
		}
		UdpAssert.Assert(udpPacket.IsPooled);
		udpPacket.IsPooled = false;
		udpPacket.Position = 0;
		udpPacket.Size = socket.Config.PacketDatagramSize - socket.PacketPipeConfig.HeaderSize << 3;
		return udpPacket;
	}

	public void Free()
	{
		lock (pool)
		{
			while (pool.Count > 0)
			{
				pool.Pop();
			}
		}
	}
}
