using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using UdpKit.Utils;
using UnityEngine;

namespace UdpKit.Platform.DotNet.Utils;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal static class DotNetPlatformUtils
{
	internal static List<UdpEndPoint> ResolveHostAddressesViaHostName(int port = 0, bool ipv6 = false)
	{
		List<UdpEndPoint> list = new List<UdpEndPoint>();
		try
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (ipv6 && iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					UdpEndPoint item = new UdpEndPoint(UdpIPv6Address.Parse(iPAddress.ToString()), (ushort)port);
					if (item.IsLan)
					{
						list.Add(item);
					}
				}
				if (!ipv6 && iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					UdpEndPoint item2 = new UdpEndPoint(UdpIPv4Address.Parse(iPAddress.ToString()), (ushort)port);
					if (item2.IsLan)
					{
						list.Add(item2);
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	internal static List<UdpEndPoint> ResolveHostLocalAddressViaNetworkInterfaces(int port = 0, bool ipv6 = false)
	{
		HashSet<UdpEndPoint> hashSet = new HashSet<UdpEndPoint>();
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				try
				{
					if ((networkInterface.OperationalStatus != OperationalStatus.Up && networkInterface.OperationalStatus != OperationalStatus.Unknown) || !ValidType(networkInterface))
					{
						continue;
					}
					foreach (string item3 in GetIpsFromNetworkInterface(networkInterface, ipv6))
					{
						if (ipv6)
						{
							UdpEndPoint item = new UdpEndPoint(UdpIPv6Address.Parse(item3), (ushort)port);
							if (item.IsLan)
							{
								hashSet.Add(item);
							}
						}
						else if (!ipv6)
						{
							UdpEndPoint item2 = new UdpEndPoint(UdpIPv4Address.Parse(item3), (ushort)port);
							if (item2.IsLan)
							{
								hashSet.Add(item2);
							}
						}
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
		}
		return new List<UdpEndPoint>(hashSet);
	}

	private static bool ValidType(NetworkInterface n)
	{
		switch (n.NetworkInterfaceType)
		{
		case NetworkInterfaceType.Unknown:
		case NetworkInterfaceType.Ethernet:
		case NetworkInterfaceType.Ethernet3Megabit:
		case NetworkInterfaceType.FastEthernetT:
		case NetworkInterfaceType.FastEthernetFx:
		case NetworkInterfaceType.Wireless80211:
		case NetworkInterfaceType.GigabitEthernet:
			return true;
		default:
			return false;
		}
	}

	private static HashSet<string> GetIpsFromNetworkInterface(NetworkInterface n, bool ipv6)
	{
		HashSet<string> hashSet = new HashSet<string>();
		IPInterfaceProperties iPProperties;
		try
		{
			iPProperties = n.GetIPProperties();
		}
		catch
		{
			return hashSet;
		}
		if (iPProperties != null)
		{
			try
			{
				foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
				{
					try
					{
						if (!ipv6 && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							hashSet.Add(unicastAddress.Address.ToString());
						}
						if (ipv6 && unicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6)
						{
							hashSet.Add(unicastAddress.Address.ToString());
						}
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
				}
			}
			catch (Exception message2)
			{
				Debug.LogError(message2);
			}
		}
		return hashSet;
	}

	internal static DotNetInterface ParseInterface(NetworkInterface n)
	{
		HashSet<UdpIPv4Address> hashSet = new HashSet<UdpIPv4Address>(UdpIPv4Address.Comparer.Instance);
		HashSet<UdpIPv4Address> hashSet2 = new HashSet<UdpIPv4Address>(UdpIPv4Address.Comparer.Instance);
		HashSet<UdpIPv4Address> hashSet3 = new HashSet<UdpIPv4Address>(UdpIPv4Address.Comparer.Instance);
		IPInterfaceProperties iPProperties;
		try
		{
			iPProperties = n.GetIPProperties();
		}
		catch
		{
			return null;
		}
		if (iPProperties != null)
		{
			try
			{
				foreach (IPAddress dnsAddress in iPProperties.DnsAddresses)
				{
					try
					{
						if (dnsAddress.AddressFamily == AddressFamily.InterNetwork)
						{
							hashSet.Add(dnsAddress.ConvertToUdpIPv4Address());
						}
					}
					catch (Exception ex)
					{
						UdpLog.Warn(ex.ToString());
					}
				}
			}
			catch (Exception ex2)
			{
				UdpLog.Warn(ex2.ToString());
			}
			try
			{
				foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
				{
					try
					{
						if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							UdpIPv4Address item = unicastAddress.Address.ConvertToUdpIPv4Address();
							hashSet2.Add(item);
							hashSet.Add(new UdpIPv4Address(item.Byte3, item.Byte2, item.Byte1, 1));
						}
					}
					catch (Exception ex3)
					{
						UdpLog.Warn(ex3.ToString());
					}
				}
			}
			catch (Exception ex4)
			{
				UdpLog.Warn(ex4.ToString());
			}
			try
			{
				foreach (MulticastIPAddressInformation multicastAddress in iPProperties.MulticastAddresses)
				{
					try
					{
						if (multicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							hashSet3.Add(multicastAddress.Address.ConvertToUdpIPv4Address());
						}
					}
					catch (Exception ex5)
					{
						UdpLog.Warn(ex5.ToString());
					}
				}
			}
			catch (Exception ex6)
			{
				UdpLog.Warn(ex6.ToString());
			}
			if (hashSet2.Count == 0 || hashSet.Count == 0)
			{
				return null;
			}
		}
		return new DotNetInterface(n, hashSet.ToArray(), hashSet2.ToArray(), hashSet3.ToArray());
	}

	internal static HashSet<UdpIPv4Address> FindBroadcastAddress()
	{
		HashSet<UdpIPv4Address> hashSet = new HashSet<UdpIPv4Address>();
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				try
				{
					if ((networkInterface.OperationalStatus != OperationalStatus.Up && networkInterface.OperationalStatus != OperationalStatus.Unknown) || !ValidType(networkInterface))
					{
						continue;
					}
					IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
					if (!IsValidInterface(iPProperties))
					{
						continue;
					}
					foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
					{
						if (unicastAddress.Address.AddressFamily != AddressFamily.InterNetwork)
						{
							continue;
						}
						if (iPProperties.DhcpServerAddresses.Count == 0)
						{
							byte[] addressBytes = unicastAddress.Address.GetAddressBytes();
							addressBytes[3] = byte.MaxValue;
							UdpIPv4Address item = UdpIPv4Address.Parse(new IPAddress(addressBytes).ToString());
							if (item.IsPrivate)
							{
								hashSet.Add(item);
							}
							continue;
						}
						foreach (IPAddress dhcpServerAddress in iPProperties.DhcpServerAddresses)
						{
							byte[] addressBytes2 = unicastAddress.Address.GetAddressBytes();
							byte[] addressBytes3 = unicastAddress.IPv4Mask.GetAddressBytes();
							if (addressBytes2.Length != addressBytes3.Length)
							{
								throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
							}
							byte[] array = new byte[addressBytes2.Length];
							for (int j = 0; j < array.Length; j++)
							{
								array[j] = (byte)(addressBytes2[j] | (addressBytes3[j] ^ 0xFF));
							}
							UdpIPv4Address item2 = UdpIPv4Address.Parse(new IPAddress(array).ToString());
							if (item2.IsPrivate)
							{
								hashSet.Add(item2);
							}
						}
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
		}
		return hashSet;
	}

	internal static bool IsValidInterface(IPInterfaceProperties p)
	{
		bool flag = true;
		foreach (GatewayIPAddressInformation gatewayAddress in p.GatewayAddresses)
		{
			byte[] addressBytes = gatewayAddress.Address.GetAddressBytes();
			flag &= addressBytes.Length == 4 && addressBytes[0] != 0 && addressBytes[1] != 0 && addressBytes[2] != 0 && addressBytes[3] != 0;
		}
		return flag;
	}
}
