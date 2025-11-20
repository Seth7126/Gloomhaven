using System;
using Photon.Bolt.Internal;
using UdpKit;

namespace Photon.Bolt;

public static class ProtocolTokenUtils
{
	private static byte[] tempBytes;

	private static UdpPacket tempPacket;

	public static byte[] ToByteArray(this IProtocolToken token)
	{
		if (token == null)
		{
			return null;
		}
		if (tempBytes == null || tempBytes.Length != BoltCore._config.ProtocolTokenMaxSize)
		{
			tempBytes = new byte[BoltCore._config.ProtocolTokenMaxSize];
		}
		if (tempPacket == null)
		{
			tempPacket = new UdpPacket(null);
		}
		Array.Clear(tempBytes, 0, tempBytes.Length);
		tempPacket.Ptr = 0;
		tempPacket.Data = tempBytes;
		tempPacket.Size = tempBytes.Length << 3;
		tempPacket.WriteByte(Factory.GetTokenId(token));
		token.Write(tempPacket);
		return tempPacket.DuplicateData();
	}

	public static IProtocolToken ToToken(this byte[] bytes)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return null;
		}
		if (tempPacket == null)
		{
			tempPacket = new UdpPacket(null);
		}
		tempPacket.Ptr = 8;
		tempPacket.Data = bytes;
		tempPacket.Size = bytes.Length << 3;
		IProtocolToken protocolToken = Factory.NewToken(bytes[0]);
		protocolToken.Read(tempPacket);
		return protocolToken;
	}

	public static void WriteToken(this UdpPacket packet, IProtocolToken token)
	{
		if (packet.WriteBool(token != null))
		{
			packet.WriteByte(Factory.GetTokenId(token));
			token.Write(packet);
		}
	}

	public static IProtocolToken ReadToken(this UdpPacket packet)
	{
		IProtocolToken protocolToken = null;
		if (packet.ReadBool())
		{
			protocolToken = Factory.NewToken(packet.ReadByte());
			protocolToken.Read(packet);
		}
		return protocolToken;
	}

	public static void SerializeToken(this UdpPacket packet, ref IProtocolToken token)
	{
		if (packet.Write)
		{
			packet.WriteToken(token);
		}
		else
		{
			token = packet.ReadToken();
		}
	}

	internal static void Release(this IProtocolToken token)
	{
		(token as PooledProtocolTokenBase)?.Dispose();
	}

	public static T GetToken<T>() where T : PooledProtocolTokenBase
	{
		return Factory.NewToken(Factory.GetTokenId<T>()) as T;
	}
}
