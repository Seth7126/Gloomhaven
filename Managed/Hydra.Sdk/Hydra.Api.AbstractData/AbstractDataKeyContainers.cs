using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.AbstractData;

public sealed class AbstractDataKeyContainers : IMessage<AbstractDataKeyContainers>, IMessage, IEquatable<AbstractDataKeyContainers>, IDeepCloneable<AbstractDataKeyContainers>, IBufferMessage
{
	private static readonly MessageParser<AbstractDataKeyContainers> _parser = new MessageParser<AbstractDataKeyContainers>(() => new AbstractDataKeyContainers());

	private UnknownFieldSet _unknownFields;

	public const int KeyFieldNumber = 1;

	private string key_ = "";

	public const int ContainersFieldNumber = 2;

	private static readonly FieldCodec<AbstractDataContainerData> _repeated_containers_codec = FieldCodec.ForMessage(18u, AbstractDataContainerData.Parser);

	private readonly RepeatedField<AbstractDataContainerData> containers_ = new RepeatedField<AbstractDataContainerData>();

	[DebuggerNonUserCode]
	public static MessageParser<AbstractDataKeyContainers> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => AbstractDataServiceContractsReflection.Descriptor.MessageTypes[2];

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
	public RepeatedField<AbstractDataContainerData> Containers => containers_;

	[DebuggerNonUserCode]
	public AbstractDataKeyContainers()
	{
	}

	[DebuggerNonUserCode]
	public AbstractDataKeyContainers(AbstractDataKeyContainers other)
		: this()
	{
		key_ = other.key_;
		containers_ = other.containers_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public AbstractDataKeyContainers Clone()
	{
		return new AbstractDataKeyContainers(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as AbstractDataKeyContainers);
	}

	[DebuggerNonUserCode]
	public bool Equals(AbstractDataKeyContainers other)
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
		if (!containers_.Equals(other.containers_))
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
		num ^= containers_.GetHashCode();
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
		containers_.WriteTo(ref output, _repeated_containers_codec);
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
		num += containers_.CalculateSize(_repeated_containers_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(AbstractDataKeyContainers other)
	{
		if (other != null)
		{
			if (other.Key.Length != 0)
			{
				Key = other.Key;
			}
			containers_.Add(other.containers_);
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
				containers_.AddEntriesFrom(ref input, _repeated_containers_codec);
				break;
			}
		}
	}
}
