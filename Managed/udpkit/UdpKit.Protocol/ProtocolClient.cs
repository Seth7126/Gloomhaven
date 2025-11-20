#define DEBUG
using System;
using System.Collections.Generic;
using UdpKit.Platform;

namespace UdpKit.Protocol;

internal class ProtocolClient : Context
{
	private struct AckCallback
	{
		public UdpEndPoint Filter;

		public Action<Query> Action;
	}

	private struct MsgHandler
	{
		public UdpEndPoint Filter;

		public Action<Message> Action;
	}

	public uint LastSend;

	public byte[] Buffer = new byte[1024];

	public UdpPlatformSocket Socket;

	private List<Query> Queries;

	private Dictionary<Type, MsgHandler> Handlers;

	private Dictionary<Type, AckCallback> Callbacks;

	public UdpPlatform Platform => Socket.Platform;

	public ProtocolClient(UdpPlatformSocket socket, Guid gameId, Guid peerId)
		: base(gameId, peerId)
	{
		Socket = socket;
		Queries = new List<Query>();
		Callbacks = new Dictionary<Type, AckCallback>();
		Handlers = new Dictionary<Type, MsgHandler>();
	}

	public void SetHandler<T>(Action<T> handler) where T : Message
	{
		SetHandler(handler, new UdpEndPoint(new UdpIPv4Address(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), ushort.MaxValue));
	}

	public void SetHandler<T>(Action<T> handler, UdpEndPoint filter) where T : Message
	{
		Handlers[typeof(T)] = new MsgHandler
		{
			Filter = filter,
			Action = delegate(Message m)
			{
				handler((T)m);
			}
		};
	}

	public void SetCallback<T>(Action<T> callback) where T : Query
	{
		SetCallback(callback, new UdpEndPoint(new UdpIPv4Address(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), ushort.MaxValue));
	}

	public void SetCallback<T>(Action<T> callback, UdpEndPoint filter) where T : Query
	{
		Callbacks[typeof(T)] = new AckCallback
		{
			Filter = filter,
			Action = delegate(Query q)
			{
				callback((T)q);
			}
		};
	}

	public void Update(uint now)
	{
		for (int i = 0; i < Queries.Count; i++)
		{
			Query query = Queries[i];
			if (query.Timeout >= now)
			{
				continue;
			}
			AckCallback callback;
			if (query.Resend && query.Attempts < 10)
			{
				if (GetValidCallback(query, out callback))
				{
					Send(query, query.Target);
				}
				else
				{
					RemoveQueryAt(ref i);
				}
				continue;
			}
			try
			{
				if (GetValidCallback(query, out callback))
				{
					callback.Action(query);
				}
			}
			finally
			{
				RemoveQueryAt(ref i);
			}
		}
	}

	private void RemoveQueryAt(ref int i)
	{
		Queries.RemoveAt(i);
		i--;
	}

	private bool GetValidCallback(Query qry, out AckCallback callback)
	{
		if (Callbacks.TryGetValue(qry.GetType(), out callback) && (callback.Filter & qry.Target) == qry.Target)
		{
			return true;
		}
		return false;
	}

	private bool GetValidHandler(Message msg, out MsgHandler handler)
	{
		if (Handlers.TryGetValue(msg.GetType(), out handler) && (handler.Filter & msg.Sender) == msg.Sender)
		{
			return true;
		}
		return false;
	}

	public void Recv(UdpEndPoint endpoint, byte[] buffer, int offset)
	{
		try
		{
			Message message = ParseMessage(buffer, ref offset);
			message.Sender = endpoint;
			UdpLog.Info("Received {0} From {1}", message.GetType().Name, endpoint);
			MsgHandler handler;
			if (message is Result)
			{
				QueryResult(message as Result);
			}
			else if (GetValidHandler(message, out handler))
			{
				handler.Action(message);
			}
		}
		catch (UdpException)
		{
		}
	}

	public void Send(Message msg, UdpEndPoint endpoint)
	{
		if (msg is Query)
		{
			Query query = (Query)msg;
			query.SendTime = Platform.GetPrecisionTime();
			query.Target = endpoint;
			query.Attempts++;
			query.Timeout = query.SendTime + query.BaseTimeout * query.Attempts;
			if (query.Attempts == 1)
			{
				if (query.IsUnique)
				{
					QueryFilter(query.GetType(), endpoint);
				}
				Queries.Add(query);
			}
		}
		UdpLog.Info("Sending To {0}", endpoint);
		Socket.SendTo(Buffer, WriteMessage(msg, Buffer), endpoint);
		LastSend = Platform.GetPrecisionTime();
	}

	private void QueryResult(Result result)
	{
		UdpAssert.Assert(result.Query != Guid.Empty);
		for (int i = 0; i < Queries.Count; i++)
		{
			Query query = Queries[i];
			if (query.MessageId == result.Query)
			{
				RemoveQueryAt(ref i);
				query.Result = result;
				if (GetValidCallback(query, out var callback))
				{
					callback.Action(query);
				}
				break;
			}
		}
	}

	private void QueryFilter(Type t, UdpEndPoint endpoint)
	{
		for (int i = 0; i < Queries.Count; i++)
		{
			if (Queries[i].GetType() == t && Queries[i].Target == endpoint)
			{
				RemoveQueryAt(ref i);
			}
		}
	}
}
