using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class NetworkUtils
{
	[StructLayout(LayoutKind.Sequential)]
	public class GetBandwidthInfoRequest : RequestBase
	{
		public GetBandwidthInfoRequest()
			: base(ServiceTypes.NetworkUtils, FunctionTypes.NetworkUtilsGetBandwidthInfo)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetBasicNetworkInfoRequest : RequestBase
	{
		public GetBasicNetworkInfoRequest()
			: base(ServiceTypes.NetworkUtils, FunctionTypes.NetworkUtilsGetBasicNetworkInfo)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetDetailedNetworkInfoRequest : RequestBase
	{
		public GetDetailedNetworkInfoRequest()
			: base(ServiceTypes.NetworkUtils, FunctionTypes.NetworkUtilsGetDetailedNetworkInfo)
		{
		}
	}

	public enum NetworkConnectionState
	{
		Disconnected,
		Connecting,
		ObtainingIP,
		ObtainedIP
	}

	public struct NpBandwidthTestResult
	{
		internal double uploadBps;

		internal double downloadBps;

		internal int result;

		public double UploadBps => uploadBps;

		public double DownloadBps => downloadBps;

		internal void Read(MemoryBuffer buffer)
		{
			uploadBps = buffer.ReadDouble();
			downloadBps = buffer.ReadDouble();
			result = buffer.ReadInt32();
		}

		public override string ToString()
		{
			return "Up Bps = " + uploadBps + " Down Bps = " + downloadBps;
		}
	}

	public struct NetInAddr
	{
		internal uint addr;

		public uint Addr
		{
			get
			{
				return addr;
			}
			set
			{
				addr = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			addr = buffer.ReadUInt32();
		}

		public override string ToString()
		{
			byte[] bytes = BitConverter.GetBytes(addr);
			string text = bytes[0].ToString();
			for (int i = 1; i < bytes.Length; i++)
			{
				text = text + "." + bytes[i];
			}
			return text;
		}
	}

	public enum RouterNatType
	{
		Type1 = 1,
		Type2,
		Type3
	}

	public enum RouterStun
	{
		Unchecked,
		Failed,
		OK
	}

	public struct NatRouterInfo
	{
		internal RouterStun stunStatus;

		internal RouterNatType natType;

		internal NetInAddr mappedAddr;

		public RouterStun StunStatus => stunStatus;

		public RouterNatType NatType => natType;

		public NetInAddr MappedAddr => mappedAddr;

		internal void Read(MemoryBuffer buffer)
		{
			stunStatus = (RouterStun)buffer.ReadInt32();
			natType = (RouterNatType)buffer.ReadInt32();
			mappedAddr.Read(buffer);
		}

		public override string ToString()
		{
			return string.Concat("Stun Status = ", stunStatus, " : Nat Type = ", natType, " : Mapped Addr = ", mappedAddr.ToString());
		}
	}

	public struct NetEtherAddr
	{
		public const int SCE_NET_ETHER_ADDR_LEN = 6;

		internal byte[] data;

		public byte[] Data => data;

		internal void Read(MemoryBuffer buffer)
		{
			data = new byte[6];
			buffer.ReadData(ref data);
		}

		public override string ToString()
		{
			if (data == null)
			{
				return "0.0.0.0.0.0";
			}
			string text = data[0].ToString();
			for (int i = 1; i < data.Length; i++)
			{
				text = text + "." + data[i];
			}
			return text;
		}
	}

	public class BandwidthInfoResponse : ResponseBase
	{
		internal NpBandwidthTestResult bandwidth;

		public NpBandwidthTestResult Bandwidth => bandwidth;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.BandwidthInfoBegin);
			bandwidth.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.BandwidthInfoEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class BasicNetworkInfoResponse : ResponseBase
	{
		internal string ipAddress;

		internal NatRouterInfo natInfo;

		internal NetworkConnectionState connectionStatus;

		public string IpAddress => ipAddress;

		public NatRouterInfo NatInfo => natInfo;

		public NetworkConnectionState ConnectionStatus => connectionStatus;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NetStateBasicBegin);
			memoryBuffer.ReadString(ref ipAddress);
			natInfo.Read(memoryBuffer);
			connectionStatus = (NetworkConnectionState)memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NetStateBasicEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum NetworkDevice
	{
		Wired,
		Wireless
	}

	public enum NetworkLink
	{
		Disconnected,
		Connected
	}

	public enum WfiSecurity
	{
		NoSecurity,
		WEP,
		WPAPSK_WPA2PSK,
		WPAPSK_TKIP,
		WPAPSK_AES,
		WPA2PSK_TKIP,
		WPA2PSK_AES,
		Unsupported
	}

	public enum NetworkIPConfig
	{
		DHCP,
		Static,
		PPPoE
	}

	public enum NetworkHTTPProxyConfig
	{
		Off,
		On
	}

	public class DetailedNetworkInfoResponse : ResponseBase
	{
		internal NatRouterInfo natInfo;

		internal NetworkConnectionState connectionStatus;

		internal NetworkDevice device;

		internal NetEtherAddr ethernetAddress;

		internal byte rssiPercentage;

		internal byte channel;

		internal uint mtu;

		internal NetworkLink link;

		internal WfiSecurity wifiSecurity;

		internal NetworkIPConfig ipConfig;

		internal NetworkHTTPProxyConfig httpProxyConfig;

		internal ushort httpProxyPort;

		internal NetEtherAddr bssid;

		internal string ssid = "";

		internal string dhcpHostname = "";

		internal string pppoeAuthName = "";

		internal string ipAddress = "";

		internal string netmask = "";

		internal string defaultRoute = "";

		internal string primaryDNS = "";

		internal string secondaryDNS = "";

		internal string httpProxyServer = "";

		public NatRouterInfo NatInfo => natInfo;

		public NetworkConnectionState ConnectionStatus => connectionStatus;

		public NetworkDevice Device => device;

		public NetEtherAddr EthernetAddress => ethernetAddress;

		public byte RssiPercentage => rssiPercentage;

		public byte Channel => channel;

		public uint MTU => mtu;

		public NetworkLink Link => link;

		public WfiSecurity WifiSecurity => wifiSecurity;

		public NetworkIPConfig IpConfig => ipConfig;

		public NetworkHTTPProxyConfig HttpProxyConfig => httpProxyConfig;

		public ushort HttpProxyPort => httpProxyPort;

		public NetEtherAddr BSSID => bssid;

		public string SSID => ssid;

		public string DhcpHostname => dhcpHostname;

		public string PPPoeAuthName => pppoeAuthName;

		public string IpAddress => ipAddress;

		public string Netmask => netmask;

		public string DefaultRoute => defaultRoute;

		public string PrimaryDNS => primaryDNS;

		public string SecondaryDNS => secondaryDNS;

		public string HttpProxyServer => httpProxyServer;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NetStateDetailedBegin);
			natInfo.Read(memoryBuffer);
			connectionStatus = (NetworkConnectionState)memoryBuffer.ReadUInt32();
			device = (NetworkDevice)memoryBuffer.ReadUInt32();
			ethernetAddress.Read(memoryBuffer);
			rssiPercentage = memoryBuffer.ReadUInt8();
			channel = memoryBuffer.ReadUInt8();
			mtu = memoryBuffer.ReadUInt32();
			link = (NetworkLink)memoryBuffer.ReadUInt32();
			wifiSecurity = (WfiSecurity)memoryBuffer.ReadUInt32();
			ipConfig = (NetworkIPConfig)memoryBuffer.ReadUInt32();
			httpProxyConfig = (NetworkHTTPProxyConfig)memoryBuffer.ReadUInt32();
			httpProxyPort = memoryBuffer.ReadUInt16();
			bssid.Read(memoryBuffer);
			memoryBuffer.ReadString(ref ssid);
			memoryBuffer.ReadString(ref dhcpHostname);
			memoryBuffer.ReadString(ref pppoeAuthName);
			memoryBuffer.ReadString(ref ipAddress);
			memoryBuffer.ReadString(ref netmask);
			memoryBuffer.ReadString(ref defaultRoute);
			memoryBuffer.ReadString(ref primaryDNS);
			memoryBuffer.ReadString(ref secondaryDNS);
			memoryBuffer.ReadString(ref httpProxyServer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NetStateDetailedEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public enum NetworkEvent
	{
		none,
		networkConnected,
		networkDisconnected
	}

	public class NetStateChangeResponse : ResponseBase
	{
		internal NetworkEvent netEvent;

		public NetworkEvent NetEvent => netEvent;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NetStateChangeBegin);
			netEvent = (NetworkEvent)memoryBuffer.ReadInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NetStateChangeEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetBandwidthInfo(GetBandwidthInfoRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetBasicNetworkInfo(GetBasicNetworkInfoRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetDetailedNetworkInfo(GetDetailedNetworkInfoRequest request, out APIResult result);

	public static int GetBandwidthInfo(GetBandwidthInfoRequest request, BandwidthInfoResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetBandwidthInfo(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetBasicNetworkInfoInfo(GetBasicNetworkInfoRequest request, BasicNetworkInfoResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetBasicNetworkInfo(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetDetailedNetworkInfo(GetDetailedNetworkInfoRequest request, DetailedNetworkInfoResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetDetailedNetworkInfo(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
