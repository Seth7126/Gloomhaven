using Photon.Bolt.Internal;
using UdpKit;

namespace Photon.Bolt;

public abstract class PooledProtocolToken : PooledProtocolTokenBase
{
	public abstract override void Read(UdpPacket packet);

	public abstract override void Write(UdpPacket packet);

	public abstract override void Reset();
}
