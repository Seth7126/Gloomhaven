#define DEBUG
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

public class ControllableReleaseEvent : Event
{
	public int PlayerID
	{
		get
		{
			return Storage.Values[OffsetStorage].Int0;
		}
		set
		{
			if (value < 0 || value > 255)
			{
				BoltLog.Warn("Property 'PlayerID' is being set to a value larger than the compression settings, it will be clamped to [+0, +255]");
			}
			value = Mathf.Clamp(value, 0, 255);
			int @int = Storage.Values[OffsetStorage].Int0;
			Storage.Values[OffsetStorage].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int ControllableID
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

	public ControllableReleaseEvent()
		: base(ControllableReleaseEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[ControllableReleaseEvent PlayerID={PlayerID} ControllableID={ControllableID}]";
	}

	private static ControllableReleaseEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isServer)
		{
			throw new BoltException("You are not the server, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)ControllableReleaseEvent_Meta.Instance).TypeKey) is ControllableReleaseEvent controllableReleaseEvent))
		{
			return null;
		}
		controllableReleaseEvent.Targets = targets;
		controllableReleaseEvent.TargetConnection = connection;
		controllableReleaseEvent.Reliability = reliability;
		return controllableReleaseEvent;
	}

	public static ControllableReleaseEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableReleaseEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static ControllableReleaseEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableReleaseEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static ControllableReleaseEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableReleaseEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int PlayerID, int ControllableID)
	{
		ControllableReleaseEvent controllableReleaseEvent = Create(targets, connection, reliability);
		if (controllableReleaseEvent == null)
		{
			return false;
		}
		controllableReleaseEvent.PlayerID = PlayerID;
		controllableReleaseEvent.ControllableID = ControllableID;
		controllableReleaseEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int PlayerID, int ControllableID)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, PlayerID, ControllableID);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int PlayerID, int ControllableID)
	{
		return Post((byte)targets, null, reliability, PlayerID, ControllableID);
	}

	public static bool Post(BoltConnection connection, int PlayerID, int ControllableID)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, PlayerID, ControllableID);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int PlayerID, int ControllableID)
	{
		return Post(10, connection, reliability, PlayerID, ControllableID);
	}

	public static bool Post(int PlayerID, int ControllableID)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, PlayerID, ControllableID);
	}

	public static bool Post(ReliabilityModes reliability, int PlayerID, int ControllableID)
	{
		return Post(2, null, reliability, PlayerID, ControllableID);
	}
}
