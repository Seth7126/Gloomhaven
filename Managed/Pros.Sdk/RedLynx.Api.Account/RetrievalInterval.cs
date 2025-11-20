using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Account;

public sealed class RetrievalInterval : IMessage<RetrievalInterval>, IMessage, IEquatable<RetrievalInterval>, IDeepCloneable<RetrievalInterval>, IBufferMessage
{
	private static readonly MessageParser<RetrievalInterval> _parser = new MessageParser<RetrievalInterval>(() => new RetrievalInterval());

	private UnknownFieldSet _unknownFields;

	public const int OrderFieldNumber = 1;

	private int order_;

	public const int DurationInSecondsFieldNumber = 2;

	private int durationInSeconds_;

	public const int DelayInSecondsFieldNumber = 3;

	private int delayInSeconds_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<RetrievalInterval> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => AccountContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Order
	{
		get
		{
			return order_;
		}
		set
		{
			order_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DurationInSeconds
	{
		get
		{
			return durationInSeconds_;
		}
		set
		{
			durationInSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DelayInSeconds
	{
		get
		{
			return delayInSeconds_;
		}
		set
		{
			delayInSeconds_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RetrievalInterval()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RetrievalInterval(RetrievalInterval other)
		: this()
	{
		order_ = other.order_;
		durationInSeconds_ = other.durationInSeconds_;
		delayInSeconds_ = other.delayInSeconds_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RetrievalInterval Clone()
	{
		return new RetrievalInterval(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as RetrievalInterval);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(RetrievalInterval other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Order != other.Order)
		{
			return false;
		}
		if (DurationInSeconds != other.DurationInSeconds)
		{
			return false;
		}
		if (DelayInSeconds != other.DelayInSeconds)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (Order != 0)
		{
			num ^= Order.GetHashCode();
		}
		if (DurationInSeconds != 0)
		{
			num ^= DurationInSeconds.GetHashCode();
		}
		if (DelayInSeconds != 0)
		{
			num ^= DelayInSeconds.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (Order != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(Order);
		}
		if (DurationInSeconds != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(DurationInSeconds);
		}
		if (DelayInSeconds != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(DelayInSeconds);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (Order != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Order);
		}
		if (DurationInSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DurationInSeconds);
		}
		if (DelayInSeconds != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DelayInSeconds);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(RetrievalInterval other)
	{
		if (other != null)
		{
			if (other.Order != 0)
			{
				Order = other.Order;
			}
			if (other.DurationInSeconds != 0)
			{
				DurationInSeconds = other.DurationInSeconds;
			}
			if (other.DelayInSeconds != 0)
			{
				DelayInSeconds = other.DelayInSeconds;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
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
				Order = input.ReadInt32();
				break;
			case 16u:
				DurationInSeconds = input.ReadInt32();
				break;
			case 24u:
				DelayInSeconds = input.ReadInt32();
				break;
			}
		}
	}
}
