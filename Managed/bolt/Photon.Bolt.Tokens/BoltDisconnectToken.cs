using UdpKit;

namespace Photon.Bolt.Tokens;

public class BoltDisconnectToken : IProtocolToken
{
	public string reason;

	public UdpConnectionDisconnectReason disconnectReason;

	public BoltDisconnectToken()
	{
	}

	public BoltDisconnectToken(string reason, UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected)
	{
		this.reason = reason;
		this.disconnectReason = disconnectReason;
	}

	public void Read(UdpPacket packet)
	{
		reason = packet.ReadString();
		disconnectReason = (UdpConnectionDisconnectReason)packet.ReadInt();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(reason);
		packet.WriteInt((int)disconnectReason);
	}
}
