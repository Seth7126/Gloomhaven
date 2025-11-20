#define DEBUG
using System;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

public class GameActionEvent : Event
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

	public int SupplementaryDataIDMed
	{
		get
		{
			return Storage.Values[OffsetStorage + 6].Int0;
		}
		set
		{
			if (value < 0 || value > 63)
			{
				BoltLog.Warn("Property 'SupplementaryDataIDMed' is being set to a value larger than the compression settings, it will be clamped to [+0, +63]");
			}
			value = Mathf.Clamp(value, 0, 63);
			int @int = Storage.Values[OffsetStorage + 6].Int0;
			Storage.Values[OffsetStorage + 6].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public int SupplementaryDataIDMax
	{
		get
		{
			return Storage.Values[OffsetStorage + 7].Int0;
		}
		set
		{
			int @int = Storage.Values[OffsetStorage + 7].Int0;
			Storage.Values[OffsetStorage + 7].Int0 = value;
			if (!NetworkValue.Diff(@int, value))
			{
			}
		}
	}

	public bool SupplementaryDataBoolean
	{
		get
		{
			return Storage.Values[OffsetStorage + 8].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 8].Bool;
			Storage.Values[OffsetStorage + 8].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public Guid SupplementaryDataGuid
	{
		get
		{
			return Storage.Values[OffsetStorage + 9].Guid;
		}
		set
		{
			Guid guid = Storage.Values[OffsetStorage + 9].Guid;
			Storage.Values[OffsetStorage + 9].Guid = value;
			if (!NetworkValue.Diff(guid, value))
			{
			}
		}
	}

	public IProtocolToken SupplementaryDataToken
	{
		get
		{
			return Storage.Values[OffsetStorage + 10].ProtocolToken;
		}
		set
		{
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 10].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 10].ProtocolToken = value;
			if (!NetworkValue.Diff(protocolToken, value))
			{
			}
		}
	}

	public IProtocolToken SupplementaryDataToken2
	{
		get
		{
			return Storage.Values[OffsetStorage + 11].ProtocolToken;
		}
		set
		{
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 11].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 11].ProtocolToken = value;
			if (!NetworkValue.Diff(protocolToken, value))
			{
			}
		}
	}

	public IProtocolToken SupplementaryDataToken3
	{
		get
		{
			return Storage.Values[OffsetStorage + 12].ProtocolToken;
		}
		set
		{
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 12].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 12].ProtocolToken = value;
			if (!NetworkValue.Diff(protocolToken, value))
			{
			}
		}
	}

	public IProtocolToken SupplementaryDataToken4
	{
		get
		{
			return Storage.Values[OffsetStorage + 13].ProtocolToken;
		}
		set
		{
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 13].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 13].ProtocolToken = value;
			if (!NetworkValue.Diff(protocolToken, value))
			{
			}
		}
	}

	public bool SyncViaStateUpdate
	{
		get
		{
			return Storage.Values[OffsetStorage + 14].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 14].Bool;
			Storage.Values[OffsetStorage + 14].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public bool ValidateAction
	{
		get
		{
			return Storage.Values[OffsetStorage + 15].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 15].Bool;
			Storage.Values[OffsetStorage + 15].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public bool DoNotForwardAction
	{
		get
		{
			return Storage.Values[OffsetStorage + 16].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 16].Bool;
			Storage.Values[OffsetStorage + 16].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public bool BinaryDataIncludesLoggingDetails
	{
		get
		{
			return Storage.Values[OffsetStorage + 17].Bool;
		}
		set
		{
			bool a = Storage.Values[OffsetStorage + 17].Bool;
			Storage.Values[OffsetStorage + 17].Bool = value;
			if (!NetworkValue.Diff(a, value))
			{
			}
		}
	}

	public GameActionEvent()
		: base(GameActionEvent_Meta.Instance)
	{
	}

	public override string ToString()
	{
		return $"[GameActionEvent ActionID={ActionID} ActionTypeID={ActionTypeID} PlayerID={PlayerID} ActorID={ActorID} TargetPhaseID={TargetPhaseID} SupplementaryDataIDMin={SupplementaryDataIDMin} SupplementaryDataIDMed={SupplementaryDataIDMed} SupplementaryDataIDMax={SupplementaryDataIDMax} SupplementaryDataBoolean={SupplementaryDataBoolean} SupplementaryDataGuid={SupplementaryDataGuid} SupplementaryDataToken={SupplementaryDataToken} SupplementaryDataToken2={SupplementaryDataToken2} SupplementaryDataToken3={SupplementaryDataToken3} SupplementaryDataToken4={SupplementaryDataToken4} SyncViaStateUpdate={SyncViaStateUpdate} ValidateAction={ValidateAction} DoNotForwardAction={DoNotForwardAction} BinaryDataIncludesLoggingDetails={BinaryDataIncludesLoggingDetails}]";
	}

	protected override void PrepareRelease()
	{
		Storage.Values[OffsetStorage + 10].ProtocolToken.Release();
		Storage.Values[OffsetStorage + 11].ProtocolToken.Release();
		Storage.Values[OffsetStorage + 12].ProtocolToken.Release();
		Storage.Values[OffsetStorage + 13].ProtocolToken.Release();
	}

	private static GameActionEvent Create(byte targets, BoltConnection connection, ReliabilityModes reliability)
	{
		if (!(Factory.NewEvent(((IFactory)GameActionEvent_Meta.Instance).TypeKey) is GameActionEvent gameActionEvent))
		{
			return null;
		}
		gameActionEvent.Targets = targets;
		gameActionEvent.TargetConnection = connection;
		gameActionEvent.Reliability = reliability;
		return gameActionEvent;
	}

	public static GameActionEvent Create(GlobalTargets targets)
	{
		return Create((byte)targets, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameActionEvent Create(GlobalTargets targets, ReliabilityModes reliability)
	{
		return Create((byte)targets, null, reliability);
	}

	public static GameActionEvent Create(BoltConnection connection)
	{
		return Create(10, connection, ReliabilityModes.ReliableOrdered);
	}

	public static GameActionEvent Create(BoltConnection connection, ReliabilityModes reliability)
	{
		return Create(10, connection, reliability);
	}

	public static GameActionEvent Create()
	{
		return Create(2, null, ReliabilityModes.ReliableOrdered);
	}

	public static GameActionEvent Create(ReliabilityModes reliability)
	{
		return Create(2, null, reliability);
	}

	private static bool Post(byte targets, BoltConnection connection, ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		GameActionEvent gameActionEvent = Create(targets, connection, reliability);
		if (gameActionEvent == null)
		{
			return false;
		}
		gameActionEvent.ActionID = ActionID;
		gameActionEvent.ActionTypeID = ActionTypeID;
		gameActionEvent.PlayerID = PlayerID;
		gameActionEvent.ActorID = ActorID;
		gameActionEvent.TargetPhaseID = TargetPhaseID;
		gameActionEvent.SupplementaryDataIDMin = SupplementaryDataIDMin;
		gameActionEvent.SupplementaryDataIDMed = SupplementaryDataIDMed;
		gameActionEvent.SupplementaryDataIDMax = SupplementaryDataIDMax;
		gameActionEvent.SupplementaryDataBoolean = SupplementaryDataBoolean;
		gameActionEvent.SupplementaryDataGuid = SupplementaryDataGuid;
		gameActionEvent.SupplementaryDataToken = SupplementaryDataToken;
		gameActionEvent.SupplementaryDataToken2 = SupplementaryDataToken2;
		gameActionEvent.SupplementaryDataToken3 = SupplementaryDataToken3;
		gameActionEvent.SupplementaryDataToken4 = SupplementaryDataToken4;
		gameActionEvent.SyncViaStateUpdate = SyncViaStateUpdate;
		gameActionEvent.ValidateAction = ValidateAction;
		gameActionEvent.DoNotForwardAction = DoNotForwardAction;
		gameActionEvent.BinaryDataIncludesLoggingDetails = BinaryDataIncludesLoggingDetails;
		gameActionEvent.Send();
		return true;
	}

	public static bool Post(GlobalTargets targets, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		return Post((byte)targets, null, ReliabilityModes.ReliableOrdered, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataIDMed, SupplementaryDataIDMax, SupplementaryDataBoolean, SupplementaryDataGuid, SupplementaryDataToken, SupplementaryDataToken2, SupplementaryDataToken3, SupplementaryDataToken4, SyncViaStateUpdate, ValidateAction, DoNotForwardAction, BinaryDataIncludesLoggingDetails);
	}

	public static bool Post(GlobalTargets targets, ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		return Post((byte)targets, null, reliability, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataIDMed, SupplementaryDataIDMax, SupplementaryDataBoolean, SupplementaryDataGuid, SupplementaryDataToken, SupplementaryDataToken2, SupplementaryDataToken3, SupplementaryDataToken4, SyncViaStateUpdate, ValidateAction, DoNotForwardAction, BinaryDataIncludesLoggingDetails);
	}

	public static bool Post(BoltConnection connection, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		return Post(10, connection, ReliabilityModes.ReliableOrdered, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataIDMed, SupplementaryDataIDMax, SupplementaryDataBoolean, SupplementaryDataGuid, SupplementaryDataToken, SupplementaryDataToken2, SupplementaryDataToken3, SupplementaryDataToken4, SyncViaStateUpdate, ValidateAction, DoNotForwardAction, BinaryDataIncludesLoggingDetails);
	}

	public static bool Post(BoltConnection connection, ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		return Post(10, connection, reliability, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataIDMed, SupplementaryDataIDMax, SupplementaryDataBoolean, SupplementaryDataGuid, SupplementaryDataToken, SupplementaryDataToken2, SupplementaryDataToken3, SupplementaryDataToken4, SyncViaStateUpdate, ValidateAction, DoNotForwardAction, BinaryDataIncludesLoggingDetails);
	}

	public static bool Post(int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		return Post(2, null, ReliabilityModes.ReliableOrdered, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataIDMed, SupplementaryDataIDMax, SupplementaryDataBoolean, SupplementaryDataGuid, SupplementaryDataToken, SupplementaryDataToken2, SupplementaryDataToken3, SupplementaryDataToken4, SyncViaStateUpdate, ValidateAction, DoNotForwardAction, BinaryDataIncludesLoggingDetails);
	}

	public static bool Post(ReliabilityModes reliability, int ActionID, int ActionTypeID, int PlayerID, int ActorID, int TargetPhaseID, int SupplementaryDataIDMin, int SupplementaryDataIDMed, int SupplementaryDataIDMax, bool SupplementaryDataBoolean, Guid SupplementaryDataGuid, IProtocolToken SupplementaryDataToken, IProtocolToken SupplementaryDataToken2, IProtocolToken SupplementaryDataToken3, IProtocolToken SupplementaryDataToken4, bool SyncViaStateUpdate, bool ValidateAction, bool DoNotForwardAction, bool BinaryDataIncludesLoggingDetails)
	{
		return Post(2, null, reliability, ActionID, ActionTypeID, PlayerID, ActorID, TargetPhaseID, SupplementaryDataIDMin, SupplementaryDataIDMed, SupplementaryDataIDMax, SupplementaryDataBoolean, SupplementaryDataGuid, SupplementaryDataToken, SupplementaryDataToken2, SupplementaryDataToken3, SupplementaryDataToken4, SyncViaStateUpdate, ValidateAction, DoNotForwardAction, BinaryDataIncludesLoggingDetails);
	}
}
