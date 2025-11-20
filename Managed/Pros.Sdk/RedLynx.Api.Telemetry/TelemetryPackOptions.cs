using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Telemetry;

public sealed class TelemetryPackOptions : IMessage<TelemetryPackOptions>, IMessage, IEquatable<TelemetryPackOptions>, IDeepCloneable<TelemetryPackOptions>, IBufferMessage
{
	private static readonly MessageParser<TelemetryPackOptions> _parser = new MessageParser<TelemetryPackOptions>(() => new TelemetryPackOptions());

	private UnknownFieldSet _unknownFields;

	public const int IsFinalFieldNumber = 1;

	private bool isFinal_;

	public const int IsLocalTimeFieldNumber = 2;

	private bool isLocalTime_;

	public const int IsCompressedFieldNumber = 3;

	private bool isCompressed_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<TelemetryPackOptions> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TelemetryContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool IsFinal
	{
		get
		{
			return isFinal_;
		}
		set
		{
			isFinal_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool IsLocalTime
	{
		get
		{
			return isLocalTime_;
		}
		set
		{
			isLocalTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool IsCompressed
	{
		get
		{
			return isCompressed_;
		}
		set
		{
			isCompressed_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TelemetryPackOptions()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TelemetryPackOptions(TelemetryPackOptions other)
		: this()
	{
		isFinal_ = other.isFinal_;
		isLocalTime_ = other.isLocalTime_;
		isCompressed_ = other.isCompressed_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TelemetryPackOptions Clone()
	{
		return new TelemetryPackOptions(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as TelemetryPackOptions);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(TelemetryPackOptions other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (IsFinal != other.IsFinal)
		{
			return false;
		}
		if (IsLocalTime != other.IsLocalTime)
		{
			return false;
		}
		if (IsCompressed != other.IsCompressed)
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
		if (IsFinal)
		{
			num ^= IsFinal.GetHashCode();
		}
		if (IsLocalTime)
		{
			num ^= IsLocalTime.GetHashCode();
		}
		if (IsCompressed)
		{
			num ^= IsCompressed.GetHashCode();
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
		if (IsFinal)
		{
			output.WriteRawTag(8);
			output.WriteBool(IsFinal);
		}
		if (IsLocalTime)
		{
			output.WriteRawTag(16);
			output.WriteBool(IsLocalTime);
		}
		if (IsCompressed)
		{
			output.WriteRawTag(24);
			output.WriteBool(IsCompressed);
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
		if (IsFinal)
		{
			num += 2;
		}
		if (IsLocalTime)
		{
			num += 2;
		}
		if (IsCompressed)
		{
			num += 2;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(TelemetryPackOptions other)
	{
		if (other != null)
		{
			if (other.IsFinal)
			{
				IsFinal = other.IsFinal;
			}
			if (other.IsLocalTime)
			{
				IsLocalTime = other.IsLocalTime;
			}
			if (other.IsCompressed)
			{
				IsCompressed = other.IsCompressed;
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
				IsFinal = input.ReadBool();
				break;
			case 16u:
				IsLocalTime = input.ReadBool();
				break;
			case 24u:
				IsCompressed = input.ReadBool();
				break;
			}
		}
	}
}
