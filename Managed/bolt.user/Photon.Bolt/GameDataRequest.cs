#define DEBUG
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

public class GameDataRequest : Event
{
	public int DataActionID
	{
		get
		{
			return Storage.Values[OffsetStorage].Int0;
		}
		set
		{
			if (value < 0 || value > 15)
			{
				BoltLog.Warn("Property 'DataActionID' is being set to a value larger than the compression settings, it will be clamped to [+0, +15]");
			}
			value = Mathf.Clamp(value, 0, 15);
			int @int = Storage.Values[OffsetStorage].Int0;
			Storage.Values[OffsetStorage].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public GameDataRequest()
		: base(GameDataRequest_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[GameDataRequest DataActionID={DataActionID}]";
	}

	private static GameDataRequest Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)GameDataRequest_Meta.Instance).TypeKey) is GameDataRequest gameDataRequest))
		{
			return null;
		}
		gameDataRequest.Targets = targets;
		gameDataRequest.TargetConnection = connection;
		gameDataRequest.Reliability = reliability;
		return gameDataRequest;
	}

	public static GameDataRequest Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameDataRequest Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static GameDataRequest Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static GameDataRequest Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static GameDataRequest Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameDataRequest Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int DataActionID)
	{
		GameDataRequest gameDataRequest = Create(targets, connection, reliability);
		if (gameDataRequest == null)
		{
			return false;
		}
		gameDataRequest.DataActionID = DataActionID;
		gameDataRequest.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int DataActionID)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, DataActionID);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int DataActionID)
	{
		return Post((byte)targets, null, reliability, DataActionID);
	}

	public static bool Post(BoltConnection connection, int DataActionID)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, DataActionID);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int DataActionID)
	{
		return Post(10, connection, reliability, DataActionID);
	}

	public static bool Post(int DataActionID)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, DataActionID);
	}

	public static bool Post(ReliabilityModes reliability, int DataActionID)
	{
		return Post(2, null, reliability, DataActionID);
	}
}
