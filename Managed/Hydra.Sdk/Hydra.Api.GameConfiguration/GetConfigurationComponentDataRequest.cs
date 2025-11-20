using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GameConfiguration;

public sealed class GetConfigurationComponentDataRequest : IMessage<GetConfigurationComponentDataRequest>, IMessage, IEquatable<GetConfigurationComponentDataRequest>, IDeepCloneable<GetConfigurationComponentDataRequest>, IBufferMessage
{
	private static readonly MessageParser<GetConfigurationComponentDataRequest> _parser = new MessageParser<GetConfigurationComponentDataRequest>(() => new GetConfigurationComponentDataRequest());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ConfigurationContext context_;

	public const int ComponentsFieldNumber = 2;

	private static readonly FieldCodec<ConfigurationComponent> _repeated_components_codec = FieldCodec.ForMessage(18u, ConfigurationComponent.Parser);

	private readonly RepeatedField<ConfigurationComponent> components_ = new RepeatedField<ConfigurationComponent>();

	[DebuggerNonUserCode]
	public static MessageParser<GetConfigurationComponentDataRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationContractsReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ConfigurationContext Context
	{
		get
		{
			return context_;
		}
		set
		{
			context_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ConfigurationComponent> Components => components_;

	[DebuggerNonUserCode]
	public GetConfigurationComponentDataRequest()
	{
	}

	[DebuggerNonUserCode]
	public GetConfigurationComponentDataRequest(GetConfigurationComponentDataRequest other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		components_ = other.components_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetConfigurationComponentDataRequest Clone()
	{
		return new GetConfigurationComponentDataRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetConfigurationComponentDataRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetConfigurationComponentDataRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(Context, other.Context))
		{
			return false;
		}
		if (!components_.Equals(other.components_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (context_ != null)
		{
			num ^= Context.GetHashCode();
		}
		num ^= components_.GetHashCode();
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
		if (context_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(Context);
		}
		components_.WriteTo(ref output, _repeated_components_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (context_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Context);
		}
		num += components_.CalculateSize(_repeated_components_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetConfigurationComponentDataRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.context_ != null)
		{
			if (context_ == null)
			{
				Context = new ConfigurationContext();
			}
			Context.MergeFrom(other.Context);
		}
		components_.Add(other.components_);
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
				if (context_ == null)
				{
					Context = new ConfigurationContext();
				}
				input.ReadMessage(Context);
				break;
			case 18u:
				components_.AddEntriesFrom(ref input, _repeated_components_codec);
				break;
			}
		}
	}
}
