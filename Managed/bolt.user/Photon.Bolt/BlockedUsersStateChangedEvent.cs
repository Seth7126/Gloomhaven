using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class BlockedUsersStateChangedEvent : Event
{
	public IProtocolToken Data
	{
		get
		{
			return Storage.Values[OffsetStorage].ProtocolToken;
		}
		set
		{
			IProtocolToken protocolToken = Storage.Values[OffsetStorage].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage].ProtocolToken = value;
			if (!NetworkValue.Diff(protocolToken, value))
			{
			}
		}
	}

	public BlockedUsersStateChangedEvent()
		: base(BlockedUsersStateChangedEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[BlockedUsersStateChangedEvent Data={Data}]";
	}

	protected override void PrepareRelease()
	{
		Storage.Values[OffsetStorage].ProtocolToken.Release();
	}

	private static BlockedUsersStateChangedEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)BlockedUsersStateChangedEvent_Meta.Instance).TypeKey) is BlockedUsersStateChangedEvent blockedUsersStateChangedEvent))
		{
			return null;
		}
		blockedUsersStateChangedEvent.Targets = targets;
		blockedUsersStateChangedEvent.TargetConnection = connection;
		blockedUsersStateChangedEvent.Reliability = reliability;
		return blockedUsersStateChangedEvent;
	}

	public static BlockedUsersStateChangedEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static BlockedUsersStateChangedEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static BlockedUsersStateChangedEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static BlockedUsersStateChangedEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static BlockedUsersStateChangedEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static BlockedUsersStateChangedEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, IProtocolToken Data)
	{
		BlockedUsersStateChangedEvent blockedUsersStateChangedEvent = Create(targets, connection, reliability);
		if (blockedUsersStateChangedEvent == null)
		{
			return false;
		}
		blockedUsersStateChangedEvent.Data = Data;
		blockedUsersStateChangedEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, IProtocolToken Data)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, Data);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, IProtocolToken Data)
	{
		return Post((byte)targets, null, reliability, Data);
	}

	public static bool Post(BoltConnection connection, IProtocolToken Data)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, Data);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, IProtocolToken Data)
	{
		return Post(10, connection, reliability, Data);
	}

	public static bool Post(IProtocolToken Data)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, Data);
	}

	public static bool Post(ReliabilityModes reliability, IProtocolToken Data)
	{
		return Post(2, null, reliability, Data);
	}
}
