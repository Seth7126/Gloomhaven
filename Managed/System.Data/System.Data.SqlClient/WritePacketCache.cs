using System.Collections.Generic;

namespace System.Data.SqlClient;

internal sealed class WritePacketCache : IDisposable
{
	private bool _disposed;

	private Stack<SNIPacket> _packets;

	public WritePacketCache()
	{
		_disposed = false;
		_packets = new Stack<SNIPacket>();
	}

	public SNIPacket Take(SNIHandle sniHandle)
	{
		SNIPacket sNIPacket;
		if (_packets.Count > 0)
		{
			sNIPacket = _packets.Pop();
			SNINativeMethodWrapper.SNIPacketReset(sniHandle, SNINativeMethodWrapper.IOType.WRITE, sNIPacket, SNINativeMethodWrapper.ConsumerNumber.SNI_Consumer_SNI);
		}
		else
		{
			sNIPacket = new SNIPacket(sniHandle);
		}
		return sNIPacket;
	}

	public void Add(SNIPacket packet)
	{
		if (!_disposed)
		{
			_packets.Push(packet);
		}
		else
		{
			packet.Dispose();
		}
	}

	public void Clear()
	{
		while (_packets.Count > 0)
		{
			_packets.Pop().Dispose();
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Clear();
		}
	}
}
