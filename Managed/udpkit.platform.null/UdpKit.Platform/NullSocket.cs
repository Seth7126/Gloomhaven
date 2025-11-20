using System.Threading;

namespace UdpKit.Platform;

internal class NullSocket : UdpPlatformSocket
{
	private readonly NullPlatform platform;

	internal override bool Broadcast { get; set; }

	internal override UdpEndPoint EndPoint => UdpEndPoint.Any;

	internal override string Error => "";

	internal override bool IsBound => true;

	internal override UdpPlatform Platform => platform;

	public NullSocket(NullPlatform p)
	{
		platform = p;
	}

	internal override void Bind(UdpEndPoint ep)
	{
	}

	internal override void Close()
	{
	}

	internal override int RecvFrom(byte[] buffer, int bufferSize, ref UdpEndPoint remoteEndpoint)
	{
		return 0;
	}

	internal override bool RecvPoll(int timeout)
	{
		if (timeout > 0)
		{
			Thread.Sleep(1);
		}
		return false;
	}

	internal override bool RecvPoll()
	{
		return RecvPoll(0);
	}

	internal override int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint)
	{
		return bytesToSend;
	}
}
