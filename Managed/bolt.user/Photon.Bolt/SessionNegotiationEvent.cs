using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class SessionNegotiationEvent : Event
{
	public string Data
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

	public int MessageType
	{
		get
		{
			return Storage.Values[OffsetStorage + 1].Int0;
		}
		set
		{
			int @int = Storage.Values[OffsetStorage + 1].Int0;
			Storage.Values[OffsetStorage + 1].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int Platform
	{
		get
		{
			return Storage.Values[OffsetStorage + 2].Int0;
		}
		set
		{
			int @int = Storage.Values[OffsetStorage + 2].Int0;
			Storage.Values[OffsetStorage + 2].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public SessionNegotiationEvent()
		: base(SessionNegotiationEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[SessionNegotiationEvent Data={Data} MessageType={MessageType} Platform={Platform}]";
	}

	private static SessionNegotiationEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)SessionNegotiationEvent_Meta.Instance).TypeKey) is SessionNegotiationEvent sessionNegotiationEvent))
		{
			return null;
		}
		sessionNegotiationEvent.Targets = targets;
		sessionNegotiationEvent.TargetConnection = connection;
		sessionNegotiationEvent.Reliability = reliability;
		return sessionNegotiationEvent;
	}

	public static SessionNegotiationEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static SessionNegotiationEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static SessionNegotiationEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static SessionNegotiationEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static SessionNegotiationEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static SessionNegotiationEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, string Data, int MessageType, int Platform)
	{
		SessionNegotiationEvent sessionNegotiationEvent = Create(targets, connection, reliability);
		if (sessionNegotiationEvent == null)
		{
			return false;
		}
		sessionNegotiationEvent.Data = Data;
		sessionNegotiationEvent.MessageType = MessageType;
		sessionNegotiationEvent.Platform = Platform;
		sessionNegotiationEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, string Data, int MessageType, int Platform)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, Data, MessageType, Platform);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, string Data, int MessageType, int Platform)
	{
		return Post((byte)targets, null, reliability, Data, MessageType, Platform);
	}

	public static bool Post(BoltConnection connection, string Data, int MessageType, int Platform)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, Data, MessageType, Platform);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, string Data, int MessageType, int Platform)
	{
		return Post(10, connection, reliability, Data, MessageType, Platform);
	}

	public static bool Post(string Data, int MessageType, int Platform)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, Data, MessageType, Platform);
	}

	public static bool Post(ReliabilityModes reliability, string Data, int MessageType, int Platform)
	{
		return Post(2, null, reliability, Data, MessageType, Platform);
	}
}
