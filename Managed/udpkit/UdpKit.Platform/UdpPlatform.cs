using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UdpKit.Platform;

public abstract class UdpPlatform
{
	internal UdpSocket udpSocket { get; set; }

	internal object Token { get; set; }

	internal virtual bool IsNull => false;

	internal virtual bool SessionListProvidedExternally => false;

	internal virtual bool ShutdownOnConnectFailure => true;

	internal virtual bool SupportsBroadcast => false;

	internal virtual bool SupportsMasterServer => false;

	internal virtual float SessionListUpdateRate => 5f;

	internal virtual bool SkipWanCheck => false;

	internal virtual Dictionary<string, object> GetPlatformMetadata()
	{
		return new Dictionary<string, object>();
	}

	internal abstract UdpSessionSource GetSessionSource();

	internal virtual UdpSession GetCurrentSession()
	{
		return null;
	}

	internal virtual bool SessionListHasChanged()
	{
		return false;
	}

	internal virtual List<UdpSession> GetSessionList()
	{
		return new List<UdpSession>();
	}

	internal virtual bool HandleSetHostInfo(UdpSession session, Action<bool, UdpSessionError> result = null)
	{
		return false;
	}

	internal virtual bool HandleConnectToSession(UdpSession session, object protocolToken, Action<bool, UdpSessionError> result = null)
	{
		result?.Invoke(arg1: false, UdpSessionError.Error);
		return false;
	}

	internal virtual bool HandleConnectToRandomSession(UdpSessionFilter sessionFilter, object protocolToken, Action<bool, UdpSessionError> result = null)
	{
		result?.Invoke(arg1: false, UdpSessionError.Error);
		return false;
	}

	internal virtual UdpSession BuildSession(string id)
	{
		return UdpSessionImpl.Build(id);
	}

	internal abstract UdpPlatformSocket CreateSocket(bool ipv6);

	internal abstract List<UdpPlatformInterface> GetNetworkInterfaces();

	internal virtual UdpIPv4Address GetBroadcastAddress()
	{
		return UdpIPv4Address.Broadcast;
	}

	internal virtual List<UdpEndPoint> ResolveHostAddresses(int port = 0, bool ipv6 = false)
	{
		return new List<UdpEndPoint> { ipv6 ? new UdpEndPoint(UdpIPv6Address.Any, (ushort)port) : new UdpEndPoint(UdpIPv4Address.Any, (ushort)port) };
	}

	internal virtual UdpPlatformSocket CreateBroadcastSocket(UdpEndPoint endpoint, bool bind = true)
	{
		UdpPlatformSocket udpPlatformSocket = CreateSocket(endpoint);
		udpPlatformSocket.Broadcast = true;
		return udpPlatformSocket;
	}

	internal UdpPlatformSocket CreateSocket(UdpEndPoint endpoint)
	{
		UdpPlatformSocket udpPlatformSocket = CreateSocket(endpoint.IPv6);
		udpPlatformSocket.Bind(endpoint);
		while (!udpPlatformSocket.IsBound)
		{
		}
		return udpPlatformSocket;
	}

	internal virtual void OnStartBegin()
	{
	}

	internal virtual void OnStartDone(UdpEndPoint localEndPoint, Action<bool, UdpConnectionDisconnectReason> doneCallback)
	{
		doneCallback(arg1: true, UdpConnectionDisconnectReason.Unknown);
	}

	internal virtual void OnStartupFailed()
	{
	}

	internal virtual Task OnShutdown()
	{
		return Task.CompletedTask;
	}

	internal virtual void OnConnect(UdpConnection connection)
	{
		connection.ConnectionType = UdpConnectionType.Direct;
	}

	internal virtual void OnInternalEvent(UdpEvent evt)
	{
	}

	internal abstract uint GetPrecisionTime();

	internal virtual void Configure(UdpConfig config)
	{
	}

	public override string ToString()
	{
		return GetType().ToString();
	}
}
