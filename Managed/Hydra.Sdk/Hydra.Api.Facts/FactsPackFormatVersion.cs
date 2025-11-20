using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Facts;

public sealed class FactsPackFormatVersion : IMessage<FactsPackFormatVersion>, IMessage, IEquatable<FactsPackFormatVersion>, IDeepCloneable<FactsPackFormatVersion>, IBufferMessage
{
	private static readonly MessageParser<FactsPackFormatVersion> _parser = new MessageParser<FactsPackFormatVersion>(() => new FactsPackFormatVersion());

	private UnknownFieldSet _unknownFields;

	public const int MajorFieldNumber = 1;

	private int major_;

	public const int MinorFieldNumber = 2;

	private int minor_;

	[DebuggerNonUserCode]
	public static MessageParser<FactsPackFormatVersion> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => FactsContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int Major
	{
		get
		{
			return major_;
		}
		set
		{
			major_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Minor
	{
		get
		{
			return minor_;
		}
		set
		{
			minor_ = value;
		}
	}

	[DebuggerNonUserCode]
	public FactsPackFormatVersion()
	{
	}

	[DebuggerNonUserCode]
	public FactsPackFormatVersion(FactsPackFormatVersion other)
		: this()
	{
		major_ = other.major_;
		minor_ = other.minor_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public FactsPackFormatVersion Clone()
	{
		return new FactsPackFormatVersion(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as FactsPackFormatVersion);
	}

	[DebuggerNonUserCode]
	public bool Equals(FactsPackFormatVersion other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Major != other.Major)
		{
			return false;
		}
		if (Minor != other.Minor)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Major != 0)
		{
			num ^= Major.GetHashCode();
		}
		if (Minor != 0)
		{
			num ^= Minor.GetHashCode();
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
		if (Major != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(Major);
		}
		if (Minor != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Minor);
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
		if (Major != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Major);
		}
		if (Minor != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Minor);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(FactsPackFormatVersion other)
	{
		if (other != null)
		{
			if (other.Major != 0)
			{
				Major = other.Major;
			}
			if (other.Minor != 0)
			{
				Minor = other.Minor;
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
			case 8u:
				Major = input.ReadInt32();
				break;
			case 16u:
				Minor = input.ReadInt32();
				break;
			}
		}
	}
}
