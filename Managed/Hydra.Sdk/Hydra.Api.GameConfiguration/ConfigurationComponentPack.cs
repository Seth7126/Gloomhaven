using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GameConfiguration;

public sealed class ConfigurationComponentPack : IMessage<ConfigurationComponentPack>, IMessage, IEquatable<ConfigurationComponentPack>, IDeepCloneable<ConfigurationComponentPack>, IBufferMessage
{
	private static readonly MessageParser<ConfigurationComponentPack> _parser = new MessageParser<ConfigurationComponentPack>(() => new ConfigurationComponentPack());

	private UnknownFieldSet _unknownFields;

	public const int SchemasFieldNumber = 1;

	private static readonly FieldCodec<ConfigurationComponentDataItem> _repeated_schemas_codec = FieldCodec.ForMessage(10u, ConfigurationComponentDataItem.Parser);

	private readonly RepeatedField<ConfigurationComponentDataItem> schemas_ = new RepeatedField<ConfigurationComponentDataItem>();

	public const int DataFieldNumber = 2;

	private ConfigurationComponentDataItem data_;

	[DebuggerNonUserCode]
	public static MessageParser<ConfigurationComponentPack> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationManagementContractsReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<ConfigurationComponentDataItem> Schemas => schemas_;

	[DebuggerNonUserCode]
	public ConfigurationComponentDataItem Data
	{
		get
		{
			return data_;
		}
		set
		{
			data_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentPack()
	{
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentPack(ConfigurationComponentPack other)
		: this()
	{
		schemas_ = other.schemas_.Clone();
		data_ = ((other.data_ != null) ? other.data_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentPack Clone()
	{
		return new ConfigurationComponentPack(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ConfigurationComponentPack);
	}

	[DebuggerNonUserCode]
	public bool Equals(ConfigurationComponentPack other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!schemas_.Equals(other.schemas_))
		{
			return false;
		}
		if (!object.Equals(Data, other.Data))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= schemas_.GetHashCode();
		if (data_ != null)
		{
			num ^= Data.GetHashCode();
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
		schemas_.WriteTo(ref output, _repeated_schemas_codec);
		if (data_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Data);
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
		num += schemas_.CalculateSize(_repeated_schemas_codec);
		if (data_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Data);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ConfigurationComponentPack other)
	{
		if (other == null)
		{
			return;
		}
		schemas_.Add(other.schemas_);
		if (other.data_ != null)
		{
			if (data_ == null)
			{
				Data = new ConfigurationComponentDataItem();
			}
			Data.MergeFrom(other.Data);
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
				schemas_.AddEntriesFrom(ref input, _repeated_schemas_codec);
				break;
			case 18u:
				if (data_ == null)
				{
					Data = new ConfigurationComponentDataItem();
				}
				input.ReadMessage(Data);
				break;
			}
		}
	}
}
