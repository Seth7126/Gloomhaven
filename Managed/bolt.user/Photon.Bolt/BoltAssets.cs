namespace Photon.Bolt;

public static class BoltAssets
{
	public static class ControllableState
	{
		public static readonly string ControllableID = "ControllableID";

		public static readonly string ControllerID = "ControllerID";

		public new static string ToString()
		{
			return "ControllableState";
		}
	}

	public static class GHControllableState
	{
		public static readonly string Level = "Level";

		public static readonly string PerkPoints = "PerkPoints";

		public static readonly string ActivePerks = "ActivePerks";

		public static readonly string CardInventory = "CardInventory";

		public static readonly string ItemInventory = "ItemInventory";

		public static readonly string StartingTile = "StartingTile";

		public static readonly string StartRoundCards = "StartRoundCards";

		public new static string ToString()
		{
			return "GHControllableState";
		}
	}

	public static class PlayerState
	{
		public static readonly string LatestProcessedActionID = "LatestProcessedActionID";

		public new static string ToString()
		{
			return "PlayerState";
		}
	}

	public static class VoicePlayer
	{
		public static readonly string Transform = "Transform";

		public static readonly string VoicePlayerID = "VoicePlayerID";

		public new static string ToString()
		{
			return "VoicePlayer";
		}
	}

	public static class ControllableAssignmentEvent
	{
		public static readonly string PlayerID = "PlayerID";

		public static readonly string ControllableID = "ControllableID";

		public static readonly string ReleaseFirst = "ReleaseFirst";

		public new static string ToString()
		{
			return "ControllableAssignmentEvent";
		}
	}

	public static class ControllableAssignmentRequest
	{
		public new static string ToString()
		{
			return "ControllableAssignmentRequest";
		}
	}

	public static class ControllableDestructionEvent
	{
		public static readonly string ControllableID = "ControllableID";

		public new static string ToString()
		{
			return "ControllableDestructionEvent";
		}
	}

	public static class ControllableReleaseEvent
	{
		public static readonly string PlayerID = "PlayerID";

		public static readonly string ControllableID = "ControllableID";

		public new static string ToString()
		{
			return "ControllableReleaseEvent";
		}
	}

	public static class GameActionEvent
	{
		public static readonly string ActionID = "ActionID";

		public static readonly string ActionTypeID = "ActionTypeID";

		public static readonly string PlayerID = "PlayerID";

		public static readonly string ActorID = "ActorID";

		public static readonly string TargetPhaseID = "TargetPhaseID";

		public static readonly string SupplementaryDataIDMin = "SupplementaryDataIDMin";

		public static readonly string SupplementaryDataIDMed = "SupplementaryDataIDMed";

		public static readonly string SupplementaryDataIDMax = "SupplementaryDataIDMax";

		public static readonly string SupplementaryDataBoolean = "SupplementaryDataBoolean";

		public static readonly string SupplementaryDataGuid = "SupplementaryDataGuid";

		public static readonly string SupplementaryDataToken = "SupplementaryDataToken";

		public static readonly string SupplementaryDataToken2 = "SupplementaryDataToken2";

		public static readonly string SupplementaryDataToken3 = "SupplementaryDataToken3";

		public static readonly string SupplementaryDataToken4 = "SupplementaryDataToken4";

		public static readonly string SyncViaStateUpdate = "SyncViaStateUpdate";

		public static readonly string ValidateAction = "ValidateAction";

		public static readonly string DoNotForwardAction = "DoNotForwardAction";

		public static readonly string BinaryDataIncludesLoggingDetails = "BinaryDataIncludesLoggingDetails";

		public new static string ToString()
		{
			return "GameActionEvent";
		}
	}

	public static class GameActionEventClassID
	{
		public static readonly string ActionID = "ActionID";

		public static readonly string ActionTypeID = "ActionTypeID";

		public static readonly string PlayerID = "PlayerID";

		public static readonly string ActorID = "ActorID";

		public static readonly string TargetPhaseID = "TargetPhaseID";

		public static readonly string SupplementaryDataIDMin = "SupplementaryDataIDMin";

		public static readonly string SupplementaryDataBoolean = "SupplementaryDataBoolean";

		public static readonly string ClassID = "ClassID";

		public new static string ToString()
		{
			return "GameActionEventClassID";
		}
	}

	public static class GameDataEvent
	{
		public static readonly string DataActionID = "DataActionID";

		public static readonly string ChunkSize = "ChunkSize";

		public static readonly string ChunkIndex = "ChunkIndex";

		public static readonly string TotalSize = "TotalSize";

		public static readonly string Complete = "Complete";

		public new static string ToString()
		{
			return "GameDataEvent";
		}
	}

	public static class GameDataRequest
	{
		public static readonly string DataActionID = "DataActionID";

		public new static string ToString()
		{
			return "GameDataRequest";
		}
	}

	public static class NetworkActionEvent
	{
		public static readonly string Token = "Token";

		public new static string ToString()
		{
			return "NetworkActionEvent";
		}
	}

	public static class PlayerEntityInitializedEvent
	{
		public new static string ToString()
		{
			return "PlayerEntityInitializedEvent";
		}
	}

	public static class PlayerEntityRequest
	{
		public new static string ToString()
		{
			return "PlayerEntityRequest";
		}
	}

	public static class SavePointReachedEvent
	{
		public new static string ToString()
		{
			return "SavePointReachedEvent";
		}
	}

	public static class SessionNegotiationEvent
	{
		public static readonly string Data = "Data";

		public static readonly string MessageType = "MessageType";

		public static readonly string Platform = "Platform";

		public new static string ToString()
		{
			return "SessionNegotiationEvent";
		}
	}

	public static class BlockedUsersStateChangedEvent
	{
		public static readonly string Data = "Data";

		public new static string ToString()
		{
			return "BlockedUsersStateChangedEvent";
		}
	}

	public static class LogNetworkMessageEvent
	{
		public static readonly string Message = "Message";

		public new static string ToString()
		{
			return "LogNetworkMessageEvent";
		}
	}

	public static string Combine(string asset1, string asset2)
	{
		return $"{asset1}.{asset2}";
	}

	public static string Combine(string asset1, string asset2, string asset3)
	{
		return $"{asset1}.{asset2}.{asset3}";
	}
}
