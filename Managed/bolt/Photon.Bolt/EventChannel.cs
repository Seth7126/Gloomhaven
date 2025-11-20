#define DEBUG
using System.Collections.Generic;
using Photon.Bolt.Channel;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal class EventChannel : BoltChannel
{
	private readonly List<EventUnreliable> unreliableSend;

	private readonly EventReliableSendBuffer reliableOrderedSend;

	private readonly EventReliableRecvBuffer reliableOrderedRecv;

	public EventChannel()
	{
		unreliableSend = new List<EventUnreliable>(256);
		reliableOrderedSend = new EventReliableSendBuffer(10, 12);
		reliableOrderedRecv = new EventReliableRecvBuffer(10, 12);
	}

	public void Queue(Event ev)
	{
		if (ev.Reliability == ReliabilityModes.Unreliable)
		{
			unreliableSend.Add(EventUnreliable.Wrap(ev));
			ev.IncrementRefs();
		}
		else if (reliableOrderedSend.TryEnqueue(EventReliable.Wrap(ev)))
		{
			ev.IncrementRefs();
		}
		else
		{
			BoltLog.Warn("The reliable-ordered event queue for {0} is full, disconnecting", base.connection);
			base.connection.Disconnect();
		}
	}

	public override void Delivered(Packet packet)
	{
		for (int i = 0; i < packet.ReliableEvents.Count; i++)
		{
			reliableOrderedSend.SetDelivered(packet.ReliableEvents[i]);
		}
		EventReliable value;
		while (reliableOrderedSend.TryRemove(out value))
		{
			value.NetworkEvent.DecrementRefs();
		}
		packet.ReliableEvents.Clear();
	}

	public override void Lost(Packet packet)
	{
		for (int i = 0; i < packet.ReliableEvents.Count; i++)
		{
			reliableOrderedSend.SetSend(packet.ReliableEvents[i]);
		}
		packet.ReliableEvents.Clear();
	}

	public override void Pack(Packet packet)
	{
		int position = packet.UdpPacket.Position;
		for (int i = 0; i < unreliableSend.Count; i++)
		{
			EventUnreliable value = unreliableSend[i];
			if (value.NetworkEvent.IsEntityEvent && !base.connection._entityChannel.ExistsOnRemote(value.NetworkEvent.TargetEntity))
			{
				unreliableSend[i].NetworkEvent.DecrementRefs();
				unreliableSend.RemoveAt(i);
				i--;
			}
			else
			{
				value.Priority = (value.NetworkEvent.IsEntityEvent ? value.NetworkEvent.TargetEntity.PriorityCalculator.CalculateEventPriority(base.connection, value.NetworkEvent) : 10f);
				unreliableSend[i] = value;
			}
		}
		unreliableSend.Sort(EventUnreliable.PriorityComparer.Instance);
		int position2 = packet.UdpPacket.Position;
		EventReliable value2;
		while (reliableOrderedSend.TryNext(out value2))
		{
			int position3 = packet.UdpPacket.Position;
			bool flag = PackEvent(value2.NetworkEvent, packet.UdpPacket, value2.Sequence);
			bool flag2 = !packet.UdpPacket.Overflowing;
			if (!flag)
			{
				BoltLog.Error("Reliable failed to pack, this means all other reliable events will stall");
			}
			if (flag && flag2)
			{
				packet.ReliableEvents.Add(value2);
				continue;
			}
			packet.UdpPacket.Ptr = position3;
			reliableOrderedSend.SetSend(value2);
			break;
		}
		for (int j = 0; j < unreliableSend.Count; j++)
		{
			int position4 = packet.UdpPacket.Position;
			bool flag3 = PackEvent(unreliableSend[j].NetworkEvent, packet.UdpPacket, 0u);
			bool flag4 = !packet.UdpPacket.Overflowing;
			if (flag3 && flag4)
			{
				unreliableSend[j].NetworkEvent.DecrementRefs();
				unreliableSend.RemoveAt(j);
				j--;
			}
			else
			{
				packet.UdpPacket.Ptr = position4;
			}
		}
		packet.UdpPacket.WriteStopMarker();
		for (int k = 0; k < unreliableSend.Count; k++)
		{
			EventUnreliable value3 = unreliableSend[k];
			if (value3.Skipped)
			{
				unreliableSend.RemoveAt(k);
				k--;
			}
			else
			{
				value3.Skipped = true;
				unreliableSend[k] = value3;
			}
		}
		packet.Stats.EventBits = packet.UdpPacket.Position - position;
	}

	private bool PackEvent(Event ev, UdpPacket stream, uint sequence)
	{
		BoltLog.Debug("sending event {0}", ev);
		stream.WriteContinueMarker();
		stream.WriteTypeId(ev.Meta.TypeId);
		stream.WriteInt(ev.Targets, 5);
		if (stream.WriteBool(ev.Reliability == ReliabilityModes.ReliableOrdered))
		{
			stream.WriteUInt(sequence, 12);
		}
		else if (ev.IsEntityEvent)
		{
			stream.WriteEntity(ev.TargetEntity);
		}
		stream.WriteByteArrayLengthPrefixed(ev.BinaryData, BoltCore._config.packetSize / 2);
		return ev.Pack(base.connection, stream);
	}

	public override void Read(Packet packet)
	{
		int position = packet.UdpPacket.Position;
		while (packet.UdpPacket.ReadStopMarker())
		{
			uint sequence = 0u;
			Event obj = ReadEvent(packet.UdpPacket, ref sequence);
			BoltLog.Debug("recv event {0}", obj);
			if (obj.Reliability == ReliabilityModes.Unreliable)
			{
				EventDispatcher.Received(obj);
				continue;
			}
			RecvBufferAddResult recvBufferAddResult = reliableOrderedRecv.TryEnqueue(EventReliable.Wrap(obj, sequence));
			RecvBufferAddResult recvBufferAddResult2 = recvBufferAddResult;
			if ((uint)recvBufferAddResult2 <= 2u)
			{
				BoltLog.Debug("FAILED");
				obj.DecrementRefs();
			}
		}
		EventReliable value;
		while (reliableOrderedRecv.TryRemove(out value))
		{
			EventDispatcher.Received(value.NetworkEvent);
		}
		packet.Stats.EventBits = packet.UdpPacket.Position - position;
	}

	private Event ReadEvent(UdpPacket stream, ref uint sequence)
	{
		Event obj = Factory.NewEvent(stream.ReadTypeId());
		obj.Targets = stream.ReadInt(5);
		obj.SourceConnection = base.connection;
		if (stream.ReadBool())
		{
			sequence = stream.ReadUInt(12);
			obj.Reliability = ReliabilityModes.ReliableOrdered;
		}
		else
		{
			if (obj.IsEntityEvent)
			{
				obj.TargetEntity = stream.ReadEntity();
			}
			obj.Reliability = ReliabilityModes.Unreliable;
		}
		obj.BinaryData = stream.ReadByteArraySimple();
		obj.Read(base.connection, stream);
		return obj;
	}
}
