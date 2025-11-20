using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GameConfiguration;

public sealed class ConfigurationComponentSnapshot : IMessage<ConfigurationComponentSnapshot>, IMessage, IEquatable<ConfigurationComponentSnapshot>, IDeepCloneable<ConfigurationComponentSnapshot>, IBufferMessage
{
	private static readonly MessageParser<ConfigurationComponentSnapshot> _parser = new MessageParser<ConfigurationComponentSnapshot>(() => new ConfigurationComponentSnapshot());

	private UnknownFieldSet _unknownFields;

	public const int ComponentFieldNumber = 1;

	private ConfigurationComponent component_;

	public const int ComponentHashFieldNumber = 2;

	private string componentHash_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<ConfigurationComponentSnapshot> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ConfigurationComponent Component
	{
		get
		{
			return component_;
		}
		set
		{
			component_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string ComponentHash
	{
		get
		{
			return componentHash_;
		}
		set
		{
			componentHash_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentSnapshot()
	{
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentSnapshot(ConfigurationComponentSnapshot other)
		: this()
	{
		component_ = ((other.component_ != null) ? other.component_.Clone() : null);
		componentHash_ = other.componentHash_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ConfigurationComponentSnapshot Clone()
	{
		return new ConfigurationComponentSnapshot(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ConfigurationComponentSnapshot);
	}

	[DebuggerNonUserCode]
	public bool Equals(ConfigurationComponentSnapshot other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Component, other.Component))
		{
			return false;
		}
		if (ComponentHash != other.ComponentHash)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (component_ != null)
		{
			num ^= Component.GetHashCode();
		}
		if (ComponentHash.Length != 0)
		{
			num ^= ComponentHash.GetHashCode();
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
		if (component_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Component);
		}
		if (ComponentHash.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(ComponentHash);
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
		if (component_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Component);
		}
		if (ComponentHash.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(ComponentHash);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ConfigurationComponentSnapshot other)
	{
		if (other == null)
		{
			return;
		}
		if (other.component_ != null)
		{
			if (component_ == null)
			{
				Component = new ConfigurationComponent();
			}
			Component.MergeFrom(other.Component);
		}
		if (other.ComponentHash.Length != 0)
		{
			ComponentHash = other.ComponentHash;
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
				if (component_ == null)
				{
					Component = new ConfigurationComponent();
				}
				input.ReadMessage(Component);
				break;
			case 18u:
				ComponentHash = input.ReadString();
				break;
			}
		}
	}
}
