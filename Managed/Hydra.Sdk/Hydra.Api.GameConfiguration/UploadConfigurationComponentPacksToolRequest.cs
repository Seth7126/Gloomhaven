using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.GameConfiguration;

public sealed class UploadConfigurationComponentPacksToolRequest : IMessage<UploadConfigurationComponentPacksToolRequest>, IMessage, IEquatable<UploadConfigurationComponentPacksToolRequest>, IDeepCloneable<UploadConfigurationComponentPacksToolRequest>, IBufferMessage
{
	private static readonly MessageParser<UploadConfigurationComponentPacksToolRequest> _parser = new MessageParser<UploadConfigurationComponentPacksToolRequest>(() => new UploadConfigurationComponentPacksToolRequest());

	private UnknownFieldSet _unknownFields;

	public const int ToolContextFieldNumber = 1;

	private ToolContext toolContext_;

	public const int PacksFieldNumber = 2;

	private static readonly FieldCodec<ConfigurationComponentPack> _repeated_packs_codec = FieldCodec.ForMessage(18u, ConfigurationComponentPack.Parser);

	private readonly RepeatedField<ConfigurationComponentPack> packs_ = new RepeatedField<ConfigurationComponentPack>();

	[DebuggerNonUserCode]
	public static MessageParser<UploadConfigurationComponentPacksToolRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationManagementContractsReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ToolContext ToolContext
	{
		get
		{
			return toolContext_;
		}
		set
		{
			toolContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ConfigurationComponentPack> Packs => packs_;

	[DebuggerNonUserCode]
	public UploadConfigurationComponentPacksToolRequest()
	{
	}

	[DebuggerNonUserCode]
	public UploadConfigurationComponentPacksToolRequest(UploadConfigurationComponentPacksToolRequest other)
		: this()
	{
		toolContext_ = ((other.toolContext_ != null) ? other.toolContext_.Clone() : null);
		packs_ = other.packs_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UploadConfigurationComponentPacksToolRequest Clone()
	{
		return new UploadConfigurationComponentPacksToolRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UploadConfigurationComponentPacksToolRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UploadConfigurationComponentPacksToolRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ToolContext, other.ToolContext))
		{
			return false;
		}
		if (!packs_.Equals(other.packs_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (toolContext_ != null)
		{
			num ^= ToolContext.GetHashCode();
		}
		num ^= packs_.GetHashCode();
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
		if (toolContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(ToolContext);
		}
		packs_.WriteTo(ref output, _repeated_packs_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (toolContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ToolContext);
		}
		num += packs_.CalculateSize(_repeated_packs_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UploadConfigurationComponentPacksToolRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.toolContext_ != null)
		{
			if (toolContext_ == null)
			{
				ToolContext = new ToolContext();
			}
			ToolContext.MergeFrom(other.ToolContext);
		}
		packs_.Add(other.packs_);
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
				if (toolContext_ == null)
				{
					ToolContext = new ToolContext();
				}
				input.ReadMessage(ToolContext);
				break;
			case 18u:
				packs_.AddEntriesFrom(ref input, _repeated_packs_codec);
				break;
			}
		}
	}
}
