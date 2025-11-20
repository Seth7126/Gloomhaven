using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UdpKit.Utils;

namespace UdpKit.Platform;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class DotNetSocket : UdpPlatformSocket
{
	private string error;

	protected Socket socket;

	private readonly DotNetPlatform platform;

	protected EndPoint recvEndPoint;

	protected UdpEndPoint endpoint;

	private static byte[] PING_DATA = new byte[0];

	private static UdpEndPoint PING_TARGET_4 = UdpEndPoint.Parse("203.0.113.0:65530");

	private static UdpEndPoint PING_TARGET_6 = UdpEndPoint.Parse("[2001:db8::]:65530");

	internal override UdpPlatform Platform => platform;

	internal override string Error => error;

	internal override bool IsBound => socket != null && socket.IsBound;

	internal override UdpEndPoint EndPoint
	{
		get
		{
			VerifyIsBound();
			return endpoint;
		}
	}

	internal override bool Broadcast
	{
		get
		{
			VerifyIsBound();
			try
			{
				error = null;
				return socket.EnableBroadcast;
			}
			catch (SocketException exn)
			{
				HandleSocketException(exn);
				return false;
			}
		}
		set
		{
			VerifyIsBound();
			try
			{
				socket.EnableBroadcast = value;
			}
			catch (SocketException exn)
			{
				error = null;
				HandleSocketException(exn);
			}
		}
	}

	public DotNetSocket(DotNetPlatform platform, bool ipv6)
	{
		this.platform = platform;
		try
		{
			socket = new Socket(ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (ipv6)
			{
				socket.DualMode = true;
			}
			socket.Blocking = false;
		}
		catch (SocketException exn)
		{
			HandleSocketException(exn);
		}
		if (ipv6)
		{
			recvEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
		}
		else
		{
			recvEndPoint = new IPEndPoint(IPAddress.Any, 0);
		}
	}

	internal override void Close()
	{
		VerifyIsBound();
		try
		{
			error = null;
			socket.Close();
		}
		catch (SocketException exn)
		{
			HandleSocketException(exn);
		}
	}

	internal override void Bind(UdpEndPoint ep)
	{
		try
		{
			error = null;
			socket.Bind(ep.ConvertToIPEndPoint());
			SendTo(PING_DATA, ep.IPv6 ? PING_TARGET_6 : PING_TARGET_4);
			endpoint = socket.LocalEndPoint.ConvertToUdpEndPoint();
		}
		catch (SocketException exn)
		{
			HandleSocketException(exn);
		}
	}

	internal override bool RecvPoll()
	{
		return RecvPoll(0);
	}

	internal override bool RecvPoll(int timeout)
	{
		try
		{
			return socket.Poll(timeout * 1000, SelectMode.SelectRead);
		}
		catch (SocketException exn)
		{
			HandleSocketException(exn);
			return false;
		}
	}

	internal override int RecvFrom(byte[] buffer, int bufferSize, ref UdpEndPoint remoteEndpoint)
	{
		try
		{
			int num = socket.ReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref recvEndPoint);
			if (num > 0)
			{
				remoteEndpoint = recvEndPoint.ConvertToUdpEndPoint();
				return num;
			}
			return -1;
		}
		catch (SocketException exn)
		{
			HandleSocketException(exn);
			return -1;
		}
	}

	internal override int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint)
	{
		try
		{
			return socket.SendTo(buffer, 0, bytesToSend, SocketFlags.None, endpoint.ConvertToIPEndPoint());
		}
		catch (SocketException exn)
		{
			HandleSocketException(exn);
			return -1;
		}
	}

	private void HandleSocketException(SocketException exn)
	{
		if (exn.ErrorCode != 10054)
		{
			error = exn.ErrorCode + ": " + exn.SocketErrorCode;
			UdpLog.Warn(error);
		}
	}

	private void VerifyIsBound()
	{
		if (!IsBound)
		{
			throw new InvalidOperationException();
		}
	}
}
