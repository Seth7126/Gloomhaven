using Photon.Bolt.Utils;

namespace Photon.Bolt;

internal class EventReliableRecvBuffer
{
	private struct Node
	{
		public bool Received;

		public EventReliable Value;
	}

	private int tail;

	private int mask;

	private int sequenceShift;

	private uint sequenceNext;

	private uint sequenceMask;

	private readonly Node[] nodes;

	public EventReliableRecvBuffer(int windowBits, int sequenceBits)
	{
		nodes = new Node[1 << windowBits];
		mask = nodes.Length - 1;
		sequenceShift = 32 - sequenceBits;
		sequenceMask = (uint)((1 << sequenceBits) - 1);
		sequenceNext = 0u;
	}

	public bool TryRemove(out EventReliable value)
	{
		Node node = nodes[tail];
		if (node.Received)
		{
			value = node.Value;
			nodes[tail] = default(Node);
			tail++;
			tail &= mask;
			sequenceNext = value.Sequence + 1;
			sequenceNext &= sequenceMask;
		}
		else
		{
			value = default(EventReliable);
		}
		return node.Received;
	}

	public RecvBufferAddResult TryEnqueue(EventReliable value)
	{
		int num = Math.SequenceDistance(value.Sequence, sequenceNext, sequenceShift);
		int num2 = (tail + num) & mask;
		if (num <= -nodes.Length || num >= nodes.Length)
		{
			return RecvBufferAddResult.OutOfBounds;
		}
		if (num < 0)
		{
			return RecvBufferAddResult.Old;
		}
		if (nodes[num2].Received)
		{
			return RecvBufferAddResult.AlreadyExists;
		}
		nodes[num2].Received = true;
		nodes[num2].Value = value;
		return RecvBufferAddResult.Added;
	}
}
