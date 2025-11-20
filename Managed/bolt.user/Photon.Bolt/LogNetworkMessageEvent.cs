using System;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class LogNetworkMessageEvent : Event
{
	public string Message
	{
		get
		{
			return Storage.Values[OffsetStorage].String;
		}
		set
		{
			string a = Storage.Values[OffsetStorage].String;
			Storage.Values[OffsetStorage].String = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public LogNetworkMessageEvent()
		: base(LogNetworkMessageEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[LogNetworkMessageEvent Message={Message}]";
	}

	public static LogNetworkMessageEvent Create(BoltEntity entity, EntityTargets targets)
	{
		if (!entity)
		{
			throw new ArgumentNullException("entity");
		}
		if (!entity.IsAttached)
		{
			throw new BoltException("You can not raise events on entities which are not attached");
		}
		if (!(Factory.NewEvent(((IFactory)LogNetworkMessageEvent_Meta.Instance).TypeKey) is LogNetworkMessageEvent logNetworkMessageEvent))
		{
			return null;
		}
		logNetworkMessageEvent.Targets = (int)targets;
		logNetworkMessageEvent.TargetEntity = entity.Entity;
		logNetworkMessageEvent.Reliability = ReliabilityModes.Unreliable;
		return logNetworkMessageEvent;
	}

	public static LogNetworkMessageEvent Create(BoltEntity entity)
	{
		return Create(entity, EntityTargets.Everyone);
	}

	private static LogNetworkMessageEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)LogNetworkMessageEvent_Meta.Instance).TypeKey) is LogNetworkMessageEvent logNetworkMessageEvent))
		{
			return null;
		}
		logNetworkMessageEvent.Targets = targets;
		logNetworkMessageEvent.TargetConnection = connection;
		logNetworkMessageEvent.Reliability = reliability;
		return logNetworkMessageEvent;
	}

	public static LogNetworkMessageEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static LogNetworkMessageEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static LogNetworkMessageEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static LogNetworkMessageEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static LogNetworkMessageEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static LogNetworkMessageEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	public static bool Post(BoltEntity entity, EntityTargets targets, string Message)
	{
		LogNetworkMessageEvent logNetworkMessageEvent = Create(entity, targets);
		if (logNetworkMessageEvent == null)
		{
			return false;
		}
		logNetworkMessageEvent.Message = Message;
		logNetworkMessageEvent.Send();
		return true;
	}

	public static bool Post(BoltEntity entity, string Message)
	{
		return Post(entity, EntityTargets.Everyone, Message);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, string Message)
	{
		LogNetworkMessageEvent logNetworkMessageEvent = Create(targets, connection, reliability);
		if (logNetworkMessageEvent == null)
		{
			return false;
		}
		logNetworkMessageEvent.Message = Message;
		logNetworkMessageEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, string Message)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, Message);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, string Message)
	{
		return Post((byte)targets, null, reliability, Message);
	}

	public static bool Post(BoltConnection connection, string Message)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, Message);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, string Message)
	{
		return Post(10, connection, reliability, Message);
	}

	public static bool Post(string Message)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, Message);
	}

	public static bool Post(ReliabilityModes reliability, string Message)
	{
		return Post(2, null, reliability, Message);
	}
}
