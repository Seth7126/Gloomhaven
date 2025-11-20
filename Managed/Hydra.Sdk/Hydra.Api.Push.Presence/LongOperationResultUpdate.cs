using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Errors;

namespace Hydra.Api.Push.Presence;

public sealed class LongOperationResultUpdate : IMessage<LongOperationResultUpdate>, IMessage, IEquatable<LongOperationResultUpdate>, IDeepCloneable<LongOperationResultUpdate>, IBufferMessage
{
	private static readonly MessageParser<LongOperationResultUpdate> _parser = new MessageParser<LongOperationResultUpdate>(() => new LongOperationResultUpdate());

	private UnknownFieldSet _unknownFields;

	public const int CorrelationIdFieldNumber = 1;

	private string correlationId_ = "";

	public const int ResultCodeFieldNumber = 2;

	private ErrorCode resultCode_ = ErrorCode.Success;

	[DebuggerNonUserCode]
	public static MessageParser<LongOperationResultUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PresenceReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string CorrelationId
	{
		get
		{
			return correlationId_;
		}
		set
		{
			correlationId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ErrorCode ResultCode
	{
		get
		{
			return resultCode_;
		}
		set
		{
			resultCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	public LongOperationResultUpdate()
	{
	}

	[DebuggerNonUserCode]
	public LongOperationResultUpdate(LongOperationResultUpdate other)
		: this()
	{
		correlationId_ = other.correlationId_;
		resultCode_ = other.resultCode_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public LongOperationResultUpdate Clone()
	{
		return new LongOperationResultUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as LongOperationResultUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(LongOperationResultUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CorrelationId != other.CorrelationId)
		{
			return false;
		}
		if (ResultCode != other.ResultCode)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (CorrelationId.Length != 0)
		{
			num ^= CorrelationId.GetHashCode();
		}
		if (ResultCode != ErrorCode.Success)
		{
			num ^= ResultCode.GetHashCode();
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
		if (CorrelationId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(CorrelationId);
		}
		if (ResultCode != ErrorCode.Success)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)ResultCode);
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
		if (CorrelationId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(CorrelationId);
		}
		if (ResultCode != ErrorCode.Success)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ResultCode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(LongOperationResultUpdate other)
	{
		if (other != null)
		{
			if (other.CorrelationId.Length != 0)
			{
				CorrelationId = other.CorrelationId;
			}
			if (other.ResultCode != ErrorCode.Success)
			{
				ResultCode = other.ResultCode;
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
				CorrelationId = input.ReadString();
				break;
			case 16u:
				ResultCode = (ErrorCode)input.ReadEnum();
				break;
			}
		}
	}
}
