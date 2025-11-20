using System;
using UdpKit;

namespace Photon.Bolt;

internal struct PropertyFloatCompressionSettings
{
	private int _bits;

	private float _pack;

	private float _read;

	private float _shift;

	public int BitsRequired => _bits;

	public static PropertyFloatCompressionSettings Create()
	{
		return new PropertyFloatCompressionSettings
		{
			_bits = 32
		};
	}

	public static PropertyFloatCompressionSettings Create(int bits, float shift, float pack, float read)
	{
		PropertyFloatCompressionSettings result = default(PropertyFloatCompressionSettings);
		result._bits = bits;
		result._pack = pack;
		result._read = read;
		result._shift = shift;
		return result;
	}

	public void Pack(UdpPacket stream, float value)
	{
		switch (_bits)
		{
		case 0:
			break;
		case 32:
			stream.WriteFloat(value);
			break;
		default:
			stream.WriteInt((int)Math.Round((value + _shift) * _pack), _bits);
			break;
		}
	}

	public float Read(UdpPacket stream)
	{
		return _bits switch
		{
			0 => 0f, 
			32 => stream.ReadFloat(), 
			_ => (float)stream.ReadInt(_bits) * _read + (0f - _shift), 
		};
	}
}
