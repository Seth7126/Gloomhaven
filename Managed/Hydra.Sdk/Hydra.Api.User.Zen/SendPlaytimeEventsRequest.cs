using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Hydra.Api.Infrastructure.Context;

namespace Hydra.Api.User.Zen;

public sealed class SendPlaytimeEventsRequest : IMessage<SendPlaytimeEventsRequest>, IMessage, IEquatable<SendPlaytimeEventsRequest>, IDeepCloneable<SendPlaytimeEventsRequest>, IBufferMessage
{
	private static readonly MessageParser<SendPlaytimeEventsRequest> _parser = new MessageParser<SendPlaytimeEventsRequest>(() => new SendPlaytimeEventsRequest());

	private UnknownFieldSet _unknownFields;

	public const int EventsFieldNumber = 1;

	private static readonly FieldCodec<PlaytimeEvent> _repeated_events_codec = FieldCodec.ForMessage(10u, PlaytimeEvent.Parser);

	private readonly RepeatedField<PlaytimeEvent> events_ = new RepeatedField<PlaytimeEvent>();

	public const int UserContextFieldNumber = 2;

	private UserContext userContext_;

	[DebuggerNonUserCode]
	public static MessageParser<SendPlaytimeEventsRequest> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => ZenEconomyApiContractsReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<PlaytimeEvent> Events => events_;

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
	public SendPlaytimeEventsRequest()
	{
	}

	[DebuggerNonUserCode]
	public SendPlaytimeEventsRequest(SendPlaytimeEventsRequest other)
		: this()
	{
		events_ = other.events_.Clone();
		userContext_ = ((other.userContext_ != null) ? other.userContext_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public SendPlaytimeEventsRequest Clone()
	{
		return new SendPlaytimeEventsRequest(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as SendPlaytimeEventsRequest);
	}

	[DebuggerNonUserCode]
	public bool Equals(SendPlaytimeEventsRequest other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!events_.Equals(other.events_))
		{
			return false;
		}
		if (!object.Equals(UserContext, other.UserContext))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= events_.GetHashCode();
		if (userContext_ != null)
		{
			num ^= UserContext.GetHashCode();
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
		events_.WriteTo(ref output, _repeated_events_codec);
		if (userContext_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(UserContext);
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
		num += events_.CalculateSize(_repeated_events_codec);
		if (userContext_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(UserContext);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(SendPlaytimeEventsRequest other)
	{
		if (other == null)
		{
			return;
		}
		events_.Add(other.events_);
		if (other.userContext_ != null)
		{
			if (userContext_ == null)
			{
				UserContext = new UserContext();
			}
			UserContext.MergeFrom(other.UserContext);
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
				events_.AddEntriesFrom(ref input, _repeated_events_codec);
				break;
			case 18u:
				if (userContext_ == null)
				{
					UserContext = new UserContext();
				}
				input.ReadMessage(UserContext);
				break;
			}
		}
	}
}
