using System;

namespace UdpKit.Protocol;

internal class ProtocolService
{
	private ProtocolClient client;

	public ProtocolClient Client => client;

	public uint SendTime { get; private set; }

	public ProtocolService(ProtocolClient p)
	{
		client = p;
	}

	public void Send<T>(UdpEndPoint endpoint) where T : Message
	{
		Send(endpoint, client.CreateMessage<T>());
	}

	public void Send(UdpEndPoint endpoint, Message msg)
	{
		SendTime = client.Platform.GetPrecisionTime();
		client.Send(msg, endpoint);
	}

	public void Send<T>(UdpEndPoint endpoint, Action<T> setup) where T : Message
	{
		T val = client.CreateMessage<T>();
		setup(val);
		SendTime = client.Platform.GetPrecisionTime();
		client.Send(val, endpoint);
	}
}
