using System.Collections.Generic;
using Photon.Bolt.Utils;

namespace Photon.Bolt;

internal class EventReliableSendBuffer
{
	private enum State
	{
		Free,
		Send,
		Transit,
		Delivered
	}

	private struct Node
	{
		public State State;

		public EventReliable Value;
	}

	private int tail;

	private int mask;

	private int shift;

	private int count;

	private Node[] nodes;

	private BoltSequenceGenerator generator;

	public IEnumerable<EventReliable> Pending
	{
		get
		{
			EventReliable val;
			while (TryNext(out val))
			{
				yield return val;
			}
		}
	}

	public IEnumerable<EventReliable> Delivered
	{
		get
		{
			EventReliable val;
			while (TryRemove(out val))
			{
				yield return val;
			}
		}
	}

	public bool Full => count == nodes.Length;

	public EventReliableSendBuffer(int windowBits, int sequenceBits)
	{
		nodes = new Node[1 << windowBits];
		shift = 32 - sequenceBits;
		mask = nodes.Length - 1;
		generator = new BoltSequenceGenerator(sequenceBits, uint.MaxValue);
	}

	public bool TryEnqueue(EventReliable value)
	{
		int num = -1;
		if (count == 0)
		{
			num = tail;
		}
		else
		{
			if (count == nodes.Length)
			{
				return false;
			}
			num = (tail + count) & mask;
		}
		nodes[num].Value = value;
		nodes[num].Value.Sequence = generator.Next();
		nodes[num].State = State.Send;
		count++;
		return true;
	}

	public bool TryNext(out EventReliable value)
	{
		for (int i = 0; i < count; i++)
		{
			int num = (tail + i) & mask;
			if (nodes[num].State == State.Send)
			{
				nodes[num].State = State.Transit;
				value = nodes[num].Value;
				return true;
			}
		}
		value = default(EventReliable);
		return false;
	}

	public void SetDelivered(EventReliable value)
	{
		ChangeState(value, State.Delivered);
	}

	public void SetSend(EventReliable value)
	{
		ChangeState(value, State.Send);
	}

	public bool TryRemove(out EventReliable value)
	{
		if (count > 0 && nodes[tail].State == State.Delivered)
		{
			value = nodes[tail].Value;
			nodes[tail] = default(Node);
			tail++;
			tail &= mask;
			count--;
			return true;
		}
		value = default(EventReliable);
		return false;
	}

	private void ChangeState(EventReliable value, State state)
	{
		if (count != 0)
		{
			int num = SequenceDistance(value.Sequence, nodes[tail].Value.Sequence);
			if (num >= 0 && num < count)
			{
				nodes[(tail + num) & mask].State = state;
			}
		}
	}

	private int SequenceDistance(uint from, uint to)
	{
		from <<= shift;
		to <<= shift;
		return (int)(from - to) >> shift;
	}
}
