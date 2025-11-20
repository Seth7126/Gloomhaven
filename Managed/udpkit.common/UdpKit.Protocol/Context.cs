#define DEBUG
using System;

namespace UdpKit.Protocol;

internal class Context
{
	private readonly Guid peerId;

	private readonly Guid gameId;

	private static readonly Type[] messageTypes;

	public Guid PeerId => peerId;

	public Guid GameId => gameId;

	public Context(Guid game)
		: this(game, Guid.NewGuid())
	{
	}

	public Context(Guid game, Guid peer)
	{
		gameId = game;
		peerId = peer;
	}

	static Context()
	{
		messageTypes = new Type[128];
		RegisterMessageType<Ack>();
		RegisterMessageType<Error>();
		RegisterMessageType<BroadcastSearch>();
		RegisterMessageType<BroadcastSession>();
		RegisterMessageType<PeerConnect>();
		RegisterMessageType<PeerConnectResult>();
		RegisterMessageType<PeerDisconnect>();
		RegisterMessageType<PeerReconnect>();
		RegisterMessageType<PeerKeepAlive>();
		RegisterMessageType<HostInfo>();
		RegisterMessageType<HostRegister>();
		RegisterMessageType<GetHostList>();
		RegisterMessageType<ProbeEndPoint>();
		RegisterMessageType<ProbeEndPointResult>();
		RegisterMessageType<ProbeHairpin>();
		RegisterMessageType<ProbeUnsolicited>();
		RegisterMessageType<ProbeFeatures>();
		RegisterMessageType<Punch>();
		RegisterMessageType<PunchOnce>();
		RegisterMessageType<PunchRequest>();
		RegisterMessageType<DirectConnectionLan>();
		RegisterMessageType<DirectConnectionWan>();
	}

	public T CreateMessage<T>() where T : Message
	{
		for (byte b = 1; b < messageTypes.Length; b++)
		{
			if ((object)messageTypes[b] == typeof(T))
			{
				T val = (T)Activator.CreateInstance(messageTypes[b]);
				val.Context = this;
				val.PeerId = peerId;
				val.GameId = gameId;
				val.Init(b);
				return val;
			}
		}
		throw new NotSupportedException();
	}

	public T CreateMessage<T>(Query query) where T : Result
	{
		T val = CreateMessage<T>();
		val.Query = query.MessageId;
		return val;
	}

	public Message CreateMessage(byte type)
	{
		if (messageTypes[type] != null)
		{
			Message message = (Message)Activator.CreateInstance(messageTypes[type]);
			message.Context = this;
			message.Init(type);
			return message;
		}
		throw new NotSupportedException("message id: " + type);
	}

	public int WriteMessage(Message msg, byte[] buffer)
	{
		UdpLog.Info("Writing: {0}", msg.GetType().Name);
		buffer[0] = byte.MaxValue;
		return msg.Serialize(1, buffer, pack: true);
	}

	public Message ParseMessage(byte[] bytes)
	{
		int offset = 0;
		return ParseMessage(bytes, ref offset);
	}

	public Message ParseMessage(byte[] bytes, ref int offset)
	{
		UdpAssert.Assert(bytes[offset] == byte.MaxValue);
		Message message = CreateMessage(bytes[offset + 1]);
		message.Context = this;
		message.InitBuffer(offset + 1, bytes, pack: false);
		UdpLog.Info("Parsing: {0}", message.GetType().Name);
		offset = message.Serialize();
		return message;
	}

	public static void RegisterMessageType<T>() where T : Message
	{
		for (byte b = 1; b < messageTypes.Length; b++)
		{
			if (messageTypes[b] == null)
			{
				messageTypes[b] = typeof(T);
				return;
			}
		}
		throw new IndexOutOfRangeException();
	}
}
