using System.Collections.Generic;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class PacketPool
{
	private static readonly Stack<Packet> Pool = new Stack<Packet>();

	public static Packet Acquire()
	{
		return (Pool.Count == 0) ? new Packet() : Pool.Pop();
	}

	public static void ReturnToPool(Packet packet)
	{
		Pool.Push(packet);
	}
}
