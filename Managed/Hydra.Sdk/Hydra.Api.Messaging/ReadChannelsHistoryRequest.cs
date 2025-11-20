using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Messaging;

public sealed class ReadChannelsHistoryRequest : IMessage<ReadChannelsHistoryRequest>, IMessage, IEquatable<ReadChannelsHistoryRequest>, IDeepCloneable<ReadChannelsHistoryRequest>, IBufferMessage
{
	private static readonly MessageParser<ReadChannelsHistoryRequest> _parser = new MessageParser<ReadChannelsHistoryRequest>(() => new ReadChannelsHistoryRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ChannelIndexesFieldNumber = 2;

	private static readonly FieldCodec<ChannelIndex> _repeated_channelIndexes_codec = FieldCodec.ForMessage(18u, ChannelIndex.Parser);

	private readonly RepeatedField<ChannelIndex> channelIndexes_ = new RepeatedField<ChannelIndex>();

	[DebuggerNonUserCode]
	public static MessageParser<ReadChannelsHistoryRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[20];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public UserContext UserContext
	{
		get
		{
			return userContext_;
		}
		set
		{
			userContext_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChannelIndex> ChannelIndexes => channelIndexes_;

	[DebuggerNonUserCode]
	public ReadChannelsHistoryRequest()
	{
	}

	[DebuggerNonUserCode]
	public ReadChannelsHistoryRequest(ReadChannelsHistoryRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		channelIndexes_ = other.channelIndexes_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public ReadChannelsHistoryRequest Clone()
	{
		return new ReadChannelsHistoryRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as ReadChannelsHistoryRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(ReadChannelsHistoryRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		if (!channelIndexes_.Equals(other.channelIndexes_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
		}
		num ^= channelIndexes_.GetHashCode();
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
		if (userContext_ != null)
		{
			output.WriteRawTag(10);
			output.WriteMessage(UserContext);
		}
		channelIndexes_.WriteTo(ref output, _repeated_channelIndexes_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		num += channelIndexes_.CalculateSize(_repeated_channelIndexes_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(ReadChannelsHistoryRequest other)
	{
		if (other == null)
		{
			return;
		}
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
		}
		channelIndexes_.Add(other.channelIndexes_);
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
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			case 18u:
				channelIndexes_.AddEntriesFrom(ref input, _repeated_channelIndexes_codec);
				break;
			}
		}
	}
}
