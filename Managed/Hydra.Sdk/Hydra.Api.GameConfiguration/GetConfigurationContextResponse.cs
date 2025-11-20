using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GameConfiguration;

public sealed class GetConfigurationContextResponse : IMessage<GetConfigurationContextResponse>, IMessage, IEquatable<GetConfigurationContextResponse>, IDeepCloneable<GetConfigurationContextResponse>, IBufferMessage
{
	private static readonly MessageParser<GetConfigurationContextResponse> _parser = new MessageParser<GetConfigurationContextResponse>(() => new GetConfigurationContextResponse());

	private UnknownFieldSet _unknownFields;

	public const int ContextFieldNumber = 1;

	private ConfigurationContext context_;

	public const int SnapshotsFieldNumber = 2;

	private static readonly FieldCodec<ConfigurationComponentSnapshot> _repeated_snapshots_codec = FieldCodec.ForMessage(18u, ConfigurationComponentSnapshot.Parser);

	private readonly RepeatedField<ConfigurationComponentSnapshot> snapshots_ = new RepeatedField<ConfigurationComponentSnapshot>();

	[DebuggerNonUserCode]
	public static MessageParser<GetConfigurationContextResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationContractsReflection.Descriptor.MessageTypes[1];

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
	public RepeatedField<ConfigurationComponentSnapshot> Snapshots => snapshots_;

	[DebuggerNonUserCode]
	public GetConfigurationContextResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetConfigurationContextResponse(GetConfigurationContextResponse other)
		: this()
	{
		context_ = ((other.context_ != null) ? other.context_.Clone() : null);
		snapshots_ = other.snapshots_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetConfigurationContextResponse Clone()
	{
		return new GetConfigurationContextResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetConfigurationContextResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetConfigurationContextResponse other)
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
		if (!snapshots_.Equals(other.snapshots_))
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
		num ^= snapshots_.GetHashCode();
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
		snapshots_.WriteTo(ref output, _repeated_snapshots_codec);
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
		num += snapshots_.CalculateSize(_repeated_snapshots_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetConfigurationContextResponse other)
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
		snapshots_.Add(other.snapshots_);
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
				snapshots_.AddEntriesFrom(ref input, _repeated_snapshots_codec);
				break;
			}
		}
	}
}
