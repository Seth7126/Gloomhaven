using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Telemetry;

public sealed class TelemetryPackHeader : IMessage<TelemetryPackHeader>, IMessage, IEquatable<TelemetryPackHeader>, IDeepCloneable<TelemetryPackHeader>, IBufferMessage
{
	private static readonly MessageParser<TelemetryPackHeader> _parser = new MessageParser<TelemetryPackHeader>(() => new TelemetryPackHeader());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private static readonly FieldCodec<TelemetryContext> _repeated_context_codec = FieldCodec.ForMessage(10u, TelemetryContext.Parser);

	private readonly RepeatedField<TelemetryContext> context_ = new RepeatedField<TelemetryContext>();

	public const int PackNumberFieldNumber = 2;

	private int packNumber_;

	public const int StartTimeFieldNumber = 3;

	private long startTime_;

	public const int EndTimeFieldNumber = 4;

	private long endTime_;

	public const int InitTimeFieldNumber = 5;

	private long initTime_;

	public const int FormatFieldNumber = 7;

	private TelemetryPackFormat format_ = TelemetryPackFormat.Unknown;

	public const int VersionFieldNumber = 8;

	private TelemetryPackFormatVersion version_;

	public const int OptionsFieldNumber = 9;

	private TelemetryPackOptions options_;

	[DebuggerNonUserCode]
	public static MessageParser<TelemetryPackHeader> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => TelemetryContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<TelemetryContext> Context => context_;

	[DebuggerNonUserCode]
	public int PackNumber
	{
		get
		{
			return packNumber_;
		}
		set
		{
			packNumber_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long StartTime
	{
		get
		{
			return startTime_;
		}
		set
		{
			startTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long EndTime
	{
		get
		{
			return endTime_;
		}
		set
		{
			endTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long InitTime
	{
		get
		{
			return initTime_;
		}
		set
		{
			initTime_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TelemetryPackFormat Format
	{
		get
		{
			return format_;
		}
		set
		{
			format_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TelemetryPackFormatVersion Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TelemetryPackOptions Options
	{
		get
		{
			return options_;
		}
		set
		{
			options_ = value;
		}
	}

	[DebuggerNonUserCode]
	public TelemetryPackHeader()
	{
	}

	[DebuggerNonUserCode]
	public TelemetryPackHeader(TelemetryPackHeader other)
		: this()
	{
		context_ = other.context_.Clone();
		packNumber_ = other.packNumber_;
		startTime_ = other.startTime_;
		endTime_ = other.endTime_;
		initTime_ = other.initTime_;
		format_ = other.format_;
		version_ = ((other.version_ != null) ? other.version_.Clone() : null);
		options_ = ((other.options_ != null) ? other.options_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public TelemetryPackHeader Clone()
	{
		return new TelemetryPackHeader(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as TelemetryPackHeader);
	}

	[DebuggerNonUserCode]
	public bool Equals(TelemetryPackHeader other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!context_.Equals(other.context_))
		{
			return false;
		}
		if (PackNumber != other.PackNumber)
		{
			return false;
		}
		if (StartTime != other.StartTime)
		{
			return false;
		}
		if (EndTime != other.EndTime)
		{
			return false;
		}
		if (InitTime != other.InitTime)
		{
			return false;
		}
		if (Format != other.Format)
		{
			return false;
		}
		if (!object.Equals(Version, other.Version))
		{
			return false;
		}
		if (!object.Equals(Options, other.Options))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= context_.GetHashCode();
		if (PackNumber != 0)
		{
			num ^= PackNumber.GetHashCode();
		}
		if (StartTime != 0)
		{
			num ^= StartTime.GetHashCode();
		}
		if (EndTime != 0)
		{
			num ^= EndTime.GetHashCode();
		}
		if (InitTime != 0)
		{
			num ^= InitTime.GetHashCode();
		}
		if (Format != TelemetryPackFormat.Unknown)
		{
			num ^= Format.GetHashCode();
		}
		if (version_ != null)
		{
			num ^= Version.GetHashCode();
		}
		if (options_ != null)
		{
			num ^= Options.GetHashCode();
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
		context_.WriteTo(ref output, _repeated_context_codec);
		if (PackNumber != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(PackNumber);
		}
		if (StartTime != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt64(StartTime);
		}
		if (EndTime != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt64(EndTime);
		}
		if (InitTime != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt64(InitTime);
		}
		if (Format != TelemetryPackFormat.Unknown)
		{
			output.WriteRawTag(56);
			output.WriteEnum((int)Format);
		}
		if (version_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(Version);
		}
		if (options_ != null)
		{
			output.WriteRawTag(74);
			output.WriteMessage(Options);
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
		num += context_.CalculateSize(_repeated_context_codec);
		if (PackNumber != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(PackNumber);
		}
		if (StartTime != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTime);
		}
		if (EndTime != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTime);
		}
		if (InitTime != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(InitTime);
		}
		if (Format != TelemetryPackFormat.Unknown)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Format);
		}
		if (version_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Version);
		}
		if (options_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Options);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(TelemetryPackHeader other)
	{
		if (other == null)
		{
			return;
		}
		context_.Add(other.context_);
		if (other.PackNumber != 0)
		{
			PackNumber = other.PackNumber;
		}
		if (other.StartTime != 0)
		{
			StartTime = other.StartTime;
		}
		if (other.EndTime != 0)
		{
			EndTime = other.EndTime;
		}
		if (other.InitTime != 0)
		{
			InitTime = other.InitTime;
		}
		if (other.Format != TelemetryPackFormat.Unknown)
		{
			Format = other.Format;
		}
		if (other.version_ != null)
		{
			if (version_ == null)
			{
				Version = new TelemetryPackFormatVersion();
			}
			Version.MergeFrom(other.Version);
		}
		if (other.options_ != null)
		{
			if (options_ == null)
			{
				Options = new TelemetryPackOptions();
			}
			Options.MergeFrom(other.Options);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				context_.AddEntriesFrom(ref input, _repeated_context_codec);
				break;
			case 16u:
				PackNumber = input.ReadInt32();
				break;
			case 24u:
				StartTime = input.ReadInt64();
				break;
			case 32u:
				EndTime = input.ReadInt64();
				break;
			case 40u:
				InitTime = input.ReadInt64();
				break;
			case 56u:
				Format = (TelemetryPackFormat)input.ReadEnum();
				break;
			case 66u:
				if (version_ == null)
				{
					Version = new TelemetryPackFormatVersion();
				}
				input.ReadMessage(Version);
				break;
			case 74u:
				if (options_ == null)
				{
					Options = new TelemetryPackOptions();
				}
				input.ReadMessage(Options);
				break;
			}
		}
	}
}
