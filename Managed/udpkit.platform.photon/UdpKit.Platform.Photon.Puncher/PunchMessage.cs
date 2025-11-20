namespace UdpKit.Platform.Photon.Puncher;

internal class PunchMessage
{
	public UdpEndPoint target;

	internal StunMsgHeader msg;

	internal byte[] buffer;

	internal int bufferSize;

	public PunchMessage(UdpEndPoint target, byte[] buffer, int bufferSize)
	{
		this.target = target;
		this.buffer = buffer;
		this.bufferSize = bufferSize;
		msg = new StunMsgHeader().Decode(buffer);
	}

	public int GetRemoteId()
	{
		return 0;
	}

	public bool IsPunch()
	{
		return msg.Type == 10 || msg.Type == 266;
	}

	public bool IsPing()
	{
		return msg.Type == 10;
	}

	public override string ToString()
	{
		return $"[PunchMessage] {target} {IsPunch()}";
	}
}
