using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal struct PropertyVectorCompressionSettings
{
	public bool StrictComparison;

	public PropertyFloatCompressionSettings X;

	public PropertyFloatCompressionSettings Y;

	public PropertyFloatCompressionSettings Z;

	public int BitsRequired => X.BitsRequired + Y.BitsRequired + Z.BitsRequired;

	public static PropertyVectorCompressionSettings Create(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z)
	{
		return Create(x, y, z, strict: false);
	}

	public static PropertyVectorCompressionSettings Create(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
	{
		return new PropertyVectorCompressionSettings
		{
			X = x,
			Y = y,
			Z = z,
			StrictComparison = strict
		};
	}

	public void Pack(UdpPacket stream, Vector3 value)
	{
		X.Pack(stream, value.x);
		Y.Pack(stream, value.y);
		Z.Pack(stream, value.z);
	}

	public Vector3 Read(UdpPacket stream)
	{
		Vector3 result = default(Vector3);
		result.x = X.Read(stream);
		result.y = Y.Read(stream);
		result.z = Z.Read(stream);
		return result;
	}
}
