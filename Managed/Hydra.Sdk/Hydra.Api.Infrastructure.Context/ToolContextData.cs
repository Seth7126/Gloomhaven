using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public sealed class ToolContextData : IMessage<ToolContextData>, IMessage, IEquatable<ToolContextData>, IDeepCloneable<ToolContextData>, IBufferMessage
{
	private static readonly MessageParser<ToolContextData> _parser = new MessageParser<ToolContextData>(() => new ToolContextData());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int KernelSessionIdFieldNumber = 2;

	private string kernelSessionId_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ToolContextData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ToolContextReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string KernelSessionId
	{
		get
		{
			return kernelSessionId_;
		}
		set
		{
			kernelSessionId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ToolContextData()
	{
	}

	[DebuggerNonUserCode]
	public ToolContextData(ToolContextData other)
		: this()
	{
		titleId_ = other.titleId_;
		kernelSessionId_ = other.kernelSessionId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ToolContextData Clone()
	{
		return new ToolContextData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ToolContextData);
	}

	[DebuggerNonUserCode]
	public bool Equals(ToolContextData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (KernelSessionId != other.KernelSessionId)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (KernelSessionId.Length != 0)
		{
			num ^= KernelSessionId.GetHashCode();
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
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TitleId);
		}
		if (KernelSessionId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(KernelSessionId);
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
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (KernelSessionId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(KernelSessionId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ToolContextData other)
	{
		if (other != null)
		{
			if (other.TitleId.Length != 0)
			{
				TitleId = other.TitleId;
			}
			if (other.KernelSessionId.Length != 0)
			{
				KernelSessionId = other.KernelSessionId;
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
				TitleId = input.ReadString();
				break;
			case 18u:
				KernelSessionId = input.ReadString();
				break;
			}
		}
	}
}
