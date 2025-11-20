#define DEBUG
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

public class GameActionEventClassID : Event
{
	public int ActionID
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

	public int ActionTypeID
	{
		get
		{
			return Storage.Values[OffsetStorage + 1].Int0;
		}
		set
		{
			if (value < 0 || value > 127)
			{
				BoltLog.Warn("Property 'ActionTypeID' is being set to a value larger than the compression settings, it will be clamped to [+0, +127]");
			}
			value = Mathf.Clamp(value, 0, 127);
			int @int = Storage.Values[OffsetStorage + 1].Int0;
			Storage.Values[OffsetStorage + 1].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int PlayerID
	{
		get
		{
			return Storage.Values[OffsetStorage + 2].Int0;
		}
		set
		{
			if (value < 0 || value > 255)
			{
				BoltLog.Warn("Property 'PlayerID' is being set to a value larger than the compression settings, it will be clamped to [+0, +255]");
			}
			value = Mathf.Clamp(value, 0, 255);
			int @int = Storage.Values[OffsetStorage + 2].Int0;
			Storage.Values[OffsetStorage + 2].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int ActorID
	{
		get
		{
			return Storage.Values[OffsetStorage + 3].Int0;
		}
		set
		{
			int @int = Storage.Values[OffsetStorage + 3].Int0;
			Storage.Values[OffsetStorage + 3].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int TargetPhaseID
	{
		get
		{
			return Storage.Values[OffsetStorage + 4].Int0;
		}
		set
		{
			if (value < 0 || value > 127)
			{
				BoltLog.Warn("Property 'TargetPhaseID' is being set to a value larger than the compression settings, it will be clamped to [+0, +127]");
			}
			value = Mathf.Clamp(value, 0, 127);
			int @int = Storage.Values[OffsetStorage + 4].Int0;
			Storage.Values[OffsetStorage + 4].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int SupplementaryDataIDMin
	{
		get
		{
			return Storage.Values[OffsetStorage + 5].Int0;
		}
		set
		{
			if (value < 0 || value > 7)
			{
				BoltLog.Warn("Property 'SupplementaryDataIDMin' is being set to a value larger than the compression settings, it will be clamped to [+0, +7]");
			}
			value = Mathf.Clamp(value, 0, 7);
			int @int = Storage.Values[OffsetStorage + 5].Int0;
			Storage.Values[OffsetStorage + 5].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public bool SupplementaryDataBoolean
	{
		get
		{
			return Storage.Values[OffsetStorage + 6].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 6].Bool;
			Storage.Values[OffsetStorage + 6].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public string ClassID
	{
		get
		{
			return Storage.Values[OffsetStorage + 7].String;
		}
		set
		{
			string a = Storage.Values[OffsetStorage + 7].String;
			Storage.Values[OffsetStorage + 7].String = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public GameActionEventClassID()
		: base(GameActionEventClassID_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[GameActionEventClassID ActionID={ActionID} ActionTypeID={ActionTypeID} PlayerID={PlayerID} ActorID={ActorID} TargetPhaseID={TargetPhaseID} SupplementaryDataIDMin={SupplementaryDataIDMin} SupplementaryDataBoolean={SupplementaryDataBoolean} ClassID={ClassID}]";
	}

	private static GameActionEventClassID Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)GameActionEventClassID_Meta.Instance).TypeKey) is GameActionEventClassID gameActionEventClassID))
		{
			return null;
		}
		gameActionEventClassID.Targets = targets;
		gameActionEventClassID.TargetConnection = connection;
		gameActionEventClassID.Reliability = reliability;
		return gameActionEventClassID;
	}

	public static GameActionEventClassID Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameActionEventClassID Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static GameActionEventClassID Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static GameActionEventClassID Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static GameActionEventClassID Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameActionEventClassID Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		GameActionEventClassID gameActionEventClassID = Create(targets, connection, reliability);
		if (gameActionEventClassID == null)
		{
			return false;
		}
		gameActionEventClassID.ActionID = ActionID;
		gameActionEventClassID.ActionTypeID = ActionTypeID;
		gameActionEventClassID.PlayerID = PlayerID;
		gameActionEventClassID.ActorID = ActorID;
		gameActionEventClassID.TargetPhaseID = TargetPhaseID;
		gameActionEventClassID.SupplementaryDataIDMin = SupplementaryDataIDMin;
		gameActionEventClassID.SupplementaryDataBoolean = SupplementaryDataBoolean;
		gameActionEventClassID.ClassID = ClassID;
		gameActionEventClassID.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataBoolean, ClassID);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		return Post((byte)targets, null, reliability, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataBoolean, ClassID);
	}

	public static bool Post(BoltConnection connection, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataBoolean, ClassID);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		return Post(10, connection, reliability, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataBoolean, ClassID);
	}

	public static bool Post(int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataBoolean, ClassID);
	}

	public static bool Post(ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, bool SupplementaryDataBoolean, string ClassID)
	{
		return Post(2, null, reliability, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataBoolean, ClassID);
	}
}
