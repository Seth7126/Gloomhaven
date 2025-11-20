using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UdpKit.Platform.Photon;
using UdpKit.Platform.Photon.Puncher;
using UdpKit.Utils;

namespace UdpKit.Platform;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
public class PhotonPlatform : UdpPlatform
{
	private readonly DotNetPlatform dotNetPlatform;

	internal readonly StunManager stunManager;

	public PhotonPlatformConfig PhotonConfig { get; private set; }

	internal override bool ShutdownOnConnectFailure => false;

	internal override bool SupportsMasterServer => PhotonConfig.UsePunchThrough;

	internal override bool SessionListProvidedExternally => true;

	internal override bool SkipWanCheck => true;

	internal override bool SupportsBroadcast => dotNetPlatform.SupportsBroadcast;

	internal override float SessionListUpdateRate => dotNetPlatform.SessionListUpdateRate;

	public PhotonPlatform(PhotonPlatformConfig config = null)
	{
		PhotonConfig = config ?? new PhotonPlatformConfig();
		stunManager = new StunManager(PhotonConfig);
		if (PhotonConfig.CustomSTUNServer != null)
		{
			string[] array = PhotonConfig.CustomSTUNServer.Split(new char[1] { ':' });
			if (array.Length == 2 && int.TryParse(array[1], out var result) && result != 0)
			{
				StunServers.SetCustomSTUNServer(array[0], result);
			}
			else
			{
				UdpLog.Warn("Unable to parse CustomSTUNServer");
			}
		}
		dotNetPlatform = new DotNetPlatform();
		GetPrecisionTime();
	}

	internal override Dictionary<string, object> GetPlatformMetadata()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		fill("Region", PhotonPoller.GetConnectedRegion());
		fill("Data", PhotonPoller.GetCustomAuthData());
		fill("UserId", PhotonPoller.GetUserId());
		fill("Nickname", PhotonPoller.GetNickName());
		fill("LobbyStatistics", PhotonPoller.GetLobbyStatistics());
		return data;
		void fill(string key, object value)
		{
			if (value != null)
			{
				data[key] = value;
			}
		}
	}

	internal override uint GetPrecisionTime()
	{
		return dotNetPlatform.GetPrecisionTime();
	}

	internal override UdpSessionSource GetSessionSource()
	{
		return UdpSessionSource.Photon;
	}

	internal override UdpSession GetCurrentSession()
	{
		return PhotonPoller.GetSession();
	}

	internal override bool SessionListHasChanged()
	{
		return PhotonPoller.SessionHasChanged();
	}

	internal override List<UdpSession> GetSessionList()
	{
		return PhotonPoller.GetSessions();
	}

	internal override bool HandleSetHostInfo(UdpSession session, Action<bool, UdpSessionError> result = null)
	{
		if (PhotonConfig.UsePunchThrough)
		{
			while (!stunManager.IsDone)
			{
			}
			session.LanEndPoint = stunManager.InternalEndPoint;
			session.WanEndPoint = stunManager.ExternalEndpoint;
		}
		else
		{
			session.LanEndPoint = UdpEndPoint.Any;
			session.WanEndPoint = UdpEndPoint.Any;
		}
		return PhotonPoller.SetHostInfo(session, result);
	}

	internal override bool HandleConnectToSession(UdpSession session, object protocolToken, Action<bool, UdpSessionError> result = null)
	{
		return PhotonPoller.JoinSession(session, protocolToken, result);
	}

	internal override bool HandleConnectToRandomSession(UdpSessionFilter sessionFilter, object protocolToken, Action<bool, UdpSessionError> result = null)
	{
		return PhotonPoller.JoinRandomSession(sessionFilter, protocolToken, result);
	}

	internal override UdpSession BuildSession(string id)
	{
		return PhotonSession.Build(id);
	}

	internal override List<UdpEndPoint> ResolveHostAddresses(int port = 0, bool ipv6 = false)
	{
		return dotNetPlatform.ResolveHostAddresses(port, ipv6);
	}

	internal override UdpPlatformSocket CreateBroadcastSocket(UdpEndPoint endpoint, bool bind = true)
	{
		return dotNetPlatform.CreateBroadcastSocket(endpoint, bind);
	}

	internal override UdpIPv4Address GetBroadcastAddress()
	{
		return dotNetPlatform.GetBroadcastAddress();
	}

	internal override List<UdpPlatformInterface> GetNetworkInterfaces()
	{
		return dotNetPlatform.GetNetworkInterfaces();
	}

	internal override UdpPlatformSocket CreateSocket(bool ipv6)
	{
		PhotonSocket photonSocket = new PhotonSocket(this, dotNetPlatform, ipv6);
		stunManager.Reset();
		stunManager.SetupSocket(photonSocket);
		if (PhotonConfig.ForceExternalEndPoint != null)
		{
			stunManager.SetExternalInfo(PhotonConfig.ForceExternalEndPoint.ConvertToUdpEndPoint());
		}
		return photonSocket;
	}

	internal override void OnStartBegin()
	{
		PhotonPoller.CreatePoller(this, PhotonConfig, force: true);
		if (PhotonConfig.UsePunchThrough)
		{
			PhotonPoller.Instance.OnUpdate -= stunManager.Service;
			PhotonPoller.Instance.OnUpdate += stunManager.Service;
		}
	}

	internal override Task OnShutdown()
	{
		PhotonPoller.StopPhotonClient();
		return Task.Delay(1000);
	}

	internal override void OnStartupFailed()
	{
		PhotonPoller.StopPhotonClient();
	}

	internal override void OnStartDone(UdpEndPoint localEndPoint, Action<bool, UdpConnectionDisconnectReason> doneCallback)
	{
		stunManager.SetInternalEndPoint(localEndPoint);
		PhotonPoller.StartDone(localEndPoint, doneCallback);
	}

	internal override void OnConnect(UdpConnection connection)
	{
		PhotonPoller.Connected(connection);
	}

	internal override void Configure(UdpConfig config)
	{
		base.Configure(config);
		dotNetPlatform.Configure(config);
		PhotonConfig.BackgroundConnectionTimeout = (int)config.ConnectionTimeout;
		PhotonConfig.CurrentPlatform = config.CurrentPlatform;
		PhotonConfig.ConnectionRequestAttempts = config.ConnectRequestAttempts;
		PhotonConfig.ConnectionLANRequestAttempts = config.ConnectRequestLANAttempts;
	}
}
