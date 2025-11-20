using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UdpKit;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct UdpEndPoint : IEquatable<UdpEndPoint>, IComparable<UdpEndPoint>
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Native
	{
		public UdpIPv4Address Address;

		public ushort Port;

		public UdpEndPoint AsManaged => new UdpEndPoint(Address, Port);
	}

	public class Comparer : IEqualityComparer<UdpEndPoint>
	{
		bool IEqualityComparer<UdpEndPoint>.Equals(UdpEndPoint x, UdpEndPoint y)
		{
			return Compare(x, y) == 0;
		}

		int IEqualityComparer<UdpEndPoint>.GetHashCode(UdpEndPoint obj)
		{
			return obj.GetHashCode();
		}
	}

	public static readonly UdpEndPoint Any = new UdpEndPoint(UdpIPv4Address.Any, 0);

	public static readonly UdpEndPoint AnyIPv6 = new UdpEndPoint(UdpIPv6Address.Any, 0);

	[FieldOffset(0)]
	public readonly ushort Port;

	[FieldOffset(0)]
	public readonly UdpSteamID SteamId;

	[FieldOffset(8)]
	public readonly bool IPv6;

	[FieldOffset(12)]
	public readonly UdpIPv4Address Address;

	[FieldOffset(12)]
	public readonly UdpIPv6Address AddressIPv6;

	public bool IsWan => (IPv6 ? AddressIPv6.IsWan : Address.IsWan) && Port > 0;

	public bool IsLan => (IPv6 ? AddressIPv6.IsPrivate : Address.IsPrivate) && Port > 0;

	public Native AsNative
	{
		get
		{
			if (IPv6)
			{
				throw new UdpException("Native does not support IPv6 currently");
			}
			return new Native
			{
				Address = Address,
				Port = Port
			};
		}
	}

	public UdpEndPoint(UdpIPv4Address address, ushort port)
	{
		SteamId = default(UdpSteamID);
		AddressIPv6 = default(UdpIPv6Address);
		Address = address;
		IPv6 = false;
		Port = port;
	}

	public UdpEndPoint(UdpIPv6Address address, ushort port)
	{
		SteamId = default(UdpSteamID);
		Address = default(UdpIPv4Address);
		AddressIPv6 = address;
		IPv6 = true;
		Port = port;
	}

	public UdpEndPoint(UdpSteamID steamId)
	{
		Port = 0;
		IPv6 = false;
		Address = default(UdpIPv4Address);
		AddressIPv6 = default(UdpIPv6Address);
		SteamId = steamId;
	}

	public UdpEndPoint(ulong id)
		: this(new UdpSteamID(id))
	{
	}

	public int CompareTo(UdpEndPoint other)
	{
		return Compare(this, other);
	}

	public bool Equals(UdpEndPoint other)
	{
		return Compare(this, other) == 0;
	}

	public override int GetHashCode()
	{
		return (int)(Address.Packed ^ Port);
	}

	public override bool Equals(object obj)
	{
		return obj is UdpEndPoint y && Compare(this, y) == 0;
	}

	public override string ToString()
	{
		if (Address.IsAny && AddressIPv6.IsAny && SteamId.Id != 0)
		{
			return $"[EndPoint ID {SteamId.Id}]";
		}
		if (IPv6)
		{
			return $"[EndPoint IPv6 {new IPAddress(AddressIPv6.Bytes)}:{Port}]";
		}
		return $"[EndPoint IPv4 {Address}:{Port}]";
	}

	public static UdpEndPoint Parse(string endpoint)
	{
		string[] array = endpoint.Split(new char[1] { ':' });
		if (array.Length != 2)
		{
			Regex regex = new Regex("\\[(.*)\\]:(\\d+)", RegexOptions.IgnoreCase);
			MatchCollection matchCollection = regex.Matches(endpoint);
			if (matchCollection.Count > 0)
			{
				UdpIPv6Address address = UdpIPv6Address.Parse(matchCollection[0].Groups[1].Value);
				ushort port = ushort.Parse(matchCollection[0].Groups[2].Value);
				return new UdpEndPoint(address, port);
			}
			throw new FormatException("endpoint is not in the correct format");
		}
		UdpIPv4Address address2 = UdpIPv4Address.Parse(array[0]);
		return new UdpEndPoint(address2, ushort.Parse(array[1]));
	}

	public static bool operator ==(UdpEndPoint x, UdpEndPoint y)
	{
		return Compare(x, y) == 0;
	}

	public static bool operator !=(UdpEndPoint x, UdpEndPoint y)
	{
		return Compare(x, y) != 0;
	}

	public static UdpEndPoint operator &(UdpEndPoint a, UdpEndPoint b)
	{
		return new UdpEndPoint(a.Address & b.Address, (ushort)(a.Port & b.Port));
	}

	private static int Compare(UdpEndPoint x, UdpEndPoint y)
	{
		if (x.AddressIPv6.IsAny && y.AddressIPv6.IsAny && x.Address.IsAny && y.Address.IsAny)
		{
			ulong id = x.SteamId.Id;
			return id.CompareTo(y.SteamId.Id);
		}
		int num = ((x.IPv6 && y.IPv6) ? x.AddressIPv6.CompareTo(y.AddressIPv6) : x.Address.CompareTo(y.Address));
		if (num == 0)
		{
			ushort port = x.Port;
			num = port.CompareTo(y.Port);
		}
		return num;
	}
}
