using System.Text;
using Photon.Bolt;
using UdpKit;

namespace FFSNet;

public sealed class PlayerToken : IProtocolToken
{
	public int PlayerID { get; set; }

	public string PlatformPlayerID { get; set; }

	public string PlatformNetworkAccountPlayerID { get; set; }

	public string Username { get; set; }

	public string PlatformName { get; set; }

	public byte[] RecentPlayerKey { get; set; }

	public PlayerToken(int playerID, string platformPlayerID, string userName, string platformName, string platformNetworkAccountPlayerID, byte[] recentPlayerKey)
	{
		PlayerID = playerID;
		PlatformPlayerID = platformPlayerID;
		Username = userName;
		PlatformName = platformName;
		PlatformNetworkAccountPlayerID = platformNetworkAccountPlayerID;
		RecentPlayerKey = recentPlayerKey;
	}

	public PlayerToken()
	{
		PlayerID = 0;
		Username = "NONE";
		PlatformPlayerID = PlatformLayer.UserData.GetDefaultPlatformID();
		PlatformName = PlatformLayer.Instance.PlatformID;
		PlatformNetworkAccountPlayerID = PlatformLayer.UserData.GetDefaultPlatformID();
		RecentPlayerKey = null;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(PlayerID);
		packet.WriteString(PlatformPlayerID, Encoding.UTF8);
		packet.WriteString(Username, Encoding.UTF8);
		packet.WriteString(PlatformName, Encoding.UTF8);
		packet.WriteString(PlatformNetworkAccountPlayerID, Encoding.UTF8);
		if (RecentPlayerKey != null)
		{
			packet.WriteBool(value: true);
			packet.WriteByteArray(RecentPlayerKey, RecentPlayerKey.Length);
		}
		else
		{
			packet.WriteBool(value: false);
		}
	}

	public void Read(UdpPacket packet)
	{
		PlayerID = packet.ReadInt();
		PlatformPlayerID = packet.ReadString(Encoding.UTF8);
		Username = packet.ReadString(Encoding.UTF8);
		PlatformName = packet.ReadString(Encoding.UTF8);
		PlatformNetworkAccountPlayerID = packet.ReadString(Encoding.UTF8);
		if (packet.ReadBool())
		{
			RecentPlayerKey = new byte[64];
			packet.ReadByteArray(RecentPlayerKey);
		}
	}
}
