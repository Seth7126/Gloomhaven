using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Telemetry;

public sealed class TelemetryPackOptions : IMessage<TelemetryPackOptions>, IMessage, IEquatable<TelemetryPackOptions>, IDeepCloneable<TelemetryPackOptions>, IBufferMessage
{
	private static readonly MessageParser<TelemetryPackOptions> _parser = new MessageParser<TelemetryPackOptions>(() => new TelemetryPackOptions());

	private UnknownFieldSet _unknownFields;

	public const int IsLocalTimeFieldNumber = 2;

	private bool isLocalTime_;

	public const int CompressionFieldNumber = 4;

	private TelemetryPackCompression compression_ = TelemetryPackCompression.None;

	[DebuggerNonUserCode]
	public static MessageParser<TelemetryPackOptions> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => TelemetryContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
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
	public TelemetryPackCompression Compression
	{
		get
		{
			return compression_;
		}
		set
		{
			compression_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TelemetryPackOptions()
	{
	}

	[DebuggerNonUserCode]
	public TelemetryPackOptions(TelemetryPackOptions other)
		: this()
	{
		isLocalTime_ = other.isLocalTime_;
		compression_ = other.compression_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TelemetryPackOptions Clone()
	{
		return new TelemetryPackOptions(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TelemetryPackOptions);
	}

	[DebuggerNonUserCode]
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
		if (IsLocalTime != other.IsLocalTime)
		{
			return false;
		}
		if (Compression != other.Compression)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (IsLocalTime)
		{
			num ^= IsLocalTime.GetHashCode();
		}
		if (Compression != TelemetryPackCompression.None)
		{
			num ^= Compression.GetHashCode();
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
		if (IsLocalTime)
		{
			output.WriteRawTag(16);
			output.WriteBool(IsLocalTime);
		}
		if (Compression != TelemetryPackCompression.None)
		{
			output.WriteRawTag(32);
			output.WriteEnum((int)Compression);
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
		if (IsLocalTime)
		{
			num += 2;
		}
		if (Compression != TelemetryPackCompression.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Compression);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TelemetryPackOptions other)
	{
		if (other != null)
		{
			if (other.IsLocalTime)
			{
				IsLocalTime = other.IsLocalTime;
			}
			if (other.Compression != TelemetryPackCompression.None)
			{
				Compression = other.Compression;
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
			case 16u:
				IsLocalTime = input.ReadBool();
				break;
			case 32u:
				Compression = (TelemetryPackCompression)input.ReadEnum();
				break;
			}
		}
	}
}
