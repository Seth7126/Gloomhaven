using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Infrastructure.Context;

public sealed class ConfigurationContextData : IMessage<ConfigurationContextData>, IMessage, IEquatable<ConfigurationContextData>, IDeepCloneable<ConfigurationContextData>, IBufferMessage
{
	private static readonly MessageParser<ConfigurationContextData> _parser = new MessageParser<ConfigurationContextData>(() => new ConfigurationContextData());

	private UnknownFieldSet _unknownFields;

	public const int TitleIdFieldNumber = 1;

	private string titleId_ = "";

	public const int ConfigurationHashFieldNumber = 2;

	private string configurationHash_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ConfigurationContextData> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ConfigurationContextReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string TitleId
	{
		get
		{
			return titleId_;
		}
		set
		{
			titleId_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ConfigurationHash
	{
		get
		{
			return configurationHash_;
		}
		set
		{
			configurationHash_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ConfigurationContextData()
	{
	}

	[DebuggerNonUserCode]
	public ConfigurationContextData(ConfigurationContextData other)
		: this()
	{
		titleId_ = other.titleId_;
		configurationHash_ = other.configurationHash_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ConfigurationContextData Clone()
	{
		return new ConfigurationContextData(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ConfigurationContextData);
	}

	[DebuggerNonUserCode]
	public bool Equals(ConfigurationContextData other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TitleId != other.TitleId)
		{
			return false;
		}
		if (ConfigurationHash != other.ConfigurationHash)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TitleId.Length != 0)
		{
			num ^= TitleId.GetHashCode();
		}
		if (ConfigurationHash.Length != 0)
		{
			num ^= ConfigurationHash.GetHashCode();
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
		if (TitleId.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteString(TitleId);
		}
		if (ConfigurationHash.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ConfigurationHash);
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
		if (TitleId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(TitleId);
		}
		if (ConfigurationHash.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ConfigurationHash);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ConfigurationContextData other)
	{
		if (other != null)
		{
			if (other.TitleId.Length != 0)
			{
				TitleId = other.TitleId;
			}
			if (other.ConfigurationHash.Length != 0)
			{
				ConfigurationHash = other.ConfigurationHash;
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
				TitleId = input.ReadString();
				break;
			case 18u:
				ConfigurationHash = input.ReadString();
				break;
			}
		}
	}
}
