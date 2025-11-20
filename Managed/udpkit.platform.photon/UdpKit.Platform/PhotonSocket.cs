using System.Reflection;
using UdpKit.Platform.Photon;

namespace UdpKit.Platform;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
internal class PhotonSocket : DotNetSocket
{
	private const int EMPTY_PACKET = -1;

	private PhotonPlatform photonPlatform;

	public PhotonSocket(PhotonPlatform photonPlatform, DotNetPlatform dotNetPlatform, bool ipv6)
		: base(dotNetPlatform, ipv6)
	{
		this.photonPlatform = photonPlatform;
	}

	internal override int RecvFrom(byte[] buffer, int bufferSize, ref UdpEndPoint remoteEndpoint)
	{
		int num = PhotonPoller.RecvFrom(buffer, ref remoteEndpoint);
		if (num == -1)
		{
			num = base.RecvFrom(buffer, bufferSize, ref remoteEndpoint);
			if (num != -1 && photonPlatform.stunManager.RecvStun(buffer))
			{
				return -1;
			}
		}
		return num;
	}

	internal override bool RecvPoll(int timeout)
	{
		return PhotonPoller.RecvPoll() || base.RecvPoll(timeout);
	}

	internal override int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint)
	{
		if (endpoint.Port == 0)
		{
			return PhotonPoller.SendTo(buffer, bytesToSend, endpoint);
		}
		return base.SendTo(buffer, bytesToSend, endpoint);
	}
}
