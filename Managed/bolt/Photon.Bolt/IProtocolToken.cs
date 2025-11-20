using UdpKit;

namespace Photon.Bolt;

public interface IProtocolToken
{
	void Read(UdpPacket packet);

	void Write(UdpPacket packet);
}
