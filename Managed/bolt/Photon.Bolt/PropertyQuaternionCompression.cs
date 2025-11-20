using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal struct PropertyQuaternionCompression
{
	private bool QuaternionMode;

	private bool QuaternionStrictComparison;

	public PropertyVectorCompressionSettings Euler;

	public PropertyFloatCompressionSettings Quaternion;

	public int BitsRequired
	{
		get
		{
			if (QuaternionMode)
			{
				return Quaternion.BitsRequired * 4;
			}
			return Euler.BitsRequired;
		}
	}

	public bool StrictComparison
	{
		get
		{
			if (QuaternionMode)
			{
				return QuaternionStrictComparison;
			}
			return Euler.StrictComparison;
		}
	}

	public static PropertyQuaternionCompression Create(PropertyVectorCompressionSettings euler)
	{
		return new PropertyQuaternionCompression
		{
			Euler = euler,
			QuaternionMode = false
		};
	}

	public static PropertyQuaternionCompression Create(PropertyFloatCompressionSettings quaternion)
	{
		return Create(quaternion, strict: false);
	}

	public static PropertyQuaternionCompression Create(PropertyFloatCompressionSettings quaternion, bool strict)
	{
		return new PropertyQuaternionCompression
		{
			Quaternion = quaternion,
			QuaternionMode = true,
			QuaternionStrictComparison = strict
		};
	}

	public void Pack(UdpPacket stream, Quaternion value)
	{
		if (QuaternionMode)
		{
			Quaternion.Pack(stream, value.x);
			Quaternion.Pack(stream, value.y);
			Quaternion.Pack(stream, value.z);
			Quaternion.Pack(stream, value.w);
		}
		else
		{
			Euler.Pack(stream, value.eulerAngles);
		}
	}

	public Quaternion Read(UdpPacket stream)
	{
		if (QuaternionMode)
		{
			Quaternion result = default(Quaternion);
			result.x = Quaternion.Read(stream);
			result.y = Quaternion.Read(stream);
			result.z = Quaternion.Read(stream);
			result.w = Quaternion.Read(stream);
			return result;
		}
		return UnityEngine.Quaternion.Euler(Euler.Read(stream));
	}
}
