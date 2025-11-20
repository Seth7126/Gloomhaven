using System;
using System.Text;
using Photon.Bolt;
using UdpKit;

namespace FFSNet;

public sealed class UserToken : IProtocolToken
{
	public const string c_DevVersion = "DevVersion";

	public int BuildType { get; private set; }

	public string ServerPassword { get; private set; }

	public string Username { get; private set; }

	public string PlatformPlayerID { get; private set; }

	public string GameVersion { get; private set; }

	public string CurrentMod { get; private set; }

	public string PlatformName { get; private set; }

	public bool CrossplayEnabled { get; private set; }

	public string PlatformNetworkAccountPlayerID { get; private set; }

	public byte[] RecentPlayerKey { get; private set; }

	public UserToken(int buildType, string serverPassword, string username, string platformPlayerID, string gameVersion, string currentMod, string platformName, bool crossplayEnabled, string platformNetworkAccountPlayerID, byte[] recentPlayerKey)
	{
		PlatformName = platformName;
		BuildType = buildType;
		ServerPassword = serverPassword;
		PlatformPlayerID = platformPlayerID;
		Username = username;
		GameVersion = gameVersion;
		CurrentMod = currentMod;
		CrossplayEnabled = crossplayEnabled;
		PlatformNetworkAccountPlayerID = platformNetworkAccountPlayerID;
		RecentPlayerKey = recentPlayerKey;
	}

	public void MaskBadWordsInUsername(Action onCallback)
	{
		Username.GetCensoredStringAsync(delegate(string censoredUsername)
		{
			Username = censoredUsername;
			onCallback?.Invoke();
		});
	}

	public UserToken()
	{
		BuildType = 0;
		ServerPassword = string.Empty;
		Username = string.Empty;
		PlatformPlayerID = string.Empty;
		GameVersion = string.Empty;
		CurrentMod = string.Empty;
		PlatformName = string.Empty;
		CrossplayEnabled = false;
		PlatformNetworkAccountPlayerID = string.Empty;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(BuildType);
		packet.WriteString(ServerPassword, Encoding.ASCII);
		packet.WriteString(Username, Encoding.UTF8);
		packet.WriteString(PlatformPlayerID, Encoding.UTF8);
		packet.WriteString(GameVersion, Encoding.ASCII);
		packet.WriteString(CurrentMod, Encoding.ASCII);
		packet.WriteString(PlatformName, Encoding.UTF8);
		packet.WriteBool(CrossplayEnabled);
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
		BuildType = packet.ReadInt();
		ServerPassword = packet.ReadString(Encoding.ASCII);
		Username = packet.ReadString(Encoding.UTF8);
		PlatformPlayerID = packet.ReadString(Encoding.UTF8);
		GameVersion = packet.ReadString(Encoding.ASCII);
		CurrentMod = packet.ReadString(Encoding.ASCII);
		PlatformName = packet.ReadString(Encoding.UTF8);
		CrossplayEnabled = packet.ReadBool();
		PlatformNetworkAccountPlayerID = packet.ReadString(Encoding.UTF8);
		if (packet.ReadBool())
		{
			RecentPlayerKey = new byte[64];
			packet.ReadByteArray(RecentPlayerKey);
		}
	}
}
