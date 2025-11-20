using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Challenges;

public sealed class ChallengeOperationList : IMessage<ChallengeOperationList>, IMessage, IEquatable<ChallengeOperationList>, IDeepCloneable<ChallengeOperationList>, IBufferMessage
{
	private static readonly MessageParser<ChallengeOperationList> _parser = new MessageParser<ChallengeOperationList>(() => new ChallengeOperationList());

	private UnknownFieldSet _unknownFields;

	public const int UserIdFieldNumber = 1;

	private string userId_ = "";

	public const int OperationsFieldNumber = 2;

	private static readonly FieldCodec<ChallengeCounterOperation> _repeated_operations_codec = FieldCodec.ForMessage(18u, ChallengeCounterOperation.Parser);

	private readonly RepeatedField<ChallengeCounterOperation> operations_ = new RepeatedField<ChallengeCounterOperation>();

	[DebuggerNonUserCode]
	public static MessageParser<ChallengeOperationList> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ChallengesCoreReflection.Descriptor.MessageTypes[8];

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
	public RepeatedField<ChallengeCounterOperation> Operations => operations_;

	[DebuggerNonUserCode]
	public ChallengeOperationList()
	{
	}

	[DebuggerNonUserCode]
	public ChallengeOperationList(ChallengeOperationList other)
		: this()
	{
		userId_ = other.userId_;
		operations_ = other.operations_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ChallengeOperationList Clone()
	{
		return new ChallengeOperationList(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ChallengeOperationList);
	}

	[DebuggerNonUserCode]
	public bool Equals(ChallengeOperationList other)
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
		if (!operations_.Equals(other.operations_))
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
		num ^= operations_.GetHashCode();
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
		operations_.WriteTo(ref output, _repeated_operations_codec);
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
		num += operations_.CalculateSize(_repeated_operations_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ChallengeOperationList other)
	{
		if (other != null)
		{
			if (other.UserId.Length != 0)
			{
				UserId = other.UserId;
			}
			operations_.Add(other.operations_);
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
				operations_.AddEntriesFrom(ref input, _repeated_operations_codec);
				break;
			}
		}
	}
}
