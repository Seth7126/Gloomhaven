using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpKit.Utils;

public static class UdpEndPointExtensions
{
	private static readonly Dictionary<IPEndPoint, UdpEndPoint> ipEndPointBuffer = new Dictionary<IPEndPoint, UdpEndPoint>();

	private static readonly Dictionary<UdpEndPoint, IPEndPoint> udpEndPointBuffer = new Dictionary<UdpEndPoint, IPEndPoint>();

	public static UdpEndPoint ConvertToUdpEndPoint(this EndPoint endpoint)
	{
		return ((IPEndPoint)endpoint).ConvertToUdpEndPoint();
	}

	public static UdpEndPoint ConvertToUdpEndPoint(this IPEndPoint endpoint)
	{
		Register(endpoint);
		return ipEndPointBuffer[endpoint];
	}

	public static IPEndPoint ConvertToIPEndPoint(this UdpEndPoint endpoint)
	{
		Register(endpoint);
		return udpEndPointBuffer[endpoint];
	}

	public static UdpIPv4Address ConvertToUdpIPv4Address(this IPAddress address)
	{
		return new UdpIPv4Address(address.ToLong());
	}

	private static void Register(UdpEndPoint endPoint, IPEndPoint ipEndPoint = null)
	{
		if (!udpEndPointBuffer.ContainsKey(endPoint))
		{
			if (ipEndPoint == null)
			{
				ipEndPoint = ((!endPoint.IPv6) ? new IPEndPoint(new IPAddress(new byte[4]
				{
					endPoint.Address.Byte3,
					endPoint.Address.Byte2,
					endPoint.Address.Byte1,
					endPoint.Address.Byte0
				}), endPoint.Port) : new IPEndPoint(new IPAddress(endPoint.AddressIPv6.Bytes), endPoint.Port));
			}
			udpEndPointBuffer.Add(endPoint, ipEndPoint);
			Register(ipEndPoint, endPoint);
		}
	}

	private static void Register(IPEndPoint ipEndPoint, UdpEndPoint endPoint = default(UdpEndPoint))
	{
		if (!ipEndPointBuffer.ContainsKey(ipEndPoint))
		{
			if (endPoint == default(UdpEndPoint))
			{
				endPoint = ((ipEndPoint.Address.AddressFamily != AddressFamily.InterNetworkV6) ? new UdpEndPoint(new UdpIPv4Address(ipEndPoint.Address.ToLong()), (ushort)ipEndPoint.Port) : new UdpEndPoint(new UdpIPv6Address(ipEndPoint.Address.GetAddressBytes()), (ushort)ipEndPoint.Port));
			}
			ipEndPointBuffer.Add(ipEndPoint, endPoint);
			Register(endPoint, ipEndPoint);
		}
	}

	internal static byte[] SerializeEndPoint(this UdpEndPoint endPoint)
	{
		if (endPoint.IPv6)
		{
			return Encoding.UTF8.GetBytes($"[{endPoint.AddressIPv6.ToString()}]:{endPoint.Port}");
		}
		return Encoding.UTF8.GetBytes($"{endPoint.Address.ToString()}:{endPoint.Port}");
	}

	internal static UdpEndPoint DeserializeEndPoint(this byte[] encodedEndPoint)
	{
		string endpoint = Encoding.UTF8.GetString(encodedEndPoint);
		return UdpEndPoint.Parse(endpoint);
	}

	internal static bool IsSameNetwork(this UdpEndPoint endPoint, UdpEndPoint otherEndPoint)
	{
		if (endPoint.IPv6 || otherEndPoint.IPv6)
		{
			return false;
		}
		IPEndPoint iPEndPoint = endPoint.ConvertToIPEndPoint();
		IPEndPoint iPEndPoint2 = otherEndPoint.ConvertToIPEndPoint();
		return iPEndPoint.Address.IsInSameSubnet(iPEndPoint2.Address);
	}
}
