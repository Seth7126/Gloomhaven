using System.Collections.Generic;
using Photon.Bolt;
using UdpKit;

namespace Assets.Script.Networking.Tokens;

public class BlockedUsersDataToken : IProtocolToken
{
	public string UserId { get; set; }

	public List<string> BlockedUsersIds { get; set; }

	public bool ResponseRequired { get; set; }

	public BlockedUsersDataToken()
	{
		BlockedUsersIds = new List<string>();
		UserId = string.Empty;
	}

	public void Read(UdpPacket packet)
	{
		UserId = packet.ReadString();
		int num = packet.ReadInt();
		for (int i = 0; i < num; i++)
		{
			string item = packet.ReadString();
			BlockedUsersIds.Add(item);
		}
		ResponseRequired = packet.ReadBool();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(UserId);
		packet.WriteInt(BlockedUsersIds.Count);
		foreach (string blockedUsersId in BlockedUsersIds)
		{
			packet.WriteString(blockedUsersId);
		}
		packet.WriteBool(ResponseRequired);
	}
}
