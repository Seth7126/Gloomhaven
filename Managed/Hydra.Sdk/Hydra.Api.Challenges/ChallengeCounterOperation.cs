using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ChallengeCounterOperation : IMessage<ChallengeCounterOperation>, IMessage, IEquatable<ChallengeCounterOperation>, IDeepCloneable<ChallengeCounterOperation>, IBufferMessage
{
	private static readonly MessageParser<ChallengeCounterOperation> _parser = new MessageParser<ChallengeCounterOperation>(() => new ChallengeCounterOperation());

	private UnknownFieldSet _unknownFields;

	public const int ChallengeCounterIdFieldNumber = 1;

	private string challengeCounterId_ = "";

	public const int OperationTypeFieldNumber = 2;

	private ChallengeOperationType operationType_ = ChallengeOperationType.ChallengeOpertaionTypeNone;

	public const int ValueFieldNumber = 3;

	private ulong value_;

	[DebuggerNonUserCode]
	public static MessageParser<ChallengeCounterOperation> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[9];

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
	public ChallengeOperationType OperationType
	{
		get
		{
			return operationType_;
		}
		set
		{
			operationType_ = value;
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
	public ChallengeCounterOperation()
	{
	}

	[DebuggerNonUserCode]
	public ChallengeCounterOperation(ChallengeCounterOperation other)
		: this()
	{
		challengeCounterId_ = other.challengeCounterId_;
		operationType_ = other.operationType_;
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChallengeCounterOperation Clone()
	{
		return new ChallengeCounterOperation(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChallengeCounterOperation);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChallengeCounterOperation other)
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
		if (OperationType != other.OperationType)
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
		if (ChallengeCounterId.Length != 0)
		{
			num ^= ChallengeCounterId.GetHashCode();
		}
		if (OperationType != ChallengeOperationType.ChallengeOpertaionTypeNone)
		{
			num ^= OperationType.GetHashCode();
		}
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
		if (ChallengeCounterId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(ChallengeCounterId);
		}
		if (OperationType != ChallengeOperationType.ChallengeOpertaionTypeNone)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)OperationType);
		}
		if (Value != 0)
		{
			output.WriteRawTag(24);
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
		if (ChallengeCounterId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ChallengeCounterId);
		}
		if (OperationType != ChallengeOperationType.ChallengeOpertaionTypeNone)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)OperationType);
		}
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
	public void MergeFrom(ChallengeCounterOperation other)
	{
		if (other != null)
		{
			if (other.ChallengeCounterId.Length != 0)
			{
				ChallengeCounterId = other.ChallengeCounterId;
			}
			if (other.OperationType != ChallengeOperationType.ChallengeOpertaionTypeNone)
			{
				OperationType = other.OperationType;
			}
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
				ChallengeCounterId = input.ReadString();
				break;
			case 16u:
				OperationType = (ChallengeOperationType)input.ReadEnum();
				break;
			case 24u:
				Value = input.ReadUInt64();
				break;
			}
		}
	}
}
