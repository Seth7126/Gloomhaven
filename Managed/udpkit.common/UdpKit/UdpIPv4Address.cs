using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace UdpKit;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct UdpIPv4Address : IEquatable<UdpIPv4Address>, IComparable<UdpIPv4Address>
{
	public class Comparer : IComparer<UdpIPv4Address>, IEqualityComparer<UdpIPv4Address>
	{
		public static readonly Comparer Instance = new Comparer();

		private Comparer()
		{
		}

		int IComparer<UdpIPv4Address>.Compare(UdpIPv4Address x, UdpIPv4Address y)
		{
			return CompareOrder(x, y);
		}

		bool IEqualityComparer<UdpIPv4Address>.Equals(UdpIPv4Address x, UdpIPv4Address y)
		{
			return CompareOrder(x, y) == 0;
		}

		int IEqualityComparer<UdpIPv4Address>.GetHashCode(UdpIPv4Address obj)
		{
			return (int)obj.Packed;
		}
	}

	public static readonly UdpIPv4Address Any = default(UdpIPv4Address);

	public static readonly UdpIPv4Address Localhost = new UdpIPv4Address(127, 0, 0, 1);

	public static readonly UdpIPv4Address Broadcast = new UdpIPv4Address(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[FieldOffset(0)]
	public readonly uint Packed;

	[FieldOffset(0)]
	public byte Byte0;

	[FieldOffset(1)]
	public byte Byte1;

	[FieldOffset(2)]
	public byte Byte2;

	[FieldOffset(3)]
	public byte Byte3;

	public bool IsAny => Byte0 == 0 && Byte1 == 0 && Byte2 == 0 && Byte3 == 0;

	public bool IsLocalHost => Byte3 == 127 && Byte2 == 0 && Byte1 == 0 && Byte0 == 1;

	public bool IsBroadcast => Byte3 == byte.MaxValue && Byte2 == byte.MaxValue && Byte1 == byte.MaxValue && Byte0 == byte.MaxValue;

	public bool IsPrivate => Byte3 == 10 || (Byte3 == 172 && Byte2 >= 16 && Byte2 <= 31) || (Byte3 == 192 && Byte2 == 168);

	public bool IsWan => !IsAny && !IsLocalHost && !IsBroadcast && !IsPrivate;

	public UdpIPv4Address(uint packed)
	{
		Byte0 = (Byte1 = (Byte2 = (Byte3 = 0)));
		Packed = packed;
	}

	public UdpIPv4Address(long addr)
	{
		Byte0 = (Byte1 = (Byte2 = (Byte3 = 0)));
		Packed = (uint)IPAddress.NetworkToHostOrder((int)addr);
	}

	public UdpIPv4Address(byte a, byte b, byte c, byte d)
	{
		Packed = 0u;
		Byte0 = d;
		Byte1 = c;
		Byte2 = b;
		Byte3 = a;
	}

	public bool Equals(UdpIPv4Address other)
	{
		return CompareOrder(this, other) == 0;
	}

	public int CompareTo(UdpIPv4Address other)
	{
		return CompareOrder(this, other);
	}

	public override int GetHashCode()
	{
		return (int)Packed;
	}

	public override bool Equals(object obj)
	{
		return obj is UdpIPv4Address y && CompareOrder(this, y) == 0;
	}

	public override string ToString()
	{
		return $"{Byte3}.{Byte2}.{Byte1}.{Byte0}";
	}

	public static bool operator ==(UdpIPv4Address x, UdpIPv4Address y)
	{
		return CompareOrder(x, y) == 0;
	}

	public static bool operator !=(UdpIPv4Address x, UdpIPv4Address y)
	{
		return CompareOrder(x, y) != 0;
	}

	public static UdpIPv4Address operator &(UdpIPv4Address a, UdpIPv4Address b)
	{
		return new UdpIPv4Address(a.Packed & b.Packed);
	}

	private static int CompareOrder(UdpIPv4Address x, UdpIPv4Address y)
	{
		if (x.Packed > y.Packed)
		{
			return 1;
		}
		if (x.Packed < y.Packed)
		{
			return -1;
		}
		return 0;
	}

	public static UdpIPv4Address Parse(string address)
	{
		string[] array = address.Split(new char[1] { '.' });
		if (array.Length != 4)
		{
			throw new FormatException("address is not in the correct format");
		}
		return new UdpIPv4Address(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2]), byte.Parse(array[3]));
	}
}
