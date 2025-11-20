using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ChallengeCounterFilterItem : IMessage<ChallengeCounterFilterItem>, IMessage, IEquatable<ChallengeCounterFilterItem>, IDeepCloneable<ChallengeCounterFilterItem>, IBufferMessage
{
	private static readonly MessageParser<ChallengeCounterFilterItem> _parser = new MessageParser<ChallengeCounterFilterItem>(() => new ChallengeCounterFilterItem());

	private UnknownFieldSet _unknownFields;

	public const int NameFieldNumber = 1;

	private string name_ = "";

	public const int ValueFieldNumber = 2;

	private string value_ = "";

	public const int OperationFieldNumber = 3;

	private ChallengeCounterFilterOperationType operation_ = ChallengeCounterFilterOperationType.None;

	[DebuggerNonUserCode]
	public static MessageParser<ChallengeCounterFilterItem> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ChallengeCounterFilterOperationType Operation
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
	public ChallengeCounterFilterItem()
	{
	}

	[DebuggerNonUserCode]
	public ChallengeCounterFilterItem(ChallengeCounterFilterItem other)
		: this()
	{
		name_ = other.name_;
		value_ = other.value_;
		operation_ = other.operation_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChallengeCounterFilterItem Clone()
	{
		return new ChallengeCounterFilterItem(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChallengeCounterFilterItem);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChallengeCounterFilterItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (Value != other.Value)
		{
			return false;
		}
		if (Operation != other.Operation)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
		if (Value.Length != 0)
		{
			num ^= Value.GetHashCode();
		}
		if (Operation != ChallengeCounterFilterOperationType.None)
		{
			num ^= Operation.GetHashCode();
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
		if (Name.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Name);
		}
		if (Value.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Value);
		}
		if (Operation != ChallengeCounterFilterOperationType.None)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)Operation);
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
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		if (Value.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Value);
		}
		if (Operation != ChallengeCounterFilterOperationType.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Operation);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChallengeCounterFilterItem other)
	{
		if (other != null)
		{
			if (other.Name.Length != 0)
			{
				Name = other.Name;
			}
			if (other.Value.Length != 0)
			{
				Value = other.Value;
			}
			if (other.Operation != ChallengeCounterFilterOperationType.None)
			{
				Operation = other.Operation;
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
				Name = input.ReadString();
				break;
			case 18u:
				Value = input.ReadString();
				break;
			case 24u:
				Operation = (ChallengeCounterFilterOperationType)input.ReadEnum();
				break;
			}
		}
	}
}
