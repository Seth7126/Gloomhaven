using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GameConfiguration;

public sealed class ConfigurationComponentDataItem : IMessage<ConfigurationComponentDataItem>, IMessage, IEquatable<ConfigurationComponentDataItem>, IDeepCloneable<ConfigurationComponentDataItem>, IBufferMessage
{
	private static readonly MessageParser<ConfigurationComponentDataItem> _parser = new MessageParser<ConfigurationComponentDataItem>(() => new ConfigurationComponentDataItem());

	private UnknownFieldSet _unknownFields;

	public const int NameFieldNumber = 1;

	private string name_ = "";

	public const int ContentFieldNumber = 2;

	private ByteString content_ = ByteString.Empty;

	public const int TypeFieldNumber = 3;

	private ComponentDataType type_ = ComponentDataType.Json;

	[DebuggerNonUserCode]
	public static MessageParser<ConfigurationComponentDataItem> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationManagementContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString Content
	{
		get
		{
			return content_;
		}
		set
		{
			content_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ComponentDataType Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentDataItem()
	{
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentDataItem(ConfigurationComponentDataItem other)
		: this()
	{
		name_ = other.name_;
		content_ = other.content_;
		type_ = other.type_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentDataItem Clone()
	{
		return new ConfigurationComponentDataItem(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ConfigurationComponentDataItem);
	}

	[DebuggerNonUserCode]
	public bool Equals(ConfigurationComponentDataItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Name != other.Name)
		{
			return false;
		}
		if (Content != other.Content)
		{
			return false;
		}
		if (Type != other.Type)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
		}
		if (Content.Length != 0)
		{
			num ^= Content.GetHashCode();
		}
		if (Type != ComponentDataType.Json)
		{
			num ^= Type.GetHashCode();
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
		if (Name.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(Name);
		}
		if (Content.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(Content);
		}
		if (Type != ComponentDataType.Json)
		{
			output.WriteRawTag(24);
			output.WriteEnum((int)Type);
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
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		if (Content.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Content);
		}
		if (Type != ComponentDataType.Json)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Type);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ConfigurationComponentDataItem other)
	{
		if (other != null)
		{
			if (other.Name.Length != 0)
			{
				Name = other.Name;
			}
			if (other.Content.Length != 0)
			{
				Content = other.Content;
			}
			if (other.Type != ComponentDataType.Json)
			{
				Type = other.Type;
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
				Name = input.ReadString();
				break;
			case 18u:
				Content = input.ReadBytes();
				break;
			case 24u:
				Type = (ComponentDataType)input.ReadEnum();
				break;
			}
		}
	}
}
