namespace Photon.Bolt.Utils;

internal struct BoltSequenceGenerator
{
	private readonly uint mask;

	private uint sequence;

	public BoltSequenceGenerator(int bits)
		: this(bits, 0u)
	{
	}

	public BoltSequenceGenerator(int bits, uint start)
	{
		mask = (uint)((1 << bits) - 1);
		sequence = start & mask;
	}

	public uint Next()
	{
		sequence++;
		sequence &= mask;
		return sequence;
	}
}
