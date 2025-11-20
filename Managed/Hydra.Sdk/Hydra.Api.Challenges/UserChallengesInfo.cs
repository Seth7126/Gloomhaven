using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class UserChallengesInfo : IMessage<UserChallengesInfo>, IMessage, IEquatable<UserChallengesInfo>, IDeepCloneable<UserChallengesInfo>, IBufferMessage
{
	private static readonly MessageParser<UserChallengesInfo> _parser = new MessageParser<UserChallengesInfo>(() => new UserChallengesInfo());

	private UnknownFieldSet _unknownFields;

	public const int ChallengesFieldNumber = 1;

	private static readonly FieldCodec<Challenge> _repeated_challenges_codec = FieldCodec.ForMessage(10u, Challenge.Parser);

	private readonly RepeatedField<Challenge> challenges_ = new RepeatedField<Challenge>();

	public const int CurrentTickFieldNumber = 2;

	private ulong currentTick_;

	public const int CurrentTickExpirationSecFieldNumber = 3;

	private ulong currentTickExpirationSec_;

	public const int DailySlotInfoFieldNumber = 4;

	private static readonly FieldCodec<ChallengeDailySlotInfo> _repeated_dailySlotInfo_codec = FieldCodec.ForMessage(34u, ChallengeDailySlotInfo.Parser);

	private readonly RepeatedField<ChallengeDailySlotInfo> dailySlotInfo_ = new RepeatedField<ChallengeDailySlotInfo>();

	[DebuggerNonUserCode]
	public static MessageParser<UserChallengesInfo> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<Challenge> Challenges => challenges_;

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
	public UserChallengesInfo()
	{
	}

	[DebuggerNonUserCode]
	public UserChallengesInfo(UserChallengesInfo other)
		: this()
	{
		challenges_ = other.challenges_.Clone();
		currentTick_ = other.currentTick_;
		currentTickExpirationSec_ = other.currentTickExpirationSec_;
		dailySlotInfo_ = other.dailySlotInfo_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UserChallengesInfo Clone()
	{
		return new UserChallengesInfo(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UserChallengesInfo);
	}

	[DebuggerNonUserCode]
	public bool Equals(UserChallengesInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!challenges_.Equals(other.challenges_))
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
		num ^= challenges_.GetHashCode();
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
		challenges_.WriteTo(ref output, _repeated_challenges_codec);
		if (CurrentTick != 0)
		{
			output.WriteRawTag(16);
			output.WriteUInt64(CurrentTick);
		}
		if (CurrentTickExpirationSec != 0)
		{
			output.WriteRawTag(24);
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
		num += challenges_.CalculateSize(_repeated_challenges_codec);
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
	public void MergeFrom(UserChallengesInfo other)
	{
		if (other != null)
		{
			challenges_.Add(other.challenges_);
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
			case 10u:
				challenges_.AddEntriesFrom(ref input, _repeated_challenges_codec);
				break;
			case 16u:
				CurrentTick = input.ReadUInt64();
				break;
			case 24u:
				CurrentTickExpirationSec = input.ReadUInt64();
				break;
			case 34u:
				dailySlotInfo_.AddEntriesFrom(ref input, _repeated_dailySlotInfo_codec);
				break;
			}
		}
	}
}
