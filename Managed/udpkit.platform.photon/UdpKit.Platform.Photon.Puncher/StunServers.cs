#define DEBUG
using System.Net;
using System.Net.Sockets;

namespace UdpKit.Platform.Photon.Puncher;

internal static class StunServers
{
	public class StunServer
	{
		public IPEndPoint IPv4 = null;

		public IPEndPoint IPv6 = null;
	}

	public static StunServer GOOGLE_1;

	public static StunServer GOOGLE_2;

	public static StunServer GOOGLE_3;

	public static StunServer GOOGLE_4;

	public static StunServer STUN_PROTOCOL;

	public static StunServer CUSTOM_STUN_SERVER;

	public static StunServer[] ALL_SERVERS => new StunServer[6] { CUSTOM_STUN_SERVER, GOOGLE_1, GOOGLE_2, GOOGLE_3, GOOGLE_4, STUN_PROTOCOL };

	public static bool RefreshServers()
	{
		CUSTOM_STUN_SERVER = CUSTOM_STUN_SERVER ?? BuildIP("stun.l.google.com", 19302);
		GOOGLE_1 = GOOGLE_1 ?? BuildIP("stun1.l.google.com", 19302);
		GOOGLE_2 = GOOGLE_2 ?? BuildIP("stun2.l.google.com", 19302);
		GOOGLE_3 = GOOGLE_3 ?? BuildIP("stun3.l.google.com", 19302);
		GOOGLE_4 = GOOGLE_4 ?? BuildIP("stun4.l.google.com", 19302);
		STUN_PROTOCOL = STUN_PROTOCOL ?? BuildIP("stun.stunprotocol.org", 3478);
		return CUSTOM_STUN_SERVER != null || GOOGLE_1 != null || GOOGLE_2 != null || GOOGLE_3 != null || GOOGLE_4 != null || STUN_PROTOCOL != null;
	}

	private static StunServer BuildIP(string ipOrname, int port)
	{
		if (IPAddress.TryParse(ipOrname, out var address))
		{
			return new StunServer
			{
				IPv4 = new IPEndPoint(address, port)
			};
		}
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(ipOrname);
			StunServer stunServer = new StunServer();
			IPAddress[] array = hostAddresses;
			foreach (IPAddress iPAddress in array)
			{
				if (stunServer.IPv4 == null && iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					UdpLog.Info("STUN Server {0} resolved as {1}", ipOrname, iPAddress);
					stunServer.IPv4 = new IPEndPoint(iPAddress, port);
				}
				if (stunServer.IPv6 == null && iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					UdpLog.Info("STUN Server {0} resolved as {1}", ipOrname, iPAddress);
					stunServer.IPv6 = new IPEndPoint(iPAddress, port);
				}
			}
			return stunServer;
		}
		catch
		{
			UdpLog.Warn("Unable to resolve STUN Address");
		}
		return null;
	}

	public static void SetCustomSTUNServer(string ipOrname, int port)
	{
		CUSTOM_STUN_SERVER = BuildIP(ipOrname, port);
		if (CUSTOM_STUN_SERVER == null)
		{
			UdpLog.Warn("Unable to parse Custom STUN Server");
			return;
		}
		UdpLog.Debug("Using Custom STUN Server at {0}", CUSTOM_STUN_SERVER);
	}
}
