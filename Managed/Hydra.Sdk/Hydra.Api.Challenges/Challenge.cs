using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class Challenge : IMessage<Challenge>, IMessage, IEquatable<Challenge>, IDeepCloneable<Challenge>, IBufferMessage
{
	private static readonly MessageParser<Challenge> _parser = new MessageParser<Challenge>(() => new Challenge());

	private UnknownFieldSet _unknownFields;

	public const int ChallengeIdFieldNumber = 1;

	private string challengeId_ = "";

	public const int DefinitionFieldNumber = 2;

	private string definition_ = "";

	public const int TypeFieldNumber = 3;

	private ChallengeType type_ = ChallengeType.None;

	public const int StateFieldNumber = 4;

	private ChallengeState state_ = ChallengeState.None;

	public const int SlotFieldNumber = 5;

	private int slot_;

	public const int TickFieldNumber = 6;

	private uint tick_;

	public const int CountersFieldNumber = 7;

	private static readonly FieldCodec<ChallengeCounter> _repeated_counters_codec = FieldCodec.ForMessage(58u, ChallengeCounter.Parser);

	private readonly RepeatedField<ChallengeCounter> counters_ = new RepeatedField<ChallengeCounter>();

	[DebuggerNonUserCode]
	public static MessageParser<Challenge> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string ChallengeId
	{
		get
		{
			return challengeId_;
		}
		set
		{
			challengeId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Definition
	{
		get
		{
			return definition_;
		}
		set
		{
			definition_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ChallengeType Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChallengeState State
	{
		get
		{
			return state_;
		}
		set
		{
			state_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Slot
	{
		get
		{
			return slot_;
		}
		set
		{
			slot_ = value;
		}
	}

	[DebuggerNonUserCode]
	public uint Tick
	{
		get
		{
			return tick_;
		}
		set
		{
			tick_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChallengeCounter> Counters => counters_;

	[DebuggerNonUserCode]
	public Challenge()
	{
	}

	[DebuggerNonUserCode]
	public Challenge(Challenge other)
		: this()
	{
		challengeId_ = other.challengeId_;
		definition_ = other.definition_;
		type_ = other.type_;
		state_ = other.state_;
		slot_ = other.slot_;
		tick_ = other.tick_;
		counters_ = other.counters_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public Challenge Clone()
	{
		return new Challenge(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as Challenge);
	}

	[DebuggerNonUserCode]
	public bool Equals(Challenge other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ChallengeId != other.ChallengeId)
		{
			return false;
		}
		if (Definition != other.Definition)
		{
			return false;
		}
		if (Type != other.Type)
		{
			return false;
		}
		if (State != other.State)
		{
			return false;
		}
		if (Slot != other.Slot)
		{
			return false;
		}
		if (Tick != other.Tick)
		{
			return false;
		}
		if (!counters_.Equals(other.counters_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (ChallengeId.Length != 0)
		{
			num ^= ChallengeId.GetHashCode();
		}
		if (Definition.Length != 0)
		{
			num ^= Definition.GetHashCode();
		}
		if (Type != ChallengeType.None)
		{
			num ^= Type.GetHashCode();
		}
		if (State != ChallengeState.None)
		{
			num ^= State.GetHashCode();
		}
		if (Slot != 0)
		{
			num ^= Slot.GetHashCode();
		}
		if (Tick != 0)
		{
			num ^= Tick.GetHashCode();
		}
		num ^= counters_.GetHashCode();
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
		if (ChallengeId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ChallengeId);
		}
		if (Definition.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Definition);
		}
		if (Type != ChallengeType.None)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)Type);
		}
		if (State != ChallengeState.None)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)State);
		}
		if (Slot != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(Slot);
		}
		if (Tick != 0)
		{
			output.WriteRawTag(48);
			output.WriteUInt32(Tick);
		}
		counters_.WriteTo(ref output, _repeated_counters_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (ChallengeId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ChallengeId);
		}
		if (Definition.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Definition);
		}
		if (Type != ChallengeType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Type);
		}
		if (State != ChallengeState.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)State);
		}
		if (Slot != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Slot);
		}
		if (Tick != 0)
		{
			num += 1 + CodedOutputStream.ComputeUInt32Size(Tick);
		}
		num += counters_.CalculateSize(_repeated_counters_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(Challenge other)
	{
		if (other != null)
		{
			if (other.ChallengeId.Length != 0)
			{
				ChallengeId = other.ChallengeId;
			}
			if (other.Definition.Length != 0)
			{
				Definition = other.Definition;
			}
			if (other.Type != ChallengeType.None)
			{
				Type = other.Type;
			}
			if (other.State != ChallengeState.None)
			{
				State = other.State;
			}
			if (other.Slot != 0)
			{
				Slot = other.Slot;
			}
			if (other.Tick != 0)
			{
				Tick = other.Tick;
			}
			counters_.Add(other.counters_);
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
				ChallengeId = input.ReadString();
				break;
			case 18u:
				Definition = input.ReadString();
				break;
			case 24u:
				Type = (ChallengeType)input.ReadEnum();
				break;
			case 32u:
				State = (ChallengeState)input.ReadEnum();
				break;
			case 40u:
				Slot = input.ReadInt32();
				break;
			case 48u:
				Tick = input.ReadUInt32();
				break;
			case 58u:
				counters_.AddEntriesFrom(ref input, _repeated_counters_codec);
				break;
			}
		}
	}
}
