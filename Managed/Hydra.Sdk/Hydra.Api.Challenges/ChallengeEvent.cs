using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ChallengeEvent : IMessage<ChallengeEvent>, IMessage, IEquatable<ChallengeEvent>, IDeepCloneable<ChallengeEvent>, IBufferMessage
{
	private static readonly MessageParser<ChallengeEvent> _parser = new MessageParser<ChallengeEvent>(() => new ChallengeEvent());

	private UnknownFieldSet _unknownFields;

	public const int EventNameFieldNumber = 1;

	private string eventName_ = "";

	public const int OperationFieldNumber = 2;

	private ChallengeOperationType operation_ = ChallengeOperationType.ChallengeOpertaionTypeNone;

	public const int EventFilterFieldNumber = 3;

	private static readonly FieldCodec<ChallengeCounterFilterItem> _repeated_eventFilter_codec = FieldCodec.ForMessage(26u, ChallengeCounterFilterItem.Parser);

	private readonly RepeatedField<ChallengeCounterFilterItem> eventFilter_ = new RepeatedField<ChallengeCounterFilterItem>();

	public const int ValueFieldNumber = 4;

	private ulong value_;

	[DebuggerNonUserCode]
	public static MessageParser<ChallengeEvent> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string EventName
	{
		get
		{
			return eventName_;
		}
		set
		{
			eventName_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ChallengeOperationType Operation
	{
		get
		{
			return operation_;
		}
		set
		{
			operation_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChallengeCounterFilterItem> EventFilter => eventFilter_;

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
	public ChallengeEvent()
	{
	}

	[DebuggerNonUserCode]
	public ChallengeEvent(ChallengeEvent other)
		: this()
	{
		eventName_ = other.eventName_;
		operation_ = other.operation_;
		eventFilter_ = other.eventFilter_.Clone();
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChallengeEvent Clone()
	{
		return new ChallengeEvent(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChallengeEvent);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChallengeEvent other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (EventName != other.EventName)
		{
			return false;
		}
		if (Operation != other.Operation)
		{
			return false;
		}
		if (!eventFilter_.Equals(other.eventFilter_))
		{
			return false;
		}
		if (Value != other.Value)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (EventName.Length != 0)
		{
			num ^= EventName.GetHashCode();
		}
		if (Operation != ChallengeOperationType.ChallengeOpertaionTypeNone)
		{
			num ^= Operation.GetHashCode();
		}
		num ^= eventFilter_.GetHashCode();
		if (Value != 0)
		{
			num ^= Value.GetHashCode();
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
		if (EventName.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(EventName);
		}
		if (Operation != ChallengeOperationType.ChallengeOpertaionTypeNone)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)Operation);
		}
		eventFilter_.WriteTo(ref output, _repeated_eventFilter_codec);
		if (Value != 0)
		{
			output.WriteRawTag(32);
			output.WriteUInt64(Value);
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
		if (EventName.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(EventName);
		}
		if (Operation != ChallengeOperationType.ChallengeOpertaionTypeNone)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Operation);
		}
		num += eventFilter_.CalculateSize(_repeated_eventFilter_codec);
		if (Value != 0)
		{
			num += 1 + CodedOutputStream.ComputeUInt64Size(Value);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChallengeEvent other)
	{
		if (other != null)
		{
			if (other.EventName.Length != 0)
			{
				EventName = other.EventName;
			}
			if (other.Operation != ChallengeOperationType.ChallengeOpertaionTypeNone)
			{
				Operation = other.Operation;
			}
			eventFilter_.Add(other.eventFilter_);
			if (other.Value != 0)
			{
				Value = other.Value;
			}
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
				EventName = input.ReadString();
				break;
			case 16u:
				Operation = (ChallengeOperationType)input.ReadEnum();
				break;
			case 26u:
				eventFilter_.AddEntriesFrom(ref input, _repeated_eventFilter_codec);
				break;
			case 32u:
				Value = input.ReadUInt64();
				break;
			}
		}
	}
}
