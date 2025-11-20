using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.GameConfiguration;

public sealed class DownloadConfigurationComponentPacksToolResponse : IMessage<DownloadConfigurationComponentPacksToolResponse>, IMessage, IEquatable<DownloadConfigurationComponentPacksToolResponse>, IDeepCloneable<DownloadConfigurationComponentPacksToolResponse>, IBufferMessage
{
	private static readonly MessageParser<DownloadConfigurationComponentPacksToolResponse> _parser = new MessageParser<DownloadConfigurationComponentPacksToolResponse>(() => new DownloadConfigurationComponentPacksToolResponse());

	private UnknownFieldSet _unknownFields;

	public const int PacksFieldNumber = 1;

	private static readonly FieldCodec<ConfigurationComponentPack> _repeated_packs_codec = FieldCodec.ForMessage(10u, ConfigurationComponentPack.Parser);

	private readonly RepeatedField<ConfigurationComponentPack> packs_ = new RepeatedField<ConfigurationComponentPack>();

	[DebuggerNonUserCode]
	public static MessageParser<DownloadConfigurationComponentPacksToolResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => GameConfigurationManagementContractsReflection.Descriptor.MessageTypes[9];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<ConfigurationComponentPack> Packs => packs_;

	[DebuggerNonUserCode]
	public DownloadConfigurationComponentPacksToolResponse()
	{
	}

	[DebuggerNonUserCode]
	public DownloadConfigurationComponentPacksToolResponse(DownloadConfigurationComponentPacksToolResponse other)
		: this()
	{
		packs_ = other.packs_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public DownloadConfigurationComponentPacksToolResponse Clone()
	{
		return new DownloadConfigurationComponentPacksToolResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as DownloadConfigurationComponentPacksToolResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(DownloadConfigurationComponentPacksToolResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
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
		num += packs_.CalculateSize(_repeated_packs_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(DownloadConfigurationComponentPacksToolResponse other)
	{
		if (other != null)
		{
			packs_.Add(other.packs_);
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
			uint num2 = num;
			uint num3 = num2;
			if (num3 != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				packs_.AddEntriesFrom(ref input, _repeated_packs_codec);
			}
		}
	}
}
