#define DEBUG
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

public class ControllableAssignmentEvent : Event
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

	public bool ReleaseFirst
	{
		get
		{
			return Storage.Values[OffsetStorage + 2].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 2].Bool;
			Storage.Values[OffsetStorage + 2].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public ControllableAssignmentEvent()
		: base(ControllableAssignmentEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[ControllableAssignmentEvent PlayerID={PlayerID} ControllableID={ControllableID} ReleaseFirst={ReleaseFirst}]";
	}

	private static ControllableAssignmentEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!BoltCore.isServer)
		{
			throw new BoltException("You are not the server, you can not raise this event");
		}
		if (!(Factory.NewEvent(((IFactory)ControllableAssignmentEvent_Meta.Instance).TypeKey) is ControllableAssignmentEvent controllableAssignmentEvent))
		{
			return null;
		}
		controllableAssignmentEvent.Targets = targets;
		controllableAssignmentEvent.TargetConnection = connection;
		controllableAssignmentEvent.Reliability = reliability;
		return controllableAssignmentEvent;
	}

	public static ControllableAssignmentEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableAssignmentEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static ControllableAssignmentEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableAssignmentEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static ControllableAssignmentEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static ControllableAssignmentEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		ControllableAssignmentEvent controllableAssignmentEvent = Create(targets, connection, reliability);
		if (controllableAssignmentEvent == null)
		{
			return false;
		}
		controllableAssignmentEvent.PlayerID = PlayerID;
		controllableAssignmentEvent.ControllableID = ControllableID;
		controllableAssignmentEvent.ReleaseFirst = ReleaseFirst;
		controllableAssignmentEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, PlayerID, ControllableID, ReleaseFirst);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		return Post((byte)targets, null, reliability, PlayerID, ControllableID, ReleaseFirst);
	}

	public static bool Post(BoltConnection connection, int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, PlayerID, ControllableID, ReleaseFirst);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		return Post(10, connection, reliability, PlayerID, ControllableID, ReleaseFirst);
	}

	public static bool Post(int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, PlayerID, ControllableID, ReleaseFirst);
	}

	public static bool Post(ReliabilityModes reliability, int PlayerID, int ControllableID, bool ReleaseFirst)
	{
		return Post(2, null, reliability, PlayerID, ControllableID, ReleaseFirst);
	}
}
