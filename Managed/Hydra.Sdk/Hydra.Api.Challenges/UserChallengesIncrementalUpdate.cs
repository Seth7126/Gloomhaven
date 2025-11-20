using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class UserChallengesIncrementalUpdate : IMessage<UserChallengesIncrementalUpdate>, IMessage, IEquatable<UserChallengesIncrementalUpdate>, IDeepCloneable<UserChallengesIncrementalUpdate>, IBufferMessage
{
	private static readonly MessageParser<UserChallengesIncrementalUpdate> _parser = new MessageParser<UserChallengesIncrementalUpdate>(() => new UserChallengesIncrementalUpdate());

	private UnknownFieldSet _unknownFields;

	public const int UpdateTypeFieldNumber = 1;

	private ChallengeUpdateType updateType_ = ChallengeUpdateType.None;

	public const int ChallengesFieldNumber = 2;

	private static readonly FieldCodec<Challenge> _repeated_challenges_codec = FieldCodec.ForMessage(18u, Challenge.Parser);

	private readonly RepeatedField<Challenge> challenges_ = new RepeatedField<Challenge>();

	public const int RemovedChallengesFieldNumber = 3;

	private static readonly FieldCodec<string> _repeated_removedChallenges_codec = FieldCodec.ForString(26u);

	private readonly RepeatedField<string> removedChallenges_ = new RepeatedField<string>();

	public const int CurrentTickFieldNumber = 4;

	private ulong currentTick_;

	public const int CurrentTickExpirationSecFieldNumber = 5;

	private ulong currentTickExpirationSec_;

	public const int DailySlotInfoFieldNumber = 6;

	private static readonly FieldCodec<ChallengeDailySlotInfo> _repeated_dailySlotInfo_codec = FieldCodec.ForMessage(50u, ChallengeDailySlotInfo.Parser);

	private readonly RepeatedField<ChallengeDailySlotInfo> dailySlotInfo_ = new RepeatedField<ChallengeDailySlotInfo>();

	[DebuggerNonUserCode]
	public static MessageParser<UserChallengesIncrementalUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ChallengeUpdateType UpdateType
	{
		get
		{
			return updateType_;
		}
		set
		{
			updateType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<Challenge> Challenges => challenges_;

	[DebuggerNonUserCode]
	public RepeatedField<string> RemovedChallenges => removedChallenges_;

	[DebuggerNonUserCode]
	public ulong CurrentTick
	{
		get
		{
			return currentTick_;
		}
		set
		{
			currentTick_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ulong CurrentTickExpirationSec
	{
		get
		{
			return currentTickExpirationSec_;
		}
		set
		{
			currentTickExpirationSec_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChallengeDailySlotInfo> DailySlotInfo => dailySlotInfo_;

	[DebuggerNonUserCode]
	public UserChallengesIncrementalUpdate()
	{
	}

	[DebuggerNonUserCode]
	public UserChallengesIncrementalUpdate(UserChallengesIncrementalUpdate other)
		: this()
	{
		updateType_ = other.updateType_;
		challenges_ = other.challenges_.Clone();
		removedChallenges_ = other.removedChallenges_.Clone();
		currentTick_ = other.currentTick_;
		currentTickExpirationSec_ = other.currentTickExpirationSec_;
		dailySlotInfo_ = other.dailySlotInfo_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserChallengesIncrementalUpdate Clone()
	{
		return new UserChallengesIncrementalUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserChallengesIncrementalUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserChallengesIncrementalUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UpdateType != other.UpdateType)
		{
			return false;
		}
		if (!challenges_.Equals(other.challenges_))
		{
			return false;
		}
		if (!removedChallenges_.Equals(other.removedChallenges_))
		{
			return false;
		}
		if (CurrentTick != other.CurrentTick)
		{
			return false;
		}
		if (CurrentTickExpirationSec != other.CurrentTickExpirationSec)
		{
			return false;
		}
		if (!dailySlotInfo_.Equals(other.dailySlotInfo_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UpdateType != ChallengeUpdateType.None)
		{
			num ^= UpdateType.GetHashCode();
		}
		num ^= challenges_.GetHashCode();
		num ^= removedChallenges_.GetHashCode();
		if (CurrentTick != 0)
		{
			num ^= CurrentTick.GetHashCode();
		}
		if (CurrentTickExpirationSec != 0)
		{
			num ^= CurrentTickExpirationSec.GetHashCode();
		}
		num ^= dailySlotInfo_.GetHashCode();
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
		if (UpdateType != ChallengeUpdateType.None)
		{
			output.WriteRawTag(8);
			output.WriteEnum((int)UpdateType);
		}
		challenges_.WriteTo(ref output, _repeated_challenges_codec);
		removedChallenges_.WriteTo(ref output, _repeated_removedChallenges_codec);
		if (CurrentTick != 0)
		{
			output.WriteRawTag(32);
			output.WriteUInt64(CurrentTick);
		}
		if (CurrentTickExpirationSec != 0)
		{
			output.WriteRawTag(40);
			output.WriteUInt64(CurrentTickExpirationSec);
		}
		dailySlotInfo_.WriteTo(ref output, _repeated_dailySlotInfo_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (UpdateType != ChallengeUpdateType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)UpdateType);
		}
		num += challenges_.CalculateSize(_repeated_challenges_codec);
		num += removedChallenges_.CalculateSize(_repeated_removedChallenges_codec);
		if (CurrentTick != 0)
		{
			num += 1 + CodedOutputStream.ComputeUInt64Size(CurrentTick);
		}
		if (CurrentTickExpirationSec != 0)
		{
			num += 1 + CodedOutputStream.ComputeUInt64Size(CurrentTickExpirationSec);
		}
		num += dailySlotInfo_.CalculateSize(_repeated_dailySlotInfo_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UserChallengesIncrementalUpdate other)
	{
		if (other != null)
		{
			if (other.UpdateType != ChallengeUpdateType.None)
			{
				UpdateType = other.UpdateType;
			}
			challenges_.Add(other.challenges_);
			removedChallenges_.Add(other.removedChallenges_);
			if (other.CurrentTick != 0)
			{
				CurrentTick = other.CurrentTick;
			}
			if (other.CurrentTickExpirationSec != 0)
			{
				CurrentTickExpirationSec = other.CurrentTickExpirationSec;
			}
			dailySlotInfo_.Add(other.dailySlotInfo_);
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
			case 8u:
				UpdateType = (ChallengeUpdateType)input.ReadEnum();
				break;
			case 18u:
				challenges_.AddEntriesFrom(ref input, _repeated_challenges_codec);
				break;
			case 26u:
				removedChallenges_.AddEntriesFrom(ref input, _repeated_removedChallenges_codec);
				break;
			case 32u:
				CurrentTick = input.ReadUInt64();
				break;
			case 40u:
				CurrentTickExpirationSec = input.ReadUInt64();
				break;
			case 50u:
				dailySlotInfo_.AddEntriesFrom(ref input, _repeated_dailySlotInfo_codec);
				break;
			}
		}
	}
}
