#define DEBUG
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

public class GameDataEvent : Event
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

	public int ChunkSize
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

	public int ChunkIndex
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

	public int TotalSize
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

	public bool Complete
	{
		get
		{
			return Storage.Values[OffsetStorage + 4].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 4].Bool;
			Storage.Values[OffsetStorage + 4].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public GameDataEvent()
		: base(GameDataEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[GameDataEvent DataActionID={DataActionID} ChunkSize={ChunkSize} ChunkIndex={ChunkIndex} TotalSize={TotalSize} Complete={Complete}]";
	}

	private static GameDataEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)GameDataEvent_Meta.Instance).TypeKey) is GameDataEvent gameDataEvent))
		{
			return null;
		}
		gameDataEvent.Targets = targets;
		gameDataEvent.TargetConnection = connection;
		gameDataEvent.Reliability = reliability;
		return gameDataEvent;
	}

	public static GameDataEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameDataEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static GameDataEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static GameDataEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static GameDataEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameDataEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		GameDataEvent gameDataEvent = Create(targets, connection, reliability);
		if (gameDataEvent == null)
		{
			return false;
		}
		gameDataEvent.DataActionID = DataActionID;
		gameDataEvent.ChunkSize = ChunkSize;
		gameDataEvent.ChunkIndex = ChunkIndex;
		gameDataEvent.TotalSize = TotalSize;
		gameDataEvent.Complete = Complete;
		gameDataEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, DataActionID, ChunkSize, ChunkIndex, TotalSize, Complete);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		return Post((byte)targets, null, reliability, DataActionID, ChunkSize, ChunkIndex, TotalSize, Complete);
	}

	public static bool Post(BoltConnection connection, int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, DataActionID, ChunkSize, ChunkIndex, TotalSize, Complete);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		return Post(10, connection, reliability, DataActionID, ChunkSize, ChunkIndex, TotalSize, Complete);
	}

	public static bool Post(int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, DataActionID, ChunkSize, ChunkIndex, TotalSize, Complete);
	}

	public static bool Post(ReliabilityModes reliability, int DataActionID, int ChunkSize, int ChunkIndex, int TotalSize, bool Complete)
	{
		return Post(2, null, reliability, DataActionID, ChunkSize, ChunkIndex, TotalSize, Complete);
	}
}
