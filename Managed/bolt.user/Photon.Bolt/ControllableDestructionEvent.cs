using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class ControllableDestructionEvent : Event
{
	public int ControllableID
	{
		get
		{
			return Storage.Values[OffsetStorage].Int0;
		}
		set
		{
			int @int = Storage.Values[OffsetStorage].Int0;
			Storage.Values[OffsetStorage].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public ControllableDestructionEvent()
		: base(ControllableDestructionEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[ControllableDestructionEvent ControllableID={ControllableID}]";
	}

	private static ControllableDestructionEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isServer)
		{
			throw new BoltException("You are not the server, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)ControllableDestructionEvent_Meta.Instance).TypeKey) is ControllableDestructionEvent controllableDestructionEvent))
		{
			return null;
		}
		controllableDestructionEvent.Targets = targets;
		controllableDestructionEvent.TargetConnection = connection;
		controllableDestructionEvent.Reliability = reliability;
		return controllableDestructionEvent;
	}

	public static ControllableDestructionEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableDestructionEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static ControllableDestructionEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableDestructionEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static ControllableDestructionEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableDestructionEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int ControllableID)
	{
		ControllableDestructionEvent controllableDestructionEvent = Create(targets, connection, reliability);
		if (controllableDestructionEvent == null)
		{
			return false;
		}
		controllableDestructionEvent.ControllableID = ControllableID;
		controllableDestructionEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int ControllableID)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, ControllableID);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int ControllableID)
	{
		return Post((byte)targets, null, reliability, ControllableID);
	}

	public static bool Post(BoltConnection connection, int ControllableID)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, ControllableID);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int ControllableID)
	{
		return Post(10, connection, reliability, ControllableID);
	}

	public static bool Post(int ControllableID)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, ControllableID);
	}

	public static bool Post(ReliabilityModes reliability, int ControllableID)
	{
		return Post(2, null, reliability, ControllableID);
	}
}
