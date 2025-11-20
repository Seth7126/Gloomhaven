using System;
using System.Net;

namespace UdpKit.Utils;

public static class IPAddressExtensions
{
	public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
	{
		byte[] addressBytes = address.GetAddressBytes();
		byte[] addressBytes2 = subnetMask.GetAddressBytes();
		if (addressBytes.Length != addressBytes2.Length)
		{
			throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
		}
		byte[] array = new byte[addressBytes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)(addressBytes[i] | (addressBytes2[i] ^ 0xFF));
		}
		return new IPAddress(array);
	}

	public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
	{
		byte[] addressBytes = address.GetAddressBytes();
		byte[] addressBytes2 = subnetMask.GetAddressBytes();
		if (addressBytes.Length != addressBytes2.Length)
		{
			throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
		}
		byte[] array = new byte[addressBytes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)(addressBytes[i] & addressBytes2[i]);
		}
		return new IPAddress(array);
	}

	public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
	{
		IPAddress networkAddress = address.GetNetworkAddress(subnetMask);
		IPAddress networkAddress2 = address2.GetNetworkAddress(subnetMask);
		return networkAddress.Equals(networkAddress2);
	}

	public static bool IsInSameSubnet(this IPAddress address2, IPAddress address)
	{
		return address2.IsInSameSubnet(address, SubnetMask.ClassA) || address2.IsInSameSubnet(address, SubnetMask.ClassB) || address2.IsInSameSubnet(address, SubnetMask.ClassC);
	}

	public static bool IsPrivate(this IPAddress address)
	{
		byte[] addressBytes = address.GetAddressBytes();
		return addressBytes[0] == 10 || (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31) || (addressBytes[0] == 192 && addressBytes[1] == 168);
	}

	public static long ToLong(this IPAddress address)
	{
		byte[] addressBytes = address.GetAddressBytes();
		long num = (long)((ulong)addressBytes[3] << 24);
		num += (long)((ulong)addressBytes[2] << 16);
		num += (long)((ulong)addressBytes[1] << 8);
		return num + addressBytes[0];
	}

	public static IPAddress FromLongAddr(this long addr)
	{
		return new IPAddress(addr);
	}
}
