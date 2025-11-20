using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class PlayerEntityRequest : Event
{
	public PlayerEntityRequest()
		: base(PlayerEntityRequest_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[PlayerEntityRequest]";
	}

	private static PlayerEntityRequest Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isClient)
		{
			throw new BoltException("You are not a client, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)PlayerEntityRequest_Meta.Instance).TypeKey) is PlayerEntityRequest playerEntityRequest))
		{
			return null;
		}
		playerEntityRequest.Targets = targets;
		playerEntityRequest.TargetConnection = connection;
		playerEntityRequest.Reliability = reliability;
		return playerEntityRequest;
	}

	public static PlayerEntityRequest Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static PlayerEntityRequest Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static PlayerEntityRequest Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static PlayerEntityRequest Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static PlayerEntityRequest Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static PlayerEntityRequest Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		PlayerEntityRequest playerEntityRequest = Create(targets, connection, reliability);
		if (playerEntityRequest == null)
		{
			return false;
		}
		playerEntityRequest.Send();
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
