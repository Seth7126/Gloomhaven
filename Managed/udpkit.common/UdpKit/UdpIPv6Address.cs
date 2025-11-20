#define DEBUG
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace UdpKit;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct UdpIPv6Address : IEquatable<UdpIPv6Address>, IComparable<UdpIPv6Address>
{
	public class Comparer : IComparer<UdpIPv6Address>, IEqualityComparer<UdpIPv6Address>
	{
		public static readonly Comparer Instance = new Comparer();

		private Comparer()
		{
		}

		int IComparer<UdpIPv6Address>.Compare(UdpIPv6Address x, UdpIPv6Address y)
		{
			return CompareOrder(x, y);
		}

		bool IEqualityComparer<UdpIPv6Address>.Equals(UdpIPv6Address x, UdpIPv6Address y)
		{
			return CompareOrder(x, y) == 0;
		}

		int IEqualityComparer<UdpIPv6Address>.GetHashCode(UdpIPv6Address obj)
		{
			return (int)(obj.Packed0 ^ obj.Packed1);
		}
	}

	public static readonly UdpIPv6Address Any = default(UdpIPv6Address);

	public static readonly UdpIPv6Address Mask = new UdpIPv6Address(new byte[16]
	{
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255
	});

	public static readonly UdpIPv6Address Localhost = new UdpIPv6Address(new byte[16]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 1
	});

	[FieldOffset(0)]
	public byte Byte0;

	[FieldOffset(1)]
	public byte Byte1;

	[FieldOffset(2)]
	public byte Byte2;

	[FieldOffset(3)]
	public byte Byte3;

	[FieldOffset(4)]
	public byte Byte4;

	[FieldOffset(5)]
	public byte Byte5;

	[FieldOffset(6)]
	public byte Byte6;

	[FieldOffset(7)]
	public byte Byte7;

	[FieldOffset(8)]
	public byte Byte8;

	[FieldOffset(9)]
	public byte Byte9;

	[FieldOffset(10)]
	public byte Byte10;

	[FieldOffset(11)]
	public byte Byte11;

	[FieldOffset(12)]
	public byte Byte12;

	[FieldOffset(13)]
	public byte Byte13;

	[FieldOffset(14)]
	public byte Byte14;

	[FieldOffset(15)]
	public byte Byte15;

	[FieldOffset(0)]
	public readonly ulong Packed0;

	[FieldOffset(8)]
	public readonly ulong Packed1;

	public bool IsAny => Packed0 == 0L && Packed1 == 0;

	public bool IsLocalHost => this == Localhost;

	public bool IsBroadcast => false;

	public bool IsWan => !IsAny && !IsLocalHost && !IsBroadcast && !IsPrivate;

	public bool IsPrivate
	{
		get
		{
			if (IsAny)
			{
				return true;
			}
			IPAddress iPAddress = new IPAddress(Bytes);
			string text = iPAddress.ToString();
			string text2 = text.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
			if (iPAddress.AddressFamily != AddressFamily.InterNetworkV6)
			{
				return false;
			}
			if (iPAddress.IsIPv6SiteLocal)
			{
				return true;
			}
			if (text2.Substring(0, 2) == "fc" && text2.Length >= 4)
			{
				return true;
			}
			if (text2.Substring(0, 2) == "fd" && text2.Length >= 4)
			{
				return true;
			}
			if (text2 == "fe80")
			{
				return true;
			}
			if (text2 == "100")
			{
				return true;
			}
			return false;
		}
	}

	public byte[] Bytes => new byte[16]
	{
		Byte0, Byte1, Byte2, Byte3, Byte4, Byte5, Byte6, Byte7, Byte8, Byte9,
		Byte10, Byte11, Byte12, Byte13, Byte14, Byte15
	};

	public UdpIPv6Address(byte[] address)
	{
		UdpAssert.Assert(address.Length == 16);
		Packed0 = 0uL;
		Packed1 = 0uL;
		Byte0 = address[0];
		Byte1 = address[1];
		Byte2 = address[2];
		Byte3 = address[3];
		Byte4 = address[4];
		Byte5 = address[5];
		Byte6 = address[6];
		Byte7 = address[7];
		Byte8 = address[8];
		Byte9 = address[9];
		Byte10 = address[10];
		Byte11 = address[11];
		Byte12 = address[12];
		Byte13 = address[13];
		Byte14 = address[14];
		Byte15 = address[15];
	}

	public UdpIPv6Address(ulong packed0, ulong packed1)
	{
		this = default(UdpIPv6Address);
		Packed0 = packed0;
		Packed1 = packed1;
	}

	public bool Equals(UdpIPv6Address other)
	{
		return CompareOrder(this, other) == 0;
	}

	public int CompareTo(UdpIPv6Address other)
	{
		return CompareOrder(this, other);
	}

	public override int GetHashCode()
	{
		return (int)(Packed0 ^ Packed1);
	}

	public override bool Equals(object obj)
	{
		return obj is UdpIPv6Address y && CompareOrder(this, y) == 0;
	}

	public static bool operator ==(UdpIPv6Address x, UdpIPv6Address y)
	{
		return CompareOrder(x, y) == 0;
	}

	public static bool operator !=(UdpIPv6Address x, UdpIPv6Address y)
	{
		return CompareOrder(x, y) != 0;
	}

	public static UdpIPv6Address Parse(string address)
	{
		IPAddress iPAddress = IPAddress.Parse(address);
		if (iPAddress.AddressFamily != AddressFamily.InterNetworkV6)
		{
			throw new ArgumentException("Invalid IPv6 address");
		}
		return new UdpIPv6Address(iPAddress.GetAddressBytes());
	}

	public static UdpIPv6Address operator &(UdpIPv6Address a, UdpIPv6Address b)
	{
		return new UdpIPv6Address(a.Packed0 & b.Packed0, a.Packed1 & b.Packed1);
	}

	public override string ToString()
	{
		return new IPAddress(Bytes).ToString();
	}

	private static int CompareOrder(UdpIPv6Address x, UdpIPv6Address y)
	{
		if (x.Packed0 > y.Packed0)
		{
			return 1;
		}
		if (x.Packed0 < y.Packed0)
		{
			return -1;
		}
		if (x.Packed1 > y.Packed1)
		{
			return 1;
		}
		if (x.Packed1 < y.Packed1)
		{
			return -1;
		}
		return 0;
	}
}
