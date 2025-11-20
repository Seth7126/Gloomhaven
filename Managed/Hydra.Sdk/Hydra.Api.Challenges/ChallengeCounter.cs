using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ChallengeCounter : IMessage<ChallengeCounter>, IMessage, IEquatable<ChallengeCounter>, IDeepCloneable<ChallengeCounter>, IBufferMessage
{
	private static readonly MessageParser<ChallengeCounter> _parser = new MessageParser<ChallengeCounter>(() => new ChallengeCounter());

	private UnknownFieldSet _unknownFields;

	public const int ChallengeCounterIdFieldNumber = 1;

	private string challengeCounterId_ = "";

	public const int ValueFieldNumber = 2;

	private ulong value_;

	public const int GoalFieldNumber = 3;

	private ulong goal_;

	public const int MilestonesFieldNumber = 4;

	private static readonly FieldCodec<ulong> _repeated_milestones_codec = FieldCodec.ForUInt64(34u);

	private readonly RepeatedField<ulong> milestones_ = new RepeatedField<ulong>();

	[DebuggerNonUserCode]
	public static MessageParser<ChallengeCounter> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ChallengeCounterId
	{
		get
		{
			return challengeCounterId_;
		}
		set
		{
			challengeCounterId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ulong Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ulong Goal
	{
		get
		{
			return goal_;
		}
		set
		{
			goal_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ulong> Milestones => milestones_;

	[DebuggerNonUserCode]
	public ChallengeCounter()
	{
	}

	[DebuggerNonUserCode]
	public ChallengeCounter(ChallengeCounter other)
		: this()
	{
		challengeCounterId_ = other.challengeCounterId_;
		value_ = other.value_;
		goal_ = other.goal_;
		milestones_ = other.milestones_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChallengeCounter Clone()
	{
		return new ChallengeCounter(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChallengeCounter);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChallengeCounter other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ChallengeCounterId != other.ChallengeCounterId)
		{
			return false;
		}
		if (Value != other.Value)
		{
			return false;
		}
		if (Goal != other.Goal)
		{
			return false;
		}
		if (!milestones_.Equals(other.milestones_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ChallengeCounterId.Length != 0)
		{
			num ^= ChallengeCounterId.GetHashCode();
		}
		if (Value != 0)
		{
			num ^= Value.GetHashCode();
		}
		if (Goal != 0)
		{
			num ^= Goal.GetHashCode();
		}
		num ^= milestones_.GetHashCode();
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
		if (ChallengeCounterId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ChallengeCounterId);
		}
		if (Value != 0)
		{
			output.WriteRawTag(16);
			output.WriteUInt64(Value);
		}
		if (Goal != 0)
		{
			output.WriteRawTag(24);
			output.WriteUInt64(Goal);
		}
		milestones_.WriteTo(ref output, _repeated_milestones_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (ChallengeCounterId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ChallengeCounterId);
		}
		if (Value != 0)
		{
			num += 1 + CodedOutputStream.ComputeUInt64Size(Value);
		}
		if (Goal != 0)
		{
			num += 1 + CodedOutputStream.ComputeUInt64Size(Goal);
		}
		num += milestones_.CalculateSize(_repeated_milestones_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChallengeCounter other)
	{
		if (other != null)
		{
			if (other.ChallengeCounterId.Length != 0)
			{
				ChallengeCounterId = other.ChallengeCounterId;
			}
			if (other.Value != 0)
			{
				Value = other.Value;
			}
			if (other.Goal != 0)
			{
				Goal = other.Goal;
			}
			milestones_.Add(other.milestones_);
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
				ChallengeCounterId = input.ReadString();
				break;
			case 16u:
				Value = input.ReadUInt64();
				break;
			case 24u:
				Goal = input.ReadUInt64();
				break;
			case 32u:
			case 34u:
				milestones_.AddEntriesFrom(ref input, _repeated_milestones_codec);
				break;
			}
		}
	}
}
