using System;
using System.Net;

namespace UdpKit.Utils;

public static class SubnetMask
{
	public static readonly IPAddress ClassA = IPAddress.Parse("255.0.0.0");

	public static readonly IPAddress ClassB = IPAddress.Parse("255.255.0.0");

	public static readonly IPAddress ClassC = IPAddress.Parse("255.255.255.0");

	public static IPAddress CreateByHostBitLength(int hostpartLength)
	{
		int num = 32 - hostpartLength;
		if (num < 2)
		{
			throw new ArgumentException("Number of hosts is to large for IPv4");
		}
		byte[] array = new byte[4];
		for (int i = 0; i < 4; i++)
		{
			if (i * 8 + 8 <= num)
			{
				array[i] = byte.MaxValue;
				continue;
			}
			if (i * 8 > num)
			{
				array[i] = 0;
				continue;
			}
			int totalWidth = num - i * 8;
			string value = string.Empty.PadLeft(totalWidth, '1').PadRight(8, '0');
			array[i] = Convert.ToByte(value, 2);
		}
		return new IPAddress(array);
	}

	public static IPAddress CreateByNetBitLength(int netpartLength)
	{
		int hostpartLength = 32 - netpartLength;
		return CreateByHostBitLength(hostpartLength);
	}

	public static IPAddress CreateByHostNumber(int numberOfHosts)
	{
		int num = numberOfHosts + 1;
		string text = Convert.ToString(num, 2);
		return CreateByHostBitLength(text.Length);
	}
}
