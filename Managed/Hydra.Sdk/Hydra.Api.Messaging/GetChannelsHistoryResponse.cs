using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class GetChannelsHistoryResponse : IMessage<GetChannelsHistoryResponse>, IMessage, IEquatable<GetChannelsHistoryResponse>, IDeepCloneable<GetChannelsHistoryResponse>, IBufferMessage
{
	private static readonly MessageParser<GetChannelsHistoryResponse> _parser = new MessageParser<GetChannelsHistoryResponse>(() => new GetChannelsHistoryResponse());

	private UnknownFieldSet _unknownFields;

	public const int ChannelHistoriesFieldNumber = 1;

	private static readonly FieldCodec<ChannelHistory> _repeated_channelHistories_codec = FieldCodec.ForMessage(10u, ChannelHistory.Parser);

	private readonly RepeatedField<ChannelHistory> channelHistories_ = new RepeatedField<ChannelHistory>();

	[DebuggerNonUserCode]
	public static MessageParser<GetChannelsHistoryResponse> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[19];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<ChannelHistory> ChannelHistories => channelHistories_;

	[DebuggerNonUserCode]
	public GetChannelsHistoryResponse()
	{
	}

	[DebuggerNonUserCode]
	public GetChannelsHistoryResponse(GetChannelsHistoryResponse other)
		: this()
	{
		channelHistories_ = other.channelHistories_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public GetChannelsHistoryResponse Clone()
	{
		return new GetChannelsHistoryResponse(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as GetChannelsHistoryResponse);
	}

	[DebuggerNonUserCode]
	public bool Equals(GetChannelsHistoryResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!channelHistories_.Equals(other.channelHistories_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= channelHistories_.GetHashCode();
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
		channelHistories_.WriteTo(ref output, _repeated_channelHistories_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += channelHistories_.CalculateSize(_repeated_channelHistories_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(GetChannelsHistoryResponse other)
	{
		if (other != null)
		{
			channelHistories_.Add(other.channelHistories_);
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
				channelHistories_.AddEntriesFrom(ref input, _repeated_channelHistories_codec);
			}
		}
	}
}
