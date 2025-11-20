using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using UdpKit.Platform.DotNet.Utils;
using UdpKit.Utils;
using UnityEngine;

namespace UdpKit.Platform;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class DotNetPlatform : UdpPlatform
{
	private UdpConfig _config;

	internal override bool SessionListProvidedExternally => true;

	internal override bool SupportsBroadcast => true;

	internal override bool SupportsMasterServer => true;

	public DotNetPlatform()
	{
		GetPrecisionTime();
	}

	internal override UdpSessionSource GetSessionSource()
	{
		return UdpSessionSource.Lan;
	}

	internal override UdpIPv4Address GetBroadcastAddress()
	{
		List<UdpIPv4Address> list = new List<UdpIPv4Address>();
		list.AddRange(DotNetPlatformUtils.FindBroadcastAddress());
		if (list.Count == 0)
		{
			foreach (UdpEndPoint item in ResolveHostAddresses(1))
			{
				UdpIPv4Address address = item.Address;
				address.Byte0 = byte.MaxValue;
				if (address.IsPrivate)
				{
					list.Add(address);
				}
			}
		}
		return (list.Count == 0) ? UdpIPv4Address.Any : list[0];
	}

	internal override UdpPlatformSocket CreateSocket(bool ipv6)
	{
		return new DotNetSocket(this, ipv6);
	}

	internal override List<UdpPlatformInterface> GetNetworkInterfaces()
	{
		List<UdpPlatformInterface> list = new List<UdpPlatformInterface>();
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				try
				{
					if ((networkInterface.OperationalStatus == OperationalStatus.Up || networkInterface.OperationalStatus == OperationalStatus.Unknown) && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
					{
						DotNetInterface dotNetInterface = DotNetPlatformUtils.ParseInterface(networkInterface);
						if (dotNetInterface != null)
						{
							list.Add(dotNetInterface);
						}
					}
				}
				catch (Exception ex)
				{
					UdpLog.Error(ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			UdpLog.Error(ex2.Message);
		}
		return list;
	}

	internal override List<UdpEndPoint> ResolveHostAddresses(int port = 0, bool ipv6 = false)
	{
		List<UdpEndPoint> list = new List<UdpEndPoint>();
		list.AddRange(DotNetPlatformUtils.ResolveHostAddressesViaHostName(port, ipv6));
		bool flag = true;
		RuntimePlatform currentPlatform = _config.CurrentPlatform;
		RuntimePlatform runtimePlatform = currentPlatform;
		if (runtimePlatform == RuntimePlatform.Switch)
		{
			flag = false;
		}
		if (flag && list.Count == 0)
		{
			list.AddRange(DotNetPlatformUtils.ResolveHostLocalAddressViaNetworkInterfaces(port, ipv6));
		}
		return (list.Count == 0) ? base.ResolveHostAddresses(port, ipv6) : list;
	}

	internal override uint GetPrecisionTime()
	{
		return PrecisionTimer.GetCurrentTime();
	}

	internal override void Configure(UdpConfig config)
	{
		_config = config;
	}
}
