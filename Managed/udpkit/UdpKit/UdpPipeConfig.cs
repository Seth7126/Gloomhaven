namespace UdpKit;

internal struct UdpPipeConfig
{
	public const int TYPE_BYTES = 1;

	public const int PING_BYTES = 2;

	public byte PipeId;

	public bool UpdatePing;

	public uint Timeout;

	public int WindowSize;

	public int DatagramSize;

	public int AckBytes;

	public int SequenceBytes;

	public int SequenceBits => SequenceBytes * 8;

	public int AckBits => AckBytes * 8;

	public int HeaderSize => 3 + SequenceBytes + SequenceBytes + AckBytes;

	public int HeaderSizeBits => HeaderSize * 8;

	public uint NextSequence(uint seq)
	{
		seq++;
		seq &= (uint)((1 << SequenceBytes * 8) - 1);
		return seq;
	}

	public int Distance(uint from, uint to)
	{
		int num = (4 - SequenceBytes) * 8;
		to <<= num;
		from <<= num;
		return (int)(from - to) >> num;
	}
}
