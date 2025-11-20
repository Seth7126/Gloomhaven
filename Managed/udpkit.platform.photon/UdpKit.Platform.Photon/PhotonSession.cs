using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UdpKit.Utils;

namespace UdpKit.Platform.Photon;

public class PhotonSession : UdpSessionImpl
{
	internal static string KEY_UDP_SESSION_ID = "UdpSessionId";

	internal static string KEY_USER_TOKEN = "UserToken";

	internal static string KEY_PUNCH_ENABLED = "PunchEnabled";

	internal static string KEY_LAN_ENDPOINT = "LANEndPoint";

	internal static string KEY_WAN_ENDPOINT = "WANEndPoint";

	public Hashtable Properties { get; internal set; }

	public bool IsOpen { get; internal set; }

	public bool IsVisible { get; internal set; }

	public override bool IsDedicatedServer => false;

	public override UdpSessionSource Source => UdpSessionSource.Photon;

	public bool IsPunchEnabled { get; internal set; }

	internal PhotonSession()
	{
		Source = UdpSessionSource.Photon;
	}

	public new static PhotonSession Build(string roomName)
	{
		return new PhotonSession
		{
			HostName = roomName,
			IsOpen = true,
			IsVisible = true
		};
	}

	internal static PhotonSession Convert(RoomInfo roomInfo)
	{
		bool flag = roomInfo.CustomProperties.ContainsKey(KEY_LAN_ENDPOINT) && roomInfo.CustomProperties.ContainsKey(KEY_WAN_ENDPOINT);
		UdpEndPoint lanEndPoint = UdpEndPoint.Any;
		UdpEndPoint wanEndPoint = UdpEndPoint.Any;
		if (flag)
		{
			lanEndPoint = ((roomInfo.CustomProperties[KEY_LAN_ENDPOINT] is byte[] encodedEndPoint) ? encodedEndPoint.DeserializeEndPoint() : UdpEndPoint.Any);
			wanEndPoint = ((roomInfo.CustomProperties[KEY_WAN_ENDPOINT] is byte[] encodedEndPoint2) ? encodedEndPoint2.DeserializeEndPoint() : UdpEndPoint.Any);
		}
		PhotonSession photonSession = new PhotonSession();
		photonSession.HostName = roomInfo.Name;
		photonSession.Id = new Guid((roomInfo.CustomProperties[KEY_UDP_SESSION_ID] as string) ?? "");
		photonSession.HostData = roomInfo.CustomProperties[KEY_USER_TOKEN] as byte[];
		photonSession.ConnectionsCurrent = roomInfo.PlayerCount;
		photonSession.ConnectionsMax = roomInfo.MaxPlayers;
		photonSession.Properties = roomInfo.CustomProperties;
		photonSession.IsOpen = roomInfo.IsOpen;
		photonSession.IsVisible = roomInfo.IsVisible;
		photonSession.IsPunchEnabled = flag;
		photonSession.LanEndPoint = lanEndPoint;
		photonSession.WanEndPoint = wanEndPoint;
		return photonSession;
	}

	internal static PhotonSession Convert(Room roomInfo)
	{
		return Convert((RoomInfo)roomInfo);
	}
}
