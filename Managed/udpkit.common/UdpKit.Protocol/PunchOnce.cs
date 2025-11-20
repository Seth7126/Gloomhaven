using System;

namespace UdpKit.Protocol;

internal class PunchOnce : Message
{
	public Guid RemotePeerId;

	public UdpEndPoint RemoteEndPoint;

	protected override void OnSerialize()
	{
		base.OnSerialize();
		Serialize(ref RemotePeerId);
		Serialize(ref RemoteEndPoint);
	}
}
