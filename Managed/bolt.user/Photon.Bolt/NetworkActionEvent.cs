using Photon.Bolt.Internal;

namespace Photon.Bolt;

public class NetworkActionEvent : Event
{
	public IProtocolToken Token
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

	public NetworkActionEvent()
		: base(NetworkActionEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[NetworkActionEvent Token={Token}]";
	}

	protected override void PrepareRelease()
	{
		Storage.Values[OffsetStorage].ProtocolToken.Release();
	}

	private static NetworkActionEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)NetworkActionEvent_Meta.Instance).TypeKey) is NetworkActionEvent networkActionEvent))
		{
			return null;
		}
		networkActionEvent.Targets = targets;
		networkActionEvent.TargetConnection = connection;
		networkActionEvent.Reliability = reliability;
		return networkActionEvent;
	}

	public static NetworkActionEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static NetworkActionEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static NetworkActionEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static NetworkActionEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static NetworkActionEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static NetworkActionEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, IProtocolToken Token)
	{
		NetworkActionEvent networkActionEvent = Create(targets, connection, reliability);
		if (networkActionEvent == null)
		{
			return false;
		}
		networkActionEvent.Token = Token;
		networkActionEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, IProtocolToken Token)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, Token);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, IProtocolToken Token)
	{
		return Post((byte)targets, null, reliability, Token);
	}

	public static bool Post(BoltConnection connection, IProtocolToken Token)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, Token);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, IProtocolToken Token)
	{
		return Post(10, connection, reliability, Token);
	}

	public static bool Post(IProtocolToken Token)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, Token);
	}

	public static bool Post(ReliabilityModes reliability, IProtocolToken Token)
	{
		return Post(2, null, reliability, Token);
	}
}
