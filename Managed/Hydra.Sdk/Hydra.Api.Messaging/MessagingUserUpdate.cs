using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Hydra.Api.Messaging;

public sealed class MessagingUserUpdate : IMessage<MessagingUserUpdate>, IMessage, IEquatable<MessagingUserUpdate>, IDeepCloneable<MessagingUserUpdate>, IBufferMessage
{
	private static readonly MessageParser<MessagingUserUpdate> _parser = new MessageParser<MessagingUserUpdate>(() => new MessagingUserUpdate());

	private UnknownFieldSet _unknownFields;

	public const int MessagesFieldNumber = 1;

	private static readonly FieldCodec<ChannelUserMessage> _repeated_messages_codec = FieldCodec.ForMessage(10u, ChannelUserMessage.Parser);

	private readonly RepeatedField<ChannelUserMessage> messages_ = new RepeatedField<ChannelUserMessage>();

	[DebuggerNonUserCode]
	public static MessageParser<MessagingUserUpdate> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => MessagingContractsReflection.Descriptor.MessageTypes[34];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<ChannelUserMessage> Messages => messages_;

	[DebuggerNonUserCode]
	public MessagingUserUpdate()
	{
	}

	[DebuggerNonUserCode]
	public MessagingUserUpdate(MessagingUserUpdate other)
		: this()
	{
		messages_ = other.messages_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public MessagingUserUpdate Clone()
	{
		return new MessagingUserUpdate(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as MessagingUserUpdate);
	}

	[DebuggerNonUserCode]
	public bool Equals(MessagingUserUpdate other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!messages_.Equals(other.messages_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= messages_.GetHashCode();
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
		messages_.WriteTo(ref output, _repeated_messages_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += messages_.CalculateSize(_repeated_messages_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(MessagingUserUpdate other)
	{
		if (other != null)
		{
			messages_.Add(other.messages_);
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
				messages_.AddEntriesFrom(ref input, _repeated_messages_codec);
			}
		}
	}
}
