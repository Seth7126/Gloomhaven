using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.Messaging;

public sealed class CreateChannelRequest : IMessage<CreateChannelRequest>, IMessage, IEquatable<CreateChannelRequest>, IDeepCloneable<CreateChannelRequest>, IBufferMessage
{
	private static readonly MessageParser<CreateChannelRequest> _parser = new MessageParser<CreateChannelRequest>(() => new CreateChannelRequest());

	private UnknownFieldSet _unknownFields;

	public const int UserContextFieldNumber = 1;

	private UserContext userContext_;

	public const int ChannelFieldNumber = 2;

	private Channel channel_;

	public const int ChannelConfigurationFieldNumber = 3;

	private ChannelConfiguration channelConfiguration_;

	public const int ChannelCredentialFieldNumber = 4;

	private ChannelCredential channelCredential_;

	[DebuggerNonUserCode]
	public static MessageParser<CreateChannelRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[4];

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
	public ChannelConfiguration ChannelConfiguration
	{
		get
		{
			return channelConfiguration_;
		}
		set
		{
			channelConfiguration_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ChannelCredential ChannelCredential
	{
		get
		{
			return channelCredential_;
		}
		set
		{
			channelCredential_ = value;
		}
	}

	[DebuggerNonUserCode]
	public CreateChannelRequest()
	{
	}

	[DebuggerNonUserCode]
	public CreateChannelRequest(CreateChannelRequest other)
		: this()
	{
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		channel_ = ((other.channel_ != null) ? other.channel_.Clone() : null);
		channelConfiguration_ = ((other.channelConfiguration_ != null) ? other.channelConfiguration_.Clone() : null);
		channelCredential_ = ((other.channelCredential_ != null) ? other.channelCredential_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CreateChannelRequest Clone()
	{
		return new CreateChannelRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CreateChannelRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(CreateChannelRequest other)
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
		if (!object.Equals(ChannelConfiguration, other.ChannelConfiguration))
		{
			return false;
		}
		if (!object.Equals(ChannelCredential, other.ChannelCredential))
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
		if (channelConfiguration_ != null)
		{
			num ^= ChannelConfiguration.GetHashCode();
		}
		if (channelCredential_ != null)
		{
			num ^= ChannelCredential.GetHashCode();
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
		if (channelConfiguration_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(ChannelConfiguration);
		}
		if (channelCredential_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(ChannelCredential);
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
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (channel_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Channel);
		}
		if (channelConfiguration_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelConfiguration);
		}
		if (channelCredential_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ChannelCredential);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CreateChannelRequest other)
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
		if (other.channelConfiguration_ != null)
		{
			if (channelConfiguration_ == null)
			{
				ChannelConfiguration = new ChannelConfiguration();
			}
			ChannelConfiguration.MergeFrom(other.ChannelConfiguration);
		}
		if (other.channelCredential_ != null)
		{
			if (channelCredential_ == null)
			{
				ChannelCredential = new ChannelCredential();
			}
			ChannelCredential.MergeFrom(other.ChannelCredential);
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
				if (channelConfiguration_ == null)
				{
					ChannelConfiguration = new ChannelConfiguration();
				}
				input.ReadMessage(ChannelConfiguration);
				break;
			case 34u:
				if (channelCredential_ == null)
				{
					ChannelCredential = new ChannelCredential();
				}
				input.ReadMessage(ChannelCredential);
				break;
			}
		}
	}
}
