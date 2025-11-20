using UdpKit;

namespace Photon.Bolt;

internal struct PropertyIntCompressionSettings
{
	private int _bits;

	private int _shift;

	public int BitsRequired => _bits;

	public static PropertyIntCompressionSettings Create()
	{
		return new PropertyIntCompressionSettings
		{
			_bits = 32
		};
	}

	public static PropertyIntCompressionSettings Create(int bits, int shift)
	{
		return new PropertyIntCompressionSettings
		{
			_bits = bits,
			_shift = shift
		};
	}

	public void Pack(UdpPacket stream, int value)
	{
		stream.WriteInt_Shifted(value, _bits, _shift);
	}

	public int Read(UdpPacket stream)
	{
		return stream.ReadInt_Shifted(_bits, _shift);
	}
}
