using System;
using System.Collections.Generic;
using UdpKit;

namespace Photon.Bolt.Internal;

public class Packet : IDisposable
{
	public volatile bool Pooled = true;

	public int Frame;

	public int Number;

	public PacketStats Stats;

	public UdpPacket UdpPacket;

	public List<EventReliable> ReliableEvents = new List<EventReliable>();

	public Queue<EntityProxyEnvelope> EntityUpdates = new Queue<EntityProxyEnvelope>();

	public void Dispose()
	{
		Frame = 0;
		Number = 0;
		UdpPacket?.Dispose();
		UdpPacket = null;
		Stats.Clear();
		ReliableEvents.Clear();
		EntityUpdates.Clear();
		PacketPool.ReturnToPool(this);
	}
}
