using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class PlayerEntityInitializedEvent : Event
{
	public PlayerEntityInitializedEvent()
		: base(PlayerEntityInitializedEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[PlayerEntityInitializedEvent]";
	}

	private static PlayerEntityInitializedEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isClient)
		{
			throw new BoltException("You are not a client, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)PlayerEntityInitializedEvent_Meta.Instance).TypeKey) is PlayerEntityInitializedEvent playerEntityInitializedEvent))
		{
			return null;
		}
		playerEntityInitializedEvent.Targets = targets;
		playerEntityInitializedEvent.TargetConnection = connection;
		playerEntityInitializedEvent.Reliability = reliability;
		return playerEntityInitializedEvent;
	}

	public static PlayerEntityInitializedEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static PlayerEntityInitializedEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static PlayerEntityInitializedEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static PlayerEntityInitializedEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static PlayerEntityInitializedEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static PlayerEntityInitializedEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		PlayerEntityInitializedEvent playerEntityInitializedEvent = Create(targets, connection, reliability);
		if (playerEntityInitializedEvent == null)
		{
			return false;
		}
		playerEntityInitializedEvent.Send();
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
