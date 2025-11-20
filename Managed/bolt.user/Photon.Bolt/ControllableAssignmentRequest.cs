using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class ControllableAssignmentRequest : Event
{
	public ControllableAssignmentRequest()
		: base(ControllableAssignmentRequest_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[ControllableAssignmentRequest]";
	}

	private static ControllableAssignmentRequest Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isClient)
		{
			throw new BoltException("You are not a client, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)ControllableAssignmentRequest_Meta.Instance).TypeKey) is ControllableAssignmentRequest controllableAssignmentRequest))
		{
			return null;
		}
		controllableAssignmentRequest.Targets = targets;
		controllableAssignmentRequest.TargetConnection = connection;
		controllableAssignmentRequest.Reliability = reliability;
		return controllableAssignmentRequest;
	}

	public static ControllableAssignmentRequest Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableAssignmentRequest Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static ControllableAssignmentRequest Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableAssignmentRequest Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static ControllableAssignmentRequest Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableAssignmentRequest Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		ControllableAssignmentRequest controllableAssignmentRequest = Create(targets, connection, reliability);
		if (controllableAssignmentRequest == null)
		{
			return false;
		}
		controllableAssignmentRequest.Send();
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
