using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.AbstractData;

public sealed class AbstractDataKeyContainerNames : IMessage<AbstractDataKeyContainerNames>, IMessage, IEquatable<AbstractDataKeyContainerNames>, IDeepCloneable<AbstractDataKeyContainerNames>, IBufferMessage
{
	private static readonly MessageParser<AbstractDataKeyContainerNames> _parser = new MessageParser<AbstractDataKeyContainerNames>(() => new AbstractDataKeyContainerNames());

	private UnknownFieldSet _unknownFields;

	public const int KeyFieldNumber = 1;

	private string key_ = "";

	public const int ContainerNamesFieldNumber = 2;

	private static readonly FieldCodec<string> _repeated_containerNames_codec = FieldCodec.ForString(18u);

	private readonly RepeatedField<string> containerNames_ = new RepeatedField<string>();

	[DebuggerNonUserCode]
	public static MessageParser<AbstractDataKeyContainerNames> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[3];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Key
	{
		get
		{
			return key_;
		}
		set
		{
			key_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<string> ContainerNames => containerNames_;

	[DebuggerNonUserCode]
	public AbstractDataKeyContainerNames()
	{
	}

	[DebuggerNonUserCode]
	public AbstractDataKeyContainerNames(AbstractDataKeyContainerNames other)
		: this()
	{
		key_ = other.key_;
		containerNames_ = other.containerNames_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public AbstractDataKeyContainerNames Clone()
	{
		return new AbstractDataKeyContainerNames(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as AbstractDataKeyContainerNames);
	}

	[DebuggerNonUserCode]
	public bool Equals(AbstractDataKeyContainerNames other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Key != other.Key)
		{
			return false;
		}
		if (!containerNames_.Equals(other.containerNames_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Key.Length != 0)
		{
			num ^= Key.GetHashCode();
		}
		num ^= containerNames_.GetHashCode();
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
		if (Key.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Key);
		}
		containerNames_.WriteTo(ref output, _repeated_containerNames_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Key.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Key);
		}
		num += containerNames_.CalculateSize(_repeated_containerNames_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(AbstractDataKeyContainerNames other)
	{
		if (other != null)
		{
			if (other.Key.Length != 0)
			{
				Key = other.Key;
			}
			containerNames_.Add(other.containerNames_);
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
				Key = input.ReadString();
				break;
			case 18u:
				containerNames_.AddEntriesFrom(ref input, _repeated_containerNames_codec);
				break;
			}
		}
	}
}
