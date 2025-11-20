using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class PerUserChallengeCounter : IMessage<PerUserChallengeCounter>, IMessage, IEquatable<PerUserChallengeCounter>, IDeepCloneable<PerUserChallengeCounter>, IBufferMessage
{
	private static readonly MessageParser<PerUserChallengeCounter> _parser = new MessageParser<PerUserChallengeCounter>(() => new PerUserChallengeCounter());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int ChallengeCountersFieldNumber = 2;

	private static readonly FieldCodec<ChallengeCounterWithEvents> _repeated_challengeCounters_codec = FieldCodec.ForMessage(18u, ChallengeCounterWithEvents.Parser);

	private readonly RepeatedField<ChallengeCounterWithEvents> challengeCounters_ = new RepeatedField<ChallengeCounterWithEvents>();

	[DebuggerNonUserCode]
	public static MessageParser<PerUserChallengeCounter> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesContractsReflection.Descriptor.MessageTypes[9];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string UserId
	{
		get
		{
			return userId_;
		}
		set
		{
			userId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChallengeCounterWithEvents> ChallengeCounters => challengeCounters_;

	[DebuggerNonUserCode]
	public PerUserChallengeCounter()
	{
	}

	[DebuggerNonUserCode]
	public PerUserChallengeCounter(PerUserChallengeCounter other)
		: this()
	{
		userId_ = other.userId_;
		challengeCounters_ = other.challengeCounters_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PerUserChallengeCounter Clone()
	{
		return new PerUserChallengeCounter(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PerUserChallengeCounter);
	}

	[DebuggerNonUserCode]
	public bool Equals(PerUserChallengeCounter other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (UserId != other.UserId)
		{
			return false;
		}
		if (!challengeCounters_.Equals(other.challengeCounters_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (UserId.Length != 0)
		{
			num ^= UserId.GetHashCode();
		}
		num ^= challengeCounters_.GetHashCode();
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
		if (UserId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(UserId);
		}
		challengeCounters_.WriteTo(ref output, _repeated_challengeCounters_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (UserId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(UserId);
		}
		num += challengeCounters_.CalculateSize(_repeated_challengeCounters_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PerUserChallengeCounter other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			challengeCounters_.Add(other.challengeCounters_);
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
				UserId = input.ReadString();
				break;
			case 18u:
				challengeCounters_.AddEntriesFrom(ref input, _repeated_challengeCounters_codec);
				break;
			}
		}
	}
}
