using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Zen.ZenTournaments;

public sealed class TournamentData : IMessage<TournamentData>, IMessage, IEquatable<TournamentData>, IDeepCloneable<TournamentData>, IBufferMessage
{
	private static readonly MessageParser<TournamentData> _parser = new MessageParser<TournamentData>(() => new TournamentData());

	private UnknownFieldSet _unknownFields;

	public const int TournamentIdFieldNumber = 1;

	private string tournamentId_ = "";

	public const int TournamentLeaderboardIdFieldNumber = 2;

	private string tournamentLeaderboardId_ = "";

	public const int CreateDataFieldNumber = 3;

	private TournamentCreateData createData_;

	public const int PublicDataFieldNumber = 4;

	private TournamentPublicData publicData_;

	public const int TournamentTypeFieldNumber = 6;

	private TournamentType tournamentType_ = TournamentType.None;

	public const int TournamentHashFieldNumber = 7;

	private string tournamentHash_ = "";

	public const int IsPasswordProtectedFieldNumber = 8;

	private bool isPasswordProtected_;

	public const int OwnerUserIdFieldNumber = 9;

	private string ownerUserId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<TournamentData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenTournamentsContractsReflection.Descriptor.MessageTypes[16];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TournamentId
	{
		get
		{
			return tournamentId_;
		}
		set
		{
			tournamentId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string TournamentLeaderboardId
	{
		get
		{
			return tournamentLeaderboardId_;
		}
		set
		{
			tournamentLeaderboardId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TournamentCreateData CreateData
	{
		get
		{
			return createData_;
		}
		set
		{
			createData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentPublicData PublicData
	{
		get
		{
			return publicData_;
		}
		set
		{
			publicData_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TournamentType TournamentType
	{
		get
		{
			return tournamentType_;
		}
		set
		{
			tournamentType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string TournamentHash
	{
		get
		{
			return tournamentHash_;
		}
		set
		{
			tournamentHash_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public bool IsPasswordProtected
	{
		get
		{
			return isPasswordProtected_;
		}
		set
		{
			isPasswordProtected_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string OwnerUserId
	{
		get
		{
			return ownerUserId_;
		}
		set
		{
			ownerUserId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public TournamentData()
	{
	}

	[DebuggerNonUserCode]
	public TournamentData(TournamentData other)
		: this()
	{
		tournamentId_ = other.tournamentId_;
		tournamentLeaderboardId_ = other.tournamentLeaderboardId_;
		createData_ = ((other.createData_ != null) ? other.createData_.Clone() : null);
		publicData_ = ((other.publicData_ != null) ? other.publicData_.Clone() : null);
		tournamentType_ = other.tournamentType_;
		tournamentHash_ = other.tournamentHash_;
		isPasswordProtected_ = other.isPasswordProtected_;
		ownerUserId_ = other.ownerUserId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TournamentData Clone()
	{
		return new TournamentData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TournamentData);
	}

	[DebuggerNonUserCode]
	public bool Equals(TournamentData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TournamentId != other.TournamentId)
		{
			return false;
		}
		if (TournamentLeaderboardId != other.TournamentLeaderboardId)
		{
			return false;
		}
		if (!object.Equals(CreateData, other.CreateData))
		{
			return false;
		}
		if (!object.Equals(PublicData, other.PublicData))
		{
			return false;
		}
		if (TournamentType != other.TournamentType)
		{
			return false;
		}
		if (TournamentHash != other.TournamentHash)
		{
			return false;
		}
		if (IsPasswordProtected != other.IsPasswordProtected)
		{
			return false;
		}
		if (OwnerUserId != other.OwnerUserId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TournamentId.Length != 0)
		{
			num ^= TournamentId.GetHashCode();
		}
		if (TournamentLeaderboardId.Length != 0)
		{
			num ^= TournamentLeaderboardId.GetHashCode();
		}
		if (createData_ != null)
		{
			num ^= CreateData.GetHashCode();
		}
		if (publicData_ != null)
		{
			num ^= PublicData.GetHashCode();
		}
		if (TournamentType != TournamentType.None)
		{
			num ^= TournamentType.GetHashCode();
		}
		if (TournamentHash.Length != 0)
		{
			num ^= TournamentHash.GetHashCode();
		}
		if (IsPasswordProtected)
		{
			num ^= IsPasswordProtected.GetHashCode();
		}
		if (OwnerUserId.Length != 0)
		{
			num ^= OwnerUserId.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (TournamentId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TournamentId);
		}
		if (TournamentLeaderboardId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(TournamentLeaderboardId);
		}
		if (createData_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(CreateData);
		}
		if (publicData_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(PublicData);
		}
		if (TournamentType != TournamentType.None)
		{
			output.WriteRawTag(48);
			output.WriteEnum((int)TournamentType);
		}
		if (TournamentHash.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(TournamentHash);
		}
		if (IsPasswordProtected)
		{
			output.WriteRawTag(64);
			output.WriteBool(IsPasswordProtected);
		}
		if (OwnerUserId.Length != 0)
		{
			output.WriteRawTag(74);
			output.WriteString(OwnerUserId);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (TournamentId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentId);
		}
		if (TournamentLeaderboardId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentLeaderboardId);
		}
		if (createData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CreateData);
		}
		if (publicData_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(PublicData);
		}
		if (TournamentType != TournamentType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)TournamentType);
		}
		if (TournamentHash.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TournamentHash);
		}
		if (IsPasswordProtected)
		{
			num += 2;
		}
		if (OwnerUserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(OwnerUserId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TournamentData other)
	{
		if (other == null)
		{
			return;
		}
		if (other.TournamentId.Length != 0)
		{
			TournamentId = other.TournamentId;
		}
		if (other.TournamentLeaderboardId.Length != 0)
		{
			TournamentLeaderboardId = other.TournamentLeaderboardId;
		}
		if (other.createData_ != null)
		{
			if (createData_ == null)
			{
				CreateData = new TournamentCreateData();
			}
			CreateData.MergeFrom(other.CreateData);
		}
		if (other.publicData_ != null)
		{
			if (publicData_ == null)
			{
				PublicData = new TournamentPublicData();
			}
			PublicData.MergeFrom(other.PublicData);
		}
		if (other.TournamentType != TournamentType.None)
		{
			TournamentType = other.TournamentType;
		}
		if (other.TournamentHash.Length != 0)
		{
			TournamentHash = other.TournamentHash;
		}
		if (other.IsPasswordProtected)
		{
			IsPasswordProtected = other.IsPasswordProtected;
		}
		if (other.OwnerUserId.Length != 0)
		{
			OwnerUserId = other.OwnerUserId;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				TournamentId = input.ReadString();
				break;
			case 18u:
				TournamentLeaderboardId = input.ReadString();
				break;
			case 26u:
				if (createData_ == null)
				{
					CreateData = new TournamentCreateData();
				}
				input.ReadMessage(CreateData);
				break;
			case 34u:
				if (publicData_ == null)
				{
					PublicData = new TournamentPublicData();
				}
				input.ReadMessage(PublicData);
				break;
			case 48u:
				TournamentType = (TournamentType)input.ReadEnum();
				break;
			case 58u:
				TournamentHash = input.ReadString();
				break;
			case 64u:
				IsPasswordProtected = input.ReadBool();
				break;
			case 74u:
				OwnerUserId = input.ReadString();
				break;
			}
		}
	}
}
