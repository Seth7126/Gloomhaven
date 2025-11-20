using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Messaging;

public sealed class UpdateMembersRequest : IMessage<UpdateMembersRequest>, IMessage, IEquatable<UpdateMembersRequest>, IDeepCloneable<UpdateMembersRequest>, IBufferMessage
{
	private static readonly MessageParser<UpdateMembersRequest> _parser = new MessageParser<UpdateMembersRequest>(() => new UpdateMembersRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ChannelFieldNumber = 2;

	private Channel channel_;

	public const int ChannelUsersAccessFieldNumber = 3;

	private static readonly FieldCodec<ChannelUserAccess> _repeated_channelUsersAccess_codec = FieldCodec.ForMessage(26u, ChannelUserAccess.Parser);

	private readonly RepeatedField<ChannelUserAccess> channelUsersAccess_ = new RepeatedField<ChannelUserAccess>();

	[DebuggerNonUserCode]
	public static MessageParser<UpdateMembersRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[8];

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
	public Channel Channel
	{
		get
		{
			return channel_;
		}
		set
		{
			channel_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<ChannelUserAccess> ChannelUsersAccess => channelUsersAccess_;

	[DebuggerNonUserCode]
	public UpdateMembersRequest()
	{
	}

	[DebuggerNonUserCode]
	public UpdateMembersRequest(UpdateMembersRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		channel_ = ((other.channel_ != null) ? other.channel_.Clone() : null);
		channelUsersAccess_ = other.channelUsersAccess_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public UpdateMembersRequest Clone()
	{
		return new UpdateMembersRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as UpdateMembersRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(UpdateMembersRequest other)
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
		if (!object.Equals(Channel, other.Channel))
		{
			return false;
		}
		if (!channelUsersAccess_.Equals(other.channelUsersAccess_))
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
		if (channel_ != null)
		{
			num ^= Channel.GetHashCode();
		}
		num ^= channelUsersAccess_.GetHashCode();
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
		if (channel_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Channel);
		}
		channelUsersAccess_.WriteTo(ref output, _repeated_channelUsersAccess_codec);
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
		if (channel_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Channel);
		}
		num += channelUsersAccess_.CalculateSize(_repeated_channelUsersAccess_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(UpdateMembersRequest other)
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
		if (other.channel_ != null)
		{
			if (channel_ == null)
			{
				Channel = new Channel();
			}
			Channel.MergeFrom(other.Channel);
		}
		channelUsersAccess_.Add(other.channelUsersAccess_);
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
				if (channel_ == null)
				{
					Channel = new Channel();
				}
				input.ReadMessage(Channel);
				break;
			case 26u:
				channelUsersAccess_.AddEntriesFrom(ref input, _repeated_channelUsersAccess_codec);
				break;
			}
		}
	}
}
