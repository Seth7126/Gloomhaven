using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class SavePointReachedEvent : Event
{
	public SavePointReachedEvent()
		: base(SavePointReachedEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[SavePointReachedEvent]";
	}

	private static SavePointReachedEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isServer)
		{
			throw new BoltException("You are not the server, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)SavePointReachedEvent_Meta.Instance).TypeKey) is SavePointReachedEvent savePointReachedEvent))
		{
			return null;
		}
		savePointReachedEvent.Targets = targets;
		savePointReachedEvent.TargetConnection = connection;
		savePointReachedEvent.Reliability = reliability;
		return savePointReachedEvent;
	}

	public static SavePointReachedEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static SavePointReachedEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static SavePointReachedEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static SavePointReachedEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static SavePointReachedEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static SavePointReachedEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		SavePointReachedEvent savePointReachedEvent = Create(targets, connection, reliability);
		if (savePointReachedEvent == null)
		{
			return false;
		}
		savePointReachedEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Post((byte)targets, null, reliability);
	}

	public static bool Post(BoltConnection connection)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability)
	{
		return Post(10, connection, reliability);
	}

	public static bool Post()
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static bool Post(ReliabilityModes reliability)
	{
		return Post(2, null, reliability);
	}
}
