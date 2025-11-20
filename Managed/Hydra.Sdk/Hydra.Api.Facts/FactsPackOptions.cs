using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Facts;

public sealed class FactsPackOptions : IMessage<FactsPackOptions>, IMessage, IEquatable<FactsPackOptions>, IDeepCloneable<FactsPackOptions>, IBufferMessage
{
	private static readonly MessageParser<FactsPackOptions> _parser = new MessageParser<FactsPackOptions>(() => new FactsPackOptions());

	private UnknownFieldSet _unknownFields;

	public const int IsFinalFieldNumber = 1;

	private bool isFinal_;

	public const int IsLocalTimeFieldNumber = 2;

	private bool isLocalTime_;

	public const int IsCompressedFieldNumber = 3;

	private bool isCompressed_;

	[DebuggerNonUserCode]
	public static MessageParser<FactsPackOptions> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => FactsContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
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
	public FactsPackOptions()
	{
	}

	[DebuggerNonUserCode]
	public FactsPackOptions(FactsPackOptions other)
		: this()
	{
		isFinal_ = other.isFinal_;
		isLocalTime_ = other.isLocalTime_;
		isCompressed_ = other.isCompressed_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public FactsPackOptions Clone()
	{
		return new FactsPackOptions(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as FactsPackOptions);
	}

	[DebuggerNonUserCode]
	public bool Equals(FactsPackOptions other)
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
	public void MergeFrom(FactsPackOptions other)
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
