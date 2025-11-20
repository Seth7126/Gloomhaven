using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace RedLynx.Api.Banner;

public sealed class BannerConfig : IMessage<BannerConfig>, IMessage, IEquatable<BannerConfig>, IDeepCloneable<BannerConfig>, IBufferMessage
{
	private static readonly MessageParser<BannerConfig> _parser = new MessageParser<BannerConfig>(() => new BannerConfig());

	private UnknownFieldSet _unknownFields;

	public const int DisplayDurationMsFieldNumber = 1;

	private int displayDurationMs_;

	public const int MinSeverityDisplayFieldNumber = 2;

	private BannerSeverity minSeverityDisplay_;

	public const int SystemModeFieldNumber = 3;

	private BannerSystemMode systemMode_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BannerConfig> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => BannerContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DisplayDurationMs
	{
		get
		{
			return displayDurationMs_;
		}
		set
		{
			displayDurationMs_ = value;
		}
	}

	[Obsolete]
	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerSeverity MinSeverityDisplay
	{
		get
		{
			return minSeverityDisplay_;
		}
		set
		{
			minSeverityDisplay_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerSystemMode SystemMode
	{
		get
		{
			return systemMode_;
		}
		set
		{
			systemMode_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerConfig()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerConfig(BannerConfig other)
		: this()
	{
		displayDurationMs_ = other.displayDurationMs_;
		minSeverityDisplay_ = other.minSeverityDisplay_;
		systemMode_ = other.systemMode_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BannerConfig Clone()
	{
		return new BannerConfig(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BannerConfig);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BannerConfig other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (DisplayDurationMs != other.DisplayDurationMs)
		{
			return false;
		}
		if (MinSeverityDisplay != other.MinSeverityDisplay)
		{
			return false;
		}
		if (SystemMode != other.SystemMode)
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
		if (DisplayDurationMs != 0)
		{
			num ^= DisplayDurationMs.GetHashCode();
		}
		if (MinSeverityDisplay != BannerSeverity.None)
		{
			num ^= MinSeverityDisplay.GetHashCode();
		}
		if (SystemMode != BannerSystemMode.Normal)
		{
			num ^= SystemMode.GetHashCode();
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
		if (DisplayDurationMs != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(DisplayDurationMs);
		}
		if (MinSeverityDisplay != BannerSeverity.None)
		{
			output.WriteRawTag(16);
			output.WriteEnum((int)MinSeverityDisplay);
		}
		if (SystemMode != BannerSystemMode.Normal)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)SystemMode);
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
		if (DisplayDurationMs != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DisplayDurationMs);
		}
		if (MinSeverityDisplay != BannerSeverity.None)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)MinSeverityDisplay);
		}
		if (SystemMode != BannerSystemMode.Normal)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)SystemMode);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BannerConfig other)
	{
		if (other != null)
		{
			if (other.DisplayDurationMs != 0)
			{
				DisplayDurationMs = other.DisplayDurationMs;
			}
			if (other.MinSeverityDisplay != BannerSeverity.None)
			{
				MinSeverityDisplay = other.MinSeverityDisplay;
			}
			if (other.SystemMode != BannerSystemMode.Normal)
			{
				SystemMode = other.SystemMode;
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
				DisplayDurationMs = input.ReadInt32();
				break;
			case 16u:
				MinSeverityDisplay = (BannerSeverity)input.ReadEnum();
				break;
			case 24u:
				SystemMode = (BannerSystemMode)input.ReadEnum();
				break;
			}
		}
	}
}
