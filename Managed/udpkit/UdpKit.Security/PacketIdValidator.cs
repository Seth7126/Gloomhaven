using System;
using System.Collections.Generic;

namespace UdpKit.Security;

public class PacketIdValidator : Singleton<PacketIdValidator>, IPacketIdValidator
{
	private readonly Dictionary<UdpEndPoint, uint> _packetIdsConnectionReceive;

	private readonly Dictionary<UdpEndPoint, uint> _packetIdsConnectionSend;

	private UdpByteConverter _byteConverter;

	private const int IdSize = 4;

	public PacketIdValidator()
	{
		_byteConverter = default(UdpByteConverter);
		_packetIdsConnectionReceive = new Dictionary<UdpEndPoint, uint>();
		_packetIdsConnectionSend = new Dictionary<UdpEndPoint, uint>();
	}

	public int PrefixPacketId(UdpEndPoint endPoint, byte[] buffer, int length)
	{
		if (!_packetIdsConnectionSend.TryGetValue(endPoint, out var value))
		{
			value = 0u;
		}
		_packetIdsConnectionSend[endPoint] = value + 1;
		PrefixIdToBuffer(value, buffer, length);
		return length + 4;
	}

	public int ValidatePacketId(UdpEndPoint endPoint, byte[] buffer, int length)
	{
		_byteConverter.Byte0 = buffer[0];
		_byteConverter.Byte1 = buffer[1];
		_byteConverter.Byte2 = buffer[2];
		_byteConverter.Byte3 = buffer[3];
		uint unsigned = _byteConverter.Unsigned32;
		if (_packetIdsConnectionReceive.TryGetValue(endPoint, out var value) && unsigned <= value)
		{
			return -1;
		}
		_packetIdsConnectionReceive[endPoint] = unsigned;
		Buffer.BlockCopy(buffer, 4, buffer, 0, length - 4);
		return length - 4;
	}

	private void PrefixIdToBuffer(uint id, byte[] buffer, int length)
	{
		Buffer.BlockCopy(buffer, 0, buffer, 4, length);
		_byteConverter.Unsigned32 = id;
		buffer[0] = _byteConverter.Byte0;
		buffer[1] = _byteConverter.Byte1;
		buffer[2] = _byteConverter.Byte2;
		buffer[3] = _byteConverter.Byte3;
	}

	public void Clear()
	{
		_packetIdsConnectionReceive.Clear();
		_packetIdsConnectionSend.Clear();
	}

	public void RemoveEndPointReference(UdpEndPoint endPoint)
	{
		_packetIdsConnectionReceive.Remove(endPoint);
		_packetIdsConnectionSend.Remove(endPoint);
	}
}
