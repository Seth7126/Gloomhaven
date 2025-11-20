using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Photon.Bolt;

[StructLayout(LayoutKind.Explicit)]
public struct NetworkId : IEquatable<NetworkId>
{
	public class EqualityComparer : IEqualityComparer<NetworkId>
	{
		public static readonly EqualityComparer Instance = new EqualityComparer();

		private EqualityComparer()
		{
		}

		bool IEqualityComparer<NetworkId>.Equals(NetworkId x, NetworkId y)
		{
			return x.Packed == y.Packed;
		}

		int IEqualityComparer<NetworkId>.GetHashCode(NetworkId x)
		{
			return x.Packed.GetHashCode();
		}
	}

	[FieldOffset(0)]
	internal ulong Packed;

	[FieldOffset(0)]
	internal readonly uint Connection;

	[FieldOffset(4)]
	internal readonly uint Entity;

	public bool IsZero => Packed == 0;

	public ulong PackedValue => Packed;

	public NetworkId(ulong packed)
	{
		Entity = 0u;
		Connection = 0u;
		Packed = packed;
	}

	internal NetworkId(uint connection, uint entity)
	{
		Packed = 0uL;
		Entity = entity;
		Connection = connection;
	}

	public override string ToString()
	{
		byte b = (byte)(Packed >> 56);
		byte b2 = (byte)(Packed >> 48);
		byte b3 = (byte)(Packed >> 40);
		byte b4 = (byte)(Packed >> 32);
		byte b5 = (byte)(Packed >> 24);
		byte b6 = (byte)(Packed >> 16);
		byte b7 = (byte)(Packed >> 8);
		byte b8 = (byte)Packed;
		return $"[NetworkId {b:X0}-{b2:X0}-{b3:X0}-{b4:X0}-{b5:X0}-{b6:X0}-{b7:X0}-{b8:X0}]";
	}

	public static bool operator ==(NetworkId a, NetworkId b)
	{
		return a.Packed == b.Packed;
	}

	public static bool operator !=(NetworkId a, NetworkId b)
	{
		return a.Packed != b.Packed;
	}

	public override int GetHashCode()
	{
		return Packed.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is NetworkId)
		{
			return ((NetworkId)obj).Packed == Packed;
		}
		return false;
	}

	bool IEquatable<NetworkId>.Equals(NetworkId other)
	{
		return Packed == other.Packed;
	}
}
